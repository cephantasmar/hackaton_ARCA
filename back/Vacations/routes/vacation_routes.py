from fastapi import APIRouter, Header, Query
from typing import Optional
import sys
import os

# Añadir path para imports
sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from models.vacation import VacationRequest, VacationApproval
from controllers import vacation_controller
from utils.supabase import get_current_user

router = APIRouter(prefix="/api/vacations", tags=["Vacations"])

@router.get("/me")
async def get_current_user_info(authorization: str = Header(None)):
    """Obtener información del usuario actual (incluyendo rol)"""
    user_jwt = await get_current_user(authorization)
    from utils.supabase import get_user_by_email
    user = await get_user_by_email(user_jwt["email"])
    if not user:
        from fastapi import HTTPException
        raise HTTPException(status_code=404, detail="Usuario no encontrado")
    return {
        "id": user["id"],
        "nombre": user.get("nombre", ""),
        "apellido": user.get("apellido", ""),
        "email": user["email"],
        "rol": user.get("rol", ""),
        "cargo": user.get("cargo", "")
    }

@router.get("/balance/{gestion}")
async def get_vacation_balance(gestion: int, authorization: str = Header(None)):
    """Obtener balance de días de vacaciones para una gestión del usuario autenticado"""
    user = await get_current_user(authorization)
    return await vacation_controller.get_vacation_balance(user["email"], gestion)

@router.get("/eligibility")
async def check_eligibility(authorization: str = Header(None)):
    """Verificar elegibilidad para vacaciones del usuario autenticado"""
    user = await get_current_user(authorization)
    return await vacation_controller.check_vacation_eligibility(user["email"])

@router.post("/request")
async def request_vacation(vacation: VacationRequest, authorization: str = Header(None)):
    """Solicitar vacaciones"""
    user = await get_current_user(authorization)
    return await vacation_controller.request_vacation(
        user["email"],
        vacation.fecha_inicio,
        vacation.fecha_fin,
        vacation.gestion
    )

@router.get("/my-vacations")
async def get_my_vacations(authorization: str = Header(None)):
    """Obtener mis solicitudes de vacaciones"""
    user = await get_current_user(authorization)
    return await vacation_controller.list_my_vacations(user["email"])

@router.get("/all")
async def get_all_vacations(authorization: str = Header(None)):
    """Listar todas las solicitudes de vacaciones (administradores)"""
    user = await get_current_user(authorization)
    return await vacation_controller.list_all_vacations(user["email"])

@router.patch("/{vacation_id}/approve")
async def approve_or_reject_vacation(
    vacation_id: str, 
    approval: VacationApproval, 
    authorization: str = Header(None)
):
    """Aprobar o rechazar solicitud de vacaciones"""
    user = await get_current_user(authorization)
    return await vacation_controller.approve_vacation(
        vacation_id,
        user["email"],
        approval.approved,
        approval.motivo_rechazo
    )

@router.delete("/{vacation_id}")
async def delete_vacation(vacation_id: str, authorization: str = Header(None)):
    """Eliminar solicitud de vacaciones (solo pendientes)"""
    user = await get_current_user(authorization)
    return await vacation_controller.delete_vacation(vacation_id, user["email"])

