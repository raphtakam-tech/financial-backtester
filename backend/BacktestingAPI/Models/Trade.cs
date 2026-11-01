using System;

namespace BacktestingAPI.Models
{

    /// Represents an executed trade in a backtest run.

    public class Trade
    {
        public DateTime Date { get; set; }
        public string Type { get; set; } = string.Empty; // BUY or SELL
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Total { get; set; }
    }
}
