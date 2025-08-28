import { DoctorScheduleService } from '@/services/doctorScheduleService'
import type {
    CreateDoctorScheduleRequest,
    DoctorSchedule,
    UpdateDoctorScheduleRequest
} from '@/types/doctorSchedule'
import { defineStore } from 'pinia'
import { computed, ref } from 'vue'

export const useDoctorScheduleStore = defineStore('doctorSchedule', () => {
  // State
  const schedules = ref<DoctorSchedule[]>([])
  const currentSchedule = ref<DoctorSchedule | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  // Computed
  const activeSchedules = computed(() => 
    schedules.value.filter(schedule => schedule.isActive)
  )

  const schedulesByDay = computed(() => {
    const grouped: Record<number, DoctorSchedule[]> = {}
    schedules.value.forEach(schedule => {
      if (!grouped[schedule.dayOfWeek]) {
        grouped[schedule.dayOfWeek] = []
      }
      grouped[schedule.dayOfWeek].push(schedule)
    })
    return grouped
  })

  const totalSlots = computed(() => 
    schedules.value.reduce((total, schedule) => total + schedule.totalSlotsPerDay, 0)
  )

  // Actions
  async function fetchDoctorSchedules(doctorId: string) {
    loading.value = true
    error.value = null
    
    try {
      const fetchedSchedules = await DoctorScheduleService.getDoctorSchedules(doctorId)
      // Sort by day of week for consistent display
      schedules.value = fetchedSchedules.sort((a, b) => a.dayOfWeek - b.dayOfWeek)
    } catch (err: any) {
      error.value = err.response?.data?.message || 'Failed to fetch doctor schedules'
      console.error('Error fetching doctor schedules:', err)
    } finally {
      loading.value = false
    }
  }

  async function fetchActiveDoctorSchedules(doctorId: string) {
    loading.value = true
    error.value = null
    
    try {
      schedules.value = await DoctorScheduleService.getActiveDoctorSchedules(doctorId)
    } catch (err: any) {
      error.value = err.response?.data?.message || 'Failed to fetch active doctor schedules'
      console.error('Error fetching active doctor schedules:', err)
    } finally {
      loading.value = false
    }
  }

  async function fetchSchedule(id: string) {
    loading.value = true
    error.value = null
    
    try {
      currentSchedule.value = await DoctorScheduleService.getSchedule(id)
    } catch (err: any) {
      error.value = err.response?.data?.message || 'Failed to fetch schedule'
      console.error('Error fetching schedule:', err)
    } finally {
      loading.value = false
    }
  }

  async function createSchedule(request: CreateDoctorScheduleRequest): Promise<DoctorSchedule | null> {
    loading.value = true
    error.value = null
    
    try {
      console.log('🔍 Starting createSchedule with request:', request)
      
      // Check for conflicts before creating
      const hasConflict = await DoctorScheduleService.hasConflictingSchedule(
        request.doctorId,
        request.dayOfWeek,
        request.startTime,
        request.endTime
      )

      if (hasConflict) {
        error.value = `Schedule conflict detected for ${getDayName(request.dayOfWeek)}. Please choose different times.`
        console.log('❌ Schedule conflict detected')
        return null
      }

      console.log('✅ No conflicts, creating schedule...')
      const newSchedule = await DoctorScheduleService.createSchedule(request)
      console.log('✅ Schedule created successfully:', newSchedule)
      console.log('✅ Create result:', JSON.stringify(newSchedule, null, 2))
      
      // Add to local state immediately for better UX and sort by day of week
      const initialLength = schedules.value.length
      schedules.value.push(newSchedule)
      schedules.value.sort((a, b) => a.dayOfWeek - b.dayOfWeek)
      console.log(`📝 Added to local state. Length: ${initialLength} → ${schedules.value.length}`)
      
      return newSchedule
    } catch (err: any) {
      error.value = err.response?.data?.message || 'Failed to create schedule'
      console.error('❌ Error creating schedule:', err)
      return null
    } finally {
      loading.value = false
    }
  }

  async function updateSchedule(id: string, request: UpdateDoctorScheduleRequest): Promise<DoctorSchedule | null> {
    loading.value = true
    error.value = null
    
    try {
      const updatedSchedule = await DoctorScheduleService.updateSchedule(id, request)
      
      // Update in local state
      const index = schedules.value.findIndex(s => s.id === id)
      if (index !== -1) {
        schedules.value[index] = updatedSchedule
      }
      
      if (currentSchedule.value?.id === id) {
        currentSchedule.value = updatedSchedule
      }
      
      return updatedSchedule
    } catch (err: any) {
      error.value = err.response?.data?.message || 'Failed to update schedule'
      console.error('Error updating schedule:', err)
      return null
    } finally {
      loading.value = false
    }
  }

  async function deleteSchedule(id: string): Promise<boolean> {
    loading.value = true
    error.value = null
    
    try {
      await DoctorScheduleService.deleteSchedule(id)
      
      // Remove from local state
      schedules.value = schedules.value.filter(s => s.id !== id)
      
      if (currentSchedule.value?.id === id) {
        currentSchedule.value = null
      }
      
      return true
    } catch (err: any) {
      error.value = err.response?.data?.message || 'Failed to delete schedule'
      console.error('Error deleting schedule:', err)
      return false
    } finally {
      loading.value = false
    }
  }

  async function checkConflict(
    doctorId: string,
    dayOfWeek: number,
    startTime: string,
    endTime: string,
    excludeScheduleId?: string
  ): Promise<boolean> {
    try {
      return await DoctorScheduleService.hasConflictingSchedule(
        doctorId,
        dayOfWeek,
        startTime,
        endTime,
        excludeScheduleId
      )
    } catch (err) {
      console.error('Error checking schedule conflict:', err)
      return false
    }
  }

  function clearError() {
    error.value = null
  }

  function clearSchedules() {
    schedules.value = []
    currentSchedule.value = null
    error.value = null
  }

  // Helper function
  function getDayName(dayOfWeek: number): string {
    const days = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday']
    return days[dayOfWeek] || 'Unknown'
  }

  return {
    // State
    schedules,
    currentSchedule,
    loading,
    error,
    
    // Computed
    activeSchedules,
    schedulesByDay,
    totalSlots,
    
    // Actions
    fetchDoctorSchedules,
    fetchActiveDoctorSchedules,
    fetchSchedule,
    createSchedule,
    updateSchedule,
    deleteSchedule,
    checkConflict,
    clearError,
    clearSchedules,
    getDayName
  }
})
