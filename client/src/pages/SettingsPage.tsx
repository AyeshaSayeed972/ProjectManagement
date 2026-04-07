import React, { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { Layout } from '@/components/layout/Layout'
import { JiraSettingsForm } from '@/components/forms/JiraSettingsForm'
import { getJiraSettings } from '@/api/jiraApi'
import { useAuth } from '@/hooks/useAuth'
import type { JiraSettingsResponse } from '@/types'

export const SettingsPage: React.FC = () => {
  const { user } = useAuth()
  const navigate = useNavigate()
  const [settings, setSettings] = useState<JiraSettingsResponse | null>(null)
  const [isLoading, setIsLoading] = useState(true)

  useEffect(() => {
    if (user?.role !== 'PM') {
      navigate('/dashboard')
      return
    }
    getJiraSettings()
      .then(setSettings)
      .catch(() => setSettings(null))
      .finally(() => setIsLoading(false))
  }, [user, navigate])

  return (
    <Layout>
      <div className="max-w-2xl">
        <h1 className="text-2xl font-bold text-gray-900 mb-6">Settings</h1>

        <div className="bg-white rounded-xl border border-gray-200 p-6 shadow-sm">
          <div className="flex items-start justify-between mb-6">
            <div>
              <h2 className="text-lg font-semibold text-gray-900">Jira Integration</h2>
              <p className="text-sm text-gray-500 mt-1">
                Connect to your Jira Cloud workspace. You need a Jira API token — generate one at{' '}
                <span className="font-mono text-xs bg-gray-100 px-1 py-0.5 rounded">
                  id.atlassian.com/manage-profile/security/api-tokens
                </span>
              </p>
            </div>
            {!isLoading && (
              <span
                className={`shrink-0 ml-4 text-xs font-semibold px-2.5 py-1 rounded-full ${
                  settings?.isConfigured
                    ? 'bg-green-100 text-green-700'
                    : 'bg-yellow-100 text-yellow-700'
                }`}
              >
                {settings?.isConfigured ? 'Configured' : 'Not configured'}
              </span>
            )}
          </div>

          {isLoading ? (
            <div className="space-y-3">
              {[1, 2, 3].map((i) => (
                <div key={i} className="h-10 bg-gray-100 rounded-lg animate-pulse" />
              ))}
            </div>
          ) : (
            <JiraSettingsForm current={settings} onSaved={setSettings} />
          )}
        </div>
      </div>
    </Layout>
  )
}
