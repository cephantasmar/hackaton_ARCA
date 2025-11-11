from datetime import datetime, timedelta
from typing import Dict, List, Optional
import httpx
from utils.supabase import (
    get_user_by_email, 
    SUPABASE_URL, 
    SUPABASE_SERVICE_ROLE_KEY
)

async def calculate_seniority_years(contrato_fecha_inicio: str) -> int:
    """Calcula los años de antigüedad desde la fecha de inicio del contrato"""
    try:
        fecha_inicio = datetime.strptime(contrato_fecha_inicio, "%Y-%m-%d")
        hoy = datetime.now()
        years = (hoy - fecha_inicio).days // 365
        return years
    except Exception as e:
        print(f"❌ Error calculando antigüedad: {e}")
        return 0

async def get_employee_contract(empleado_id: str) -> Optional[Dict]:
    """Obtiene el contrato activo del empleado desde la tabla contratos"""
    try:
        async with httpx.AsyncClient(timeout=10.0) as client:
            headers = {
                "apikey": SUPABASE_SERVICE_ROLE_KEY,
                "Authorization": f"Bearer {SUPABASE_SERVICE_ROLE_KEY}"
            }
            response = await client.get(
                f"{SUPABASE_URL}/rest/v1/contratos?usuario_id=eq.{empleado_id}&activo=eq.true&select=*&order=fecha_inicio.desc&limit=1",
                headers=headers
            )
            if response.status_code == 200:
                contratos = response.json()
                return contratos[0] if contratos else None
    except Exception as e:
        print(f"❌ Error obteniendo contrato: {e}")
    return None

async def get_vacation_requests_by_user(empleado_id: str, gestion: int) -> List[Dict]:
    """Obtiene todas las solicitudes de vacaciones aprobadas de un empleado para una gestión"""
    try:
        async with httpx.AsyncClient(timeout=10.0) as client:
            headers = {
                "apikey": SUPABASE_SERVICE_ROLE_KEY,
                "Authorization": f"Bearer {SUPABASE_SERVICE_ROLE_KEY}"
            }
            response = await client.get(
                f"{SUPABASE_URL}/rest/v1/solicitudes_vacaciones?empleado_id=eq.{empleado_id}&gestion=eq.{gestion}&estado=eq.aprobada&select=*",
                headers=headers
            )
            if response.status_code == 200:
                return response.json()
    except Exception as e:
        print(f"❌ Error obteniendo solicitudes: {e}")
    return []

async def check_vacation_eligibility(email: str) -> Dict:
    """
    Verifica si el empleado es elegible para solicitar vacaciones.
    Requisito: Tener al menos 1 año de antigüedad (1 gestión completada)
    """
    try:
        # Obtener usuario
        user = await get_user_by_email(email)
        if not user:
            return {"eligible": False, "message": "Usuario no encontrado"}
        
        # Obtener contrato del empleado
        contrato = await get_employee_contract(user["id"])
        if not contrato:
            return {"eligible": False, "message": "No se encontró contrato activo del empleado"}
        
        # Calcular años de antigüedad
        years = await calculate_seniority_years(contrato["fecha_inicio"])
        
        if years < 1:
            return {
                "eligible": False, 
                "message": f"Se requiere al menos 1 año de antigüedad. Tienes {years} año(s)",
                "years_of_service": years
            }
        
        return {
            "eligible": True,
            "message": f"Elegible para vacaciones. Antigüedad: {years} año(s)",
            "years_of_service": years
        }
    
    except Exception as e:
        print(f"❌ Error verificando elegibilidad: {e}")
        return {"eligible": False, "message": f"Error del servidor: {str(e)}"}

async def get_vacation_balance(email: str, gestion: int) -> Dict:
    """
    Calcula el balance de días de vacaciones disponibles.
    - 15 días por cada año de antigüedad completado
    - Resta los días ya aprobados en esa gestión
    """
    try:
        # Obtener usuario
        user = await get_user_by_email(email)
        if not user:
            return {"error": "Usuario no encontrado"}
        
        # Obtener contrato
        contrato = await get_employee_contract(user["id"])
        if not contrato:
            return {"error": "No se encontró contrato activo"}
        
        # Calcular antigüedad
        years = await calculate_seniority_years(contrato["fecha_inicio"])
        
        # Días disponibles = 15 días por año de antigüedad
        dias_disponibles = years * 15
        
        # Obtener días ya aprobados en esta gestión
        solicitudes = await get_vacation_requests_by_user(user["id"], gestion)
        dias_usados = sum(s["dias_solicitados"] for s in solicitudes)
        
        dias_restantes = dias_disponibles - dias_usados
        
        return {
            "empleado_id": user["id"],
            "nombre": f"{user.get('nombre', '')} {user.get('apellido', '')}",
            "gestion": gestion,
            "years_of_service": years,
            "dias_disponibles": dias_disponibles,
            "dias_usados": dias_usados,
            "dias_restantes": max(0, dias_restantes)
        }
    
    except Exception as e:
        print(f"❌ Error calculando balance: {e}")
        return {"error": f"Error del servidor: {str(e)}"}

async def request_vacation(email: str, fecha_inicio: str, fecha_fin: str, gestion: int) -> Dict:
    """
    Crea una nueva solicitud de vacaciones.
    Validaciones:
    - El empleado debe ser elegible (>= 1 año de antigüedad)
    - No puede exceder 15 días en la solicitud
    - No puede exceder el balance disponible para la gestión
    """
    try:
        # Verificar elegibilidad
        eligibility = await check_vacation_eligibility(email)
        if not eligibility["eligible"]:
            return {"error": eligibility["message"]}
        
        # Obtener usuario
        user = await get_user_by_email(email)
        
        # Calcular días solicitados
        start = datetime.strptime(fecha_inicio, "%Y-%m-%d")
        end = datetime.strptime(fecha_fin, "%Y-%m-%d")
        dias_solicitados = (end - start).days + 1
        
        # Validar límite de 15 días por solicitud
        if dias_solicitados > 15:
            return {"error": "No se pueden solicitar más de 15 días por solicitud"}
        
        if dias_solicitados <= 0:
            return {"error": "La fecha de fin debe ser posterior a la fecha de inicio"}
        
        # Verificar balance disponible
        balance = await get_vacation_balance(email, gestion)
        if "error" in balance:
            return balance
        
        if dias_solicitados > balance["dias_restantes"]:
            return {
                "error": f"Días insuficientes. Solicitados: {dias_solicitados}, Disponibles: {balance['dias_restantes']}"
            }
        
        # Crear solicitud
        async with httpx.AsyncClient(timeout=10.0) as client:
            headers = {
                "apikey": SUPABASE_SERVICE_ROLE_KEY,
                "Authorization": f"Bearer {SUPABASE_SERVICE_ROLE_KEY}",
                "Content-Type": "application/json",
                "Prefer": "return=representation"
            }
            
            data = {
                "empleado_id": str(user["id"]),
                "fecha_inicio": fecha_inicio,
                "fecha_fin": fecha_fin,
                "dias_solicitados": dias_solicitados,
                "gestion": gestion,
                "estado": "pendiente"
            }
            
            response = await client.post(
                f"{SUPABASE_URL}/rest/v1/solicitudes_vacaciones",
                headers=headers,
                json=data
            )
            
            if response.status_code == 201:
                return {
                    "success": True,
                    "message": "Solicitud de vacaciones creada exitosamente",
                    "data": response.json()[0] if response.json() else data
                }
            else:
                return {"error": f"Error al crear solicitud: {response.text}"}
    
    except Exception as e:
        print(f"❌ Error creando solicitud: {e}")
        return {"error": f"Error del servidor: {str(e)}"}

async def list_my_vacations(email: str) -> Dict:
    """Lista todas las solicitudes de vacaciones del usuario actual"""
    try:
        user = await get_user_by_email(email)
        if not user:
            return {"error": "Usuario no encontrado"}
        
        async with httpx.AsyncClient(timeout=10.0) as client:
            headers = {
                "apikey": SUPABASE_SERVICE_ROLE_KEY,
                "Authorization": f"Bearer {SUPABASE_SERVICE_ROLE_KEY}"
            }
            response = await client.get(
                f"{SUPABASE_URL}/rest/v1/solicitudes_vacaciones?empleado_id=eq.{user['id']}&select=*&order=created_at.desc",
                headers=headers
            )
            
            if response.status_code == 200:
                return {"vacations": response.json()}
            else:
                return {"error": f"Error al obtener solicitudes: {response.text}"}
    
    except Exception as e:
        print(f"❌ Error listando mis vacaciones: {e}")
        return {"error": f"Error del servidor: {str(e)}"}

async def list_all_vacations(admin_email: str) -> Dict:
    """
    Lista todas las solicitudes de vacaciones.
    Solo para administradores.
    """
    try:
        # Verificar que el usuario sea administrador
        admin = await get_user_by_email(admin_email)
        if not admin:
            return {"error": "Usuario no encontrado"}
        
        if admin.get("rol", "").lower() not in ["admin", "administrador", "director"]:
            return {"error": "No tienes permisos para ver todas las solicitudes. Solo administradores."}
        
        # Obtener todas las solicitudes
        async with httpx.AsyncClient(timeout=10.0) as client:
            headers = {
                "apikey": SUPABASE_SERVICE_ROLE_KEY,
                "Authorization": f"Bearer {SUPABASE_SERVICE_ROLE_KEY}"
            }
            
            # Obtener solicitudes
            response = await client.get(
                f"{SUPABASE_URL}/rest/v1/solicitudes_vacaciones?select=*&order=created_at.desc",
                headers=headers
            )
            
            if response.status_code != 200:
                return {"error": f"Error al obtener solicitudes: {response.text}"}
            
            vacations = response.json()
            
            # Enriquecer con datos del empleado
            for vacation in vacations:
                empleado_id = vacation.get("empleado_id")
                if empleado_id:
                    emp_response = await client.get(
                        f"{SUPABASE_URL}/rest/v1/usuarios?id=eq.{empleado_id}&select=id,nombre,apellido,email,cargo",
                        headers=headers
                    )
                    if emp_response.status_code == 200 and emp_response.json():
                        vacation["empleado"] = emp_response.json()[0]
                    else:
                        vacation["empleado"] = None
            
            return {"vacations": vacations}
    
    except Exception as e:
        print(f"❌ Error listando todas las vacaciones: {e}")
        return {"error": f"Error del servidor: {str(e)}"}

async def approve_vacation(vacation_id: str, admin_email: str, approved: bool, motivo_rechazo: str = None) -> Dict:
    """
    Aprueba o rechaza una solicitud de vacaciones.
    Solo para administradores.
    """
    try:
        # Verificar que el usuario sea administrador
        admin = await get_user_by_email(admin_email)
        if not admin:
            return {"error": "Administrador no encontrado"}
        
        if admin.get("rol", "").lower() not in ["admin", "administrador", "director"]:
            return {"error": "No tienes permisos para aprobar solicitudes. Solo administradores."}
        
        async with httpx.AsyncClient(timeout=10.0) as client:
            headers = {
                "apikey": SUPABASE_SERVICE_ROLE_KEY,
                "Authorization": f"Bearer {SUPABASE_SERVICE_ROLE_KEY}",
                "Content-Type": "application/json",
                "Prefer": "return=representation"
            }
            
            # Verificar que la solicitud existe
            check_response = await client.get(
                f"{SUPABASE_URL}/rest/v1/solicitudes_vacaciones?id=eq.{vacation_id}&select=*",
                headers=headers
            )
            
            if check_response.status_code != 200 or not check_response.json():
                return {"error": "Solicitud no encontrada"}
            
            solicitud = check_response.json()[0]
            
            # No permitir cambiar solicitudes ya procesadas
            if solicitud["estado"] != "pendiente":
                return {"error": f"La solicitud ya fue {solicitud['estado']}"}
            
            data = {
                "estado": "aprobada" if approved else "rechazada",
                "aprobado_por": str(admin["id"]),
                "fecha_aprobacion": datetime.now().isoformat()
            }
            
            if not approved and motivo_rechazo:
                data["motivo_rechazo"] = motivo_rechazo
            
            response = await client.patch(
                f"{SUPABASE_URL}/rest/v1/solicitudes_vacaciones?id=eq.{vacation_id}",
                headers=headers,
                json=data
            )
            
            if response.status_code == 200:
                return {
                    "success": True,
                    "message": f"Solicitud {'aprobada' if approved else 'rechazada'} exitosamente",
                    "data": response.json()[0] if response.json() else data
                }
            else:
                return {"error": f"Error al actualizar solicitud: {response.text}"}
    
    except Exception as e:
        print(f"❌ Error aprobando/rechazando solicitud: {e}")
        return {"error": f"Error del servidor: {str(e)}"}

async def delete_vacation(vacation_id: str, email: str) -> Dict:
    """Elimina (cancela) una solicitud de vacaciones propia que esté pendiente"""
    try:
        user = await get_user_by_email(email)
        if not user:
            return {"error": "Usuario no encontrado"}
        
        async with httpx.AsyncClient(timeout=10.0) as client:
            headers = {
                "apikey": SUPABASE_SERVICE_ROLE_KEY,
                "Authorization": f"Bearer {SUPABASE_SERVICE_ROLE_KEY}"
            }
            
            # Verificar que la solicitud sea del usuario y esté pendiente
            response = await client.get(
                f"{SUPABASE_URL}/rest/v1/solicitudes_vacaciones?id=eq.{vacation_id}&empleado_id=eq.{user['id']}&select=*",
                headers=headers
            )
            
            if response.status_code != 200 or not response.json():
                return {"error": "Solicitud no encontrada o no autorizada"}
            
            vacation = response.json()[0]
            if vacation["estado"] != "pendiente":
                return {"error": "Solo se pueden eliminar solicitudes pendientes"}
            
            # Eliminar
            response = await client.delete(
                f"{SUPABASE_URL}/rest/v1/solicitudes_vacaciones?id=eq.{vacation_id}",
                headers=headers
            )
            
            if response.status_code == 204:
                return {"success": True, "message": "Solicitud eliminada exitosamente"}
            else:
                return {"error": f"Error al eliminar solicitud: {response.text}"}
    
    except Exception as e:
        print(f"❌ Error eliminando solicitud: {e}")
        return {"error": f"Error del servidor: {str(e)}"}
