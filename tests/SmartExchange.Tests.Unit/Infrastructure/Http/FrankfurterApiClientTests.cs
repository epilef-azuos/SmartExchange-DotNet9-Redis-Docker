using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;
using SmartExchange.Application.Common.Exceptions;
using SmartExchange.Application.Common.Interfaces;
using SmartExchange.Domain.ValueObjects;
using SmartExchange.Infrastructure;
using SmartExchange.Infrastructure.Http.Clients;
using SmartExchange.Tests.Unit.Helpers;

namespace SmartExchange.Tests.Unit.Infrastructure.Http;

public class FrankfurterApiClientTests
{
    private const string LatestBrlJson = """{"amount":1.0,"base":"USD","date":"2026-10-22","rates":{"BRL":5.049}}""";
    private const string LatestRatesJson = """{"amount":1.0,"base":"USD","date":"2026-10-22","rates":{"BRL":5.049,"EUR":0.87}}""";
    private const string CurrenciesJson = """{"AUD":"Australian Dollar","BRL":"Brazilian Real"}""";
    private const string TimeSeriesJson = """
        {"amount":1.0,"base":"USD","start_date":"2026-10-01","end_date":"2026-10-02",
         "rates":{"2026-10-01":{"BRL":5.1885},"2026-10-02":{"BRL":5.2214}}}
        """;

    private static readonly Currency Usd = new("USD");
    private static readonly Currency Brl = new("BRL");

    [Fact]
    public async Task GetLatestRateAsync_WhenApiReturns200_MapsRateAndUtcTimestamp()
    {
        var handler = HttpMessageHandlerMock.Create((HttpStatusCode.OK, LatestBrlJson));
        var client = new FrankfurterApiClient(HttpMessageHandlerMock.CreateClient(handler));

        var rate = await client.GetLatestRateAsync(Usd, Brl);

        rate.BaseCurrency.Should().Be(Usd);
        rate.TargetCurrency.Should().Be(Brl);
        rate.Rate.Should().Be(5.049m);
        rate.TimestampUtc.Should().Be(new DateTimeOffset(2026, 10, 22, 0, 0, 0, TimeSpan.Zero));
        VerifyRequestedUri(handler, "https://api.frankfurter.app/latest?base=USD&symbols=BRL");
    }

    [Fact]
    public async Task GetLatestRatesAsync_WhenApiReturns200_ReturnsAllRatesForBaseCurrency()
    {
        var handler = HttpMessageHandlerMock.Create((HttpStatusCode.OK, LatestRatesJson));
        var client = new FrankfurterApiClient(HttpMessageHandlerMock.CreateClient(handler));

        var rates = await client.GetLatestRatesAsync(Usd);

        rates.Should().BeEquivalentTo(new Dictionary<string, decimal> { ["BRL"] = 5.049m, ["EUR"] = 0.87m });
        VerifyRequestedUri(handler, "https://api.frankfurter.app/latest?base=USD");
    }

    [Fact]
    public async Task GetCurrenciesAsync_WhenApiReturns200_ReturnsCodesWithNames()
    {
        var handler = HttpMessageHandlerMock.Create((HttpStatusCode.OK, CurrenciesJson));
        var client = new FrankfurterApiClient(HttpMessageHandlerMock.CreateClient(handler));

        var currencies = await client.GetCurrenciesAsync();

        currencies.Should().BeEquivalentTo(new Dictionary<string, string>
        {
            ["AUD"] = "Australian Dollar",
            ["BRL"] = "Brazilian Real"
        });
        VerifyRequestedUri(handler, "https://api.frankfurter.app/currencies");
    }

    [Fact]
    public async Task GetHistoricalRatesAsync_WhenApiReturns200_MapsDatesToRates()
    {
        var handler = HttpMessageHandlerMock.Create((HttpStatusCode.OK, TimeSeriesJson));
        var client = new FrankfurterApiClient(HttpMessageHandlerMock.CreateClient(handler));

        var history = await client.GetHistoricalRatesAsync(
            Usd, Brl, new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 2));

        history.Should().HaveCount(2);
        history[new DateOnly(2026, 10, 1)]["BRL"].Should().Be(5.1885m);
        history[new DateOnly(2026, 10, 2)]["BRL"].Should().Be(5.2214m);
        VerifyRequestedUri(handler, "https://api.frankfurter.app/2026-10-01..2026-10-02?base=USD&symbols=BRL");
    }

    [Fact]
    public async Task GetLatestRateAsync_WhenApiReturnsErrorStatus_ThrowsIntegrationExceptionWithStatusCode()
    {
        var handler = HttpMessageHandlerMock.Create((HttpStatusCode.NotFound, """{"message":"not found"}"""));
        var client = new FrankfurterApiClient(HttpMessageHandlerMock.CreateClient(handler));

        Func<Task> act = () => client.GetLatestRateAsync(Usd, Brl);

        var exception = await act.Should().ThrowAsync<ExchangeIntegrationException>()
            .WithMessage("*status 404*");
        exception.Which.StatusCode.Should().Be(404);
    }

    [Theory]
    [InlineData("{ invalid json")]
    [InlineData("""{"date":"2026-10-22"}""")]
    public async Task GetLatestRateAsync_WhenPayloadIsMalformed_ThrowsIntegrationException(string payload)
    {
        var handler = HttpMessageHandlerMock.Create((HttpStatusCode.OK, payload));
        var client = new FrankfurterApiClient(HttpMessageHandlerMock.CreateClient(handler));

        Func<Task> act = () => client.GetLatestRateAsync(Usd, Brl);

        await act.Should().ThrowAsync<ExchangeIntegrationException>()
            .WithMessage("*payload inválido*");
    }

    [Fact]
    public async Task GetLatestRateAsync_WhenPayloadIsNull_ThrowsIntegrationException()
    {
        var handler = HttpMessageHandlerMock.Create((HttpStatusCode.OK, "null"));
        var client = new FrankfurterApiClient(HttpMessageHandlerMock.CreateClient(handler));

        Func<Task> act = () => client.GetLatestRateAsync(Usd, Brl);

        await act.Should().ThrowAsync<ExchangeIntegrationException>()
            .WithMessage("*payload vazio*");
    }

    [Fact]
    public async Task GetLatestRateAsync_WhenResponseLacksTargetCurrency_ThrowsIntegrationException()
    {
        var handler = HttpMessageHandlerMock.Create((HttpStatusCode.OK, """{"date":"2026-10-22","rates":{"EUR":0.87}}"""));
        var client = new FrankfurterApiClient(HttpMessageHandlerMock.CreateClient(handler));

        Func<Task> act = () => client.GetLatestRateAsync(Usd, Brl);

        await act.Should().ThrowAsync<ExchangeIntegrationException>()
            .WithMessage("*não contém a taxa para BRL*");
    }

    [Fact]
    public async Task GetLatestRateAsync_WhenRateIsNotPositive_ThrowsIntegrationException()
    {
        var handler = HttpMessageHandlerMock.Create((HttpStatusCode.OK, """{"date":"2026-10-22","rates":{"BRL":0}}"""));
        var client = new FrankfurterApiClient(HttpMessageHandlerMock.CreateClient(handler));

        Func<Task> act = () => client.GetLatestRateAsync(Usd, Brl);

        await act.Should().ThrowAsync<ExchangeIntegrationException>()
            .WithMessage("*taxa retornada*inválida*");
    }

    [Fact]
    public async Task GetHistoricalRatesAsync_WhenDateKeyIsInvalid_ThrowsIntegrationException()
    {
        var payload = """{"start_date":"2026-10-01","end_date":"2026-10-02","rates":{"2026-13-45":{"BRL":5.1}}}""";
        var handler = HttpMessageHandlerMock.Create((HttpStatusCode.OK, payload));
        var client = new FrankfurterApiClient(HttpMessageHandlerMock.CreateClient(handler));

        Func<Task> act = () => client.GetHistoricalRatesAsync(
            Usd, Brl, new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 2));

        await act.Should().ThrowAsync<ExchangeIntegrationException>()
            .WithMessage("*data inválida*");
    }

    [Fact]
    public async Task GetLatestRateAsync_WhenTransportFails_ThrowsIntegrationException()
    {
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("connection refused"));
        var client = new FrankfurterApiClient(HttpMessageHandlerMock.CreateClient(handler));

        Func<Task> act = () => client.GetLatestRateAsync(Usd, Brl);

        await act.Should().ThrowAsync<ExchangeIntegrationException>()
            .WithMessage("*Falha de comunicação*");
    }

    [Fact]
    public async Task ResilientClient_WhenApiKeepsReturning500_RetriesThreeTimesThenThrows()
    {
        var handler = HttpMessageHandlerMock.Create((HttpStatusCode.InternalServerError, string.Empty));
        var client = CreateResilientClient(handler);

        Func<Task> act = () => client.GetLatestRateAsync(Usd, Brl);

        var exception = await act.Should().ThrowAsync<ExchangeIntegrationException>();
        exception.Which.StatusCode.Should().Be(500);
        handler.Protected().Verify(
            "SendAsync",
            Times.Exactly(4),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task ResilientClient_WhenApiReturns503ThenRecovers_ReturnsRateAfterRetry()
    {
        var handler = HttpMessageHandlerMock.Create(
            (HttpStatusCode.ServiceUnavailable, string.Empty),
            (HttpStatusCode.OK, LatestBrlJson));
        var client = CreateResilientClient(handler);

        var rate = await client.GetLatestRateAsync(Usd, Brl);

        rate.Rate.Should().Be(5.049m);
        handler.Protected().Verify(
            "SendAsync",
            Times.Exactly(2),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    // Pipeline de resiliência real, com o handler primário substituído pelo mock e sem atraso entre tentativas.
    private static IExchangeRateProvider CreateResilientClient(Mock<HttpMessageHandler> handler)
    {
        var services = new ServiceCollection();

        services.AddHttpClient<IExchangeRateProvider, FrankfurterApiClient>(client =>
            {
                client.BaseAddress = new Uri(HttpMessageHandlerMock.BaseUrl);
            })
            .ConfigurePrimaryHttpMessageHandler(() => handler.Object)
            .AddStandardResilienceHandler(options =>
            {
                DependencyInjection.ConfigureFrankfurterResilience(options);
                options.Retry.Delay = TimeSpan.FromMilliseconds(1);
            });

        return services.BuildServiceProvider().GetRequiredService<IExchangeRateProvider>();
    }

    private static void VerifyRequestedUri(Mock<HttpMessageHandler> handler, string expectedUri)
    {
        handler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(request => request.RequestUri!.ToString() == expectedUri),
            ItExpr.IsAny<CancellationToken>());
    }
}
