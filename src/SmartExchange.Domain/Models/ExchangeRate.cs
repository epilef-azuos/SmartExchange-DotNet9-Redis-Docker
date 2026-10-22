using SmartExchange.Domain.Common;
using SmartExchange.Domain.ValueObjects;

namespace SmartExchange.Domain.Models;

public sealed record ExchangeRate
{
    public ExchangeRate(Currency baseCurrency, Currency targetCurrency, decimal rate, DateTimeOffset timestampUtc)
    {
        if (baseCurrency == targetCurrency)
        {
            throw new DomainException("A moeda base e a moeda destino devem ser diferentes.");
        }

        if (rate <= 0)
        {
            throw new DomainException("A taxa de câmbio deve ser maior que zero.");
        }

        if (timestampUtc.Offset != TimeSpan.Zero)
        {
            throw new DomainException("O timestamp da taxa deve estar em UTC.");
        }

        BaseCurrency = baseCurrency;
        TargetCurrency = targetCurrency;
        Rate = rate;
        TimestampUtc = timestampUtc;
    }

    public Currency BaseCurrency { get; }

    public Currency TargetCurrency { get; }

    public decimal Rate { get; }

    public DateTimeOffset TimestampUtc { get; }
}
