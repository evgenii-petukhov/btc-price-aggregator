using PriceAggregator.Persistence.Repository.Abstraction;

namespace PriceAggregator.Persistence.Repository;

public class UnitOfWork(PricesDbContext context) : IUnitOfWork, IDisposable
{
    private readonly PricesDbContext _context = context;
    private bool _disposedValue;

    public async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            _context.ChangeTracker.Clear();
            throw;
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _context.Dispose();
            }

            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
