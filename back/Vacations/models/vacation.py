from pydantic import BaseModel, Field, validator
from typing import Optional
from datetime import date

class VacationRequest(BaseModel):
    """Modelo para solicitud de vacaciones"""
    fecha_inicio: str = Field(..., description="Fecha de inicio de vacaciones (YYYY-MM-DD)")
    fecha_fin: str = Field(..., description="Fecha de fin de vacaciones (YYYY-MM-DD)")
    gestion: int = Field(..., description="Año de la gestión (ej: 2024, 2025)")
    
    @validator('gestion')
    def validate_gestion(cls, v):
        if v < 2020 or v > 2100:
            raise ValueError('Gestión debe estar entre 2020 y 2100')
        return v

class VacationApproval(BaseModel):
    """Modelo para aprobar/rechazar solicitud de vacaciones"""
    approved: bool = Field(..., description="True para aprobar, False para rechazar")
    motivo_rechazo: Optional[str] = Field(None, description="Motivo del rechazo (si aplica)")

