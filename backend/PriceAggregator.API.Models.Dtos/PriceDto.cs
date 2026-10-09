namespace PriceAggregator.API.Models.Dtos;

public class PriceDto
{
    public DateTime Date { get; set; }

    public bool IsCached { get; set; }

    public decimal AvgPrice { get; set; }

    public decimal Bitstamp { get; set; }

    public decimal Bitfinex { get; set; }
}
