import React from 'react'
import { Bell } from 'lucide-react'
import { useAuth } from '@/hooks/useAuth'

interface HeaderProps {
  title: string
}

export const Header: React.FC<HeaderProps> = ({ title }) => {
  const { user } = useAuth()

  return (
    <header className="h-16 bg-white border-b border-gray-200 flex items-center justify-between px-6 shrink-0">
      <h1 className="text-xl font-semibold text-gray-900">{title}</h1>
      <div className="flex items-center gap-3">
        <button className="p-2 rounded-lg text-gray-400 hover:text-gray-600 hover:bg-gray-100 transition-colors">
          <Bell className="w-5 h-5" />
        </button>
        {user && (
          <div className="w-8 h-8 rounded-full bg-blue-600 flex items-center justify-center text-white text-sm font-semibold">
            {user.username.charAt(0).toUpperCase()}
          </div>
        )}
      </div>
    </header>
  )
}
