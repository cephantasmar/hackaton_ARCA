<!-- filepath: c:\Users\CESAR\Documents\UNIVERSIDAD\8 semestre\ARQ_SOFT\h1\front\src\views\Contracts.vue -->
<template>
  <div class="contracts-container">
    <div class="header">
      <h1>Gestión de Contratos</h1>
      <button @click="showCreateModal = true" class="btn-primary">
        <i class="fas fa-plus"></i> Nuevo Contrato
      </button>
    </div>

    <div class="stats">
      <div class="stat-card">
        <i class="fas fa-file-contract"></i>
        <div>
          <p>Total Contratos</p>
          <h2>{{ contratos.length }}</h2>
        </div>
      </div>
      <div class="stat-card">
        <i class="fas fa-check-circle"></i>
        <div>
          <p>Contratos Activos</p>
          <h2>{{ contratosActivos }}</h2>
        </div>
      </div>
    </div>

    <div class="contracts-table">
      <table>
        <thead>
          <tr>
            <th>Funcionario</th>
            <th>Cargo</th>
            <th>Fecha Inicio</th>
            <th>Salario</th>
            <th>Tiempo Prueba</th>
            <th>Estado</th>
            <th>Acciones</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="contrato in contratos" :key="contrato.id">
            <td>{{ contrato.nombre }} {{ contrato.apellido }}</td>
            <td>{{ contrato.cargo || 'N/A' }}</td>
            <td>{{ formatDate(contrato.fecha_inicio) }}</td>
            <td>${{ formatNumber(contrato.salario) }}</td>
            <td>{{ contrato.tiempo_prueba }} días</td>
            <td>
              <span :class="['badge', contrato.activo ? 'badge-active' : 'badge-inactive']">
                {{ contrato.activo ? 'Activo' : 'Inactivo' }}
              </span>
            </td>
            <td>
              <button @click="viewContract(contrato)" class="btn-icon" title="Ver detalles">
                <i class="fas fa-eye"></i> 
              </button>
              <button @click="editContract(contrato)" class="btn-icon btn-edit" title="Editar">
                <i class="fas fa-edit"></i> editar
              </button>
              <button @click="deleteContract(contrato.id)" class="btn-icon btn-danger" title="Eliminar">
                <i class="fas fa-trash"></i> eliminar
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Modal Crear/Editar Contrato -->
    <div v-if="showCreateModal || showEditModal" class="modal" @click.self="closeModals">
      <div class="modal-content">
        <div class="modal-header">
          <h2>{{ showEditModal ? 'Editar Contrato' : 'Nuevo Contrato' }}</h2>
          <button @click="closeModals" class="btn-close">&times;</button>
        </div>
        <form @submit.prevent="submitForm">
          <div class="form-group" v-if="!showEditModal">
            <label>Funcionario *</label>
            <select v-model="formData.usuario_id" required>
              <option value="">Seleccione un funcionario</option>
              <option v-for="user in usuariosSinContrato" :key="user.id" :value="user.id">
                {{ user.nombre }} {{ user.apellido }} - {{ user.email }}
              </option>
            </select>
            <small v-if="usuariosSinContrato.length === 0" class="form-hint warning">
              ⚠️ No hay funcionarios disponibles. Todos tienen contrato activo.
            </small>
            <small v-else class="form-hint">
              ℹ️ Solo se muestran funcionarios sin contrato activo
            </small>
          </div>
          <div class="form-group">
            <label>Fecha de Inicio *</label>
            <input type="date" v-model="formData.fecha_inicio" required />
          </div>
          <div class="form-group">
            <label>Salario *</label>
            <input type="number" step="0.01" min="0" v-model="formData.salario" required />
          </div>
          <div class="form-group">
            <label>Tiempo de Prueba (días) *</label>
            <input type="number" min="1" v-model.number="formData.tiempo_prueba" required />
          </div>
          <div class="form-group" v-if="showEditModal">
            <label class="checkbox-label">
              <input type="checkbox" v-model="formData.activo" />
              Contrato Activo
            </label>
          </div>
          <div class="modal-actions">
            <button type="button" @click="closeModals" class="btn-secondary">Cancelar</button>
            <button type="submit" class="btn-primary" :disabled="loading">
              {{ loading ? 'Procesando...' : (showEditModal ? 'Actualizar' : 'Crear') }}
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script>
export default {
  name: 'Contracts',
  data() {
    return {
      contratos: [],
      usuarios: [],
      showCreateModal: false,
      showEditModal: false,
      selectedContrato: null,
      loading: false,
      formData: {
        usuario_id: '',
        fecha_inicio: '',
        salario: '',
        tiempo_prueba: 30,
        activo: true
      }
    }
  },
  computed: {
    contratosActivos() {
      return this.contratos.filter(c => c.activo).length
    },
    usuariosSinContrato() {
      // Filtrar usuarios que no tienen contrato activo
      const usuariosConContratoActivo = this.contratos
        .filter(c => c.activo)
        .map(c => c.usuario_id)
      
      return this.usuarios.filter(u => !usuariosConContratoActivo.includes(u.id))
    }
  },
  async mounted() {
    await this.loadContratos()
    await this.loadUsuarios()
  },
  methods: {
    async loadContratos() {
      try {
        const token = localStorage.getItem('token')
        const backendUrl = window.location.hostname === 'localhost' 
          ? 'http://localhost:5003' 
          : '/api/contracts'
        
        const response = await fetch(`${backendUrl}/api/contracts/`, {
          headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
          }
        })
        
        if (response.ok) {
          this.contratos = await response.json()
        } else {
          console.error('Error loading contracts:', response.statusText)
        }
      } catch (error) {
        console.error('Error loading contracts:', error)
        alert('Error al cargar los contratos')
      }
    },
    async loadUsuarios() {
      try {
        const token = localStorage.getItem('token')
        const backendUrl = window.location.hostname === 'localhost' 
          ? 'http://localhost:5003' 
          : '/api/contracts'
        
        const response = await fetch(`${backendUrl}/api/auth/users`, {
          headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
          }
        })
        
        if (response.ok) {
          this.usuarios = await response.json()
        }
      } catch (error) {
        console.error('Error loading users:', error)
      }
    },
    async submitForm() {
      if (this.loading) return
      
      this.loading = true
      try {
        const token = localStorage.getItem('token')
        const backendUrl = window.location.hostname === 'localhost' 
          ? 'http://localhost:5003' 
          : '/api/contracts'
        
        const url = this.showEditModal 
          ? `${backendUrl}/api/contracts/${this.selectedContrato.id}`
          : `${backendUrl}/api/contracts/`
        
        const method = this.showEditModal ? 'PUT' : 'POST'
        
        const response = await fetch(url, {
          method,
          headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
          },
          body: JSON.stringify(this.formData)
        })
        
        if (response.ok) {
          await this.loadContratos()
          this.closeModals()
          alert(this.showEditModal ? 'Contrato actualizado exitosamente' : 'Contrato creado exitosamente')
        } else {
          const error = await response.json()
          alert(`Error: ${error.detail || 'No se pudo guardar el contrato'}`)
        }
      } catch (error) {
        console.error('Error saving contract:', error)
        alert('Error al guardar el contrato')
      } finally {
        this.loading = false
      }
    },
    async deleteContract(id) {
      if (!confirm('¿Está seguro de eliminar este contrato?')) return
      
      try {
        const token = localStorage.getItem('token')
        const backendUrl = window.location.hostname === 'localhost' 
          ? 'http://localhost:5003' 
          : '/api/contracts'
        
        const response = await fetch(`${backendUrl}/api/contracts/${id}`, {
          method: 'DELETE',
          headers: {
            'Authorization': `Bearer ${token}`
          }
        })
        
        if (response.ok) {
          await this.loadContratos()
          alert('Contrato eliminado exitosamente')
        } else {
          alert('Error al eliminar el contrato')
        }
      } catch (error) {
        console.error('Error deleting contract:', error)
        alert('Error al eliminar el contrato')
      }
    },
    viewContract(contrato) {
      this.$router.push(`/contracts/${contrato.id}`)
    },
    editContract(contrato) {
      this.selectedContrato = contrato
      this.formData = {
        fecha_inicio: contrato.fecha_inicio,
        salario: contrato.salario,
        tiempo_prueba: contrato.tiempo_prueba,
        activo: contrato.activo
      }
      this.showEditModal = true
    },
    closeModals() {
      this.showCreateModal = false
      this.showEditModal = false
      this.selectedContrato = null
      this.formData = {
        usuario_id: '',
        fecha_inicio: '',
        salario: '',
        tiempo_prueba: 30,
        activo: true
      }
    },
    formatDate(date) {
      if (!date) return 'N/A'
      return new Date(date).toLocaleDateString('es-ES')
    },
    formatNumber(num) {
      if (!num) return '0.00'
      return Number(num).toLocaleString('es-ES', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
    }
  }
}
</script>

<style scoped>
.contracts-container {
  padding: 2rem;
  max-width: 1400px;
  margin: 0 auto;
}

.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;
}

.header h1 {
  color: #333;
  margin: 0;
}

.stats {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 1rem;
  margin-bottom: 2rem;
}

.stat-card {
  background: white;
  padding: 1.5rem;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
  display: flex;
  align-items: center;
  gap: 1rem;
}

.stat-card i {
  font-size: 2.5rem;
  color: #4CAF50;
}

.stat-card p {
  margin: 0;
  color: #666;
  font-size: 0.875rem;
}

.stat-card h2 {
  margin: 0.25rem 0 0 0;
  color: #333;
  font-size: 2rem;
}

.contracts-table {
  background: white;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
  overflow-x: auto;
}

table {
  width: 100%;
  border-collapse: collapse;
}

th, td {
  padding: 1rem;
  text-align: left;
  border-bottom: 1px solid #eee;
}

th {
  background: #f5f5f5;
  font-weight: 600;
  color: #333;
}

tr:hover {
  background: #fafafa;
}

.badge {
  padding: 0.25rem 0.75rem;
  border-radius: 12px;
  font-size: 0.875rem;
  font-weight: 500;
  display: inline-block;
}

.badge-active {
  background: #e8f5e9;
  color: #2e7d32;
}

.badge-inactive {
  background: #ffebee;
  color: #c62828;
}

.btn-primary {
  background: #4CAF50;
  color: white;
  padding: 0.75rem 1.5rem;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 1rem;
  transition: background 0.3s;
}

.btn-primary:hover:not(:disabled) {
  background: #45a049;
}

.btn-primary:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.btn-icon {
  background: none;
  border: none;
  cursor: pointer;
  padding: 0.5rem;
  color: #666;
  font-size: 1rem;
  transition: color 0.3s;
}

.btn-icon:hover {
  color: #4CAF50;
}

.btn-edit:hover {
  color: #2196F3;
}

.btn-danger:hover {
  color: #f44336;
}

.form-hint {
  display: block;
  margin-top: 0.5rem;
  font-size: 0.875rem;
  color: #666;
}

.form-hint.warning {
  color: #ff9800;
  font-weight: 500;
}

.modal {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: rgba(0,0,0,0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.modal-content {
  background: white;
  padding: 2rem;
  border-radius: 8px;
  width: 90%;
  max-width: 500px;
  max-height: 90vh;
  overflow-y: auto;
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
}

.modal-header h2 {
  margin: 0;
  color: #333;
}

.btn-close {
  background: none;
  border: none;
  font-size: 2rem;
  cursor: pointer;
  color: #666;
  line-height: 1;
  padding: 0;
}

.btn-close:hover {
  color: #333;
}

.form-group {
  margin-bottom: 1rem;
}

.form-group label {
  display: block;
  margin-bottom: 0.5rem;
  font-weight: 500;
  color: #333;
}

.form-group input,
.form-group select {
  width: 100%;
  padding: 0.75rem;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 1rem;
  box-sizing: border-box;
}

.form-group input:focus,
.form-group select:focus {
  outline: none;
  border-color: #4CAF50;
}

.checkbox-label {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  cursor: pointer;
}

.checkbox-label input[type="checkbox"] {
  width: auto;
  cursor: pointer;
}

.modal-actions {
  display: flex;
  gap: 1rem;
  justify-content: flex-end;
  margin-top: 1.5rem;
}

.btn-secondary {
  background: #9e9e9e;
  color: white;
  padding: 0.75rem 1.5rem;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  transition: background 0.3s;
}

.btn-secondary:hover {
  background: #757575;
}
</style>