import React from 'react'
import { useForm } from 'react-hook-form'
import { Input } from '@/components/ui/Input'
import { Button } from '@/components/ui/Button'
import type { TaskResponse } from '@/types'

interface DevFieldsFormFields {
  prLink: string
  remarks: string
}

interface DevFieldsFormProps {
  onSubmit: (data: DevFieldsFormFields) => Promise<void>
  defaultValues?: Partial<TaskResponse>
  isLoading?: boolean
  onCancel: () => void
}

export const DevFieldsForm: React.FC<DevFieldsFormProps> = ({
  onSubmit,
  defaultValues,
  isLoading = false,
  onCancel,
}) => {
  const { register, handleSubmit } = useForm<DevFieldsFormFields>({
    defaultValues: {
      prLink: defaultValues?.prLink ?? '',
      remarks: defaultValues?.remarks ?? '',
    },
  })

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      <Input
        id="prLink"
        label="PR Link"
        placeholder="https://github.com/..."
        type="url"
        {...register('prLink')}
      />

      <div className="flex flex-col gap-1">
        <label htmlFor="remarks" className="text-sm font-medium text-gray-700">
          Remarks
        </label>
        <textarea
          id="remarks"
          rows={3}
          placeholder="Any notes or remarks..."
          className="block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm text-gray-900 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-colors resize-none"
          {...register('remarks')}
        />
      </div>

      <div className="flex gap-3 pt-2">
        <Button type="button" variant="secondary" className="flex-1" onClick={onCancel} disabled={isLoading}>
          Cancel
        </Button>
        <Button type="submit" variant="primary" className="flex-1" loading={isLoading}>
          Save
        </Button>
      </div>
    </form>
  )
}
