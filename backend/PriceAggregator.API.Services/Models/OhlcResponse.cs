using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace PriceAggregator.API.Services.Models;

public sealed class OhlcResponse
{
    [JsonPropertyName("data")]
    public OhlcData? Data { get; set; }
}
