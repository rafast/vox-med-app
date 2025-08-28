<template>
  <div class="dashboard">
    <header class="dashboard-header">
      <div class="header-content">
        <h1>VoxMed Dashboard</h1>
        <div class="user-info">
          <span>Welcome, {{ authStore.userName }}</span>
          <button @click="handleLogout" class="logout-button">
            Logout
          </button>
        </div>
      </div>
    </header>

    <main class="dashboard-main">
      <div class="dashboard-grid">
        <!-- Quick Stats -->
        <div class="stats-grid">
          <div class="stat-card">
            <div class="stat-icon">👥</div>
            <div class="stat-content">
              <h3>Total Patients</h3>
              <p class="stat-number">{{ stats.totalPatients }}</p>
            </div>
          </div>
          
          <div class="stat-card">
            <div class="stat-icon">📅</div>
            <div class="stat-content">
              <h3>Today's Appointments</h3>
              <p class="stat-number">{{ stats.todayAppointments }}</p>
            </div>
          </div>
          
          <div class="stat-card">
            <div class="stat-icon">⏰</div>
            <div class="stat-content">
              <h3>Pending Appointments</h3>
              <p class="stat-number">{{ stats.pendingAppointments }}</p>
            </div>
          </div>
          
          <div class="stat-card">
            <div class="stat-icon">👨‍⚕️</div>
            <div class="stat-content">
              <h3>Active Doctors</h3>
              <p class="stat-number">{{ stats.activeDoctors }}</p>
            </div>
          </div>
        </div>

        <!-- Recent Activity -->
        <div class="activity-section">
          <h2>Recent Activity</h2>
          <div class="activity-list">
            <div v-for="activity in recentActivity" :key="activity.id" class="activity-item">
              <div class="activity-icon" :class="activity.type">
                {{ getActivityIcon(activity.type) }}
              </div>
              <div class="activity-content">
                <p class="activity-text">{{ activity.description }}</p>
                <small class="activity-time">{{ formatTime(activity.timestamp) }}</small>
              </div>
            </div>
          </div>
        </div>

        <!-- Quick Actions -->
        <div class="actions-section">
          <h2>Quick Actions</h2>
          <div class="action-buttons">
            <button 
              @click="router.push('/schedules')"
              class="action-button primary">
              <span class="action-icon">📅</span>
              Manage Schedules
            </button>
            <button class="action-button secondary">
              <span class="action-icon">👤</span>
              New Patient
            </button>
            <button class="action-button secondary">
              <span class="action-icon">�</span>
              Schedule Appointment
            </button>
            <button class="action-button secondary">
              <span class="action-icon">📋</span>
              View Medical Records
            </button>
            <button class="action-button secondary">
              <span class="action-icon">👨‍⚕️</span>
              Manage Staff
            </button>
          </div>
        </div>
      </div>
    </main>
  </div>
</template>

<script setup lang="ts">
import { useAuthStore } from '@/stores/auth'
import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'

const router = useRouter()
const authStore = useAuthStore()

// Dashboard stats
const stats = ref({
  totalPatients: 0,
  todayAppointments: 0,
  pendingAppointments: 0,
  activeDoctors: 0
})

// Recent activity
interface Activity {
  id: number
  type: 'appointment' | 'patient' | 'record' | 'user'
  description: string
  timestamp: Date
}

const recentActivity = ref<Activity[]>([])

// Load dashboard data
const loadDashboardData = async () => {
  // Mock data - replace with actual API calls
  stats.value = {
    totalPatients: 1247,
    todayAppointments: 18,
    pendingAppointments: 7,
    activeDoctors: 12
  }

  recentActivity.value = [
    {
      id: 1,
      type: 'patient',
      description: 'New patient Emma Wilson registered',
      timestamp: new Date(Date.now() - 1000 * 60 * 15) // 15 minutes ago
    },
    {
      id: 2,
      type: 'appointment',
      description: 'Appointment scheduled for Robert Davis',
      timestamp: new Date(Date.now() - 1000 * 60 * 30) // 30 minutes ago
    },
    {
      id: 3,
      type: 'record',
      description: 'Medical record updated for Alice Cooper',
      timestamp: new Date(Date.now() - 1000 * 60 * 45) // 45 minutes ago
    },
    {
      id: 4,
      type: 'user',
      description: 'Dr. Sarah Johnson logged in',
      timestamp: new Date(Date.now() - 1000 * 60 * 60) // 1 hour ago
    }
  ]
}

// Handle logout
const handleLogout = () => {
  authStore.logout()
  router.push('/login')
}

// Helper functions
const getActivityIcon = (type: string): string => {
  const icons = {
    appointment: '📅',
    patient: '👤',
    record: '📋',
    user: '👨‍⚕️'
  }
  return icons[type as keyof typeof icons] || '📌'
}

const formatTime = (timestamp: Date): string => {
  const now = new Date()
  const diff = now.getTime() - timestamp.getTime()
  const minutes = Math.floor(diff / (1000 * 60))
  const hours = Math.floor(minutes / 60)
  const days = Math.floor(hours / 24)

  if (days > 0) return `${days} day${days > 1 ? 's' : ''} ago`
  if (hours > 0) return `${hours} hour${hours > 1 ? 's' : ''} ago`
  if (minutes > 0) return `${minutes} minute${minutes > 1 ? 's' : ''} ago`
  return 'Just now'
}

// Load data on component mount
onMounted(() => {
  loadDashboardData()
})
</script>

<style scoped>
.dashboard {
  min-height: 100vh;
  background-color: #f7fafc;
}

.dashboard-header {
  background: white;
  border-bottom: 1px solid #e2e8f0;
  padding: 1rem 0;
  position: sticky;
  top: 0;
  z-index: 10;
}

.header-content {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 1rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.dashboard-header h1 {
  font-size: 1.5rem;
  font-weight: bold;
  color: #2d3748;
}

.user-info {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.user-info span {
  color: #4a5568;
}

.logout-button {
  background: #e53e3e;
  color: white;
  border: none;
  padding: 0.5rem 1rem;
  border-radius: 4px;
  cursor: pointer;
  font-size: 0.875rem;
  transition: background-color 0.2s;
}

.logout-button:hover {
  background: #c53030;
}

.dashboard-main {
  max-width: 1200px;
  margin: 0 auto;
  padding: 2rem 1rem;
}

.dashboard-grid {
  display: grid;
  gap: 2rem;
  grid-template-columns: 1fr;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 1rem;
}

.stat-card {
  background: white;
  padding: 1.5rem;
  border-radius: 8px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  display: flex;
  align-items: center;
  gap: 1rem;
}

.stat-icon {
  font-size: 2rem;
  width: 60px;
  height: 60px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #edf2f7;
  border-radius: 50%;
}

.stat-content h3 {
  font-size: 0.875rem;
  color: #718096;
  margin-bottom: 0.25rem;
}

.stat-number {
  font-size: 2rem;
  font-weight: bold;
  color: #2d3748;
  margin: 0;
}

.activity-section,
.actions-section {
  background: white;
  padding: 1.5rem;
  border-radius: 8px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
}

.activity-section h2,
.actions-section h2 {
  font-size: 1.25rem;
  font-weight: 600;
  color: #2d3748;
  margin-bottom: 1rem;
}

.activity-list {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.activity-item {
  display: flex;
  align-items: flex-start;
  gap: 0.75rem;
  padding: 0.75rem;
  border-radius: 6px;
  background: #f7fafc;
}

.activity-icon {
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 50%;
  font-size: 1rem;
}

.activity-icon.appointment { background: #bee3f8; }
.activity-icon.patient { background: #c6f6d5; }
.activity-icon.record { background: #fed7d7; }
.activity-icon.user { background: #e9d8fd; }

.activity-content {
  flex: 1;
}

.activity-text {
  margin: 0 0 0.25rem 0;
  color: #2d3748;
  font-size: 0.875rem;
}

.activity-time {
  color: #718096;
  font-size: 0.75rem;
}

.action-buttons {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1rem;
}

.action-button {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 1rem;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  font-size: 0.875rem;
  font-weight: 500;
  transition: all 0.2s;
}

.action-button.primary {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
}

.action-button.secondary {
  background: #edf2f7;
  color: #4a5568;
  border: 1px solid #e2e8f0;
}

.action-button:hover {
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
}

.action-icon {
  font-size: 1.25rem;
}

/* Responsive design */
@media (min-width: 768px) {
  .dashboard-grid {
    grid-template-columns: 2fr 1fr;
  }
  
  .activity-section {
    grid-column: 1;
  }
  
  .actions-section {
    grid-column: 2;
  }
}

@media (max-width: 640px) {
  .stats-grid {
    grid-template-columns: 1fr;
  }
  
  .action-buttons {
    grid-template-columns: 1fr;
  }
  
  .header-content {
    flex-direction: column;
    gap: 1rem;
    text-align: center;
  }
}
</style>
