using Microsoft.EntityFrameworkCore;
using PriceAggregator.Persistence.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace PriceAggregator.Persistence.Domain;

[Table("candle")]
[PrimaryKey(nameof(StartUnixTime), nameof(EndUnixTime), nameof(SourceType))]
public class Candle
{
    public long StartUnixTime {  get; set; }

    public long EndUnixTime { get; set; }

    public SourceTypes SourceType { get; set; }

    public decimal PriceOpen { get; set; }

    public decimal PriceClose { get; set; }

    public decimal PriceHigh { get; set; }

    public decimal PriceLow { get; set; }

    public decimal Volume { get; set; }
}
