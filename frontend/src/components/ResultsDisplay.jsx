import React from 'react'
import MetricsCard from './MetricsCard'
import PortfolioChart from './PortfolioChart'
import TradesTable from './TradesTable'
import { formatCurrency, formatPercentage } from '../utils/format'

export default function ResultsDisplay({ results, onNewBacktest }) {
  if (!results) return null

  const metrics = results.metrics || results.Metrics || {}
  const symbol = results.symbol || results.Symbol
  const strategy = results.strategy || results.Strategy
  const initialCapital = Number(results.initialCapital ?? results.InitialCapital)

  const totalReturn = metrics.totalReturn ?? metrics.TotalReturn
  const annualizedReturn = metrics.annualizedReturn ?? metrics.AnnualizedReturn

  const secondaryMetrics = [
    { label: 'Volatility', value: metrics.volatility ?? metrics.Volatility, format: 'percentage', positive: false, icon: 'wave' },
    { label: 'Sharpe Ratio', value: metrics.sharpeRatio ?? metrics.SharpeRatio, format: 'ratio', positive: (metrics.sharpeRatio ?? metrics.SharpeRatio) >= 1, icon: 'chart' },
    { label: 'Max Drawdown', value: metrics.maxDrawdown ?? metrics.MaxDrawdown, format: 'percentage', positive: false, icon: 'down' },
    { label: 'Win Rate', value: metrics.winRate ?? metrics.WinRate, format: 'percentage', positive: (metrics.winRate ?? metrics.WinRate) >= 50, icon: 'target' },
  ]

  return (
    <div className="space-y-6 animate-fadeIn">
      {/* Header */}
      <div className="bg-gradient-to-r from-blue-600 to-blue-700 rounded-xl shadow-lg p-6 text-white">
        <div className="flex items-center justify-between">
          <div className="space-y-2">
            <div className="flex items-center gap-3">
              <div className="p-2 bg-white/20 rounded-lg backdrop-blur-sm">
                <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
                </svg>
              </div>
              <div>
                <div className="text-2xl font-bold">{symbol}</div>
                <div className="text-sm text-blue-100">{strategy} Strategy</div>
              </div>
            </div>
            <div className="text-sm text-blue-100 flex items-center gap-2">
              <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              Initial Capital: {formatCurrency(initialCapital)}
            </div>
          </div>
          <button onClick={onNewBacktest} className="bg-white text-blue-600 hover:bg-blue-50 font-semibold py-2.5 px-5 rounded-lg transition-all duration-200 shadow-md hover:shadow-lg transform hover:-translate-y-0.5 active:translate-y-0 flex items-center gap-2">
            <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
            </svg>
            New Backtest
          </button>
        </div>
      </div>

      {/* Primary Metrics - Large Cards */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        <div className="bg-white rounded-xl shadow-lg p-6 border border-gray-100">
          <div className="flex items-start justify-between mb-3">
            <div className="flex items-center gap-3">
              <div className={`p-3 rounded-lg ${totalReturn >= 0 ? 'bg-green-50' : 'bg-red-50'}`}>
                <svg className={`w-6 h-6 ${totalReturn >= 0 ? 'text-green-600' : 'text-red-600'}`} fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d={totalReturn >= 0 ? "M13 7h8m0 0v8m0-8l-8 8-4-4-6 6" : "M13 17h8m0 0V9m0 8l-8-8-4 4-6-6"} />
                </svg>
              </div>
              <div>
                <div className="text-sm font-medium text-gray-600">Total Return</div>
                <div className="text-xs text-gray-500">Overall performance</div>
              </div>
            </div>
            {totalReturn >= 0 ? (
              <span className="px-3 py-1 bg-green-100 text-green-700 text-xs font-semibold rounded-full">PROFIT</span>
            ) : (
              <span className="px-3 py-1 bg-red-100 text-red-700 text-xs font-semibold rounded-full">LOSS</span>
            )}
          </div>
          <div className={`text-4xl font-bold ${totalReturn >= 0 ? 'text-green-600' : 'text-red-600'}`}>
            {formatPercentage(totalReturn)}
          </div>
        </div>

        <div className="bg-white rounded-xl shadow-lg p-6 border border-gray-100">
          <div className="flex items-start justify-between mb-3">
            <div className="flex items-center gap-3">
              <div className={`p-3 rounded-lg ${annualizedReturn >= 0 ? 'bg-blue-50' : 'bg-orange-50'}`}>
                <svg className={`w-6 h-6 ${annualizedReturn >= 0 ? 'text-blue-600' : 'text-orange-600'}`} fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M7 12l3-3 3 3 4-4M8 21l4-4 4 4M3 4h18M4 4h16v12a1 1 0 01-1 1H5a1 1 0 01-1-1V4z" />
                </svg>
              </div>
              <div>
                <div className="text-sm font-medium text-gray-600">Annualized Return</div>
                <div className="text-xs text-gray-500">Year-over-year growth</div>
              </div>
            </div>
          </div>
          <div className={`text-4xl font-bold ${annualizedReturn >= 0 ? 'text-blue-600' : 'text-orange-600'}`}>
            {formatPercentage(annualizedReturn)}
          </div>
        </div>
      </div>

      {/* Secondary Metrics - Small Cards */}
      <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
        {secondaryMetrics.map((card, idx) => (
          <MetricsCard key={idx} label={card.label} value={typeof card.value === 'number' ? card.value : Number(card.value)} format={card.format} positive={card.positive} icon={card.icon} />
        ))}
      </div>

      {/* Chart */}
      <PortfolioChart portfolioValues={results.portfolioValues || results.PortfolioValues} initialCapital={initialCapital} />

      {/* Trades Table */}
      <TradesTable trades={results.trades || results.Trades} />
    </div>
  )
}