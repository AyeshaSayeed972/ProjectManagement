import { useState, useCallback, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import toast from 'react-hot-toast'
import { getReleaseById, updateRelease, deleteRelease } from '@/api/releasesApi'
import {
  getTasksByRelease,
  createTask,
  updateTask,
  updateTaskStatus,
  updateDevFields,
  updateQAFields,
  deleteTask,
} from '@/api/tasksApi'
import { getUsers } from '@/api/usersApi'
import { usePagination } from '@/hooks/usePagination'
import { fromInputDateValue } from '@/utils/dateHelpers'
import { getNextTaskStatus } from '@/utils/statusHelpers'
import { USERS_DROPDOWN_SIZE } from '@/constants'
import type { ReleaseResponse, TaskResponse, UserResponse, PagedResult, ReleaseStatus } from '@/types'
import axios from 'axios'

export const useReleaseDetail = (releaseId: number) => {
  const navigate = useNavigate()

  const [release, setRelease] = useState<ReleaseResponse | null>(null)
  const [releaseLoading, setReleaseLoading] = useState(true)

  const { pageNumber, pageSize, goToPage, setPageSize } = usePagination(10)
  const [tasksResult, setTasksResult] = useState<PagedResult<TaskResponse> | null>(null)
  const [tasksLoading, setTasksLoading] = useState(true)

  const [users, setUsers] = useState<UserResponse[]>([])
  const [error, setError] = useState<string | null>(null)
  const [actionLoading, setActionLoading] = useState(false)
  const [actionError, setActionError] = useState<string | null>(null)

  // Modal states
  const [editReleaseOpen, setEditReleaseOpen] = useState(false)
  const [deleteReleaseOpen, setDeleteReleaseOpen] = useState(false)
  const [addTaskOpen, setAddTaskOpen] = useState(false)
  const [editTask, setEditTask] = useState<TaskResponse | null>(null)
  const [deleteTaskTarget, setDeleteTaskTarget] = useState<TaskResponse | null>(null)
  const [devFieldsTask, setDevFieldsTask] = useState<TaskResponse | null>(null)
  const [qaFieldsTask, setQAFieldsTask] = useState<TaskResponse | null>(null)

  const fetchRelease = useCallback(async () => {
    try {
      const data = await getReleaseById(releaseId)
      setRelease(data)
    } catch {
      setError('Release not found.')
    } finally {
      setReleaseLoading(false)
    }
  }, [releaseId])

  const fetchTasks = useCallback(async () => {
    setTasksLoading(true)
    try {
      const data = await getTasksByRelease(releaseId, pageNumber, pageSize)
      setTasksResult(data)
    } catch (err) {
      console.error(err)
    } finally {
      setTasksLoading(false)
    }
  }, [releaseId, pageNumber, pageSize])

  useEffect(() => { fetchRelease() }, [fetchRelease])
  useEffect(() => { fetchTasks() }, [fetchTasks])

  const loadUsers = async () => {
    if (users.length > 0) return
    try {
      const data = await getUsers(1, USERS_DROPDOWN_SIZE)
      setUsers(data.data)
    } catch (err) {
      console.error(err)
    }
  }

  // ── Release actions ──────────────────────────────────────────────────────────

  const handleEditRelease = async (data: { title: string; description: string; startDate: string; endDate: string; status: ReleaseStatus }) => {
    if (!release) return
    setActionLoading(true)
    setActionError(null)
    try {
      await updateRelease(release.id, {
        title: data.title,
        description: data.description || undefined,
        startDate: fromInputDateValue(data.startDate),
        endDate: fromInputDateValue(data.endDate),
        status: data.status,
      })
      setEditReleaseOpen(false)
      fetchRelease()
      toast.success('Release updated')
    } catch (err) {
      const msg = axios.isAxiosError(err) ? err.response?.data?.message ?? 'Failed to update.' : 'Failed to update.'
      setActionError(msg)
      toast.error(msg)
    } finally {
      setActionLoading(false)
    }
  }

  const handleDeleteRelease = async () => {
    if (!release) return
    setActionLoading(true)
    try {
      await deleteRelease(release.id)
      toast.success('Release deleted')
      navigate('/releases')
    } catch {
      setActionLoading(false)
    }
  }

  // ── Task actions ─────────────────────────────────────────────────────────────

  const handleAddTask = async (data: { title: string; assignedToUserId: number; assignedToQAUserId: number }) => {
    setActionLoading(true)
    setActionError(null)
    try {
      await createTask({ title: data.title, releaseId, assignedToUserId: data.assignedToUserId, assignedToQAUserId: data.assignedToQAUserId })
      setAddTaskOpen(false)
      fetchTasks()
      fetchRelease()
      toast.success('Task created')
    } catch (err) {
      const msg = axios.isAxiosError(err) ? err.response?.data?.message ?? 'Failed to create task.' : 'Failed to create task.'
      setActionError(msg)
      toast.error(msg)
    } finally {
      setActionLoading(false)
    }
  }

  const handleEditTask = async (data: { title: string; assignedToUserId: number; assignedToQAUserId: number }) => {
    if (!editTask) return
    setActionLoading(true)
    setActionError(null)
    try {
      await updateTask(editTask.id, { title: data.title, assignedToUserId: data.assignedToUserId, assignedToQAUserId: data.assignedToQAUserId })
      setEditTask(null)
      fetchTasks()
      toast.success('Task updated')
    } catch (err) {
      const msg = axios.isAxiosError(err) ? err.response?.data?.message ?? 'Failed to update task.' : 'Failed to update task.'
      setActionError(msg)
      toast.error(msg)
    } finally {
      setActionLoading(false)
    }
  }

  const handleDeleteTask = async () => {
    if (!deleteTaskTarget) return
    setActionLoading(true)
    try {
      await deleteTask(deleteTaskTarget.id)
      setDeleteTaskTarget(null)
      fetchTasks()
      fetchRelease()
      toast.success('Task deleted')
    } catch (err) {
      const msg = axios.isAxiosError(err) ? err.response?.data?.message ?? 'Failed to delete task.' : 'Failed to delete task.'
      toast.error(msg)
    } finally {
      setActionLoading(false)
    }
  }

  const handleAdvanceStatus = async (task: TaskResponse) => {
    const next = getNextTaskStatus(task.status)
    if (!next) return
    try {
      await updateTaskStatus(task.id, next)
      fetchTasks()
      toast.success(`Status updated to ${next}`)
    } catch (err) {
      if (axios.isAxiosError(err)) {
        const msg = err.response?.data?.message ?? 'Failed to advance status.'
        setError(msg)
        toast.error(msg)
      }
    }
  }

  const handleDevFields = async (data: { prLink: string; remarks: string }) => {
    if (!devFieldsTask) return
    setActionLoading(true)
    setActionError(null)
    try {
      await updateDevFields(devFieldsTask.id, {
        prLink: data.prLink || undefined,
        remarks: data.remarks || undefined,
      })
      setDevFieldsTask(null)
      fetchTasks()
      toast.success('Dev fields saved')
    } catch (err) {
      const msg = axios.isAxiosError(err) ? err.response?.data?.message ?? 'Failed to save.' : 'Failed to save.'
      setActionError(msg)
      toast.error(msg)
    } finally {
      setActionLoading(false)
    }
  }

  const handleQAFields = async (data: { remarks: string }) => {
    if (!qaFieldsTask) return
    setActionLoading(true)
    setActionError(null)
    try {
      await updateQAFields(qaFieldsTask.id, {
        remarks: data.remarks || undefined,
      })
      setQAFieldsTask(null)
      fetchTasks()
      toast.success('QA fields saved')
    } catch (err) {
      const msg = axios.isAxiosError(err) ? err.response?.data?.message ?? 'Failed to save.' : 'Failed to save.'
      setActionError(msg)
      toast.error(msg)
    } finally {
      setActionLoading(false)
    }
  }

  return {
    // Data
    release,
    releaseLoading,
    tasksResult,
    tasksLoading,
    users,
    error,
    actionLoading,
    actionError,
    // Pagination
    pageNumber,
    pageSize,
    goToPage,
    setPageSize,
    // Modal state
    editReleaseOpen, setEditReleaseOpen,
    deleteReleaseOpen, setDeleteReleaseOpen,
    addTaskOpen, setAddTaskOpen,
    editTask, setEditTask,
    deleteTaskTarget, setDeleteTaskTarget,
    devFieldsTask, setDevFieldsTask,
    qaFieldsTask, setQAFieldsTask,
    // Actions
    loadUsers,
    handleEditRelease,
    handleDeleteRelease,
    handleAddTask,
    handleEditTask,
    handleDeleteTask,
    handleAdvanceStatus,
    handleDevFields,
    handleQAFields,
  }
}
