// ── Enums ────────────────────────────────────────────────────────────────────

export type UserRole = 'PM' | 'Developer' | 'QA'

export type TaskStatus =
  | 'Pending'
  | 'InProgress'
  | 'PRRaised'
  | 'Merged'
  | 'QAApproved'
  | 'Deployed'
  | 'Done'

export type ReleaseStatus = 'Upcoming' | 'Active' | 'Shipped' | 'Cancelled'

// ── Auth ─────────────────────────────────────────────────────────────────────

export interface AuthUser {
  username: string
  role: UserRole
}

// ── Common ───────────────────────────────────────────────────────────────────

export interface PagedResult<T> {
  data: T[]
  totalCount: number
  pageNumber: number
  pageSize: number
  totalPages: number
  hasNextPage: boolean
  hasPreviousPage: boolean
}

// ── Users ────────────────────────────────────────────────────────────────────

export interface UserResponse {
  id: number
  username: string
  role: UserRole
}

// ── Releases ─────────────────────────────────────────────────────────────────

export interface TaskSummary {
  id: number
  title: string
  assignedToUsername: string
  assignedToQAUsername: string | null
  status: TaskStatus
}

export interface ReleaseResponse {
  id: number
  title: string
  description: string | null
  startDate: string
  endDate: string
  status: ReleaseStatus
  createdAt: string
  tasks: TaskSummary[]
}

export interface CreateReleaseDto {
  title: string
  description?: string
  startDate: string
  endDate: string
}

export interface UpdateReleaseDto {
  title: string
  description?: string
  startDate: string
  endDate: string
  status: ReleaseStatus
}

// ── Tasks ─────────────────────────────────────────────────────────────────────

export interface TaskResponse {
  id: number
  title: string
  releaseId: number
  releaseTitle: string
  assignedToUserId: number
  assignedToUsername: string
  assignedToQAUserId: number | null
  assignedToQAUsername: string | null
  prLink: string | null
  remarks: string | null
  jiraIssueKey: string | null
  status: TaskStatus
  createdAt: string
}

export interface CreateTaskDto {
  title: string
  releaseId: number
  assignedToUserId: number
  assignedToQAUserId: number
}

export interface UpdateTaskDto {
  title: string
  assignedToUserId: number
  assignedToQAUserId: number
}

export interface UpdateDevFieldsDto {
  prLink?: string
  remarks?: string
}

export interface UpdateQAFieldsDto {
  remarks?: string
}

// ── Jira ─────────────────────────────────────────────────────────────────────

export interface JiraSettingsResponse {
  baseUrl: string
  email: string
  isConfigured: boolean
}

export interface UpsertJiraSettingsDto {
  baseUrl: string
  email: string
  apiToken: string
}

export interface JiraIssueResponse {
  key: string
  summary: string
  status: string
  assignee: string | null
  issueType: string | null
  browseUrl: string
}

export interface JiraSearchResult {
  issues: JiraIssueResponse[]
  total: number
  startAt: number
  maxResults: number
}

export interface JiraImportDto {
  jiraIssueKey: string
  releaseId: number
  assignedToUserId: number
  assignedToQAUserId: number
}

export interface CreateJiraIssueRequest {
  projectKey: string
  issueType?: string
  description?: string
}
