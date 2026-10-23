using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartExchange.Domain.Entities;

namespace SmartExchange.Infrastructure.Persistence.Configurations;

public sealed class ConversionAuditLogConfiguration : IEntityTypeConfiguration<ConversionAuditLog>
{
    public void Configure(EntityTypeBuilder<ConversionAuditLog> builder)
    {
        builder.ToTable("conversion_audit_logs");

        builder.HasKey(log => log.Id);

        builder.Property(log => log.ClientTier).HasMaxLength(20).IsRequired();
        builder.Property(log => log.SourceCurrency).HasMaxLength(3).IsRequired();
        builder.Property(log => log.TargetCurrency).HasMaxLength(3).IsRequired();

        // Quantias monetárias: 2 casas decimais.
        builder.Property(log => log.BaseAmount).HasPrecision(18, 2);
        builder.Property(log => log.ConvertedAmount).HasPrecision(18, 2);
        builder.Property(log => log.SpreadAmount).HasPrecision(18, 2);

        // Taxas e percentuais: 6 casas decimais.
        builder.Property(log => log.BaseRate).HasPrecision(18, 6);
        builder.Property(log => log.FinalRateWithSpread).HasPrecision(18, 6);
        builder.Property(log => log.SpreadPercentage).HasPrecision(18, 6);

        builder.Property(log => log.CreatedAtUtc).IsRequired();

        builder.HasIndex(log => log.CreatedAtUtc);
    }
}
