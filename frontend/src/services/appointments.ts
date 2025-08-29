import type {
    Appointment,
    AppointmentStatus,
    CreateAppointmentRequest,
    TimeSlot,
    UpdateAppointmentRequest
} from '@/types/appointment'

// API base URL from environment variables
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:8080'

// Helper function to get auth token
function getAuthToken(): string | null {
  return localStorage.getItem('voxmed_token')
}

// Helper function to create fetch options
function createFetchOptions(method: string = 'GET', body?: any): RequestInit {
  const headers: HeadersInit = {
    'Content-Type': 'application/json'
  }

  const token = getAuthToken()
  if (token) {
    headers['Authorization'] = `Bearer ${token}`
  }

  const options: RequestInit = {
    method,
    headers
  }

  if (body && (method === 'POST' || method === 'PUT' || method === 'PATCH')) {
    options.body = JSON.stringify(body)
  }

  return options
}

// Helper function to handle API responses
async function handleResponse<T>(response: Response): Promise<T> {
  if (!response.ok) {
    const errorText = await response.text()
    throw new Error(`API Error: ${response.status} - ${errorText}`)
  }
  
  // Handle empty responses (like DELETE)
  if (response.status === 204) {
    return undefined as any
  }
  
  return response.json()
}

export const appointmentsApi = {
  // Get all appointments
  async getAll(): Promise<Appointment[]> {
    const response = await fetch(`${API_BASE_URL}/api/appointments`, createFetchOptions())
    return handleResponse<Appointment[]>(response)
  },

  // Get appointment by ID
  async getById(id: string): Promise<Appointment> {
    const response = await fetch(`${API_BASE_URL}/api/appointments/${id}`, createFetchOptions())
    return handleResponse<Appointment>(response)
  },

  // Create new appointment
  async create(appointment: CreateAppointmentRequest): Promise<Appointment> {
    const response = await fetch(
      `${API_BASE_URL}/api/appointments`, 
      createFetchOptions('POST', appointment)
    )
    return handleResponse<Appointment>(response)
  },

  // Update appointment
  async update(id: string, appointment: UpdateAppointmentRequest): Promise<Appointment> {
    const response = await fetch(
      `${API_BASE_URL}/api/appointments/${id}`, 
      createFetchOptions('PUT', appointment)
    )
    return handleResponse<Appointment>(response)
  },

  // Delete appointment
  async delete(id: string): Promise<void> {
    const response = await fetch(
      `${API_BASE_URL}/api/appointments/${id}`, 
      createFetchOptions('DELETE')
    )
    return handleResponse<void>(response)
  },

  // Confirm appointment
  async confirm(id: string): Promise<Appointment> {
    const response = await fetch(
      `${API_BASE_URL}/api/appointments/${id}/confirm`, 
      createFetchOptions('PATCH')
    )
    return handleResponse<Appointment>(response)
  },

  // Cancel appointment
  async cancel(id: string, reason?: string): Promise<Appointment> {
    const response = await fetch(
      `${API_BASE_URL}/api/appointments/${id}/cancel`, 
      createFetchOptions('PATCH', { cancellationReason: reason })
    )
    return handleResponse<Appointment>(response)
  },

  // Complete appointment
  async complete(id: string, notes?: string): Promise<Appointment> {
    const response = await fetch(
      `${API_BASE_URL}/api/appointments/${id}/complete`, 
      createFetchOptions('PATCH', { notes })
    )
    return handleResponse<Appointment>(response)
  },

  // Get doctor's appointments
  async getDoctorAppointments(doctorId: string): Promise<Appointment[]> {
    const response = await fetch(
      `${API_BASE_URL}/api/appointments/doctor/${doctorId}`, 
      createFetchOptions()
    )
    return handleResponse<Appointment[]>(response)
  },

  // Get patient's appointments
  async getPatientAppointments(patientId: string): Promise<Appointment[]> {
    const response = await fetch(
      `${API_BASE_URL}/api/appointments/patient/${patientId}`, 
      createFetchOptions()
    )
    return handleResponse<Appointment[]>(response)
  },

  // Get appointments by date range
  async getByDateRange(startDate: string, endDate: string): Promise<Appointment[]> {
    const response = await fetch(
      `${API_BASE_URL}/api/appointments/date-range?startDate=${startDate}&endDate=${endDate}`, 
      createFetchOptions()
    )
    return handleResponse<Appointment[]>(response)
  },

  // Get doctor's appointments by date range
  async getDoctorAppointmentsByDateRange(
    doctorId: string, 
    startDate: string, 
    endDate: string
  ): Promise<Appointment[]> {
    const response = await fetch(
      `${API_BASE_URL}/api/appointments/doctor/${doctorId}/date-range?startDate=${startDate}&endDate=${endDate}`, 
      createFetchOptions()
    )
    return handleResponse<Appointment[]>(response)
  },

  // Get upcoming patient appointments
  async getUpcomingPatientAppointments(patientId: string): Promise<Appointment[]> {
    const response = await fetch(
      `${API_BASE_URL}/api/appointments/patient/${patientId}/upcoming`, 
      createFetchOptions()
    )
    return handleResponse<Appointment[]>(response)
  },

  // Get upcoming doctor appointments
  async getUpcomingDoctorAppointments(doctorId: string): Promise<Appointment[]> {
    const response = await fetch(
      `${API_BASE_URL}/api/appointments/doctor/${doctorId}/upcoming`, 
      createFetchOptions()
    )
    return handleResponse<Appointment[]>(response)
  },

  // Get appointments by status
  async getByStatus(status: AppointmentStatus): Promise<Appointment[]> {
    const response = await fetch(
      `${API_BASE_URL}/api/appointments/status/${status}`, 
      createFetchOptions()
    )
    return handleResponse<Appointment[]>(response)
  },

  // Get today's appointments for a doctor
  async getTodayAppointments(doctorId: string): Promise<Appointment[]> {
    const response = await fetch(
      `${API_BASE_URL}/api/appointments/doctor/${doctorId}/today`, 
      createFetchOptions()
    )
    return handleResponse<Appointment[]>(response)
  },

  // Get pending appointments
  async getPendingAppointments(): Promise<Appointment[]> {
    const response = await fetch(
      `${API_BASE_URL}/api/appointments/pending`, 
      createFetchOptions()
    )
    return handleResponse<Appointment[]>(response)
  },

  // Get available time slots
  async getAvailableTimeSlots(doctorId: string, date: string): Promise<TimeSlot[]> {
    const response = await fetch(
      `${API_BASE_URL}/api/appointments/doctor/${doctorId}/available-slots?date=${date}`, 
      createFetchOptions()
    )
    return handleResponse<TimeSlot[]>(response)
  }
}
