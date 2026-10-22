using FluentAssertions;
using SmartExchange.Domain.Common;
using SmartExchange.Domain.Enums;
using SmartExchange.Domain.Models;
using SmartExchange.Domain.Strategies;
using SmartExchange.Domain.ValueObjects;

namespace SmartExchange.Tests.Unit.Domain;

public class ConversionResultTests
{
    private static readonly Currency Usd = new("USD");
    private static readonly Currency Brl = new("BRL");
    private static readonly Currency Eur = new("EUR");
    private static readonly DateTimeOffset QuoteTime = new(2026, 10, 22, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Calculate_Standard_ConsolidatesAllConversionData()
    {
        var rate = new ExchangeRate(Usd, Brl, 5.00m, QuoteTime);

        var result = ConversionResult.Calculate(new Money(1000m, Usd), rate, new StandardSpreadStrategy());

        result.Source.Should().Be(new Money(1000m, Usd));
        result.Target.Should().Be(new Money(5175.00m, Brl));
        result.BaseRate.Should().Be(5.00m);
        result.RateWithSpread.Should().Be(5.175000m);
        result.SpreadPercentage.Should().Be(0.035m);
        result.SpreadAmount.Should().Be(175.00m);
        result.ClientTier.Should().Be(ClientTier.Standard);
        result.TimestampUtc.Should().Be(QuoteTime);
    }

    [Fact]
    public void Calculate_Vip_AppliesLowerSpread()
    {
        var rate = new ExchangeRate(Usd, Brl, 5.00m, QuoteTime);

        var result = ConversionResult.Calculate(new Money(1000m, Usd), rate, new VipSpreadStrategy());

        result.SpreadAmount.Should().Be(25.00m);
        result.Target.Should().Be(new Money(5025.00m, Brl));
        result.ClientTier.Should().Be(ClientTier.Vip);
    }

    [Fact]
    public void Calculate_WithMismatchedSourceCurrency_ThrowsDomainException()
    {
        var rate = new ExchangeRate(Usd, Brl, 5.00m, QuoteTime);

        Action act = () => ConversionResult.Calculate(new Money(100m, Eur), rate, new StandardSpreadStrategy());

        act.Should().Throw<DomainException>().WithMessage("*não corresponde à moeda base*");
    }

    [Fact]
    public void ExchangeRate_WithSameBaseAndTargetCurrency_ThrowsDomainException()
    {
        Action act = () => new ExchangeRate(Usd, Usd, 1m, QuoteTime);

        act.Should().Throw<DomainException>().WithMessage("*devem ser diferentes*");
    }

    [Fact]
    public void ExchangeRate_WithNonPositiveRate_ThrowsDomainException()
    {
        Action act = () => new ExchangeRate(Usd, Brl, 0m, QuoteTime);

        act.Should().Throw<DomainException>().WithMessage("*maior que zero*");
    }

    [Fact]
    public void ExchangeRate_WithNonUtcTimestamp_ThrowsDomainException()
    {
        var localTime = new DateTimeOffset(2026, 10, 22, 9, 0, 0, TimeSpan.FromHours(-3));

        Action act = () => new ExchangeRate(Usd, Brl, 5m, localTime);

        act.Should().Throw<DomainException>().WithMessage("*deve estar em UTC*");
    }
}
