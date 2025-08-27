import { defineStore } from 'pinia'
import { computed, ref } from 'vue'

// API base URL from environment variables
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000'

// Types for authentication (matching backend DTOs)
export interface User {
  id: string
  email: string
  firstName: string
  lastName: string
  fullName: string
  userType: 'Doctor' | 'Patient'
  userTypeString: string
  isActive: boolean
  // Doctor-specific properties
  medicalLicenseNumber?: string
  specialization?: string
  department?: string
  // Patient-specific properties
  patientId?: string
  dateOfBirth?: string
  bloodType?: string
  emergencyContact?: string
  phone?: string
  lastLoginAt?: string
}

export interface LoginCredentials {
  email: string
  password: string
}

export interface LoginResponse {
  token: string
  refreshToken: string
  expiresAt: string
  user: User
}

export interface AuthResponse {
  success: boolean
  message: string
  data?: LoginResponse
  errors?: string[]
}

export const useAuthStore = defineStore('auth', () => {
  // State
  const user = ref<User | null>(null)
  const token = ref<string | null>(null)
  const refreshToken = ref<string | null>(null)
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  // Getters
  const isAuthenticated = computed(() => !!token.value && !!user.value)
  const userRole = computed(() => user.value?.userType || '')
  const userName = computed(() => user.value?.fullName || '')
  const isDoctor = computed(() => user.value?.userType === 'Doctor')
  const isPatient = computed(() => user.value?.userType === 'Patient')

  // Actions
  const login = async (credentials: LoginCredentials): Promise<void> => {
    isLoading.value = true
    error.value = null
    
    try {
      const response = await fetch(`${API_BASE_URL}/api/auth/login`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(credentials),
      })

      if (!response.ok) {
        const errorData = await response.json().catch(() => ({ message: 'Login failed' }))
        throw new Error(errorData.message || 'Login failed')
      }

      const authResponse: AuthResponse = await response.json()
      
      if (!authResponse.success || !authResponse.data) {
        throw new Error(authResponse.message || 'Login failed')
      }

      // Store authentication data
      const { data } = authResponse
      user.value = data.user
      token.value = data.token
      refreshToken.value = data.refreshToken
      
      // Store in localStorage for persistence
      localStorage.setItem('voxmed_token', data.token)
      localStorage.setItem('voxmed_refresh_token', data.refreshToken)
      localStorage.setItem('voxmed_user', JSON.stringify(data.user))
      
      console.log('Login successful for user:', data.user.email)
    } catch (error: any) {
      console.error('Login error:', error)
      error.value = error.message || 'Login failed'
      throw error
    } finally {
      isLoading.value = false
    }
  }

  const logout = async (): Promise<void> => {
    try {
      // Call logout API if token exists
      if (token.value) {
        await fetch(`${API_BASE_URL}/api/auth/logout`, {
          method: 'POST',
          headers: {
            'Authorization': `Bearer ${token.value}`,
            'Content-Type': 'application/json',
          },
        }).catch(() => {
          // Ignore logout API errors, just clear local state
        })
      }
    } finally {
      // Clear state
      user.value = null
      token.value = null
      refreshToken.value = null
      error.value = null
      
      // Clear storage
      localStorage.removeItem('voxmed_token')
      localStorage.removeItem('voxmed_refresh_token')
      localStorage.removeItem('voxmed_user')
      
      console.log('User logged out')
    }
  }

  const restoreSession = (): boolean => {
    try {
      const storedToken = localStorage.getItem('voxmed_token')
      const storedRefreshToken = localStorage.getItem('voxmed_refresh_token')
      const storedUser = localStorage.getItem('voxmed_user')
      
      if (storedToken && storedUser) {
        token.value = storedToken
        refreshToken.value = storedRefreshToken
        user.value = JSON.parse(storedUser)
        console.log('Session restored for user:', user.value?.email)
        return true
      }
      
      return false
    } catch (error) {
      console.error('Session restoration failed:', error)
      return false
    }
  }

  const refreshAuthToken = async (): Promise<boolean> => {
    if (!refreshToken.value) return false
    
    try {
      const response = await fetch(`${API_BASE_URL}/api/auth/refresh`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(refreshToken.value),
      })

      if (!response.ok) {
        throw new Error('Token refresh failed')
      }

      const authResponse: AuthResponse = await response.json()
      
      if (authResponse.success && authResponse.data) {
        token.value = authResponse.data.token
        refreshToken.value = authResponse.data.refreshToken
        
        // Update stored tokens
        localStorage.setItem('voxmed_token', authResponse.data.token)
        localStorage.setItem('voxmed_refresh_token', authResponse.data.refreshToken)
        
        return true
      }
      
      return false
    } catch (error) {
      console.error('Token refresh failed:', error)
      logout()
      return false
    }
  }

  return {
    // State
    user,
    token,
    isLoading,
    error,
    
    // Getters
    isAuthenticated,
    userRole,
    userName,
    isDoctor,
    isPatient,
    
    // Actions
    login,
    logout,
    restoreSession,
    refreshAuthToken
  }
})
