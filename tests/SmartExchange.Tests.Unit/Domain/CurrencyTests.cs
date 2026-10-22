using FluentAssertions;
using SmartExchange.Domain.Common;
using SmartExchange.Domain.ValueObjects;

namespace SmartExchange.Tests.Unit.Domain;

public class CurrencyTests
{
    [Theory]
    [InlineData("USD", "USD")]
    [InlineData("usd", "USD")]
    [InlineData("eUr", "EUR")]
    [InlineData(" brl ", "BRL")]
    public void Constructor_WithValidIsoCode_NormalizesToUpperCase(string input, string expected)
    {
        var currency = new Currency(input);

        currency.Code.Should().Be(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("US")]
    [InlineData("USDD")]
    [InlineData("US1")]
    [InlineData("U$D")]
    [InlineData("ÇÃO")]
    public void Constructor_WithInvalidCode_ThrowsDomainException(string input)
    {
        Action act = () => new Currency(input);

        act.Should().Throw<DomainException>().WithMessage("*Código de moeda inválido*");
    }

    [Fact]
    public void Constructor_WithNull_ThrowsDomainException()
    {
        Action act = () => new Currency(null!);

        act.Should().Throw<DomainException>().WithMessage("*Código de moeda inválido*");
    }

    [Fact]
    public void Equality_IgnoresInputCase()
    {
        var lower = new Currency("usd");
        var upper = new Currency("USD");

        lower.Should().Be(upper);
    }

    [Fact]
    public void ToString_ReturnsIsoCode()
    {
        new Currency("brl").ToString().Should().Be("BRL");
    }
}
