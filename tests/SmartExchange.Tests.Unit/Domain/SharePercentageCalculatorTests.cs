using FluentAssertions;
using SmartExchange.Domain.Services;

namespace SmartExchange.Tests.Unit.Domain;

public class SharePercentageCalculatorTests
{
    [Fact]
    public void Calculate_WithZeroTotal_ReturnsZeroShares()
    {
        var shares = SharePercentageCalculator.Calculate(new[] { 0m, 0m });

        shares.Should().Equal(0m, 0m);
    }

    [Fact]
    public void Calculate_WithEqualThirds_DistributesRemainderSoSharesSumToOneHundred()
    {
        var shares = SharePercentageCalculator.Calculate(new[] { 100m, 100m, 100m });

        shares.Should().Equal(33.34m, 33.33m, 33.33m);
        shares.Sum().Should().Be(100.00m);
    }

    [Fact]
    public void Calculate_WithSingleAsset_ReturnsOneHundredWithTwoDecimals()
    {
        var shares = SharePercentageCalculator.Calculate(new[] { 2350.00m });

        shares.Should().Equal(100.00m);
        shares[0].ToString(System.Globalization.CultureInfo.InvariantCulture).Should().Be("100.00");
    }

    [Theory]
    [MemberData(nameof(CarteirasArbitrarias))]
    public void Calculate_AlwaysSumsToExactlyOneHundred(decimal[] amounts)
    {
        var shares = SharePercentageCalculator.Calculate(amounts);

        shares.Sum().Should().Be(100.00m);
        shares.Should().OnlyContain(s => s >= 0m && s <= 100m);
    }

    public static IEnumerable<object[]> CarteirasArbitrarias()
    {
        yield return new object[] { new[] { 1.00m, 2.00m, 3.00m, 4.00m, 5.00m, 6.00m, 7.00m } };
        yield return new object[] { new[] { 999.99m, 0.01m, 0.01m } };
        yield return new object[] { new[] { 123.45m, 678.90m, 0.5m, 77.77m } };
    }
}
