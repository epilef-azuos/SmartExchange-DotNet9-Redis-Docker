using MediatR;
using SmartExchange.Application.Exchange.Dtos;

namespace SmartExchange.Application.Exchange.Queries;

public sealed record GetAuditLogsQuery(int Take) : IRequest<IReadOnlyList<AuditLogDto>>;
