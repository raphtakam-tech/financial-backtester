import React from 'react'
import { formatCurrency, formatPercentage, formatNumber } from '../utils/format'

function formatValue(value, format) {
  switch (format) {
    case 'currency':
      return formatCurrency(value)
    case 'percentage':
      return formatPercentage(value)
    case 'number':
      return formatNumber(value)
    case 'ratio':
      return value !== null && value !== undefined ? value.toFixed(2) : '-'
    default:
      return value
  }
}

function getIcon(iconName) {
  const icons = {
    wave: (
      <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M7 16V4m0 0L3 8m4-4l4 4m6 0v12m0 0l4-4m-4 4l-4-4" />
      </svg>
    ),
    chart: (
      <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M16 8v8m-4-5v5m-4-2v2m-2 4h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
      </svg>
    ),
    down: (
      <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M13 17h8m0 0V9m0 8l-8-8-4 4-6-6" />
      </svg>
    ),
    target: (
      <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
      </svg>
    )
  }
  return icons[iconName] || icons.chart
}

export default function MetricsCard({ label, value, format = 'number', positive, icon }) {
  const isPositive = typeof positive === 'boolean' ? positive : (typeof value === 'number' ? value >= 0 : undefined)
  
  let colorClasses = {
    text: 'text-gray-900',
    bg: 'bg-gray-50',
    border: 'border-gray-200'
  }

  if (isPositive === true) {
    colorClasses = {
      text: 'text-green-600',
      bg: 'bg-green-50',
      border: 'border-green-100'
    }
  } else if (isPositive === false) {
    colorClasses = {
      text: 'text-red-600',
      bg: 'bg-red-50',
      border: 'border-red-100'
    }
  }

  return (
    <div className={`bg-white rounded-lg shadow-md p-4 border ${colorClasses.border} transition-all hover:shadow-lg`}>
      <div className="flex items-center justify-between mb-3">
        <div className="text-xs font-semibold text-gray-600 uppercase tracking-wide">{label}</div>
        {icon && (
          <div className={`p-1.5 rounded-md ${colorClasses.bg} ${colorClasses.text}`}>
            {getIcon(icon)}
          </div>
        )}
      </div>
      <div className={`text-2xl font-bold ${colorClasses.text}`}>{formatValue(value, format)}</div>
      {isPositive !== undefined && (
        <div className="mt-2 flex items-center gap-1">
          {isPositive ? (
            <>
              <svg className="w-3 h-3 text-green-600" fill="currentColor" viewBox="0 0 20 20">
                <path fillRule="evenodd" d="M5.293 9.707a1 1 0 010-1.414l4-4a1 1 0 011.414 0l4 4a1 1 0 01-1.414 1.414L11 7.414V15a1 1 0 11-2 0V7.414L6.707 9.707a1 1 0 01-1.414 0z" clipRule="evenodd" />
              </svg>
              <span className="text-xs font-medium text-green-600">Positive</span>
            </>
          ) : (
            <>
              <svg className="w-3 h-3 text-red-600" fill="currentColor" viewBox="0 0 20 20">
                <path fillRule="evenodd" d="M14.707 10.293a1 1 0 010 1.414l-4 4a1 1 0 01-1.414 0l-4-4a1 1 0 111.414-1.414L9 12.586V5a1 1 0 012 0v7.586l2.293-2.293a1 1 0 011.414 0z" clipRule="evenodd" />
              </svg>
              <span className="text-xs font-medium text-red-600">Negative</span>
            </>
          )}
        </div>
      )}
    </div>
  )
}