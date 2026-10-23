namespace SmartExchange.Application.Exchange.Dtos;

public sealed record PortfolioAssetInput(string Currency, decimal Amount);

public sealed record PortfolioAssetValuation(
    string Currency,
    decimal Amount,
    decimal ExchangeRate,
    decimal ConvertedAmount,
    decimal SharePercentage);

public sealed record PortfolioValuationResponse(
    string TargetCurrency,
    decimal TotalValue,
    IReadOnlyList<PortfolioAssetValuation> Assets);
