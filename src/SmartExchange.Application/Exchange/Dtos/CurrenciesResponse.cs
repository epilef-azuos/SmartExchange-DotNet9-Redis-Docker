namespace SmartExchange.Application.Exchange.Dtos;

public sealed record CurrenciesResponse(IReadOnlyDictionary<string, string> Currencies);
