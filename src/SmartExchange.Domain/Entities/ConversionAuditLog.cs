using SmartExchange.Domain.Models;

namespace SmartExchange.Domain.Entities;

// Registro append-only: não há setters públicos nem métodos de alteração.
public sealed class ConversionAuditLog
{
    // Construtor exigido pelo EF Core para materialização.
    private ConversionAuditLog()
    {
        ClientTier = string.Empty;
        SourceCurrency = string.Empty;
        TargetCurrency = string.Empty;
    }

    private ConversionAuditLog(
        Guid id,
        string clientTier,
        string sourceCurrency,
        string targetCurrency,
        decimal baseAmount,
        decimal convertedAmount,
        decimal baseRate,
        decimal finalRateWithSpread,
        decimal spreadPercentage,
        decimal spreadAmount,
        DateTimeOffset createdAtUtc)
    {
        Id = id;
        ClientTier = clientTier;
        SourceCurrency = sourceCurrency;
        TargetCurrency = targetCurrency;
        BaseAmount = baseAmount;
        ConvertedAmount = convertedAmount;
        BaseRate = baseRate;
        FinalRateWithSpread = finalRateWithSpread;
        SpreadPercentage = spreadPercentage;
        SpreadAmount = spreadAmount;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; }

    public string ClientTier { get; }

    public string SourceCurrency { get; }

    public string TargetCurrency { get; }

    public decimal BaseAmount { get; }

    public decimal ConvertedAmount { get; }

    public decimal BaseRate { get; }

    public decimal FinalRateWithSpread { get; }

    public decimal SpreadPercentage { get; }

    public decimal SpreadAmount { get; }

    public DateTimeOffset CreatedAtUtc { get; }

    public static ConversionAuditLog FromConversion(Guid id, ConversionResult result, DateTimeOffset createdAtUtc)
    {
        return new ConversionAuditLog(
            id,
            result.ClientTier.ToString(),
            result.Source.Currency.Code,
            result.Target.Currency.Code,
            result.Source.Amount,
            result.Target.Amount,
            result.BaseRate,
            result.RateWithSpread,
            result.SpreadPercentage,
            result.SpreadAmount,
            createdAtUtc);
    }
}
