namespace SmartExchange.Application.Exchange.Dtos;

public sealed record HistoricalRatePoint(DateOnly Date, decimal Rate);

public sealed record HistoricalRatesResponse(
    string BaseCurrency,
    string TargetCurrency,
    string Period,
    DateOnly StartDate,
    DateOnly EndDate,
    IReadOnlyList<HistoricalRatePoint> Points);
