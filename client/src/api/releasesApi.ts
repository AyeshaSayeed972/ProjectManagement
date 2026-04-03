import axiosInstance from '@/api/axios'
import type { PagedResult, ReleaseResponse, CreateReleaseDto, UpdateReleaseDto } from '@/types'
import { DEFAULT_PAGE_SIZE } from '@/constants'

export const getReleases = async (pageNumber = 1, pageSize = DEFAULT_PAGE_SIZE): Promise<PagedResult<ReleaseResponse>> => {
  const res = await axiosInstance.get<PagedResult<ReleaseResponse>>('/releases', {
    params: { pageNumber, pageSize },
  })
  return res.data
}

export const getReleaseById = async (id: number): Promise<ReleaseResponse> => {
  const res = await axiosInstance.get<ReleaseResponse>(`/releases/${id}`)
  return res.data
}

export const createRelease = async (dto: CreateReleaseDto): Promise<ReleaseResponse> => {
  const res = await axiosInstance.post<ReleaseResponse>('/releases', dto)
  return res.data
}

export const updateRelease = async (id: number, dto: UpdateReleaseDto): Promise<ReleaseResponse> => {
  const res = await axiosInstance.put<ReleaseResponse>(`/releases/${id}`, dto)
  return res.data
}

export const deleteRelease = async (id: number): Promise<void> => {
  await axiosInstance.delete(`/releases/${id}`)
}
