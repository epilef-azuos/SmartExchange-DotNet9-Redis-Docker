using FluentAssertions;
using SmartExchange.Domain.Common;
using SmartExchange.Domain.ValueObjects;

namespace SmartExchange.Tests.Unit.Domain;

public class MoneyTests
{
    private static readonly Currency Usd = new("USD");
    private static readonly Currency Brl = new("BRL");

    [Fact]
    public void Constructor_WithZeroAmount_IsAllowed()
    {
        var money = new Money(0m, Usd);

        money.Amount.Should().Be(0m);
    }

    [Fact]
    public void Constructor_WithNegativeAmount_ThrowsDomainException()
    {
        Action act = () => new Money(-0.01m, Usd);

        act.Should().Throw<DomainException>().WithMessage("*não pode ser negativo*");
    }

    [Fact]
    public void Constructor_WithNullCurrency_ThrowsDomainException()
    {
        Action act = () => new Money(10m, null!);

        act.Should().Throw<DomainException>().WithMessage("*moeda é obrigatória*");
    }

    [Fact]
    public void Add_WithSameCurrency_SumsAmounts()
    {
        var result = new Money(10m, Usd).Add(new Money(5.5m, Usd));

        result.Should().Be(new Money(15.5m, Usd));
    }

    [Fact]
    public void Add_WithDifferentCurrency_ThrowsDomainException()
    {
        var usd = new Money(10m, Usd);
        var brl = new Money(5m, Brl);

        Action act = () => usd.Add(brl);

        act.Should().Throw<DomainException>().WithMessage("*moedas diferentes*");
    }

    [Fact]
    public void Subtract_WithSameCurrency_SubtractsAmounts()
    {
        var result = new Money(10m, Usd).Subtract(new Money(4m, Usd));

        result.Should().Be(new Money(6m, Usd));
    }

    [Fact]
    public void Subtract_WithDifferentCurrency_ThrowsDomainException()
    {
        Action act = () => new Money(10m, Usd).Subtract(new Money(4m, Brl));

        act.Should().Throw<DomainException>().WithMessage("*moedas diferentes*");
    }

    [Fact]
    public void Subtract_ResultingInNegativeAmount_ThrowsDomainException()
    {
        Action act = () => new Money(4m, Usd).Subtract(new Money(10m, Usd));

        act.Should().Throw<DomainException>().WithMessage("*não pode ser negativo*");
    }

    [Fact]
    public void Multiply_ByPositiveFactor_ScalesAmount()
    {
        var result = new Money(10m, Usd).Multiply(1.5m);

        result.Should().Be(new Money(15m, Usd));
    }

    [Fact]
    public void Multiply_ByNegativeFactor_ThrowsDomainException()
    {
        Action act = () => new Money(10m, Usd).Multiply(-1m);

        act.Should().Throw<DomainException>().WithMessage("*não pode ser negativo*");
    }

    [Fact]
    public void Divide_ByPositiveDivisor_DividesAmount()
    {
        var result = new Money(10m, Usd).Divide(4m);

        result.Should().Be(new Money(2.5m, Usd));
    }

    [Fact]
    public void Divide_ByZero_ThrowsDomainException()
    {
        Action act = () => new Money(10m, Usd).Divide(0m);

        act.Should().Throw<DomainException>().WithMessage("*Divisão*por zero*");
    }

    [Fact]
    public void Operators_DelegateToNamedOperations()
    {
        var a = new Money(10m, Usd);
        var b = new Money(4m, Usd);

        (a + b).Should().Be(a.Add(b));
        (a - b).Should().Be(a.Subtract(b));
        (a * 2m).Should().Be(a.Multiply(2m));
        (a / 2m).Should().Be(a.Divide(2m));
    }

    [Fact]
    public void Operations_DoNotMutateOriginalInstances()
    {
        var original = new Money(10m, Usd);
        var other = new Money(3m, Usd);

        _ = original.Add(other);
        _ = original.Subtract(other);
        _ = original.Multiply(5m);
        _ = original.Divide(2m);

        original.Amount.Should().Be(10m);
        original.Currency.Should().Be(Usd);
        other.Amount.Should().Be(3m);
    }

    [Fact]
    public void Equality_ComparesAmountAndCurrency()
    {
        new Money(10m, Usd).Should().Be(new Money(10m, Usd));
        new Money(10m, Usd).Should().NotBe(new Money(10m, Brl));
        new Money(10m, Usd).Should().NotBe(new Money(11m, Usd));
    }
}
