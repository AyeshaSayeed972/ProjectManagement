import type { TaskStatus, ReleaseStatus } from '../types'

// ── Task status ───────────────────────────────────────────────────────────────

export const TASK_STATUS_LABELS: Record<TaskStatus, string> = {
  Pending: 'Pending',
  InProgress: 'In Progress',
  PRRaised: 'PR Raised',
  Merged: 'Merged',
  QAApproved: 'QA Approved',
  Deployed: 'Deployed',
  Done: 'Done',
}

export const TASK_STATUS_COLORS: Record<TaskStatus, string> = {
  Pending: 'bg-gray-100 text-gray-700',
  InProgress: 'bg-blue-100 text-blue-700',
  PRRaised: 'bg-purple-100 text-purple-700',
  Merged: 'bg-indigo-100 text-indigo-700',
  QAApproved: 'bg-teal-100 text-teal-700',
  Deployed: 'bg-orange-100 text-orange-700',
  Done: 'bg-green-100 text-green-700',
}

// Mirrors TaskStatusTransitionService.cs — strictly one step at a time
export const NEXT_TASK_STATUS: Record<TaskStatus, TaskStatus | null> = {
  Pending: 'InProgress',
  InProgress: 'PRRaised',
  PRRaised: 'Merged',
  Merged: 'QAApproved',
  QAApproved: 'Deployed',
  Deployed: 'Done',
  Done: null,
}

export const getNextTaskStatus = (current: TaskStatus): TaskStatus | null =>
  NEXT_TASK_STATUS[current]

export const TASK_STATUS_OPTIONS: TaskStatus[] = [
  'Pending',
  'InProgress',
  'PRRaised',
  'Merged',
  'QAApproved',
  'Deployed',
  'Done',
]

// ── Release status ────────────────────────────────────────────────────────────

export const RELEASE_STATUS_LABELS: Record<ReleaseStatus, string> = {
  Upcoming: 'Upcoming',
  Active: 'Active',
  Shipped: 'Shipped',
  Cancelled: 'Cancelled',
}

export const RELEASE_STATUS_COLORS: Record<ReleaseStatus, string> = {
  Upcoming: 'bg-blue-100 text-blue-700',
  Active: 'bg-green-100 text-green-700',
  Shipped: 'bg-purple-100 text-purple-700',
  Cancelled: 'bg-red-100 text-red-700',
}

export const RELEASE_STATUS_OPTIONS: ReleaseStatus[] = [
  'Upcoming',
  'Active',
  'Shipped',
  'Cancelled',
]
