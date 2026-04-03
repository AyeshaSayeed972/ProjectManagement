import axiosInstance from '@/api/axios'
import type { AuthUser } from '@/types'

export const login = async (username: string, password: string): Promise<AuthUser> => {
  const res = await axiosInstance.post<AuthUser>('/auth/login', { username, password })
  return res.data
}

export const me = async (): Promise<AuthUser> => {
  const res = await axiosInstance.get<AuthUser>('/auth/me')
  return res.data
}

export const logout = async (): Promise<void> => {
  await axiosInstance.post('/auth/logout')
}
