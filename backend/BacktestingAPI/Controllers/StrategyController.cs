using BacktestingAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace BacktestingAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StrategyController : ControllerBase
    {
        private readonly StrategyService _strategyService;

        public StrategyController(StrategyService strategyService)
        {
            _strategyService = strategyService;
        }


        /// Returns available strategies and descriptions.

        [HttpGet("list")]
        public IActionResult GetStrategies()
        {
            var list = _strategyService.GetAvailableStrategies()
                .Select(x => new { name = x.Name, description = x.Description })
                .ToList();
            return Ok(list);
        }


        /// Returns required parameters and defaults for a given strategy.

        [HttpGet("{strategyType}/parameters")]
        public IActionResult GetParameters([FromRoute] string strategyType)
        {
            try
            {
                var p = _strategyService.GetStrategyParameters(strategyType);
                return Ok(p);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }
    }
}
