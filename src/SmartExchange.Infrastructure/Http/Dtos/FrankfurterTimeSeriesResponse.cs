using System.Text.Json.Serialization;

namespace SmartExchange.Infrastructure.Http.Dtos;

public sealed record FrankfurterTimeSeriesResponse
{
    [JsonPropertyName("start_date")]
    public required DateOnly StartDate { get; init; }

    [JsonPropertyName("end_date")]
    public required DateOnly EndDate { get; init; }

    [JsonPropertyName("rates")]
    public required Dictionary<string, Dictionary<string, decimal>> Rates { get; init; }
}
