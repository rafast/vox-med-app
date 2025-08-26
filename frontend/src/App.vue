<script setup lang="ts">
import { useAuthStore } from '@/stores/auth'
import { onMounted } from 'vue'
import { RouterView } from 'vue-router'

const authStore = useAuthStore()

// Restore authentication session on app startup
onMounted(() => {
  authStore.restoreSession()
})
</script>

<template>
  <div id="app">
    <RouterView />
  </div>
</template>

<style>
/* Global Reset */
*,
*::before,
*::after {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
}

/* Ensure html and body have no margins */
html,
body {
  margin: 0;
  padding: 0;
  width: 100%;
  height: 100%;
}

/* Root App Styles */
#app {
  min-height: 100vh;
  width: 100vw;
  margin: 0;
  padding: 0;
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', 'Roboto', 'Oxygen',
    'Ubuntu', 'Cantarell', 'Fira Sans', 'Droid Sans', 'Helvetica Neue',
    sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  background-color: #f7fafc;
  color: #2d3748;
}

/* Utility Classes */
.text-center { text-align: center; }
.text-left { text-align: left; }
.text-right { text-align: right; }
.mb-4 { margin-bottom: 1rem; }
.mt-4 { margin-top: 1rem; }
.p-4 { padding: 1rem; }
.w-full { width: 100%; }
.flex { display: flex; }
.items-center { align-items: center; }
.justify-center { justify-content: center; }
.justify-between { justify-content: space-between; }

/* Button Styles */
.btn {
  padding: 0.5rem 1rem;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 0.875rem;
  font-weight: 500;
  transition: all 0.2s;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  text-decoration: none;
}

.btn-primary {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
}

.btn-secondary {
  background: #edf2f7;
  color: #4a5568;
  border: 1px solid #e2e8f0;
}

.btn-danger {
  background: #e53e3e;
  color: white;
}

.btn:hover {
  opacity: 0.9;
  transform: translateY(-1px);
}

.btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
  transform: none;
}

/* Form Styles */
.form-group {
  margin-bottom: 1rem;
}

.form-label {
  display: block;
  margin-bottom: 0.5rem;
  font-weight: 500;
  color: #2d3748;
}

.form-input {
  width: 100%;
  padding: 0.75rem;
  border: 1px solid #e2e8f0;
  border-radius: 6px;
  font-size: 1rem;
  transition: border-color 0.2s, box-shadow 0.2s;
}

.form-input:focus {
  outline: none;
  border-color: #667eea;
  box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
}

/* Card Styles */
.card {
  background: white;
  border-radius: 8px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  padding: 1.5rem;
}

/* Error/Success Messages */
.error {
  color: #e53e3e;
  font-size: 0.875rem;
}

.error-message {
  background-color: #fed7d7;
  border: 1px solid #feb2b2;
  color: #9b2c2c;
  padding: 0.75rem;
  border-radius: 6px;
  margin: 1rem 0;
}

.success-message {
  background-color: #c6f6d5;
  border: 1px solid #9ae6b4;
  color: #276749;
  padding: 0.75rem;
  border-radius: 6px;
  margin: 1rem 0;
}

/* Loading Spinner */
.loading {
  display: inline-block;
  width: 20px;
  height: 20px;
  border: 2px solid #e2e8f0;
  border-radius: 50%;
  border-top-color: #667eea;
  animation: spin 1s ease-in-out infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

/* Responsive */
@media (max-width: 640px) {
  .container {
    padding: 0 1rem;
  }
  
  .card {
    margin: 1rem;
    padding: 1rem;
  }
}
</style>