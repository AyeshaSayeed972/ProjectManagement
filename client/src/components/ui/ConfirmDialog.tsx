import React from 'react'
import { AlertTriangle } from 'lucide-react'
import { Modal } from './Modal'
import { Button } from './Button'

interface ConfirmDialogProps {
  isOpen: boolean
  onClose: () => void
  onConfirm: () => void
  title: string
  message: string
  confirmLabel?: string
  isLoading?: boolean
}

export const ConfirmDialog: React.FC<ConfirmDialogProps> = ({
  isOpen,
  onClose,
  onConfirm,
  title,
  message,
  confirmLabel = 'Delete',
  isLoading = false,
}) => {
  return (
    <Modal isOpen={isOpen} onClose={onClose} title={title} size="sm">
      <div className="flex flex-col items-center gap-4 text-center">
        <div className="flex items-center justify-center w-12 h-12 rounded-full bg-red-100">
          <AlertTriangle className="w-6 h-6 text-red-600" />
        </div>
        <p className="text-sm text-gray-600">{message}</p>
        <div className="flex gap-3 w-full">
          <Button variant="secondary" className="flex-1" onClick={onClose} disabled={isLoading}>
            Cancel
          </Button>
          <Button variant="danger" className="flex-1" onClick={onConfirm} loading={isLoading}>
            {confirmLabel}
          </Button>
        </div>
      </div>
    </Modal>
  )
}
