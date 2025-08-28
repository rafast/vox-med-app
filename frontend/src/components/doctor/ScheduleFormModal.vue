<template>
  <div class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
    <div class="bg-white rounded-lg shadow-xl max-w-lg w-full max-h-[90vh] overflow-y-auto">
      <form @submit.prevent="handleSubmit">
        <!-- Header -->
        <div class="flex justify-between items-center p-6 border-b border-gray-200">
          <h2 class="text-xl font-semibold text-gray-900">
            {{ isEditing ? 'Edit Schedule' : 'Create Schedule' }}
          </h2>
          <button
            type="button"
            @click="$emit('close')"
            class="text-gray-400 hover:text-gray-600 transition-colors"
          >
            ✕
          </button>
        </div>

        <!-- Form Content -->
        <div class="p-6 space-y-4">
          <!-- Error Message -->
          <div v-if="error" class="bg-red-50 border border-red-200 text-red-600 px-4 py-3 rounded-lg">
            {{ error }}
          </div>

          <!-- Day of Week -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2">
              Day of Week *
            </label>
            <select
              v-model="form.dayOfWeek"
              :disabled="isEditing"
              class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 disabled:bg-gray-100 disabled:cursor-not-allowed"
              required
            >
              <option value="">Select a day</option>
              <option
                v-for="day in DAYS_OF_WEEK"
                :key="day.value"
                :value="day.value"
              >
                {{ day.label }}
              </option>
            </select>
            <p v-if="isEditing" class="text-xs text-gray-500 mt-1">
              Day cannot be changed when editing
            </p>
          </div>

          <!-- Time Range -->
          <div class="grid grid-cols-2 gap-4">
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-2">
                Start Time *
              </label>
              <input
                v-model="form.startTime"
                type="time"
                class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                required
              />
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-2">
                End Time *
              </label>
              <input
                v-model="form.endTime"
                type="time"
                class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                required
              />
            </div>
          </div>

          <!-- Slot Duration -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2">
              Slot Duration (minutes) *
            </label>
            <select
              v-model.number="form.slotDurationMinutes"
              class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
              required
            >
              <option :value="15">15 minutes</option>
              <option :value="30">30 minutes</option>
              <option :value="45">45 minutes</option>
              <option :value="60">1 hour</option>
              <option :value="90">1.5 hours</option>
              <option :value="120">2 hours</option>
            </select>
          </div>

          <!-- Effective Date Range -->
          <div class="grid grid-cols-2 gap-4">
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-2">
                Effective From
              </label>
              <input
                v-model="form.effectiveFrom"
                type="date"
                class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
              />
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-2">
                Effective Until
              </label>
              <input
                v-model="form.effectiveTo"
                type="date"
                class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
              />
            </div>
          </div>

          <!-- Active Status -->
          <div class="flex items-center">
            <input
              v-model="form.isActive"
              type="checkbox"
              id="isActive"
              class="w-4 h-4 text-blue-600 bg-gray-100 border-gray-300 rounded focus:ring-blue-500 focus:ring-2"
            />
            <label for="isActive" class="ml-2 text-sm font-medium text-gray-700">
              Schedule is active
            </label>
          </div>

          <!-- Schedule Preview -->
          <div v-if="form.startTime && form.endTime && form.slotDurationMinutes" class="bg-blue-50 p-4 rounded-lg">
            <h4 class="text-sm font-medium text-blue-900 mb-2">Schedule Preview</h4>
            <div class="text-sm text-blue-700">
              <p><strong>Day:</strong> {{ selectedDayName }}</p>
              <p><strong>Time:</strong> {{ form.startTime }} - {{ form.endTime }}</p>
              <p><strong>Duration:</strong> {{ totalHours }} hours</p>
              <p><strong>Slots:</strong> {{ calculateSlots() }} slots of {{ form.slotDurationMinutes }} minutes each</p>
            </div>
          </div>
        </div>

        <!-- Footer -->
        <div class="flex justify-end gap-3 p-6 border-t border-gray-200">
          <button
            type="button"
            @click="$emit('close')"
            class="px-4 py-2 text-gray-700 bg-gray-100 hover:bg-gray-200 rounded-lg transition-colors"
          >
            Cancel
          </button>
          <button
            type="submit"
            :disabled="loading || !isFormValid"
            class="px-4 py-2 bg-blue-600 hover:bg-blue-700 disabled:bg-gray-300 disabled:cursor-not-allowed text-white rounded-lg transition-colors flex items-center gap-2"
          >
            <div v-if="loading" class="animate-spin rounded-full h-4 w-4 border-b-2 border-white"></div>
            {{ loading ? 'Saving...' : (isEditing ? 'Update' : 'Create') }}
          </button>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useDoctorScheduleStore } from '@/stores/doctorSchedule'
import type { CreateDoctorScheduleRequest, DoctorSchedule, UpdateDoctorScheduleRequest } from '@/types/doctorSchedule'
import { DAYS_OF_WEEK } from '@/types/doctorSchedule'
import { computed, onMounted, ref, watch } from 'vue'

// Props
interface Props {
  schedule?: DoctorSchedule | null
  doctorId: string
}
const props = defineProps<Props>()

// Emits
const emit = defineEmits<{
  close: []
  saved: []
}>()

// Store
const scheduleStore = useDoctorScheduleStore()

// Reactive data
const loading = ref(false)
const error = ref<string | null>(null)

// Form data
const form = ref({
  dayOfWeek: null as number | null,
  startTime: '',
  endTime: '',
  slotDurationMinutes: 30,
  effectiveFrom: '',
  effectiveTo: '',
  isActive: true
})

// Computed
const isEditing = computed(() => !!props.schedule)

const selectedDayName = computed(() => {
  if (form.value.dayOfWeek === null) return ''
  return DAYS_OF_WEEK.find(day => day.value === form.value.dayOfWeek)?.label || ''
})

const totalHours = computed(() => {
  if (!form.value.startTime || !form.value.endTime) return 0
  const start = new Date(`2000-01-01 ${form.value.startTime}`)
  const end = new Date(`2000-01-01 ${form.value.endTime}`)
  return Math.round(((end.getTime() - start.getTime()) / (1000 * 60 * 60)) * 100) / 100
})

const isFormValid = computed(() => {
  return form.value.dayOfWeek !== null &&
         form.value.startTime &&
         form.value.endTime &&
         form.value.slotDurationMinutes > 0 &&
         form.value.startTime < form.value.endTime
})

// Methods
function calculateSlots(): number {
  if (!form.value.startTime || !form.value.endTime || !form.value.slotDurationMinutes) return 0
  
  const start = new Date(`2000-01-01 ${form.value.startTime}`)
  const end = new Date(`2000-01-01 ${form.value.endTime}`)
  const totalMinutes = (end.getTime() - start.getTime()) / (1000 * 60)
  
  return Math.floor(totalMinutes / form.value.slotDurationMinutes)
}

async function handleSubmit() {
  error.value = null
  loading.value = true

  try {
    let success = false
    
    if (isEditing.value && props.schedule) {
      console.log('📝 Updating existing schedule...')
      // Update existing schedule
      const updateData: UpdateDoctorScheduleRequest = {
        startTime: form.value.startTime,
        endTime: form.value.endTime,
        slotDurationMinutes: form.value.slotDurationMinutes,
        effectiveFrom: form.value.effectiveFrom || undefined,
        effectiveTo: form.value.effectiveTo || undefined,
        isActive: form.value.isActive
      }

      const result = await scheduleStore.updateSchedule(props.schedule.id, updateData)
      success = result !== null
      console.log('✅ Update result:', { success, result })
    } else {
      console.log('🆕 Creating new schedule...')
      // Create new schedule
      if (form.value.dayOfWeek === null) {
        error.value = 'Please select a day of week'
        return
      }

      const createData: CreateDoctorScheduleRequest = {
        doctorId: props.doctorId,
        dayOfWeek: form.value.dayOfWeek,
        startTime: form.value.startTime,
        endTime: form.value.endTime,
        slotDurationMinutes: form.value.slotDurationMinutes,
        effectiveFrom: form.value.effectiveFrom || undefined,
        effectiveTo: form.value.effectiveTo || undefined,
        isActive: form.value.isActive
      }

      console.log('📤 Create data:', createData)
      const result = await scheduleStore.createSchedule(createData)
      success = result !== null
      console.log('✅ Create result:', { success, result })
    }

    if (success && !scheduleStore.error) {
      console.log('🎉 Operation successful, emitting saved event')
      // Success - emit saved event
      emit('saved')
    } else {
      console.log('❌ Operation failed:', scheduleStore.error)
      error.value = scheduleStore.error || 'Failed to save schedule'
    }
  } catch (err: any) {
    console.error('❌ Exception in handleSubmit:', err)
    error.value = err.message || 'An error occurred while saving the schedule'
  } finally {
    loading.value = false
  }
}

// Initialize form with existing data if editing
onMounted(() => {
  if (props.schedule) {
    form.value = {
      dayOfWeek: props.schedule.dayOfWeek,
      startTime: props.schedule.startTime,
      endTime: props.schedule.endTime,
      slotDurationMinutes: props.schedule.slotDurationMinutes,
      effectiveFrom: props.schedule.effectiveFrom || '',
      effectiveTo: props.schedule.effectiveTo || '',
      isActive: props.schedule.isActive
    }
  }
})

// Watch for store errors
watch(() => scheduleStore.error, (newError) => {
  if (newError) {
    error.value = newError
  }
})
</script>
