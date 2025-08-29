<template>
  <div class="min-h-screen bg-gray-50">
    <!-- Header -->
    <div class="bg-white shadow">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="flex justify-between items-center py-6">
          <div class="flex items-center">
            <div class="flex-shrink-0">
              <h1 class="text-2xl font-bold text-gray-900">
                {{ authStore.isPatient ? 'Patient Dashboard' : 'Appointments Dashboard' }}
              </h1>
              <p v-if="authStore.isPatient" class="text-sm text-gray-600 mt-1">
                Welcome back, {{ authStore.userName }}
              </p>
            </div>
          </div>
          <div class="flex items-center space-x-4">
            <button
              v-if="authStore.isPatient"
              @click="showCreateModal = true"
              class="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-md text-sm font-medium flex items-center"
            >
              <svg class="h-5 w-5 mr-2" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
              </svg>
              Book Appointment
            </button>
            <button
              @click="router.push('/dashboard')"
              v-if="authStore.isDoctor"
              class="bg-gray-100 hover:bg-gray-200 text-gray-700 px-4 py-2 rounded-md text-sm font-medium"
            >
              Back to Dashboard
            </button>
            <button
              @click="authStore.logout()"
              class="bg-gray-100 hover:bg-gray-200 text-gray-700 px-4 py-2 rounded-md text-sm font-medium"
            >
              Logout
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Main Content -->
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <!-- Stats Cards -->
      <div class="grid grid-cols-1 md:grid-cols-4 gap-6 mb-8">
        <div class="bg-white overflow-hidden shadow rounded-lg">
          <div class="p-5">
            <div class="flex items-center">
              <div class="flex-shrink-0">
                <svg class="h-6 w-6 text-blue-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                </svg>
              </div>
              <div class="ml-5 w-0 flex-1">
                <dl>
                  <dt class="text-sm font-medium text-gray-500 truncate">
                    Upcoming Appointments
                  </dt>
                  <dd class="text-lg font-medium text-gray-900">
                    {{ upcomingCount }}
                  </dd>
                </dl>
              </div>
            </div>
          </div>
        </div>

        <div class="bg-white overflow-hidden shadow rounded-lg">
          <div class="p-5">
            <div class="flex items-center">
              <div class="flex-shrink-0">
                <svg class="h-6 w-6 text-yellow-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
              </div>
              <div class="ml-5 w-0 flex-1">
                <dl>
                  <dt class="text-sm font-medium text-gray-500 truncate">
                    Pending Appointments
                  </dt>
                  <dd class="text-lg font-medium text-gray-900">
                    {{ pendingCount }}
                  </dd>
                </dl>
              </div>
            </div>
          </div>
        </div>

        <div class="bg-white overflow-hidden shadow rounded-lg">
          <div class="p-5">
            <div class="flex items-center">
              <div class="flex-shrink-0">
                <svg class="h-6 w-6 text-green-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
              </div>
              <div class="ml-5 w-0 flex-1">
                <dl>
                  <dt class="text-sm font-medium text-gray-500 truncate">
                    Completed This Month
                  </dt>
                  <dd class="text-lg font-medium text-gray-900">
                    {{ completedCount }}
                  </dd>
                </dl>
              </div>
            </div>
          </div>
        </div>

        <div class="bg-white overflow-hidden shadow rounded-lg">
          <div class="p-5">
            <div class="flex items-center">
              <div class="flex-shrink-0">
                <svg class="h-6 w-6 text-purple-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z" />
                </svg>
              </div>
              <div class="ml-5 w-0 flex-1">
                <dl>
                  <dt class="text-sm font-medium text-gray-500 truncate">
                    Total Appointments
                  </dt>
                  <dd class="text-lg font-medium text-gray-900">
                    {{ appointments.length }}
                  </dd>
                </dl>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Appointments Section -->
      <div class="mb-8">
        <div class="flex items-center justify-between mb-6">
          <div>
            <h2 class="text-2xl font-bold text-gray-900">My Appointments</h2>
            <p class="mt-1 text-sm text-gray-600">View and manage your upcoming and past appointments</p>
          </div>
          <div class="flex space-x-3">
            <button class="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-blue-500">
              Filter
            </button>
            <button class="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-blue-500">
              Sort
            </button>
          </div>
        </div>

        <!-- Loading State -->
        <div v-if="loading" class="text-center py-12">
          <div class="inline-block animate-spin rounded-full h-10 w-10 border-b-2 border-blue-600"></div>
          <p class="mt-4 text-sm text-gray-500">Loading your appointments...</p>
        </div>

        <!-- Empty State -->
        <div v-else-if="appointments.length === 0" class="text-center py-16 bg-white rounded-xl shadow-sm border border-gray-200">
          <div class="max-w-sm mx-auto">
            <div class="bg-blue-50 rounded-full w-20 h-20 flex items-center justify-center mx-auto mb-4">
              <svg class="h-10 w-10 text-blue-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
              </svg>
            </div>
            <h3 class="text-lg font-semibold text-gray-900 mb-2">No appointments yet</h3>
            <p class="text-gray-500 mb-6">Get started by booking your first appointment with one of our healthcare professionals.</p>
            <button
              @click="showCreateModal = true"
              class="inline-flex items-center px-6 py-3 border border-transparent text-base font-medium rounded-lg shadow-sm text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 transition-colors duration-200"
            >
              <svg class="h-5 w-5 mr-2" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
              </svg>
              Book Your First Appointment
            </button>
          </div>
        </div>

        <!-- Appointments Grid -->
        <div v-else class="grid grid-cols-1 lg:grid-cols-2 gap-6">
          <div 
            v-for="appointment in appointments" 
            :key="appointment.id"
            class="bg-white rounded-xl shadow-sm border border-gray-200 hover:shadow-md transition-shadow duration-200 overflow-hidden"
          >
            <!-- Card Header -->
            <div class="p-6 pb-4">
              <div class="flex items-start justify-between">
                <div class="flex items-center space-x-4">
                  <div 
                    class="flex-shrink-0 w-12 h-12 rounded-full flex items-center justify-center ring-2 ring-white"
                    :class="getStatusColor(appointment.status)"
                  >
                    <svg class="h-6 w-6 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path v-if="getStatusIcon(appointment.status) === 'clock'" stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
                      <path v-else-if="getStatusIcon(appointment.status) === 'calendar'" stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                      <path v-else-if="getStatusIcon(appointment.status) === 'check-circle'" stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                      <path v-else-if="getStatusIcon(appointment.status) === 'x-circle'" stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 14l2-2m0 0l2-2m-2 2l-2-2m2 2l2 2m7-2a9 9 0 11-18 0 9 9 0 0118 0z" />
                      <path v-else stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L3.232 16.5c-.77.833.192 2.5 1.732 2.5z" />
                    </svg>
                  </div>
                  <div>
                    <h3 class="text-lg font-semibold text-gray-900">
                      {{ formatAppointmentType(appointment.type) }}
                    </h3>
                    <div class="flex items-center mt-1">
                      <span 
                        class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium capitalize"
                        :class="getStatusBadgeColor(appointment.status)"
                      >
                        {{ appointment.status }}
                      </span>
                    </div>
                  </div>
                </div>
                <div class="flex space-x-2">
                  <button
                    v-if="canEdit(appointment)"
                    @click="editAppointment(appointment)"
                    class="p-2 text-gray-400 hover:text-blue-500 hover:bg-blue-50 rounded-lg transition-colors duration-200"
                    title="Edit appointment"
                  >
                    <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15.232 5.232l3.536 3.536m-2.036-5.036a2.5 2.5 0 113.536 3.536L6.5 21.036H3v-3.572L16.732 3.732z" />
                    </svg>
                  </button>
                  <button
                    v-if="canCancel(appointment)"
                    @click="cancelAppointment(appointment)"
                    class="p-2 text-gray-400 hover:text-red-500 hover:bg-red-50 rounded-lg transition-colors duration-200"
                    title="Cancel appointment"
                  >
                    <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
                    </svg>
                  </button>
                </div>
              </div>
            </div>

            <!-- Card Body -->
            <div class="px-6 pb-6">
              <div class="space-y-3">
                <!-- Date and Time -->
                <div class="flex items-center text-gray-600">
                  <svg class="flex-shrink-0 mr-3 h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                  </svg>
                  <span class="text-sm font-medium">{{ formatDateTime(appointment.appointmentDateTime) }}</span>
                </div>

                <!-- Duration -->
                <div class="flex items-center text-gray-600">
                  <svg class="flex-shrink-0 mr-3 h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
                  </svg>
                  <span class="text-sm">{{ appointment.durationMinutes }} minutes</span>
                </div>

                <!-- Doctor -->
                <div class="flex items-center text-gray-600">
                  <svg class="flex-shrink-0 mr-3 h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                  </svg>
                  <span class="text-sm">Doctor ID: {{ appointment.doctorId }}</span>
                </div>

                <!-- Reason -->
                <div v-if="appointment.reason" class="flex items-start text-gray-600">
                  <svg class="flex-shrink-0 mr-3 h-4 w-4 mt-0.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                  </svg>
                  <span class="text-sm">{{ appointment.reason }}</span>
                </div>
              </div>
            </div>

            <!-- Card Footer -->
            <div class="px-6 py-4 bg-gray-50 border-t border-gray-200">
              <div class="flex items-center justify-between">
                <div class="flex items-center space-x-4">
                  <span v-if="appointment.symptoms" class="text-xs text-gray-500">
                    Symptoms reported
                  </span>
                  <span v-if="appointment.diagnosis" class="text-xs text-gray-500">
                    Diagnosed
                  </span>
                </div>
                <div class="flex space-x-3">
                  <button
                    v-if="canEdit(appointment)"
                    @click="editAppointment(appointment)"
                    class="text-sm font-medium text-blue-600 hover:text-blue-700 transition-colors duration-200"
                  >
                    Edit Details
                  </button>
                  <button
                    v-if="appointment.status === 'Scheduled'"
                    class="text-sm font-medium text-green-600 hover:text-green-700 transition-colors duration-200"
                  >
                    Join Now
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

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
import { ref, onMounted, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { appointmentsApi } from '@/services/appointments'
import type { Appointment, AppointmentStatus, AppointmentType } from '@/types/appointment'
// import AppointmentModal from '@/components/AppointmentModal.vue'

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

function getStatusColor(status: string): string {
  const colorMap: Record<string, string> = {
    'Pending': 'bg-yellow-500',
    'Scheduled': 'bg-blue-500',
    'Confirmed': 'bg-green-500',
    'InProgress': 'bg-purple-500',
    'Completed': 'bg-gray-500',
    'Cancelled': 'bg-red-500',
    'NoShow': 'bg-red-500',
    'Rescheduled': 'bg-orange-500'
  }
  return colorMap[status] || 'bg-gray-500'
}

function getStatusBadgeColor(status: string): string {
  const colorMap: Record<string, string> = {
    'Pending': 'bg-yellow-100 text-yellow-800',
    'Scheduled': 'bg-blue-100 text-blue-800',
    'Confirmed': 'bg-green-100 text-green-800',
    'InProgress': 'bg-purple-100 text-purple-800',
    'Completed': 'bg-gray-100 text-gray-800',
    'Cancelled': 'bg-red-100 text-red-800',
    'NoShow': 'bg-red-100 text-red-800',
    'Rescheduled': 'bg-orange-100 text-orange-800'
  }
  return colorMap[status] || 'bg-gray-100 text-gray-800'
}

function getStatusIcon(status: string) {
  // For now, return simple SVG class names
  const iconMap: Record<string, string> = {
    'Pending': 'clock',
    'Scheduled': 'calendar',
    'Confirmed': 'check-circle',
    'InProgress': 'clock',
    'Completed': 'check-circle',
    'Cancelled': 'x-circle',
    'NoShow': 'x-circle',
    'Rescheduled': 'exclamation-triangle'
  }
  return iconMap[status] || 'calendar'
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

// Lifecycle
onMounted(() => {
  loadAppointments()
})
</script>
