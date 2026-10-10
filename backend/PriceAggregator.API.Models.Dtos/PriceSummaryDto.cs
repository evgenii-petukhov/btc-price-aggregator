namespace PriceAggregator.API.Models.Dtos;

public class PriceSummaryDto
{
    public DateTime Date { get; set; }

    public bool IsCached { get; set; }

    public IEnumerable<PriceItemDto>? Prices {  get; set; }
}
