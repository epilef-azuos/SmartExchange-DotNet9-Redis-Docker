using FluentAssertions;
using SmartExchange.Domain.Common;
using SmartExchange.Domain.Enums;
using SmartExchange.Domain.Strategies;

namespace SmartExchange.Tests.Unit.Domain;

public class SpreadStrategyTests
{
    [Fact]
    public void StandardSpreadStrategy_HasThreePointFivePercentSpread()
    {
        var strategy = new StandardSpreadStrategy();

        strategy.Tier.Should().Be(ClientTier.Standard);
        strategy.SpreadPercentage.Should().Be(0.035m);
    }

    [Fact]
    public void PremiumSpreadStrategy_HasOnePointFivePercentSpread()
    {
        var strategy = new PremiumSpreadStrategy();

        strategy.Tier.Should().Be(ClientTier.Premium);
        strategy.SpreadPercentage.Should().Be(0.015m);
    }

    [Fact]
    public void VipSpreadStrategy_HasZeroPointFivePercentSpread()
    {
        var strategy = new VipSpreadStrategy();

        strategy.Tier.Should().Be(ClientTier.Vip);
        strategy.SpreadPercentage.Should().Be(0.005m);
    }

    [Fact]
    public void CalculateSpreadAmount_Standard_ReturnsThreePointFivePercentOfBase()
    {
        new StandardSpreadStrategy().CalculateSpreadAmount(1000m).Should().Be(35.00m);
    }

    [Fact]
    public void CalculateSpreadAmount_Premium_ReturnsOnePointFivePercentOfBase()
    {
        new PremiumSpreadStrategy().CalculateSpreadAmount(1000m).Should().Be(15.00m);
    }

    [Fact]
    public void CalculateSpreadAmount_Vip_ReturnsZeroPointFivePercentOfBase()
    {
        new VipSpreadStrategy().CalculateSpreadAmount(1000m).Should().Be(5.00m);
    }

    [Fact]
    public void CalculateSpreadAmount_RoundsHalfToEvenAtTwoDecimals()
    {
        // 5 * 0.005 = 0.025 — empate: ToEven resulta em 0.02 (AwayFromZero daria 0.03).
        new VipSpreadStrategy().CalculateSpreadAmount(5m).Should().Be(0.02m);
    }

    [Fact]
    public void CalculateSpreadAmount_RoundsNonMidpointUpAtTwoDecimals()
    {
        // 100.50 * 0.035 = 3.5175 -> 3.52
        new StandardSpreadStrategy().CalculateSpreadAmount(100.50m).Should().Be(3.52m);
    }

    [Fact]
    public void CalculateSpreadAmount_WithZeroBase_ReturnsZero()
    {
        new StandardSpreadStrategy().CalculateSpreadAmount(0m).Should().Be(0m);
    }

    [Fact]
    public void CalculateSpreadAmount_WithNegativeBase_ThrowsDomainException()
    {
        Action act = () => new StandardSpreadStrategy().CalculateSpreadAmount(-1m);

        act.Should().Throw<DomainException>().WithMessage("*não pode ser negativo*");
    }

    [Fact]
    public void ApplySpreadToRate_Standard_IncreasesRateByThreePointFivePercent()
    {
        new StandardSpreadStrategy().ApplySpreadToRate(5.00m).Should().Be(5.175000m);
    }

    [Fact]
    public void ApplySpreadToRate_Premium_IncreasesRateByOnePointFivePercent()
    {
        new PremiumSpreadStrategy().ApplySpreadToRate(1.10m).Should().Be(1.116500m);
    }

    [Fact]
    public void ApplySpreadToRate_Vip_IncreasesRateByZeroPointFivePercent()
    {
        new VipSpreadStrategy().ApplySpreadToRate(4.00m).Should().Be(4.020000m);
    }

    [Fact]
    public void ApplySpreadToRate_RoundsToSixDecimals()
    {
        // 1.234567 * 1.035 = 1.277776845 -> 1.277777
        new StandardSpreadStrategy().ApplySpreadToRate(1.234567m).Should().Be(1.277777m);
    }

    [Fact]
    public void ApplySpreadToRate_WithZeroRate_ThrowsDomainException()
    {
        Action act = () => new StandardSpreadStrategy().ApplySpreadToRate(0m);

        act.Should().Throw<DomainException>().WithMessage("*maior que zero*");
    }

    [Fact]
    public void ApplySpreadToRate_WithNegativeRate_ThrowsDomainException()
    {
        Action act = () => new StandardSpreadStrategy().ApplySpreadToRate(-1m);

        act.Should().Throw<DomainException>().WithMessage("*maior que zero*");
    }

    [Fact]
    public void Resolver_Standard_ReturnsStandardStrategy()
    {
        var strategy = new SpreadStrategyResolver().Resolve(ClientTier.Standard);

        strategy.Should().BeOfType<StandardSpreadStrategy>();
    }

    [Fact]
    public void Resolver_Premium_ReturnsPremiumStrategy()
    {
        var strategy = new SpreadStrategyResolver().Resolve(ClientTier.Premium);

        strategy.Should().BeOfType<PremiumSpreadStrategy>();
    }

    [Fact]
    public void Resolver_Vip_ReturnsVipStrategy()
    {
        var strategy = new SpreadStrategyResolver().Resolve(ClientTier.Vip);

        strategy.Should().BeOfType<VipSpreadStrategy>();
    }

    [Fact]
    public void Resolver_WithUndefinedTier_ThrowsDomainException()
    {
        Action act = () => new SpreadStrategyResolver().Resolve((ClientTier)99);

        act.Should().Throw<DomainException>().WithMessage("*sem estratégia de spread*");
    }
}
