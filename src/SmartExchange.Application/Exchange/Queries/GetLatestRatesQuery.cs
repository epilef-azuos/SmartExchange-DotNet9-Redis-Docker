using MediatR;
using SmartExchange.Application.Exchange.Dtos;

namespace SmartExchange.Application.Exchange.Queries;

public sealed record GetLatestRatesQuery(string Base) : IRequest<LatestRatesResponse>;
