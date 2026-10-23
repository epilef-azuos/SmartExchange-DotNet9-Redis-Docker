using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SmartExchange.Application;
using SmartExchange.Application.Common.Interfaces;
using SmartExchange.Application.Exchange.Commands;
using SmartExchange.Application.Exchange.Dtos;
using SmartExchange.Application.Exchange.Queries;
using SmartExchange.Domain.Strategies;

namespace SmartExchange.Tests.Unit.Application;

public class DependencyInjectionTests
{
    [Fact]
    public void AddApplication_ResolvesConvertCurrencyHandlerWithAllDependencies()
    {
        var services = new ServiceCollection();
        services.AddSingleton(new Mock<IExchangeRateProvider>().Object);
        services.AddSingleton(new Mock<IConversionAuditRepository>().Object);
        services.AddLogging();
        services.AddApplication();

        using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true });

        provider.GetRequiredService<IRequestHandler<ConvertCurrencyCommand, ConversionResponse>>()
            .Should().BeOfType<SmartExchange.Application.Exchange.Commands.ConvertCurrencyCommandHandler>();
        provider.GetRequiredService<ISpreadStrategyResolver>().Should().BeOfType<SpreadStrategyResolver>();
    }

    [Fact]
    public void AddApplication_ResolvesAllRegisteredRequestHandlers()
    {
        var services = new ServiceCollection();
        services.AddSingleton(new Mock<IExchangeRateProvider>().Object);
        services.AddSingleton(new Mock<IConversionAuditRepository>().Object);
        services.AddLogging();
        services.AddApplication();

        using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true });

        provider.GetRequiredService<IRequestHandler<GetCurrenciesQuery, CurrenciesResponse>>().Should().NotBeNull();
        provider.GetRequiredService<IRequestHandler<GetLatestRatesQuery, LatestRatesResponse>>().Should().NotBeNull();
    }
}
