using FluentAssertions;
using SmartExchange.Domain.Entities;
using SmartExchange.Domain.Models;
using SmartExchange.Domain.Strategies;
using SmartExchange.Domain.ValueObjects;

namespace SmartExchange.Tests.Unit.Domain;

public class ConversionAuditLogTests
{
    [Fact]
    public void FromConversion_MapsEveryConversionFieldToAuditRecord()
    {
        var id = Guid.NewGuid();
        var createdAtUtc = new DateTimeOffset(2026, 10, 22, 12, 30, 0, TimeSpan.Zero);
        var rate = new ExchangeRate(new Currency("USD"), new Currency("BRL"), 5.00m, createdAtUtc);
        var result = ConversionResult.Calculate(new Money(1000m, new Currency("USD")), rate, new StandardSpreadStrategy());

        var log = ConversionAuditLog.FromConversion(id, result, createdAtUtc);

        log.Id.Should().Be(id);
        log.ClientTier.Should().Be("Standard");
        log.SourceCurrency.Should().Be("USD");
        log.TargetCurrency.Should().Be("BRL");
        log.BaseAmount.Should().Be(1000m);
        log.ConvertedAmount.Should().Be(5175.00m);
        log.BaseRate.Should().Be(5.00m);
        log.FinalRateWithSpread.Should().Be(5.175000m);
        log.SpreadPercentage.Should().Be(0.035m);
        log.SpreadAmount.Should().Be(175.00m);
        log.CreatedAtUtc.Should().Be(createdAtUtc);
    }
}
