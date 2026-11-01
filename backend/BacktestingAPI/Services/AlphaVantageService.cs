using System.Globalization;
using System.Text.Json;
using BacktestingAPI.Data;
using BacktestingAPI.Models;

namespace BacktestingAPI.Services
{
    /// Service to fetch historical stock prices using Yahoo Finance (no API key required).
    /// Falls back to SQLite cache when data is already fresh.
    public class AlphaVantageService
    {
        private readonly HttpClient _httpClient;
        private readonly StockDataRepository _repository;
        private readonly ILogger<AlphaVantageService> _logger;
        private readonly TimeSpan _cacheStalenessThreshold;

        public AlphaVantageService(HttpClient httpClient, StockDataRepository repository, ILogger<AlphaVantageService> logger, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _repository = repository;
            _logger = logger;

            // Add a browser-like User-Agent so Yahoo Finance doesn't block the request
            if (!_httpClient.DefaultRequestHeaders.Contains("User-Agent"))
            {
                _httpClient.DefaultRequestHeaders.Add(
                    "User-Agent",
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36"
                );
            }

            var stalenessDays = 1;
            int.TryParse(configuration["AlphaVantage:CacheStalenessDays"], out stalenessDays);
            _cacheStalenessThreshold = TimeSpan.FromDays(stalenessDays > 0 ? stalenessDays : 1);
        }

        /// Fetches historical daily data for symbol between startDate and endDate inclusive.
        /// Checks SQLite cache first; calls Yahoo Finance API only when data is missing or stale.
        public async Task<List<StockData>> FetchHistoricalDataAsync(string symbol, DateTime startDate, DateTime endDate, CancellationToken ct = default)
        {
            symbol = symbol?.Trim().ToUpperInvariant() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(symbol)) throw new ArgumentException("Symbol is required.");

            var lastUpdate = await _repository.GetLastUpdateDateAsync(symbol);
            var isCached = await _repository.IsDataCachedAsync(symbol, startDate, endDate);
            var recentEnough = lastUpdate.HasValue && (DateTime.UtcNow - lastUpdate.Value) < _cacheStalenessThreshold;

            if (isCached && recentEnough)
            {
                _logger.LogInformation("Returning cached data for {Symbol} from {Start} to {End}", symbol, startDate, endDate);
                return await _repository.GetBySymbolAndDateRangeAsync(symbol, startDate, endDate);
            }

            // Fetch from Yahoo Finance
            // Add a buffer of 7 days before startDate to ensure enough data for indicators (SMA, RSI, etc.)
            var fetchFrom = startDate.AddDays(-7);
            var period1 = new DateTimeOffset(fetchFrom, TimeSpan.Zero).ToUnixTimeSeconds();
            var period2 = new DateTimeOffset(endDate.AddDays(1), TimeSpan.Zero).ToUnixTimeSeconds();

            var url = $"https://query1.finance.yahoo.com/v8/finance/chart/{Uri.EscapeDataString(symbol)}" +
                      $"?interval=1d&period1={period1}&period2={period2}&events=history";

            try
            {
                _logger.LogInformation("Calling Yahoo Finance API for {Symbol}", symbol);
                using var response = await _httpClient.GetAsync(url, ct);
                var content = await response.Content.ReadAsStringAsync(ct);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Yahoo Finance API error for {Symbol}: {Status}", symbol, response.StatusCode);
                    throw new Exception($"Yahoo Finance API error: {response.StatusCode}");
                }

                var list = ParseYahooResponse(content, symbol);

                if (list.Count == 0)
                {
                    _logger.LogError("Yahoo Finance returned no data for {Symbol}", symbol);
                    // Fallback to cache if available
                    var cached = await _repository.GetBySymbolAndDateRangeAsync(symbol, startDate, endDate);
                    if (cached.Count > 0) return cached;
                    throw new Exception($"No data found for symbol '{symbol}'. Please verify the ticker is correct (e.g. GOOGL, MSFT, TSLA).");
                }

                list = list.OrderBy(x => x.Date).ToList();
                await _repository.BulkInsertAsync(list);

                _logger.LogInformation("Fetched and cached {Count} rows for {Symbol}", list.Count, symbol);

                return list.Where(x => x.Date >= startDate && x.Date <= endDate).ToList();
            }
            catch (TaskCanceledException)
            {
                _logger.LogError("Yahoo Finance request timed out for {Symbol}", symbol);
                throw;
            }
            catch (Exception ex) when (ex.Message.StartsWith("No data found") || ex.Message.StartsWith("Yahoo Finance"))
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching data from Yahoo Finance for {Symbol}", symbol);
                var cached = await _repository.GetBySymbolAndDateRangeAsync(symbol, startDate, endDate);
                if (cached.Count > 0) return cached;
                throw;
            }
        }

        private List<StockData> ParseYahooResponse(string content, string symbol)
        {
            var list = new List<StockData>();

            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;

            // Yahoo Finance v8 structure: result[0].timestamp + indicators.quote[0]
            if (!root.TryGetProperty("chart", out var chart)) return list;
            if (!chart.TryGetProperty("result", out var results)) return list;
            if (results.ValueKind != JsonValueKind.Array || results.GetArrayLength() == 0) return list;

            var result = results[0];

            if (!result.TryGetProperty("timestamp", out var timestamps)) return list;
            if (!result.TryGetProperty("indicators", out var indicators)) return list;
            if (!indicators.TryGetProperty("quote", out var quotes)) return list;
            if (quotes.GetArrayLength() == 0) return list;

            var quote = quotes[0];

            var opens   = quote.TryGetProperty("open",   out var o) ? o : (JsonElement?)null;
            var highs   = quote.TryGetProperty("high",   out var h) ? h : (JsonElement?)null;
            var lows    = quote.TryGetProperty("low",    out var l) ? l : (JsonElement?)null;
            var closes  = quote.TryGetProperty("close",  out var c) ? c : (JsonElement?)null;
            var volumes = quote.TryGetProperty("volume", out var v) ? v : (JsonElement?)null;

            var tsArray = timestamps.EnumerateArray().ToList();

            for (int i = 0; i < tsArray.Count; i++)
            {
                try
                {
                    var unixTs = tsArray[i].GetInt64();
                    var date = DateTimeOffset.FromUnixTimeSeconds(unixTs).UtcDateTime.Date;

                    // Skip if any OHLCV value is null (market holiday padding)
                    if (opens?[i].ValueKind   == JsonValueKind.Null) continue;
                    if (highs?[i].ValueKind   == JsonValueKind.Null) continue;
                    if (lows?[i].ValueKind    == JsonValueKind.Null) continue;
                    if (closes?[i].ValueKind  == JsonValueKind.Null) continue;
                    if (volumes?[i].ValueKind == JsonValueKind.Null) continue;

                    var open   = opens.HasValue   ? (decimal)opens.Value[i].GetDouble()   : 0m;
                    var high   = highs.HasValue   ? (decimal)highs.Value[i].GetDouble()   : 0m;
                    var low    = lows.HasValue    ? (decimal)lows.Value[i].GetDouble()    : 0m;
                    var close  = closes.HasValue  ? (decimal)closes.Value[i].GetDouble()  : 0m;
                    var volume = volumes.HasValue ? volumes.Value[i].GetInt64() : 0L;

                    list.Add(new StockData
                    {
                        Symbol      = symbol,
                        Date        = date,
                        Open        = open,
                        High        = high,
                        Low         = low,
                        Close       = close,
                        Volume      = volume,
                        LastUpdated = DateTime.UtcNow
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to parse row {Index} for {Symbol}", i, symbol);
                }
            }

            return list;
        }
    }
}