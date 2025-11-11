import { createRouter, createWebHistory } from 'vue-router'
import { supabase } from './supabase'  // <-- Ruta corregida

// Vistas
import SignIn from './views/SignIn.vue'
import Home from './views/Home.vue'
import AuthCallback from './views/AuthCallback.vue'
import Foro from './views/Foro.vue'
import Features from './views/Features.vue'
import Pricing from './views/Pricing.vue'
import Info from './views/info.vue'
import Contact from './views/Contact.vue'
import Nosotros from './views/Nosotros.vue'
import Base from './views/BD.vue'
import Courses from './views/Courses.vue'
import MyCourses from './views/MyCourses.vue'
import CourseDetail from './views/CourseDetail.vue'

const routes = [
  { 
    path: '/', 
    redirect: '/signin'
  },
  { 
    path: '/signin', 
    component: SignIn,
    meta: { requiresGuest: true }
  },
  { 
    path: '/auth/callback', 
    component: AuthCallback
  },
  { 
    path: '/home', 
    component: Home, 
    meta: { requiresAuth: true } 
  },
  { 
    path: '/foro', 
    component: Foro, 
    meta: { requiresAuth: true } 
  },
  { 
    path: '/features', 
    component: Features, 
    meta: { requiresAuth: true } 
  },
  { 
    path: '/pricing', 
    component: Pricing, 
    meta: { requiresAuth: true } 
  },
  { 
    path: '/info', 
    component: Info, 
    meta: { requiresAuth: true } 
  },
  { 
    path: '/contact', 
    component: Contact, 
    meta: { requiresAuth: true } 
  },
  { 
    path: '/nosotros', 
    component: Nosotros, 
    meta: { requiresAuth: true } 
  },
  { 
    path: '/base', 
    component: Base, 
    meta: { 
      requiresAuth: true,
      requiresAdmin: true 
    } 
  },
  { 
    path: '/courses', 
    component: Courses, 
    meta: { requiresAuth: true } 
  },
  { 
    path: '/my-courses', 
    component: MyCourses, 
    meta: { requiresAuth: true } 
  },
  { 
    path: '/courses/:id', 
    component: CourseDetail, 
    meta: { requiresAuth: true } 
  },
  { 
    path: '/:pathMatch(.*)*', 
    redirect: '/home'
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

// Cache de rol de usuario
let userRoleCache = null
let userProfileCache = null

// Función para obtener perfil del usuario
async function getUserProfile() {
  if (userProfileCache) return userProfileCache
  
  try {
    const token = localStorage.getItem('token')
    if (!token) {
      console.log('No token found in localStorage')
      return null
    }
    
    const backendUrl = window.location.hostname === 'localhost' 
      ? 'http://localhost:5002' 
      : '/api/auth'
    
    const response = await fetch(`${backendUrl}/api/auth/user-profile`, {
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      }
    })
    
    if (response.ok) {
      const profile = await response.json()
      userProfileCache = profile
      userRoleCache = profile.rol || 'Empleado'
      return profile
    } else {
      console.warn('Failed to fetch user profile, status:', response.status)
      // Limpiar cache si hay error de autenticación
      if (response.status === 401) {
        localStorage.removeItem('token')
        userProfileCache = null
        userRoleCache = null
      }
    }
    
    return null
  } catch (error) {
    console.error('Error fetching user profile:', error)
    userRoleCache = 'Empleado'
    return null
  }
}

// Función para obtener solo el rol
async function getUserRole() {
  if (userRoleCache) return userRoleCache
  
  const profile = await getUserProfile()
  return profile?.rol || 'Empleado'
}

// Verificar si el email está verificado
function isEmailVerified(user) {
  return user?.email_confirmed_at || user?.confirmed_at || user?.email_verified
}

// Guardia de navegación
router.beforeEach(async (to, from, next) => {
  try {
    const { data: { session } } = await supabase.auth.getSession()
    const token = localStorage.getItem('token')
    const isAuthenticated = !!(session && token && isEmailVerified(session.user))

    // Rutas que requieren autenticación
    if (to.meta.requiresAuth) {
      if (!isAuthenticated) {
        console.log('No autenticado, redirigiendo a signin')
        localStorage.removeItem('token')
        userProfileCache = null
        userRoleCache = null
        return next('/signin')
      }

      // Verificación de rol Admin
      if (to.meta.requiresAdmin) {
        const userRole = await getUserRole()
        console.log('Verificando rol admin, rol actual:', userRole)
        if (userRole !== 'Admin') {
          console.log('Acceso denegado, no es admin')
          return next('/home')
        }
      }
      
      next()
    } 
    // Rutas para invitados (cuando no está autenticado)
    else if (to.meta.requiresGuest && isAuthenticated) {
      console.log('Ya autenticado, redirigiendo a home')
      next('/home')
    } 
    // Permitir acceso a rutas públicas
    else {
      next()
    }
    
  } catch (error) {
    console.error('Error en guardia de navegación:', error)
    localStorage.removeItem('token')
    userProfileCache = null
    userRoleCache = null
    next('/signin')
  }
})

// Limpiar cache cuando cambie la autenticación
supabase.auth.onAuthStateChange(async (event, session) => {
  console.log('Auth state changed:', event)
  
  if (event === 'SIGNED_OUT') {
    localStorage.removeItem('token')
    userProfileCache = null
    userRoleCache = null
  } else if (event === 'SIGNED_IN' && session) {
    localStorage.setItem('token', session.access_token)
    // Limpiar cache para forzar refresco del perfil
    userProfileCache = null
    userRoleCache = null
  }
})

export default router

// Exportar funciones para usar en componentes
export { getUserProfile, getUserRole }