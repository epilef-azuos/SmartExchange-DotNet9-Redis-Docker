using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using Polly;
using SmartExchange.Application.Common.Interfaces;
using SmartExchange.Infrastructure.Http;
using SmartExchange.Infrastructure.Http.Clients;

namespace SmartExchange.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureHttpServices(this IServiceCollection services)
    {
        services.AddOptions<FrankfurterOptions>()
            .BindConfiguration(FrankfurterOptions.SectionName);

        services.AddHttpClient<IExchangeRateProvider, FrankfurterApiClient>((serviceProvider, client) =>
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
}
