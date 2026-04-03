import React from 'react'

interface InputProps extends React.InputHTMLAttributes<HTMLInputElement> {
  label?: string
  error?: string
}

export const Input = React.forwardRef<HTMLInputElement, InputProps>(
  ({ label, error, id, className = '', ...rest }, ref) => {
    return (
      <div className="flex flex-col gap-1">
        {label && (
          <label htmlFor={id} className="text-sm font-medium text-gray-700">
            {label}
          </label>
        )}
        <input
          id={id}
          ref={ref}
          className={`
            block w-full rounded-lg border px-3 py-2 text-sm text-gray-900 placeholder-gray-400
            focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-colors
            ${error ? 'border-red-400 focus:ring-red-400 focus:border-red-400' : 'border-gray-300'}
            ${className}
          `}
          {...rest}
        />
        {error && <p className="text-xs text-red-600">{error}</p>}
      </div>
    )
  },
)
Input.displayName = 'Input'
