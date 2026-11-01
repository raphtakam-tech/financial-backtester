import React from 'react'
import ResultsDisplay from '../components/ResultsDisplay'

// Optional separate page if routing expands later
export default function Results({ results, onNewBacktest }) {
  return (
    <div className="max-w-7xl mx-auto px-4 py-8">
      {results ? (
        <ResultsDisplay results={results} onNewBacktest={onNewBacktest} />
      ) : (
        <div className="bg-white rounded-lg shadow-md p-6">No results to display. Check console for errors ?? </div>
      )}
    </div>
  )
}
