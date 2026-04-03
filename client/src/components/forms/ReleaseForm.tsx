import React from 'react'
import { useForm } from 'react-hook-form'
import { Input } from '@/components/ui/Input'
import { Button } from '@/components/ui/Button'
import { RELEASE_STATUS_OPTIONS } from '@/utils/statusHelpers'
import { toInputDateValue } from '@/utils/dateHelpers'
import type { ReleaseResponse, ReleaseStatus } from '@/types'

interface ReleaseFormFields {
  title: string
  description: string
  startDate: string
  endDate: string
  status: ReleaseStatus
}

interface ReleaseFormProps {
  onSubmit: (data: ReleaseFormFields) => Promise<void>
  defaultValues?: Partial<ReleaseResponse>
  isLoading?: boolean
  onCancel: () => void
  isEdit?: boolean
}

export const ReleaseForm: React.FC<ReleaseFormProps> = ({
  onSubmit,
  defaultValues,
  isLoading = false,
  onCancel,
  isEdit = false,
}) => {
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<ReleaseFormFields>({
    defaultValues: {
      title: defaultValues?.title ?? '',
      description: defaultValues?.description ?? '',
      startDate: defaultValues?.startDate ? toInputDateValue(defaultValues.startDate) : '',
      endDate: defaultValues?.endDate ? toInputDateValue(defaultValues.endDate) : '',
      status: defaultValues?.status ?? 'Upcoming',
    },
  })

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      <Input
        id="title"
        label="Title"
        placeholder="Release title"
        error={errors.title?.message}
        {...register('title', { required: 'Title is required', maxLength: { value: 200, message: 'Max 200 characters' } })}
      />

      <div className="flex flex-col gap-1">
        <label htmlFor="description" className="text-sm font-medium text-gray-700">
          Description
        </label>
        <textarea
          id="description"
          rows={3}
          placeholder="Optional description"
          className="block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm text-gray-900 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-colors resize-none"
          {...register('description')}
        />
      </div>

      <div className="grid grid-cols-2 gap-4">
        <Input
          id="startDate"
          label="Start Date"
          type="date"
          error={errors.startDate?.message}
          {...register('startDate', { required: 'Start date is required' })}
        />
        <Input
          id="endDate"
          label="End Date"
          type="date"
          error={errors.endDate?.message}
          {...register('endDate', { required: 'End date is required' })}
        />
      </div>

      {isEdit && (
        <div className="flex flex-col gap-1">
          <label htmlFor="status" className="text-sm font-medium text-gray-700">
            Status
          </label>
          <select
            id="status"
            className="block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm text-gray-900 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
            {...register('status')}
          >
            {RELEASE_STATUS_OPTIONS.map((s) => (
              <option key={s} value={s}>{s}</option>
            ))}
          </select>
        </div>
      )}

      <div className="flex gap-3 pt-2">
        <Button type="button" variant="secondary" className="flex-1" onClick={onCancel} disabled={isLoading}>
          Cancel
        </Button>
        <Button type="submit" variant="primary" className="flex-1" loading={isLoading}>
          {isEdit ? 'Save Changes' : 'Create Release'}
        </Button>
      </div>
    </form>
  )
}
