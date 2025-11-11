// Vacations API Service
const API_BASE = 'http://localhost:5010/api/vacations'

export const vacationsService = {
  /**
   * Obtener balance de vacaciones
   */
  async getBalance(gestion) {
    const token = localStorage.getItem('token')
    const response = await fetch(`${API_BASE}/balance/${gestion}`, {
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      }
    })
    if (!response.ok) throw new Error('Error al obtener balance de vacaciones')
    return response.json()
  },

  /**
   * Solicitar vacaciones
   */
  async requestVacation(vacationData) {
    const token = localStorage.getItem('token')
    const response = await fetch(`${API_BASE}/request`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(vacationData)
    })
    if (!response.ok) {
      const error = await response.json()
      throw new Error(error.detail || 'Error al solicitar vacaciones')
    }
    return response.json()
  },

  /**
   * Obtener mis vacaciones
   */
  async getMyVacations() {
    const token = localStorage.getItem('token')
    const response = await fetch(`${API_BASE}/my-vacations`, {
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      }
    })
    if (!response.ok) throw new Error('Error al obtener mis vacaciones')
    return response.json()
  },

  /**
   * Obtener todas las solicitudes (admin)
   */
  async getAllVacations(estado = null) {
    const token = localStorage.getItem('token')
    const url = estado ? `${API_BASE}/all?estado=${estado}` : `${API_BASE}/all`
    const response = await fetch(url, {
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      }
    })
    if (!response.ok) throw new Error('Error al obtener todas las vacaciones')
    return response.json()
  },

  /**
   * Obtener información del usuario actual
   */
  async getCurrentUser() {
    const token = localStorage.getItem('token')
    const response = await fetch(`${API_BASE}/me`, {
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      }
    })
    if (!response.ok) throw new Error('Error al obtener información del usuario')
    return response.json()
  },

  /**
   * Aprobar o rechazar solicitud
   */
  async approveOrReject(vacationId, approved, motivoRechazo = null) {
    const token = localStorage.getItem('token')
    const response = await fetch(`${API_BASE}/${vacationId}/approve`, {
      method: 'PATCH',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        approved,
        motivo_rechazo: motivoRechazo
      })
    })
    if (!response.ok) {
      const error = await response.json()
      throw new Error(error.detail || error.error || 'Error al procesar solicitud')
    }
    return response.json()
  },

  /**
   * Eliminar solicitud de vacaciones
   */
  async deleteVacation(vacationId) {
    const token = localStorage.getItem('token')
    const response = await fetch(`${API_BASE}/${vacationId}`, {
      method: 'DELETE',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      }
    })
    if (!response.ok) {
      const error = await response.json()
      throw new Error(error.detail || 'Error al eliminar solicitud')
    }
    return response.json()
  },

  /**
   * Verificar elegibilidad
   */
  async checkEligibility() {
    const token = localStorage.getItem('token')
    const response = await fetch(`${API_BASE}/eligibility`, {
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      }
    })
    if (!response.ok) throw new Error('Error al verificar elegibilidad')
    return response.json()
  }
}

export default vacationsService
