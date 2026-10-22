using SmartExchange.Domain.Enums;

namespace SmartExchange.Domain.Strategies;

public sealed class StandardSpreadStrategy : SpreadStrategyBase
{
    public StandardSpreadStrategy() : base(ClientTier.Standard, 0.035m)
    {
    }
}
