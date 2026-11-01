using BacktestingAPI.Models;

namespace BacktestingAPI.Strategies
{

    /// Moving Average Convergence Divergence (MACD) Strategy.
    /// 
    /// MACD shows the relationship between two moving averages of a security's price.
    /// 
    /// Signals:
    /// - BUY: When MACD line crosses above signal line and not currently holding
    /// - SELL: When MACD line crosses below signal line and currently holding
    /// - HOLD: Otherwise
    /// 
    /// Formula:
    /// 1. Fast EMA = EMA of close prices over fast period (default 12)
    /// 2. Slow EMA = EMA of close prices over slow period (default 26)
    /// 3. MACD Line = Fast EMA - Slow EMA
    /// 4. Signal Line = EMA of MACD line over signal period (default 9)

    public class MacdStrategy : IStrategy
    {
        public string Name => "MACD";

        public Dictionary<string, object> Parameters { get; set; } = new(StringComparer.OrdinalIgnoreCase)
        {
            {"fastPeriod", 12},
            {"slowPeriod", 26},
            {"signalPeriod", 9}
        };

        private bool _isHolding = false;

        public string GetSignal(List<StockData> historicalData, int currentIndex)
        {
            int fastPeriod = GetIntParam("fastPeriod", 12);
            int slowPeriod = GetIntParam("slowPeriod", 26);
            int signalPeriod = GetIntParam("signalPeriod", 9);

            // Need at least slowPeriod + signalPeriod days before generating signals
            int minDataPoints = slowPeriod + signalPeriod;
            if (currentIndex < minDataPoints)
                return "HOLD";

            // Calculate MACD and Signal for current day
            var (currentMacd, currentSignal) = CalculateMACDAndSignal(historicalData, fastPeriod, slowPeriod, signalPeriod, currentIndex);
            if (!currentMacd.HasValue || !currentSignal.HasValue)
                return "HOLD";

            // Calculate MACD and Signal for previous day to detect crossovers
            var (prevMacd, prevSignal) = CalculateMACDAndSignal(historicalData, fastPeriod, slowPeriod, signalPeriod, currentIndex - 1);
            if (!prevMacd.HasValue || !prevSignal.HasValue)
                return "HOLD";

            // Detect crossovers
            if (!_isHolding)
            {
                // BUY: MACD crosses above Signal (bullish crossover)
                if (prevMacd.Value <= prevSignal.Value && currentMacd.Value > currentSignal.Value)
                {
                    _isHolding = true;
                    return "BUY";
                }
            }
            else
            {
                // SELL: MACD crosses below Signal (bearish crossover)
                if (prevMacd.Value >= prevSignal.Value && currentMacd.Value < currentSignal.Value)
                {
                    _isHolding = false;
                    return "SELL";
                }
            }

            return "HOLD";
        }


        /// Calculate MACD line and Signal line at a specific index.
        /// Returns (MACD, Signal) tuple.

        private (decimal? macd, decimal? signal) CalculateMACDAndSignal(List<StockData> data, int fastPeriod, int slowPeriod, int signalPeriod, int endIndex)
        {
            // Need enough data for slow EMA
            if (endIndex < slowPeriod - 1)
                return (null, null);

            // Calculate Fast EMA
            decimal? fastEma = CalculateEMA(data, fastPeriod, endIndex);
            if (!fastEma.HasValue)
                return (null, null);

            // Calculate Slow EMA
            decimal? slowEma = CalculateEMA(data, slowPeriod, endIndex);
            if (!slowEma.HasValue)
                return (null, null);

            // MACD Line = Fast EMA - Slow EMA
            decimal macdValue = fastEma.Value - slowEma.Value;

            // Need enough MACD values to calculate signal line
            if (endIndex < slowPeriod + signalPeriod - 2)
                return (macdValue, null);

            // Calculate Signal Line (EMA of MACD values)
            // Build list of MACD values for signal calculation
            List<decimal> macdValues = new List<decimal>();
            for (int i = slowPeriod - 1; i <= endIndex; i++)
            {
                var fastEmaI = CalculateEMA(data, fastPeriod, i);
                var slowEmaI = CalculateEMA(data, slowPeriod, i);
                if (fastEmaI.HasValue && slowEmaI.HasValue)
                {
                    macdValues.Add(fastEmaI.Value - slowEmaI.Value);
                }
            }

            decimal? signalValue = CalculateEMAFromValues(macdValues, signalPeriod);

            return (macdValue, signalValue);
        }


        /// Calculate Exponential Moving Average of close prices.

        private decimal? CalculateEMA(List<StockData> data, int period, int endIndex)
        {
            if (endIndex < period - 1)
                return null;

            // First EMA is simple average
            decimal sum = 0;
            for (int i = endIndex - period + 1; i <= endIndex - period + 1 + period - 1; i++)
            {
                if (i > endIndex) break;
                sum += data[i].Close;
            }
            decimal ema = sum / period;

            // Calculate multiplier
            decimal multiplier = 2m / (period + 1);

            // Apply EMA formula for remaining days
            for (int i = endIndex - period + 1 + period; i <= endIndex; i++)
            {
                ema = (data[i].Close * multiplier) + (ema * (1 - multiplier));
            }

            return ema;
        }


        /// Calculate EMA from a list of values (used for signal line).

        private decimal? CalculateEMAFromValues(List<decimal> values, int period)
        {
            if (values.Count < period)
                return null;

            // First EMA is simple average
            decimal sum = 0;
            for (int i = 0; i < period; i++)
            {
                sum += values[i];
            }
            decimal ema = sum / period;

            // Calculate multiplier
            decimal multiplier = 2m / (period + 1);

            // Apply EMA formula for remaining values
            for (int i = period; i < values.Count; i++)
            {
                ema = (values[i] * multiplier) + (ema * (1 - multiplier));
            }

            return ema;
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
