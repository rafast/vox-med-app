import { useAuthStore } from '@/stores/auth'
import { createRouter, createWebHistory } from 'vue-router'

// Extend the RouteMeta interface to include our custom properties
declare module 'vue-router' {
  interface RouteMeta {
    requiresAuth?: boolean
    requiresGuest?: boolean
    allowedRoles?: string[]
  }
}

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      redirect: '/login'
    },
    {
      path: '/login',
      name: 'login',
      component: () => import('../views/LoginView.vue'),
      meta: { requiresGuest: true }
    },
    {
      path: '/dashboard',
      name: 'dashboard',
      component: () => import('../views/DashboardView.vue'),
      meta: { requiresAuth: true, allowedRoles: ['Doctor'] }
    },
    {
      path: '/schedules',
      name: 'doctor-schedules',
      component: () => import('../views/DoctorSchedulesView.vue'),
      meta: { requiresAuth: true, allowedRoles: ['Doctor'] }
    },
    {
      path: '/patients',
      name: 'patient-dashboard',
      component: () => import('../views/PatientDashboardView.vue'),
      meta: { requiresAuth: true, allowedRoles: ['Patient', 'Doctor'] }
    },
    {
      path: '/about',
      name: 'about',
      component: () => import('../views/AboutView.vue'),
    },
  ],
})

// Navigation guard for authentication and authorization
router.beforeEach((to, from, next) => {
  const authStore = useAuthStore()
  
  // Check if route requires authentication
  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    // Redirect to login with return url
    next({ 
      path: '/login', 
      query: { redirect: to.fullPath } 
    })
    return
  }
  
  // Check if route requires guest (not authenticated)
  if (to.meta.requiresGuest && authStore.isAuthenticated) {
    // Redirect based on user type
    if (authStore.isPatient) {
      next('/patients')
    } else {
      next('/dashboard')
    }
    return
  }
  
  // Check role-based access
  if (to.meta.allowedRoles && authStore.isAuthenticated) {
    const userRole = authStore.userRole
    const allowedRoles = to.meta.allowedRoles as string[]
    
    if (!allowedRoles.includes(userRole)) {
      // Redirect to appropriate dashboard based on user role
      if (authStore.isPatient) {
        next('/patients')
      } else if (authStore.isDoctor) {
        next('/dashboard')
      } else {
        next('/login')
      }
      return
    }
  }
  
  next()
})

export default router
