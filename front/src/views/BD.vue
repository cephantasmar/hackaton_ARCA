<template>
  <div class="dashboard-container">
    <!-- Header -->
    <div class="dashboard-header">
      <div class="header-content">
        <h1 class="title">Gestión de Usuarios</h1>
        <div class="user-info">
          <span class="user-email">{{ currentUserEmail }}</span>
        </div>
      </div>
    </div>

    <!-- Alertas -->
    <div v-if="mensajeAlerta.show" :class="['alert', mensajeAlerta.type]">
      <span class="alert-icon">{{ mensajeAlerta.type === 'success' ? '✓' : '⚠' }}</span>
      {{ mensajeAlerta.text }}
      <button class="alert-close" @click="mensajeAlerta.show = false">×</button>
    </div>

    <!-- Panel de Control -->
    <div class="control-panel">
      <div class="panel-header">
        <h2>Panel de Control</h2>
        <div class="control-buttons">
          <button 
            @click="cargarUsuarios" 
            class="btn btn-primary btn-icon" 
            :disabled="cargando"
          >
            <span class="btn-icon">🔄</span>
            {{ cargando ? 'Cargando...' : 'Actualizar' }}
          </button>
          
          <button 
            @click="mostrarModalCrear = true" 
            class="btn btn-success btn-icon"
          >
            <span class="btn-icon">➕</span>
            Crear Usuario
          </button>
        </div>
      </div>
      
      <!-- Filtros y Búsqueda -->
      <div class="filters-section">
        <div class="filter-group">
          <label class="filter-label">Filtrar por Rol:</label>
          <select v-model="filtroRol" @change="aplicarFiltros" class="filter-select">
            <option value="">Todos los roles</option>
            <option value="Admin">Admin</option>
            <option value="Empleado">Empleado</option>
          </select>
        </div>
        
        <div class="filter-group">
          <label class="filter-label">Filtrar por Cargo:</label>
          <input 
            v-model="filtroCargo" 
            @input="aplicarFiltros"
            placeholder="Todos los cargos" 
            class="filter-input"
          >
        </div>
        
        <div class="search-group">
          <input 
            v-model="filtroBusqueda" 
            @input="aplicarBusqueda"
            placeholder="Buscar por nombre, email, cargo..." 
            class="search-input"
          >
          <span class="search-icon">🔍</span>
        </div>
      </div>

      <!-- Stats -->
      <div class="stats-section">
        <div class="stat-card" @click="filtroRol = ''; aplicarFiltros()" :class="{ active: !filtroRol }">
          <div class="stat-icon">👥</div>
          <div class="stat-content">
            <span class="stat-number">{{ usuarios.length }}</span>
            <span class="stat-label">Total Usuarios</span>
          </div>
        </div>
        <div class="stat-card" @click="filtroRol = 'Admin'; aplicarFiltros()" :class="{ active: filtroRol === 'Admin' }">
          <div class="stat-icon">👑</div>
          <div class="stat-content">
            <span class="stat-number">{{ contarPorRol('Admin') }}</span>
            <span class="stat-label">Administradores</span>
          </div>
        </div>
        <div class="stat-card" @click="filtroRol = 'Empleado'; aplicarFiltros()" :class="{ active: filtroRol === 'Empleado' }">
          <div class="stat-icon">💼</div>
          <div class="stat-content">
            <span class="stat-number">{{ contarPorRol('Empleado') }}</span>
            <span class="stat-label">Empleados</span>
          </div>
        </div>
      </div>
    </div>

    <!-- Estado de Carga -->
    <div v-if="cargando" class="loading-state">
      <div class="spinner"></div>
      <p>Cargando usuarios...</p>
    </div>

    <div v-if="error && !cargando" class="error-state">
      <div class="error-icon">⚠️</div>
      <h3>Error al cargar usuarios</h3>
      <p>{{ error }}</p>
      <button @click="cargarUsuarios" class="btn btn-outline">Reintentar</button>
    </div>

    <!-- Tabla de Usuarios -->
    <div v-if="!cargando && usuariosFiltrados.length > 0" class="table-section">
      <div class="table-header">
        <h3>Lista de Usuarios</h3>
        <div class="table-info">
          <span class="user-count">{{ usuariosFiltrados.length }} usuarios</span>
          <span v-if="filtroRol || filtroCargo" class="filter-indicator">
            Filtros: 
            <span v-if="filtroRol" class="filter-tag">{{ filtroRol }}</span>
            <span v-if="filtroRol && filtroCargo" class="filter-separator">•</span>
            <span v-if="filtroCargo" class="filter-tag">{{ filtroCargo }}</span>
            <button @click="limpiarFiltros" class="clear-filter">Limpiar</button>
          </span>
        </div>
      </div>

      <div class="table-container">
        <table class="users-table">
          <thead>
            <tr>
              <th class="col-name">Nombre</th>
              <th class="col-email">Email</th>
              <th class="col-cargo">Cargo</th>
              <th class="col-remuneracion">Remuneración</th>
              <th class="col-role">Rol</th>
              <th class="col-actions">Acciones</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="usuario in usuariosFiltrados" :key="usuario.id" class="user-row">
              <td class="user-name">
                <div class="avatar">{{ getInitials(usuario.nombre, usuario.apellido) }}</div>
                <div class="name-info">
                  <strong>{{ usuario.nombre || 'N/A' }} {{ usuario.apellido || '' }}</strong>
                  <small>ID: {{ usuario.id?.substring(0, 8) }}...</small>
                </div>
              </td>
              <td class="user-email">
                <span class="email-text">{{ usuario.email }}</span>
              </td>
              <td class="user-cargo">
                <span class="cargo-badge">{{ usuario.cargo || 'Sin cargo' }}</span>
              </td>
              <td class="user-remuneracion">
                <span class="remuneracion-value">
                  {{ usuario.remuneracion ? `$${usuario.remuneracion.toLocaleString()}` : 'No especificado' }}
                </span>
              </td>
              <td class="user-role">
                <select 
                  v-model="usuario.rol" 
                  @change="actualizarRol(usuario)"
                  :class="['role-select', `role-${usuario.rol?.toLowerCase()}`]"
                  :disabled="actualizandoId === usuario.id"
                >
                  <option value="Admin">Administrador</option>
                  <option value="Empleado">Empleado</option>
                </select>
                <div v-if="actualizandoId === usuario.id" class="update-spinner"></div>
              </td>
              <td class="user-actions">
                <button 
                  @click="eliminarUsuario(usuario)" 
                  class="btn btn-danger btn-sm"
                  :disabled="actualizandoId === usuario.id"
                >
                  🗑️ Eliminar
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Estado Sin Datos -->
    <div v-if="!cargando && usuariosFiltrados.length === 0" class="empty-state">
      <div class="empty-icon">👥</div>
      <h3>No hay usuarios registrados</h3>
      <p v-if="filtroRol || filtroCargo">No se encontraron usuarios con los filtros aplicados</p>
      <p v-else>Comienza agregando el primer usuario al sistema</p>
      <button @click="mostrarModalCrear = true" class="btn btn-success btn-lg">
        ➕ Crear Primer Usuario
      </button>
    </div>

    <!-- Modal Crear Usuario -->
    <div v-if="mostrarModalCrear" class="modal-overlay">
      <div class="modal">
        <div class="modal-header">
          <h3>Crear Nuevo Usuario</h3>
          <button @click="cerrarModal" class="modal-close">×</button>
        </div>
        
        <div class="modal-body">
          <form @submit.prevent="crearUsuario" class="user-form">
            <div class="form-row">
              <div class="form-group">
                <label for="nombre">Nombre *</label>
                <input 
                  id="nombre"
                  v-model="nuevoUsuario.nombre" 
                  type="text" 
                  required
                  placeholder="Ingresa el nombre"
                  class="form-input"
                >
              </div>
              
              <div class="form-group">
                <label for="apellido">Apellido *</label>
                <input 
                  id="apellido"
                  v-model="nuevoUsuario.apellido" 
                  type="text" 
                  required
                  placeholder="Ingresa el apellido"
                  class="form-input"
                >
              </div>
            </div>
            
            <div class="form-group">
              <label for="email">Email *</label>
              <input 
                id="email"
                v-model="nuevoUsuario.email" 
                type="email" 
                required
                placeholder="usuario@empresa.com"
                class="form-input"
              >
            </div>

            <div class="form-row">
              <div class="form-group">
                <label for="cargo">Cargo</label>
                <input 
                  id="cargo"
                  v-model="nuevoUsuario.cargo" 
                  type="text" 
                  placeholder="Ej: Desarrollador, Diseñador..."
                  class="form-input"
                >
              </div>
              
              <div class="form-group">
                <label for="remuneracion">Remuneración</label>
                <input 
                  id="remuneracion"
                  v-model="nuevoUsuario.remuneracion" 
                  type="number"
                  step="0.01"
                  min="0"
                  placeholder="0.00"
                  class="form-input"
                >
              </div>
            </div>
            
            <div class="form-group">
              <label for="rol">Rol *</label>
              <select 
                id="rol"
                v-model="nuevoUsuario.rol" 
                required
                class="form-select"
              >
                <option value="Empleado">Empleado</option>
                <option value="Admin">Administrador</option>
              </select>
            </div>
          </form>
        </div>
        
        <div class="modal-footer">
          <button @click="cerrarModal" class="btn btn-outline">Cancelar</button>
          <button 
            @click="crearUsuario" 
            :disabled="creandoUsuario" 
            class="btn btn-success"
          >
            <span v-if="creandoUsuario" class="btn-spinner"></span>
            {{ creandoUsuario ? 'Creando...' : 'Crear Usuario' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { ref, onMounted, computed } from 'vue'
import { supabase } from '../supabase'

export default {
  name: 'BD',
  setup() {
    // Estados reactivos
    const usuarios = ref([])
    const currentUserEmail = ref('')
    const cargando = ref(false)
    const error = ref('')
    const actualizandoId = ref(null)
    const creandoUsuario = ref(false)
    const mostrarModalCrear = ref(false)
    const filtroBusqueda = ref('')
    const filtroRol = ref('')
    const filtroCargo = ref('')
    const mensajeAlerta = ref({
      show: false,
      text: '',
      type: 'success'
    })

    const API_BASE_URL = 'http://localhost:5009/api'

    // Nuevo usuario
    const nuevoUsuario = ref({
      nombre: '',
      apellido: '',
      email: '',
      rol: 'Empleado',
      cargo: '',
      remuneracion: null
    })

    // Computed
    const usuariosFiltrados = computed(() => {
      let filtered = usuarios.value
      
      // Aplicar filtro de búsqueda
      if (filtroBusqueda.value) {
        const search = filtroBusqueda.value.toLowerCase()
        filtered = filtered.filter(usuario => 
          usuario.nombre?.toLowerCase().includes(search) ||
          usuario.apellido?.toLowerCase().includes(search) ||
          usuario.email?.toLowerCase().includes(search) ||
          usuario.cargo?.toLowerCase().includes(search) ||
          usuario.rol?.toLowerCase().includes(search)
        )
      }
      
      // Aplicar filtro de rol
      if (filtroRol.value) {
        filtered = filtered.filter(usuario => usuario.rol === filtroRol.value)
      }
      
      // Aplicar filtro de cargo
      if (filtroCargo.value) {
        filtered = filtered.filter(usuario => 
          usuario.cargo?.toLowerCase().includes(filtroCargo.value.toLowerCase())
        )
      }
      
      return filtered
    })

    // Funciones
    const getInitials = (nombre, apellido) => {
      return ((nombre?.[0] || '') + (apellido?.[0] || '')).toUpperCase()
    }

    const mostrarAlerta = (text, type = 'success') => {
      mensajeAlerta.value = { show: true, text, type }
      setTimeout(() => {
        mensajeAlerta.value.show = false
      }, 5000)
    }

    const contarPorRol = (rol) => {
      return usuarios.value.filter(u => u.rol === rol).length
    }

    const aplicarFiltros = async () => {
      cargando.value = true
      error.value = ''
      
      try {
        let url = `${API_BASE_URL}/usuarios/filtrar`
        const params = new URLSearchParams()
        
        if (filtroRol.value) params.append('rol', filtroRol.value)
        if (filtroCargo.value) params.append('cargo', filtroCargo.value)
        
        if (params.toString()) {
          url += `?${params.toString()}`
        }

        const response = await fetch(url)
        
        if (!response.ok) throw new Error(`Error HTTP: ${response.status}`)
        
        const data = await response.json()
        usuarios.value = data.usuarios
        
      } catch (err) {
        error.value = `Error al cargar usuarios: ${err.message}`
        console.error('Error:', err)
      } finally {
        cargando.value = false
      }
    }

    const aplicarBusqueda = async () => {
      if (!filtroBusqueda.value) {
        await cargarUsuarios()
        return
      }

      cargando.value = true
      error.value = ''
      
      try {
        const response = await fetch(`${API_BASE_URL}/usuarios/buscar?search=${encodeURIComponent(filtroBusqueda.value)}`)
        
        if (!response.ok) throw new Error(`Error HTTP: ${response.status}`)
        
        const data = await response.json()
        usuarios.value = data.usuarios
        
      } catch (err) {
        error.value = `Error al buscar usuarios: ${err.message}`
        console.error('Error:', err)
      } finally {
        cargando.value = false
      }
    }

    const limpiarFiltros = () => {
      filtroRol.value = ''
      filtroCargo.value = ''
      filtroBusqueda.value = ''
      cargarUsuarios()
    }

    const cargarUsuarios = async () => {
      cargando.value = true
      error.value = ''
      
      try {
        const response = await fetch(`${API_BASE_URL}/usuarios`)
        
        if (!response.ok) throw new Error(`Error HTTP: ${response.status}`)
        
        const data = await response.json()
        usuarios.value = data.usuarios
        
        // Obtener email del usuario actual
        const { data: { session } } = await supabase.auth.getSession()
        if (session?.user?.email) {
          currentUserEmail.value = session.user.email
        }
        
      } catch (err) {
        error.value = `Error al cargar usuarios: ${err.message}`
        console.error('Error:', err)
      } finally {
        cargando.value = false
      }
    }

    const actualizarRol = async (usuario) => {
      actualizandoId.value = usuario.id
      
      try {
        const response = await fetch(`${API_BASE_URL}/usuarios/${usuario.id}/rol`, {
          method: 'PUT',
          headers: {
            'Content-Type': 'application/json'
          },
          body: JSON.stringify({ rol: usuario.rol })
        })

        if (!response.ok) throw new Error(`Error HTTP: ${response.status}`)

        mostrarAlerta(`Rol actualizado correctamente para ${usuario.nombre}`, 'success')
        
      } catch (err) {
        console.error('Error actualizando rol:', err)
        // Recargar para revertir cambios
        await cargarUsuarios()
        mostrarAlerta(`Error al actualizar rol: ${err.message}`, 'error')
      } finally {
        actualizandoId.value = null
      }
    }

    const eliminarUsuario = async (usuario) => {
      if (!confirm(`¿Estás seguro de que quieres eliminar a ${usuario.nombre} ${usuario.apellido}? Esta acción no se puede deshacer.`)) {
        return
      }

      actualizandoId.value = usuario.id
      
      try {
        const response = await fetch(`${API_BASE_URL}/usuarios/${usuario.id}`, {
          method: 'DELETE'
        })

        if (!response.ok) throw new Error(`Error HTTP: ${response.status}`)

        // Eliminar localmente
        usuarios.value = usuarios.value.filter(u => u.id !== usuario.id)
        mostrarAlerta(`Usuario ${usuario.nombre} eliminado correctamente`, 'success')
        
      } catch (err) {
        console.error('Error eliminando usuario:', err)
        mostrarAlerta(`Error al eliminar usuario: ${err.message}`, 'error')
      } finally {
        actualizandoId.value = null
      }
    }

    const crearUsuario = async () => {
      if (!nuevoUsuario.value.nombre || !nuevoUsuario.value.apellido || !nuevoUsuario.value.email) {
        mostrarAlerta('Nombre, apellido y email son requeridos', 'error')
        return
      }

      creandoUsuario.value = true
      
      try {
        const response = await fetch(`${API_BASE_URL}/usuarios`, {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json'
          },
          body: JSON.stringify(nuevoUsuario.value)
        })

        if (!response.ok) {
          const errorText = await response.text()
          throw new Error(`Error HTTP: ${response.status} - ${errorText}`)
        }

        const resultado = await response.json()
        mostrarAlerta('Usuario creado correctamente', 'success')
        
        // Cerrar modal y recargar usuarios
        cerrarModal()
        await cargarUsuarios()
        
      } catch (err) {
        console.error('Error creando usuario:', err)
        mostrarAlerta(`Error al crear usuario: ${err.message}`, 'error')
      } finally {
        creandoUsuario.value = false
      }
    }

    const cerrarModal = () => {
      mostrarModalCrear.value = false
      // Resetear el formulario
      nuevoUsuario.value = {
        nombre: '',
        apellido: '',
        email: '',
        rol: 'Empleado',
        cargo: '',
        remuneracion: null
      }
    }

    onMounted(() => {
      cargarUsuarios()
    })

    return {
      usuarios,
      usuariosFiltrados,
      currentUserEmail,
      cargando,
      error,
      actualizandoId,
      creandoUsuario,
      mostrarModalCrear,
      filtroBusqueda,
      filtroRol,
      filtroCargo,
      mensajeAlerta,
      nuevoUsuario,
      cargarUsuarios,
      actualizarRol,
      eliminarUsuario,
      crearUsuario,
      cerrarModal,
      contarPorRol,
      mostrarAlerta,
      aplicarFiltros,
      aplicarBusqueda,
      limpiarFiltros,
      getInitials
    }
  }
}
</script>

<style scoped>
/* Estilos generales */
.dashboard-container {
  min-height: 100vh;
  background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%);
  padding: 20px;
  font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
}

/* Header */
.dashboard-header {
  background: white;
  border-radius: 12px;
  padding: 24px;
  margin-bottom: 24px;
  box-shadow: 0 2px 10px rgba(0,0,0,0.08);
  border: 1px solid #e1e5e9;
}

.header-content {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.title {
  color: #2c3e50;
  font-size: 2rem;
  font-weight: 700;
  margin: 0;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
}

.user-info {
  display: flex;
  align-items: center;
  gap: 12px;
}

.user-email {
  background: #f8f9fa;
  padding: 8px 16px;
  border-radius: 20px;
  font-weight: 500;
  color: #6c757d;
  border: 1px solid #e9ecef;
}

/* Alertas */
.alert {
  display: flex;
  align-items: center;
  padding: 16px 20px;
  border-radius: 8px;
  margin-bottom: 20px;
  font-weight: 500;
  gap: 12px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.1);
}

.alert.success {
  background: #d4edda;
  color: #155724;
  border: 1px solid #c3e6cb;
}

.alert.error {
  background: #f8d7da;
  color: #721c24;
  border: 1px solid #f5c6cb;
}

.alert-icon {
  font-weight: bold;
}

.alert-close {
  margin-left: auto;
  background: none;
  border: none;
  font-size: 1.2rem;
  cursor: pointer;
  opacity: 0.7;
}

.alert-close:hover {
  opacity: 1;
}

/* Panel de Control */
.control-panel {
  background: white;
  border-radius: 12px;
  padding: 24px;
  margin-bottom: 24px;
  box-shadow: 0 2px 10px rgba(0,0,0,0.08);
  border: 1px solid #e1e5e9;
}

.panel-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

.panel-header h2 {
  color: #2c3e50;
  font-size: 1.5rem;
  font-weight: 600;
  margin: 0;
}

.control-buttons {
  display: flex;
  gap: 12px;
}

/* Filtros */
.filters-section {
  display: flex;
  gap: 20px;
  align-items: center;
  margin-bottom: 24px;
  flex-wrap: wrap;
}

.filter-group {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.filter-label {
  font-weight: 600;
  color: #495057;
  font-size: 0.9rem;
}

.filter-select, .filter-input {
  padding: 10px 12px;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  font-size: 0.9rem;
  background: white;
  color: #2d3748;
  transition: all 0.3s ease;
  min-width: 150px;
}

.filter-select:focus, .filter-input:focus {
  outline: none;
  border-color: #667eea;
  box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
}

.search-group {
  position: relative;
  display: flex;
  align-items: center;
}

.search-input {
  padding: 10px 12px 10px 40px;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  font-size: 0.9rem;
  width: 280px;
  color: #2d3748;
  transition: all 0.3s ease;
}

.search-input:focus {
  outline: none;
  border-color: #667eea;
  box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
}

.search-icon {
  position: absolute;
  left: 12px;
  color: #6c757d;
}

/* Stats */
.stats-section {
  display: flex;
  gap: 16px;
}

.stat-card {
  background: white;
  padding: 20px;
  border-radius: 12px;
  flex: 1;
  cursor: pointer;
  transition: all 0.3s ease;
  border: 2px solid transparent;
  box-shadow: 0 2px 8px rgba(0,0,0,0.06);
  display: flex;
  align-items: center;
  gap: 16px;
}

.stat-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0,0,0,0.1);
}

.stat-card.active {
  border-color: #667eea;
  background: #f8f9ff;
}

.stat-icon {
  font-size: 2rem;
  opacity: 0.8;
}

.stat-content {
  display: flex;
  flex-direction: column;
}

.stat-number {
  font-size: 1.75rem;
  font-weight: 700;
  color: #2c3e50;
}

.stat-label {
  font-size: 0.85rem;
  color: #6c757d;
  font-weight: 500;
}

/* Botones */
.btn {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  padding: 10px 20px;
  border: none;
  border-radius: 8px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
  text-decoration: none;
  font-size: 0.9rem;
}

.btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.btn-primary {
  background: #667eea;
  color: white;
}

.btn-primary:hover:not(:disabled) {
  background: #5a6fd8;
  transform: translateY(-1px);
}

.btn-success {
  background: #28a745;
  color: white;
}

.btn-success:hover:not(:disabled) {
  background: #218838;
  transform: translateY(-1px);
}

.btn-danger {
  background: #dc3545;
  color: white;
  padding: 8px 16px;
  font-size: 0.8rem;
}

.btn-danger:hover:not(:disabled) {
  background: #c82333;
}

.btn-outline {
  background: transparent;
  border: 1px solid #6c757d;
  color: #6c757d;
}

.btn-outline:hover {
  background: #6c757d;
  color: white;
}

.btn-sm {
  padding: 6px 12px;
  font-size: 0.8rem;
}

.btn-lg {
  padding: 12px 24px;
  font-size: 1rem;
}

.btn-icon {
  font-size: 1rem;
}

.btn-spinner {
  width: 16px;
  height: 16px;
  border: 2px solid transparent;
  border-top: 2px solid currentColor;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

/* Estados */
.loading-state, .error-state, .empty-state {
  text-align: center;
  padding: 60px 20px;
  background: white;
  border-radius: 12px;
  box-shadow: 0 2px 10px rgba(0,0,0,0.08);
  border: 1px solid #e1e5e9;
}

.spinner {
  width: 40px;
  height: 40px;
  border: 4px solid #f3f3f3;
  border-top: 4px solid #667eea;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin: 0 auto 20px;
}

.error-icon, .empty-icon {
  font-size: 3rem;
  margin-bottom: 20px;
  opacity: 0.7;
}

.error-state h3, .empty-state h3 {
  color: #2c3e50;
  margin-bottom: 10px;
}

.error-state p, .empty-state p {
  color: #6c757d;
  margin-bottom: 20px;
}

/* Tabla */
.table-section {
  background: white;
  border-radius: 12px;
  overflow: hidden;
  box-shadow: 0 2px 10px rgba(0,0,0,0.08);
  border: 1px solid #e1e5e9;
}

.table-header {
  padding: 20px 24px;
  border-bottom: 1px solid #e9ecef;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.table-header h3 {
  margin: 0;
  color: #2c3e50;
  font-weight: 600;
}

.table-info {
  display: flex;
  align-items: center;
  gap: 16px;
}

.user-count {
  color: #6c757d;
  font-weight: 500;
}

.filter-indicator {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 0.9rem;
  color: #6c757d;
}

.filter-tag {
  background: #e3f2fd;
  color: #1976d2;
  padding: 4px 8px;
  border-radius: 6px;
  font-weight: 500;
}

.filter-separator {
  color: #adb5bd;
}

.clear-filter {
  background: none;
  border: none;
  color: #667eea;
  cursor: pointer;
  font-size: 0.8rem;
  text-decoration: underline;
}

.clear-filter:hover {
  color: #5a6fd8;
}

.table-container {
  overflow-x: auto;
}

.users-table {
  width: 100%;
  border-collapse: collapse;
}

.users-table th {
  background: #f8f9fa;
  padding: 16px 12px;
  text-align: left;
  font-weight: 600;
  color: #495057;
  border-bottom: 1px solid #e9ecef;
  white-space: nowrap;
  font-size: 0.9rem;
}

.users-table td {
  padding: 16px 12px;
  border-bottom: 1px solid #e9ecef;
  color: #2d3748;
  vertical-align: middle;
}

.users-table tr:hover {
  background: #f8f9fa;
}

/* Columnas */
.col-name { width: 220px; }
.col-email { width: 240px; }
.col-cargo { width: 160px; }
.col-remuneracion { width: 140px; }
.col-role { width: 160px; }
.col-actions { width: 120px; }

/* Filas de usuario */
.user-row {
  transition: all 0.2s ease;
}

.user-name {
  display: flex;
  align-items: center;
  gap: 12px;
}

.avatar {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  font-weight: 600;
  font-size: 0.9rem;
}

.name-info {
  display: flex;
  flex-direction: column;
}

.name-info strong {
  color: #2c3e50;
}

.name-info small {
  color: #6c757d;
  font-size: 0.75rem;
}

.user-email {
  color: #495057;
}

.email-text {
  font-family: 'Courier New', monospace;
  font-size: 0.9rem;
}

.user-cargo .cargo-badge {
  background: #e8f5e8;
  color: #2e7d32;
  padding: 6px 12px;
  border-radius: 16px;
  font-size: 0.8rem;
  font-weight: 500;
}

.user-remuneracion .remuneracion-value {
  font-weight: 600;
  color: #28a745;
  font-family: 'Courier New', monospace;
}

/* Select de roles */
.role-select {
  padding: 8px 12px;
  border: 1px solid #e2e8f0;
  border-radius: 6px;
  font-size: 0.9rem;
  transition: all 0.3s ease;
  background: white;
  color: #2d3748;
  width: 100%;
  cursor: pointer;
}

.role-select:focus {
  outline: none;
  border-color: #667eea;
}

.role-select.role-admin {
  background: #e3f2fd;
  border-color: #2196f3;
  color: #1976d2;
}

.role-select.role-empleado {
  background: #e8f5e8;
  border-color: #4caf50;
  color: #2e7d32;
}

.user-role {
  position: relative;
}

.update-spinner {
  position: absolute;
  right: 8px;
  top: 50%;
  transform: translateY(-50%);
  width: 16px;
  height: 16px;
  border: 2px solid transparent;
  border-top: 2px solid #667eea;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

/* Modal */
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
  padding: 20px;
  backdrop-filter: blur(4px);
}

.modal {
  background: white;
  border-radius: 12px;
  width: 100%;
  max-width: 500px;
  max-height: 90vh;
  overflow-y: auto;
  box-shadow: 0 20px 60px rgba(0,0,0,0.3);
}

.modal-header {
  padding: 24px;
  border-bottom: 1px solid #e9ecef;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.modal-header h3 {
  margin: 0;
  color: #2c3e50;
  font-weight: 600;
}

.modal-close {
  background: none;
  border: none;
  font-size: 1.5rem;
  cursor: pointer;
  color: #6c757d;
  padding: 0;
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 50%;
  transition: all 0.2s ease;
}

.modal-close:hover {
  background: #f8f9fa;
  color: #495057;
}

.modal-body {
  padding: 24px;
}

.modal-footer {
  padding: 24px;
  border-top: 1px solid #e9ecef;
  display: flex;
  gap: 12px;
  justify-content: flex-end;
}

/* Formulario */
.user-form {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.form-row {
  display: flex;
  gap: 16px;
}

.form-row .form-group {
  flex: 1;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.form-group label {
  font-weight: 600;
  color: #495057;
  font-size: 0.9rem;
}

.form-input, .form-select {
  padding: 12px;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  font-size: 0.9rem;
  transition: all 0.3s ease;
  color: #2d3748;
  background: white;
}

.form-input:focus, .form-select:focus {
  outline: none;
  border-color: #667eea;
  box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
}

.form-input::placeholder {
  color: #a0aec0;
}

/* Responsive */
@media (max-width: 768px) {
  .dashboard-container {
    padding: 16px;
  }
  
  .header-content {
    flex-direction: column;
    align-items: flex-start;
    gap: 16px;
  }
  
  .title {
    font-size: 1.5rem;
  }
  
  .control-panel {
    padding: 20px;
  }
  
  .panel-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 16px;
  }
  
  .filters-section {
    flex-direction: column;
    align-items: stretch;
  }
  
  .filter-group {
    width: 100%;
  }
  
  .filter-select, .filter-input {
    width: 100%;
  }
  
  .search-input {
    width: 100%;
  }
  
  .stats-section {
    flex-direction: column;
  }
  
  .table-header {
    flex-direction: column;
    align-items: stretch;
    gap: 12px;
  }
  
  .users-table {
    font-size: 0.8rem;
  }
  
  .form-row {
    flex-direction: column;
    gap: 16px;
  }
  
  .modal {
    margin: 16px;
    width: calc(100% - 32px);
  }
  
  .modal-footer {
    flex-direction: column;
  }
}

@media (max-width: 480px) {
  .control-buttons {
    flex-direction: column;
    width: 100%;
  }
  
  .btn {
    width: 100%;
    justify-content: center;
  }
  
  .user-actions {
    display: flex;
    flex-direction: column;
    gap: 8px;
  }
}
</style>