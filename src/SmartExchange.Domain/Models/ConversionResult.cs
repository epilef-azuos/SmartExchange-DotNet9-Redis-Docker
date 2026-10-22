using SmartExchange.Domain.Common;
using SmartExchange.Domain.Enums;
using SmartExchange.Domain.Strategies;
using SmartExchange.Domain.ValueObjects;

namespace SmartExchange.Domain.Models;

public sealed record ConversionResult
{
    private ConversionResult(
        Money source,
        Money target,
        decimal baseRate,
        decimal rateWithSpread,
        decimal spreadPercentage,
        decimal spreadAmount,
        ClientTier clientTier,
        DateTimeOffset timestampUtc)
    {
        Source = source;
        Target = target;
        BaseRate = baseRate;
        RateWithSpread = rateWithSpread;
        SpreadPercentage = spreadPercentage;
        SpreadAmount = spreadAmount;
        ClientTier = clientTier;
        TimestampUtc = timestampUtc;
    }

    public Money Source { get; }

    public Money Target { get; }

    public decimal BaseRate { get; }

    public decimal RateWithSpread { get; }

    public decimal SpreadPercentage { get; }

    public decimal SpreadAmount { get; }

    public ClientTier ClientTier { get; }

    public DateTimeOffset TimestampUtc { get; }

    public static ConversionResult Calculate(Money source, ExchangeRate rate, ISpreadStrategy strategy)
    {
        if (source.Currency != rate.BaseCurrency)
        {
            throw new DomainException(
                $"A moeda de origem ({source.Currency}) não corresponde à moeda base da taxa ({rate.BaseCurrency}).");
        }

        var rateWithSpread = strategy.ApplySpreadToRate(rate.Rate);
        var baseTargetAmount = FinancialRounding.RoundAmount(source.Amount * rate.Rate);
        var spreadAmount = strategy.CalculateSpreadAmount(source.Amount * rate.Rate);
        var target = new Money(baseTargetAmount + spreadAmount, rate.TargetCurrency);

        return new ConversionResult(
            source,
            target,
            rate.Rate,
            rateWithSpread,
            strategy.SpreadPercentage,
            spreadAmount,
            strategy.Tier,
            rate.TimestampUtc);
    }
}
