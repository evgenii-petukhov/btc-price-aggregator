using PriceAggregator.API.Models.Dtos;
using PriceAggregator.API.Services.Abstractions;

namespace PriceAggregator.API.Services;

public class PriceService : IPriceService
{
    public async Task<PriceDto> GetPriceAsync(DateTime date, CancellationToken cancellationToken)
    {
        return new PriceDto();
    }
}
