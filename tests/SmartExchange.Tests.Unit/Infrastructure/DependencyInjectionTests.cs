using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartExchange.Application.Common.Interfaces;
using SmartExchange.Infrastructure;
using SmartExchange.Infrastructure.Caching;

namespace SmartExchange.Tests.Unit.Infrastructure;

public class DependencyInjectionTests
{
    [Fact]
    public void AddInfrastructure_RegistersCachedDecoratorAsExchangeRateProvider()
    {
        var configuration = new ConfigurationManager();
        configuration["ConnectionStrings:Redis"] = "localhost:6379";
        configuration["ConnectionStrings:AuditDb"] = "Host=localhost;Database=smartexchange;Username=smartexchange;Password=smartexchange";

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddInfrastructure(configuration);

        using var provider = services.BuildServiceProvider();

        provider.GetRequiredService<IExchangeRateProvider>().Should().BeOfType<CachedExchangeRateProvider>();
    }
}
