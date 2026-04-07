import React from 'react'
import { ExternalLink, Link2, Link2Off, Plus } from 'lucide-react'
import type { TaskResponse } from '@/types'

interface JiraIssuePanelProps {
  task: TaskResponse
  isPM: boolean
  jiraBaseUrl: string
  onLink: (task: TaskResponse) => void
  onUnlink: (task: TaskResponse) => void
  onCreateIssue: (task: TaskResponse) => void
}

export const JiraIssuePanel: React.FC<JiraIssuePanelProps> = ({
  task,
  isPM,
  jiraBaseUrl,
  onLink,
  onUnlink,
  onCreateIssue,
}) => {
  if (task.jiraIssueKey) {
    const browseUrl = `${jiraBaseUrl}/browse/${task.jiraIssueKey}`
    return (
      <div className="flex items-center gap-1.5 flex-wrap">
        <a
          href={browseUrl}
          target="_blank"
          rel="noopener noreferrer"
          className="inline-flex items-center gap-1 text-xs font-medium text-blue-600 hover:text-blue-800 bg-blue-50 hover:bg-blue-100 border border-blue-200 px-2 py-0.5 rounded transition-colors"
        >
          {task.jiraIssueKey}
          <ExternalLink className="w-3 h-3" />
        </a>
        {isPM && (
          <button
            onClick={() => onUnlink(task)}
            title="Unlink Jira issue"
            className="p-0.5 text-gray-400 hover:text-red-500 transition-colors rounded"
          >
            <Link2Off className="w-3.5 h-3.5" />
          </button>
        )}
      </div>
    )
  }

  if (!isPM) return null

  return (
    <div className="flex items-center gap-1.5">
      <button
        onClick={() => onLink(task)}
        title="Link existing Jira issue"
        className="inline-flex items-center gap-1 text-xs text-gray-500 hover:text-blue-600 transition-colors"
      >
        <Link2 className="w-3.5 h-3.5" />
        Link
      </button>
      <span className="text-gray-300">|</span>
      <button
        onClick={() => onCreateIssue(task)}
        title="Create new Jira issue"
        className="inline-flex items-center gap-1 text-xs text-gray-500 hover:text-blue-600 transition-colors"
      >
        <Plus className="w-3.5 h-3.5" />
        Create
      </button>
    </div>
  )
}
