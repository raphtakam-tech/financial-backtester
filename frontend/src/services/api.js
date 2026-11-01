import axios from 'axios'

const BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000'

const api = axios.create({
  baseURL: BASE_URL,
  timeout: 30000,
})

// Response interceptor for unified error messages
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response) {
      const status = error.response.status
      const errorData = error.response.data || {}
      const serverMessage = errorData.error || errorData.message
      let message = 'Unexpected server error.'
      if (status === 400) message = serverMessage || 'Invalid request. Please check your inputs.'
      else if (status === 404) message = serverMessage || 'Resource not found.'
      else if (status === 429) message = 'Daily API limit reached. Please try again later.'
      else if (status >= 500) message = serverMessage || 'Server error. Please try again later.'
      return Promise.reject({ message, status })
    }

    if (error.request) {
      return Promise.reject({ message: 'Could not connect to server. Please check your connection.' })
    }

    return Promise.reject({ message: error.message || 'An unknown error occurred.' })
  }
)

export async function getAvailableStrategies() {
  const res = await api.get('/api/strategy/list')
  return res.data
}

export async function getStrategyParameters(strategyType) {
  const res = await api.get(`/api/strategy/${encodeURIComponent(strategyType)}/parameters`)
  return res.data
}

export async function runBacktest(request) {
  const res = await api.post('/api/backtest/run', request)
  return res.data
}

export async function getCacheStatus(symbol) {
  const res = await api.get(`/api/stockdata/cache-status/${encodeURIComponent(symbol)}`)
  return res.data
}

export default {
  getAvailableStrategies,
  getStrategyParameters,
  runBacktest,
  getCacheStatus,
}
