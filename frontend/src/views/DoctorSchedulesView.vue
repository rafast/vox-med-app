<template>
  <div class="doctor-schedules-page">
    <!-- Header -->
    <header class="schedules-header">
      <div class="header-content">
        <div class="header-left">
          <button
            @click="goBackToDashboard"
            class="back-to-dashboard-btn"
            title="Back to Dashboard"
          >
            <span class="btn-icon">←</span>
            Dashboard
          </button>
          <div class="header-info">
            <h1>📅 Doctor Schedules</h1>
            <p>Manage your weekly availability and time slots</p>
          </div>
        </div>
        <button
          @click="showCreateForm = true"
          class="add-schedule-btn"
        >
          <span class="btn-icon">+</span>
          Add New Schedule
        </button>
      </div>
    </header>

    <main class="schedules-main">
      <!-- Error Message -->
      <div v-if="error" class="error-banner">
        <span class="error-icon">⚠️</span>
        <span class="error-text">{{ error }}</span>
        <button @click="clearError" class="error-close">×</button>
      </div>

      <!-- Quick Stats -->
      <div class="stats-section">
        <div class="stats-grid">
          <div class="stat-card total">
            <div class="stat-icon">📊</div>
            <div class="stat-content">
              <h3>Total Schedules</h3>
              <p class="stat-number">{{ schedules.length }}</p>
            </div>
          </div>
          
          <div class="stat-card active">
            <div class="stat-icon">✅</div>
            <div class="stat-content">
              <h3>Active Schedules</h3>
              <p class="stat-number">{{ activeSchedulesCount }}</p>
            </div>
          </div>
          
          <div class="stat-card slots">
            <div class="stat-icon">⏰</div>
            <div class="stat-content">
              <h3>Total Slots</h3>
              <p class="stat-number">{{ totalSlotsCount }}</p>
            </div>
          </div>
          
          <div class="stat-card coverage">
            <div class="stat-icon">📅</div>
            <div class="stat-content">
              <h3>Days Covered</h3>
              <p class="stat-number">{{ uniqueDaysCount }}/7</p>
            </div>
          </div>
        </div>
      </div>

      <!-- Loading State -->
      <div v-if="loading" class="loading-state">
        <div class="loading-spinner"></div>
        <p>Loading your schedules...</p>
      </div>

      <!-- Empty State -->
      <div v-else-if="schedules.length === 0" class="empty-state">
        <div class="empty-icon">📅</div>
        <h3>No schedules found</h3>
        <p>Create your first schedule to start managing your availability</p>
        <button
          @click="showCreateForm = true"
          class="create-first-btn"
        >
          <span class="btn-icon">+</span>
          Create Your First Schedule
        </button>
      </div>

      <!-- Schedule Cards -->
      <div v-else class="schedules-grid">
        <div
          v-for="schedule in schedules"
          :key="schedule.id"
          :class="[
            'schedule-card',
            { 'inactive': !schedule.isActive }
          ]"
        >
          <!-- Card Header -->
          <div class="card-header">
            <div class="day-info">
              <h3 class="day-name">{{ getDayName(schedule.dayOfWeek) }}</h3>
              <span class="day-emoji">{{ getDayEmoji(schedule.dayOfWeek) }}</span>
            </div>
            
            <div class="card-actions">
              <span
                :class="[
                  'status-badge',
                  schedule.isActive ? 'active' : 'inactive'
                ]"
              >
                {{ schedule.isActive ? '🟢 Active' : '🔴 Inactive' }}
              </span>
              
              <button
                @click="toggleDropdown(schedule.id)"
                class="more-actions-btn"
                title="More actions"
              >
                ⋮
              </button>
              
              <div
                v-if="activeDropdown === schedule.id"
                class="actions-dropdown"
              >
                <button
                  @click="editSchedule(schedule)"
                  class="dropdown-item edit"
                >
                  <span class="item-icon">✏️</span>
                  Edit Schedule
                </button>
                <button
                  @click="toggleScheduleStatus(schedule)"
                  class="dropdown-item toggle"
                >
                  <span class="item-icon">{{ schedule.isActive ? '⏸️' : '▶️' }}</span>
                  {{ schedule.isActive ? 'Deactivate' : 'Activate' }}
                </button>
                <button
                  @click="confirmDelete(schedule)"
                  class="dropdown-item delete"
                >
                  <span class="item-icon">🗑️</span>
                  Delete
                </button>
              </div>
            </div>
          </div>

          <!-- Time Info -->
          <div class="time-info">
            <div class="time-range">
              <span class="time-label">Hours</span>
              <span class="time-value">{{ schedule.startTime }} - {{ schedule.endTime }}</span>
            </div>
            
            <div class="slot-info">
              <span class="slot-label">Slot Duration</span>
              <span class="slot-value">{{ schedule.slotDurationMinutes }} min</span>
            </div>
          </div>

          <!-- Slots Summary -->
          <div class="slots-summary">
            <div class="slots-count">
              <span class="slots-number">{{ schedule.totalSlotsPerDay }}</span>
              <span class="slots-text">Available Slots</span>
            </div>
            
            <div class="schedule-duration">
              <span class="duration-text">
                {{ calculateDuration(schedule.startTime, schedule.endTime) }} hours
              </span>
            </div>
          </div>

          <!-- Effective Dates -->
          <div v-if="schedule.effectiveFrom || schedule.effectiveTo" class="effective-dates">
            <span class="dates-label">📅 Effective Period</span>
            <div class="dates-range">
              <span v-if="schedule.effectiveFrom">
                From: {{ formatDate(schedule.effectiveFrom) }}
              </span>
              <span v-if="schedule.effectiveTo">
                To: {{ formatDate(schedule.effectiveTo) }}
              </span>
              <span v-if="!schedule.effectiveFrom && !schedule.effectiveTo">
                Ongoing
              </span>
            </div>
          </div>

          <!-- Quick Actions -->
          <div class="quick-actions">
            <button
              @click="editSchedule(schedule)"
              class="quick-action edit"
              title="Edit Schedule"
            >
              ✏️ Edit
            </button>
            <button
              @click="toggleScheduleStatus(schedule)"
              :class="[
                'quick-action',
                schedule.isActive ? 'deactivate' : 'activate'
              ]"
              :title="schedule.isActive ? 'Deactivate' : 'Activate'"
            >
              {{ schedule.isActive ? '⏸️ Pause' : '▶️ Activate' }}
            </button>
          </div>
        </div>
      </div>
    </main>

    <!-- Create/Edit Form Modal (custom overlay) -->
    <div v-if="showCreateForm || editingSchedule" class="modal-overlay" @click="closeForm">
      <div class="modal-content" @click.stop>
        <ScheduleFormModal
          :schedule="editingSchedule"
          :doctor-id="doctorId"
          @close="closeForm"
          @saved="handleScheduleSaved"
        />
      </div>
    </div>

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
import ScheduleFormModal from '@/components/doctor/ScheduleFormModal.vue'
import ConfirmDialog from '@/components/ui/ConfirmDialog.vue'
import { useAuthStore } from '@/stores/auth'
import { useDoctorScheduleStore } from '@/stores/doctorSchedule'
import type { DoctorSchedule } from '@/types/doctorSchedule'
import { DAYS_OF_WEEK } from '@/types/doctorSchedule'
import { computed, onMounted, onUnmounted, ref } from 'vue'
import { useRouter } from 'vue-router'

// Router
const router = useRouter()

// Stores
const scheduleStore = useDoctorScheduleStore()
const authStore = useAuthStore()

// Reactive data
const showCreateForm = ref(false)
const editingSchedule = ref<DoctorSchedule | null>(null)
const scheduleToDelete = ref<DoctorSchedule | null>(null)
const activeDropdown = ref<string | null>(null)

// Computed - destructure from store
const { schedules, loading, error } = scheduleStore
const { clearError } = scheduleStore

// Get doctor ID from current user
const doctorId = ref(authStore.user?.id || '')

// Computed properties for stats
const activeSchedulesCount = computed(() => 
  schedules.filter((s: DoctorSchedule) => s.isActive).length
)

const totalSlotsCount = computed(() => 
  schedules.reduce((total: number, schedule: DoctorSchedule) => total + schedule.totalSlotsPerDay, 0)
)

const uniqueDaysCount = computed(() => {
  const days = new Set(schedules.map((s: DoctorSchedule) => s.dayOfWeek))
  return days.size
})

// Methods
function getDayName(dayOfWeek: number): string {
  return DAYS_OF_WEEK.find(day => day.value === dayOfWeek)?.label || 'Unknown'
}

function getDayEmoji(dayOfWeek: number): string {
  const emojis = {
    1: '📅', // Monday
    2: '🗓️', // Tuesday  
    3: '📆', // Wednesday
    4: '🗓️', // Thursday
    5: '📅', // Friday
    6: '🌅', // Saturday
    7: '🌄'  // Sunday
  }
  return emojis[dayOfWeek as keyof typeof emojis] || '📅'
}

function calculateDuration(startTime: string, endTime: string): string {
  const start = new Date(`2000-01-01T${startTime}`)
  const end = new Date(`2000-01-01T${endTime}`)
  const diffMs = end.getTime() - start.getTime()
  const diffHours = diffMs / (1000 * 60 * 60)
  return diffHours.toFixed(1)
}

function formatDate(dateString: string): string {
  return new Date(dateString).toLocaleDateString()
}

function goBackToDashboard() {
  router.push('/dashboard')
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

function handleScheduleSaved() {
  console.log('🎉 handleScheduleSaved called - closing form')
  closeForm()
  // No need to refresh - the store already adds the new schedule to local state
  console.log('📊 Current schedules count:', schedules.length)
}

// Close dropdown when clicking outside
function handleClickOutside(event: MouseEvent) {
  const target = event.target as Element
  if (!target.closest('.relative')) {
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

/* Modal styles (same as PatientDashboardView) */
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
.doctor-schedules-page {
  min-height: 100vh;
  background-color: #f7fafc;
}

/* Header Styles */
.schedules-header {
  background: white;
  border-bottom: 1px solid #e2e8f0;
  padding: 1.5rem 0;
  position: sticky;
  top: 0;
  z-index: 10;
}

.header-content {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 1rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.header-left {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.back-to-dashboard-btn {
  background: linear-gradient(135deg, #4299e1 0%, #3182ce 100%);
  color: white;
  border: none;
  padding: 0.5rem 1rem;
  border-radius: 6px;
  cursor: pointer;
  font-weight: 500;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  transition: all 0.2s;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  font-size: 0.875rem;
}

.back-to-dashboard-btn:hover {
  transform: translateY(-1px);
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.15);
  background: linear-gradient(135deg, #3182ce 0%, #2c5aa0 100%);
}

.back-to-dashboard-btn .btn-icon {
  font-size: 1rem;
  font-weight: bold;
}

.header-info h1 {
  font-size: 1.75rem;
  font-weight: bold;
  color: #2d3748;
  margin-bottom: 0.25rem;
}

.header-info p {
  color: #718096;
  font-size: 0.875rem;
}

.add-schedule-btn {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  border: none;
  padding: 0.75rem 1.5rem;
  border-radius: 8px;
  cursor: pointer;
  font-weight: 500;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  transition: all 0.2s;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.add-schedule-btn:hover {
  transform: translateY(-1px);
  box-shadow: 0 4px 8px rgba(0, 0, 0, 0.15);
}

.btn-icon {
  font-size: 1.125rem;
  font-weight: bold;
}

/* Main Content */
.schedules-main {
  max-width: 1200px;
  margin: 0 auto;
  padding: 2rem 1rem;
}

/* Error Banner */
.error-banner {
  background: linear-gradient(135deg, #fc8181 0%, #f56565 100%);
  color: white;
  padding: 1rem 1.5rem;
  border-radius: 8px;
  margin-bottom: 2rem;
  display: flex;
  align-items: center;
  gap: 0.75rem;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.error-icon {
  font-size: 1.25rem;
}

.error-text {
  flex: 1;
  font-weight: 500;
}

.error-close {
  background: none;
  border: none;
  color: white;
  font-size: 1.25rem;
  cursor: pointer;
  padding: 0.25rem;
  border-radius: 4px;
  transition: background-color 0.2s;
}

.error-close:hover {
  background: rgba(255, 255, 255, 0.1);
}

/* Stats Section */
.stats-section {
  margin-bottom: 2rem;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 1rem;
}

.stat-card {
  background: white;
  padding: 1.5rem;
  border-radius: 12px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05);
  display: flex;
  align-items: center;
  gap: 1rem;
  transition: all 0.2s;
  border: 1px solid #e2e8f0;
}

.stat-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
}

.stat-card.total {
  border-left: 4px solid #4299e1;
}

.stat-card.active {
  border-left: 4px solid #48bb78;
}

.stat-card.slots {
  border-left: 4px solid #ed8936;
}

.stat-card.coverage {
  border-left: 4px solid #9f7aea;
}

.stat-icon {
  font-size: 2rem;
  width: 60px;
  height: 60px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #f7fafc;
  border-radius: 50%;
}

.stat-content h3 {
  font-size: 0.875rem;
  color: #718096;
  margin-bottom: 0.25rem;
  font-weight: 500;
}

.stat-number {
  font-size: 1.875rem;
  font-weight: bold;
  color: #2d3748;
  line-height: 1;
}

/* Loading State */
.loading-state {
  background: white;
  padding: 3rem;
  border-radius: 12px;
  text-align: center;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05);
}

.loading-spinner {
  width: 48px;
  height: 48px;
  border: 4px solid #e2e8f0;
  border-top: 4px solid #4299e1;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin: 0 auto 1rem;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

.loading-state p {
  color: #718096;
  font-size: 1rem;
}

/* Empty State */
.empty-state {
  background: white;
  padding: 4rem 2rem;
  border-radius: 12px;
  text-align: center;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05);
}

.empty-icon {
  font-size: 4rem;
  margin-bottom: 1rem;
}

.empty-state h3 {
  font-size: 1.5rem;
  font-weight: bold;
  color: #2d3748;
  margin-bottom: 0.5rem;
}

.empty-state p {
  color: #718096;
  margin-bottom: 2rem;
  font-size: 1rem;
}

.create-first-btn {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  border: none;
  padding: 0.875rem 2rem;
  border-radius: 8px;
  cursor: pointer;
  font-weight: 500;
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  transition: all 0.2s;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.create-first-btn:hover {
  transform: translateY(-1px);
  box-shadow: 0 4px 8px rgba(0, 0, 0, 0.15);
}

/* Schedule Cards Grid */
.schedules-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
  gap: 1.5rem;
}

.schedule-card {
  background: white;
  border-radius: 12px;
  padding: 1.5rem;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05);
  border: 1px solid #e2e8f0;
  transition: all 0.2s;
  position: relative;
  overflow: hidden;
}

.schedule-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 20px rgba(0, 0, 0, 0.1);
}

.schedule-card::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  height: 4px;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
}

.schedule-card.inactive {
  opacity: 0.6;
}

.schedule-card.inactive::before {
  background: #cbd5e0;
}

/* Card Header */
.card-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 1.5rem;
}

.day-info {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.day-name {
  font-size: 1.25rem;
  font-weight: bold;
  color: #2d3748;
  margin: 0;
}

.day-emoji {
  font-size: 1.5rem;
}

.card-actions {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  position: relative;
}

.status-badge {
  padding: 0.25rem 0.75rem;
  border-radius: 20px;
  font-size: 0.75rem;
  font-weight: 500;
  display: flex;
  align-items: center;
  gap: 0.25rem;
}

.status-badge.active {
  background: #c6f6d5;
  color: #22543d;
}

.status-badge.inactive {
  background: #e2e8f0;
  color: #4a5568;
}

.more-actions-btn {
  background: none;
  border: none;
  padding: 0.5rem;
  border-radius: 6px;
  cursor: pointer;
  font-size: 1.25rem;
  color: #718096;
  transition: all 0.2s;
}

.more-actions-btn:hover {
  background: #f7fafc;
  color: #4a5568;
}

.actions-dropdown {
  position: absolute;
  top: 100%;
  right: 0;
  background: white;
  border-radius: 8px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
  border: 1px solid #e2e8f0;
  min-width: 160px;
  z-index: 20;
  overflow: hidden;
}

.dropdown-item {
  width: 100%;
  background: none;
  border: none;
  padding: 0.75rem 1rem;
  text-align: left;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  transition: background-color 0.2s;
  font-size: 0.875rem;
}

.dropdown-item:hover {
  background: #f7fafc;
}

.dropdown-item.delete {
  color: #e53e3e;
}

.dropdown-item.delete:hover {
  background: #fed7d7;
}

.item-icon {
  font-size: 1rem;
}

/* Time Info */
.time-info {
  margin-bottom: 1rem;
}

.time-range, .slot-info {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 0.5rem;
}

.time-label, .slot-label {
  font-size: 0.875rem;
  color: #718096;
  font-weight: 500;
}

.time-value, .slot-value {
  font-size: 0.875rem;
  color: #2d3748;
  font-weight: 600;
}

/* Slots Summary */
.slots-summary {
  background: #f7fafc;
  padding: 1rem;
  border-radius: 8px;
  margin-bottom: 1rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.slots-count {
  text-align: center;
}

.slots-number {
  display: block;
  font-size: 1.5rem;
  font-weight: bold;
  color: #4299e1;
  line-height: 1;
}

.slots-text {
  font-size: 0.75rem;
  color: #718096;
  font-weight: 500;
}

.schedule-duration {
  text-align: center;
}

.duration-text {
  font-size: 0.875rem;
  color: #718096;
  font-weight: 500;
}

/* Effective Dates */
.effective-dates {
  margin-bottom: 1rem;
  padding: 0.75rem;
  background: #edf2f7;
  border-radius: 6px;
}

.dates-label {
  font-size: 0.75rem;
  color: #4a5568;
  font-weight: 500;
  margin-bottom: 0.5rem;
  display: block;
}

.dates-range {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.dates-range span {
  font-size: 0.75rem;
  color: #2d3748;
}

/* Quick Actions */
.quick-actions {
  display: flex;
  gap: 0.5rem;
}

.quick-action {
  flex: 1;
  padding: 0.5rem 0.75rem;
  border: 1px solid #e2e8f0;
  background: white;
  border-radius: 6px;
  cursor: pointer;
  font-size: 0.75rem;
  font-weight: 500;
  transition: all 0.2s;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.25rem;
}

.quick-action:hover {
  background: #f7fafc;
}

.quick-action.edit {
  color: #4299e1;
  border-color: #4299e1;
}

.quick-action.edit:hover {
  background: #ebf8ff;
}

.quick-action.activate {
  color: #48bb78;
  border-color: #48bb78;
}

.quick-action.activate:hover {
  background: #f0fff4;
}

.quick-action.deactivate {
  color: #ed8936;
  border-color: #ed8936;
}

.quick-action.deactivate:hover {
  background: #fffaf0;
}

/* Responsive Design */
@media (max-width: 768px) {
  .header-content {
    flex-direction: column;
    align-items: stretch;
    gap: 1rem;
  }

  .header-left {
    flex-direction: column;
    align-items: stretch;
    gap: 0.75rem;
  }

  .back-to-dashboard-btn {
    align-self: flex-start;
  }

  .stats-grid {
    grid-template-columns: 1fr;
  }

  .schedules-grid {
    grid-template-columns: 1fr;
  }

  .slots-summary {
    flex-direction: column;
    gap: 1rem;
    text-align: center;
  }

  .quick-actions {
    flex-direction: column;
  }
}
</style>
