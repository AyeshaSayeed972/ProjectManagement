import axios from 'axios'

const axiosInstance = axios.create({
  baseURL: '/api',
  withCredentials: true, // send auth_session cookie on every request
})

// Attach XSRF-TOKEN cookie value as X-XSRF-TOKEN header on every request
axiosInstance.interceptors.request.use((config) => {
  const token = document.cookie
    .split('; ')
    .find((row) => row.startsWith('XSRF-TOKEN='))
    ?.split('=')[1]
  if (token) config.headers['X-XSRF-TOKEN'] = decodeURIComponent(token)
  return config
})

// On 401, redirect to login
axiosInstance.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      window.location.href = '/login'
    }
    return Promise.reject(error)
  },
)

export default axiosInstance
