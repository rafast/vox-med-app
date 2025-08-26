<template>
  <div class="login-container">
    <div class="login-card">
      <div class="login-header">
        <h1>VoxMed</h1>
        <p>Medical Management System</p>
      </div>
      
      <form @submit.prevent="handleLogin" class="login-form">
        <div class="form-group">
          <label for="email">Email</label>
          <input
            id="email"
            v-model="loginForm.email"
            type="email"
            placeholder="Enter your email"
            required
            :disabled="isLoading"
          />
          <span v-if="errors.email" class="error">{{ errors.email }}</span>
        </div>

        <div class="form-group">
          <label for="password">Password</label>
          <input
            id="password"
            v-model="loginForm.password"
            type="password"
            placeholder="Enter your password"
            required
            :disabled="isLoading"
          />
          <span v-if="errors.password" class="error">{{ errors.password }}</span>
        </div>



        <button 
          type="submit" 
          class="login-button"
          :disabled="isLoading"
        >
          <span v-if="isLoading" class="loading-spinner"></span>
          {{ isLoading ? 'Signing in...' : 'Sign In' }}
        </button>

        <div v-if="errors.general" class="error general-error">
          {{ errors.general }}
        </div>
      </form>


    </div>
  </div>
</template>

<script setup lang="ts">
import { useAuthStore } from '@/stores/auth'
import { reactive, ref } from 'vue'
import { useRouter } from 'vue-router'

const router = useRouter()
const authStore = useAuthStore()

// Form data
const loginForm = reactive({
  email: '',
  password: ''
})

// Loading state
const isLoading = ref(false)

// Form validation errors
const errors = reactive({
  email: '',
  password: '',
  general: ''
})

// Clear errors when user types
const clearErrors = () => {
  errors.email = ''
  errors.password = ''
  errors.general = ''
}

// Validate form
const validateForm = (): boolean => {
  clearErrors()
  let isValid = true

  if (!loginForm.email) {
    errors.email = 'Email is required'
    isValid = false
  } else if (!/\S+@\S+\.\S+/.test(loginForm.email)) {
    errors.email = 'Please enter a valid email'
    isValid = false
  }

  if (!loginForm.password) {
    errors.password = 'Password is required'
    isValid = false
  } else if (loginForm.password.length < 6) {
    errors.password = 'Password must be at least 6 characters'
    isValid = false
  }

  return isValid
}

// Handle login submission
const handleLogin = async () => {
  if (!validateForm()) return

  isLoading.value = true
  clearErrors()

  try {
    await authStore.login({
      email: loginForm.email,
      password: loginForm.password
    })

    // Redirect to dashboard or intended page
    const redirectTo = router.currentRoute.value.query.redirect as string
    router.push(redirectTo || '/dashboard')
  } catch (error: any) {
    errors.general = error.message || 'Login failed. Please try again.'
  } finally {
    isLoading.value = false
  }
}

// Watch for input changes to clear errors
import { watch } from 'vue'
watch([() => loginForm.email, () => loginForm.password], clearErrors)
</script>

<style scoped>
.login-container {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  /* Doctor clinic background with professional overlay */
  background: 
    linear-gradient(135deg, rgba(59, 130, 246, 0.75) 0%, rgba(37, 99, 235, 0.85) 100%),
    url('https://images.unsplash.com/photo-1559757148-5c350d0d3c56?ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&w=2831&q=80') center/cover;
  /* Fallback gradient for when image doesn't load */
  background-color: #3b82f6;
  padding: 20px;
  position: relative;
  background-attachment: fixed;
}

/* Medical pattern overlay for professionalism */
.login-container::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-image: 
    radial-gradient(circle at 2px 2px, rgba(255,255,255,0.08) 1px, transparent 0);
  background-size: 25px 25px;
  z-index: 0;
}

/* Alternative doctor clinic background as fallback */
.login-container::after {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: 
    linear-gradient(135deg, rgba(59, 130, 246, 0.8) 0%, rgba(37, 99, 235, 0.9) 100%),
    url('https://images.unsplash.com/photo-1576091160399-112ba8d25d1f?ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&w=2070&q=80') center/cover;
  opacity: 0;
  z-index: -1;
  transition: opacity 0.3s ease;
}

/* Responsive adjustments */
@media (max-width: 768px) {
  .login-container {
    /* Simpler background for mobile */
    background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%);
    padding: 10px;
  }
  
  .login-container::before,
  .login-container::after {
    display: none;
  }
}

.login-card {
  background: rgba(255, 255, 255, 0.96);
  padding: 2.5rem;
  border-radius: 16px;
  box-shadow: 
    0 25px 50px rgba(0, 0, 0, 0.15),
    0 10px 20px rgba(0, 0, 0, 0.1),
    0 0 0 1px rgba(255, 255, 255, 0.1);
  width: 100%;
  max-width: 400px;
  backdrop-filter: blur(20px);
  border: 1px solid rgba(255, 255, 255, 0.2);
  position: relative;
  z-index: 2;
  transform: translateY(0);
  transition: transform 0.3s ease, box-shadow 0.3s ease;
}

.login-card:hover {
  transform: translateY(-2px);
  box-shadow: 
    0 30px 60px rgba(0, 0, 0, 0.2),
    0 15px 25px rgba(0, 0, 0, 0.1),
    0 0 0 1px rgba(255, 255, 255, 0.1);
}

.login-header {
  text-align: center;
  margin-bottom: 2.5rem;
  position: relative;
}

.login-header::before {
  content: '⚕';
  font-size: 2rem;
  color: #3b82f6;
  display: block;
  margin-bottom: 0.5rem;
  opacity: 0.8;
}

.login-header h1 {
  font-size: 2.2rem;
  font-weight: 700;
  color: #1e293b;
  margin-bottom: 0.5rem;
  letter-spacing: -0.025em;
  background: linear-gradient(135deg, #1e293b 0%, #3b82f6 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

.login-header p {
  color: #64748b;
  font-size: 0.95rem;
  font-weight: 500;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.login-form {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.form-group {
  display: flex;
  flex-direction: column;
}

.form-group label {
  margin-bottom: 0.5rem;
  font-weight: 500;
  color: #2d3748;
}

.form-group input {
  padding: 0.75rem;
  border: 1px solid #e2e8f0;
  border-radius: 6px;
  font-size: 1rem;
  transition: border-color 0.2s, box-shadow 0.2s;
}

.form-group input:focus {
  outline: none;
  border-color: #667eea;
  box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
}

.form-group input:disabled {
  background-color: #f7fafc;
  cursor: not-allowed;
}

.login-button {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  padding: 0.75rem;
  border: none;
  border-radius: 6px;
  font-size: 1rem;
  font-weight: 500;
  cursor: pointer;
  transition: opacity 0.2s;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
}

.login-button:hover:not(:disabled) {
  opacity: 0.9;
}

.login-button:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.loading-spinner {
  width: 16px;
  height: 16px;
  border: 2px solid transparent;
  border-top: 2px solid white;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

.error {
  color: #e53e3e;
  font-size: 0.875rem;
  margin-top: 0.25rem;
}

.general-error {
  text-align: center;
  padding: 0.75rem;
  background-color: #fed7d7;
  border: 1px solid #feb2b2;
  border-radius: 6px;
  margin-top: 1rem;
}

/* Responsive design */
@media (max-width: 480px) {
  .login-container {
    padding: 10px;
  }
  
  .login-card {
    padding: 1.5rem;
  }
  
  .login-header h1 {
    font-size: 1.5rem;
  }
}
</style>
