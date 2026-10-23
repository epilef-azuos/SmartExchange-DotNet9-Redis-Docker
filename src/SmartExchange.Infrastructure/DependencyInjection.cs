using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using SmartExchange.Application.Common.Interfaces;
using SmartExchange.Infrastructure.Caching;
using SmartExchange.Infrastructure.Http;
using SmartExchange.Infrastructure.Http.Clients;
using SmartExchange.Infrastructure.Persistence;
using SmartExchange.Infrastructure.Persistence.Repositories;

namespace SmartExchange.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<FrankfurterOptions>()
            .Bind(configuration.GetSection(FrankfurterOptions.SectionName));

        services.AddInfrastructureHttpServices();
        services.AddCaching(configuration);
        services.AddAuditPersistence(configuration);

        return services;
    }

    public static IServiceCollection AddInfrastructureHttpServices(this IServiceCollection services)
    {
        services.AddOptions<FrankfurterOptions>();

        // O cliente é registrado pelo tipo concreto: quem resolve IExchangeRateProvider recebe o decorator de cache,
        // e o FrankfurterApiClient não tem conhecimento do cache.
        services.AddHttpClient<FrankfurterApiClient>((serviceProvider, client) =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<FrankfurterOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
            })
            .AddStandardResilienceHandler(ConfigureFrankfurterResilience);

        return services;
    }

    public static void ConfigureFrankfurterResilience(HttpStandardResilienceOptions options)
    {
        options.Retry.MaxRetryAttempts = 3;
        options.Retry.BackoffType = DelayBackoffType.Exponential;
        options.Retry.UseJitter = true;

        options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(5);
        options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(45);

        options.CircuitBreaker.FailureRatio = 0.5;
        options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(10);
        options.CircuitBreaker.MinimumThroughput = 5;
    }

    private static IServiceCollection AddCaching(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
        });

        services.AddTransient<IExchangeRateProvider>(serviceProvider => new CachedExchangeRateProvider(
            serviceProvider.GetRequiredService<FrankfurterApiClient>(),
            serviceProvider.GetRequiredService<IDistributedCache>(),
            serviceProvider.GetRequiredService<ILogger<CachedExchangeRateProvider>>()));

        return services;
    }

    private static IServiceCollection AddAuditPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AuditDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("AuditDb")));

        services.AddScoped<IConversionAuditRepository, ConversionAuditRepository>();

        return services;
    }
}
