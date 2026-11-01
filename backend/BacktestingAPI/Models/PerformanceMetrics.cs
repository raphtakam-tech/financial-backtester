using System;

namespace BacktestingAPI.Models
{

    /// Aggregated performance statistics for a backtest.
    /// Percentages are expressed as percent units (e.g., 25 = 25%).

    public class PerformanceMetrics
    {
        public decimal TotalReturn { get; set; }
        public decimal CumulativeReturn { get; set; }
        public decimal AnnualizedReturn { get; set; }
        public decimal Volatility { get; set; }
        public decimal SharpeRatio { get; set; }
        public decimal MaxDrawdown { get; set; }
        public int NumberOfTrades { get; set; }
        public decimal WinRate { get; set; }
    }
}
