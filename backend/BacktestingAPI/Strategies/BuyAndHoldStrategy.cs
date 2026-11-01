using BacktestingAPI.Models;

namespace BacktestingAPI.Strategies
{

    /// Buy on first day, sell on last day, hold otherwise.

    public class BuyAndHoldStrategy : IStrategy
    {
        public string Name => "BuyAndHold";

        public Dictionary<string, object> Parameters { get; set; } = new();

        public string GetSignal(List<StockData> historicalData, int currentIndex)
        {
            if (historicalData.Count == 0) return "HOLD";
            if (currentIndex == 0) return "BUY";
            if (currentIndex == historicalData.Count - 1) return "SELL";
            return "HOLD";
        }
    }
}
