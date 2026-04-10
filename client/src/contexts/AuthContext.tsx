import React, { createContext, useReducer, useEffect, useCallback } from 'react'
import axios from 'axios'
import { login as apiLogin, logout as apiLogout } from '@/api/authApi'
import type { AuthUser, UserRole } from '@/types'

interface AuthState {
  user: AuthUser | null
  isAuthenticated: boolean
  isLoading: boolean
}

type AuthAction =
  | { type: 'LOGIN_SUCCESS'; payload: AuthUser }
  | { type: 'LOGOUT' }
  | { type: 'SET_LOADING'; payload: boolean }

const initialState: AuthState = {
  user: null,
  isAuthenticated: false,
  isLoading: true,
}

function authReducer(state: AuthState, action: AuthAction): AuthState {
  switch (action.type) {
    case 'LOGIN_SUCCESS':
      return { user: action.payload, isAuthenticated: true, isLoading: false }
    case 'LOGOUT':
      return { user: null, isAuthenticated: false, isLoading: false }
    case 'SET_LOADING':
      return { ...state, isLoading: action.payload }
    default:
      return state
  }
}

interface AuthContextValue extends AuthState {
  login: (username: string, password: string) => Promise<void>
  logout: () => Promise<void>
}

export const AuthContext = createContext<AuthContextValue | null>(null)

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [state, dispatch] = useReducer(authReducer, initialState)

  // On mount: fetch antiforgery token, then check existing session
  useEffect(() => {
    axios
      .get('/api/auth/antiforgery', { withCredentials: true })
      .then(() =>
        axios.get<{ username: string; role: UserRole }>('/api/auth/me', { withCredentials: true })
      )
      .then((res) => {
        dispatch({ type: 'LOGIN_SUCCESS', payload: { username: res.data.username, role: res.data.role } })
      })
      .catch(() => {
        dispatch({ type: 'SET_LOADING', payload: false })
      })
  }, [])

  const login = useCallback(async (username: string, password: string) => {
    const data = await apiLogin(username, password)
    dispatch({ type: 'LOGIN_SUCCESS', payload: { username: data.username, role: data.role } })
  }, [])

  const logout = useCallback(async () => {
    try {
      await apiLogout()
    } catch {
      // If logout API fails, still clear local state
    }
    dispatch({ type: 'LOGOUT' })
  }, [])

  return (
    <AuthContext.Provider value={{ ...state, login, logout }}>
      {children}
    </AuthContext.Provider>
  )
}
