using MediatR;
using SmartExchange.Application.Common.Exceptions;
using SmartExchange.Application.Common.Interfaces;
using SmartExchange.Application.Exchange.Dtos;
using SmartExchange.Domain.Common;
using SmartExchange.Domain.ValueObjects;

namespace SmartExchange.Application.Exchange.Queries;

public sealed class GetHistoricalRatesQueryHandler : IRequestHandler<GetHistoricalRatesQuery, HistoricalRatesResponse>
{
    private const string DefaultPeriod = "30d";

    private readonly IExchangeRateProvider _rateProvider;
    private readonly TimeProvider _timeProvider;

    public GetHistoricalRatesQueryHandler(IExchangeRateProvider rateProvider, TimeProvider timeProvider)
    {
        _rateProvider = rateProvider;
        _timeProvider = timeProvider;
    }

    public async Task<HistoricalRatesResponse> Handle(GetHistoricalRatesQuery request, CancellationToken cancellationToken)
    {
        var baseCurrency = new Currency(request.Base);
        var target = new Currency(request.Target);
        var period = string.IsNullOrWhiteSpace(request.Period) ? DefaultPeriod : request.Period.Trim().ToLowerInvariant();
        var days = ParsePeriodDays(period);

        var endDate = DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime);
        var startDate = endDate.AddDays(-days);

        var history = await _rateProvider.GetHistoricalRatesAsync(baseCurrency, target, startDate, endDate, cancellationToken);

        var points = history
            .Where(entry => entry.Value.ContainsKey(target.Code))
            .OrderBy(entry => entry.Key)
            .Select(entry => new HistoricalRatePoint(entry.Key, FinancialRounding.RoundRate(entry.Value[target.Code])))
            .ToList();

        return new HistoricalRatesResponse(baseCurrency.Code, target.Code, period, startDate, endDate, points);
    }

    private static int ParsePeriodDays(string period) => period switch
    {
        "7d" => 7,
        "30d" => 30,
        "90d" => 90,
        _ => throw new ValidationException($"Período inválido: '{period}'. Use 7d, 30d ou 90d.")
    };
}
