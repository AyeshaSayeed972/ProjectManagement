import React, { useState } from 'react'
import { useForm } from 'react-hook-form'
import toast from 'react-hot-toast'
import { Input } from '@/components/ui/Input'
import { Button } from '@/components/ui/Button'
import { upsertJiraSettings, testJiraConnection } from '@/api/jiraApi'
import type { JiraSettingsResponse, UpsertJiraSettingsDto } from '@/types'

interface JiraSettingsFormFields {
  baseUrl: string
  email: string
  apiToken: string
}

interface JiraSettingsFormProps {
  current: JiraSettingsResponse | null
  onSaved: (updated: JiraSettingsResponse) => void
}

export const JiraSettingsForm: React.FC<JiraSettingsFormProps> = ({ current, onSaved }) => {
  const [testing, setTesting] = useState(false)
  const [saving, setSaving] = useState(false)

  const {
    register,
    handleSubmit,
    getValues,
    formState: { errors },
  } = useForm<JiraSettingsFormFields>({
    defaultValues: {
      baseUrl: current?.baseUrl ?? '',
      email: current?.email ?? '',
      apiToken: '',
    },
  })

  const handleTest = async () => {
    const { baseUrl, email, apiToken } = getValues()
    if (!baseUrl || !email || !apiToken) {
      toast.error('Fill in all fields before testing.')
      return
    }
    setTesting(true)
    try {
      // Save first so the server can use the credentials
      await upsertJiraSettings({ baseUrl, email, apiToken })
      const { connectedAs } = await testJiraConnection()
      toast.success(`Connected as ${connectedAs}`)
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message
      toast.error(msg ?? 'Connection failed. Check your credentials.')
    } finally {
      setTesting(false)
    }
  }

  const onSubmit = async (data: JiraSettingsFormFields) => {
    setSaving(true)
    try {
      const dto: UpsertJiraSettingsDto = {
        baseUrl: data.baseUrl,
        email: data.email,
        apiToken: data.apiToken,
      }
      const updated = await upsertJiraSettings(dto)
      onSaved(updated)
      toast.success('Jira settings saved.')
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message
      toast.error(msg ?? 'Failed to save settings.')
    } finally {
      setSaving(false)
    }
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      <Input
        id="baseUrl"
        label="Jira Base URL"
        placeholder="https://yourorg.atlassian.net"
        error={errors.baseUrl?.message}
        {...register('baseUrl', {
          required: 'Base URL is required',
          validate: (v) =>
            /^https?:\/\/.+/.test(v) || 'Must be a valid URL starting with https://',
        })}
      />
      <Input
        id="email"
        label="Atlassian Account Email"
        placeholder="you@example.com"
        error={errors.email?.message}
        {...register('email', {
          required: 'Email is required',
          pattern: { value: /^[^\s@]+@[^\s@]+\.[^\s@]+$/, message: 'Invalid email address' },
        })}
      />
      {/* ── SECRET ── Enter the Jira API token generated at https://id.atlassian.com/manage-profile/security/api-tokens */}
      <Input
        id="apiToken"
        label="API Token"
        type="password"
        placeholder="Paste your Jira API token"
        error={errors.apiToken?.message}
        {...register('apiToken', { required: 'API token is required' })}
      />

      <div className="flex gap-3 pt-2">
        <Button
          type="button"
          variant="secondary"
          className="flex-1"
          onClick={handleTest}
          loading={testing}
          disabled={saving}
        >
          Test Connection
        </Button>
        <Button
          type="submit"
          variant="primary"
          className="flex-1"
          loading={saving}
          disabled={testing}
        >
          Save Settings
        </Button>
      </div>
    </form>
  )
}
