import React from 'react'

interface CardProps {
  title: string
  value: string | number
  icon: React.ReactNode
  colorClass?: string
}

export const Card: React.FC<CardProps> = ({ title, value, icon, colorClass = 'bg-blue-100 text-blue-600' }) => {
  return (
    <div className="bg-white rounded-xl border border-gray-200 p-5 flex items-center gap-4 shadow-sm">
      <div className={`flex items-center justify-center w-12 h-12 rounded-xl ${colorClass}`}>
        {icon}
      </div>
      <div>
        <p className="text-sm text-gray-500">{title}</p>
        <p className="text-2xl font-bold text-gray-900">{value}</p>
      </div>
    </div>
  )
}
