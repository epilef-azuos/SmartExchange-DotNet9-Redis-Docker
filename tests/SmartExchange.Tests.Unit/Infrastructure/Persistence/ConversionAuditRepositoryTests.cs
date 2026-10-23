using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SmartExchange.Infrastructure.Persistence;
using SmartExchange.Infrastructure.Persistence.Repositories;

namespace SmartExchange.Tests.Unit.Infrastructure.Persistence;

// Apenas a validação de entrada: as operações de escrita e leitura exigem um PostgreSQL real (teste de integração).
public class ConversionAuditRepositoryTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetRecentAuditsAsync_WithNonPositiveTake_ThrowsArgumentOutOfRange(int take)
    {
        var options = new DbContextOptionsBuilder<AuditDbContext>()
            .UseNpgsql("Host=localhost;Database=smartexchange")
            .Options;
        var repository = new ConversionAuditRepository(new AuditDbContext(options));

        Func<Task> act = () => repository.GetRecentAuditsAsync(take);

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }
}
