import axiosInstance from '@/api/axios'
import type {
  JiraSettingsResponse,
  UpsertJiraSettingsDto,
  JiraIssueResponse,
  JiraSearchResult,
  JiraImportDto,
  CreateJiraIssueRequest,
  TaskResponse,
} from '@/types'

export const getJiraSettings = async (): Promise<JiraSettingsResponse> => {
  const res = await axiosInstance.get<JiraSettingsResponse>('/jira/settings')
  return res.data
}

export const upsertJiraSettings = async (dto: UpsertJiraSettingsDto): Promise<JiraSettingsResponse> => {
  const res = await axiosInstance.put<JiraSettingsResponse>('/jira/settings', dto)
  return res.data
}

export const testJiraConnection = async (): Promise<{ connectedAs: string }> => {
  const res = await axiosInstance.post<{ connectedAs: string }>('/jira/settings/test')
  return res.data
}

export const searchJiraIssues = async (
  jql = 'created >= -30d ORDER BY created DESC',
  startAt = 0,
  maxResults = 20,
): Promise<JiraSearchResult> => {
  const res = await axiosInstance.get<JiraSearchResult>('/jira/issues/search', {
    params: { jql, startAt, maxResults },
  })
  return res.data
}

export const getJiraIssue = async (issueKey: string): Promise<JiraIssueResponse> => {
  const res = await axiosInstance.get<JiraIssueResponse>(`/jira/issues/${issueKey}`)
  return res.data
}

export const importFromJira = async (dto: JiraImportDto): Promise<TaskResponse> => {
  const res = await axiosInstance.post<TaskResponse>('/jira/import', dto)
  return res.data
}

export const linkJiraIssue = async (taskId: number, issueKey: string): Promise<TaskResponse> => {
  const res = await axiosInstance.patch<TaskResponse>(`/tasks/${taskId}/jira/link`, {
    jiraIssueKey: issueKey,
  })
  return res.data
}

export const unlinkJiraIssue = async (taskId: number): Promise<TaskResponse> => {
  const res = await axiosInstance.delete<TaskResponse>(`/tasks/${taskId}/jira/link`)
  return res.data
}

export const createJiraIssueForTask = async (
  taskId: number,
  dto: CreateJiraIssueRequest,
): Promise<TaskResponse> => {
  const res = await axiosInstance.post<TaskResponse>(`/tasks/${taskId}/jira/create-issue`, dto)
  return res.data
}
