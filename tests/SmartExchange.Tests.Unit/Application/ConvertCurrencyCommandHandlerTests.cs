using FluentAssertions;
using MediatR;
using Moq;
using SmartExchange.Application.Common.Exceptions;
using SmartExchange.Application.Common.Interfaces;
using SmartExchange.Application.Exchange.Commands;
using SmartExchange.Application.Exchange.Events;
using SmartExchange.Domain.Common;
using SmartExchange.Domain.Enums;
using SmartExchange.Domain.Models;
using SmartExchange.Domain.Strategies;
using SmartExchange.Domain.ValueObjects;

namespace SmartExchange.Tests.Unit.Application;

public class ConvertCurrencyCommandHandlerTests
{
    private static readonly DateTimeOffset QuoteTime = new(2026, 10, 22, 0, 0, 0, TimeSpan.Zero);

    private readonly Mock<IExchangeRateProvider> _rates = new();
    private readonly Mock<IPublisher> _publisher = new();
    private readonly ConvertCurrencyCommandHandler _sut;

    public ConvertCurrencyCommandHandlerTests()
    {
        _sut = new ConvertCurrencyCommandHandler(_rates.Object, new SpreadStrategyResolver(), _publisher.Object);
    }

    [Fact]
    public async Task Handle_WithDefaultTier_ReturnsConversionWithStandardSpreadAndPublishesNotification()
    {
        SetupRate("USD", "BRL", 5.00m);

        var response = await _sut.Handle(new ConvertCurrencyCommand("usd", "brl", 1000m, null), CancellationToken.None);

        response.SourceCurrency.Should().Be("USD");
        response.TargetCurrency.Should().Be("BRL");
        response.TargetAmount.Should().Be(5175.00m);
        response.SpreadAmount.Should().Be(175.00m);
        response.BaseRate.Should().Be(5.00m);
        response.RateWithSpread.Should().Be(5.175000m);
        response.SpreadPercentage.Should().Be(0.035m);
        response.ClientTier.Should().Be(ClientTier.Standard);

        _publisher.Verify(
            p => p.Publish(It.Is<ConversionExecutedNotification>(n => n.Result.Target.Amount == 5175.00m), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithPremiumTierIgnoringCase_AppliesOnePointFivePercentSpread()
    {
        SetupRate("USD", "BRL", 5.00m);

        var response = await _sut.Handle(new ConvertCurrencyCommand("USD", "BRL", 1000m, "premium"), CancellationToken.None);

        response.ClientTier.Should().Be(ClientTier.Premium);
        response.SpreadAmount.Should().Be(75.00m);
        response.TargetAmount.Should().Be(5075.00m);
        response.RateWithSpread.Should().Be(5.075000m);
    }

    [Fact]
    public async Task Handle_WithUnknownTier_ThrowsValidationExceptionWithoutQueryingRatesOrPublishing()
    {
        Func<Task> act = () => _sut.Handle(new ConvertCurrencyCommand("USD", "BRL", 1000m, "Gold"), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>().WithMessage("*Perfil de cliente inválido*");
        _rates.Verify(p => p.GetLatestRateAsync(It.IsAny<Currency>(), It.IsAny<Currency>(), It.IsAny<CancellationToken>()), Times.Never);
        _publisher.Verify(p => p.Publish(It.IsAny<ConversionExecutedNotification>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public async Task Handle_WithNonPositiveAmount_ThrowsValidationException(decimal amount)
    {
        Func<Task> act = () => _sut.Handle(new ConvertCurrencyCommand("USD", "BRL", amount, null), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>().WithMessage("*maior que zero*");
    }

    [Fact]
    public async Task Handle_WithInvalidCurrencyCode_ThrowsDomainException()
    {
        Func<Task> act = () => _sut.Handle(new ConvertCurrencyCommand("US", "BRL", 1000m, null), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*Código de moeda inválido*");
    }

    [Fact]
    public async Task Handle_WithSameSourceAndTargetCurrency_ThrowsDomainException()
    {
        Func<Task> act = () => _sut.Handle(new ConvertCurrencyCommand("USD", "usd", 1000m, null), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*devem ser diferentes*");
        _publisher.Verify(p => p.Publish(It.IsAny<ConversionExecutedNotification>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenProviderFails_PropagatesErrorWithoutPublishing()
    {
        _rates.Setup(p => p.GetLatestRateAsync(It.IsAny<Currency>(), It.IsAny<Currency>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ExchangeIntegrationException("Falha de comunicação com a Frankfurter API."));

        Func<Task> act = () => _sut.Handle(new ConvertCurrencyCommand("USD", "BRL", 1000m, null), CancellationToken.None);

        await act.Should().ThrowAsync<ExchangeIntegrationException>();
        _publisher.Verify(p => p.Publish(It.IsAny<ConversionExecutedNotification>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private void SetupRate(string from, string to, decimal rate)
    {
        var source = new Currency(from);
        var target = new Currency(to);

        _rates.Setup(p => p.GetLatestRateAsync(source, target, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ExchangeRate(source, target, rate, QuoteTime));
    }
}
