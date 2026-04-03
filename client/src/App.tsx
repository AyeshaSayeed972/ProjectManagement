import React from 'react'
import { Routes, Route, Navigate, Outlet } from 'react-router-dom'
import { Toaster } from 'react-hot-toast'
import { useAuth } from '@/hooks/useAuth'
import { LoginPage } from '@/pages/LoginPage'
import { DashboardPage } from '@/pages/DashboardPage'
import { ReleasesPage } from '@/pages/ReleasesPage'
import { ReleaseDetailPage } from '@/pages/ReleaseDetailPage'
import { TasksPage } from '@/pages/TasksPage'
import { Loader2 } from 'lucide-react'

const ProtectedRoute: React.FC = () => {
  const { isAuthenticated, isLoading } = useAuth()

  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gray-50">
        <Loader2 className="w-8 h-8 text-blue-600 animate-spin" />
      </div>
    )
  }

  return isAuthenticated ? <Outlet /> : <Navigate to="/login" replace />
}

const App: React.FC = () => {
  return (
    <>
    <Toaster position="top-right" />
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/" element={<Navigate to="/dashboard" replace />} />

      <Route element={<ProtectedRoute />}>
        <Route path="/dashboard" element={<DashboardPage />} />
        <Route path="/releases" element={<ReleasesPage />} />
        <Route path="/releases/:id" element={<ReleaseDetailPage />} />
        <Route path="/tasks" element={<TasksPage />} />
      </Route>

      <Route path="*" element={<Navigate to="/dashboard" replace />} />
    </Routes>
    </>
  )
}

export default App
