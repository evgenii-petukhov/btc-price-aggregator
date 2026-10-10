using PriceAggregator.API.Models.Dtos;
using PriceAggregator.API.Services.Abstractions;
using PriceAggregator.API.Services.Abstractions.Models;
using PriceAggregator.Persistence.Enums;
using PriceAggregator.Persistence.Repository.Abstraction;

namespace PriceAggregator.API.Services;

public class RequestCacheService(
    IRequestServiceResolver requestServiceResolver,
    ICandleRepository repository,
    IUnitOfWork unitOfWork) : IRequestCacheService
{
    private readonly IRequestServiceResolver _requestServiceResolver = requestServiceResolver;
    private readonly ICandleRepository _repository = repository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<IReadOnlyList<CandleDto>> GetPricesAsync(
        long unixTimeStart,
        long unixTimeEnd,
        IReadOnlyCollection<SourceType> sourceTypes,
        CancellationToken cancellationToken = default)
    {
        var results = new Dictionary<SourceType, CandleDto>();
        var missing = new List<SourceType>();

        foreach (var sourceType in sourceTypes)
        {
            var candleEntity = await _repository.GetAsync(
                unixTimeStart,
                unixTimeEnd,
                sourceType,
                cancellationToken);

            if (candleEntity == null)
            {
                missing.Add(sourceType);
                continue;
            }

            results[sourceType] = new CandleDto
            {
                UnitTimeStart = unixTimeStart,
                PriceOpen = candleEntity.PriceOpen,
                PriceClose = candleEntity.PriceClose,
                PriceHigh = candleEntity.PriceHigh,
                PriceLow = candleEntity.PriceLow,
                Volume = candleEntity.Volume
            };
        }

        if (missing.Count > 0)
        {
            var responses = await Task.WhenAll(
                missing.Select(t => ResolveRequestService(t).GetPriceAsync(unixTimeStart, unixTimeEnd, cancellationToken)));

            for (var i = 0; i < missing.Count; i++)
            {
                var sourceType = missing[i];
                var candleResponse = responses[i];

                await _repository.AddAsync(
                    unixTimeStart,
                    unixTimeEnd,
                    sourceType,
                    candleResponse.PriceOpen,
                    candleResponse.PriceClose,
                    candleResponse.PriceHigh,
                    candleResponse.PriceLow,
                    candleResponse.Volume);

                results[sourceType] = Map(unixTimeStart, candleResponse);
            }

            await _unitOfWork.SaveAsync(cancellationToken);
        }

        return [.. sourceTypes.Select(sourceType => results[sourceType])];
    }

    private IRequestService ResolveRequestService(SourceType sourceType)
    {
        return _requestServiceResolver.Resolve(sourceType)
            ?? throw new InvalidOperationException($"No request service registered for source '{sourceType}'.");
    }

    private static CandleDto Map(long unixTimeStart, CandleResponse candleResponse)
    {
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
