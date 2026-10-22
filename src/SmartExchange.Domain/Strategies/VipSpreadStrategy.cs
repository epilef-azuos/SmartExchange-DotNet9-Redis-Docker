using SmartExchange.Domain.Enums;

namespace SmartExchange.Domain.Strategies;

public sealed class VipSpreadStrategy : SpreadStrategyBase
{
    public VipSpreadStrategy() : base(ClientTier.Vip, 0.005m)
    {
    }
}
