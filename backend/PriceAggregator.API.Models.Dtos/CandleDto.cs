using PriceAggregator.Persistence.Enums;

namespace PriceAggregator.API.Models.Dtos;

public class CandleDto
{
    public SourceType SourceType { get; set; }

    public long UnitTimeStart { get; set; }

    public decimal PriceOpen { get; set; }

    public decimal PriceClose { get; set; }

    public decimal PriceHigh { get; set; }

    public decimal PriceLow { get; set; }

    public decimal Volume { get; set; }

    public bool IsCached { get; set; }
}
