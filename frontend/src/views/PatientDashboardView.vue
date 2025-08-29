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
                  <h4>{{ AppointmentType[appointment.type] }}</h4>
                  <span class="appointment-status" :class="getStatusBadgeClass(appointment.status)">
                    {{ formatStatus(appointment.status) }}
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

    <!-- Create/Edit Appointment Modal -->
    <div v-if="showCreateModal || showEditModal" class="modal-overlay" @click="closeModal">
      <div class="modal-content" @click.stop>
        <div class="modal-header">
          <h3>{{ showEditModal ? 'Edit Appointment' : 'Book New Appointment' }}</h3>
          <button @click="closeModal" class="modal-close">×</button>
        </div>
        
        <form @submit.prevent="saveAppointment" class="modal-body">
          <div class="form-group">
            <label for="appointmentType">Appointment Type</label>
            <select id="appointmentType" v-model="appointmentForm.type" required>
              <option value="">Select appointment type</option>
              <option value="Consultation">Consultation</option>
              <option value="FollowUp">Follow-up</option>
              <option value="Checkup">Check-up</option>
              <option value="Emergency">Emergency</option>
              <option value="Vaccination">Vaccination</option>
              <option value="LabWork">Lab Work</option>
              <option value="Imaging">Imaging</option>
              <option value="Therapy">Therapy</option>
            </select>
          </div>

          <div class="form-group">
            <label for="appointmentDate">Date & Time</label>
            <input 
              id="appointmentDate" 
              type="datetime-local" 
              v-model="appointmentForm.appointmentDateTime" 
              required
            >
          </div>

          <div class="form-group">
            <label for="duration">Duration (minutes)</label>
            <select id="duration" v-model="appointmentForm.durationMinutes" required>
              <option value="30">30 minutes</option>
              <option value="45">45 minutes</option>
              <option value="60">1 hour</option>
              <option value="90">1.5 hours</option>
              <option value="120">2 hours</option>
            </select>
          </div>

          <div class="form-group">
            <label for="doctorId">Doctor *</label>
            <select id="doctorId" v-model="appointmentForm.doctorId" required>
              <option value="">Select a doctor</option>
              <template v-if="doctors.length > 0">
                <option v-for="doctor in doctors" :key="doctor.id" :value="doctor.id">
                  {{ doctor.name }}<span v-if="doctor.specialty"> - {{ doctor.specialty }}</span>
                </option>
              </template>
              <template v-else>
                <option disabled value="">No doctors available</option>
              </template>
            </select>
          </div>

          <div class="form-group">
            <label class="checkbox-label">
              <input type="checkbox" v-model="appointmentForm.isOnline">
              Online appointment (Telemedicine)
            </label>
          </div>

          <div class="form-group">
            <label for="reason">Reason for Visit</label>
            <textarea 
              id="reason" 
              v-model="appointmentForm.reason" 
              placeholder="Please describe the reason for your appointment..."
              rows="3"
            ></textarea>
          </div>

          <div class="form-group">
            <label for="symptoms">Symptoms (Optional)</label>
            <textarea 
              id="symptoms" 
              v-model="appointmentForm.symptoms" 
              placeholder="Describe any symptoms you're experiencing..."
              rows="3"
            ></textarea>
          </div>

          <div class="modal-footer">
            <button type="button" @click="closeModal" class="btn-secondary">Cancel</button>
            <button type="submit" class="btn-primary" :disabled="isSubmitting">
              {{ isSubmitting ? 'Saving...' : (showEditModal ? 'Update Appointment' : 'Book Appointment') }}
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { appointmentsApi } from '@/services/appointments'
import { useAuthStore } from '@/stores/auth'
import { AppointmentType, type Appointment } from '@/types/appointment'
import axios from 'axios'
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
const apiBaseUrl: string = import.meta.env.VITE_API_BASE_URL

const router = useRouter()
const authStore = useAuthStore()

// State
const appointments = ref<Appointment[]>([])
const loading = ref(true)
const showCreateModal = ref(false)
const showEditModal = ref(false)
const selectedAppointment = ref<Appointment | null>(null)
const isSubmitting = ref(false)

// Doctor list state
const doctors = ref<Array<{ id: string, name: string, specialty?: string }>>([])

async function loadDoctors() {
  try {
    // Replace with your actual API endpoint if different
    const response = await axios.get(`${apiBaseUrl}/api/Doctors`)
    doctors.value = response.data
  } catch (error) {
    console.error('Failed to load doctors:', error)
    doctors.value = []
  }
}

// Appointment form data
const appointmentForm = ref({
  type: '' as AppointmentType | '',
  appointmentDateTime: '',
  durationMinutes: 30,
  doctorId: '',
  reason: '',
  symptoms: '',
  isOnline: false
})

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
// Map numeric status to string
function formatStatus(status: string | number): string {
  const statusMap: Record<number | string, string> = {
    0: 'Pending',
    1: 'Scheduled',
    2: 'Confirmed',
    3: 'InProgress',
    4: 'Completed',
    5: 'Cancelled',
    6: 'NoShow',
    7: 'Rescheduled',
    'Pending': 'Pending',
    'Scheduled': 'Scheduled',
    'Confirmed': 'Confirmed',
    'InProgress': 'In Progress',
    'Completed': 'Completed',
    'Cancelled': 'Cancelled',
    'NoShow': 'No Show',
    'Rescheduled': 'Rescheduled'
  }
  return statusMap[status] || String(status)
}
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

function formatAppointmentType(type: AppointmentType): number {
  // If type is a number, return as is
  if (typeof type === 'number') return type;
  // If type is a string, map to int
  const typeMap: Record<string, number> = {
    'Consultation': 0,
    'FollowUp': 1,
    'Emergency': 2,
    'Procedure': 3,
    'Surgery': 4,
    'Checkup': 5,
    'Vaccination': 6,
    'LabWork': 7,
    'Imaging': 8,
    'Therapy': 9
  };
  return typeMap[type] ?? 0;
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

async function saveAppointment() {
  try {
    isSubmitting.value = true

    const patientId = authStore.user?.id
    if (!patientId) {
      console.error('No patient ID found')
      return
    }

    // Validate required fields
    if (!appointmentForm.value.type || !appointmentForm.value.appointmentDateTime || !appointmentForm.value.doctorId) {
      console.error('Required fields missing')
      return
    }

    if (showEditModal.value && selectedAppointment.value) {
      // Update existing appointment
      const updateData = {
        appointmentDateTime: appointmentForm.value.appointmentDateTime + 'Z', // Add UTC timezone
        durationMinutes: Number(appointmentForm.value.durationMinutes),
        type: appointmentForm.value.type as AppointmentType,
        reason: appointmentForm.value.reason || undefined,
        symptoms: appointmentForm.value.symptoms || undefined
      }
      await appointmentsApi.update(selectedAppointment.value.id, updateData)
    } else {
      // Create new appointment
      const createData = {
        patientId,
        doctorId: appointmentForm.value.doctorId,
        appointmentDateTime: appointmentForm.value.appointmentDateTime + 'Z', // Add UTC timezone
        durationMinutes: Number(appointmentForm.value.durationMinutes),
        type: formatAppointmentType(appointmentForm.value.type),
        reason: appointmentForm.value.reason || undefined,
        symptoms: appointmentForm.value.symptoms || undefined,
        isOnline: appointmentForm.value.isOnline
      }
      try {
        await appointmentsApi.create(createData)
      } catch (error: any) {         
        const errorMessage = typeof error === 'string' ? error : (error?.message || JSON.stringify(error));
        const message = extractApiErrorMessage(errorMessage);
        if (errorMessage.includes('400')) {
          alert('Failed to create appointment. Error:' + message);
          return;
        }        
          alert('Failed to create appointment. Please try again.')
        return;
      }
    }

    // Reset form
    resetForm()
    closeModal()
    await loadAppointments() // Refresh the list
  } catch (error) {
    console.error('Failed to save appointment:', error)
    alert('Failed to save appointment. Please try again.' + error)
  } finally {
    isSubmitting.value = false
  }
}

function extractApiErrorMessage(errorString: string): string | null {
  // Match the JSON part after the dash
  const match = errorString.match(/-\s*(\{.*\})$/);
  if (match && match[1]) {
    try {
      const errorObj = JSON.parse(match[1]);
      return errorObj.error || null;
    } catch {
      return null;
    }
  }
  return null;
}

function resetForm() {
  appointmentForm.value = {
    type: '' as AppointmentType | '',
    appointmentDateTime: '',
    durationMinutes: 30,
    doctorId: '',
    reason: '',
    symptoms: '',
    isOnline: false
  }
}

function closeModal() {
  showCreateModal.value = false
  showEditModal.value = false
  selectedAppointment.value = null
  resetForm()
}

async function handleSave() {
  closeModal()
  await loadAppointments() // Refresh the list
}

async function handleLogout() {
  await authStore.logout()
  router.push('/login')
}

// Lifecycle
onMounted(() => {
  loadAppointments()
  loadDoctors()
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

/* Modal Styles */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  padding: 1rem;
}

.modal-content {
  background: white;
  border-radius: 12px;
  box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04);
  max-width: 500px;
  width: 100%;
  max-height: 90vh;
  overflow-y: auto;
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1.5rem 1.5rem 1rem;
  border-bottom: 1px solid #e2e8f0;
}

.modal-header h3 {
  font-size: 1.25rem;
  font-weight: 600;
  color: #1a202c;
  margin: 0;
}

.modal-close {
  background: none;
  border: none;
  font-size: 1.5rem;
  cursor: pointer;
  color: #718096;
  padding: 0;
  width: 24px;
  height: 24px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.modal-close:hover {
  color: #2d3748;
}

.modal-body {
  padding: 1.5rem;
}

.form-group {
  margin-bottom: 1rem;
}

.form-group label {
  display: block;
  font-size: 0.875rem;
  font-weight: 500;
  color: #374151;
  margin-bottom: 0.5rem;
}

.form-group input,
.form-group select,
.form-group textarea {
  width: 100%;
  padding: 0.75rem;
  border: 1px solid #d1d5db;
  border-radius: 6px;
  font-size: 0.875rem;
  transition: border-color 0.2s, box-shadow 0.2s;
}

.form-group input:focus,
.form-group select:focus,
.form-group textarea:focus {
  outline: none;
  border-color: #3b82f6;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}

.form-group textarea {
  resize: vertical;
  min-height: 80px;
}

.modal-footer {
  display: flex;
  justify-content: flex-end;
  gap: 0.75rem;
  padding-top: 1rem;
  border-top: 1px solid #e2e8f0;
}

.btn-primary,
.btn-secondary {
  padding: 0.75rem 1.5rem;
  border-radius: 6px;
  font-size: 0.875rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
  border: none;
}

.btn-primary {
  background-color: #3b82f6;
  color: white;
}

.btn-primary:hover:not(:disabled) {
  background-color: #2563eb;
}

.btn-primary:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.btn-secondary {
  background-color: #f3f4f6;
  color: #374151;
  border: 1px solid #d1d5db;
}

.btn-secondary:hover {
  background-color: #e5e7eb;
}

.checkbox-label {
  display: flex !important;
  align-items: center;
  gap: 0.5rem;
  cursor: pointer;
  font-weight: normal !important;
}

.checkbox-label input[type="checkbox"] {
  width: auto !important;
  margin: 0;
}
</style>
