using PriceAggregator.API.Models.Dtos;
using PriceAggregator.Persistence.Enums;

namespace PriceAggregator.API.Services.Abstractions;

public interface IPriceSummaryService
{
    public Task<PriceSummaryDto> GetSummaryAsync(
        DateTime date,
        InstrumentType instrumentType,
        CancellationToken cancellationToken);
}
