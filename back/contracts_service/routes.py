from fastapi import APIRouter, HTTPException, Depends
from typing import List
from models import ContratoCreate, ContratoUpdate, ContratoResponse, ContratoWithUsuario
from database import get_supabase_client
from supabase import Client

router = APIRouter(prefix="/api/contracts", tags=["Contratos"])
auth_router = APIRouter(prefix="/api/auth", tags=["Autenticación"])

def get_db():
    return get_supabase_client()

@router.get("/", response_model=List[ContratoWithUsuario])
async def listar_contratos(supabase: Client = Depends(get_db)):
    """Listar todos los contratos con información del usuario"""
    try:
        response = supabase.table("contratos").select(
            "*, usuarios(nombre, apellido, email, cargo)"
        ).order("created_at", desc=True).execute()
        
        # Transformar datos
        contratos = []
        for row in response.data:
            usuario = row.pop('usuarios')
            contrato = {**row, **usuario}
            contratos.append(contrato)
        
        return contratos
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@router.get("/{contrato_id}", response_model=ContratoWithUsuario)
async def obtener_contrato(contrato_id: str, supabase: Client = Depends(get_db)):
    """Obtener un contrato específico"""
    try:
        response = supabase.table("contratos").select(
            "*, usuarios(nombre, apellido, email, cargo)"
        ).eq("id", contrato_id).single().execute()
        
        if not response.data:
            raise HTTPException(status_code=404, detail="Contrato no encontrado")
        
        usuario = response.data.pop('usuarios')
        contrato = {**response.data, **usuario}
        
        return contrato
    except Exception as e:
        if "not found" in str(e).lower():
            raise HTTPException(status_code=404, detail="Contrato no encontrado")
        raise HTTPException(status_code=500, detail=str(e))

@router.get("/usuario/{usuario_id}", response_model=List[ContratoResponse])
async def listar_contratos_usuario(usuario_id: str, supabase: Client = Depends(get_db)):
    """Listar contratos de un usuario específico"""
    try:
        response = supabase.table("contratos").select("*").eq(
            "usuario_id", usuario_id
        ).order("created_at", desc=True).execute()
        
        return response.data
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@router.post("/", response_model=ContratoResponse)
async def crear_contrato(contrato: ContratoCreate, supabase: Client = Depends(get_db)):
    """Crear un nuevo contrato"""
    try:
        # Verificar que el usuario existe
        user_response = supabase.table("usuarios").select("id").eq(
            "id", contrato.usuario_id
        ).single().execute()
        
        if not user_response.data:
            raise HTTPException(status_code=404, detail="Usuario no encontrado")
        
        # Crear contrato
        data = {
            "usuario_id": contrato.usuario_id,
            "fecha_inicio": contrato.fecha_inicio.isoformat(),
            "salario": float(contrato.salario),
            "tiempo_prueba": contrato.tiempo_prueba
        }
        
        response = supabase.table("contratos").insert(data).execute()
        
        return response.data[0]
    except HTTPException:
        raise
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@router.put("/{contrato_id}", response_model=ContratoResponse)
async def actualizar_contrato(
    contrato_id: str, 
    contrato: ContratoUpdate, 
    supabase: Client = Depends(get_db)
):
    """Actualizar un contrato existente"""
    try:
        # Verificar que el contrato existe
        existing = supabase.table("contratos").select("id").eq(
            "id", contrato_id
        ).single().execute()
        
        if not existing.data:
            raise HTTPException(status_code=404, detail="Contrato no encontrado")
        
        # Construir datos de actualización
        update_data = {}
        if contrato.fecha_inicio is not None:
            update_data["fecha_inicio"] = contrato.fecha_inicio.isoformat()
        if contrato.salario is not None:
            update_data["salario"] = float(contrato.salario)
        if contrato.tiempo_prueba is not None:
            update_data["tiempo_prueba"] = contrato.tiempo_prueba
        if contrato.activo is not None:
            update_data["activo"] = contrato.activo
        
        if not update_data:
            raise HTTPException(status_code=400, detail="No hay campos para actualizar")
        
        response = supabase.table("contratos").update(update_data).eq(
            "id", contrato_id
        ).execute()
        
        return response.data[0]
    except HTTPException:
        raise
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@router.delete("/{contrato_id}")
async def eliminar_contrato(contrato_id: str, supabase: Client = Depends(get_db)):
    """Eliminar un contrato"""
    try:
        response = supabase.table("contratos").delete().eq(
            "id", contrato_id
        ).execute()
        
        if not response.data:
            raise HTTPException(status_code=404, detail="Contrato no encontrado")
        
        return {"message": "Contrato eliminado exitosamente"}
    except HTTPException:
        raise
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@router.get("/stats/activos")
async def contar_contratos_activos(supabase: Client = Depends(get_db)):
    """Contar contratos activos"""
    try:
        response = supabase.table("contratos").select(
            "id", count="exact"
        ).eq("activo", True).execute()
        
        return {"count": response.count}
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


@auth_router.get("/users")
async def listar_usuarios(supabase: Client = Depends(get_db)):
    """Listar todos los usuarios"""
    try:
        response = supabase.table("usuarios").select("*").order("created_at", desc=True).execute()
        
        return response.data
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
