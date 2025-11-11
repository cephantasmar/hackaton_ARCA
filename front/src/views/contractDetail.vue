<!-- filepath: c:\Users\CESAR\Documents\UNIVERSIDAD\8 semestre\ARQ_SOFT\h1\front\src\views\ContractDetail.vue -->
<template>
  <div class="contract-detail">
    <div class="header">
      <button @click="$router.back()" class="btn-back">
        <i class="fas fa-arrow-left"></i> Volver
      </button>
      <h1>Detalle del Contrato</h1>
    </div>

    <div v-if="loading" class="loading">
      <p>Cargando...</p>
    </div>

    <div v-else-if="contrato" class="contract-card">
      <div class="contract-header">
        <div>
          <h2>{{ contrato.nombre }} {{ contrato.apellido }}</h2>
          <p class="email">{{ contrato.email }}</p>
        </div>
        <span :class="['status-badge', contrato.activo ? 'active' : 'inactive']">
          {{ contrato.activo ? 'Activo' : 'Inactivo' }}
        </span>
      </div>

      <div class="contract-info">
        <div class="info-section">
          <h3>Información Laboral</h3>
          <div class="info-grid">
            <div class="info-item">
              <label>Cargo</label>
              <p>{{ contrato.cargo || 'No especificado' }}</p>
            </div>
            <div class="info-item">
              <label>Fecha de Inicio</label>
              <p>{{ formatDate(contrato.fecha_inicio) }}</p>
            </div>
            <div class="info-item">
              <label>Salario</label>
              <p class="salary">${{ formatNumber(contrato.salario) }}</p>
            </div>
            <div class="info-item">
              <label>Tiempo de Prueba</label>
              <p>{{ contrato.tiempo_prueba }} días</p>
            </div>
          </div>
        </div>

        <div class="info-section">
          <h3>Fechas del Sistema</h3>
          <div class="info-grid">
            <div class="info-item">
              <label>Creado</label>
              <p>{{ formatDateTime(contrato.created_at) }}</p>
            </div>
            <div class="info-item">
              <label>Última Actualización</label>
              <p>{{ formatDateTime(contrato.updated_at) }}</p>
            </div>
          </div>
        </div>
      </div>

      <div class="contract-document">
        <h3>Vista Previa del Contrato</h3>
        <div class="document-preview">
          <div class="document-header">
            <h4>CONTRATO DE TRABAJO</h4>
            <p class="document-date">{{ formatDate(contrato.fecha_inicio) }}</p>
          </div>
          
          <div class="document-body">
            <p><strong>ENTRE:</strong></p>
            <p>La Empresa [NOMBRE DE LA EMPRESA], en adelante denominada "EL EMPLEADOR"</p>
            <br>
            <p><strong>Y:</strong></p>
            <p><strong>{{ contrato.nombre }} {{ contrato.apellido }}</strong>, en adelante denominado "EL TRABAJADOR"</p>
            <br>
            <p>Se ha convenido el presente contrato de trabajo bajo las siguientes cláusulas:</p>
            <br>
            <p><strong>PRIMERA:</strong> El trabajador se compromete a prestar sus servicios en el cargo de <strong>{{ contrato.cargo || 'N/A' }}</strong>.</p>
            <br>
            <p><strong>SEGUNDA:</strong> La fecha de inicio de labores será el <strong>{{ formatDate(contrato.fecha_inicio) }}</strong>.</p>
            <br>
            <p><strong>TERCERA:</strong> El salario mensual acordado es de <strong>${{ formatNumber(contrato.salario) }}</strong> ({{ numeroALetras(contrato.salario) }} pesos).</p>
            <br>
            <p><strong>CUARTA:</strong> Se establece un período de prueba de <strong>{{ contrato.tiempo_prueba }} días</strong> contados a partir de la fecha de inicio.</p>
            <br>
            <p><strong>QUINTA:</strong> Durante el período de prueba, cualquiera de las partes podrá dar por terminado el contrato sin previo aviso.</p>
            <br>
            <p>Firmado en la ciudad de __________, a los {{ new Date().getDate() }} días del mes de {{ getMesActual() }} de {{ new Date().getFullYear() }}.</p>
          </div>
          
          <div class="document-signatures">
            <div class="signature-block">
              <div class="signature-line"></div>
              <p><strong>EL EMPLEADOR</strong></p>
            </div>
            <div class="signature-block">
              <div class="signature-line"></div>
              <p><strong>EL TRABAJADOR</strong></p>
              <p>{{ contrato.nombre }} {{ contrato.apellido }}</p>
            </div>
          </div>
        </div>
        
        <button @click="downloadContract" class="btn-download">
          <i class="fas fa-download"></i> Descargar Contrato (PDF)
        </button>
      </div>
    </div>

    <div v-else class="error">
      <p>No se pudo cargar el contrato</p>
    </div>
  </div>
</template>

<script>
export default {
  name: 'ContractDetail',
  data() {
    return {
      contrato: null,
      loading: true
    }
  },
  async mounted() {
    await this.loadContrato()
  },
  methods: {
    async loadContrato() {
      try {
        const token = localStorage.getItem('token')
        const backendUrl = window.location.hostname === 'localhost' 
          ? 'http://localhost:5003' 
          : '/api/contracts'
        
        const response = await fetch(`${backendUrl}/api/contracts/${this.$route.params.id}`, {
          headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
          }
        })
        
        if (response.ok) {
          this.contrato = await response.json()
        } else {
          console.error('Error loading contract')
        }
      } catch (error) {
        console.error('Error loading contract:', error)
      } finally {
        this.loading = false
      }
    },
    formatDate(date) {
      if (!date) return 'N/A'
      return new Date(date).toLocaleDateString('es-ES', { 
        year: 'numeric', 
        month: 'long', 
        day: 'numeric' 
      })
    },
    formatDateTime(date) {
      if (!date) return 'N/A'
      return new Date(date).toLocaleString('es-ES')
    },
    formatNumber(num) {
      if (!num) return '0.00'
      return Number(num).toLocaleString('es-ES', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
    },
    getMesActual() {
      const meses = ['enero', 'febrero', 'marzo', 'abril', 'mayo', 'junio', 
                     'julio', 'agosto', 'septiembre', 'octubre', 'noviembre', 'diciembre']
      return meses[new Date().getMonth()]
    },
    numeroALetras(numero) {
      // Implementación simple - podrías usar una librería como numero-a-letras
      return `${numero}`
    },
    downloadContract() {
      alert('Funcionalidad de descarga PDF en desarrollo.\nPuedes usar Ctrl+P para imprimir este documento.')
      window.print()
    }
  }
}
</script>

<style scoped>
.contract-detail {
  padding: 2rem;
  max-width: 1000px;
  margin: 0 auto;
}

.header {
  display: flex;
  align-items: center;
  gap: 1rem;
  margin-bottom: 2rem;
}

.header h1 {
  margin: 0;
  color: #333;
}

.btn-back {
  background: #f5f5f5;
  border: none;
  padding: 0.75rem 1rem;
  border-radius: 4px;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  transition: background 0.3s;
}

.btn-back:hover {
  background: #e0e0e0;
}

.loading, .error {
  text-align: center;
  padding: 3rem;
  color: #666;
}

.contract-card {
  background: white;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.1);
  padding: 2rem;
}

.contract-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  padding-bottom: 1.5rem;
  border-bottom: 2px solid #eee;
  margin-bottom: 2rem;
}

.contract-header h2 {
  margin: 0;
  color: #333;
}

.email {
  color: #666;
  margin-top: 0.5rem;
}

.status-badge {
  padding: 0.5rem 1rem;
  border-radius: 20px;
  font-weight: 600;
  font-size: 0.875rem;
}

.status-badge.active {
  background: #e8f5e9;
  color: #2e7d32;
}

.status-badge.inactive {
  background: #ffebee;
  color: #c62828;
}

.contract-info {
  display: flex;
  flex-direction: column;
  gap: 2rem;
}

.info-section h3 {
  margin-bottom: 1rem;
  color: #333;
  font-size: 1.25rem;
}

.info-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1.5rem;
}

.info-item label {
  display: block;
  font-size: 0.875rem;
  color: #666;
  margin-bottom: 0.5rem;
}

.info-item p {
  font-size: 1rem;
  font-weight: 500;
  color: #333;
  margin: 0;
}

.salary {
  color: #4CAF50;
  font-size: 1.5rem;
  font-weight: 600;
}

.contract-document {
  margin-top: 2rem;
  padding-top: 2rem;
  border-top: 2px solid #eee;
}

.contract-document > h3 {
  margin-bottom: 1rem;
  color: #333;
}

.document-preview {
  background: #f9f9f9;
  padding: 2.5rem;
  border-radius: 8px;
  margin: 1rem 0;
  font-family: 'Times New Roman', serif;
  line-height: 1.8;
  box-shadow: inset 0 0 10px rgba(0,0,0,0.05);
}

.document-header {
  text-align: center;
  margin-bottom: 2rem;
}

.document-header h4 {
  font-size: 1.5rem;
  margin: 0 0 0.5rem 0;
  text-transform: uppercase;
  letter-spacing: 1px;
}

.document-date {
  color: #666;
  font-size: 0.9rem;
}

.document-body p {
  margin: 0.5rem 0;
  text-align: justify;
}

.document-signatures {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 3rem;
  margin-top: 4rem;
}

.signature-block {
  text-align: center;
}

.signature-line {
  border-top: 2px solid #333;
  margin-bottom: 0.5rem;
}

.signature-block p {
  margin: 0.25rem 0;
  font-size: 0.9rem;
}

.btn-download {
  background: #2196F3;
  color: white;
  padding: 0.75rem 1.5rem;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  margin-top: 1rem;
  transition: background 0.3s;
}

.btn-download:hover {
  background: #1976D2;
}

@media print {
  .header,
  .btn-download {
    display: none;
  }
  
  .contract-card {
    box-shadow: none;
  }
  
  .document-preview {
    background: white;
    box-shadow: none;
  }
}
</style>