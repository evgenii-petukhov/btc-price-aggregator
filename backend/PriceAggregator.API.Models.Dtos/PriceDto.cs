namespace PriceAggregator.API.Models.Dtos;

public class PriceDto
{
    public DateTime Date { get; set; }

    public bool IsCached { get; set; }

    public decimal BitstampPrice { get; set; }

    public decimal BitfinexPrice { get; set; }
}
