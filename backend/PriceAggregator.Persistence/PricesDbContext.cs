using Microsoft.EntityFrameworkCore;
using PriceAggregator.Persistence.Domain;

namespace PriceAggregator.Persistence;

public class PricesDbContext(DbContextOptions<PricesDbContext> options) : DbContext(options)
{
    public DbSet<Candle> Candles { get; set; }
}
