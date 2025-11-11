from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from dotenv import load_dotenv
import os

# Cargar variables de entorno desde .env
load_dotenv()

# Verificar que las variables estén configuradas
SUPABASE_URL = os.getenv("SUPABASE_URL")
SUPABASE_ANON_KEY = os.getenv("SUPABASE_ANON_KEY")

if not all([SUPABASE_URL, SUPABASE_ANON_KEY]):
    raise RuntimeError("❌ Variables de entorno de Supabase no configuradas. Verifica tu archivo .env")

from routes import router, auth_router

app = FastAPI(
    title="Contratos Service",
    description="Microservicio de gestión de contratos",
    version="1.0.0"
)

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

app.include_router(router)
app.include_router(auth_router)

@app.get("/health")
async def health_check():
    return {"status": "healthy", "service": "contracts"}

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=5003)
