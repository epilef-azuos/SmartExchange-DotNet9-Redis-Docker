using SmartExchange.Domain.Entities;

namespace SmartExchange.Application.Exchange.Dtos;

public sealed record AuditLogDto(
    Guid Id,
    string ClientTier,
    string SourceCurrency,
    string TargetCurrency,
    decimal BaseAmount,
    decimal ConvertedAmount,
    decimal BaseRate,
    decimal FinalRateWithSpread,
    decimal SpreadPercentage,
    decimal SpreadAmount,
    DateTimeOffset CreatedAtUtc)
{
    public static AuditLogDto From(ConversionAuditLog log) => new(
        log.Id,
        log.ClientTier,
        log.SourceCurrency,
        log.TargetCurrency,
        log.BaseAmount,
        log.ConvertedAmount,
        log.BaseRate,
        log.FinalRateWithSpread,
        log.SpreadPercentage,
        log.SpreadAmount,
        log.CreatedAtUtc);
}
