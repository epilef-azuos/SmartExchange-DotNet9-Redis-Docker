using SmartExchange.Domain.Enums;

namespace SmartExchange.Domain.Strategies;

public interface ISpreadStrategyResolver
{
    ISpreadStrategy Resolve(ClientTier tier);
}
