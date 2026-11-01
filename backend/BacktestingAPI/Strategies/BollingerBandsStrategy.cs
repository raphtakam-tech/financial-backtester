using BacktestingAPI.Models;

namespace BacktestingAPI.Strategies
{

    /// Bollinger Bands Strategy.
    /// 
    /// Bollinger Bands consist of a moving average with upper and lower bands based on standard deviation.
    /// This is a mean reversion strategy that assumes prices will revert to the mean.
    /// 
    /// Signals:
    /// - BUY: When price touches or crosses below lower band and not currently holding
    /// - SELL: When price touches or crosses above upper band and currently holding
    /// - HOLD: Otherwise
    /// 
    /// Formula:
    /// 1. Middle Band = SMA of close prices over period (default 20)
    /// 2. Standard Deviation = sqrt(sum((Close - SMA)²) / period)
    /// 3. Upper Band = Middle Band + (standardDeviations × StdDev) (default 2.0)
    /// 4. Lower Band = Middle Band - (standardDeviations × StdDev)

    public class BollingerBandsStrategy : IStrategy
    {
        public string Name => "BollingerBands";

        public Dictionary<string, object> Parameters { get; set; } = new(StringComparer.OrdinalIgnoreCase)
        {
            {"period", 20},
            {"standardDeviations", 2.0m}
        };

        private bool _isHolding = false;

        public string GetSignal(List<StockData> historicalData, int currentIndex)
        {
            int period = GetIntParam("period", 20);
            decimal stdDevMultiplier = GetDecimalParam("standardDeviations", 2.0m);

            // Need at least 'period' days of data
            if (currentIndex < period - 1)
                return "HOLD";

            // Calculate Bollinger Bands
            var (upperBand, middleBand, lowerBand) = CalculateBollingerBands(historicalData, period, stdDevMultiplier, currentIndex);
            if (!upperBand.HasValue || !lowerBand.HasValue)
                return "HOLD";

            decimal currentPrice = historicalData[currentIndex].Close;

            // Generate signals based on price touching bands
            if (!_isHolding)
            {
                // BUY: Price touches or crosses below lower band (oversold)
                if (currentPrice <= lowerBand.Value)
                {
                    _isHolding = true;
                    return "BUY";
                }
            }
            else
            {
                // SELL: Price touches or crosses above upper band (overbought)
                if (currentPrice >= upperBand.Value)
                {
                    _isHolding = false;
                    return "SELL";
                }
            }

            return "HOLD";
        }


        /// Calculate Bollinger Bands at a specific index.
        /// Returns (upperBand, middleBand, lowerBand) tuple.

        private (decimal? upper, decimal? middle, decimal? lower) CalculateBollingerBands(List<StockData> data, int period, decimal stdDevMultiplier, int endIndex)
        {
            if (endIndex < period - 1)
                return (null, null, null);

            // Calculate Middle Band (SMA)
            decimal sum = 0;
            for (int i = endIndex - period + 1; i <= endIndex; i++)
            {
                sum += data[i].Close;
            }
            decimal middleBand = sum / period;

            // Calculate Standard Deviation
            decimal varianceSum = 0;
            for (int i = endIndex - period + 1; i <= endIndex; i++)
            {
                decimal deviation = data[i].Close - middleBand;
                varianceSum += deviation * deviation;
            }
            decimal variance = varianceSum / period;
            decimal stdDev = (decimal)Math.Sqrt((double)variance);

            // Calculate Upper and Lower Bands
            decimal upperBand = middleBand + (stdDevMultiplier * stdDev);
            decimal lowerBand = middleBand - (stdDevMultiplier * stdDev);

            return (upperBand, middleBand, lowerBand);
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
