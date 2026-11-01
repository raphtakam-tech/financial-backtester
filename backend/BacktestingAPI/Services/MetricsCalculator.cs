using BacktestingAPI.Models;

namespace BacktestingAPI.Services
{

    /// Static utility methods to compute portfolio performance metrics.

    public static class MetricsCalculator
    {

        /// ((final - initial) / initial) * 100

        public static decimal CalculateTotalReturn(decimal initial, decimal final)
        {
            if (initial == 0) return 0;
            return (final - initial) / initial * 100m;
        }


        /// (1 + totalReturnFraction)^(365/days) - 1, where totalReturnFraction = totalReturnPct/100.
        /// Returns the annualized return as a percentage (e.g., 12 for 12%).

        public static decimal CalculateAnnualizedReturn(decimal totalReturnPct, int days)
        {
            if (days <= 0) return 0;
            var tr = (double)(totalReturnPct / 100m);
            var yearsFactor = 365.0 / days;
            var annualFraction = Math.Pow(1 + tr, yearsFactor) - 1;
            return (decimal)annualFraction * 100m;
        }


        /// Standard deviation of daily returns times sqrt(252) to annualize.

        public static decimal CalculateVolatility(List<decimal> dailyReturns)
        {
            if (dailyReturns.Count <= 1) return 0;
            var avg = dailyReturns.Average();
            var variance = dailyReturns.Select(r => Math.Pow((double)(r - avg), 2)).Average();
            var stdDev = Math.Sqrt(variance);
            return (decimal)(stdDev * Math.Sqrt(252));
        }


        /// (annualizedReturn - riskFreeRate) / volatility, with annualizedReturn as fraction.

        /// <param name="annualizedReturnFraction">The annualized return as a decimal fraction (e.g., 0.12 for 12%), not a percentage.</param>
        /// <param name="volatility">The annualized volatility as a decimal fraction.</param>
        /// <param name="riskFreeRate">The risk-free rate as a decimal fraction (default is 0.02 for 2%).</param>
        public static decimal CalculateSharpeRatio(decimal annualizedReturnFraction, decimal volatility, decimal riskFreeRate = 0.02m)
        {
            if (volatility == 0) return 0;
            return (annualizedReturnFraction - riskFreeRate) / volatility;
        }


        /// Max drawdown as a percentage.

        public static decimal CalculateMaxDrawdown(List<decimal> portfolioValues)
        {
            if (portfolioValues.Count == 0) return 0;
            decimal peak = portfolioValues[0];
            decimal maxDd = 0;

            foreach (var v in portfolioValues)
            {
                if (v > peak) peak = v;
                if (peak == 0) continue;
                var dd = (peak - v) / peak; // fraction
                if (dd > maxDd) maxDd = dd;
            }
            return maxDd * 100m;
        }


        /// Percent of BUY-SELL trade pairs that are profitable.

        public static decimal CalculateWinRate(List<Trade> trades)
        {
            int wins = 0, total = 0;
            Trade? pendingBuy = null;
            foreach (var t in trades)
            {
                if (t.Type.Equals("BUY", StringComparison.OrdinalIgnoreCase))
                {
                    pendingBuy = t;
                }
                else if (t.Type.Equals("SELL", StringComparison.OrdinalIgnoreCase) && pendingBuy != null)
                {
                    total++;
                    if (t.Price > pendingBuy.Price) wins++;
                    pendingBuy = null;
                }
            }
            if (total == 0) return 0;
            return (decimal)wins / total * 100m;
        }
    }
}
