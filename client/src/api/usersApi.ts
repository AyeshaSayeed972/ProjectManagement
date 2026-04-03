import axiosInstance from '@/api/axios'
import type { PagedResult, UserResponse, UserRole } from '@/types'
import { DEFAULT_PAGE_SIZE } from '@/constants'

export const getUsers = async (
  pageNumber = 1,
  pageSize = DEFAULT_PAGE_SIZE,
  role?: UserRole,
): Promise<PagedResult<UserResponse>> => {
  const res = await axiosInstance.get<PagedResult<UserResponse>>('/users', {
    params: { pageNumber, pageSize, ...(role ? { role } : {}) },
  })
  return res.data
}
