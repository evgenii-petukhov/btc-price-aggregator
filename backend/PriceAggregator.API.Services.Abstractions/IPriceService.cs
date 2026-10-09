using PriceAggregator.API.Models.Dtos;

namespace PriceAggregator.API.Services.Abstractions;

public interface IPriceService
{
    public Task<PriceDto> GetPriceAsync(DateTime date, CancellationToken cancellationToken);
}
