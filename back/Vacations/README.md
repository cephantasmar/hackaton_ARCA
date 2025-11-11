# Microservicio de Vacaciones

Sistema de gestión de vacaciones para empleados con 1+ año de antigüedad.

## Características

- **Elegibilidad**: Empleados con 1 año de antigüedad pueden solicitar vacaciones
- **Límite**: 15 días por gestión completada
- **Estados**: Pendiente, Aprobado, Rechazado
- **Balance**: Ver días disponibles, usados y pendientes
- **Gestión**: Administradores pueden aprobar/rechazar solicitudes

## Estructura del Backend

```
back/Vacations/
├── main.py                 # Aplicación FastAPI principal
├── requirements.txt        # Dependencias Python
├── Dockerfile             # Configuración Docker
├── schema.sql             # Schema SQL para Supabase
├── .env                   # Variables de entorno
├── controllers/
│   └── vacation_controller.py  # Lógica de negocio
├── models/
│   └── vacation.py        # Modelos Pydantic
├── routes/
│   └── vacation_routes.py # Rutas de la API
└── utils/
    └── supabase.py        # Helpers de Supabase
```

## Instalación Backend

### 1. Crear base de datos en Supabase

Ejecuta el script `schema.sql` en tu proyecto de Supabase para crear las tablas necesarias:

```sql
-- Ejecutar en Supabase SQL Editor
-- Ver schema.sql para el script completo
```

### 2. Configurar variables de entorno

Crea un archivo `.env` en `back/Vacations/` con:

```env
SUPABASE_URL=tu_url_de_supabase
SUPABASE_ANON_KEY=tu_anon_key
SUPABASE_SERVICE_ROLE_KEY=tu_service_role_key
SUPABASE_JWT_SECRET=tu_jwt_secret
```

### 3. Instalar dependencias

```bash
cd back/Vacations
pip install -r requirements.txt
```

### 4. Ejecutar el servidor

```bash
uvicorn main:app --reload --port 8003
```

El servidor estará disponible en: http://localhost:8003

## API Endpoints

### Balance de Vacaciones
```
GET /api/vacations/balance/{empleado_id}/{gestion}
```
Obtiene el balance de días de vacaciones para un empleado en una gestión específica.

### Solicitar Vacaciones
```
POST /api/vacations/request
Content-Type: application/json

{
  "empleado_id": 1,
  "fecha_inicio": "2025-01-15",
  "fecha_fin": "2025-01-29",
  "gestion": 2025
}
```

### Mis Vacaciones
```
GET /api/vacations/my-vacations
```
Lista todas las solicitudes del empleado autenticado.

### Todas las Solicitudes (Admin)
```
GET /api/vacations/all?estado=pendiente
```
Lista todas las solicitudes (con filtro opcional por estado).

### Aprobar/Rechazar
```
PATCH /api/vacations/{vacation_id}/approve
Content-Type: application/json

{
  "estado": "aprobado",  // o "rechazado"
  "motivo_rechazo": "Motivo..." // solo si es rechazado
}
```

### Eliminar Solicitud
```
DELETE /api/vacations/{vacation_id}
```
Solo solicitudes pendientes pueden ser eliminadas.

### Verificar Elegibilidad
```
GET /api/vacations/eligibility/{empleado_id}
```

## Estructura del Frontend

```
front/src/
├── services/
│   └── vacations.js          # API service
├── views/
│   ├── MyVacations.vue       # Vista de empleado
│   └── VacationsManagement.vue  # Vista de administrador
└── router.js                 # Rutas agregadas
```

## Instalación Frontend

Las rutas ya están configuradas en el router:

- `/my-vacations` - Para todos los empleados
- `/vacations-management` - Solo para directores/administradores

### Uso

1. **Empleados**: Pueden ver su balance, solicitar vacaciones y ver el estado de sus solicitudes
2. **Administradores**: Pueden ver todas las solicitudes, aprobar o rechazar

## Reglas de Negocio

1. **Antigüedad**: El empleado debe tener al menos 1 año de antigüedad
2. **Días máximos**: 15 días por gestión completada
3. **Validaciones**:
   - Las fechas de fin deben ser >= fechas de inicio
   - No se pueden solicitar más días de los disponibles
   - Solo solicitudes pendientes pueden ser eliminadas
   - El rechazo requiere un motivo

## Modelo de Datos

### Tabla: empleados
```sql
id              SERIAL PRIMARY KEY
nombre          VARCHAR(100)
apellido        VARCHAR(100)
email           VARCHAR(255) UNIQUE
fecha_contratacion  DATE
activo          BOOLEAN
created_at      TIMESTAMP
```

### Tabla: vacaciones
```sql
id              SERIAL PRIMARY KEY
empleado_id     INTEGER (FK a empleados)
fecha_inicio    DATE
fecha_fin       DATE
dias_solicitados INTEGER
gestion         INTEGER
estado          VARCHAR(20) -- pendiente, aprobado, rechazado
motivo_rechazo  TEXT
aprobado_por    INTEGER (FK a empleados)
created_at      TIMESTAMP
updated_at      TIMESTAMP
```

## Docker

### Construir imagen
```bash
cd back/Vacations
docker build -t vacations-api .
```

### Ejecutar contenedor
```bash
docker run -p 8003:8000 --env-file .env vacations-api
```

## Pruebas

### Crear un empleado de prueba
```sql
INSERT INTO empleados (nombre, apellido, email, fecha_contratacion, activo)
VALUES ('Juan', 'Pérez', 'juan.perez@empresa.com', '2023-01-15', true);
```

### Solicitar vacaciones (ejemplo con curl)
```bash
curl -X POST http://localhost:8003/api/vacations/request \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "empleado_id": 1,
    "fecha_inicio": "2025-02-01",
    "fecha_fin": "2025-02-10",
    "gestion": 2025
  }'
```

## Swagger Documentation

Una vez que el servidor esté corriendo, visita:
```
http://localhost:8003/docs
```

## Troubleshooting

### Error: "No tiene derecho a vacaciones"
- Verificar que el empleado tenga al menos 1 año de antigüedad
- Verificar que la fecha_contratacion esté correctamente registrada

### Error: "No hay suficientes días disponibles"
- Verificar el balance con GET /api/vacations/balance
- Revisar si hay solicitudes pendientes que estén "bloqueando" días

### Error: "Token inválido"
- Verificar que el token JWT esté presente en el header Authorization
- Verificar que SUPABASE_JWT_SECRET esté correctamente configurado

## Próximos Pasos

- [ ] Agregar notificaciones por email
- [ ] Implementar calendario visual
- [ ] Agregar reportes de vacaciones
- [ ] Implementar sistema de aprobación multinivel
- [ ] Agregar histórico de gestiones anteriores
