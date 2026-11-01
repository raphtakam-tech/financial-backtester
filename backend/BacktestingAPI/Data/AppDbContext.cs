using BacktestingAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BacktestingAPI.Data
{

    /// Application EF Core database context configured for SQLite.

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<StockData> StockData => Set<StockData>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<StockData>()
                .HasIndex(s => new { s.Symbol, s.Date })
                .IsUnique();
        }
    }
}
