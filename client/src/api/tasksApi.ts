import axiosInstance from '@/api/axios'
import type {
  PagedResult,
  TaskResponse,
  CreateTaskDto,
  UpdateTaskDto,
  UpdateDevFieldsDto,
  UpdateQAFieldsDto,
  TaskStatus,
} from '@/types'
import { DEFAULT_PAGE_SIZE } from '@/constants'

export const getAllTasks = async (
  pageNumber = 1,
  pageSize = DEFAULT_PAGE_SIZE,
  status?: TaskStatus,
  assignedToUsername?: string,
  userRole?: string,
): Promise<PagedResult<TaskResponse>> => {
  const res = await axiosInstance.get<PagedResult<TaskResponse>>('/tasks', {
    params: { pageNumber, pageSize, ...(status && { status }), ...(assignedToUsername && { assignedToUsername }), ...(userRole && { userRole }) },
  })
  return res.data
}

export const getTasksByRelease = async (
  releaseId: number,
  pageNumber = 1,
  pageSize = DEFAULT_PAGE_SIZE,
  status?: TaskStatus,
  assignedToUsername?: string,
  userRole?: string,
): Promise<PagedResult<TaskResponse>> => {
  const res = await axiosInstance.get<PagedResult<TaskResponse>>(`/tasks/release/${releaseId}`, {
    params: { pageNumber, pageSize, ...(status && { status }), ...(assignedToUsername && { assignedToUsername }), ...(userRole && { userRole }) },
  })
  return res.data
}

export const getTaskById = async (id: number): Promise<TaskResponse> => {
  const res = await axiosInstance.get<TaskResponse>(`/tasks/${id}`)
  return res.data
}

export const createTask = async (dto: CreateTaskDto): Promise<TaskResponse> => {
  const res = await axiosInstance.post<TaskResponse>('/tasks', dto)
  return res.data
}

export const updateTask = async (id: number, dto: UpdateTaskDto): Promise<TaskResponse> => {
  const res = await axiosInstance.put<TaskResponse>(`/tasks/${id}`, dto)
  return res.data
}

export const updateTaskStatus = async (id: number, newStatus: TaskStatus): Promise<TaskResponse> => {
  const res = await axiosInstance.patch<TaskResponse>(`/tasks/${id}/status`, { newStatus })
  return res.data
}

export const updateDevFields = async (id: number, dto: UpdateDevFieldsDto): Promise<TaskResponse> => {
  const res = await axiosInstance.patch<TaskResponse>(`/tasks/${id}/dev-fields`, dto)
  return res.data
}

export const updateQAFields = async (id: number, dto: UpdateQAFieldsDto): Promise<TaskResponse> => {
  const res = await axiosInstance.patch<TaskResponse>(`/tasks/${id}/qa-fields`, dto)
  return res.data
}

export const deleteTask = async (id: number): Promise<void> => {
  await axiosInstance.delete(`/tasks/${id}`)
}
