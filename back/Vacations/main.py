from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from datetime import datetime
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

from dotenv import load_dotenv
load_dotenv()

SUPABASE_URL = os.getenv("SUPABASE_URL")
SUPABASE_ANON_KEY = os.getenv("SUPABASE_ANON_KEY")
SUPABASE_SERVICE_ROLE_KEY = os.getenv("SUPABASE_SERVICE_ROLE_KEY")

if not all([SUPABASE_URL, SUPABASE_ANON_KEY, SUPABASE_SERVICE_ROLE_KEY]):
    raise RuntimeError("Variables de entorno de Supabase no configuradas. Verifica tu archivo .env")

from routes.vacation_routes import router as vacation_router

app = FastAPI(
    title="Vacations Microservice",
    version="1.0.0",
    description="API para gestión de vacaciones de empleados"
)

# CORS
app.add_middleware(
    CORSMiddleware,
    allow_origins=[
        "http://localhost:5173",
        "http://localhost:3000",
        "http://frontend:80",
        "http://localhost:5001"
    ],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Registrar rutas
app.include_router(vacation_router)

# Endpoints básicos
@app.get("/")
async def root():
    return {
        "service": "Vacations API",
        "version": "1.0.0",
        "status": "running",
        "description": "Gestión de vacaciones para empleados con 1+ año de antigüedad"
    }

@app.get("/health")
async def health():
    return {
        "status": "healthy",
        "timestamp": datetime.utcnow().isoformat(),
        "service": "vacations"
    }

# Manejo de errores global
@app.exception_handler(Exception)
async def global_exception_handler(request, exc):
    print(f"❌ Error no controlado: {exc}")
    return {
        "error": "Error interno del servidor",
        "detail": str(exc)
    }
