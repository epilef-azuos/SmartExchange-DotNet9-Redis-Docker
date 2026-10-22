using SmartExchange.Domain.Enums;

namespace SmartExchange.Domain.Strategies;

public sealed class PremiumSpreadStrategy : SpreadStrategyBase
{
    public PremiumSpreadStrategy() : base(ClientTier.Premium, 0.015m)
    {
    }
}
