using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartExchange.Infrastructure.Http.Clients;
using SmartExchange.Infrastructure.Persistence;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace SmartExchange.Tests.Integration.Fixtures;

// Sobe PostgreSQL e Redis descartáveis (portas dinâmicas), aplica as migrations e
// aponta a API para esses contêineres. Os contêineres são removidos ao final da suíte.
public sealed class SmartExchangeApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("smartexchange")
        .WithUsername("smartexchange")
        .WithPassword("smartexchange")
        .Build();

    private readonly RedisContainer _redis = new RedisBuilder()
        .WithImage("redis:7-alpine")
        .Build();

    public FakeFrankfurterHandler FrankfurterHandler { get; } = new();

    public string RedisConnectionString => _redis.GetConnectionString();

    public async Task InitializeAsync()
    {
        await Task.WhenAll(_postgres.StartAsync(), _redis.StartAsync());

        // Criar o host só depois de os contêineres estarem prontos garante que a configuração já tenha as portas dinâmicas.
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AuditDbContext>();
        await context.Database.MigrateAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();
        await _redis.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:AuditDb"] = _postgres.GetConnectionString(),
                ["ConnectionStrings:Redis"] = _redis.GetConnectionString()
            });
        });

        builder.ConfigureTestServices(services =>
        {
            services.AddHttpClient<FrankfurterApiClient>()
                .ConfigurePrimaryHttpMessageHandler(() => FrankfurterHandler);
        });
    }
}

[CollectionDefinition(Name)]
public sealed class IntegrationCollection : ICollectionFixture<SmartExchangeApiFactory>
{
    public const string Name = "Integration";
}
