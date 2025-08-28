import type {
    CreateDoctorScheduleRequest,
    DoctorSchedule,
    PaginatedResponse,
    UpdateDoctorScheduleRequest
} from '@/types/doctorSchedule'

// API base URL from environment variables
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:8080'

// Helper function to get auth token
function getAuthToken(): string | null {
  return localStorage.getItem('authToken')
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

  if (body) {
    options.body = JSON.stringify(body)
  }

  return options
}

// Helper function to handle fetch responses
async function handleResponse<T>(response: Response): Promise<T> {
  if (!response.ok) {
    const errorText = await response.text()
    let errorMessage = `HTTP ${response.status}: ${response.statusText}`
    
    try {
      const errorData = JSON.parse(errorText)
      errorMessage = errorData.message || errorData.title || errorMessage
    } catch {
      // If not JSON, use the text or default message
      errorMessage = errorText || errorMessage
    }
    
    throw new Error(errorMessage)
  }

  const text = await response.text()
  return text ? JSON.parse(text) : ({} as T)
}

export class DoctorScheduleService {
  private static readonly BASE_PATH = '/api/doctorschedules'

  /**
   * Create a new doctor schedule
   */
  static async createSchedule(request: CreateDoctorScheduleRequest): Promise<DoctorSchedule> {
    const response = await fetch(
      `${API_BASE_URL}${this.BASE_PATH}`,
      createFetchOptions('POST', request)
    )
    const result = await handleResponse<any>(response)
    
    // Handle wrapped response format {success: true, result: {...}}
    if (result && typeof result === 'object' && 'success' in result && 'result' in result) {
      return result.result as DoctorSchedule
    }
    
    // Handle direct response format
    return result as DoctorSchedule
  }

  /**
   * Get a specific doctor schedule by ID
   */
  static async getSchedule(id: string): Promise<DoctorSchedule> {
    const response = await fetch(
      `${API_BASE_URL}${this.BASE_PATH}/${id}`,
      createFetchOptions('GET')
    )
    return handleResponse<DoctorSchedule>(response)
  }

  /**
   * Get all schedules for a specific doctor
   */
  static async getDoctorSchedules(doctorId: string): Promise<DoctorSchedule[]> {
    const response = await fetch(
      `${API_BASE_URL}${this.BASE_PATH}/doctor/${doctorId}`,
      createFetchOptions('GET')
    )
    return handleResponse<DoctorSchedule[]>(response)
  }

  /**
   * Get active schedules for a specific doctor
   */
  static async getActiveDoctorSchedules(doctorId: string): Promise<DoctorSchedule[]> {
    const response = await fetch(
      `${API_BASE_URL}${this.BASE_PATH}/doctor/${doctorId}/active`,
      createFetchOptions('GET')
    )
    return handleResponse<DoctorSchedule[]>(response)
  }

  /**
   * Update an existing doctor schedule
   */
  static async updateSchedule(id: string, request: UpdateDoctorScheduleRequest): Promise<DoctorSchedule> {
    const response = await fetch(
      `${API_BASE_URL}${this.BASE_PATH}/${id}`,
      createFetchOptions('PUT', request)
    )
    const result = await handleResponse<any>(response)
    
    // Handle wrapped response format {success: true, result: {...}}
    if (result && typeof result === 'object' && 'success' in result && 'result' in result) {
      return result.result as DoctorSchedule
    }
    
    // Handle direct response format
    return result as DoctorSchedule
  }

  /**
   * Delete a doctor schedule (soft delete)
   */
  static async deleteSchedule(id: string): Promise<void> {
    const response = await fetch(
      `${API_BASE_URL}${this.BASE_PATH}/${id}`,
      createFetchOptions('DELETE')
    )
    await handleResponse<void>(response)
  }

  /**
   * Get paginated doctor schedules with optional filters
   */
  static async getPaginatedSchedules(
    page: number = 1,
    pageSize: number = 10,
    doctorId?: string,
    isActive?: boolean
  ): Promise<PaginatedResponse<DoctorSchedule>> {
    const params = new URLSearchParams({
      page: page.toString(),
      pageSize: pageSize.toString()
    })

    if (doctorId) params.append('doctorId', doctorId)
    if (isActive !== undefined) params.append('isActive', isActive.toString())

    const response = await fetch(
      `${API_BASE_URL}${this.BASE_PATH}?${params}`,
      createFetchOptions('GET')
    )
    
    const data = await handleResponse<DoctorSchedule[]>(response)
    
    // Extract pagination info from headers
    const totalCount = parseInt(response.headers.get('x-total-count') || '0')
    const currentPage = parseInt(response.headers.get('x-page') || '1')
    const currentPageSize = parseInt(response.headers.get('x-page-size') || '10')

    return {
      data,
      totalCount,
      page: currentPage,
      pageSize: currentPageSize
    }
  }

  /**
   * Check if a doctor has any conflicting schedules
   */
  static async hasConflictingSchedule(
    doctorId: string,
    dayOfWeek: number,
    startTime: string,
    endTime: string,
    excludeScheduleId?: string
  ): Promise<boolean> {
    try {
      const schedules = await this.getDoctorSchedules(doctorId)
      
      return schedules.some(schedule => {
        if (excludeScheduleId && schedule.id === excludeScheduleId) return false
        if (schedule.dayOfWeek !== dayOfWeek) return false
        if (!schedule.isActive) return false

        // Check for time overlap
        const existingStart = schedule.startTime
        const existingEnd = schedule.endTime

        return (
          (startTime >= existingStart && startTime < existingEnd) ||
          (endTime > existingStart && endTime <= existingEnd) ||
          (startTime <= existingStart && endTime >= existingEnd)
        )
      })
    } catch (error) {
      console.error('Error checking for conflicting schedules:', error)
      return false
    }
  }
}
