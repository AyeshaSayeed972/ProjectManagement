import React, { useEffect, useState, useCallback } from 'react'
import { Link } from 'react-router-dom'
import { Layout } from '@/components/layout/Layout'
import { Badge } from '@/components/ui/Badge'
import { Table, Column } from '@/components/ui/Table'
import { Pagination } from '@/components/ui/Pagination'
import { getAllTasks, getTasksByRelease } from '@/api/tasksApi'
import { getReleases } from '@/api/releasesApi'
import { usePagination } from '@/hooks/usePagination'
import { useAuth } from '@/hooks/useAuth'
import { TASK_STATUS_OPTIONS, TASK_STATUS_LABELS } from '@/utils/statusHelpers'
import { formatDate } from '@/utils/dateHelpers'
import type { TaskResponse, TaskStatus, PagedResult, ReleaseResponse } from '@/types'

export const TasksPage: React.FC = () => {
  const { user } = useAuth()
  const { pageNumber, pageSize, goToPage, setPageSize } = usePagination(10)

  const [result, setResult] = useState<PagedResult<TaskResponse> | null>(null)
  const [releases, setReleases] = useState<ReleaseResponse[]>([])
  const [isLoading, setIsLoading] = useState(true)

  // Filters
  const [statusFilter, setStatusFilter] = useState<TaskStatus | 'All'>('All')
  const [releaseFilter, setReleaseFilter] = useState<number | 'All'>('All')
  const [myTasksOnly, setMyTasksOnly] = useState(false)

  const fetchReleases = useCallback(async () => {
    try {
      const data = await getReleases(1, 50)
      setReleases(data.data)
    } catch (err) {
      console.error(err)
    }
  }, [])

  const fetchTasks = useCallback(async () => {
    setIsLoading(true)
    try {
      const status = statusFilter !== 'All' ? statusFilter : undefined
      const assignedTo = myTasksOnly && user?.username ? user.username : undefined
      const userRole = user?.role;

      let data: PagedResult<TaskResponse>
      if (releaseFilter !== 'All') {
        data = await getTasksByRelease(releaseFilter, pageNumber, pageSize, status, assignedTo, userRole)
      } else {
        data = await getAllTasks(pageNumber, pageSize, status, assignedTo, userRole)
      }
      setResult(data)
    } catch (err) {
      console.error(err)
    } finally {
      setIsLoading(false)
    }
  }, [pageNumber, pageSize, statusFilter, releaseFilter, myTasksOnly, user?.username, user?.role])

  useEffect(() => { fetchReleases() }, [fetchReleases])
  useEffect(() => { fetchTasks() }, [fetchTasks])

  const handleStatusChange = (value: TaskStatus | 'All') => {
    setStatusFilter(value)
    goToPage(1)
  }

  const handleReleaseChange = (value: number | 'All') => {
    setReleaseFilter(value)
    goToPage(1)
  }

  const handleMyTasksToggle = (value: boolean) => {
    setMyTasksOnly(value)
    goToPage(1)
  }

  const columns: Column<TaskResponse>[] = [
    {
      key: 'title',
      header: 'Task',
      render: (row) => (
        <Link
          to={`/releases/${row.releaseId}`}
          className="font-medium text-gray-900 hover:text-blue-600 transition-colors"
        >
          {row.title}
        </Link>
      ),
    },
    {
      key: 'releaseTitle',
      header: 'Release',
      render: (row) => (
        <Link to={`/releases/${row.releaseId}`} className="text-blue-600 hover:text-blue-800 text-sm">
          {row.releaseTitle}
        </Link>
      ),
    },
    {
      key: 'assignedToUsername',
      header: 'Assignee',
      render: (row) => (
        <div className="flex items-center gap-2">
          <div className="w-6 h-6 rounded-full bg-slate-200 flex items-center justify-center text-xs font-semibold text-slate-600">
            {row.assignedToUsername.charAt(0).toUpperCase()}
          </div>
          <span>{row.assignedToUsername}</span>
        </div>
      ),
    },
    {
      key: 'status',
      header: 'Status',
      render: (row) => <Badge type="task" status={row.status} />,
    },
    {
      key: 'createdAt',
      header: 'Release Date',
      render: (row) => <span className="text-gray-500 text-sm">{formatDate(row.createdAt)}</span>,
    },
  ]

  const isNotPM = user?.role !== 'PM'

  return (
    <Layout title="Tasks">
      <div className="flex flex-wrap items-center gap-3 mb-6">
        {isNotPM && (
          <div className="flex rounded-lg border border-gray-300 overflow-hidden">
            <button
              onClick={() => handleMyTasksToggle(false)}
              className={`px-4 py-2 text-sm font-medium transition-colors ${
                !myTasksOnly ? 'bg-blue-600 text-white' : 'bg-white text-gray-600 hover:bg-gray-50'
              }`}
            >
              All Tasks
            </button>
            <button
              onClick={() => handleMyTasksToggle(true)}
              className={`px-4 py-2 text-sm font-medium transition-colors ${
                myTasksOnly ? 'bg-blue-600 text-white' : 'bg-white text-gray-600 hover:bg-gray-50'
              }`}
            >
              My Tasks
            </button>
          </div>
        )}

        <select
          value={statusFilter}
          onChange={(e) => handleStatusChange(e.target.value as TaskStatus | 'All')}
          className="border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
        >
          <option value="All">All Statuses</option>
          {TASK_STATUS_OPTIONS.map((s) => (
            <option key={s} value={s}>{TASK_STATUS_LABELS[s]}</option>
          ))}
        </select>

        <select
          value={releaseFilter}
          onChange={(e) => handleReleaseChange(e.target.value === 'All' ? 'All' : Number(e.target.value))}
          className="border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
        >
          <option value="All">All Releases</option>
          {releases.map((r) => (
            <option key={r.id} value={r.id}>{r.title}</option>
          ))}
        </select>

        <span className="text-sm text-gray-500 ml-auto">
          {result ? `${result.totalCount} task${result.totalCount !== 1 ? 's' : ''}` : ''}
        </span>
      </div>

      <Table
        columns={columns}
        data={result?.data ?? []}
        isLoading={isLoading}
        emptyMessage="No tasks match your filters."
        keyExtractor={(row) => `${row.releaseId}-${row.id}`}
      />

      {result && result.totalPages > 1 && (
        <div className="mt-4">
          <Pagination
            pageNumber={result.pageNumber}
            pageSize={result.pageSize}
            totalCount={result.totalCount}
            totalPages={result.totalPages}
            hasNextPage={result.hasNextPage}
            hasPreviousPage={result.hasPreviousPage}
            onPageChange={goToPage}
            onPageSizeChange={setPageSize}
          />
        </div>
      )}
    </Layout>
  )
}
