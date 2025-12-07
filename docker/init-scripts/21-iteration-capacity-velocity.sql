-- HU-016: Control de capacidad y velocidad
-- Agregar campos para tracking de capacidad del equipo y velocidad

-- Agregar columnas a la tabla iterations
ALTER TABLE iterations 
ADD COLUMN IF NOT EXISTS planned_capacity_hours DECIMAL(10,2) DEFAULT NULL,
ADD COLUMN IF NOT EXISTS team_size INTEGER DEFAULT NULL,
ADD COLUMN IF NOT EXISTS planned_points INTEGER DEFAULT NULL,
ADD COLUMN IF NOT EXISTS completed_points INTEGER DEFAULT NULL;

-- Comentarios descriptivos
COMMENT ON COLUMN iterations.planned_capacity_hours IS 'Horas totales de capacidad planificada del equipo para esta iteración';
COMMENT ON COLUMN iterations.team_size IS 'Número de miembros del equipo asignados a esta iteración';
COMMENT ON COLUMN iterations.planned_points IS 'Puntos de historia planificados para completar en esta iteración';
COMMENT ON COLUMN iterations.completed_points IS 'Puntos de historia realmente completados al final de la iteración';

-- Agregar datos de ejemplo para iteraciones existentes (opcional - para testing)
-- UPDATE iterations SET team_size = 4, planned_capacity_hours = 160 WHERE team_size IS NULL;
