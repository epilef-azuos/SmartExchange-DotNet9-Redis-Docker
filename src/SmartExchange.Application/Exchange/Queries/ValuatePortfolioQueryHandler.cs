using MediatR;
using SmartExchange.Application.Common.Exceptions;
using SmartExchange.Application.Common.Interfaces;
using SmartExchange.Application.Exchange.Dtos;
using SmartExchange.Domain.Common;
using SmartExchange.Domain.Services;
using SmartExchange.Domain.ValueObjects;

namespace SmartExchange.Application.Exchange.Queries;

public sealed class ValuatePortfolioQueryHandler : IRequestHandler<ValuatePortfolioQuery, PortfolioValuationResponse>
{
    private readonly IExchangeRateProvider _rateProvider;

    public ValuatePortfolioQueryHandler(IExchangeRateProvider rateProvider)
    {
        _rateProvider = rateProvider;
    }

    public async Task<PortfolioValuationResponse> Handle(ValuatePortfolioQuery request, CancellationToken cancellationToken)
    {
        if (request.Assets is null || request.Assets.Count == 0)
        {
            throw new ValidationException("A carteira deve conter pelo menos um ativo.");
        }

        var target = new Currency(request.TargetCurrency);
        var valuations = new List<(string Currency, decimal Amount, decimal Rate, decimal Converted)>();

        foreach (var asset in request.Assets)
        {
            if (asset.Amount <= 0)
            {
                throw new ValidationException($"O montante do ativo {asset.Currency} deve ser maior que zero.");
            }

            var currency = new Currency(asset.Currency);
            var rate = currency == target
                ? 1m
                : (await _rateProvider.GetLatestRateAsync(currency, target, cancellationToken)).Rate;

            valuations.Add((currency.Code, asset.Amount, rate, FinancialRounding.RoundAmount(asset.Amount * rate)));
        }

        var shares = SharePercentageCalculator.Calculate(valuations.Select(v => v.Converted).ToList());

        var assets = valuations
            .Select((v, index) => new PortfolioAssetValuation(v.Currency, v.Amount, v.Rate, v.Converted, shares[index]))
            .ToList();

        return new PortfolioValuationResponse(
            target.Code,
            assets.Sum(a => a.ConvertedAmount),
            assets);
    }
}
