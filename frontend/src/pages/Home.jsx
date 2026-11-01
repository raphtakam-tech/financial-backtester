import React, { useState } from 'react'
import BacktestForm from '../components/BacktestForm'
import ResultsDisplay from '../components/ResultsDisplay'
import LoadingSpinner from '../components/LoadingSpinner'
import api from '../services/api'

export default function Home() {
  const [results, setResults] = useState(null)
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState('')

  const handleRun = async (payload) => {
    setError('')
    setIsLoading(true)
    try {
      const res = await api.runBacktest(payload)
      setResults(res)
    } catch (err) {
      setResults(null)
      setError(err?.message || 'Failed to run backtest')
    } finally {
      setIsLoading(false)
    }
  }

  const reset = () => {
    setResults(null)
    setError('')
  }

  return (
    <div className="max-w-7xl mx-auto px-4 py-8">
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6 items-start">
        {/* Form Section */}
        <div className="lg:col-span-1">
          <BacktestForm onRun={handleRun} isLoading={isLoading} error={error} />
        </div>

        {/* Results Section */}
        <div className="lg:col-span-2">
          {isLoading && (
            <div className="bg-white rounded-xl shadow-lg p-8 border border-gray-100">
              <LoadingSpinner />
            </div>
          )}

          {!isLoading && results && (
            <ResultsDisplay results={results} onNewBacktest={reset} />
          )}

          {!isLoading && !results && !error && (
            <div className="bg-gradient-to-br from-blue-50 to-indigo-50 rounded-xl shadow-lg p-8 border border-blue-100">
              <div className="flex flex-col items-center text-center space-y-6">
                <div className="p-4 bg-white rounded-full shadow-md">
                  <svg className="w-16 h-16 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
                  </svg>
                </div>
                
                <div className="space-y-3">
                  <div className="text-3xl font-bold text-gray-900">
                    Welcome to GPR1 - Backtesting
                  </div>
                  <div className="text-lg text-gray-600 max-w-md mx-auto">
                    Test your trading strategies with historical market data and see how they would have performed
                  </div>
                </div>

                <div className="grid grid-cols-1 md:grid-cols-3 gap-4 w-full max-w-2xl pt-4">
                  <div className="bg-white rounded-lg p-4 shadow-sm border border-blue-100">
                    <div className="flex items-center justify-center w-10 h-10 bg-blue-100 rounded-lg mx-auto mb-3">
                      <svg className="w-6 h-6 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
                      </svg>
                    </div>
                    <div className="text-sm font-semibold text-gray-900 mb-1">1. Configure</div>
                    <div className="text-xs text-gray-600">Set your parameters in the form on the left</div>
                  </div>

                  <div className="bg-white rounded-lg p-4 shadow-sm border border-blue-100">
                    <div className="flex items-center justify-center w-10 h-10 bg-green-100 rounded-lg mx-auto mb-3">
                      <svg className="w-6 h-6 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M14.752 11.168l-3.197-2.132A1 1 0 0010 9.87v4.263a1 1 0 001.555.832l3.197-2.132a1 1 0 000-1.664z" />
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                      </svg>
                    </div>
                    <div className="text-sm font-semibold text-gray-900 mb-1">2. Run Test</div>
                    <div className="text-xs text-gray-600">Execute the backtest with your strategy</div>
                  </div>

                  <div className="bg-white rounded-lg p-4 shadow-sm border border-blue-100">
                    <div className="flex items-center justify-center w-10 h-10 bg-purple-100 rounded-lg mx-auto mb-3">
                      <svg className="w-6 h-6 text-purple-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
                      </svg>
                    </div>
                    <div className="text-sm font-semibold text-gray-900 mb-1">3. Analyze</div>
                    <div className="text-xs text-gray-600">Review detailed performance metrics</div>
                  </div>
                </div>

                <div className="flex items-center gap-2 text-sm text-gray-500 pt-4">
                  <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                  </svg>
                  <span>Get started by selecting a stock symbol and clicking <span className="font-semibold text-blue-600">Run Backtest</span></span>
                </div>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  )
}