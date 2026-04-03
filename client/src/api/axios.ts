import axios from 'axios'

const axiosInstance = axios.create({
  baseURL: '/api',
  withCredentials: true, // send HttpOnly cookies on every request
})

// Handle 401 — attempt silent token refresh via cookie, queue concurrent failures
let isRefreshing = false
let failedQueue: Array<{ resolve: () => void; reject: (err: unknown) => void }> = []

const processQueue = (error: unknown) => {
  failedQueue.forEach((prom) => {
    if (error) prom.reject(error)
    else prom.resolve()
  })
  failedQueue = []
}

axiosInstance.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config as typeof error.config & { _retry?: boolean }

    if (error.response?.status === 401 && !originalRequest._retry) {
      if (isRefreshing) {
        return new Promise<void>((resolve, reject) => {
          failedQueue.push({ resolve, reject })
        })
          .then(() => axiosInstance(originalRequest))
          .catch((err) => Promise.reject(err))
      }

      originalRequest._retry = true
      isRefreshing = true

      try {
        // Browser automatically sends the refresh_token HttpOnly cookie
        await axios.post('/api/auth/refresh', null, { withCredentials: true })
        processQueue(null)
        return axiosInstance(originalRequest)
      } catch (refreshError) {
        processQueue(refreshError)
        window.location.href = '/login'
        return Promise.reject(refreshError)
      } finally {
        isRefreshing = false
      }
    }

    return Promise.reject(error)
  },
)

export default axiosInstance
