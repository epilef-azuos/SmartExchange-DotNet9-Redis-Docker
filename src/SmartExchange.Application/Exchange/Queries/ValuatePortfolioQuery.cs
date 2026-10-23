using MediatR;
using SmartExchange.Application.Exchange.Dtos;

namespace SmartExchange.Application.Exchange.Queries;

public sealed record ValuatePortfolioQuery(string TargetCurrency, IReadOnlyList<PortfolioAssetInput> Assets)
    : IRequest<PortfolioValuationResponse>;
