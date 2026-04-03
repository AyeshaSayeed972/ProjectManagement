import { useState, useCallback } from 'react'

export const usePagination = (defaultPageSize = 10) => {
  const [pageNumber, setPageNumber] = useState(1)
  const [pageSize, setPageSizeState] = useState(defaultPageSize)

  const goToPage = useCallback((page: number) => setPageNumber(page), [])
  const nextPage = useCallback(() => setPageNumber((p) => p + 1), [])
  const prevPage = useCallback(() => setPageNumber((p) => Math.max(1, p - 1)), [])
  const reset = useCallback(() => setPageNumber(1), [])

  const setPageSize = useCallback((size: number) => {
    setPageSizeState(size)
    setPageNumber(1)
  }, [])

  return { pageNumber, pageSize, goToPage, nextPage, prevPage, reset, setPageSize }
}
