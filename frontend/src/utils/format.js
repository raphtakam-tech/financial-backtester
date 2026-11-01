export function formatCurrency(value) {
  if (value === null || value === undefined || isNaN(value)) return '-'
  return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(Number(value))
}

export function formatPercentage(value, fractionDigits = 1) {
  if (value === null || value === undefined || isNaN(value)) return '-'
  const numericValue = Number(value)
  return `${numericValue.toFixed(fractionDigits)}%`
}

export function formatDate(dateStr) {
  if (!dateStr) return '-'
  const date = new Date(dateStr)
  if (isNaN(date.getTime())) return '-'
  return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' })
}

export function formatNumber(value) {
  if (value === null || value === undefined || isNaN(value)) return '-'
  return new Intl.NumberFormat('en-US').format(Number(value))
}
