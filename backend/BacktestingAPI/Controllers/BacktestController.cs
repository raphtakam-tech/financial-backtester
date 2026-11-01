using BacktestingAPI.Models;
using BacktestingAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace BacktestingAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BacktestController : ControllerBase
    {
        private readonly BacktestingEngine _engine;
        private readonly StrategyService _strategyService;
        private readonly ILogger<BacktestController> _logger;

        public BacktestController(BacktestingEngine engine, StrategyService strategyService, ILogger<BacktestController> logger)
        {
            _engine = engine;
            _strategyService = strategyService;
            _logger = logger;
        }


        /// Run a backtest for the requested symbol, dates, and strategy configuration.

        [HttpPost("run")]
        public async Task<IActionResult> Run([FromBody] BacktestRequest request)
        {
            if (request == null) return BadRequest(new { error = "Request body is required" });
            if (string.IsNullOrWhiteSpace(request.Symbol)) return BadRequest(new { error = "Symbol is required" });
            if (request.StartDate == default || request.EndDate == default || request.StartDate > request.EndDate)
                return BadRequest(new { error = "Invalid date range" });

            try
            {
                // Validate strategy exists
                _strategyService.GetStrategyParameters(request.StrategyType);

                var result = await _engine.RunBacktestAsync(request);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Backtest run failed");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
