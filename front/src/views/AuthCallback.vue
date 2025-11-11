<template>
  <div class="callback-container">
    <div class="loading-spinner"></div>
    <p>Procesando autenticación...</p>
    <p v-if="error" class="error-message">{{ error }}</p>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { supabase } from '../supabase'

const router = useRouter()
const error = ref(null)

// Verificar si el email está verificado
function isEmailVerified(user) {
  return user.email_confirmed_at || user.confirmed_at || user.email_verified
}

async function syncUser(session) {
  try {
    const userEmail = session.user?.email
    if (!userEmail) {
      console.error('❌ No se pudo obtener email del usuario')
      return false
    }

    // Verificar que el email esté confirmado
    if (!isEmailVerified(session.user)) {
      console.error('❌ Email no verificado')
      return false
    }

    const backendUrl = window.location.hostname === 'localhost' ? 'http://localhost:5002' : '/api/auth'
    
    console.log('🔹 Llamando a sync-user...', { email: userEmail })
    
    const response = await fetch(`${backendUrl}/api/auth/sync-user`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${session.access_token}`,
        'Content-Type': 'application/json'
      }
    })

    if (response.ok) {
      const result = await response.json()
      console.log('✅ Sync-user exitoso:', result)
      return true
    } else {
      const errorText = await response.text()
      console.error('❌ Error en sync-user:', errorText)
      
      // Si el error es que el usuario ya existe, considerar éxito
      if (response.status === 400 && errorText.includes('ya existe')) {
        console.log('✅ Usuario ya existe, continuando...')
        return true
      }
      return false
    }
  } catch (error) {
    console.error('❌ Error en sync:', error)
    return false
  }
}

onMounted(async () => {
  try {
    console.log('🔄 Procesando callback de OAuth...')
    
    // Obtener la sesión después del redirect de OAuth
    const { data: { session }, error: sessionError } = await supabase.auth.getSession()
    
    if (sessionError) {
      console.error('❌ Error en callback:', sessionError)
      error.value = 'Error de autenticación. Intenta nuevamente.'
      setTimeout(() => router.push('/signin?error=auth_failed'), 3000)
      return
    }
    
    if (session) {
      console.log('✅ Sesión obtenida correctamente en callback')
      console.log('📧 Email verificado:', isEmailVerified(session.user))
      
      // Guardar el token
      localStorage.setItem('token', session.access_token)
      
      // Verificar email confirmado
      if (!isEmailVerified(session.user)) {
        console.warn('⚠️ Email no verificado')
        error.value = 'Por favor, verifica tu email antes de continuar.'
        setTimeout(() => router.push('/signin?error=email_not_verified'), 3000)
        return
      }
      
      // 🔹 HACER SYNC DEL USUARIO ANTES DE REDIRIGIR
      console.log('🔄 Sincronizando usuario...')
      const syncSuccess = await syncUser(session)
      
      if (syncSuccess) {
        console.log('✅ Sync exitoso, redirigiendo a home...')
        router.push('/home')
      } else {
        console.warn('⚠️ Sync falló, pero redirigiendo igual...')
        // Redirigir incluso si el sync falló
        router.push('/home')
      }
    } else {
      console.warn('⚠️ No se encontró sesión en callback')
      error.value = 'No se pudo obtener la sesión. Intenta nuevamente.'
      setTimeout(() => router.push('/signin?error=no_session'), 3000)
    }
  } catch (catchError) {
    console.error('❌ Error inesperado en callback:', catchError)
    error.value = 'Error inesperado. Intenta nuevamente.'
    setTimeout(() => router.push('/signin?error=callback_failed'), 3000)
  }
})
</script>

<style scoped>
.callback-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  height: 100vh;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  font-family: 'Inter', sans-serif;
  text-align: center;
  padding: 2rem;
}

.loading-spinner {
  width: 50px;
  height: 50px;
  border: 4px solid rgba(255, 255, 255, 0.3);
  border-top: 4px solid white;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin-bottom: 1.5rem;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

p {
  font-size: 1.1rem;
  font-weight: 500;
  margin-bottom: 0.5rem;
}

.error-message {
  background: rgba(220, 38, 38, 0.9);
  padding: 0.75rem 1.5rem;
  border-radius: 0.5rem;
  margin-top: 1rem;
  font-weight: 600;
  max-width: 400px;
}
</style>