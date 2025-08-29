<template>
  <div class="doctor-schedule-list">
    <!-- Header -->
    <div class="flex justify-between items-center mb-6">
      <h2 class="text-2xl font-bold text-gray-800">Doctor Schedules</h2>
      <button
        @click="showCreateForm = true"
        class="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-lg flex items-center gap-2 transition-colors"
      >
        <Plus class="w-4 h-4" />
        <Plus class="w-4 h-4" />
        Add Schedule
      </button>
    </div>

    <!-- Error Message -->
    <div v-if="error" class="bg-red-50 border border-red-200 text-red-600 px-4 py-3 rounded-lg mb-4">
      {{ error }}
      <button @click="clearError" class="float-right text-red-400 hover:text-red-600">×</button>
    </div>

    <!-- Loading State -->
    <div v-if="loading" class="flex justify-center items-center py-8">
      <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
      <span class="ml-2 text-gray-600">Loading schedules...</span>
    </div>

    <!-- Empty State -->
    <div v-else-if="schedules.length === 0" class="text-center py-12">
      <Calendar class="w-16 h-16 text-gray-300 mx-auto mb-4" />
      <h3 class="text-lg font-medium text-gray-600 mb-2">No schedules found</h3>
      <p class="text-gray-500 mb-4">Create your first schedule to get started</p>
      <button
        @click="showCreateForm = true"
        class="bg-blue-600 hover:bg-blue-700 text-white px-6 py-2 rounded-lg transition-colors"
      >
        Create Schedule
      </button>
    </div>

    <!-- Schedule Grid -->
    <div v-else class="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
      <div
        v-for="schedule in schedules"
        :key="schedule.id"
        class="bg-white rounded-lg shadow-md border border-gray-200 p-6 hover:shadow-lg transition-shadow"
      >
        <!-- Day Header -->
        <div class="flex justify-between items-start mb-4">
          <div>
            <h3 class="text-lg font-semibold text-gray-800">
              {{ getDayName(schedule.dayOfWeek) }}
            </h3>
            <div class="flex items-center gap-2 mt-1">
              <span
                :class="[
                  'px-2 py-1 rounded-full text-xs font-medium',
                  schedule.isActive 
                    ? 'bg-green-100 text-green-800' 
                    : 'bg-gray-100 text-gray-600'
                ]"
              >
                {{ schedule.isActive ? 'Active' : 'Inactive' }}
              </span>
            </div>
          </div>
          
          <!-- Actions Dropdown -->
          <div class="relative">
            <button
              @click="toggleDropdown(schedule.id)"
              class="p-1 rounded-full hover:bg-gray-100 transition-colors"
            >
              <MoreVertical class="w-4 h-4 text-gray-600" />
            </button>
            
            <div
              v-if="activeDropdown === schedule.id"
              class="absolute right-0 mt-2 w-48 bg-white rounded-lg shadow-lg border border-gray-200 z-10"
            >
              <button
                @click="editSchedule(schedule)"
                class="w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-gray-50 flex items-center gap-2"
              >
                <Edit class="w-4 h-4" />
                Edit
              </button>
              <button
                @click="toggleScheduleStatus(schedule)"
                class="w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-gray-50 flex items-center gap-2"
              >
                <Power class="w-4 h-4" />
                {{ schedule.isActive ? 'Deactivate' : 'Activate' }}
              </button>
              <button
                @click="confirmDelete(schedule)"
                class="w-full text-left px-4 py-2 text-sm text-red-600 hover:bg-red-50 flex items-center gap-2"
              >
                <Trash2 class="w-4 h-4" />
                Delete
              </button>
            </div>
          </div>
        </div>

        <!-- Time Info -->
        <div class="space-y-2 mb-4">
          <div class="flex items-center gap-2 text-sm text-gray-600">
            <Clock class="w-4 h-4" />
            <span>{{ schedule.startTime }} - {{ schedule.endTime }}</span>
          </div>
          <div class="flex items-center gap-2 text-sm text-gray-600">
            <Users class="w-4 h-4" />
            <span>{{ schedule.slotDurationMinutes }} min slots</span>
          </div>
          <div class="flex items-center gap-2 text-sm text-gray-600">
            <Hash class="w-4 h-4" />
            <span>{{ schedule.totalSlotsPerDay }} slots/day</span>
          </div>
        </div>

        <!-- Effective Dates -->
        <div v-if="schedule.effectiveFrom || schedule.effectiveTo" class="text-xs text-gray-500 border-t pt-3">
          <div v-if="schedule.effectiveFrom">
            <strong>From:</strong> {{ formatDate(schedule.effectiveFrom) }}
          </div>
          <div v-if="schedule.effectiveTo">
            <strong>Until:</strong> {{ formatDate(schedule.effectiveTo) }}
          </div>
        </div>
      </div>
    </div>

    <!-- Create/Edit Form Modal -->
    <ScheduleFormModal
      v-if="showCreateForm || editingSchedule"
      :schedule="editingSchedule"
      :doctor-id="doctorId"
      @close="closeForm"
      @saved="handleScheduleSaved"
    />

    <!-- Delete Confirmation Modal -->
    <ConfirmDialog
      v-if="scheduleToDelete"
      :title="`Delete ${getDayName(scheduleToDelete.dayOfWeek)} Schedule?`"
      :message="`Are you sure you want to delete this schedule? This action cannot be undone.`"
      confirm-text="Delete"
      confirm-class="bg-red-600 hover:bg-red-700"
      @confirm="handleDelete"
      @cancel="scheduleToDelete = null"
    />
  </div>
</template>

<script setup lang="ts">
import { useAuthStore } from '@/stores/auth'
import { useDoctorScheduleStore } from '@/stores/doctorSchedule'
import type { DoctorSchedule } from '@/types/doctorSchedule'
import { DAYS_OF_WEEK } from '@/types/doctorSchedule'
import { onMounted, onUnmounted, ref } from 'vue'
// Props
interface Props {
  doctorId?: string
}
const props = withDefaults(defineProps<Props>(), {
  doctorId: ''
})

// Stores
const scheduleStore = useDoctorScheduleStore()
const authStore = useAuthStore()

// Reactive data
const showCreateForm = ref(false)
const editingSchedule = ref<DoctorSchedule | null>(null)
const scheduleToDelete = ref<DoctorSchedule | null>(null)
const activeDropdown = ref<string | null>(null)

// Computed
const { schedules, loading, error } = scheduleStore
const { clearError } = scheduleStore

// Get doctor ID from props or current user
const doctorId = ref(props.doctorId || authStore.user?.id || '')

// Methods
function getDayName(dayOfWeek: number): string {
  return DAYS_OF_WEEK.find(day => day.value === dayOfWeek)?.label || 'Unknown'
}

function formatDate(dateString: string): string {
  return new Date(dateString).toLocaleDateString()
}

function toggleDropdown(scheduleId: string) {
  activeDropdown.value = activeDropdown.value === scheduleId ? null : scheduleId
}

function editSchedule(schedule: DoctorSchedule) {
  editingSchedule.value = schedule
  activeDropdown.value = null
}

async function toggleScheduleStatus(schedule: DoctorSchedule) {
  const updateData = {
    startTime: schedule.startTime,
    endTime: schedule.endTime,
    slotDurationMinutes: schedule.slotDurationMinutes,
    effectiveFrom: schedule.effectiveFrom,
    effectiveTo: schedule.effectiveTo,
    isActive: !schedule.isActive
  }
  
  await scheduleStore.updateSchedule(schedule.id, updateData)
  activeDropdown.value = null
}

function confirmDelete(schedule: DoctorSchedule) {
  scheduleToDelete.value = schedule
  activeDropdown.value = null
}

async function handleDelete() {
  if (scheduleToDelete.value) {
    await scheduleStore.deleteSchedule(scheduleToDelete.value.id)
    scheduleToDelete.value = null
  }
}

function closeForm() {
  showCreateForm.value = false
  editingSchedule.value = null
}

async function handleScheduleSaved() {
  closeForm()
  // Refresh the schedules list
  if (doctorId.value) {
    await scheduleStore.fetchDoctorSchedules(doctorId.value)
  }
}

// Close dropdown when clicking outside
function handleClickOutside(event: MouseEvent) {
  if (!(event.target as Element).closest('.relative')) {
    activeDropdown.value = null
  }
}

// Lifecycle
onMounted(async () => {
  document.addEventListener('click', handleClickOutside)
  
  if (doctorId.value) {
    await scheduleStore.fetchDoctorSchedules(doctorId.value)
  }
})

onUnmounted(() => {
  document.removeEventListener('click', handleClickOutside)
})
</script>

<style scoped>
/* Add any custom styles if needed */
</style>
