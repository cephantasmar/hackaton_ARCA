<template>
  <div v-if="showNavbar" :class="['navbar-wrapper', { 'dark-mode': isDark }]">
    <!-- HEADER -->
    <header class="site-header">
      <div class="container">
        <div class="header-top">
          <h1 class="site-title">EMPRESA DE SERVICIOS AUXILIARES FINANCIEROS ARCA LTDA.</h1>

          <div class="user-info" v-if="userProfile">
            <span class="user-name">Hola, {{ userProfile.nombre }}</span>
            <span class="user-role" :class="userProfile.rol">{{ userProfile.rol }}</span>
          </div>

          <!-- Theme toggle button -->
          <button class="theme-toggle" @click="toggleTheme" :title="isDark ? 'Cambiar a claro' : 'Cambiar a oscuro'">
            <span v-if="isDark">☀️</span>
            <span v-else>🌙</span>
          </button>
        </div>

        <div class="header-desc">
          ARCA LTDA. Luego de un esfuerzo organizacional logra la adquisición de un sistema de gama mundial que gestionara todas las actividades de las áreas operativas de la empresa (Sistema Core); sin embargo, para abaratar los costos y debido a una evaluación técnica no se está adquiriendo el módulo de gestión humana que con la inclusión implicaría un gran costo adicional.
          Los encargados de la evaluación lograron determinar que por un tercio del costo se puede adquirir otra herramienta mejor y con mayores prestaciones como ser SALAR, SPYRAL o desarrollar uno propio en casa. 
          El área de Tecnología de ARCA está segura de que tiene el personal técnico capaz como para desarrollar el sistema que gestión humana necesita, por lo que proponen la creación de un MVP para ser presentado al Directorio (hoy) de la empresa en un tiempo récord de 2 horas con funcionalidades básicas necesarias para sustentar que puede lograrse entregar una herramienta acorde a las expectativas de la organización.
          El sistema Core adquirido está basado en una arquitectura de software completamente desarrollada en microservicios, por lo que Tecnología asegura que puede acoplar un módulo simple para probar que puede desarrollarse la herramienta que necesita gestión humana.
        </div>
      </div>
    </header>

    <!-- NAVIGATION -->
    <nav class="site-nav">
      <div class="container">
        <!-- Desktop Navigation -->
        <div class="nav-desktop">
          <div class="nav-main">
            <router-link to="/home" class="nav-link">Inicio</router-link>
            <router-link to="/personal" class="nav-link">1 Personal</router-link>
            <router-link to="/vacaciones" class="nav-link">2 Vacaciones</router-link>
            <router-link to="/contratos" class="nav-link">3 Contratos</router-link>
            <router-link to="/boletas" class="nav-link">4 Boletas de pago</router-link>

            <router-link 
              v-if="userProfile?.rol === 'Director'" 
              to="/base" 
              class="nav-link admin-link"
            >
              Roles
            </router-link>
          </div>

          <div class="nav-actions">
            <button @click="handleLogout" class="logout-btn" :disabled="loading">
              {{ loading ? 'Cerrando...' : 'Cerrar sesión' }}
            </button>
          </div>
        </div>

        <!-- Mobile Navigation -->
        <div class="nav-mobile">
          <button @click="toggleMobileMenu" class="mobile-menu-btn">
            ☰
          </button>
          
          <div v-if="mobileMenuOpen" class="mobile-menu-overlay" @click="toggleMobileMenu"></div>
          
          <div :class="['mobile-menu', { 'mobile-menu-open': mobileMenuOpen }]">
            <div class="mobile-menu-header">
              <h3>Menú</h3>
              <button @click="toggleMobileMenu" class="close-menu-btn">
                ×
              </button>
            </div>
            
            <div class="mobile-nav-links">
              <router-link to="/home" class="nav-link" @click="toggleMobileMenu">Inicio</router-link>
              <router-link to="/personal" class="nav-link" @click="toggleMobileMenu">1 Personal</router-link>
              <router-link to="/vacaciones" class="nav-link" @click="toggleMobileMenu">2 Vacaciones</router-link>
              <router-link to="/contratos" class="nav-link" @click="toggleMobileMenu">3 Contratos</router-link>
              <router-link to="/boletas" class="nav-link" @click="toggleMobileMenu">4 Boletas de pago</router-link>

              <router-link 
                v-if="userProfile?.rol === 'Director'" 
                to="/base" 
                class="nav-link admin-link"
                @click="toggleMobileMenu"
              >
                Base de Datos
              </router-link>
              
              <button @click="handleLogout" class="logout-btn mobile-logout" :disabled="loading">
                {{ loading ? 'Cerrando...' : 'Cerrar sesión' }}
              </button>
            </div>
          </div>
        </div>
      </div>
    </nav>
  </div>
</template>
  
<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { supabase } from '../supabase'

const router = useRouter()
const route = useRoute()
const loading = ref(false)
const isAuthenticated = ref(false)
const userProfile = ref(null)
const mobileMenuOpen = ref(false)

// Theme state
const isDark = ref(false)

const showNavbar = computed(() => {
  const hideOnRoutes = ['/signin', '/', '/auth/callback']
  return isAuthenticated.value && !hideOnRoutes.includes(route.path)
})

async function getUserProfile() {
  try {
    const { data: { user } } = await supabase.auth.getUser()
    if (!user) return null

    const token = localStorage.getItem('token')
    if (!token) return null

    const tenant = getTenantFromEmail(user.email)
    if (!tenant) return null

    const backendUrl = window.location.hostname === 'localhost' ? 'http://localhost:5002' : '/api/auth'
    
    const response = await fetch(`${backendUrl}/api/auth/user-profile`, {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      }
    })

    if (response.ok) {
      const profileData = await response.json()
      userProfile.value = profileData
    } else {
      userProfile.value = {
        nombre: user.user_metadata?.full_name || user.user_metadata?.name || 'Usuario',
        email: user.email,
        rol: 'estudiante'
      }
    }
    
  } catch (error) {
    const { data: { user } } = await supabase.auth.getUser()
    if (user) {
      userProfile.value = {
        nombre: user.user_metadata?.full_name || user.user_metadata?.name || 'Usuario',
        email: user.email,
        rol: 'estudiante'
      }
    }
  }
}

function getTenantFromEmail(email) {
  if (email.endsWith('@ucb.edu.bo')) return 'ucb.edu.bo'
  if (email.endsWith('@upb.edu.bo')) return 'upb.edu.bo'
  if (email.endsWith('@gmail.com')) return 'gmail.com'
  return null
}

async function checkAuth() {
  try {
    const { data: { session } } = await supabase.auth.getSession()
    const token = localStorage.getItem('token')
    
    isAuthenticated.value = !!(session && token)
    
    if (isAuthenticated.value) {
      await getUserProfile()
    } else {
      userProfile.value = null
    }
  } catch (error) {
    isAuthenticated.value = false
    userProfile.value = null
  }
}

function toggleMobileMenu() {
  mobileMenuOpen.value = !mobileMenuOpen.value
  if (mobileMenuOpen.value) {
    document.body.style.overflow = 'hidden'
  } else {
    document.body.style.overflow = 'auto'
  }
}

function setupListeners() {
  const { data: { subscription } } = supabase.auth.onAuthStateChange(async () => {
    await checkAuth()
  })

  const removeRouteListener = router.afterEach(() => {
    checkAuth()
    mobileMenuOpen.value = false
    document.body.style.overflow = 'auto'
  })

  return () => {
    subscription?.unsubscribe()
    removeRouteListener()
  }
}

async function handleLogout() {
  try {
    loading.value = true
    
    const { error } = await supabase.auth.signOut()
    if (error) throw error
    
    localStorage.removeItem('token')
    isAuthenticated.value = false
    userProfile.value = null
    mobileMenuOpen.value = false
    document.body.style.overflow = 'auto'
    
    router.push('/signin')

  } catch (error) {
    localStorage.removeItem('token')
    isAuthenticated.value = false
    userProfile.value = null
    router.push('/signin')
  } finally {
    loading.value = false
  }
}

// Theme toggle (persists in localStorage)
function toggleTheme() {
  isDark.value = !isDark.value
  localStorage.setItem('theme', isDark.value ? 'dark' : 'light')
  // also set a class on body for potential global styles
  if (isDark.value) document.body.classList.add('dark-mode')
  else document.body.classList.remove('dark-mode')
}

onMounted(() => {
  checkAuth()
  const cleanup = setupListeners()
  
  // initialize theme
  isDark.value = localStorage.getItem('theme') === 'dark'
  if (isDark.value) document.body.classList.add('dark-mode')
  else document.body.classList.remove('dark-mode')

  onUnmounted(() => {
    cleanup()
    document.body.style.overflow = 'auto'
  })
})
</script>

<style scoped>
.navbar-wrapper {
  position: sticky;
  top: 0;
  z-index: 1000;
}

.container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 20px;
}

.site-header {
  width: 100%;
  background: #111318;
  color: #ffffff;
  padding: 12px 0;
  border-bottom: 1px solid #2d3748;
}

.header-top {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 12px;
}

.site-title {
  margin: 0;
  font-size: 18px;
  font-weight: 800;
  color: #ffffff;
}

.user-info {
  display: flex;
  align-items: center;
  gap: 12px;
  font-size: 14px;
}

.user-name {
  color: #e5e7eb;
  font-weight: 500;
}

.user-role {
  padding: 6px 12px;
  border-radius: 20px;
  font-size: 12px;
  font-weight: 600;
  text-transform: capitalize;
  border: 1px solid;
}

.user-role.Director {
  background: #10b981;
  color: white;
  border-color: #10b981;
}

.user-role.estudiante {
  background: #3b82f6;
  color: white;
  border-color: #3b82f6;
}
.user-role.profesor {
  background: #f68f3b;
  color: white;
  border-color: #f68f3b;
}

.header-desc {
  margin-top: 10px;
  color: #d1d5db;
  font-size: 0.95rem;
  line-height: 1.4;
  max-width: 100%;
  padding-bottom: 8px;
}

.site-nav {
  width: 100%;
  background: #f8fafc;
  border-bottom: 1px solid #e2e8f0;
}

.nav-desktop {
  display: flex;
  justify-content: space-between;
  align-items: center;
  height: 64px;
}

@media (max-width: 768px) {
  .nav-desktop {
    display: none;
  }
}

.nav-main {
  display: flex;
  align-items: center;
  gap: 2px;
}

.nav-link {
  display: flex;
  align-items: center;
  padding: 10px 16px;
  color: #475569;
  text-decoration: none;
  font-weight: 500;
  font-size: 14px;
  border-radius: 8px;
  transition: all 0.3s ease;
  white-space: nowrap;
}

.nav-link:hover {
  background: #ffffff;
  color: #1e293b;
}

.nav-link.router-link-active,
.nav-link.router-link-exact-active {
  background: #e0e7ff;
  color: #3730a3;
  font-weight: 600;
}

.nav-link.admin-link {
  background: #fef3c7;
  color: #92400e;
  font-weight: 600;
  border: 1px solid #f59e0b;
}

.nav-link.admin-link.router-link-active {
  background: #f59e0b;
  color: white;
}

.nav-actions {
  display: flex;
  align-items: center;
}

.logout-btn {
  display: flex;
  align-items: center;
  background: #dc2626;
  color: #ffffff;
  padding: 10px 20px;
  border-radius: 8px;
  border: none;
  font-weight: 600;
  font-size: 14px;
  cursor: pointer;
  transition: all 0.3s ease;
}

.logout-btn:hover:not(:disabled) {
  background: #b91c1c;
}

.logout-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.nav-mobile {
  display: none;
}

@media (max-width: 768px) {
  .nav-mobile {
    display: block;
    height: 64px;
    display: flex;
    align-items: center;
    justify-content: space-between;
  }
}

.mobile-menu-btn {
  background: none;
  border: none;
  padding: 12px;
  cursor: pointer;
  color: #475569;
  border-radius: 8px;
  transition: all 0.3s ease;
  background: #ffffff;
}

.mobile-menu-btn:hover {
  background: #f1f5f9;
}

.mobile-menu-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0,0,0,0.5);
  z-index: 999;
}

.mobile-menu {
  position: fixed;
  top: 0;
  right: -100%;
  width: 300px;
  height: 100vh;
  background: white;
  transition: right 0.3s ease;
  z-index: 1000;
  display: flex;
  flex-direction: column;
}

.mobile-menu-open {
  right: 0;
}

.mobile-menu-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 20px;
  border-bottom: 1px solid #e2e8f0;
  background: #f8fafc;
}

.mobile-menu-header h3 {
  margin: 0;
  color: #1e293b;
  font-size: 20px;
  font-weight: 700;
}

.close-menu-btn {
  background: none;
  border: none;
  padding: 8px;
  cursor: pointer;
  color: #64748b;
  border-radius: 6px;
  transition: all 0.3s ease;
}

.close-menu-btn:hover {
  background: #f1f5f9;
}

.mobile-nav-links {
  flex: 1;
  padding: 20px;
  display: flex;
  flex-direction: column;
  gap: 8px;
  overflow-y: auto;
}

.mobile-nav-links .nav-link {
  justify-content: flex-start;
  padding: 14px 16px;
  border-radius: 8px;
  margin-bottom: 4px;
}

.mobile-logout {
  margin-top: 20px;
  justify-content: center;
}

@media (max-width: 1024px) {
  .nav-main {
    gap: 1px;
  }
  
  .nav-link {
    padding: 8px 12px;
    font-size: 13px;
  }
}

@media (max-width: 880px) {
  .nav-desktop .nav-main {
    gap: 0;
  }
  
  .nav-link {
    padding: 8px 10px;
    font-size: 12px;
  }
  
  .logout-btn {
    padding: 10px;
  }
}

.mobile-nav-links::-webkit-scrollbar {
  width: 6px;
}

.mobile-nav-links::-webkit-scrollbar-track {
  background: #f1f5f9;
}

.mobile-nav-links::-webkit-scrollbar-thumb {
  background: #cbd5e1;
  border-radius: 3px;
}

.mobile-nav-links::-webkit-scrollbar-thumb:hover {
  background: #94a3b8;
}

/* Theme toggle button */
.theme-toggle {
  background: transparent;
  border: 1px solid rgba(255,255,255,0.12);
  color: #f8fafc;
  padding: 6px 10px;
  border-radius: 8px;
  cursor: pointer;
  font-size: 16px;
  display: inline-flex;
  align-items: center;
  gap: 6px;
}

/* Dark mode overrides for this component */
.dark-mode .site-header {
  background: #0b1220;
  color: #ffffff;
  border-bottom: 1px solid #1f2937;
}

.dark-mode .header-desc {
  color: #cbd5e1;
}

.dark-mode .site-nav {
  background: #0f1724;
  border-bottom: 1px solid #111827;
}

.dark-mode .nav-link {
  color: #cbd5e1;
}

.dark-mode .nav-link:hover {
  background: rgba(255,255,255,0.02);
  color: #ffffff;
}

.dark-mode .nav-link.router-link-active,
.dark-mode .nav-link.router-link-exact-active {
  background: #1f2937;
  color: #e2e8f0;
  font-weight: 600;
}

.dark-mode .nav-link.admin-link {
  background: #3f2b07;
  color: #fde68a;
  border-color: #f59e0b;
}

.dark-mode .logout-btn {
  background: #ef4444;
  color: #fff;
}

.dark-mode .mobile-menu {
  background: #0b1220;
}

.dark-mode .mobile-menu-header {
  background: #071024;
  border-bottom: 1px solid #111827;
}

.dark-mode .mobile-menu-header h3 {
  color: #e6eef8;
}
</style>