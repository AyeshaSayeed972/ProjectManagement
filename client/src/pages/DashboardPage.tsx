import React, { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { Package, CheckSquare, Activity, User } from 'lucide-react'
import { Layout } from '@/components/layout/Layout'
import { Card } from '@/components/ui/Card'
import { Badge } from '@/components/ui/Badge'
import { getReleases } from '@/api/releasesApi'
import { useAuth } from '@/hooks/useAuth'
import { formatDateRange } from '@/utils/dateHelpers'
import { DASHBOARD_RECENT_COUNT } from '@/constants'
import type { ReleaseResponse } from '@/types'

export const DashboardPage: React.FC = () => {
  const { user } = useAuth()
  const [releases, setReleases] = useState<ReleaseResponse[]>([])
  const [totalReleases, setTotalReleases] = useState(0)
  const [isLoading, setIsLoading] = useState(true)

  useEffect(() => {
    const fetch = async () => {
      try {
        const result = await getReleases()
        setReleases(result.data)
        setTotalReleases(result.totalCount)
      } catch (err) {
        console.error(err)
      } finally {
        setIsLoading(false)
      }
    }
    fetch()
  }, [])

  const activeCount = releases.filter((r) => r.status === 'Active').length
  const totalTasks = releases.reduce((sum, r) => sum + r.tasks.length, 0)
  const myTasks = user
    ? releases.reduce(
        (sum, r) => sum + r.tasks.filter((t) => t.assignedToUsername === user.username).length,
        0,
      )
    : 0

  const recentReleases = releases.slice(0, DASHBOARD_RECENT_COUNT)

  return (
    <Layout title="Dashboard">
      {/* Stats */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 mb-8">
        <Card
          title="Total Releases"
          value={isLoading ? '—' : totalReleases}
          icon={<Package className="w-6 h-6" />}
          colorClass="bg-blue-100 text-blue-600"
        />
        <Card
          title="Active Releases"
          value={isLoading ? '—' : activeCount}
          icon={<Activity className="w-6 h-6" />}
          colorClass="bg-green-100 text-green-600"
        />
        <Card
          title="Total Tasks"
          value={isLoading ? '—' : totalTasks}
          icon={<CheckSquare className="w-6 h-6" />}
          colorClass="bg-purple-100 text-purple-600"
        />
        {user?.role !== 'PM' ? (
          <Card
            title="My Tasks"
            value={isLoading ? '—' : myTasks}
            icon={<User className="w-6 h-6" />}
            colorClass="bg-orange-100 text-orange-600"
          />
        ) : (
          <Card
            title="Shipped Releases"
            value={isLoading ? '—' : releases.filter((r) => r.status === 'Shipped').length}
            icon={<CheckSquare className="w-6 h-6" />}
            colorClass="bg-indigo-100 text-indigo-600"
          />
        )}
      </div>

      {/* Recent Releases */}
      <div>
        <div className="flex items-center justify-between mb-4">
          <h2 className="text-base font-semibold text-gray-900">Recent Releases</h2>
          <Link to="/releases" className="text-sm text-blue-600 hover:text-blue-700 font-medium">
            View all →
          </Link>
        </div>

        {isLoading ? (
          <div className="space-y-3">
            {Array.from({ length: 5 }).map((_, i) => (
              <div key={i} className="h-16 bg-gray-200 rounded-xl animate-pulse" />
            ))}
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
            {recentReleases.map((release) => (
              <Link
                key={release.id}
                to={`/releases/${release.id}`}
                className="bg-white rounded-xl border border-gray-200 p-4 hover:border-blue-300 hover:shadow-sm transition-all flex items-center justify-between group"
              >
                <div className="min-w-0">
                  <p className="text-sm font-medium text-gray-900 truncate group-hover:text-blue-600 transition-colors">
                    {release.title}
                  </p>
                  <p className="text-xs text-gray-400 mt-0.5">{formatDateRange(release.startDate, release.endDate)}</p>
                  <p className="text-xs text-gray-400">{release.tasks.length} task{release.tasks.length !== 1 ? 's' : ''}</p>
                </div>
                <Badge type="release" status={release.status} />
              </Link>
            ))}
          </div>
        )}
      </div>
    </Layout>
  )
}
