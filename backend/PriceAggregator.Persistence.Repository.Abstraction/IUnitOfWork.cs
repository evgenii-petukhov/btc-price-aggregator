namespace PriceAggregator.Persistence.Repository.Abstraction;

public interface IUnitOfWork
{
    Task SaveAsync(CancellationToken cancellationToken = default);
}
