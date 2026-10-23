using MediatR;
using SmartExchange.Application.Common.Interfaces;
using SmartExchange.Application.Exchange.Dtos;

namespace SmartExchange.Application.Exchange.Queries;

public sealed class GetCurrenciesQueryHandler : IRequestHandler<GetCurrenciesQuery, CurrenciesResponse>
{
    private readonly IExchangeRateProvider _rateProvider;

    public GetCurrenciesQueryHandler(IExchangeRateProvider rateProvider)
    {
        _rateProvider = rateProvider;
    }

    public async Task<CurrenciesResponse> Handle(GetCurrenciesQuery request, CancellationToken cancellationToken)
    {
        var currencies = await _rateProvider.GetCurrenciesAsync(cancellationToken);

        return new CurrenciesResponse(currencies);
    }
}
