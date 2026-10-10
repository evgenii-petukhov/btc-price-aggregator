namespace PriceAggregator.API.Services.Abstractions.Models;

public class CandleResponse
{
    public long UnitTimeStart { get; set; }

    public decimal PriceOpen { get; set; }

    public decimal PriceClose { get; set; }

    public decimal PriceHigh { get; set; }

    public decimal PriceLow { get; set; }

    public decimal Volume { get; set; }
}
