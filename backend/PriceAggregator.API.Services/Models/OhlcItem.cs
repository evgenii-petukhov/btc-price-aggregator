using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace PriceAggregator.API.Services.Models;

public sealed class OhlcItem
{
    [JsonPropertyName("timestamp")]
    public string? Timestamp { get; set; }

    [JsonPropertyName("open")]
    public string? Open { get; set; }

    [JsonPropertyName("close")]
    public string? Close { get; set; }

    [JsonPropertyName("high")]
    public string? High { get; set; }

    [JsonPropertyName("low")]
    public string? Low { get; set; }

    [JsonPropertyName("volume")]
    public string? Volume { get; set; }
}