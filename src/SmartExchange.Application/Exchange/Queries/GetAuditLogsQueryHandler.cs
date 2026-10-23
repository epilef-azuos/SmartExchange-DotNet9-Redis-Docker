using MediatR;
using SmartExchange.Application.Common.Exceptions;
using SmartExchange.Application.Common.Interfaces;
using SmartExchange.Application.Exchange.Dtos;

namespace SmartExchange.Application.Exchange.Queries;

public sealed class GetAuditLogsQueryHandler : IRequestHandler<GetAuditLogsQuery, IReadOnlyList<AuditLogDto>>
{
    private const int MaxTake = 100;

    private readonly IConversionAuditRepository _repository;

    public GetAuditLogsQueryHandler(IConversionAuditRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<AuditLogDto>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        if (request.Take is < 1 or > MaxTake)
        {
            throw new ValidationException($"O parâmetro 'take' deve estar entre 1 e {MaxTake}.");
        }

        var logs = await _repository.GetRecentAuditsAsync(request.Take, cancellationToken);

        return logs.Select(AuditLogDto.From).ToList();
    }
}
