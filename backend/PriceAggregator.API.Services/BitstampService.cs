using PriceAggregator.API.Models.Dtos;
using PriceAggregator.API.Services.Abstractions;
using PriceAggregator.Persistence.Repository.Abstraction;

namespace PriceAggregator.API.Services;

public class BitstampService(
    IBitstampRequestService requestService,
    ICandleRepository repository,
    IUnitOfWork unitOfWork) : IBitstampService
{
    private readonly IBitstampRequestService _requestService = requestService;
    private readonly ICandleRepository _repository = repository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<CandleDto?> GetPriceAsync(
        long unixTimeStart,
        long unixTimeEnd,
        CancellationToken cancellationToken = default)
    {
        var candleEntity = await _repository.GetAsync(
            unixTimeStart,
            unixTimeEnd,
            Persistence.Enums.SourceTypes.Bitstamp,
            cancellationToken);

        if (candleEntity != null)
        {
            return new CandleDto
            {
                UnitTimeStart = unixTimeStart,
                PriceOpen = candleEntity.PriceOpen,
                PriceClose = candleEntity.PriceClose,
                PriceHigh = candleEntity.PriceHigh,
                PriceLow = candleEntity.PriceLow,
                Volume = candleEntity.Volume
            };
        }

        var candleResponse = await _requestService.GetPriceAsync(unixTimeStart, cancellationToken);

        if (candleResponse == null)
        {
            return null;
        }

        await _repository.AddAsync(
            unixTimeStart,
            unixTimeEnd,
            Persistence.Enums.SourceTypes.Bitstamp,
            candleResponse.PriceOpen,
            candleResponse.PriceClose,
            candleResponse.PriceHigh,
            candleResponse.PriceLow,
            candleResponse.Volume);

        await _unitOfWork.SaveAsync(cancellationToken);

        return new CandleDto
        {
            UnitTimeStart = unixTimeStart,
            PriceOpen = candleResponse.PriceOpen,
            PriceClose = candleResponse.PriceClose,
            PriceHigh = candleResponse.PriceHigh,
            PriceLow = candleResponse.PriceLow,
            Volume = candleResponse.Volume
        };
    }
}
