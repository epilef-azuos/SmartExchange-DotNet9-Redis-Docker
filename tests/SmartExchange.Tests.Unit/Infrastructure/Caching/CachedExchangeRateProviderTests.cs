using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SmartExchange.Application.Common.Interfaces;
using SmartExchange.Domain.Models;
using SmartExchange.Domain.ValueObjects;
using SmartExchange.Infrastructure.Caching;

namespace SmartExchange.Tests.Unit.Infrastructure.Caching;

public class CachedExchangeRateProviderTests
{
    private static readonly Currency Usd = new("USD");
    private static readonly Currency Brl = new("BRL");
    private static readonly DateOnly Start = new(2026, 10, 1);
    private static readonly DateOnly End = new(2026, 10, 2);
    private static readonly DateTimeOffset QuoteTime = new(2026, 10, 22, 0, 0, 0, TimeSpan.Zero);

    private readonly Mock<IExchangeRateProvider> _inner = new();
    private readonly Mock<IDistributedCache> _cache = new();
    private readonly CachedExchangeRateProvider _sut;

    public CachedExchangeRateProviderTests()
    {
        _sut = new CachedExchangeRateProvider(_inner.Object, _cache.Object, NullLogger<CachedExchangeRateProvider>.Instance);
    }

    [Fact]
    public async Task GetLatestRateAsync_WhenCacheHit_ReturnsCachedValueWithoutCallingInner()
    {
        var cached = new ExchangeRate(Usd, Brl, 5.049m, QuoteTime);
        SetupCacheGet("exchange:pair:USD:BRL", JsonSerializer.SerializeToUtf8Bytes(cached));

        var rate = await _sut.GetLatestRateAsync(Usd, Brl);

        rate.Should().BeEquivalentTo(cached);
        _inner.Verify(p => p.GetLatestRateAsync(It.IsAny<Currency>(), It.IsAny<Currency>(), It.IsAny<CancellationToken>()), Times.Never);
        VerifyNoCacheWrite();
    }

    [Fact]
    public async Task GetLatestRateAsync_WhenCacheMiss_CallsInnerAndStoresWithFifteenMinuteTtl()
    {
        var fetched = new ExchangeRate(Usd, Brl, 5.049m, QuoteTime);
        SetupCacheGet("exchange:pair:USD:BRL", null);
        _inner.Setup(p => p.GetLatestRateAsync(Usd, Brl, It.IsAny<CancellationToken>())).ReturnsAsync(fetched);

        var rate = await _sut.GetLatestRateAsync(Usd, Brl);

        rate.Should().Be(fetched);
        VerifyCacheWrite("exchange:pair:USD:BRL", TimeSpan.FromMinutes(15));
    }

    [Fact]
    public async Task GetLatestRatesAsync_WhenCacheMiss_StoresWithFifteenMinuteTtl()
    {
        IReadOnlyDictionary<string, decimal> fetched = new Dictionary<string, decimal> { ["BRL"] = 5.049m };
        SetupCacheGet("exchange:latest:USD", null);
        _inner.Setup(p => p.GetLatestRatesAsync(Usd, It.IsAny<CancellationToken>())).ReturnsAsync(fetched);

        var rates = await _sut.GetLatestRatesAsync(Usd);

        rates.Should().BeEquivalentTo(fetched);
        VerifyCacheWrite("exchange:latest:USD", TimeSpan.FromMinutes(15));
    }

    [Fact]
    public async Task GetCurrenciesAsync_WhenCacheMiss_StoresWithSevenDayTtl()
    {
        IReadOnlyDictionary<string, string> fetched = new Dictionary<string, string> { ["BRL"] = "Brazilian Real" };
        SetupCacheGet("exchange:currencies", null);
        _inner.Setup(p => p.GetCurrenciesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(fetched);

        var currencies = await _sut.GetCurrenciesAsync();

        currencies.Should().BeEquivalentTo(fetched);
        VerifyCacheWrite("exchange:currencies", TimeSpan.FromDays(7));
    }

    [Fact]
    public async Task GetHistoricalRatesAsync_WhenCacheMiss_StoresWithTwentyFourHourTtl()
    {
        IReadOnlyDictionary<DateOnly, Dictionary<string, decimal>> fetched = new Dictionary<DateOnly, Dictionary<string, decimal>>
        {
            [Start] = new() { ["BRL"] = 5.1885m }
        };
        SetupCacheGet("exchange:history:USD:BRL:2026-10-01:2026-10-02", null);
        _inner.Setup(p => p.GetHistoricalRatesAsync(Usd, Brl, Start, End, It.IsAny<CancellationToken>())).ReturnsAsync(fetched);

        var history = await _sut.GetHistoricalRatesAsync(Usd, Brl, Start, End);

        history[Start]["BRL"].Should().Be(5.1885m);
        VerifyCacheWrite("exchange:history:USD:BRL:2026-10-01:2026-10-02", TimeSpan.FromHours(24));
    }

    [Fact]
    public async Task GetHistoricalRatesAsync_WhenCacheHit_RestoresDatesAndRates()
    {
        var wire = new Dictionary<string, Dictionary<string, decimal>>
        {
            ["2026-10-01"] = new() { ["BRL"] = 5.1885m }
        };
        SetupCacheGet("exchange:history:USD:BRL:2026-10-01:2026-10-02", JsonSerializer.SerializeToUtf8Bytes(wire));

        var history = await _sut.GetHistoricalRatesAsync(Usd, Brl, Start, End);

        history[Start]["BRL"].Should().Be(5.1885m);
        _inner.Verify(p => p.GetHistoricalRatesAsync(It.IsAny<Currency>(), It.IsAny<Currency>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetLatestRateAsync_WhenCacheReadFails_FallsBackToInnerProvider()
    {
        var fetched = new ExchangeRate(Usd, Brl, 5.049m, QuoteTime);
        _cache.Setup(c => c.GetAsync("exchange:pair:USD:BRL", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Redis indisponível"));
        _inner.Setup(p => p.GetLatestRateAsync(Usd, Brl, It.IsAny<CancellationToken>())).ReturnsAsync(fetched);

        var rate = await _sut.GetLatestRateAsync(Usd, Brl);

        rate.Should().Be(fetched);
    }

    [Fact]
    public async Task GetLatestRateAsync_WhenCacheWriteFails_StillReturnsQuote()
    {
        var fetched = new ExchangeRate(Usd, Brl, 5.049m, QuoteTime);
        SetupCacheGet("exchange:pair:USD:BRL", null);
        _cache.Setup(c => c.SetAsync("exchange:pair:USD:BRL", It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Redis indisponível"));
        _inner.Setup(p => p.GetLatestRateAsync(Usd, Brl, It.IsAny<CancellationToken>())).ReturnsAsync(fetched);

        var rate = await _sut.GetLatestRateAsync(Usd, Brl);

        rate.Should().Be(fetched);
    }

    [Fact]
    public async Task GetLatestRateAsync_WhenCachedPayloadIsCorrupted_TreatsAsMissAndCallsInner()
    {
        var fetched = new ExchangeRate(Usd, Brl, 5.049m, QuoteTime);
        SetupCacheGet("exchange:pair:USD:BRL", "{ corrompido"u8.ToArray());
        _inner.Setup(p => p.GetLatestRateAsync(Usd, Brl, It.IsAny<CancellationToken>())).ReturnsAsync(fetched);

        var rate = await _sut.GetLatestRateAsync(Usd, Brl);

        rate.Should().Be(fetched);
    }

    [Fact]
    public async Task GetLatestRateAsync_WhenCancelled_PropagatesCancellation()
    {
        _cache.Setup(c => c.GetAsync("exchange:pair:USD:BRL", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        Func<Task> act = () => _sut.GetLatestRateAsync(Usd, Brl);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    private void SetupCacheGet(string key, byte[]? payload)
    {
        _cache.Setup(c => c.GetAsync(key, It.IsAny<CancellationToken>())).ReturnsAsync(payload);
    }

    private void VerifyCacheWrite(string key, TimeSpan ttl)
    {
        _cache.Verify(
            c => c.SetAsync(
                key,
                It.IsAny<byte[]>(),
                It.Is<DistributedCacheEntryOptions>(options => options.AbsoluteExpirationRelativeToNow == ttl),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private void VerifyNoCacheWrite()
    {
        _cache.Verify(
            c => c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
