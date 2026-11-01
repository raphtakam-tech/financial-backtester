using BacktestingAPI.Models;

namespace BacktestingAPI.Strategies
{

    /// Contract for pluggable trading strategies.

    public interface IStrategy
    {

        /// Returns a discrete signal for the current index using available historical data: BUY, SELL, or HOLD.

        string GetSignal(List<StockData> historicalData, int currentIndex);


        /// Descriptive name of the strategy.

        string Name { get; }


        /// Strategy parameters (e.g., periods), case-insensitive keys.

        Dictionary<string, object> Parameters { get; set; }
    }
}
