import React from 'react'
import { ChevronLeft, ChevronRight } from 'lucide-react'

interface PaginationProps {
  pageNumber: number
  pageSize: number
  totalCount: number
  totalPages: number
  hasNextPage: boolean
  hasPreviousPage: boolean
  onPageChange: (page: number) => void
  onPageSizeChange?: (size: number) => void
}

export const Pagination: React.FC<PaginationProps> = ({
  pageNumber,
  pageSize,
  totalCount,
  totalPages,
  hasNextPage,
  hasPreviousPage,
  onPageChange,
  onPageSizeChange,
}) => {
  const from = Math.min((pageNumber - 1) * pageSize + 1, totalCount)
  const to = Math.min(pageNumber * pageSize, totalCount)

  return (
    <div className="flex items-center justify-between px-1 py-3 text-sm text-gray-600">
      <span>
        Showing <span className="font-medium">{from}</span>–<span className="font-medium">{to}</span> of{' '}
        <span className="font-medium">{totalCount}</span>
      </span>

      <div className="flex items-center gap-3">
        {onPageSizeChange && (
          <select
            value={pageSize}
            onChange={(e) => onPageSizeChange(Number(e.target.value))}
            className="border border-gray-300 rounded-lg px-2 py-1 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          >
            {[10, 25, 50].map((s) => (
              <option key={s} value={s}>
                {s} / page
              </option>
            ))}
          </select>
        )}

        <div className="flex items-center gap-1">
          <button
            onClick={() => onPageChange(pageNumber - 1)}
            disabled={!hasPreviousPage}
            className="p-1.5 rounded-lg border border-gray-300 hover:bg-gray-50 disabled:opacity-40 disabled:cursor-not-allowed transition-colors"
          >
            <ChevronLeft className="w-4 h-4" />
          </button>
          <span className="px-3 py-1 rounded-lg border border-gray-300 bg-white font-medium text-gray-900 min-w-[2.5rem] text-center">
            {pageNumber}
          </span>
          <span className="text-gray-400 text-xs">/ {totalPages}</span>
          <button
            onClick={() => onPageChange(pageNumber + 1)}
            disabled={!hasNextPage}
            className="p-1.5 rounded-lg border border-gray-300 hover:bg-gray-50 disabled:opacity-40 disabled:cursor-not-allowed transition-colors"
          >
            <ChevronRight className="w-4 h-4" />
          </button>
        </div>
      </div>
    </div>
  )
}
