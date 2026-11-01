using BacktestingAPI.Data;
using BacktestingAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace BacktestingAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StockDataController : ControllerBase
    {
        private readonly StockDataRepository _repo;
        private readonly AlphaVantageService _alphaService;
        private readonly ILogger<StockDataController> _logger;

        public StockDataController(StockDataRepository repo, AlphaVantageService alphaService, ILogger<StockDataController> logger)
        {
            _repo = repo;
            _alphaService = alphaService;
            _logger = logger;
        }


        /// Returns the list of symbols that exist in the cache.

        [HttpGet("symbols")]
        public async Task<IActionResult> GetSymbols()
        {
            try
            {
                var symbols = await _repo.GetSymbolsAsync();
                return Ok(symbols);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get symbols");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }


        /// Returns historical data for a symbol between start and end dates.

        [HttpGet("{symbol}")]
        public async Task<IActionResult> GetSymbolData([FromRoute] string symbol, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            if (string.IsNullOrWhiteSpace(symbol)) return BadRequest(new { error = "Symbol is required" });
            if (startDate == default || endDate == default || startDate > endDate)
                return BadRequest(new { error = "Invalid date range" });

            try
            {
                var data = await _alphaService.FetchHistoricalDataAsync(symbol, startDate, endDate);
                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching data for {Symbol}", symbol);
                return StatusCode(500, new { error = ex.Message });
            }
        }


        /// Returns whether data is cached for the symbol and the last update time.

        [HttpGet("cache-status/{symbol}")]
        public async Task<IActionResult> GetCacheStatus([FromRoute] string symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol)) return BadRequest(new { error = "Symbol is required" });
            try
            {
                var lastUpdated = await _repo.GetLastUpdateDateAsync(symbol.ToUpperInvariant());
                var isCached = lastUpdated.HasValue;
                return Ok(new { symbol = symbol.ToUpperInvariant(), isCached, lastUpdated });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking cache status for {Symbol}", symbol);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }
    }
}
