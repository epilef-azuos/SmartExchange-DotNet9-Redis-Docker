using SmartExchange.Domain.Entities;

namespace SmartExchange.Application.Common.Interfaces;

// Somente escrita e leitura histórica: a auditoria não expõe atualização nem remoção.
public interface IConversionAuditRepository
{
    Task AddAsync(ConversionAuditLog log, CancellationToken ct = default);

    Task<IReadOnlyList<ConversionAuditLog>> GetRecentAuditsAsync(int take = 50, CancellationToken ct = default);
}
