import React, { useEffect, useMemo, useState } from 'react'
import api from '../services/api'

const DEFAULTS = {
  symbol: 'AAPL',
  initialCapital: 10000,
  startDate: '',
  endDate: '',
  strategyType: 'SMA',
  parameters: { shortPeriod: 20, longPeriod: 50 }
}

const DISPLAY_STRATEGIES = [
  { label: 'SMA', value: 'SMA' },
  { label: 'Buy & Hold', value: 'BuyAndHold' }
]

export default function BacktestForm({ onRun, isLoading, error }) {
  const [formData, setFormData] = useState({ ...DEFAULTS })
  const [strategies, setStrategies] = useState(DISPLAY_STRATEGIES)
  const [paramSchema, setParamSchema] = useState({ shortPeriod: 20, longPeriod: 50 })

  // Set default dates (last 1 year)
  useEffect(() => {
    const end = new Date()
    const start = new Date()
    start.setFullYear(end.getFullYear() - 1)
    setFormData((prevFormData) => ({ ...prevFormData, startDate: start.toISOString().slice(0, 10), endDate: end.toISOString().slice(0, 10) }))
  }, [])

  // Fetch available strategies (fallback to default)
  useEffect(() => {
    api.getAvailableStrategies().then(list => {
      if (Array.isArray(list) && list.length) {
        const mapped = list.map(strategy => ({ label: strategy.name, value: strategy.name }))
        setStrategies(mapped)
      }
    }).catch(() => {})
  }, [])

  // Update parameter schema for selected strategy
  useEffect(() => {
    api.getStrategyParameters(formData.strategyType).then(p => {
      setParamSchema(p)
      // initialize missing params with defaults from schema
      setFormData((f) => ({ ...f, parameters: { ...p, ...f.parameters } }))
    }).catch(() => {
      // fallback defaults for SMA / BuyAndHold
      if (formData.strategyType === 'SMA') setParamSchema({ shortPeriod: 20, longPeriod: 50 })
      else setParamSchema({})
    })
  }, [formData.strategyType])

  const handleChange = (e) => {
    const { name, value } = e.target
    setFormData((prevFormData) => ({ ...prevFormData, [name]: name === 'initialCapital' ? Number(value) : value }))
  }

  const handleParamChange = (e) => {
    const { name, value } = e.target
    setFormData((f) => ({ ...f, parameters: { ...f.parameters, [name]: Number(value) } }))
  }

  const validate = () => {
    const start = new Date(formData.startDate)
    const end = new Date(formData.endDate)
    if (!(start < end)) return 'End date must be after start date.'
    if (!(formData.initialCapital > 0)) return 'Initial capital must be greater than 0.'
    return null
  }

  const handleSubmit = async (e) => {
    e.preventDefault()
    const validationError = validate()
    if (validationError) {
      // Option 1: If you want to display the error in the UI, you can call onRun with null and error
      if (onRun) onRun(null, validationError)
      return
    }

    const payload = {
      symbol: formData.symbol.toUpperCase(),
      startDate: formData.startDate,
      endDate: formData.endDate,
      initialCapital: Number(formData.initialCapital),
      strategyType: formData.strategyType,
      parameters: formData.strategyType === 'SMA' ? {
        shortPeriod: Number(formData.parameters.shortPeriod),
        longPeriod: Number(formData.parameters.longPeriod)
      } : {}
    }

    onRun && onRun(payload)
  }

  const strategyFields = useMemo(() => {
    if (formData.strategyType !== 'SMA') return null
    return (
      <div className="space-y-4 pt-2">
        <div className="text-sm font-semibold text-gray-700 flex items-center gap-2">
          <svg className="w-4 h-4 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M12 6V4m0 2a2 2 0 100 4m0-4a2 2 0 110 4m-6 8a2 2 0 100-4m0 4a2 2 0 110-4m0 4v2m0-6V4m6 6v10m6-2a2 2 0 100-4m0 4a2 2 0 110-4m0 4v2m0-6V4" />
          </svg>
          Strategy Parameters
        </div>
        <div className="grid grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">Short Period</label>
            <input type="number" name="shortPeriod" min="1" value={formData.parameters.shortPeriod ?? ''} onChange={handleParamChange} className="w-full border border-gray-300 rounded-lg px-4 py-2.5 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all" />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">Long Period</label>
            <input type="number" name="longPeriod" min="1" value={formData.parameters.longPeriod ?? ''} onChange={handleParamChange} className="w-full border border-gray-300 rounded-lg px-4 py-2.5 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all" />
          </div>
        </div>
      </div>
    )
  }, [formData])

  return (
    <form onSubmit={handleSubmit} className="bg-white rounded-xl shadow-lg p-6 space-y-6 border border-gray-100">
      <div className="flex items-center gap-3 pb-2 border-b border-gray-100">
        <div className="p-2 bg-blue-50 rounded-lg">
          <svg className="w-6 h-6 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
          </svg>
        </div>
        <div>
          <div className="text-xl font-bold text-gray-900">Backtest Configuration</div>
          <div className="text-xs text-gray-500">Configure your trading strategy parameters</div>
        </div>
      </div>

      {error && (
        <div className="p-4 rounded-lg bg-red-50 border border-red-200 text-red-700 text-sm flex items-start gap-3 animate-shake">
          <svg className="w-5 h-5 flex-shrink-0 mt-0.5" fill="currentColor" viewBox="0 0 20 20">
            <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clipRule="evenodd" />
          </svg>
          <span>{error}</span>
        </div>
      )}

      <div className="space-y-5">
        {/* Stock Selection */}
        <div className="space-y-2">
          <label className="block text-sm font-semibold text-gray-700 flex items-center gap-2">
            <svg className="w-4 h-4 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M7 12l3-3 3 3 4-4M8 21l4-4 4 4M3 4h18M4 4h16v12a1 1 0 01-1 1H5a1 1 0 01-1-1V4z" />
            </svg>
            Stock Symbol
          </label>
          <select name="symbol" value={formData.symbol} onChange={handleChange} className="w-full border border-gray-300 rounded-lg px-4 py-2.5 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all bg-white font-medium">
            {['AAPL', 'MSFT', 'GOOGL', 'TSLA'].map(sym => (
              <option key={sym} value={sym}>{sym}</option>
            ))}
          </select>
        </div>

        {/* Initial Capital */}
        <div className="space-y-2">
          <label className="block text-sm font-semibold text-gray-700 flex items-center gap-2">
            <svg className="w-4 h-4 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
            Initial Capital
          </label>
          <div className="relative">
            <span className="absolute left-4 top-1/2 -translate-y-1/2 text-gray-500 font-medium">$</span>
            <input type="number" name="initialCapital" value={formData.initialCapital} min="1" onChange={handleChange} className="w-full border border-gray-300 rounded-lg pl-8 pr-4 py-2.5 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all" />
          </div>
        </div>

        {/* Date Range */}
        <div className="space-y-3 pt-2">
          <div className="text-sm font-semibold text-gray-700 flex items-center gap-2">
            <svg className="w-4 h-4 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
            </svg>
            Date Range
          </div>
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-xs font-medium text-gray-600 mb-2">Start Date</label>
              <input type="date" name="startDate" value={formData.startDate} onChange={handleChange} className="w-full border border-gray-300 rounded-lg px-4 py-2.5 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all" />
            </div>
            <div>
              <label className="block text-xs font-medium text-gray-600 mb-2">End Date</label>
              <input type="date" name="endDate" value={formData.endDate} onChange={handleChange} className="w-full border border-gray-300 rounded-lg px-4 py-2.5 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all" />
            </div>
          </div>
        </div>

        {/* Strategy Selection */}
        <div className="space-y-3 pt-2">
          <div className="text-sm font-semibold text-gray-700 flex items-center gap-2">
            <svg className="w-4 h-4 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-3 7h3m-3 4h3m-6-4h.01M9 16h.01" />
            </svg>
            Strategy Type
          </div>
          <select name="strategyType" value={formData.strategyType} onChange={(e) => setFormData((f) => ({ ...f, strategyType: e.target.value }))} className="w-full border border-gray-300 rounded-lg px-4 py-2.5 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all bg-white font-medium">
            {strategies.map((strategy) => (
              <option key={strategy.value} value={strategy.value}>{strategy.label}</option>
            ))}
          </select>
        </div>

        {strategyFields}
      </div>

      <div className="pt-4">
        <button type="submit" disabled={isLoading} className="bg-gradient-to-r from-blue-600 to-blue-700 hover:from-blue-700 hover:to-blue-800 disabled:from-gray-400 disabled:to-gray-500 disabled:cursor-not-allowed text-white font-semibold py-3 px-6 rounded-lg transition-all duration-200 w-full shadow-md hover:shadow-lg transform hover:-translate-y-0.5 active:translate-y-0 flex items-center justify-center gap-2">
          {isLoading ? (
            <>
              <svg className="animate-spin h-5 w-5" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v4a4 4 0 00-4 4H4z"></path>
              </svg>
              Running Backtest...
            </>
          ) : (
            <>
              <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M14.752 11.168l-3.197-2.132A1 1 0 0010 9.87v4.263a1 1 0 001.555.832l3.197-2.132a1 1 0 000-1.664z" />
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              Run Backtest
            </>
          )}
        </button>
      </div>
    </form>
  )
}