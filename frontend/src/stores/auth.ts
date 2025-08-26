import { defineStore } from 'pinia'
import { computed, ref } from 'vue'

// Types for authentication
export interface User {
  id: string
  email: string
  firstName: string
  lastName: string
  role: string
  isActive: boolean
}

export interface LoginCredentials {
  email: string
  password: string
  rememberMe?: boolean
}

export interface AuthResponse {
  user: User
  token: string
  refreshToken: string
}

export const useAuthStore = defineStore('auth', () => {
  // State
  const user = ref<User | null>(null)
  const token = ref<string | null>(null)
  const refreshToken = ref<string | null>(null)
  const isLoading = ref(false)

  // Getters
  const isAuthenticated = computed(() => !!token.value && !!user.value)
  const userRole = computed(() => user.value?.role || '')
  const userName = computed(() => 
    user.value ? `${user.value.firstName} ${user.value.lastName}` : ''
  )

  // Actions
  const login = async (credentials: LoginCredentials): Promise<void> => {
    isLoading.value = true
    
    try {
      // TODO: Replace with actual API call
      const response = await mockLogin(credentials)
      
      // Store authentication data
      user.value = response.user
      token.value = response.token
      refreshToken.value = response.refreshToken
      
      // Store in localStorage if remember me is checked
      if (credentials.rememberMe) {
        localStorage.setItem('voxmed_token', response.token)
        localStorage.setItem('voxmed_refresh_token', response.refreshToken)
        localStorage.setItem('voxmed_user', JSON.stringify(response.user))
      } else {
        sessionStorage.setItem('voxmed_token', response.token)
        sessionStorage.setItem('voxmed_refresh_token', response.refreshToken)
        sessionStorage.setItem('voxmed_user', JSON.stringify(response.user))
      }
    } catch (error: any) {
      console.error('Login error:', error)
      throw new Error(error.message || 'Login failed')
    } finally {
      isLoading.value = false
    }
  }

  const logout = (): void => {
    // Clear state
    user.value = null
    token.value = null
    refreshToken.value = null
    
    // Clear storage
    localStorage.removeItem('voxmed_token')
    localStorage.removeItem('voxmed_refresh_token')
    localStorage.removeItem('voxmed_user')
    sessionStorage.removeItem('voxmed_token')
    sessionStorage.removeItem('voxmed_refresh_token')
    sessionStorage.removeItem('voxmed_user')
  }

  const restoreSession = (): boolean => {
    try {
      // Try localStorage first (remember me)
      let storedToken = localStorage.getItem('voxmed_token')
      let storedUser = localStorage.getItem('voxmed_user')
      let storedRefreshToken = localStorage.getItem('voxmed_refresh_token')

      // Fallback to sessionStorage
      if (!storedToken) {
        storedToken = sessionStorage.getItem('voxmed_token')
        storedUser = sessionStorage.getItem('voxmed_user')
        storedRefreshToken = sessionStorage.getItem('voxmed_refresh_token')
      }

      if (storedToken && storedUser) {
        token.value = storedToken
        refreshToken.value = storedRefreshToken
        user.value = JSON.parse(storedUser)
        return true
      }
    } catch (error) {
      console.error('Error restoring session:', error)
      logout() // Clear any corrupted data
    }
    
    return false
  }

  const refreshAuthToken = async (): Promise<boolean> => {
    if (!refreshToken.value) return false

    try {
      // TODO: Replace with actual API call
      const response = await mockRefreshToken(refreshToken.value)
      
      token.value = response.token
      
      // Update stored token
      if (localStorage.getItem('voxmed_token')) {
        localStorage.setItem('voxmed_token', response.token)
      } else {
        sessionStorage.setItem('voxmed_token', response.token)
      }
      
      return true
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
    
    // Getters
    isAuthenticated,
    userRole,
    userName,
    
    // Actions
    login,
    logout,
    restoreSession,
    refreshAuthToken
  }
})

// Mock functions - replace with actual API calls
async function mockLogin(credentials: LoginCredentials): Promise<AuthResponse> {
  // Simulate API delay
  await new Promise(resolve => setTimeout(resolve, 1000))
  
  // Mock validation
  if (credentials.email === 'admin@voxmed.com' && credentials.password === 'admin123') {
    return {
      user: {
        id: '1',
        email: 'admin@voxmed.com',
        firstName: 'System',
        lastName: 'Administrator',
        role: 'admin',
        isActive: true
      },
      token: 'mock-jwt-token-' + Date.now(),
      refreshToken: 'mock-refresh-token-' + Date.now()
    }
  } else if (credentials.email === 'doctor@voxmed.com' && credentials.password === 'doctor123') {
    return {
      user: {
        id: '2',
        email: 'doctor@voxmed.com',
        firstName: 'John',
        lastName: 'Smith',
        role: 'doctor',
        isActive: true
      },
      token: 'mock-jwt-token-' + Date.now(),
      refreshToken: 'mock-refresh-token-' + Date.now()
    }
  } else {
    throw new Error('Invalid email or password')
  }
}

async function mockRefreshToken(refreshToken: string): Promise<{ token: string }> {
  // Simulate API delay
  await new Promise(resolve => setTimeout(resolve, 500))
  
  // In a real app, validate the refresh token
  if (refreshToken.startsWith('mock-refresh-token-')) {
    return {
      token: 'mock-jwt-token-refreshed-' + Date.now()
    }
  } else {
    throw new Error('Invalid refresh token')
  }
}
