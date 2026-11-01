using System.Text.Json;
using BacktestingAPI.Models;
using BacktestingAPI.Strategies;

namespace BacktestingAPI.Services
{

    /// Service to enumerate, describe, and instantiate trading strategies.

    public class StrategyService
    {

        /// Returns the list of available strategies with a short description.

        public List<(string Name, string Description)> GetAvailableStrategies()
        {
            return new List<(string, string)>
            {
                ("SMA", "Simple Moving Average crossover strategy (short vs long)"),
                ("BuyAndHold", "Buy on first day, sell on last day benchmark"),
                ("RSI", "Relative Strength Index - overbought/oversold oscillator"),
                ("MACD", "Moving Average Convergence Divergence - trend following"),
                ("BollingerBands", "Bollinger Bands - mean reversion strategy"),
                ("Momentum", "Price Momentum - rate of change trend following")
            };
        }


        /// Returns the accepted parameters and defaults for a strategy type.

        public Dictionary<string, object> GetStrategyParameters(string strategyType)
        {
            switch ((strategyType ?? string.Empty).Trim().ToUpperInvariant())
            {
                case "SMA":
                    return new Dictionary<string, object>
                    {
                        {"shortPeriod", 20},
                        {"longPeriod", 50}
                    };
                case "BUYANDHOLD":
                    return new Dictionary<string, object>();
                case "RSI":
                    return new Dictionary<string, object>
                    {
                        {"rsiPeriod", 14},
                        {"overboughtThreshold", 70},
                        {"oversoldThreshold", 30}
                    };
                case "MACD":
                    return new Dictionary<string, object>
                    {
                        {"fastPeriod", 12},
                        {"slowPeriod", 26},
                        {"signalPeriod", 9}
                    };
                case "BOLLINGERBANDS":
                    return new Dictionary<string, object>
                    {
                        {"period", 20},
                        {"standardDeviations", 2.0m}
                    };
                case "MOMENTUM":
                    return new Dictionary<string, object>
                    {
                        {"lookbackPeriod", 10},
                        {"momentumThreshold", 0.02m}
                    };
                default:
                    throw new ArgumentException($"Unknown strategy type: {strategyType}");
            }
        }


        /// Creates a concrete strategy instance for the requested type.

        public IStrategy CreateStrategy(string type, Dictionary<string, object>? parameters)
        {
            var t = (type ?? string.Empty).Trim().ToUpperInvariant();
            switch (t)
            {
                case "SMA":
                    var smaDefaults = GetStrategyParameters("SMA");
                    var smaParams = MergeParameters(smaDefaults, parameters);
                    return new SmaStrategy
                    {
                        Parameters = smaParams
                    };
                case "BUYANDHOLD":
                    return new BuyAndHoldStrategy();
                case "RSI":
                    var rsiDefaults = GetStrategyParameters("RSI");
                    var rsiParams = MergeParameters(rsiDefaults, parameters);
                    return new RsiStrategy
                    {
                        Parameters = rsiParams
                    };
                case "MACD":
                    var macdDefaults = GetStrategyParameters("MACD");
                    var macdParams = MergeParameters(macdDefaults, parameters);
                    return new MacdStrategy
                    {
                        Parameters = macdParams
                    };
                case "BOLLINGERBANDS":
                    var bbDefaults = GetStrategyParameters("BOLLINGERBANDS");
                    var bbParams = MergeParameters(bbDefaults, parameters);
                    return new BollingerBandsStrategy
                    {
                        Parameters = bbParams
                    };
                case "MOMENTUM":
                    var momDefaults = GetStrategyParameters("MOMENTUM");
                    var momParams = MergeParameters(momDefaults, parameters);
                    return new MomentumStrategy
                    {
                        Parameters = momParams
                    };
                default:
                    throw new ArgumentException($"Unknown strategy type: {type}");
            }
        }

        private static Dictionary<string, object> MergeParameters(Dictionary<string, object> defaults, Dictionary<string, object>? overrides)
        {
            var result = new Dictionary<string, object>(defaults, StringComparer.OrdinalIgnoreCase);
            if (overrides == null) return result;

            foreach (var kvp in overrides)
            {
                if (kvp.Value is JsonElement je)
                {
                    // Convert JsonElement to CLR types (int/decimal/string)
                    if (je.ValueKind == JsonValueKind.Number)
                    {
                        if (je.TryGetInt32(out var iv)) result[kvp.Key] = iv;
                        else if (je.TryGetDecimal(out var dv)) result[kvp.Key] = dv;
                        else result[kvp.Key] = je.ToString();
                    }
                    else if (je.ValueKind == JsonValueKind.String)
                    {
                        result[kvp.Key] = je.GetString() ?? string.Empty;
                    }
                    else if (je.ValueKind == JsonValueKind.True || je.ValueKind == JsonValueKind.False)
                    {
                        result[kvp.Key] = je.GetBoolean();
                    }
                    else
                    {
                        result[kvp.Key] = je.ToString();
                    }
                }
                else
                {
                    result[kvp.Key] = kvp.Value;
                }
            }

            return result;
        }
    }
}
