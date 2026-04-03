import React from 'react'
import { useForm } from 'react-hook-form'
import { Input } from '@/components/ui/Input'
import { Button } from '@/components/ui/Button'
import type { UserResponse, TaskResponse } from '@/types'

interface TaskFormFields {
  title: string
  assignedToUserId: number
  assignedToQAUserId: number
}

interface TaskFormProps {
  onSubmit: (data: TaskFormFields) => Promise<void>
  defaultValues?: Partial<TaskResponse>
  isLoading?: boolean
  onCancel: () => void
  users: UserResponse[]
  isEdit?: boolean
}

export const TaskForm: React.FC<TaskFormProps> = ({
  onSubmit,
  defaultValues,
  isLoading = false,
  onCancel,
  users,
  isEdit = false,
}) => {
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<TaskFormFields>({
    defaultValues: {
      title: defaultValues?.title ?? '',
      assignedToUserId: defaultValues?.assignedToUserId ?? undefined,
      assignedToQAUserId: defaultValues?.assignedToQAUserId ?? undefined,
    },
  })

  const devUsers = users.filter((u) => u.role === 'Developer')
  const qaUsers  = users.filter((u) => u.role === 'QA')

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      <Input
        id="title"
        label="Title"
        placeholder="Task title"
        error={errors.title?.message}
        {...register('title', { required: 'Title is required', maxLength: { value: 200, message: 'Max 200 characters' } })}
      />

      <div className="flex flex-col gap-1">
        <label htmlFor="assignedToUserId" className="text-sm font-medium text-gray-700">
          Assign Developer
        </label>
        <select
          id="assignedToUserId"
          className={`block w-full rounded-lg border px-3 py-2 text-sm text-gray-900 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 ${
            errors.assignedToUserId ? 'border-red-400' : 'border-gray-300'
          }`}
          {...register('assignedToUserId', {
            required: 'Please assign a developer',
            valueAsNumber: true,
          })}
        >
          <option value="">Select a developer</option>
          {devUsers.map((u) => (
            <option key={u.id} value={u.id}>
              {u.username}
            </option>
          ))}
        </select>
        {errors.assignedToUserId && (
          <p className="text-xs text-red-600">{errors.assignedToUserId.message}</p>
        )}
      </div>

      <div className="flex flex-col gap-1">
        <label htmlFor="assignedToQAUserId" className="text-sm font-medium text-gray-700">
          Assign QA
        </label>
        <select
          id="assignedToQAUserId"
          className={`block w-full rounded-lg border px-3 py-2 text-sm text-gray-900 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 ${
            errors.assignedToQAUserId ? 'border-red-400' : 'border-gray-300'
          }`}
          {...register('assignedToQAUserId', {
            required: 'Please assign a QA user',
            valueAsNumber: true,
          })}
        >
          <option value="">Select a QA user</option>
          {qaUsers.map((u) => (
            <option key={u.id} value={u.id}>
              {u.username}
            </option>
          ))}
        </select>
        {errors.assignedToQAUserId && (
          <p className="text-xs text-red-600">{errors.assignedToQAUserId.message}</p>
        )}
      </div>

      <div className="flex gap-3 pt-2">
        <Button type="button" variant="secondary" className="flex-1" onClick={onCancel} disabled={isLoading}>
          Cancel
        </Button>
        <Button type="submit" variant="primary" className="flex-1" loading={isLoading}>
          {isEdit ? 'Save Changes' : 'Create Task'}
        </Button>
      </div>
    </form>
  )
}
