using System.Text.Json.Serialization;

namespace SmartExchange.Infrastructure.Http.Dtos;

public sealed record FrankfurterLatestResponse
{
    [JsonPropertyName("date")]
    public required DateOnly Date { get; init; }

    [JsonPropertyName("rates")]
    public required Dictionary<string, decimal> Rates { get; init; }
}
