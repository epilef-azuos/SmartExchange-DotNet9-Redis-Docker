using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmartExchange.Application.Exchange.Dtos;
using SmartExchange.Infrastructure.Persistence;
using SmartExchange.Tests.Integration.Fixtures;
using StackExchange.Redis;

namespace SmartExchange.Tests.Integration;

[Collection(IntegrationCollection.Name)]
public class ExchangeEndpointsIntegrationTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly SmartExchangeApiFactory _factory;

    public ExchangeEndpointsIntegrationTests(SmartExchangeApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Convert_ThenAuditEndpoint_ReturnsPersistedConversion()
    {
        var client = _factory.CreateClient();

        var convert = await client.GetAsync("/api/v1/exchange/convert?from=USD&to=BRL&amount=100&tier=Premium");

        convert.StatusCode.Should().Be(HttpStatusCode.OK);
        var conversion = await convert.Content.ReadFromJsonAsync<ConversionResponse>(JsonOptions);
        // 100 USD * 5,00 = 500,00; spread Premium de 1,5% = 7,50; total = 507,50.
        conversion!.TargetAmount.Should().Be(507.50m);
        conversion.SpreadAmount.Should().Be(7.50m);

        var audit = await client.GetAsync("/api/v1/exchange/audit?take=50");

        audit.StatusCode.Should().Be(HttpStatusCode.OK);
        var logs = await audit.Content.ReadFromJsonAsync<List<AuditLogDto>>(JsonOptions);
        logs.Should().Contain(log =>
            log.SourceCurrency == "USD" &&
            log.TargetCurrency == "BRL" &&
            log.BaseAmount == 100m &&
            log.ConvertedAmount == 507.50m &&
            log.ClientTier == "Premium");
    }

    [Fact]
    public async Task Convert_PersistsAuditRowPhysicallyInPostgres()
    {
        var client = _factory.CreateClient();

        await client.GetAsync("/api/v1/exchange/convert?from=USD&to=BRL&amount=321&tier=Vip");

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AuditDbContext>();
        var persisted = await context.ConversionAuditLogs
            .AsNoTracking()
            .AnyAsync(log => log.BaseAmount == 321m && log.ClientTier == "Vip");

        persisted.Should().BeTrue();
    }

    [Fact]
    public async Task Convert_WithInvalidCurrency_ReturnsProblemDetailsWithBadRequest()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/exchange/convert?from=US&to=BRL&amount=10");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
    }

    [Fact]
    public async Task Portfolio_Valuate_ReturnsSharesThatSumToOneHundred()
    {
        var client = _factory.CreateClient();
        var body = new
        {
            targetCurrency = "USD",
            assets = new[]
            {
                new { currency = "EUR", amount = 1000.00m },
                new { currency = "USD", amount = 250.00m }
            }
        };

        var response = await client.PostAsJsonAsync("/api/v1/exchange/portfolio/valuate", body);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var valuation = await response.Content.ReadFromJsonAsync<PortfolioValuationResponse>(JsonOptions);
        // EUR -> USD: 1000 * 1,10 = 1100,00; USD: 250,00; total = 1350,00.
        valuation!.TotalValue.Should().Be(1350.00m);
        valuation.Assets.Single(a => a.Currency == "EUR").SharePercentage.Should().Be(81.48m);
        valuation.Assets.Single(a => a.Currency == "USD").SharePercentage.Should().Be(18.52m);
        valuation.Assets.Sum(a => a.SharePercentage).Should().Be(100.00m);
    }

    [Fact]
    public async Task Currencies_IsCachedInRedisWithSevenDayTtl()
    {
        var client = _factory.CreateClient();

        var first = await client.GetAsync("/api/v1/exchange/currencies");

        first.StatusCode.Should().Be(HttpStatusCode.OK);
        var currencies = await first.Content.ReadFromJsonAsync<CurrenciesResponse>(JsonOptions);
        currencies!.Currencies.Should().ContainKey("BRL");

        await using var redis = await ConnectionMultiplexer.ConnectAsync(_factory.RedisConnectionString);
        var database = redis.GetDatabase();
        var exists = await database.KeyExistsAsync("exchange:currencies");
        var ttl = await database.KeyTimeToLiveAsync("exchange:currencies");

        exists.Should().BeTrue();
        ttl.Should().NotBeNull();
        ttl!.Value.Should().BeGreaterThan(TimeSpan.FromDays(6.9)).And.BeLessThanOrEqualTo(TimeSpan.FromDays(7));

        var requestsBefore = _factory.FrankfurterHandler.RequestCount;
        var second = await client.GetAsync("/api/v1/exchange/currencies");

        second.StatusCode.Should().Be(HttpStatusCode.OK);
        _factory.FrankfurterHandler.RequestCount.Should().Be(requestsBefore, "a segunda chamada deve ser servida pelo cache do Redis");
    }
}
