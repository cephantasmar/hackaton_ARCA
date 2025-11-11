from pydantic import BaseModel
from datetime import date
from typing import Optional
from decimal import Decimal

class ContratoCreate(BaseModel):
    usuario_id: str
    fecha_inicio: date
    salario: Decimal
    tiempo_prueba: int = 30

class ContratoUpdate(BaseModel):
    fecha_inicio: Optional[date] = None
    salario: Optional[Decimal] = None
    tiempo_prueba: Optional[int] = None
    activo: Optional[bool] = None

class ContratoResponse(BaseModel):
    id: str
    usuario_id: str
    fecha_inicio: date
    salario: Decimal
    tiempo_prueba: int
    activo: bool
    created_at: str
    updated_at: str
    
class ContratoWithUsuario(ContratoResponse):
    nombre: str
    apellido: str
    email: str
    cargo: Optional[str] = None
