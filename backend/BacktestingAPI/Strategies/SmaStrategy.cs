using BacktestingAPI.Models;

namespace BacktestingAPI.Strategies
{

    /// Simple Moving Average crossover strategy.
    /// BUY when short SMA crosses above long SMA; SELL when it crosses below.

    public class SmaStrategy : IStrategy
    {
        public string Name => "SMA";

        public Dictionary<string, object> Parameters { get; set; } = new(StringComparer.OrdinalIgnoreCase)
        {
            {"shortPeriod", 20},
            {"longPeriod", 50}
        };

        public string GetSignal(List<StockData> historicalData, int currentIndex)
        {
            int shortP = GetIntParam("shortPeriod", 20);
            int longP = GetIntParam("longPeriod", 50);

            if (shortP <= 0 || longP <= 0 || shortP >= longP) return "HOLD";
            if (currentIndex < longP - 1) return "HOLD"; // not enough data

            decimal? shortSma = SMA(historicalData, currentIndex, shortP);
            decimal? longSma = SMA(historicalData, currentIndex, longP);

            // Need previous day SMAs to detect cross
            decimal? prevShort = SMA(historicalData, currentIndex - 1, shortP);
            decimal? prevLong = SMA(historicalData, currentIndex - 1, longP);

            if (!shortSma.HasValue || !longSma.HasValue || !prevShort.HasValue || !prevLong.HasValue)
                return "HOLD";

            bool crossUp = prevShort.Value <= prevLong.Value && shortSma.Value > longSma.Value;
            bool crossDown = prevShort.Value >= prevLong.Value && shortSma.Value < longSma.Value;

            if (crossUp) return "BUY";
            if (crossDown) return "SELL";
            return "HOLD";
        }

        private static decimal? SMA(List<StockData> data, int endIndex, int period)
        {
            if (endIndex - period + 1 < 0) return null;
            decimal sum = 0;
            for (int i = endIndex - period + 1; i <= endIndex; i++)
            {
                sum += data[i].Close;
            }
            return sum / period;
        }

        private int GetIntParam(string key, int defaultValue)
        {
            if (Parameters.TryGetValue(key, out var val))
            {
                if (val is int iv) return iv;
                if (val is long lv) return (int)lv;
                if (val is string sv && int.TryParse(sv, out var parsed)) return parsed;
                if (val is decimal dv) return (int)dv;
            }
            return defaultValue;
        }
    }
}
