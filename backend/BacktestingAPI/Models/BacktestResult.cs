using System.Collections.Generic;

namespace BacktestingAPI.Models
{

    /// The complete output of a backtest run including metrics and trade history.

    public class BacktestResult
    {
        public string Symbol { get; set; } = string.Empty;
        public string Strategy { get; set; } = string.Empty;
        public decimal InitialCapital { get; set; }
        public decimal FinalCapital { get; set; }
        public PerformanceMetrics Metrics { get; set; } = new PerformanceMetrics();
        public List<Trade> Trades { get; set; } = new List<Trade>();
        public List<PortfolioValue> PortfolioValues { get; set; } = new List<PortfolioValue>();
    }
}
