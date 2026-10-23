using SmartExchange.Domain.Enums;
using SmartExchange.Domain.Models;

namespace SmartExchange.Application.Exchange.Dtos;

public sealed record ConversionResponse(
    string SourceCurrency,
    decimal SourceAmount,
    string TargetCurrency,
    decimal TargetAmount,
    decimal BaseRate,
    decimal RateWithSpread,
    decimal SpreadPercentage,
    decimal SpreadAmount,
    ClientTier ClientTier,
    DateTimeOffset TimestampUtc)
{
    public static ConversionResponse From(ConversionResult result) => new(
        result.Source.Currency.Code,
        result.Source.Amount,
        result.Target.Currency.Code,
        result.Target.Amount,
        result.BaseRate,
        result.RateWithSpread,
        result.SpreadPercentage,
        result.SpreadAmount,
        result.ClientTier,
        result.TimestampUtc);
}
