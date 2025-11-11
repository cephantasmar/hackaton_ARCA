<template>
  <div class="vacations-container">
    <!-- Vista para Empleados -->
    <div v-if="!isAdmin" class="employee-view">
      <div class="header">
        <h2>Mis Vacaciones</h2>
        <button @click="showRequestForm = true" class="btn-primary">
          + Solicitar Vacaciones
        </button>
      </div>

      <!-- Balance de vacaciones -->
      <div v-if="balance" class="balance-card">
        <h3>Balance {{ currentYear }}</h3>
        <div class="balance-grid">
          <div class="balance-item">
            <span class="label">Años de antigüedad:</span>
            <span class="value">{{ balance.years_of_service }} años</span>
          </div>
          <div class="balance-item">
            <span class="label">Días disponibles:</span>
            <span class="value" :class="{ 'text-success': balance.dias_disponibles > 0 }">
              {{ balance.dias_disponibles }} días
            </span>
          </div>
          <div class="balance-item">
            <span class="label">Días usados:</span>
            <span class="value">{{ balance.dias_usados }} días</span>
          </div>
          <div class="balance-item">
            <span class="label">Días restantes:</span>
            <span class="value">{{ balance.dias_restantes }} días</span>
          </div>
        </div>
      </div>

      <!-- Lista de mis solicitudes -->
      <div class="section">
        <h3>Mis Solicitudes</h3>
        <div v-if="myVacations.length === 0" class="empty-state">
          No tienes solicitudes de vacaciones aún
        </div>
        <div v-else class="vacations-list">
          <div v-for="vacation in myVacations" :key="vacation.id" class="vacation-card">
            <div class="card-header">
              <span class="status-badge" :class="`status-${vacation.estado}`">
                {{ vacation.estado }}
              </span>
            </div>
            <div class="card-body">
              <div class="dates">
                📅 {{ formatDate(vacation.fecha_inicio) }} - {{ formatDate(vacation.fecha_fin) }}
              </div>
              <div class="meta">
                <span>⏱️ {{ vacation.dias_solicitados }} días</span>
                <span>📆 Gestión {{ vacation.gestion }}</span>
              </div>
              <div v-if="vacation.motivo_rechazo" class="rejection-reason">
                <strong>Motivo rechazo:</strong> {{ vacation.motivo_rechazo }}
              </div>
            </div>
            <div v-if="vacation.estado === 'pendiente'" class="card-footer">
              <button @click="deleteRequest(vacation.id)" class="btn-danger-sm">
                Cancelar solicitud
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Vista para Administradores -->
    <div v-else class="admin-view">
      <div class="header">
        <h2>Gestión de Vacaciones</h2>
        <div class="filters">
          <button @click="filterStatus = null" :class="{ active: filterStatus === null }">
            Todas
          </button>
          <button @click="filterStatus = 'pendiente'" :class="{ active: filterStatus === 'pendiente' }">
            Pendientes
          </button>
          <button @click="filterStatus = 'aprobada'" :class="{ active: filterStatus === 'aprobada' }">
            Aprobadas
          </button>
          <button @click="filterStatus = 'rechazada'" :class="{ active: filterStatus === 'rechazada' }">
            Rechazadas
          </button>
        </div>
      </div>

      <div class="section">
        <div v-if="filteredVacations.length === 0" class="empty-state">
          No hay solicitudes {{ filterStatus ? `en estado "${filterStatus}"` : '' }}
        </div>
        <div v-else class="vacations-list">
          <div v-for="vacation in filteredVacations" :key="vacation.id" class="vacation-card admin">
            <div class="card-header">
              <div class="employee-info">
                <strong>{{ vacation.empleado?.nombre }} {{ vacation.empleado?.apellido }}</strong>
                <span class="email">{{ vacation.empleado?.email }}</span>
              </div>
              <span class="status-badge" :class="`status-${vacation.estado}`">
                {{ vacation.estado }}
              </span>
            </div>
            <div class="card-body">
              <div class="dates">
                📅 {{ formatDate(vacation.fecha_inicio) }} - {{ formatDate(vacation.fecha_fin) }}
              </div>
              <div class="meta">
                <span>⏱️ {{ vacation.dias_solicitados }} días</span>
                <span>📆 Gestión {{ vacation.gestion }}</span>
                <span>📝 {{ formatDate(vacation.fecha_solicitud) }}</span>
              </div>
              <div v-if="vacation.motivo_rechazo" class="rejection-reason">
                <strong>Motivo rechazo:</strong> {{ vacation.motivo_rechazo }}
              </div>
            </div>
            <div v-if="vacation.estado === 'pendiente'" class="card-footer admin-actions">
              <button @click="approveRequest(vacation.id)" class="btn-success">
                ✓ Aprobar
              </button>
              <button @click="showRejectModal(vacation)" class="btn-danger">
                ✗ Rechazar
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Modal de solicitud (solo empleados) -->
    <div v-if="showRequestForm" class="modal-overlay" @click.self="closeForm">
      <div class="modal-content">
        <h3>Solicitar Vacaciones</h3>
        <form @submit.prevent="submitRequest">
          <div class="form-group">
            <label>Fecha de inicio</label>
            <input 
              v-model="requestForm.fecha_inicio" 
              type="date" 
              required 
              :min="today"
            />
          </div>
          <div class="form-group">
            <label>Fecha de fin</label>
            <input 
              v-model="requestForm.fecha_fin" 
              type="date" 
              required 
              :min="requestForm.fecha_inicio || today"
            />
          </div>
          <div class="form-group">
            <label>Gestión</label>
            <input 
              v-model.number="requestForm.gestion" 
              type="number" 
              required 
              :value="currentYear"
              min="2020"
              max="2100"
            />
          </div>
          <div v-if="calculateDays() > 0" class="info-box">
            Días a solicitar: <strong>{{ calculateDays() }}</strong>
          </div>
          <div class="form-actions">
            <button type="submit" class="btn-primary" :disabled="loading">
              {{ loading ? 'Enviando...' : 'Solicitar' }}
            </button>
            <button type="button" class="btn-secondary" @click="closeForm">
              Cancelar
            </button>
          </div>
        </form>
      </div>
    </div>

    <!-- Modal de rechazo (solo admin) -->
    <div v-if="showRejectForm" class="modal-overlay" @click.self="closeRejectModal">
      <div class="modal-content">
        <h3>Rechazar Solicitud</h3>
        <form @submit.prevent="submitReject">
          <div class="form-group">
            <label>Motivo del rechazo</label>
            <textarea 
              v-model="rejectReason" 
              rows="4" 
              required
              placeholder="Explica por qué se rechaza esta solicitud..."
            ></textarea>
          </div>
          <div class="form-actions">
            <button type="submit" class="btn-danger" :disabled="loading">
              {{ loading ? 'Procesando...' : 'Rechazar' }}
            </button>
            <button type="button" class="btn-secondary" @click="closeRejectModal">
              Cancelar
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import vacationsService from '../services/vacations'

const currentYear = new Date().getFullYear()
const today = new Date().toISOString().split('T')[0]

const currentUser = ref(null)
const isAdmin = computed(() => {
  const rol = currentUser.value?.rol?.toLowerCase()
  return rol === 'admin' || rol === 'administrador' || rol === 'director'
})

const balance = ref(null)
const myVacations = ref([])
const allVacations = ref([])
const filterStatus = ref(null)
const loading = ref(false)
const showRequestForm = ref(false)
const showRejectForm = ref(false)
const selectedVacation = ref(null)
const rejectReason = ref('')

const requestForm = ref({
  fecha_inicio: '',
  fecha_fin: '',
  gestion: currentYear
})

const filteredVacations = computed(() => {
  if (!filterStatus.value) return allVacations.value
  return allVacations.value.filter(v => v.estado === filterStatus.value)
})

const loadUserInfo = async () => {
  try {
    currentUser.value = await vacationsService.getCurrentUser()
  } catch (error) {
    console.error('Error al cargar información del usuario:', error)
  }
}

const loadBalance = async () => {
  if (isAdmin.value) return
  
  try {
    balance.value = await vacationsService.getBalance(currentYear)
  } catch (error) {
    console.error('Error al cargar balance:', error)
  }
}

const loadVacations = async () => {
  try {
    if (isAdmin.value) {
      const response = await vacationsService.getAllVacations()
      allVacations.value = response.vacations || []
    } else {
      const response = await vacationsService.getMyVacations()
      myVacations.value = response.vacations || []
    }
  } catch (error) {
    console.error('Error al cargar vacaciones:', error)
    alert(error.message)
  }
}

const calculateDays = () => {
  if (!requestForm.value.fecha_inicio || !requestForm.value.fecha_fin) return 0
  const start = new Date(requestForm.value.fecha_inicio)
  const end = new Date(requestForm.value.fecha_fin)
  return Math.floor((end - start) / (1000 * 60 * 60 * 24)) + 1
}

const submitRequest = async () => {
  loading.value = true
  try {
    await vacationsService.requestVacation(requestForm.value)
    alert('Solicitud enviada exitosamente')
    closeForm()
    await loadBalance()
    await loadVacations()
  } catch (error) {
    console.error('Error al solicitar vacaciones:', error)
    alert(error.message || 'Error al solicitar vacaciones')
  } finally {
    loading.value = false
  }
}

const approveRequest = async (id) => {
  if (!confirm('¿Aprobar esta solicitud de vacaciones?')) return
  
  loading.value = true
  try {
    await vacationsService.approveOrReject(id, true)
    alert('Solicitud aprobada exitosamente')
    await loadVacations()
  } catch (error) {
    console.error('Error al aprobar solicitud:', error)
    alert(error.message || 'Error al aprobar solicitud')
  } finally {
    loading.value = false
  }
}

const showRejectModal = (vacation) => {
  selectedVacation.value = vacation
  showRejectForm.value = true
  rejectReason.value = ''
}

const submitReject = async () => {
  loading.value = true
  try {
    await vacationsService.approveOrReject(
      selectedVacation.value.id, 
      false, 
      rejectReason.value
    )
    alert('Solicitud rechazada')
    closeRejectModal()
    await loadVacations()
  } catch (error) {
    console.error('Error al rechazar solicitud:', error)
    alert(error.message || 'Error al rechazar solicitud')
  } finally {
    loading.value = false
  }
}

const deleteRequest = async (id) => {
  if (!confirm('¿Cancelar esta solicitud?')) return
  
  try {
    await vacationsService.deleteVacation(id)
    alert('Solicitud cancelada')
    await loadBalance()
    await loadVacations()
  } catch (error) {
    console.error('Error al cancelar solicitud:', error)
    alert(error.message || 'Error al cancelar solicitud')
  }
}

const closeForm = () => {
  showRequestForm.value = false
  requestForm.value = {
    fecha_inicio: '',
    fecha_fin: '',
    gestion: currentYear
  }
}

const closeRejectModal = () => {
  showRejectForm.value = false
  selectedVacation.value = null
  rejectReason.value = ''
}

const formatDate = (dateStr) => {
  if (!dateStr) return ''
  return new Date(dateStr).toLocaleDateString('es-ES')
}

onMounted(async () => {
  await loadUserInfo()
  await loadBalance()
  await loadVacations()
})
</script>

<style scoped>
.vacations-container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 20px;
}

.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
  flex-wrap: wrap;
  gap: 16px;
}

.header h2 {
  margin: 0;
  color: #1e293b;
}

.filters {
  display: flex;
  gap: 8px;
}

.filters button {
  padding: 8px 16px;
  border: 2px solid #e5e7eb;
  background: white;
  border-radius: 6px;
  cursor: pointer;
  font-weight: 500;
  transition: all 0.2s;
}

.filters button.active {
  background: #3b82f6;
  color: white;
  border-color: #3b82f6;
}

.filters button:hover:not(.active) {
  border-color: #3b82f6;
}

.btn-primary {
  background: #3b82f6;
  color: white;
  border: none;
  padding: 10px 20px;
  border-radius: 8px;
  cursor: pointer;
  font-weight: 600;
}

.btn-primary:hover {
  background: #2563eb;
}

.btn-primary:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.btn-secondary {
  background: #6b7280;
  color: white;
  border: none;
  padding: 10px 20px;
  border-radius: 8px;
  cursor: pointer;
}

.btn-success {
  background: #10b981;
  color: white;
  border: none;
  padding: 8px 16px;
  border-radius: 6px;
  cursor: pointer;
  font-weight: 500;
}

.btn-success:hover {
  background: #059669;
}

.btn-danger {
  background: #ef4444;
  color: white;
  border: none;
  padding: 8px 16px;
  border-radius: 6px;
  cursor: pointer;
  font-weight: 500;
}

.btn-danger:hover {
  background: #dc2626;
}

.btn-danger-sm {
  background: #ef4444;
  color: white;
  border: none;
  padding: 6px 12px;
  border-radius: 6px;
  cursor: pointer;
  font-size: 13px;
}

.balance-card {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  padding: 24px;
  border-radius: 12px;
  margin-bottom: 24px;
  box-shadow: 0 4px 6px rgba(0,0,0,0.1);
}

.balance-card h3 {
  margin-top: 0;
  margin-bottom: 16px;
}

.balance-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 16px;
}

.balance-item {
  display: flex;
  flex-direction: column;
}

.balance-item .label {
  font-size: 14px;
  opacity: 0.9;
}

.balance-item .value {
  font-size: 24px;
  font-weight: bold;
  margin-top: 4px;
}

.section {
  margin-top: 24px;
}

.section h3 {
  margin-bottom: 16px;
  color: #1e293b;
}

.empty-state {
  text-align: center;
  padding: 48px;
  color: #6b7280;
  background: #f9fafb;
  border-radius: 8px;
}

.vacations-list {
  display: grid;
  gap: 16px;
}

.vacation-card {
  background: white;
  border: 1px solid #e5e7eb;
  border-radius: 8px;
  padding: 16px;
  transition: box-shadow 0.2s;
}

.vacation-card:hover {
  box-shadow: 0 4px 6px rgba(0,0,0,0.1);
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 12px;
}

.employee-info {
  display: flex;
  flex-direction: column;
}

.employee-info strong {
  font-size: 16px;
  color: #1e293b;
}

.employee-info .email {
  font-size: 13px;
  color: #6b7280;
}

.status-badge {
  padding: 4px 12px;
  border-radius: 12px;
  font-size: 12px;
  font-weight: 600;
  text-transform: uppercase;
}

.status-pendiente {
  background: #fef3c7;
  color: #92400e;
}

.status-aprobada {
  background: #d1fae5;
  color: #065f46;
}

.status-rechazada {
  background: #fee2e2;
  color: #991b1b;
}

.card-body .dates {
  margin-bottom: 8px;
  color: #374151;
  font-size: 15px;
}

.card-body .meta {
  display: flex;
  gap: 16px;
  font-size: 13px;
  color: #6b7280;
}

.rejection-reason {
  margin-top: 12px;
  padding: 12px;
  background: #fee2e2;
  border-radius: 6px;
  font-size: 14px;
  color: #991b1b;
}

.card-footer {
  margin-top: 16px;
  padding-top: 16px;
  border-top: 1px solid #e5e7eb;
}

.admin-actions {
  display: flex;
  gap: 8px;
}

.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.modal-content {
  background: white;
  padding: 24px;
  border-radius: 12px;
  max-width: 500px;
  width: 90%;
  max-height: 90vh;
  overflow-y: auto;
}

.modal-content h3 {
  margin-top: 0;
  margin-bottom: 20px;
  color: #1e293b;
}

.form-group {
  margin-bottom: 16px;
}

.form-group label {
  display: block;
  margin-bottom: 6px;
  font-weight: 500;
  color: #374151;
}

.form-group input,
.form-group textarea {
  width: 100%;
  padding: 10px;
  border: 1px solid #d1d5db;
  border-radius: 6px;
  font-size: 14px;
}

.form-group input:focus,
.form-group textarea:focus {
  outline: none;
  border-color: #3b82f6;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}

.info-box {
  background: #eff6ff;
  border: 1px solid #bfdbfe;
  padding: 12px;
  border-radius: 6px;
  margin-bottom: 16px;
  color: #1e40af;
}

.form-actions {
  display: flex;
  gap: 12px;
  justify-content: flex-end;
  margin-top: 20px;
}
</style>
