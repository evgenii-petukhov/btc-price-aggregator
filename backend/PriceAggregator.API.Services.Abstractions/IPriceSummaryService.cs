using PriceAggregator.API.Models.Dtos;

namespace PriceAggregator.API.Services.Abstractions;

public interface IPriceSummaryService
{
    public Task<PriceSummaryDto> GetSummaryAsync(
        DateTime date,
        CancellationToken cancellationToken);
}
