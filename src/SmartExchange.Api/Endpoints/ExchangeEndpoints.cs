using MediatR;
using Microsoft.AspNetCore.Mvc;
using SmartExchange.Application.Exchange.Commands;
using SmartExchange.Application.Exchange.Queries;

namespace SmartExchange.Api.Endpoints;

public static class ExchangeEndpoints
{
    public static IEndpointRouteBuilder MapExchangeEndpoints(this IEndpointRouteBuilder app)
    {
        var exchange = app.MapGroup("/api/v1/exchange").WithTags("Exchange");

        exchange.MapGet("/convert", async (
                [FromQuery] string from,
                [FromQuery] string to,
                [FromQuery] decimal amount,
                [FromQuery] string? tier,
                ISender sender,
                CancellationToken cancellationToken) =>
            Results.Ok(await sender.Send(new ConvertCurrencyCommand(from, to, amount, tier), cancellationToken)))
            .WithName("ConvertCurrency");

        exchange.MapPost("/portfolio/valuate", async (
                ValuatePortfolioQuery query,
                ISender sender,
                CancellationToken cancellationToken) =>
            Results.Ok(await sender.Send(query, cancellationToken)))
            .WithName("ValuatePortfolio");

        exchange.MapGet("/history", async (
                [FromQuery(Name = "base")] string baseCurrency,
                [FromQuery] string target,
                [FromQuery] string? period,
                ISender sender,
                CancellationToken cancellationToken) =>
            Results.Ok(await sender.Send(new GetHistoricalRatesQuery(baseCurrency, target, period), cancellationToken)))
            .WithName("GetHistoricalRates");

        exchange.MapGet("/latest", async (
                [FromQuery(Name = "base")] string? baseCurrency,
                ISender sender,
                CancellationToken cancellationToken) =>
            Results.Ok(await sender.Send(new GetLatestRatesQuery(baseCurrency ?? "USD"), cancellationToken)))
            .WithName("GetLatestRates");

        exchange.MapGet("/currencies", async (
                ISender sender,
                CancellationToken cancellationToken) =>
            Results.Ok(await sender.Send(new GetCurrenciesQuery(), cancellationToken)))
            .WithName("GetCurrencies");

        exchange.MapGet("/audit", async (
                [FromQuery] int? take,
                ISender sender,
                CancellationToken cancellationToken) =>
            Results.Ok(await sender.Send(new GetAuditLogsQuery(take ?? 50), cancellationToken)))
            .WithName("GetAuditLogs");

        return app;
    }
}
