using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SmartExchange.Domain.Strategies;

namespace SmartExchange.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
        });

        services.TryAddSingleton(TimeProvider.System);
        services.TryAddSingleton<ISpreadStrategyResolver, SpreadStrategyResolver>();

        return services;
    }
}
