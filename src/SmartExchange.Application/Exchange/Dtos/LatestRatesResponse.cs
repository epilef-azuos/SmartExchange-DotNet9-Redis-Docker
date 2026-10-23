namespace SmartExchange.Application.Exchange.Dtos;

public sealed record LatestRatesResponse(string BaseCurrency, IReadOnlyDictionary<string, decimal> Rates);
