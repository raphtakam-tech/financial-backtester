import React from 'react'

export default function LoadingSpinner({ text = 'Running backtest...' }) {
  return (
    <div className="w-full h-full flex items-center justify-center py-16">
      <div className="flex flex-col items-center gap-6">
        <div className="relative">
          {/* Outer rotating ring */}
          <div className="absolute inset-0">
            <svg className="animate-spin h-20 w-20 text-blue-600" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
              <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="3"></circle>
              <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
            </svg>
          </div>
          
          {/* Center icon */}
          <div className="relative flex items-center justify-center h-20 w-20">
            <div className="p-3 bg-blue-50 rounded-full animate-pulse">
              <svg className="w-8 h-8 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
              </svg>
            </div>
          </div>
        </div>
        
        <div className="text-center space-y-3 max-w-md">
          <div className="text-xl font-bold text-gray-900">{text}</div>
          <div className="text-sm text-gray-600">
            This may take a few moments depending on your connection and AlphaVantage API response time
          </div>
          
          {/* Progress dots */}
          <div className="flex items-center justify-center gap-2 pt-2">
            <div className="w-2 h-2 bg-blue-600 rounded-full animate-bounce" style={{ animationDelay: '0ms' }}></div>
            <div className="w-2 h-2 bg-blue-600 rounded-full animate-bounce" style={{ animationDelay: '150ms' }}></div>
            <div className="w-2 h-2 bg-blue-600 rounded-full animate-bounce" style={{ animationDelay: '300ms' }}></div>
          </div>
        </div>

        {/* Status indicators */}
        <div className="grid grid-cols-3 gap-4 w-full max-w-md pt-4">
          <div className="bg-blue-50 rounded-lg p-3 border border-blue-100">
            <div className="flex items-center gap-2 mb-1">
              <div className="w-2 h-2 bg-blue-600 rounded-full animate-pulse"></div>
              <div className="text-xs font-semibold text-blue-900">Fetching Data</div>
            </div>
            <div className="text-xs text-blue-600">Retrieving market data</div>
          </div>
          
          <div className="bg-purple-50 rounded-lg p-3 border border-purple-100">
            <div className="flex items-center gap-2 mb-1">
              <div className="w-2 h-2 bg-purple-600 rounded-full animate-pulse" style={{ animationDelay: '300ms' }}></div>
              <div className="text-xs font-semibold text-purple-900">Processing</div>
            </div>
            <div className="text-xs text-purple-600">Applying strategy</div>
          </div>
          
          <div className="bg-green-50 rounded-lg p-3 border border-green-100">
            <div className="flex items-center gap-2 mb-1">
              <div className="w-2 h-2 bg-green-600 rounded-full animate-pulse" style={{ animationDelay: '600ms' }}></div>
              <div className="text-xs font-semibold text-green-900">Calculating</div>
            </div>
            <div className="text-xs text-green-600">Computing metrics</div>
          </div>
        </div>
      </div>
    </div>
  )
}