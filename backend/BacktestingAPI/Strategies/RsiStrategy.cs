using BacktestingAPI.Models;

namespace BacktestingAPI.Strategies
{

    /// Relative Strength Index (RSI) Strategy.
    /// 
    /// RSI measures the magnitude of recent price changes to evaluate overbought or oversold conditions.
    /// 
    /// Signals:
    /// - BUY: When RSI crosses below oversold threshold (default 30) and not currently holding
    /// - SELL: When RSI crosses above overbought threshold (default 70) and currently holding
    /// - HOLD: Otherwise
    /// 
    /// Formula:
    /// 1. Calculate gains and losses between consecutive days
    /// 2. Calculate average gain and average loss over the period (smoothed)
    /// 3. RS = Average Gain / Average Loss
    /// 4. RSI = 100 - (100 / (1 + RS))

    public class RsiStrategy : IStrategy
    {
        public string Name => "RSI";

        public Dictionary<string, object> Parameters { get; set; } = new(StringComparer.OrdinalIgnoreCase)
        {
            {"rsiPeriod", 14},
            {"overboughtThreshold", 70m},
            {"oversoldThreshold", 30m}
        };

        private bool _isHolding = false;

        public string GetSignal(List<StockData> historicalData, int currentIndex)
        {
            int period = GetIntParam("rsiPeriod", 14);
            decimal overbought = GetDecimalParam("overboughtThreshold", 70m);
            decimal oversold = GetDecimalParam("oversoldThreshold", 30m);

            // Need at least period + 1 days to calculate RSI (period for first average + 1 for comparison)
            if (currentIndex < period)
                return "HOLD";

            // Calculate current RSI
            decimal? currentRsi = CalculateRSI(historicalData, period, currentIndex);
            if (!currentRsi.HasValue)
                return "HOLD";

            // Calculate previous RSI to detect crossovers
            decimal? previousRsi = currentIndex > period ? CalculateRSI(historicalData, period, currentIndex - 1) : null;

            // Detect crossovers
            if (!_isHolding && previousRsi.HasValue)
            {
                // BUY: RSI crosses below oversold threshold (was above, now below)
                if (previousRsi.Value >= oversold && currentRsi.Value < oversold)
                {
                    _isHolding = true;
                    return "BUY";
                }
            }
            else if (_isHolding && previousRsi.HasValue)
            {
                // SELL: RSI crosses above overbought threshold (was below, now above)
                if (previousRsi.Value <= overbought && currentRsi.Value > overbought)
                {
                    _isHolding = false;
                    return "SELL";
                }
            }

            return "HOLD";
        }


        /// Calculate RSI at a specific index using smoothed averages.

        private decimal? CalculateRSI(List<StockData> data, int period, int endIndex)
        {
            if (endIndex < period)
                return null;

            // Calculate initial average gain and loss
            decimal totalGain = 0;
            decimal totalLoss = 0;

            // First, calculate gains and losses for the initial period
            for (int i = endIndex - period + 1; i <= endIndex; i++)
            {
                decimal change = data[i].Close - data[i - 1].Close;
                if (change > 0)
                    totalGain += change;
                else
                    totalLoss += Math.Abs(change);
            }

            decimal avgGain = totalGain / period;
            decimal avgLoss = totalLoss / period;

            // Apply smoothing for subsequent periods (if we're beyond the initial period)
            // Note: For the first RSI calculation, we use simple averages
            // For a more accurate implementation over time, you could store and smooth these values

            // Calculate RS (Relative Strength)
            if (avgLoss == 0)
            {
                // If no losses, RSI = 100
                return 100m;
            }

            decimal rs = avgGain / avgLoss;

            // Calculate RSI
            decimal rsi = 100m - (100m / (1m + rs));

            return Math.Round(rsi, 2);
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
