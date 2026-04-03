import React, { useEffect, useState, useCallback } from 'react'
import { useNavigate } from 'react-router-dom'
import { Plus, Pencil, Trash2 } from 'lucide-react'
import { Layout } from '@/components/layout/Layout'
import { Table, Column } from '@/components/ui/Table'
import { Badge } from '@/components/ui/Badge'
import { Button } from '@/components/ui/Button'
import { Pagination } from '@/components/ui/Pagination'
import { Modal } from '@/components/ui/Modal'
import { ConfirmDialog } from '@/components/ui/ConfirmDialog'
import { ReleaseForm } from '@/components/forms/ReleaseForm'
import { getReleases, createRelease, updateRelease, deleteRelease } from '@/api/releasesApi'
import { usePagination } from '@/hooks/usePagination'
import { useAuth } from '@/hooks/useAuth'
import { formatDateRange, fromInputDateValue } from '@/utils/dateHelpers'
import type { ReleaseResponse, PagedResult, ReleaseStatus } from '@/types'
import axios from 'axios'
import toast from 'react-hot-toast'

export const ReleasesPage: React.FC = () => {
  const { user } = useAuth()
  const navigate = useNavigate()
  const isPM = user?.role === 'PM'

  const { pageNumber, pageSize, goToPage, setPageSize } = usePagination(10)
  const [result, setResult] = useState<PagedResult<ReleaseResponse> | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  // Modals
  const [createOpen, setCreateOpen] = useState(false)
  const [editTarget, setEditTarget] = useState<ReleaseResponse | null>(null)
  const [deleteTarget, setDeleteTarget] = useState<ReleaseResponse | null>(null)
  const [actionLoading, setActionLoading] = useState(false)
  const [actionError, setActionError] = useState<string | null>(null)

  const fetchReleases = useCallback(async () => {
    setIsLoading(true)
    setError(null)
    try {
      const data = await getReleases(pageNumber, pageSize)
      setResult(data)
    } catch {
      setError('Failed to load releases.')
    } finally {
      setIsLoading(false)
    }
  }, [pageNumber, pageSize])

  useEffect(() => { fetchReleases() }, [fetchReleases])

  const handleCreate = async (data: { title: string; description: string; startDate: string; endDate: string; status: ReleaseStatus }) => {
    setActionLoading(true)
    setActionError(null)
    try {
      await createRelease({
        title: data.title,
        description: data.description || undefined,
        startDate: fromInputDateValue(data.startDate),
        endDate: fromInputDateValue(data.endDate),
      })
      setCreateOpen(false)
      fetchReleases()
      toast.success('Release created')
    } catch (err) {
      const msg = axios.isAxiosError(err) ? err.response?.data?.message ?? 'Failed to create.' : 'Failed to create.'
      setActionError(msg)
      toast.error(msg)
    } finally {
      setActionLoading(false)
    }
  }

  const handleEdit = async (data: { title: string; description: string; startDate: string; endDate: string; status: ReleaseStatus }) => {
    if (!editTarget) return
    setActionLoading(true)
    setActionError(null)
    try {
      await updateRelease(editTarget.id, {
        title: data.title,
        description: data.description || undefined,
        startDate: fromInputDateValue(data.startDate),
        endDate: fromInputDateValue(data.endDate),
        status: data.status,
      })
      setEditTarget(null)
      fetchReleases()
      toast.success('Release updated')
    } catch (err) {
      const msg = axios.isAxiosError(err) ? err.response?.data?.message ?? 'Failed to update.' : 'Failed to update.'
      setActionError(msg)
      toast.error(msg)
    } finally {
      setActionLoading(false)
    }
  }

  const handleDelete = async () => {
    if (!deleteTarget) return
    setActionLoading(true)
    try {
      await deleteRelease(deleteTarget.id)
      setDeleteTarget(null)
      fetchReleases()
      toast.success('Release deleted')
    } catch (err) {
      toast.error('Failed to delete release.')
    } finally {
      setActionLoading(false)
    }
  }

  const columns: Column<ReleaseResponse>[] = [
    {
      key: 'title',
      header: 'Title',
      render: (row) => (
        <button
          onClick={() => navigate(`/releases/${row.id}`)}
          className="text-blue-600 hover:text-blue-800 font-medium text-left"
        >
          {row.title}
        </button>
      ),
    },
    {
      key: 'description',
      header: 'Description',
      render: (row) => (
        <span className="text-gray-500 truncate max-w-[200px] block">
          {row.description || '—'}
        </span>
      ),
    },
    {
      key: 'dates',
      header: 'Dates',
      render: (row) => <span className="whitespace-nowrap">{formatDateRange(row.startDate, row.endDate)}</span>,
    },
    {
      key: 'tasks',
      header: 'Tasks',
      render: (row) => <span className="text-gray-600">{row.tasks.length}</span>,
    },
    {
      key: 'status',
      header: 'Status',
      render: (row) => <Badge type="release" status={row.status} />,
    },
    ...(isPM
      ? [
          {
            key: 'actions',
            header: 'Actions',
            render: (row: ReleaseResponse) => (
              <div className="flex items-center gap-2">
                <button
                  onClick={(e) => { e.stopPropagation(); setEditTarget(row) }}
                  className="p-1.5 rounded-lg text-gray-400 hover:text-blue-600 hover:bg-blue-50 transition-colors"
                  title="Edit"
                >
                  <Pencil className="w-4 h-4" />
                </button>
                <button
                  onClick={(e) => { e.stopPropagation(); setDeleteTarget(row) }}
                  className="p-1.5 rounded-lg text-gray-400 hover:text-red-600 hover:bg-red-50 transition-colors"
                  title="Delete"
                >
                  <Trash2 className="w-4 h-4" />
                </button>
              </div>
            ),
          } as Column<ReleaseResponse>,
        ]
      : []),
  ]

  return (
    <Layout title="Releases">
      <div className="flex items-center justify-between mb-6">
        <div>
          <p className="text-sm text-gray-500 mt-0.5">
            {result ? `${result.totalCount} release${result.totalCount !== 1 ? 's' : ''}` : ''}
          </p>
        </div>
        {isPM && (
          <Button variant="primary" onClick={() => setCreateOpen(true)}>
            <Plus className="w-4 h-4" />
            New Release
          </Button>
        )}
      </div>

      {error && (
        <div className="mb-4 px-4 py-3 rounded-lg bg-red-50 border border-red-200 text-red-700 text-sm">{error}</div>
      )}

      <Table
        columns={columns}
        data={result?.data ?? []}
        isLoading={isLoading}
        emptyMessage="No releases found."
        keyExtractor={(row) => row.id}
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

      {/* Create modal */}
      <Modal isOpen={createOpen} onClose={() => { setCreateOpen(false); setActionError(null) }} title="New Release">
        {actionError && (
          <div className="mb-4 px-4 py-3 rounded-lg bg-red-50 border border-red-200 text-red-700 text-sm">{actionError}</div>
        )}
        <ReleaseForm onSubmit={handleCreate} isLoading={actionLoading} onCancel={() => setCreateOpen(false)} />
      </Modal>

      {/* Edit modal */}
      <Modal isOpen={!!editTarget} onClose={() => { setEditTarget(null); setActionError(null) }} title="Edit Release">
        {actionError && (
          <div className="mb-4 px-4 py-3 rounded-lg bg-red-50 border border-red-200 text-red-700 text-sm">{actionError}</div>
        )}
        {editTarget && (
          <ReleaseForm
            onSubmit={handleEdit}
            defaultValues={editTarget}
            isLoading={actionLoading}
            onCancel={() => setEditTarget(null)}
            isEdit
          />
        )}
      </Modal>

      {/* Delete confirm */}
      <ConfirmDialog
        isOpen={!!deleteTarget}
        onClose={() => setDeleteTarget(null)}
        onConfirm={handleDelete}
        title="Delete Release"
        message={`Are you sure you want to delete "${deleteTarget?.title}"? All associated tasks will also be deleted.`}
        isLoading={actionLoading}
      />
    </Layout>
  )
}
