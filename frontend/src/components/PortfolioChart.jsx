import React from 'react'
import { ResponsiveContainer, LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ReferenceLine, Area, AreaChart } from 'recharts'
import { formatCurrency, formatDate } from '../utils/format'

export default function PortfolioChart({ portfolioValues = [], initialCapital = 0 }) {
  const data = (portfolioValues || []).map(portfolioValue => ({
    date: portfolioValue.date || portfolioValue.Date,
    value: Number(portfolioValue.value ?? portfolioValue.Value)
  }))

  const finalValue = data.length > 0 ? data[data.length - 1].value : initialCapital
  const isProfit = finalValue >= initialCapital
  const returnPercentage = ((finalValue - initialCapital) / initialCapital * 100).toFixed(2)

  return (
    <div className="bg-white rounded-xl shadow-lg p-6 border border-gray-100">
      <div className="flex items-center justify-between mb-6">
        <div className="flex items-center gap-3">
          <div className="p-2 bg-blue-50 rounded-lg">
            <svg className="w-6 h-6 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M7 12l3-3 3 3 4-4M8 21l4-4 4 4M3 4h18M4 4h16v12a1 1 0 01-1 1H5a1 1 0 01-1-1V4z" />
            </svg>
          </div>
          <div>
            <div className="text-xl font-bold text-gray-900">Portfolio Value Over Time</div>
            <div className="text-sm text-gray-500">Track your investment performance</div>
          </div>
        </div>
        <div className={`px-4 py-2 rounded-lg ${isProfit ? 'bg-green-50 border border-green-200' : 'bg-red-50 border border-red-200'}`}>
          <div className="text-xs font-medium text-gray-600 mb-1">Net Change</div>
          <div className={`text-lg font-bold ${isProfit ? 'text-green-600' : 'text-red-600'}`}>
            {isProfit ? '+' : ''}{returnPercentage}%
          </div>
        </div>
      </div>

      {/* Legend */}
      <div className="flex items-center gap-6 mb-4 text-sm">
        <div className="flex items-center gap-2">
          <div className="w-8 h-0.5 bg-blue-600"></div>
          <span className="text-gray-600">Portfolio Value</span>
        </div>
        <div className="flex items-center gap-2">
          <div className="w-8 h-0.5 bg-gray-400 border-t-2 border-dashed"></div>
          <span className="text-gray-600">Initial Capital: {formatCurrency(initialCapital)}</span>
        </div>
        <div className="flex items-center gap-2">
          <div className="w-4 h-4 bg-green-100 border border-green-300 rounded"></div>
          <span className="text-gray-600">Profit Zone</span>
        </div>
        <div className="flex items-center gap-2">
          <div className="w-4 h-4 bg-red-100 border border-red-300 rounded"></div>
          <span className="text-gray-600">Loss Zone</span>
        </div>
      </div>

      <div style={{ height: 400 }}>
        <ResponsiveContainer width="100%" height="100%">
          <AreaChart data={data} margin={{ top: 10, right: 20, left: 10, bottom: 0 }}>
            <defs>
              <linearGradient id="colorProfit" x1="0" y1="0" x2="0" y2="1">
                <stop offset="5%" stopColor="#10b981" stopOpacity={0.15}/>
                <stop offset="95%" stopColor="#10b981" stopOpacity={0}/>
              </linearGradient>
              <linearGradient id="colorLoss" x1="0" y1="0" x2="0" y2="1">
                <stop offset="5%" stopColor="#ef4444" stopOpacity={0.15}/>
                <stop offset="95%" stopColor="#ef4444" stopOpacity={0}/>
              </linearGradient>
            </defs>
            <CartesianGrid stroke="#E5E7EB" strokeDasharray="3 3" />
            <XAxis
              dataKey="date"
              tickFormatter={(d) => formatDate(d)}
              minTickGap={30}
              stroke="#6B7280"
              style={{ fontSize: '12px' }}
            />
            <YAxis
              tickFormatter={(v) => formatCurrency(v)}
              width={90}
              stroke="#6B7280"
              style={{ fontSize: '12px' }}
            />
            <Tooltip
              contentStyle={{
                backgroundColor: 'white',
                border: '1px solid #E5E7EB',
                borderRadius: '8px',
                padding: '12px',
                boxShadow: '0 4px 6px -1px rgba(0, 0, 0, 0.1)'
              }}
              formatter={(value) => [formatCurrency(value), 'Portfolio Value']}
              labelFormatter={(label) => formatDate(label)}
            />
            <ReferenceLine 
              y={Number(initialCapital)} 
              stroke="#9CA3AF" 
              strokeDasharray="4 4" 
              strokeWidth={2}
              label={{ 
                value: 'Initial Capital', 
                position: 'insideTopRight',
                fill: '#6B7280',
                fontSize: 12,
                fontWeight: 600
              }}
            />
            <Area 
              type="monotone" 
              dataKey="value" 
              stroke="#3B82F6" 
              strokeWidth={3}
              fill={isProfit ? "url(#colorProfit)" : "url(#colorLoss)"}
              fillOpacity={1}
            />
          </AreaChart>
        </ResponsiveContainer>
      </div>

      {/* Stats Summary */}
      <div className="grid grid-cols-3 gap-4 mt-6 pt-6 border-t border-gray-100">
        <div>
          <div className="text-xs font-medium text-gray-500 mb-1">Starting Value</div>
          <div className="text-lg font-bold text-gray-900">{formatCurrency(initialCapital)}</div>
        </div>
        <div>
          <div className="text-xs font-medium text-gray-500 mb-1">Final Value</div>
          <div className={`text-lg font-bold ${isProfit ? 'text-green-600' : 'text-red-600'}`}>
            {formatCurrency(finalValue)}
          </div>
        </div>
        <div>
          <div className="text-xs font-medium text-gray-500 mb-1">Total Gain/Loss</div>
          <div className={`text-lg font-bold ${isProfit ? 'text-green-600' : 'text-red-600'}`}>
            {isProfit ? '+' : ''}{formatCurrency(finalValue - initialCapital)}
          </div>
        </div>
      </div>
    </div>
  )
}