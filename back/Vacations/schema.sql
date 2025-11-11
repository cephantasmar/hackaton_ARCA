-- Tabla de Solicitudes de Vacaciones
-- Esta tabla almacena todas las solicitudes de vacaciones de los usuarios
CREATE TABLE IF NOT EXISTS solicitudes_vacaciones (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    empleado_id UUID NOT NULL REFERENCES usuarios(id) ON DELETE CASCADE,
    fecha_solicitud TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    fecha_inicio DATE NOT NULL,
    fecha_fin DATE NOT NULL,
    dias_solicitados INTEGER NOT NULL CHECK (dias_solicitados > 0 AND dias_solicitados <= 15),
    gestion INTEGER NOT NULL,
    estado VARCHAR(20) NOT NULL DEFAULT 'pendiente' CHECK (estado IN ('pendiente', 'aprobada', 'rechazada')),
    aprobado_por UUID REFERENCES usuarios(id) ON DELETE SET NULL,
    fecha_aprobacion TIMESTAMP WITH TIME ZONE,
    motivo_rechazo TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Índices para mejorar el rendimiento
CREATE INDEX IF NOT EXISTS idx_solicitudes_empleado ON solicitudes_vacaciones(empleado_id);
CREATE INDEX IF NOT EXISTS idx_solicitudes_estado ON solicitudes_vacaciones(estado);
CREATE INDEX IF NOT EXISTS idx_solicitudes_gestion ON solicitudes_vacaciones(gestion);
CREATE INDEX IF NOT EXISTS idx_solicitudes_empleado_gestion ON solicitudes_vacaciones(empleado_id, gestion);

-- Trigger para actualizar updated_at
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$ language 'plpgsql';

CREATE TRIGGER update_solicitudes_vacaciones_updated_at 
    BEFORE UPDATE ON solicitudes_vacaciones 
    FOR EACH ROW 
    EXECUTE FUNCTION update_updated_at_column();

-- Comentarios
COMMENT ON TABLE solicitudes_vacaciones IS 'Solicitudes de vacaciones de los empleados';
COMMENT ON COLUMN solicitudes_vacaciones.empleado_id IS 'ID del empleado (referencia a usuarios.id)';
COMMENT ON COLUMN solicitudes_vacaciones.fecha_inicio IS 'Fecha de inicio de las vacaciones';
COMMENT ON COLUMN solicitudes_vacaciones.fecha_fin IS 'Fecha de fin de las vacaciones';
COMMENT ON COLUMN solicitudes_vacaciones.dias_solicitados IS 'Número de días solicitados (máximo 15 por solicitud)';
COMMENT ON COLUMN solicitudes_vacaciones.gestion IS 'Año/gestión de las vacaciones';
COMMENT ON COLUMN solicitudes_vacaciones.estado IS 'Estado: pendiente, aprobada, rechazada';
COMMENT ON COLUMN solicitudes_vacaciones.aprobado_por IS 'ID del administrador que aprobó/rechazó (referencia a usuarios.id)';

