using SmartExchange.Domain.Models;
using SmartExchange.Domain.ValueObjects;

namespace SmartExchange.Application.Common.Interfaces;

public interface IExchangeRateProvider
{
    Task<ExchangeRate> GetLatestRateAsync(Currency baseCurrency, Currency targetCurrency, CancellationToken ct = default);

    Task<IReadOnlyDictionary<string, decimal>> GetLatestRatesAsync(Currency baseCurrency, CancellationToken ct = default);

    Task<IReadOnlyDictionary<string, string>> GetCurrenciesAsync(CancellationToken ct = default);

    Task<IReadOnlyDictionary<DateOnly, Dictionary<string, decimal>>> GetHistoricalRatesAsync(
        Currency baseCurrency,
        Currency targetCurrency,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken ct = default);
}
