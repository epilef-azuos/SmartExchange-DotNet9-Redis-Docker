namespace SmartExchange.Domain.Common;

public static class FinancialRounding
{
    public const int RateDecimals = 6;

    public const int AmountDecimals = 2;

    public static decimal RoundRate(decimal value) =>
        Math.Round(value, RateDecimals, MidpointRounding.ToEven);

    public static decimal RoundAmount(decimal value) =>
        Math.Round(value, AmountDecimals, MidpointRounding.ToEven);
}
