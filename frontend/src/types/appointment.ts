export interface Appointment {
  id: string
  patientId: string
  doctorId: string
  scheduleId?: string
  appointmentDateTime: string
  durationMinutes: number
  type: AppointmentType
  status: AppointmentStatus
  reason?: string
  notes?: string
  symptoms?: string
  diagnosis?: string
  treatment?: string
  isOnline: boolean
  location?: string
  createdAt: string
  updatedAt: string
}

export interface CreateAppointmentRequest {
  doctorId: string
  patientId: string
  scheduleId?: string
  appointmentDateTime: string
  durationMinutes: number
  type: AppointmentType
  reason?: string
  notes?: string
  symptoms?: string
  diagnosis?: string
  treatment?: string
  isOnline: boolean
  location?: string
}

export interface UpdateAppointmentRequest {
  appointmentDateTime: string
  durationMinutes: number
  type: AppointmentType
  reason?: string
  notes?: string
  symptoms?: string
  diagnosis?: string
  treatment?: string
}

export enum AppointmentType {
  Consultation = 'Consultation',
  FollowUp = 'FollowUp',
  Emergency = 'Emergency',
  Procedure = 'Procedure',
  Surgery = 'Surgery',
  Checkup = 'Checkup',
  Vaccination = 'Vaccination',
  LabWork = 'LabWork',
  Imaging = 'Imaging',
  Therapy = 'Therapy'
}

export enum AppointmentStatus {
  Pending = 'Pending',
  Scheduled = 'Scheduled',
  Confirmed = 'Confirmed',
  InProgress = 'InProgress',
  Completed = 'Completed',
  Cancelled = 'Cancelled',
  NoShow = 'NoShow',
  Rescheduled = 'Rescheduled'
}

export interface TimeSlot {
  startTime: string
  endTime: string
  durationMinutes: number
  isAvailable: boolean
}

export interface Doctor {
  id: string
  name: string
  specialization: string
  email: string
  phone?: string
}
