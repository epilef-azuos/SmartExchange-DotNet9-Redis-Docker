using MediatR;
using SmartExchange.Application.Common.Interfaces;
using SmartExchange.Application.Exchange.Dtos;
using SmartExchange.Domain.Common;
using SmartExchange.Domain.ValueObjects;

namespace SmartExchange.Application.Exchange.Queries;

public sealed class GetLatestRatesQueryHandler : IRequestHandler<GetLatestRatesQuery, LatestRatesResponse>
{
    private readonly IExchangeRateProvider _rateProvider;

    public GetLatestRatesQueryHandler(IExchangeRateProvider rateProvider)
    {
        _rateProvider = rateProvider;
    }

    public async Task<LatestRatesResponse> Handle(GetLatestRatesQuery request, CancellationToken cancellationToken)
    {
        var baseCurrency = new Currency(request.Base);
        var rates = await _rateProvider.GetLatestRatesAsync(baseCurrency, cancellationToken);

        var rounded = rates.ToDictionary(entry => entry.Key, entry => FinancialRounding.RoundRate(entry.Value));

        return new LatestRatesResponse(baseCurrency.Code, rounded);
    }
}
