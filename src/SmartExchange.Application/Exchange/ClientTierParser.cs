using SmartExchange.Application.Common.Exceptions;
using SmartExchange.Domain.Enums;

namespace SmartExchange.Application.Exchange;

internal static class ClientTierParser
{
    public static ClientTier Parse(string? tier)
    {
        if (string.IsNullOrWhiteSpace(tier))
        {
            return ClientTier.Standard;
        }

        if (Enum.TryParse<ClientTier>(tier, ignoreCase: true, out var parsed) && Enum.IsDefined(parsed))
        {
            return parsed;
        }

        throw new ValidationException($"Perfil de cliente inválido: '{tier}'. Use Standard, Premium ou Vip.");
    }
}
