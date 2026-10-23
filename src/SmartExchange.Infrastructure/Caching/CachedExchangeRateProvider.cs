using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using SmartExchange.Application.Common.Interfaces;
using SmartExchange.Domain.Models;
using SmartExchange.Domain.ValueObjects;

namespace SmartExchange.Infrastructure.Caching;

// Decorator: cache-aside sobre IExchangeRateProvider. Falhas do cache são registradas e
// ignoradas, de forma que a cotação sempre seja obtida do provedor de origem.
public sealed class CachedExchangeRateProvider : IExchangeRateProvider
{
    private static readonly TimeSpan PairTtl = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan LatestTtl = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan CurrenciesTtl = TimeSpan.FromDays(7);
    private static readonly TimeSpan HistoryTtl = TimeSpan.FromHours(24);

    private const string DateFormat = "yyyy-MM-dd";

    private readonly IExchangeRateProvider _inner;
    private readonly IDistributedCache _cache;
    private readonly ILogger<CachedExchangeRateProvider> _logger;

    public CachedExchangeRateProvider(
        IExchangeRateProvider inner,
        IDistributedCache cache,
        ILogger<CachedExchangeRateProvider> logger)
    {
        _inner = inner;
        _cache = cache;
        _logger = logger;
    }

    public Task<ExchangeRate> GetLatestRateAsync(Currency baseCurrency, Currency targetCurrency, CancellationToken ct = default)
    {
        var key = CacheKeys.Pair(baseCurrency.Code, targetCurrency.Code);

        return GetOrCreateAsync(
            key,
            PairTtl,
            token => _inner.GetLatestRateAsync(baseCurrency, targetCurrency, token),
            ct);
    }

    public Task<IReadOnlyDictionary<string, decimal>> GetLatestRatesAsync(Currency baseCurrency, CancellationToken ct = default)
    {
        var key = CacheKeys.Latest(baseCurrency.Code);

        return GetOrCreateAsync(
            key,
            LatestTtl,
            token => _inner.GetLatestRatesAsync(baseCurrency, token),
            ct);
    }

    public Task<IReadOnlyDictionary<string, string>> GetCurrenciesAsync(CancellationToken ct = default)
    {
        return GetOrCreateAsync(
            CacheKeys.Currencies(),
            CurrenciesTtl,
            token => _inner.GetCurrenciesAsync(token),
            ct);
    }

    public async Task<IReadOnlyDictionary<DateOnly, Dictionary<string, decimal>>> GetHistoricalRatesAsync(
        Currency baseCurrency,
        Currency targetCurrency,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken ct = default)
    {
        var key = CacheKeys.History(baseCurrency.Code, targetCurrency.Code, startDate, endDate);

        var cached = await GetOrCreateAsync(
            key,
            HistoryTtl,
            async token => ToWireFormat(await _inner.GetHistoricalRatesAsync(
                baseCurrency, targetCurrency, startDate, endDate, token)),
            ct);

        return FromWireFormat(cached);
    }

    private async Task<T> GetOrCreateAsync<T>(
        string key,
        TimeSpan ttl,
        Func<CancellationToken, Task<T>> fetch,
        CancellationToken ct) where T : class
    {
        var cached = await TryGetAsync<T>(key, ct);

        if (cached is not null)
        {
            return cached;
        }

        var value = await fetch(ct);

        await TrySetAsync(key, value, ttl, ct);

        return value;
    }

    private async Task<T?> TryGetAsync<T>(string key, CancellationToken ct) where T : class
    {
        try
        {
            var bytes = await _cache.GetAsync(key, ct);

            return bytes is null ? null : JsonSerializer.Deserialize<T>(bytes);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Falha ao ler a chave {CacheKey} do cache. Consultando o provedor de origem.", key);

            return null;
        }
    }

    private async Task TrySetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken ct)
    {
        try
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(value);
            var options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl };

            await _cache.SetAsync(key, bytes, options, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Falha ao gravar a chave {CacheKey} no cache. A cotação será retornada mesmo assim.", key);
        }
    }

    private static Dictionary<string, Dictionary<string, decimal>> ToWireFormat(
        IReadOnlyDictionary<DateOnly, Dictionary<string, decimal>> history)
    {
        var wire = new Dictionary<string, Dictionary<string, decimal>>();

        foreach (var (date, rates) in history)
        {
            wire[date.ToString(DateFormat, CultureInfo.InvariantCulture)] = rates;
        }

        return wire;
    }

    private static IReadOnlyDictionary<DateOnly, Dictionary<string, decimal>> FromWireFormat(
        Dictionary<string, Dictionary<string, decimal>> wire)
    {
        var history = new Dictionary<DateOnly, Dictionary<string, decimal>>();

        foreach (var (date, rates) in wire)
        {
            history[DateOnly.ParseExact(date, DateFormat, CultureInfo.InvariantCulture)] = rates;
        }

        return history;
    }
}
