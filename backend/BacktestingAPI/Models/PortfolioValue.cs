using System;

namespace BacktestingAPI.Models
{

    /// Time series point of the portfolio total value for charting.

    public class PortfolioValue
    {
        public DateTime Date { get; set; }
        public decimal Value { get; set; }
    }
}
