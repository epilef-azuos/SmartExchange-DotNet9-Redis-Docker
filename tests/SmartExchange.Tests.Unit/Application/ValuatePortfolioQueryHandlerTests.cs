using FluentAssertions;
using Moq;
using SmartExchange.Application.Common.Exceptions;
using SmartExchange.Application.Common.Interfaces;
using SmartExchange.Application.Exchange.Dtos;
using SmartExchange.Application.Exchange.Queries;
using SmartExchange.Domain.Models;
using SmartExchange.Domain.ValueObjects;

namespace SmartExchange.Tests.Unit.Application;

public class ValuatePortfolioQueryHandlerTests
{
    private static readonly DateTimeOffset QuoteTime = new(2026, 10, 22, 0, 0, 0, TimeSpan.Zero);

    private readonly Mock<IExchangeRateProvider> _rates = new();
    private readonly ValuatePortfolioQueryHandler _sut;

    public ValuatePortfolioQueryHandlerTests()
    {
        _sut = new ValuatePortfolioQueryHandler(_rates.Object);
    }

    [Fact]
    public async Task Handle_ConsolidatesTotalAndSharesThatSumToOneHundred()
    {
        SetupRate("EUR", "USD", 1.10m);
        SetupRate("BRL", "USD", 0.20m);

        var query = new ValuatePortfolioQuery("USD", new List<PortfolioAssetInput>
        {
            new("EUR", 1000m),
            new("BRL", 5000m),
            new("USD", 250m)
        });

        var response = await _sut.Handle(query, CancellationToken.None);

        response.TargetCurrency.Should().Be("USD");
        response.TotalValue.Should().Be(2350.00m);
        response.Assets.Select(a => a.ConvertedAmount).Should().Equal(1100.00m, 1000.00m, 250.00m);
        // 46,8085 / 42,5532 / 10,6383 -> o resto de centésimos vai ao maior resto fracionário (EUR e USD).
        response.Assets.Select(a => a.SharePercentage).Should().Equal(46.81m, 42.55m, 10.64m);
        response.Assets.Sum(a => a.SharePercentage).Should().Be(100.00m);
    }

    [Fact]
    public async Task Handle_DoesNotQueryRateForTargetCurrencyAsset()
    {
        var query = new ValuatePortfolioQuery("USD", new List<PortfolioAssetInput> { new("USD", 250m) });

        var response = await _sut.Handle(query, CancellationToken.None);

        response.Assets.Single().ExchangeRate.Should().Be(1m);
        response.Assets.Single().SharePercentage.Should().Be(100.00m);
        _rates.Verify(p => p.GetLatestRateAsync(It.IsAny<Currency>(), It.IsAny<Currency>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithEmptyAssets_ThrowsValidationException()
    {
        var query = new ValuatePortfolioQuery("USD", new List<PortfolioAssetInput>());

        Func<Task> act = () => _sut.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>().WithMessage("*pelo menos um ativo*");
    }

    [Fact]
    public async Task Handle_WithMissingAssetsList_ThrowsValidationException()
    {
        var query = new ValuatePortfolioQuery("USD", null!);

        Func<Task> act = () => _sut.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>().WithMessage("*pelo menos um ativo*");
    }

    [Fact]
    public async Task Handle_WithNonPositiveAssetAmount_ThrowsValidationException()
    {
        var query = new ValuatePortfolioQuery("USD", new List<PortfolioAssetInput> { new("EUR", 0m) });

        Func<Task> act = () => _sut.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>().WithMessage("*EUR*maior que zero*");
    }

    private void SetupRate(string from, string to, decimal rate)
    {
        var source = new Currency(from);
        var target = new Currency(to);

        _rates.Setup(p => p.GetLatestRateAsync(source, target, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ExchangeRate(source, target, rate, QuoteTime));
    }
}
