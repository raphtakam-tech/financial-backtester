using BacktestingAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BacktestingAPI.Data
{

    /// Repository for querying and persisting stock data to the database.

    public class StockDataRepository
    {
        private readonly AppDbContext _db;

        public StockDataRepository(AppDbContext db)
        {
            _db = db;
        }


        /// Returns all known symbols in the cache.

        public async Task<List<string>> GetSymbolsAsync()
        {
            return await _db.StockData
                .Select(s => s.Symbol)
                .Distinct()
                .OrderBy(s => s)
                .ToListAsync();
        }


        /// Retrieve data for a symbol in the given date range (inclusive), ordered by date ascending.

        public async Task<List<StockData>> GetBySymbolAndDateRangeAsync(string symbol, DateTime start, DateTime end)
        {
            return await _db.StockData
                .Where(s => s.Symbol == symbol && s.Date >= start && s.Date <= end)
                .OrderBy(s => s.Date)
                .ToListAsync();
        }


        /// Bulk insert new stock data rows; skips duplicates by symbol+date.

        public async Task BulkInsertAsync(List<StockData> data)
        {
            if (data.Count == 0) return;

            // Avoid duplicates: load existing dates for symbol set, then add only new.
            var grouped = data.GroupBy(d => d.Symbol);
            foreach (var g in grouped)
            {
                var symbol = g.Key;
                var dates = g.Select(x => x.Date).ToHashSet();
                var existingDates = await _db.StockData
                    .Where(s => s.Symbol == symbol && dates.Contains(s.Date))
                    .Select(s => s.Date)
                    .ToListAsync();

                var existingSet = existingDates.ToHashSet();
                var newRows = g.Where(x => !existingSet.Contains(x.Date)).ToList();
                if (newRows.Count > 0)
                {
                    await _db.StockData.AddRangeAsync(newRows);
                }
            }

            await _db.SaveChangesAsync();
        }


        /// Checks whether the cache already contains data broadly covering the specified range.

        public async Task<bool> IsDataCachedAsync(string symbol, DateTime start, DateTime end)
        {
            var query = _db.StockData.Where(s => s.Symbol == symbol);
            var minDate = await query.Select(s => (DateTime?)s.Date).MinAsync();
            var maxDate = await query.Select(s => (DateTime?)s.Date).MaxAsync();
            if (minDate == null || maxDate == null) return false;
            return minDate.Value <= start && maxDate.Value >= end;
        }


        /// Returns the last updated timestamp of cached data for the symbol, or null if none.

        public async Task<DateTime?> GetLastUpdateDateAsync(string symbol)
        {
            return await _db.StockData
                .Where(s => s.Symbol == symbol)
                .Select(s => (DateTime?)s.LastUpdated)
                .MaxAsync();
        }
    }
}
