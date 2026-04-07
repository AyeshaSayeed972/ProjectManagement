import React, { useState } from 'react'
import { useForm } from 'react-hook-form'
import toast from 'react-hot-toast'
import { Search } from 'lucide-react'
import { Modal } from '@/components/ui/Modal'
import { Button } from '@/components/ui/Button'
import { Input } from '@/components/ui/Input'
import { Table, Column } from '@/components/ui/Table'
import { searchJiraIssues, importFromJira } from '@/api/jiraApi'
import type { JiraIssueResponse, UserResponse, JiraImportDto } from '@/types'

interface ImportFormFields {
  assignedToUserId: string
  assignedToQAUserId: string
}

interface JiraImportModalProps {
  isOpen: boolean
  onClose: () => void
  releaseId: number
  developers: UserResponse[]
  qaUsers: UserResponse[]
  onSuccess: () => void
}

export const JiraImportModal: React.FC<JiraImportModalProps> = ({
  isOpen,
  onClose,
  releaseId,
  developers,
  qaUsers,
  onSuccess,
}) => {
  const [jql, setJql] = useState('created >= -30d ORDER BY created DESC')
  const [searching, setSearching] = useState(false)
  const [results, setResults] = useState<JiraIssueResponse[]>([])
  const [selected, setSelected] = useState<JiraIssueResponse | null>(null)
  const [importing, setImporting] = useState(false)

  const { register, handleSubmit, formState: { errors } } = useForm<ImportFormFields>()

  const handleSearch = async () => {
    setSearching(true)
    setSelected(null)
    try {
      const data = await searchJiraIssues(jql, 0, 20)
      setResults(data.issues)
      if (data.issues.length === 0) toast('No issues found for that JQL query.')
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message
      toast.error(msg ?? 'Search failed. Check your Jira settings.')
    } finally {
      setSearching(false)
    }
  }

  const onImport = async (fields: ImportFormFields) => {
    if (!selected) return
    setImporting(true)
    try {
      const dto: JiraImportDto = {
        jiraIssueKey: selected.key,
        releaseId,
        assignedToUserId: Number(fields.assignedToUserId),
        assignedToQAUserId: Number(fields.assignedToQAUserId),
      }
      await importFromJira(dto)
      toast.success(`Imported ${selected.key} as a new task.`)
      onSuccess()
      onClose()
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message
      toast.error(msg ?? 'Import failed.')
    } finally {
      setImporting(false)
    }
  }

  const columns: Column<JiraIssueResponse>[] = [
    {
      key: 'key',
      header: 'Key',
      render: (row) => (
        <span className="font-mono text-xs text-blue-700 font-semibold">{row.key}</span>
      ),
    },
    { key: 'summary', header: 'Summary' },
    { key: 'status', header: 'Status' },
    {
      key: 'assignee',
      header: 'Assignee',
      render: (row) => <span>{row.assignee ?? '—'}</span>,
    },
  ]

  return (
    <Modal isOpen={isOpen} onClose={onClose} title="Import from Jira" size="lg">
      {/* JQL search */}
      <div className="flex gap-2 mb-4">
        <input
          value={jql}
          onChange={(e) => setJql(e.target.value)}
          placeholder='e.g. project = PROJ created >= -30d ORDER BY created DESC'
          className="flex-1 rounded-lg border border-gray-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          onKeyDown={(e) => e.key === 'Enter' && handleSearch()}
        />
        <Button
          type="button"
          variant="secondary"
          onClick={handleSearch}
          loading={searching}
        >
          <Search className="w-4 h-4" />
          Search
        </Button>
      </div>

      {/* Results table */}
      {results.length > 0 && (
        <div className="mb-4">
          <p className="text-xs text-gray-500 mb-2">Click a row to select it for import.</p>
          <div className="overflow-x-auto rounded-xl border border-gray-200 bg-white shadow-sm">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  {columns.map((col) => (
                    <th
                      key={col.key}
                      className="px-4 py-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider"
                    >
                      {col.header}
                    </th>
                  ))}
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100">
                {results.map((row) => (
                  <tr
                    key={row.key}
                    onClick={() => setSelected(row)}
                    className={`cursor-pointer transition-colors ${
                      selected?.key === row.key
                        ? 'bg-blue-50 border-blue-200'
                        : 'hover:bg-gray-50'
                    }`}
                  >
                    {columns.map((col) => (
                      <td key={col.key} className="px-4 py-3 text-sm text-gray-700">
                        {col.render ? col.render(row, 0) : String((row as Record<string, unknown>)[col.key] ?? '')}
                      </td>
                    ))}
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* Assignment form — shown only when a row is selected */}
      {selected && (
        <form onSubmit={handleSubmit(onImport)} className="border-t border-gray-200 pt-4 space-y-4">
          <p className="text-sm font-medium text-gray-700">
            Assign <span className="font-mono text-blue-700">{selected.key}</span>: {selected.summary}
          </p>

          <div className="flex flex-col gap-1">
            <label className="text-sm font-medium text-gray-700">Developer</label>
            <select
              className="block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
              {...register('assignedToUserId', { required: 'Developer is required' })}
            >
              <option value="">Select developer…</option>
              {developers.map((u) => (
                <option key={u.id} value={u.id}>{u.username}</option>
              ))}
            </select>
            {errors.assignedToUserId && (
              <p className="text-xs text-red-600">{errors.assignedToUserId.message}</p>
            )}
          </div>

          <div className="flex flex-col gap-1">
            <label className="text-sm font-medium text-gray-700">QA</label>
            <select
              className="block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
              {...register('assignedToQAUserId', { required: 'QA is required' })}
            >
              <option value="">Select QA…</option>
              {qaUsers.map((u) => (
                <option key={u.id} value={u.id}>{u.username}</option>
              ))}
            </select>
            {errors.assignedToQAUserId && (
              <p className="text-xs text-red-600">{errors.assignedToQAUserId.message}</p>
            )}
          </div>

          <div className="flex gap-3 pt-2">
            <Button type="button" variant="secondary" className="flex-1" onClick={onClose}>
              Cancel
            </Button>
            <Button type="submit" variant="primary" className="flex-1" loading={importing}>
              Import as Task
            </Button>
          </div>
        </form>
      )}
    </Modal>
  )
}
