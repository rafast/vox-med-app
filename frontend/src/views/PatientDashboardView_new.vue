<template>
  <div class="dashboard">
    <header class="dashboard-header">
      <div class="header-content">
        <h1>Patient Dashboard</h1>
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
            <div class="stat-icon">📅</div>
            <div class="stat-content">
              <h3>Upcoming Appointments</h3>
              <p class="stat-number">{{ upcomingCount }}</p>
            </div>
          </div>
          
          <div class="stat-card">
            <div class="stat-icon">⏰</div>
            <div class="stat-content">
              <h3>Pending Appointments</h3>
              <p class="stat-number">{{ pendingCount }}</p>
            </div>
          </div>
          
          <div class="stat-card">
            <div class="stat-icon">✅</div>
            <div class="stat-content">
              <h3>Completed This Month</h3>
              <p class="stat-number">{{ completedCount }}</p>
            </div>
          </div>
          
          <div class="stat-card">
            <div class="stat-icon">📋</div>
            <div class="stat-content">
              <h3>Total Appointments</h3>
              <p class="stat-number">{{ appointments.length }}</p>
            </div>
          </div>
        </div>

        <!-- Recent Appointments -->
        <div class="activity-section">
          <h2>Recent Appointments</h2>
          
          <!-- Loading State -->
          <div v-if="loading" class="loading-state">
            <div class="loading-spinner"></div>
            <p>Loading your appointments...</p>
          </div>

          <!-- Empty State -->
          <div v-else-if="appointments.length === 0" class="empty-state">
            <div class="empty-icon">📅</div>
            <h3>No appointments yet</h3>
            <p>Get started by booking your first appointment with one of our healthcare professionals.</p>
            <button @click="showCreateModal = true" class="action-button primary">
              <span class="action-icon">📅</span>
              Book Your First Appointment
            </button>
          </div>

          <!-- Appointments List -->
          <div v-else class="activity-list">
            <div 
              v-for="appointment in recentAppointments" 
              :key="appointment.id" 
              class="activity-item appointment-item"
            >
              <div class="activity-icon" :class="getStatusClass(appointment.status)">
                {{ getStatusEmoji(appointment.status) }}
              </div>
              <div class="activity-content">
                <div class="appointment-header">
                  <h4>{{ formatAppointmentType(appointment.type.toString()) }}</h4>
                  <span class="appointment-status" :class="getStatusBadgeClass(appointment.status)">
                    {{ appointment.status }}
                  </span>
                </div>
                <p class="appointment-details">
                  {{ formatDateTime(appointment.appointmentDateTime) }} • {{ appointment.durationMinutes }} min
                </p>
                <p v-if="appointment.reason" class="appointment-reason">
                  {{ appointment.reason }}
                </p>
                <div class="appointment-actions">
                  <button
                    v-if="canEdit(appointment)"
                    @click="editAppointment(appointment)"
                    class="action-link edit"
                  >
                    Edit
                  </button>
                  <button
                    v-if="canCancel(appointment)"
                    @click="cancelAppointment(appointment)"
                    class="action-link cancel"
                  >
                    Cancel
                  </button>
                  <button
                    v-if="appointment.status === 'Scheduled'"
                    class="action-link join"
                  >
                    Join Now
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Quick Actions -->
        <div class="actions-section">
          <h2>Quick Actions</h2>
          <div class="action-buttons">
            <button 
              @click="showCreateModal = true"
              class="action-button primary">
              <span class="action-icon">📅</span>
              Book Appointment
            </button>
            <button 
              @click="router.push('/dashboard')"
              class="action-button primary">
              <span class="action-icon">🏠</span>
              Main Dashboard
            </button>
            <button class="action-button secondary">
              <span class="action-icon">📋</span>
              View Medical Records
            </button>
            <button class="action-button secondary">
              <span class="action-icon">👨‍⚕️</span>
              Find Doctors
            </button>
            <button class="action-button secondary">
              <span class="action-icon">💊</span>
              Prescriptions
            </button>
            <button class="action-button secondary">
              <span class="action-icon">📊</span>
              Health Reports
            </button>
          </div>
        </div>
      </div>
    </main>

    <!-- TODO: Create/Edit Appointment Modal -->
    <!-- <AppointmentModal
      v-if="showCreateModal || showEditModal"
      :appointment="selectedAppointment"
      :is-edit="showEditModal"
      @close="closeModal"
      @save="handleSave"
    /> -->
  </div>
</template>

<script setup lang="ts">
import { appointmentsApi } from '@/services/appointments'
import { useAuthStore } from '@/stores/auth'
import type { Appointment } from '@/types/appointment'
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'

const router = useRouter()
const authStore = useAuthStore()

// State
const appointments = ref<Appointment[]>([])
const loading = ref(true)
const showCreateModal = ref(false)
const showEditModal = ref(false)
const selectedAppointment = ref<Appointment | null>(null)

// Computed properties
const upcomingCount = computed(() => 
  appointments.value.filter(apt => 
    apt.status === 'Scheduled' || apt.status === 'Confirmed'
  ).length
)

const pendingCount = computed(() => 
  appointments.value.filter(apt => apt.status === 'Pending').length
)

const completedCount = computed(() => {
  const currentMonth = new Date().getMonth()
  return appointments.value.filter(apt => 
    apt.status === 'Completed' && 
    new Date(apt.appointmentDateTime).getMonth() === currentMonth
  ).length
})

const recentAppointments = computed(() => 
  appointments.value.slice(0, 5) // Show only recent 5 appointments
)

// Methods
async function loadAppointments() {
  try {
    loading.value = true
    
    // Get patient ID from authenticated user
    const patientId = authStore.user?.id
    if (!patientId) {
      console.error('No patient ID found in auth store')
      return
    }
    
    appointments.value = await appointmentsApi.getPatientAppointments(patientId)
  } catch (error) {
    console.error('Failed to load appointments:', error)
    // In a real app, show a toast notification
  } finally {
    loading.value = false
  }
}

function formatDateTime(dateTime: string): string {
  return new Date(dateTime).toLocaleString('en-US', {
    weekday: 'short',
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

function formatAppointmentType(type: string): string {
  const typeMap: Record<string, string> = {
    'Consultation': 'Consultation',
    'FollowUp': 'Follow-up',
    'Emergency': 'Emergency',
    'Procedure': 'Procedure',
    'Surgery': 'Surgery',
    'Checkup': 'Check-up',
    'Vaccination': 'Vaccination',
    'LabWork': 'Lab Work',
    'Imaging': 'Imaging',
    'Therapy': 'Therapy'
  }
  return typeMap[type] || type
}

function getStatusClass(status: string): string {
  const classMap: Record<string, string> = {
    'Pending': 'pending',
    'Scheduled': 'appointment',
    'Confirmed': 'patient',
    'InProgress': 'user',
    'Completed': 'record',
    'Cancelled': 'user',
    'NoShow': 'user',
    'Rescheduled': 'appointment'
  }
  return classMap[status] || 'record'
}

function getStatusEmoji(status: string): string {
  const emojiMap: Record<string, string> = {
    'Pending': '⏳',
    'Scheduled': '📅',
    'Confirmed': '✅',
    'InProgress': '🔄',
    'Completed': '✅',
    'Cancelled': '❌',
    'NoShow': '❌',
    'Rescheduled': '🔄'
  }
  return emojiMap[status] || '📋'
}

function getStatusBadgeClass(status: string): string {
  const classMap: Record<string, string> = {
    'Pending': 'status-pending',
    'Scheduled': 'status-scheduled',
    'Confirmed': 'status-confirmed',
    'InProgress': 'status-progress',
    'Completed': 'status-completed',
    'Cancelled': 'status-cancelled',
    'NoShow': 'status-cancelled',
    'Rescheduled': 'status-pending'
  }
  return classMap[status] || 'status-pending'
}

function canEdit(appointment: Appointment): boolean {
  return ['Pending', 'Scheduled', 'Confirmed'].includes(appointment.status)
}

function canCancel(appointment: Appointment): boolean {
  return ['Pending', 'Scheduled', 'Confirmed'].includes(appointment.status)
}

function editAppointment(appointment: Appointment) {
  selectedAppointment.value = appointment
  showEditModal.value = true
}

async function cancelAppointment(appointment: Appointment) {
  try {
    await appointmentsApi.cancel(appointment.id, 'Cancelled by patient')
    await loadAppointments() // Refresh the list
  } catch (error) {
    console.error('Failed to cancel appointment:', error)
  }
}

function closeModal() {
  showCreateModal.value = false
  showEditModal.value = false
  selectedAppointment.value = null
}

async function handleSave() {
  closeModal()
  await loadAppointments() // Refresh the list
}

function handleLogout() {
  authStore.logout()
  router.push('/login')
}

// Lifecycle
onMounted(() => {
  loadAppointments()
})
</script>

<style scoped>
/* Dashboard Layout */
.dashboard {
  min-height: 100vh;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  display: flex;
  flex-direction: column;
}

.dashboard-header {
  background: white;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
  border-bottom: 1px solid #e2e8f0;
}

.header-content {
  max-width: 1200px;
  margin: 0 auto;
  padding: 1.5rem 2rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.header-content h1 {
  font-size: 2rem;
  font-weight: 700;
  color: #2d3748;
  margin: 0;
}

.user-info {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.user-info span {
  color: #4a5568;
  font-weight: 500;
}

.logout-button {
  background: #e53e3e;
  color: white;
  border: none;
  padding: 0.5rem 1rem;
  border-radius: 0.5rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
}

.logout-button:hover {
  background: #c53030;
  transform: translateY(-1px);
}

/* Main Dashboard */
.dashboard-main {
  flex: 1;
  padding: 2rem;
  max-width: 1200px;
  margin: 0 auto;
  width: 100%;
}

.dashboard-grid {
  display: grid;
  gap: 2rem;
  grid-template-columns: 1fr;
}

/* Stats Grid */
.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 1.5rem;
  margin-bottom: 2rem;
}

.stat-card {
  background: white;
  border-radius: 1rem;
  padding: 1.5rem;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);
  display: flex;
  align-items: center;
  gap: 1rem;
  transition: all 0.3s ease;
}

.stat-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 30px rgba(0, 0, 0, 0.15);
}

.stat-icon {
  font-size: 2.5rem;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border-radius: 50%;
  width: 60px;
  height: 60px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
}

.stat-content h3 {
  font-size: 0.9rem;
  color: #718096;
  margin: 0 0 0.5rem 0;
  font-weight: 500;
}

.stat-number {
  font-size: 2rem;
  font-weight: 700;
  color: #2d3748;
  margin: 0;
}

/* Activity Section */
.activity-section, .actions-section {
  background: white;
  border-radius: 1rem;
  padding: 1.5rem;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);
}

.activity-section h2, .actions-section h2 {
  font-size: 1.5rem;
  font-weight: 600;
  color: #2d3748;
  margin: 0 0 1.5rem 0;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

/* Loading State */
.loading-state {
  text-align: center;
  padding: 3rem;
}

.loading-spinner {
  width: 40px;
  height: 40px;
  border: 4px solid #e2e8f0;
  border-top: 4px solid #667eea;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin: 0 auto 1rem;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

/* Empty State */
.empty-state {
  text-align: center;
  padding: 3rem;
}

.empty-icon {
  font-size: 4rem;
  margin-bottom: 1rem;
  opacity: 0.6;
}

.empty-state h3 {
  font-size: 1.25rem;
  font-weight: 600;
  color: #2d3748;
  margin-bottom: 0.5rem;
}

.empty-state p {
  color: #718096;
  margin-bottom: 2rem;
  max-width: 400px;
  margin-left: auto;
  margin-right: auto;
}

/* Activity List */
.activity-list {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.activity-item {
  display: flex;
  align-items: flex-start;
  gap: 1rem;
  padding: 1rem;
  border-radius: 0.75rem;
  transition: all 0.2s;
  border: 1px solid #e2e8f0;
}

.activity-item:hover {
  background: #f7fafc;
  border-color: #cbd5e0;
}

.activity-icon {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.2rem;
  flex-shrink: 0;
}

.activity-icon.appointment { background: #e6fffa; }
.activity-icon.patient { background: #f0fff4; }
.activity-icon.record { background: #f7fafc; }
.activity-icon.user { background: #fef5e7; }
.activity-icon.pending { background: #fffbeb; }

.activity-content {
  flex: 1;
  min-width: 0;
}

.appointment-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 0.5rem;
}

.appointment-header h4 {
  font-size: 1.1rem;
  font-weight: 600;
  color: #2d3748;
  margin: 0;
}

.appointment-status {
  padding: 0.25rem 0.75rem;
  border-radius: 1rem;
  font-size: 0.75rem;
  font-weight: 500;
  text-transform: capitalize;
}

.status-pending { background: #fffbeb; color: #92400e; }
.status-scheduled { background: #dbeafe; color: #1e40af; }
.status-confirmed { background: #d1fae5; color: #065f46; }
.status-progress { background: #e0e7ff; color: #3730a3; }
.status-completed { background: #f3f4f6; color: #374151; }
.status-cancelled { background: #fee2e2; color: #991b1b; }

.appointment-details {
  color: #718096;
  font-size: 0.9rem;
  margin: 0 0 0.5rem 0;
}

.appointment-reason {
  color: #4a5568;
  font-size: 0.9rem;
  margin: 0 0 1rem 0;
  font-style: italic;
}

.appointment-actions {
  display: flex;
  gap: 1rem;
}

.action-link {
  background: none;
  border: none;
  font-size: 0.875rem;
  font-weight: 500;
  cursor: pointer;
  padding: 0;
}

.action-link.edit { color: #3182ce; }
.action-link.edit:hover { color: #2c5aa0; }
.action-link.cancel { color: #e53e3e; }
.action-link.cancel:hover { color: #c53030; }
.action-link.join { color: #38a169; }
.action-link.join:hover { color: #2f855a; }

/* Action Buttons */
.action-buttons {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1rem;
}

.action-button {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 1rem 1.5rem;
  border: none;
  border-radius: 0.75rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
  text-decoration: none;
  font-size: 0.95rem;
}

.action-button.primary {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  box-shadow: 0 4px 15px rgba(102, 126, 234, 0.4);
}

.action-button.primary:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 25px rgba(102, 126, 234, 0.6);
}

.action-button.secondary {
  background: #f8f9fa;
  color: #495057;
  border: 1px solid #e2e8f0;
}

.action-button.secondary:hover {
  background: #e9ecef;
  transform: translateY(-1px);
}

.action-icon {
  font-size: 1.2rem;
}

/* Responsive Design */
@media (max-width: 768px) {
  .dashboard-main {
    padding: 1rem;
  }
  
  .header-content {
    padding: 1rem;
    flex-direction: column;
    gap: 1rem;
    align-items: flex-start;
  }
  
  .stats-grid {
    grid-template-columns: 1fr;
  }
  
  .action-buttons {
    grid-template-columns: 1fr;
  }
  
  .appointment-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 0.5rem;
  }
}

@media (min-width: 1024px) {
  .dashboard-grid {
    grid-template-columns: 2fr 1fr;
  }
  
  .stats-grid {
    grid-column: 1 / -1;
  }
}
</style>
