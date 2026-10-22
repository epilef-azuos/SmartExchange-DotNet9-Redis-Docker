using SmartExchange.Domain.Common;
using SmartExchange.Domain.Enums;

namespace SmartExchange.Domain.Strategies;

public abstract class SpreadStrategyBase : ISpreadStrategy
{
    protected SpreadStrategyBase(ClientTier tier, decimal spreadPercentage)
    {
        Tier = tier;
        SpreadPercentage = spreadPercentage;
    }

    public ClientTier Tier { get; }

    public decimal SpreadPercentage { get; }

    public decimal CalculateSpreadAmount(decimal baseAmount)
    {
        if (baseAmount < 0)
        {
            throw new DomainException("O montante base para o spread não pode ser negativo.");
        }

        return FinancialRounding.RoundAmount(baseAmount * SpreadPercentage);
    }

    public decimal ApplySpreadToRate(decimal baseRate)
    {
        if (baseRate <= 0)
        {
            throw new DomainException("A taxa base deve ser maior que zero.");
        }

        return FinancialRounding.RoundRate(baseRate * (1 + SpreadPercentage));
    }
}
