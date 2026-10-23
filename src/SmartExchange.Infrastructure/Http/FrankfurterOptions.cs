namespace SmartExchange.Infrastructure.Http;

public sealed class FrankfurterOptions
{
    public const string SectionName = "Frankfurter";

    public string BaseUrl { get; set; } = "https://api.frankfurter.app";
}
