<template>
  <div class="home-layout">
    <!-- Barra lateral -->
    <aside class="sidebar" :class="{ 'sidebar-collapsed': isSidebarCollapsed }">
      <div class="sidebar-header">
        <h3>EMPRESA DE SERVICIOS AUXILIARES FINANCIEROS ARCA LTDA.</h3>
        <p>Acceso rápido</p>
        <button class="toggle-sidebar" @click="toggleSidebar">
          {{ isSidebarCollapsed ? '→' : '←' }}
        </button>
      </div>
      <nav class="sidebar-nav">
        <router-link to="/personal" class="nav-item">
          <div class="nav-icon">1</div>
          <div class="nav-content" v-if="!isSidebarCollapsed">
            <strong>Personal</strong>
            <span>Gestión de empleados</span>
          </div>
        </router-link>

        <router-link to="/vacaciones" class="nav-item">
          <div class="nav-icon">2</div>
          <div class="nav-content" v-if="!isSidebarCollapsed">
            <strong>Vacaciones</strong>
            <span>Solicitudes y aprobaciones</span>
          </div>
        </router-link>

        <router-link to="/contratos" class="nav-item">
          <div class="nav-icon">3</div>
          <div class="nav-content" v-if="!isSidebarCollapsed">
            <strong>Contratos</strong>
            <span>Administración contractual</span>
          </div>
        </router-link>

        <router-link to="/boletas" class="nav-item">
          <div class="nav-icon">4</div>
          <div class="nav-content" v-if="!isSidebarCollapsed">
            <strong>Boletas de pago</strong>
            <span>Consulta y descarga</span>
          </div>
        </router-link>

        <!-- Opcionales -->
        <router-link to="/info" class="nav-item">
          <div class="nav-icon">i</div>
          <div class="nav-content" v-if="!isSidebarCollapsed">
            <strong>Información</strong>
            <span>Acerca del proyecto</span>
          </div>
        </router-link>
      </nav>
    </aside>

    <!-- Contenido principal -->
    <main class="main-content" :class="{ 'expanded': isSidebarCollapsed }">
      <!-- Hero de inicio (inspiracional y claro) -->
      <section class="home-hero">
        <div class="hero-content">
          <h1>ARCA LTDA. — MVP Gestión Humana</h1>
          <p class="lead">
            EMPRESA DE SERVICIOS AUXILIARES FINANCIEROS ARCA LTDA.
          </p>
          <p class="summary">
            Luego de un esfuerzo organizacional, ARCA adquiere un sistema Core de clase mundial para las áreas operativas. Para optimizar costos no se adquirió el módulo de gestión humana; por ello, Tecnología propone desarrollar un MVP rápido que demuestre la factibilidad de una solución propia.
          </p>

          <div class="hero-actions">
            <router-link to="/personal" class="btn-primary">Empezar — Personal</router-link>
            <router-link to="/info" class="btn-outline">Leer más del proyecto</router-link>
          </div>
        </div>
      </section>

      <!-- Descripción del proyecto -->
      <div class="info-negocio">
        <h2>Resumen del proyecto</h2>
        <p>
          Los encargados de la evaluación determinaron que por aproximadamente un tercio del costo se puede adquirir otra herramienta o desarrollar una solución interna con mayores prestaciones. El área de Tecnología confía en su equipo para crear un MVP funcional en tiempo récord.
        </p>

        <p>
          La propuesta es presentar un MVP al Directorio hoy mismo en 2 horas, con funcionalidades básicas que demuestren capacidad técnica y cumplan las expectativas organizacionales. El sistema Core usa arquitectura de microservicios, lo que facilita acoplar un módulo sencillo de gestión humana para pruebas e integración.
        </p>

        <h3>Objetivos del MVP</h3>
        <ul>
          <li>Demostrar integración con el Core mediante un módulo desacoplado.</li>
          <li>Probar funcionalidades básicas: gestión de personal, vacaciones, contratos y boletas de pago.</li>
          <li>Entregar una interfaz clara y usable para usuarios no técnicos.</li>
          <li>Generar métricas y evidencia técnica para decisiones del Directorio.</li>
        </ul>

        <h3>Beneficios esperados</h3>
        <ul>
          <li>Reducir costos comparado con módulos comerciales adicionales.</li>
          <li>Tener control total sobre la evolución del producto.</li>
          <li>Integración sencilla gracias a microservicios.</li>
          <li>Capacidad de iterar rápidamente tras la validación del MVP.</li>
        </ul>
      </div>

      <!-- Sección visual y llamada a la acción amigable -->
      <section class="project-visual">
        <div class="visual-grid">
          <div class="card">
            <h4>Arquitectura</h4>
            <p>Microservicios que permiten acoplar módulos independientes sin tocar el Core.</p>
          </div>
          <div class="card">
            <h4>Tiempo</h4>
            <p>Propuesta: prototipo funcional (MVP) para presentación en 2 horas.</p>
          </div>
          <div class="card">
            <h4>Módulos clave</h4>
            <p>Personal, Vacaciones, Contratos, Boletas de pago — inicio mínimo viable.</p>
          </div>
        </div>

        <div class="cta" style="margin-top:1.25rem;">
          <h3>¿Listos para validar el MVP?</h3>
          <div class="cta-actions">
            <router-link to="/personal" class="btn-primary">Abrir Personal</router-link>
            <router-link to="/contratos" class="btn-outline">Ver Contratos</router-link>
          </div>
        </div>
      </section>
    </main>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue'

// Estado reactivo para la barra lateral
const isSidebarCollapsed = ref(false)

// Función para alternar la barra lateral
const toggleSidebar = () => {
  isSidebarCollapsed.value = !isSidebarCollapsed.value
}

// Detectar cambios en el tamaño de la ventana
const handleResize = () => {
  if (window.innerWidth < 900) {
    isSidebarCollapsed.value = true
  } else {
    isSidebarCollapsed.value = false
  }
}

// Configurar listeners de eventos
onMounted(() => {
  window.addEventListener('resize', handleResize)
  handleResize() // Verificar el tamaño inicial
})

onUnmounted(() => {
  window.removeEventListener('resize', handleResize)
})
</script>

<style scoped>
.home-layout {
  display: grid;
  grid-template-columns: 280px 1fr;
  min-height: 100vh;
  gap: 0;
  transition: grid-template-columns 0.3s ease;
}

.home-layout.sidebar-collapsed {
  grid-template-columns: 70px 1fr;
}

/* Barra lateral */
.sidebar {
  background: linear-gradient(135deg, #2a4dd0 0%, #1e40af 100%);
  color: white;
  padding: 1.5rem 1rem;
  height: 100vh;
  position: sticky;
  top: 0;
  overflow-y: auto;
  transition: all 0.3s ease;
  box-shadow: 2px 0 10px rgba(0, 0, 0, 0.1);
}

.sidebar-collapsed {
  width: 70px;
  padding: 1.5rem 0.5rem;
}

.sidebar-header {
  text-align: center;
  margin-bottom: 2rem;
  padding-bottom: 1.5rem;
  border-bottom: 1px solid rgba(255,255,255,0.2);
  position: relative;
}

.sidebar-collapsed .sidebar-header {
  padding-bottom: 1rem;
}

.sidebar-header h3 {
  font-size: 1.3rem;
  font-weight: 700;
  margin-bottom: 0.5rem;
  transition: opacity 0.3s ease;
}

.sidebar-collapsed .sidebar-header h3 {
  font-size: 1rem;
}

.sidebar-header p {
  opacity: 0.8;
  font-size: 0.8rem;
  transition: opacity 0.3s ease;
}

.sidebar-collapsed .sidebar-header p {
  opacity: 0;
  height: 0;
  margin: 0;
}

.toggle-sidebar {
  position: absolute;
  top: 0;
  right: 0;
  background: rgba(255, 255, 255, 0.2);
  border: none;
  color: white;
  width: 24px;
  height: 24px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  font-size: 0.8rem;
  transition: background 0.3s ease;
}

.toggle-sidebar:hover {
  background: rgba(255, 255, 255, 0.3);
}

.sidebar-nav {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.nav-item {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 0.8rem;
  border-radius: 10px;
  text-decoration: none;
  color: white;
  transition: all 0.3s ease;
  border: 1px solid transparent;
  position: relative;
}

.sidebar-collapsed .nav-item {
  padding: 0.8rem;
  justify-content: center;
}

.nav-item:hover {
  background: rgba(255,255,255,0.1);
  border-color: rgba(255,255,255,0.3);
  transform: translateX(3px);
}

.nav-item.router-link-active {
  background: rgba(255,255,255,0.15);
  border-color: rgba(255,255,255,0.4);
}

.nav-icon {
  width: 24px;
  height: 24px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: rgba(255,255,255,0.1);
  border-radius: 8px;
  flex-shrink: 0;
}

.nav-icon svg {
  width: 16px;
  height: 16px;
}

.nav-content {
  flex: 1;
  transition: opacity 0.3s ease;
}

.sidebar-collapsed .nav-content {
  opacity: 0;
  width: 0;
  height: 0;
  overflow: hidden;
}

.nav-content strong {
  display: block;
  font-weight: 600;
  margin-bottom: 0.2rem;
  font-size: 0.9rem;
}

.nav-content span {
  font-size: 0.75rem;
  opacity: 0.8;
}

/* Contenido principal */
.main-content {
  padding: 2rem;
  background: #f8fafc;
  overflow-y: auto;
  transition: padding 0.3s ease;
}

.main-content.expanded {
  padding-left: 1rem;
}

/* Hero */
.home-hero {
  position: relative;
  border-radius: 12px;
  padding: 2.5rem 1rem;
  margin: 0 auto 1.5rem;
  max-width: 1100px;
  overflow: hidden;
  background:
    linear-gradient(135deg, rgba(34,197,94,0.95), rgba(16,185,129,0.9));
  color: #fff;
  box-shadow: 0 12px 30px rgba(0,0,0,.08);
  text-align: left;
}
.hero-content { 
  max-width: 900px; 
  margin: 0 auto; 
}
.hero-content h1 {
  font-size: 1.6rem;
  margin-bottom: .5rem;
}
.hero-content .lead {
  font-weight: 700;
  margin-bottom: .5rem;
}
.hero-content .summary {
  opacity: 0.95;
  margin-bottom: 1rem;
  line-height: 1.5;
}
.hero-actions { 
  margin-top: 1rem; 
  display:flex; 
  gap:.75rem; 
  flex-wrap: wrap; 
}
.btn-primary {
  background: #061e40;
  color: white;
  padding: .6rem 1rem;
  border-radius: 8px;
  text-decoration: none;
  font-weight: 700;
}
.btn-outline {
  background: transparent;
  color: white;
  padding: .55rem .95rem;
  border-radius: 8px;
  text-decoration: none;
  border: 2px solid rgba(255,255,255,0.15);
  font-weight: 600;
}

.info-negocio {
  margin: 1.5rem auto;
  max-width: 1100px;
  background: #fff;
  border-radius: 12px;
  box-shadow: 0 2px 10px rgba(0,0,0,0.06);
  padding: 1.25rem;
  text-align: left;
}
.info-negocio h2 { 
  color: #0b1220; 
  margin-bottom: 0.5rem; 
}
.info-negocio h3 {
  color: #0b1220;
  margin-top: 1rem;
}
.info-negocio ul {
  margin-top: 0.75rem;
  padding-left: 1.2rem;
}

.project-visual {
  max-width: 1100px;
  margin: 1.5rem auto;
}
.visual-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 12px;
}
.card {
  background: #fff;
  border-radius: 10px;
  padding: 1rem;
  box-shadow: 0 2px 8px rgba(0,0,0,.06);
}
.card h4 {
  margin: 0 0 .5rem 0;
  color: #0b1220;
}
@media (max-width: 900px) {
  .visual-grid { grid-template-columns: 1fr; }
  .hero-content h1 { font-size: 1.25rem; }
}
/* ...existing code... */
</style>