using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SmartExchange.Domain.Entities;
using SmartExchange.Infrastructure.Persistence;

namespace SmartExchange.Tests.Unit.Infrastructure.Persistence;

// Valida o modelo do EF Core sem abrir conexão com o PostgreSQL.
public class AuditDbContextModelTests
{
    private static readonly AuditDbContext Context = CreateContext();

    [Fact]
    public void ConversionAuditLog_MapsToAuditTable()
    {
        var entity = Context.Model.FindEntityType(typeof(ConversionAuditLog));

        entity!.GetTableName().Should().Be("conversion_audit_logs");
    }

    [Theory]
    [InlineData(nameof(ConversionAuditLog.BaseAmount), 18, 2)]
    [InlineData(nameof(ConversionAuditLog.ConvertedAmount), 18, 2)]
    [InlineData(nameof(ConversionAuditLog.SpreadAmount), 18, 2)]
    [InlineData(nameof(ConversionAuditLog.BaseRate), 18, 6)]
    [InlineData(nameof(ConversionAuditLog.FinalRateWithSpread), 18, 6)]
    [InlineData(nameof(ConversionAuditLog.SpreadPercentage), 18, 6)]
    public void DecimalColumns_HaveExplicitPrecision(string propertyName, int precision, int scale)
    {
        var property = Context.Model.FindEntityType(typeof(ConversionAuditLog))!.FindProperty(propertyName);

        property!.GetPrecision().Should().Be(precision);
        property.GetScale().Should().Be(scale);
    }

    private static AuditDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AuditDbContext>()
            .UseNpgsql("Host=localhost;Database=smartexchange")
            .Options;

        return new AuditDbContext(options);
    }
}
