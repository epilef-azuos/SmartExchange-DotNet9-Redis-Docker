using Microsoft.EntityFrameworkCore;
using SmartExchange.Domain.Entities;
using SmartExchange.Infrastructure.Persistence.Configurations;

namespace SmartExchange.Infrastructure.Persistence;

public sealed class AuditDbContext : DbContext
{
    public AuditDbContext(DbContextOptions<AuditDbContext> options) : base(options)
    {
    }

    public DbSet<ConversionAuditLog> ConversionAuditLogs => Set<ConversionAuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ConversionAuditLogConfiguration());
    }
}
