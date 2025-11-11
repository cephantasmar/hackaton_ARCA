<template>
  <nav class="navbar">
    <div class="nav-container">
      <!-- Logo -->
      <div class="nav-logo">
        <router-link to="/home" class="logo-link">
          <span class="logo-text">MiApp</span>
        </router-link>
      </div>

      <!-- Menú para desktop -->
      <div class="nav-menu" :class="{ 'nav-menu-mobile': isMobileMenuOpen }">
        <div class="nav-links">
          <router-link to="/home" class="nav-link">Inicio</router-link>
          <router-link to="/courses" class="nav-link">Cursos</router-link>
          <router-link to="/my-courses" class="nav-link">Mis Cursos</router-link>
          <router-link to="/foro" class="nav-link">Foro</router-link>
          
          <!-- Solo visible para Admin -->
          <router-link 
            v-if="userRole === 'Admin'" 
            to="/base" 
            class="nav-link admin-link"
          >
            Administración
          </router-link>
        </div>

        <!-- Información del usuario -->
        <div class="user-section">
          <div class="user-info">
            <span class="user-name">{{ userName }}</span>
            <span class="user-role" :class="userRole.toLowerCase()">{{ userRole }}</span>
          </div>
          <button @click="handleSignOut" class="logout-btn">
            Cerrar Sesión
          </button>
        </div>
      </div>

      <!-- Botón menú móvil -->
      <button 
        class="mobile-menu-btn" 
        @click="toggleMobileMenu"
        :aria-expanded="isMobileMenuOpen"
      >
        <span></span>
        <span></span>
        <span></span>
      </button>
    </div>
  </nav>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'
import { supabase } from '../supabase'
import { getUserProfile } from '../router.js'

const router = useRouter()
const isMobileMenuOpen = ref(false)
const userProfile = ref(null)
const userRole = ref('Empleado')
const userName = ref('Usuario')

// Cargar perfil del usuario
async function loadUserProfile() {
  try {
    const profile = await getUserProfile()
    if (profile) {
      userProfile.value = profile
      userRole.value = profile.rol || 'Empleado'
      userName.value = `${profile.nombre || ''} ${profile.apellido || ''}`.trim() || 'Usuario'
    }
  } catch (error) {
    console.error('Error loading user profile:', error)
  }
}

// Cerrar sesión
async function handleSignOut() {
  try {
    const { error } = await supabase.auth.signOut()
    if (error) throw error
    
    // Limpiar localStorage
    localStorage.removeItem('token')
    
    // Redirigir al login
    router.push('/signin')
  } catch (error) {
    console.error('Error signing out:', error)
  }
}

// Toggle menú móvil
function toggleMobileMenu() {
  isMobileMenuOpen.value = !isMobileMenuOpen.value
}

// Cerrar menú móvil al hacer clic fuera
function handleClickOutside(event) {
  const nav = document.querySelector('.nav-container')
  if (nav && !nav.contains(event.target)) {
    isMobileMenuOpen.value = false
  }
}

// Escuchar cambios de autenticación
supabase.auth.onAuthStateChange(async (event, session) => {
  if (event === 'SIGNED_OUT') {
    userProfile.value = null
    userRole.value = 'Empleado'
    userName.value = 'Usuario'
  } else if (event === 'SIGNED_IN' && session) {
    await loadUserProfile()
  }
})

onMounted(async () => {
  await loadUserProfile()
  document.addEventListener('click', handleClickOutside)
})

onUnmounted(() => {
  document.removeEventListener('click', handleClickOutside)
})
</script>

<style scoped>
.navbar {
  background: #ffffff;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
  position: sticky;
  top: 0;
  z-index: 1000;
  border-bottom: 1px solid #e5e7eb;
}

.nav-container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 1.5rem;
  display: flex;
  align-items: center;
  justify-content: space-between;
  height: 70px;
}

.nav-logo .logo-link {
  text-decoration: none;
  font-weight: 700;
  font-size: 1.5rem;
  color: #4f46e5;
}

.logo-text {
  background: linear-gradient(135deg, #4f46e5, #7c3aed);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

.nav-menu {
  display: flex;
  align-items: center;
  gap: 2rem;
  flex: 1;
  justify-content: space-between;
  margin-left: 2rem;
}

.nav-links {
  display: flex;
  align-items: center;
  gap: 1.5rem;
}

.nav-link {
  text-decoration: none;
  color: #6b7280;
  font-weight: 500;
  padding: 0.5rem 0.75rem;
  border-radius: 0.5rem;
  transition: all 0.2s ease;
  position: relative;
}

.nav-link:hover {
  color: #4f46e5;
  background: #f8fafc;
}

.nav-link.router-link-active {
  color: #4f46e5;
  background: #eef2ff;
}

.admin-link {
  color: #dc2626;
  font-weight: 600;
}

.admin-link:hover {
  color: #b91c1c;
  background: #fef2f2;
}

.admin-link.router-link-active {
  color: #dc2626;
  background: #fef2f2;
}

.user-section {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.user-info {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: 0.25rem;
}

.user-name {
  font-weight: 600;
  color: #1f2937;
  font-size: 0.9rem;
}

.user-role {
  font-size: 0.75rem;
  padding: 0.25rem 0.5rem;
  border-radius: 1rem;
  font-weight: 600;
  text-transform: uppercase;
}

.user-role.admin {
  background: #fef2f2;
  color: #dc2626;
  border: 1px solid #fecaca;
}

.user-role.empleado {
  background: #f0f9ff;
  color: #0369a1;
  border: 1px solid #bae6fd;
}

.logout-btn {
  background: #ef4444;
  color: white;
  border: none;
  padding: 0.5rem 1rem;
  border-radius: 0.5rem;
  font-weight: 500;
  cursor: pointer;
  transition: background 0.2s ease;
  font-size: 0.875rem;
}

.logout-btn:hover {
  background: #dc2626;
}

/* Menú móvil */
.mobile-menu-btn {
  display: none;
  flex-direction: column;
  background: none;
  border: none;
  cursor: pointer;
  padding: 0.5rem;
  gap: 0.25rem;
}

.mobile-menu-btn span {
  width: 20px;
  height: 2px;
  background: #374151;
  transition: all 0.3s ease;
}

/* Responsive */
@media (max-width: 768px) {
  .mobile-menu-btn {
    display: flex;
  }

  .nav-menu {
    position: absolute;
    top: 100%;
    left: 0;
    right: 0;
    background: white;
    flex-direction: column;
    padding: 1.5rem;
    box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);
    border-top: 1px solid #e5e7eb;
    transform: translateY(-10px);
    opacity: 0;
    visibility: hidden;
    transition: all 0.3s ease;
  }

  .nav-menu-mobile {
    transform: translateY(0);
    opacity: 1;
    visibility: visible;
  }

  .nav-links {
    flex-direction: column;
    width: 100%;
    gap: 0.5rem;
  }

  .nav-link {
    width: 100%;
    text-align: center;
    padding: 0.75rem 1rem;
  }

  .user-section {
    width: 100%;
    flex-direction: column;
    gap: 1rem;
    padding-top: 1rem;
    border-top: 1px solid #e5e7eb;
  }

  .user-info {
    align-items: center;
  }

  .logout-btn {
    width: 100%;
  }
}

@media (max-width: 480px) {
  .nav-container {
    padding: 0 1rem;
  }
  
  .nav-logo .logo-link {
    font-size: 1.25rem;
  }
}
</style>