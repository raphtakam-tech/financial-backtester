using BacktestingAPI.Models;

namespace BacktestingAPI.Services
{

    /// Core backtesting engine that executes a strategy over historical data.

    public class BacktestingEngine
    {
        private readonly AlphaVantageService _dataService;
        private readonly StrategyService _strategyService;
        private readonly ILogger<BacktestingEngine> _logger;

        public BacktestingEngine(AlphaVantageService dataService, StrategyService strategyService, ILogger<BacktestingEngine> logger)
        {
            _dataService = dataService;
            _strategyService = strategyService;
            _logger = logger;
        }


        /// Executes a backtest for the given request and returns the full result set.

        public async Task<BacktestResult> RunBacktestAsync(BacktestRequest request, CancellationToken ct = default)
        {
            // 1. Fetch historical data
            var data = await _dataService.FetchHistoricalDataAsync(request.Symbol, request.StartDate, request.EndDate, ct);
            if (data.Count == 0)
            {
                throw new InvalidOperationException("No historical data available for the specified range.");
            }

            // 2. Initialize portfolio state
            decimal cash = request.InitialCapital;
            int shares = 0;
            var trades = new List<Trade>();
            var pvSeries = new List<PortfolioValue>();

            // 3. Create strategy
            var strategy = _strategyService.CreateStrategy(request.StrategyType, request.Parameters);

            // 4. Loop each day
            for (int i = 0; i < data.Count; i++)
            {
                var today = data[i];
                var price = today.Close;

                var signal = strategy.GetSignal(data, i);

                if (signal.Equals("BUY", StringComparison.OrdinalIgnoreCase))
                {
                    if (cash >= price)
                    {
                        var quantity = (int)(cash / price);
                        if (quantity > 0)
                        {
                            var cost = quantity * price;
                            cash -= cost;
                            shares += quantity;
                            trades.Add(new Trade
                            {
                                Date = today.Date,
                                Type = "BUY",
                                Price = price,
                                Quantity = quantity,
                                Total = cost
                            });
                        }
                    }
                }
                else if (signal.Equals("SELL", StringComparison.OrdinalIgnoreCase))
                {
                    if (shares > 0)
                    {
                        var proceeds = shares * price;
                        trades.Add(new Trade
                        {
                            Date = today.Date,
                            Type = "SELL",
                            Price = price,
                            Quantity = shares,
                            Total = proceeds
                        });
                        cash += proceeds;
                        shares = 0;
                    }
                }

                var portfolioValue = cash + shares * price;
                pvSeries.Add(new PortfolioValue { Date = today.Date, Value = portfolioValue });
            }

            // If still holding at end, we keep as-is (no forced sell) to reflect unrealized PnL.

            // 5. Calculate metrics
            var initial = request.InitialCapital;
            var final = pvSeries[^1].Value;
            var totalReturnPct = MetricsCalculator.CalculateTotalReturn(initial, final);

            // daily returns
            var dailyReturns = new List<decimal>();
            for (int i = 1; i < pvSeries.Count; i++)
            {
                var prev = pvSeries[i - 1].Value;
                var curr = pvSeries[i].Value;
                dailyReturns.Add(prev == 0 ? 0 : (curr - prev) / prev);
            }

            var volatility = MetricsCalculator.CalculateVolatility(dailyReturns);
            var annReturnFraction = MetricsCalculator.CalculateAnnualizedReturn(totalReturnPct, pvSeries.Count);
            var sharpe = MetricsCalculator.CalculateSharpeRatio(annReturnFraction, volatility);
            var mdd = MetricsCalculator.CalculateMaxDrawdown(pvSeries.Select(p => p.Value).ToList());
            var winRate = MetricsCalculator.CalculateWinRate(trades);

            var metrics = new PerformanceMetrics
            {
                TotalReturn = totalReturnPct,
                CumulativeReturn = final,
                AnnualizedReturn = annReturnFraction,
                Volatility = volatility,
                SharpeRatio = sharpe,
                MaxDrawdown = mdd,
                NumberOfTrades = trades.Count,
                WinRate = winRate
            };

            // 6. Prepare result
            return new BacktestResult
            {
                Symbol = request.Symbol.ToUpperInvariant(),
                Strategy = strategy.Name,
                InitialCapital = initial,
                FinalCapital = final,
                Metrics = metrics,
                Trades = trades,
                PortfolioValues = pvSeries
            };
        }
    }
}
