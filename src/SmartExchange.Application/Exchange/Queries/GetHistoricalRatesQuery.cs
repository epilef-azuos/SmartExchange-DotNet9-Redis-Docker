using MediatR;
using SmartExchange.Application.Exchange.Dtos;

namespace SmartExchange.Application.Exchange.Queries;

public sealed record GetHistoricalRatesQuery(string Base, string Target, string? Period) : IRequest<HistoricalRatesResponse>;
