using System;
using System.Collections.Generic;

namespace BacktestingAPI.Models
{

    /// Input contract for running a backtest.

    public class BacktestRequest
    {
        public string Symbol { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal InitialCapital { get; set; } = 10000m;
        public string StrategyType { get; set; } = "SMA";

        // Note: model binder will often bind values as JsonElement; we'll handle conversion in StrategyService.
        public Dictionary<string, object>? Parameters { get; set; }
    }
}
