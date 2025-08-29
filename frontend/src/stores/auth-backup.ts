// import { defineStore } from 'pinia'
// import { computed, ref } from 'vue'

// // API base URL from environment variables
// const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000'

// // Types for authentication (matching backend DTOs)
// export interface User {
//   id: string
//   email: string
//   firstName: string

// This file is a backup and should not be included in the build. All code is commented out to prevent TypeScript errors.
//       localStorage.setItem('voxmed_user', JSON.stringify(data.user))
//     } catch (error: any) {
//       console.error('Login error:', error)
//       error.value = error.message || 'Login failed'
//       throw new Error(error.message || 'Login failed')
//     } finally {
//       isLoading.value = false
//     }
//   }

//   const logout = async (): Promise<void> => {
//     try {
//       // Call logout API if token exists
//       if (token.value) {
//         await fetch(`${API_BASE_URL}/api/auth/logout`, {
//           method: 'POST',
//           headers: {
//             'Authorization': `Bearer ${token.value}`,
//             'Content-Type': 'application/json',
//           },
//         }).catch(() => {
//           // Ignore logout API errors, just clear local state
//         })
//       }
//     } finally {
//       // Clear state
//       user.value = null
//       token.value = null
//       refreshToken.value = null
//       error.value = null
      
//       // Clear storage
//       localStorage.removeItem('voxmed_token')
//     localStorage.removeItem('voxmed_refresh_token')
//     localStorage.removeItem('voxmed_user')
//     sessionStorage.removeItem('voxmed_token')
//     sessionStorage.removeItem('voxmed_refresh_token')
//     sessionStorage.removeItem('voxmed_user')
//   }

//   const restoreSession = (): boolean => {
//     try {
//       // Try localStorage first (remember me)
//       let storedToken = localStorage.getItem('voxmed_token')
//       let storedUser = localStorage.getItem('voxmed_user')
//       let storedRefreshToken = localStorage.getItem('voxmed_refresh_token')

//       // Fallback to sessionStorage
//       if (!storedToken) {
//         storedToken = sessionStorage.getItem('voxmed_token')
//         storedUser = sessionStorage.getItem('voxmed_user')
//         storedRefreshToken = sessionStorage.getItem('voxmed_refresh_token')
//       }

//       if (storedToken && storedUser) {
//         token.value = storedToken
//         refreshToken.value = storedRefreshToken
//         user.value = JSON.parse(storedUser)
//         return true
//       }
//     } catch (error) {
//       console.error('Error restoring session:', error)
//       logout() // Clear any corrupted data
//     }
    
//     return false
//   }

//   const refreshAuthToken = async (): Promise<boolean> => {
//     if (!refreshToken.value) return false

//     try {
//       // TODO: Replace with actual API call
//       const response = await mockRefreshToken(refreshToken.value)
      
//       token.value = response.token
      
//       // Update stored token
//       if (localStorage.getItem('voxmed_token')) {
//         localStorage.setItem('voxmed_token', response.token)
//       } else {
//         sessionStorage.setItem('voxmed_token', response.token)
//   const restoreSession = (): boolean => {
//     try {
//       const storedToken = localStorage.getItem('voxmed_token')
//       const storedRefreshToken = localStorage.getItem('voxmed_refresh_token')
//       const storedUser = localStorage.getItem('voxmed_user')
      
//       if (storedToken && storedUser) {
//         token.value = storedToken
//         refreshToken.value = storedRefreshToken
//         user.value = JSON.parse(storedUser)
//         return true
//       }
      
//       return false
//     } catch (error) {
//       console.error('Session restoration failed:', error)
//       return false
//     }
//   }

//   const refreshAuthToken = async (): Promise<boolean> => {
//     if (!refreshToken.value) return false
    
//     try {
//       const response = await fetch(`${API_BASE_URL}/api/auth/refresh`, {
//         method: 'POST',
//         headers: {
//           'Content-Type': 'application/json',
//         },
//         body: JSON.stringify(refreshToken.value),
//       })

//       if (!response.ok) {
//         throw new Error('Token refresh failed')
//       }

//       const authResponse: AuthResponse = await response.json()
      
//       if (authResponse.success && authResponse.data) {
//         token.value = authResponse.data.token
//         refreshToken.value = authResponse.data.refreshToken
        
//         // Update stored tokens
//         localStorage.setItem('voxmed_token', authResponse.data.token)
//         localStorage.setItem('voxmed_refresh_token', authResponse.data.refreshToken)
        
//         return true
//       }
      
//       return false
//     } catch (error) {
//       console.error('Token refresh failed:', error)
//       logout()
//       return false
//     }
//   }

//   return {
//     // State
//     user,
//     token,
//     isLoading,
//     error,
    
//     // Getters
//     isAuthenticated,
//     userRole,
//     userName,
//     isDoctor,
//     isPatient,
    
//     // Actions
//     login,
//     logout,
//     restoreSession,
//     refreshAuthToken
//   }
// })
