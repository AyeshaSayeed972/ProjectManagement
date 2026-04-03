import React from 'react'
import { TASK_STATUS_COLORS, TASK_STATUS_LABELS, RELEASE_STATUS_COLORS, RELEASE_STATUS_LABELS } from '@/utils/statusHelpers'
import type { TaskStatus, ReleaseStatus } from '@/types'

interface TaskBadgeProps {
  type: 'task'
  status: TaskStatus
}

interface ReleaseBadgeProps {
  type: 'release'
  status: ReleaseStatus
}

type BadgeProps = TaskBadgeProps | ReleaseBadgeProps

export const Badge: React.FC<BadgeProps> = (props) => {
  const colorClass =
    props.type === 'task'
      ? TASK_STATUS_COLORS[props.status]
      : RELEASE_STATUS_COLORS[props.status]

  const label =
    props.type === 'task'
      ? TASK_STATUS_LABELS[props.status]
      : RELEASE_STATUS_LABELS[props.status]

  return (
    <span className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ${colorClass}`}>
      {label}
    </span>
  )
}
