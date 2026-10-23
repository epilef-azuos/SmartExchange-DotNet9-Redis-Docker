using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SmartExchange.Application.Common.Interfaces;
using SmartExchange.Application.Exchange.EventHandlers;
using SmartExchange.Application.Exchange.Events;
using SmartExchange.Domain.Entities;
using SmartExchange.Domain.Models;
using SmartExchange.Domain.Strategies;
using SmartExchange.Domain.ValueObjects;

namespace SmartExchange.Tests.Unit.Application;

public class ConversionAuditEventHandlerTests
{
    private readonly Mock<IConversionAuditRepository> _repository = new();
    private readonly ConversionAuditEventHandler _sut;

    public ConversionAuditEventHandlerTests()
    {
        _sut = new ConversionAuditEventHandler(
            _repository.Object,
            TimeProvider.System,
            NullLogger<ConversionAuditEventHandler>.Instance);
    }

    [Fact]
    public async Task Handle_PersistsAuditRecordFromConversion()
    {
        var notification = CreateNotification();
        ConversionAuditLog? persisted = null;
        _repository.Setup(r => r.AddAsync(It.IsAny<ConversionAuditLog>(), It.IsAny<CancellationToken>()))
            .Callback<ConversionAuditLog, CancellationToken>((log, _) => persisted = log)
            .Returns(Task.CompletedTask);

        await _sut.Handle(notification, CancellationToken.None);

        persisted.Should().NotBeNull();
        persisted!.SourceCurrency.Should().Be("USD");
        persisted.TargetCurrency.Should().Be("BRL");
        persisted.ConvertedAmount.Should().Be(5175.00m);
        persisted.SpreadAmount.Should().Be(175.00m);
    }

    [Fact]
    public async Task Handle_WhenPersistenceFails_DoesNotThrow()
    {
        _repository.Setup(r => r.AddAsync(It.IsAny<ConversionAuditLog>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("PostgreSQL indisponível"));

        Func<Task> act = () => _sut.Handle(CreateNotification(), CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    private static ConversionExecutedNotification CreateNotification()
    {
        var quoteTime = new DateTimeOffset(2026, 10, 22, 0, 0, 0, TimeSpan.Zero);
        var rate = new ExchangeRate(new Currency("USD"), new Currency("BRL"), 5.00m, quoteTime);
        var result = ConversionResult.Calculate(new Money(1000m, new Currency("USD")), rate, new StandardSpreadStrategy());

        return new ConversionExecutedNotification(result);
    }
}
