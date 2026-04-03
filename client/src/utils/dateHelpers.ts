export const formatDate = (isoString: string): string => {
  if (!isoString) return '—'
  return new Date(isoString).toLocaleDateString('en-US', {
    month: 'short',
    day: 'numeric',
    year: 'numeric',
  })
}

export const formatDateRange = (startIso: string, endIso: string): string =>
  `${formatDate(startIso)} – ${formatDate(endIso)}`

/** Converts an ISO string to the YYYY-MM-DD format required by <input type="date"> */
export const toInputDateValue = (isoString: string): string => {
  if (!isoString) return ''
  return isoString.split('T')[0]
}

/** Converts a YYYY-MM-DD date input value back to an ISO 8601 string */
export const fromInputDateValue = (dateStr: string): string => {
  if (!dateStr) return ''
  return new Date(dateStr + 'T00:00:00.000Z').toISOString()
}
