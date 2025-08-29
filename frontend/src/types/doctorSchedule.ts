// API types for Doctor Schedules (matching backend DTOs)
export interface DoctorSchedule {
  id: string
  doctorId: string
  dayOfWeek: number // 0 = Sunday, 1 = Monday, etc.
  startTime: string // "09:00"
  endTime: string // "17:00"
  slotDurationMinutes: number
  effectiveFrom?: string // "2025-08-28"
  effectiveTo?: string
  isActive: boolean
  dateAdded: string
  dateUpdated: string
  totalSlotsPerDay: number
}

export interface CreateDoctorScheduleRequest {
  doctorId: string
  dayOfWeek: number
  startTime: string
  endTime: string
  slotDurationMinutes: number
  effectiveFrom?: string
  effectiveTo?: string
  isActive: boolean
}

export interface UpdateDoctorScheduleRequest {
  startTime: string
  endTime: string
  slotDurationMinutes: number
  effectiveFrom?: string
  effectiveTo?: string
  isActive: boolean
}

// Day of week mapping
export const DAYS_OF_WEEK = [
  { value: 0, label: 'Sunday', short: 'Sun' },
  { value: 1, label: 'Monday', short: 'Mon' },
  { value: 2, label: 'Tuesday', short: 'Tue' },
  { value: 3, label: 'Wednesday', short: 'Wed' },
  { value: 4, label: 'Thursday', short: 'Thu' },
  { value: 5, label: 'Friday', short: 'Fri' },
  { value: 6, label: 'Saturday', short: 'Sat' }
]

// API responses
export interface ApiResponse<T> {
  data?: T
  message?: string
  errors?: string[]
}

export interface PaginatedResponse<T> {
  data: T[]
  totalCount: number
  page: number
  pageSize: number
}


export interface ApiError {
  error: string;
}