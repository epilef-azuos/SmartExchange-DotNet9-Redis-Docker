using SmartExchange.Domain.Enums;

namespace SmartExchange.Domain.Strategies;

public interface ISpreadStrategy
{
    ClientTier Tier { get; }

    decimal SpreadPercentage { get; }

    decimal CalculateSpreadAmount(decimal baseAmount);

    decimal ApplySpreadToRate(decimal baseRate);
}
