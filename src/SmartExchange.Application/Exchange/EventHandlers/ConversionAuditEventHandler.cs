using MediatR;
using Microsoft.Extensions.Logging;
using SmartExchange.Application.Common.Interfaces;
using SmartExchange.Application.Exchange.Events;
using SmartExchange.Domain.Entities;

namespace SmartExchange.Application.Exchange.EventHandlers;

// Persiste a auditoria fora do fluxo de resposta da conversão. Uma falha de persistência
// é registrada em log e não invalida a conversão já calculada e entregue ao cliente.
public sealed class ConversionAuditEventHandler : INotificationHandler<ConversionExecutedNotification>
{
    private readonly IConversionAuditRepository _repository;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<ConversionAuditEventHandler> _logger;

    public ConversionAuditEventHandler(
        IConversionAuditRepository repository,
        TimeProvider timeProvider,
        ILogger<ConversionAuditEventHandler> logger)
    {
        _repository = repository;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task Handle(ConversionExecutedNotification notification, CancellationToken cancellationToken)
    {
        var log = ConversionAuditLog.FromConversion(
            Guid.NewGuid(),
            notification.Result,
            _timeProvider.GetUtcNow());

        try
        {
            await _repository.AddAsync(log, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Falha ao persistir a auditoria {AuditId} da conversão.", log.Id);
        }
    }
}
