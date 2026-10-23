using Microsoft.EntityFrameworkCore;
using SmartExchange.Application.Common.Interfaces;
using SmartExchange.Domain.Entities;

namespace SmartExchange.Infrastructure.Persistence.Repositories;

// Append-only: não expõe update nem delete.
public sealed class ConversionAuditRepository : IConversionAuditRepository
{
    private readonly AuditDbContext _context;

    public ConversionAuditRepository(AuditDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(ConversionAuditLog log, CancellationToken ct = default)
    {
        await _context.ConversionAuditLogs.AddAsync(log, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<ConversionAuditLog>> GetRecentAuditsAsync(int take = 50, CancellationToken ct = default)
    {
        if (take <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(take), "O número de registros deve ser maior que zero.");
        }

        return await _context.ConversionAuditLogs
            .AsNoTracking()
            .OrderByDescending(log => log.CreatedAtUtc)
            .Take(take)
            .ToListAsync(ct);
    }
}
