using BacktestingAPI.Models;

namespace BacktestingAPI.Strategies
{

    /// Momentum Strategy.
    /// 
    /// A trend-following strategy that buys when price momentum is positive and strong,
    /// and sells when momentum weakens or turns negative.
    /// 
    /// Signals:
    /// - BUY: When momentum exceeds threshold (e.g., > 2%) and not currently holding
    /// - SELL: When momentum falls below threshold or turns negative and currently holding
    /// - HOLD: Otherwise
    /// 
    /// Formula:
    /// 1. Rate of Change (ROC) = (Close[today] - Close[today - lookbackPeriod]) / Close[today - lookbackPeriod]
    /// 2. Momentum % = ROC × 100

    public class MomentumStrategy : IStrategy
    {
        public string Name => "Momentum";

        public Dictionary<string, object> Parameters { get; set; } = new(StringComparer.OrdinalIgnoreCase)
        {
            {"lookbackPeriod", 10},
            {"momentumThreshold", 0.02m}  // 2% threshold
        };

        private bool _isHolding = false;

        public string GetSignal(List<StockData> historicalData, int currentIndex)
        {
            int lookbackPeriod = GetIntParam("lookbackPeriod", 10);
            decimal threshold = GetDecimalParam("momentumThreshold", 0.02m);

            // Need at least lookbackPeriod days of data
            if (currentIndex < lookbackPeriod)
                return "HOLD";

            // Calculate Rate of Change (momentum)
            decimal currentPrice = historicalData[currentIndex].Close;
            decimal pastPrice = historicalData[currentIndex - lookbackPeriod].Close;

            // Avoid division by zero
            if (pastPrice == 0)
                return "HOLD";

            // ROC = (current - past) / past
            decimal roc = (currentPrice - pastPrice) / pastPrice;

            // Generate signals based on momentum
            if (!_isHolding)
            {
                // BUY: Momentum is positive and exceeds threshold
                if (roc > threshold)
                {
                    _isHolding = true;
                    return "BUY";
                }
            }
            else
            {
                // SELL: Momentum turns negative or zero
                if (roc <= 0)
                {
                    _isHolding = false;
                    return "SELL";
                }
            }

            return "HOLD";
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

        private decimal GetDecimalParam(string key, decimal defaultValue)
        {
            if (Parameters.TryGetValue(key, out var val))
            {
                if (val is decimal dv) return dv;
                if (val is int iv) return iv;
                if (val is long lv) return lv;
                if (val is double dbv) return (decimal)dbv;
                if (val is string sv && decimal.TryParse(sv, out var parsed)) return parsed;
            }
            return defaultValue;
        }
    }
}
