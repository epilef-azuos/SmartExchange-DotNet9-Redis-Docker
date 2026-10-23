namespace SmartExchange.Infrastructure.Caching;

public static class CacheKeys
{
    public static string Latest(string baseCode) => $"exchange:latest:{baseCode}";

    public static string Pair(string baseCode, string targetCode) => $"exchange:pair:{baseCode}:{targetCode}";

    public static string Currencies() => "exchange:currencies";

    public static string History(string baseCode, string targetCode, DateOnly startDate, DateOnly endDate) =>
        $"exchange:history:{baseCode}:{targetCode}:{startDate:yyyy-MM-dd}:{endDate:yyyy-MM-dd}";
}
