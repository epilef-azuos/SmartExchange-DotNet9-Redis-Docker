using SmartExchange.Domain.Common;
using SmartExchange.Domain.Enums;

namespace SmartExchange.Domain.Strategies;

public sealed class SpreadStrategyResolver : ISpreadStrategyResolver
{
    public ISpreadStrategy Resolve(ClientTier tier) => tier switch
    {
        ClientTier.Standard => new StandardSpreadStrategy(),
        ClientTier.Premium => new PremiumSpreadStrategy(),
        ClientTier.Vip => new VipSpreadStrategy(),
        _ => throw new DomainException($"Perfil de cliente sem estratégia de spread: {tier}.")
    };
}
