<template>
  <div class="admin-vacations-container">
    <div class="header">
      <h2>Gestión de Vacaciones</h2>
      <div class="filters">
        <select v-model="selectedEstado" @change="loadVacations" class="filter-select">
          <option value="">Todos los estados</option>
          <option value="pendiente">Pendientes</option>
          <option value="aprobado">Aprobados</option>
          <option value="rechazado">Rechazados</option>
        </select>
      </div>
    </div>

    <div v-if="loading" class="loading">Cargando solicitudes...</div>
    
    <div v-else-if="!vacations.length" class="empty-state">
      No hay solicitudes de vacaciones
    </div>

    <div v-else class="vacations-table-container">
      <table class="vacations-table">
        <thead>
          <tr>
            <th>ID</th>
            <th>Empleado</th>
            <th>Email</th>
            <th>Fechas</th>
            <th>Días</th>
            <th>Gestión</th>
            <th>Estado</th>
            <th>Solicitado</th>
            <th>Acciones</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="vacation in vacations" :key="vacation.id" :class="`row-${vacation.estado}`">
            <td>{{ vacation.id }}</td>
            <td>
              <strong>{{ vacation.empleado_nombre }}</strong>
            </td>
            <td>{{ vacation.empleado_email }}</td>
            <td>
              <div class="dates-cell">
                <div>{{ formatDate(vacation.fecha_inicio) }}</div>
                <div>{{ formatDate(vacation.fecha_fin) }}</div>
              </div>
            </td>
            <td><strong>{{ vacation.dias_solicitados }}</strong> días</td>
            <td>{{ vacation.gestion }}</td>
            <td>
              <span class="status-badge" :class="`badge-${vacation.estado}`">
                {{ vacation.estado }}
              </span>
            </td>
            <td>{{ formatDateTime(vacation.created_at) }}</td>
            <td>
              <div class="actions">
                <button 
                  v-if="vacation.estado === 'pendiente'"
                  @click="approveVacation(vacation.id)" 
                  class="btn-approve"
                  title="Aprobar"
                >
                  ✓
                </button>
                <button 
                  v-if="vacation.estado === 'pendiente'"
                  @click="openRejectModal(vacation.id)" 
                  class="btn-reject"
                  title="Rechazar"
                >
                  ✗
                </button>
                <button 
                  v-if="vacation.estado !== 'pendiente'"
                  @click="showDetails(vacation)" 
                  class="btn-info"
                  title="Ver detalles"
                >
                  👁️
                </button>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Modal para rechazar -->
    <div v-if="showRejectModal" class="modal-overlay" @click.self="closeRejectModal">
      <div class="modal-content">
        <h3>Rechazar Solicitud de Vacaciones</h3>
        <form @submit.prevent="rejectVacation">
          <div class="form-group">
            <label>Motivo del rechazo *</label>
            <textarea 
              v-model="rejectForm.motivo" 
              required 
              rows="4"
              placeholder="Explica el motivo del rechazo..."
            ></textarea>
          </div>
          <div class="form-actions">
            <button type="submit" class="btn-danger" :disabled="loading">
              {{ loading ? 'Procesando...' : 'Rechazar Solicitud' }}
            </button>
            <button type="button" class="btn-secondary" @click="closeRejectModal">
              Cancelar
            </button>
          </div>
        </form>
      </div>
    </div>

    <!-- Modal de detalles -->
    <div v-if="showDetailsModal" class="modal-overlay" @click.self="closeDetailsModal">
      <div class="modal-content">
        <h3>Detalles de la Solicitud</h3>
        <div v-if="selectedVacation" class="details-content">
          <div class="detail-row">
            <span class="label">ID:</span>
            <span class="value">{{ selectedVacation.id }}</span>
          </div>
          <div class="detail-row">
            <span class="label">Empleado:</span>
            <span class="value">{{ selectedVacation.empleado_nombre }}</span>
          </div>
          <div class="detail-row">
            <span class="label">Email:</span>
            <span class="value">{{ selectedVacation.empleado_email }}</span>
          </div>
          <div class="detail-row">
            <span class="label">Fecha inicio:</span>
            <span class="value">{{ formatDate(selectedVacation.fecha_inicio) }}</span>
          </div>
          <div class="detail-row">
            <span class="label">Fecha fin:</span>
            <span class="value">{{ formatDate(selectedVacation.fecha_fin) }}</span>
          </div>
          <div class="detail-row">
            <span class="label">Días solicitados:</span>
            <span class="value">{{ selectedVacation.dias_solicitados }} días</span>
          </div>
          <div class="detail-row">
            <span class="label">Gestión:</span>
            <span class="value">{{ selectedVacation.gestion }}</span>
          </div>
          <div class="detail-row">
            <span class="label">Estado:</span>
            <span class="value">
              <span class="status-badge" :class="`badge-${selectedVacation.estado}`">
                {{ selectedVacation.estado }}
              </span>
            </span>
          </div>
          <div v-if="selectedVacation.motivo_rechazo" class="detail-row">
            <span class="label">Motivo rechazo:</span>
            <span class="value rejection-text">{{ selectedVacation.motivo_rechazo }}</span>
          </div>
          <div class="detail-row">
            <span class="label">Solicitado:</span>
            <span class="value">{{ formatDateTime(selectedVacation.created_at) }}</span>
          </div>
          <div v-if="selectedVacation.updated_at" class="detail-row">
            <span class="label">Actualizado:</span>
            <span class="value">{{ formatDateTime(selectedVacation.updated_at) }}</span>
          </div>
        </div>
        <div class="form-actions">
          <button type="button" class="btn-secondary" @click="closeDetailsModal">
            Cerrar
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import vacationsService from '../services/vacations'

const loading = ref(false)
const vacations = ref([])
const selectedEstado = ref('')
const showRejectModal = ref(false)
const showDetailsModal = ref(false)
const selectedVacation = ref(null)
const rejectForm = ref({
  vacationId: null,
  motivo: ''
})

const formatDate = (dateStr) => {
  if (!dateStr) return ''
  const date = new Date(dateStr)
  return date.toLocaleDateString('es-ES', { year: 'numeric', month: 'short', day: 'numeric' })
}

const formatDateTime = (dateStr) => {
  if (!dateStr) return ''
  const date = new Date(dateStr)
  return date.toLocaleString('es-ES', { 
    year: 'numeric', 
    month: 'short', 
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

const loadVacations = async () => {
  try {
    loading.value = true
    const estado = selectedEstado.value || null
    const response = await vacationsService.getAllVacations(estado)
    vacations.value = response.vacaciones || []
  } catch (error) {
    console.error('Error al cargar vacaciones:', error)
    alert('Error al cargar solicitudes de vacaciones')
  } finally {
    loading.value = false
  }
}

const approveVacation = async (vacationId) => {
  if (!confirm('¿Estás seguro de aprobar esta solicitud de vacaciones?')) return
  
  try {
    loading.value = true
    await vacationsService.approveOrReject(vacationId, 'aprobado')
    alert('Solicitud aprobada exitosamente')
    await loadVacations()
  } catch (error) {
    console.error('Error al aprobar:', error)
    alert(error.message || 'Error al aprobar solicitud')
  } finally {
    loading.value = false
  }
}

const openRejectModal = (vacationId) => {
  rejectForm.value.vacationId = vacationId
  rejectForm.value.motivo = ''
  showRejectModal.value = true
}

const closeRejectModal = () => {
  showRejectModal.value = false
  rejectForm.value.vacationId = null
  rejectForm.value.motivo = ''
}

const rejectVacation = async () => {
  try {
    loading.value = true
    await vacationsService.approveOrReject(
      rejectForm.value.vacationId, 
      'rechazado', 
      rejectForm.value.motivo
    )
    alert('Solicitud rechazada')
    closeRejectModal()
    await loadVacations()
  } catch (error) {
    console.error('Error al rechazar:', error)
    alert(error.message || 'Error al rechazar solicitud')
  } finally {
    loading.value = false
  }
}

const showDetails = (vacation) => {
  selectedVacation.value = vacation
  showDetailsModal.value = true
}

const closeDetailsModal = () => {
  showDetailsModal.value = false
  selectedVacation.value = null
}

onMounted(() => {
  loadVacations()
})
</script>

<style scoped>
.admin-vacations-container {
  max-width: 1400px;
  margin: 0 auto;
  padding: 20px;
}

.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
}

.header h2 {
  margin: 0;
  color: #1e293b;
}

.filter-select {
  padding: 10px 16px;
  border: 1px solid #d1d5db;
  border-radius: 8px;
  font-size: 14px;
  background: white;
  cursor: pointer;
}

.loading, .empty-state {
  text-align: center;
  padding: 60px 20px;
  color: #6b7280;
  font-size: 16px;
}

.vacations-table-container {
  background: white;
  border-radius: 12px;
  box-shadow: 0 1px 3px rgba(0,0,0,0.1);
  overflow: hidden;
}

.vacations-table {
  width: 100%;
  border-collapse: collapse;
}

.vacations-table thead {
  background: #f8fafc;
}

.vacations-table th {
  padding: 16px;
  text-align: left;
  font-weight: 600;
  color: #374151;
  border-bottom: 2px solid #e5e7eb;
}

.vacations-table td {
  padding: 16px;
  border-bottom: 1px solid #f3f4f6;
}

.vacations-table tbody tr:hover {
  background: #f9fafb;
}

.row-pendiente {
  background: #fffbeb;
}

.row-aprobado {
  background: #f0fdf4;
}

.row-rechazado {
  background: #fef2f2;
}

.dates-cell div {
  font-size: 13px;
}

.dates-cell div:first-child {
  font-weight: 600;
  color: #059669;
}

.dates-cell div:last-child {
  color: #dc2626;
}

.status-badge {
  padding: 4px 12px;
  border-radius: 12px;
  font-size: 12px;
  font-weight: bold;
  text-transform: uppercase;
}

.badge-pendiente {
  background: #fef3c7;
  color: #92400e;
}

.badge-aprobado {
  background: #d1fae5;
  color: #065f46;
}

.badge-rechazado {
  background: #fee2e2;
  color: #991b1b;
}

.actions {
  display: flex;
  gap: 8px;
}

.btn-approve, .btn-reject, .btn-info {
  width: 32px;
  height: 32px;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  font-size: 16px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.btn-approve {
  background: #10b981;
  color: white;
}

.btn-approve:hover {
  background: #059669;
}

.btn-reject {
  background: #ef4444;
  color: white;
}

.btn-reject:hover {
  background: #dc2626;
}

.btn-info {
  background: #3b82f6;
  color: white;
}

.btn-info:hover {
  background: #2563eb;
}

.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0,0,0,0.5);
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
  color: #1e293b;
}

.form-group {
  margin-bottom: 16px;
}

.form-group label {
  display: block;
  margin-bottom: 8px;
  font-weight: 600;
  color: #374151;
}

.form-group textarea {
  width: 100%;
  padding: 10px;
  border: 1px solid #d1d5db;
  border-radius: 6px;
  font-size: 14px;
  font-family: inherit;
  resize: vertical;
}

.form-actions {
  display: flex;
  gap: 12px;
  justify-content: flex-end;
  margin-top: 20px;
}

.btn-danger {
  background: #ef4444;
  color: white;
  border: none;
  padding: 10px 20px;
  border-radius: 8px;
  cursor: pointer;
  font-weight: 600;
}

.btn-danger:hover {
  background: #dc2626;
}

.btn-danger:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.btn-secondary {
  background: #e5e7eb;
  color: #374151;
  border: none;
  padding: 10px 20px;
  border-radius: 8px;
  cursor: pointer;
  font-weight: 600;
}

.btn-secondary:hover {
  background: #d1d5db;
}

.details-content {
  margin: 20px 0;
}

.detail-row {
  display: flex;
  justify-content: space-between;
  padding: 12px 0;
  border-bottom: 1px solid #f3f4f6;
}

.detail-row .label {
  font-weight: 600;
  color: #6b7280;
}

.detail-row .value {
  color: #1e293b;
}

.rejection-text {
  color: #991b1b;
  font-style: italic;
}
</style>
