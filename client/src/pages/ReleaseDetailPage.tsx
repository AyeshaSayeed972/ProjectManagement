import React from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { ArrowLeft, Plus, Pencil, Trash2, ExternalLink, ChevronRight, Download } from 'lucide-react'
import { Layout } from '@/components/layout/Layout'
import { Badge } from '@/components/ui/Badge'
import { Button } from '@/components/ui/Button'
import { Pagination } from '@/components/ui/Pagination'
import { Table, Column } from '@/components/ui/Table'
import { ReleaseDetailModals } from '@/components/release/ReleaseDetailModals'
import { JiraIssuePanel } from '@/components/jira/JiraIssuePanel'
import { useReleaseDetail } from '@/hooks/useReleaseDetail'
import { useAuth } from '@/hooks/useAuth'
import { formatDate, formatDateRange } from '@/utils/dateHelpers'
import { getNextTaskStatus, TASK_STATUS_LABELS } from '@/utils/statusHelpers'
import type { TaskResponse } from '@/types'

export const ReleaseDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>()
  const releaseId = Number(id)
  const navigate = useNavigate()
  const { user } = useAuth()
  const isPM = user?.role === 'PM'
  const isDev = user?.role === 'Developer'
  const isQA = user?.role === 'QA'

  const {
    release, releaseLoading,
    tasksResult, tasksLoading,
    users, error,
    actionLoading, actionError,
    goToPage, setPageSize,
    editReleaseOpen, setEditReleaseOpen,
    deleteReleaseOpen, setDeleteReleaseOpen,
    addTaskOpen, setAddTaskOpen,
    editTask, setEditTask,
    deleteTaskTarget, setDeleteTaskTarget,
    devFieldsTask, setDevFieldsTask,
    qaFieldsTask, setQAFieldsTask,
    jiraBaseUrl,
    linkJiraTask, setLinkJiraTask,
    createJiraIssueTask, setCreateJiraIssueTask,
    jiraImportOpen, setJiraImportOpen,
    loadUsers,
    handleEditRelease, handleDeleteRelease,
    handleAddTask, handleEditTask, handleDeleteTask,
    handleAdvanceStatus, handleDevFields, handleQAFields,
    handleLinkJira, handleUnlinkJira, handleCreateJiraIssue,
  } = useReleaseDetail(releaseId)

  const taskColumns: Column<TaskResponse>[] = [
    {
      key: 'title',
      header: 'Title',
      render: (row) => <span className="block font-medium text-gray-900 max-w-xs break-words">{row.title}</span>,
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
      key: 'prLink',
      header: 'PR Link',
      render: (row) =>
        row.prLink ? (
          <a href={row.prLink} target="_blank" rel="noreferrer" className="flex items-center gap-1 text-blue-600 hover:text-blue-800 text-xs">
            <ExternalLink className="w-3 h-3" />
            View PR
          </a>
        ) : (
          <span className="text-gray-400">—</span>
        ),
    },
    {
      key: 'jiraIssueKey',
      header: 'Jira',
      render: (row) => (
        <JiraIssuePanel
          task={row}
          isPM={isPM}
          jiraBaseUrl={jiraBaseUrl}
          onLink={(t) => setLinkJiraTask(t)}
          onUnlink={handleUnlinkJira}
          onCreateIssue={(t) => setCreateJiraIssueTask(t)}
        />
      ),
    },
    {
      key: 'assignedToQAUsername',
      header: 'QA Assignee',
      render: (row) => (
        <div className="flex items-center gap-2">
          {row.assignedToQAUsername ? (
            <>
              <div className="w-6 h-6 rounded-full bg-teal-100 flex items-center justify-center text-xs font-semibold text-teal-700">
                {row.assignedToQAUsername.charAt(0).toUpperCase()}
              </div>
              <span>{row.assignedToQAUsername}</span>
            </>
          ) : (
            <span className="text-gray-400">—</span>
          )}
        </div>
      ),
    },
    {
      key: 'actions',
      header: 'Actions',
      render: (row) => {
        const isDevAssigned = row.assignedToUsername === user?.username
        const isQAAssigned  = row.assignedToQAUsername === user?.username
        const next = getNextTaskStatus(row.status)
        return (
          <div className="flex items-center gap-1.5 flex-wrap">
            {isPM && (
              <>
                <button
                  onClick={async () => { await loadUsers(); setEditTask(row) }}
                  className="p-1.5 rounded-lg text-gray-400 hover:text-blue-600 hover:bg-blue-50 transition-colors"
                  title="Edit"
                >
                  <Pencil className="w-4 h-4" />
                </button>
                <button
                  onClick={() => setDeleteTaskTarget(row)}
                  className="p-1.5 rounded-lg text-gray-400 hover:text-red-600 hover:bg-red-50 transition-colors"
                  title="Delete"
                >
                  <Trash2 className="w-4 h-4" />
                </button>
              </>
            )}
            {next && ((['QAApproved', 'Deployed', 'Done'] as string[]).includes(next) ? (isQA && isQAAssigned) : (isDev && isDevAssigned)) && (
              <button
                onClick={() => handleAdvanceStatus(row)}
                className="flex items-center gap-1 px-2 py-1 rounded-lg bg-blue-50 text-blue-700 hover:bg-blue-100 text-xs font-medium transition-colors"
                title={`Move to ${TASK_STATUS_LABELS[next]}`}
              >
                <ChevronRight className="w-3 h-3" />
                {TASK_STATUS_LABELS[next]}
              </button>
            )}
            {isDev && isDevAssigned && (
              <button
                onClick={() => setDevFieldsTask(row)}
                className="px-2 py-1 rounded-lg bg-purple-50 text-purple-700 hover:bg-purple-100 text-xs font-medium transition-colors"
              >
                Dev Fields
              </button>
            )}
            {isQA && row.assignedToQAUsername === user?.username && (
              <button
                onClick={() => setQAFieldsTask(row)}
                className="px-2 py-1 rounded-lg bg-green-50 text-green-700 hover:bg-green-100 text-xs font-medium transition-colors"
              >
                QA Fields
              </button>
            )}
          </div>
        )
      },
    },
  ]

  if (releaseLoading) {
    return (
      <Layout title="Release Detail">
        <div className="animate-pulse space-y-4">
          <div className="h-32 bg-gray-200 rounded-xl" />
          <div className="h-64 bg-gray-200 rounded-xl" />
        </div>
      </Layout>
    )
  }

  if (!release) {
    return (
      <Layout title="Release Detail">
        <div className="text-center text-gray-500 py-20">Release not found.</div>
      </Layout>
    )
  }

  return (
    <Layout title={release.title}>
      {error && (
        <div className="mb-4 px-4 py-3 rounded-lg bg-red-50 border border-red-200 text-red-700 text-sm">{error}</div>
      )}

      {/* Release header card */}
      <div className="bg-white rounded-xl border border-gray-200 p-6 mb-6 shadow-sm">
        <div className="flex items-start justify-between gap-4">
          <div className="flex items-center gap-3">
            <button
              onClick={() => navigate('/releases')}
              className="p-1.5 rounded-lg text-gray-400 hover:text-gray-600 hover:bg-gray-100 transition-colors"
            >
              <ArrowLeft className="w-5 h-5" />
            </button>
            <div>
              <div className="flex items-center gap-3 mb-1">
                <h2 className="text-xl font-bold text-gray-900">{release.title}</h2>
                <Badge type="release" status={release.status} />
              </div>
              {release.description && (
                <p className="text-sm text-gray-500 mb-2">{release.description}</p>
              )}
              <p className="text-sm text-gray-400">
                {formatDateRange(release.startDate, release.endDate)}
                {' · '}Created {formatDate(release.createdAt)}
              </p>
            </div>
          </div>
          {isPM && (
            <div className="flex items-center gap-2 shrink-0">
              <Button variant="secondary" size="sm" onClick={() => setEditReleaseOpen(true)}>
                <Pencil className="w-4 h-4" />
                Edit
              </Button>
              <Button variant="danger" size="sm" onClick={() => setDeleteReleaseOpen(true)}>
                <Trash2 className="w-4 h-4" />
                Delete
              </Button>
            </div>
          )}
        </div>
      </div>

      {/* Tasks header */}
      <div className="flex items-center justify-between mb-4">
        <h3 className="text-base font-semibold text-gray-900">
          Tasks{tasksResult ? ` (${tasksResult.totalCount})` : ''}
        </h3>
        {isPM && (
          <div className="flex items-center gap-2">
            <Button
              variant="secondary"
              size="sm"
              onClick={() => { loadUsers(); setJiraImportOpen(true) }}
            >
              <Download className="w-4 h-4" />
              Import from Jira
            </Button>
            <Button
              variant="primary"
              size="sm"
              onClick={() => { loadUsers(); setAddTaskOpen(true) }}
            >
              <Plus className="w-4 h-4" />
              Add Task
            </Button>
          </div>
        )}
      </div>

      <Table
        columns={taskColumns}
        data={tasksResult?.data ?? []}
        isLoading={tasksLoading}
        emptyMessage="No tasks in this release."
        keyExtractor={(row) => row.id}
      />

      {tasksResult && tasksResult.totalPages > 1 && (
        <div className="mt-4">
          <Pagination
            pageNumber={tasksResult.pageNumber}
            pageSize={tasksResult.pageSize}
            totalCount={tasksResult.totalCount}
            totalPages={tasksResult.totalPages}
            hasNextPage={tasksResult.hasNextPage}
            hasPreviousPage={tasksResult.hasPreviousPage}
            onPageChange={goToPage}
            onPageSizeChange={setPageSize}
          />
        </div>
      )}

      <ReleaseDetailModals
        release={release}
        users={users}
        actionLoading={actionLoading}
        actionError={actionError}
        jiraBaseUrl={jiraBaseUrl}
        editReleaseOpen={editReleaseOpen}
        deleteReleaseOpen={deleteReleaseOpen}
        addTaskOpen={addTaskOpen}
        editTask={editTask}
        deleteTaskTarget={deleteTaskTarget}
        devFieldsTask={devFieldsTask}
        qaFieldsTask={qaFieldsTask}
        linkJiraTask={linkJiraTask}
        createJiraIssueTask={createJiraIssueTask}
        jiraImportOpen={jiraImportOpen}
        onCloseEditRelease={() => setEditReleaseOpen(false)}
        onCloseDeleteRelease={() => setDeleteReleaseOpen(false)}
        onCloseAddTask={() => setAddTaskOpen(false)}
        onCloseEditTask={() => setEditTask(null)}
        onCloseDeleteTask={() => setDeleteTaskTarget(null)}
        onCloseDevFields={() => setDevFieldsTask(null)}
        onCloseQAFields={() => setQAFieldsTask(null)}
        onCloseLinkJira={() => setLinkJiraTask(null)}
        onCloseCreateJiraIssue={() => setCreateJiraIssueTask(null)}
        onCloseJiraImport={() => setJiraImportOpen(false)}
        onEditRelease={handleEditRelease}
        onDeleteRelease={handleDeleteRelease}
        onAddTask={handleAddTask}
        onEditTask={handleEditTask}
        onDeleteTask={handleDeleteTask}
        onDevFields={handleDevFields}
        onQAFields={handleQAFields}
        onLinkJira={handleLinkJira}
        onCreateJiraIssue={handleCreateJiraIssue}
        onJiraImportSuccess={() => { loadUsers() }}
      />
    </Layout>
  )
}
