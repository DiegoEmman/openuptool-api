-- Crear tablas para IterationTask e IterationProgress

-- Tabla: iteration_tasks
CREATE TABLE IF NOT EXISTS iteration_tasks (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    iteration_id UUID NOT NULL REFERENCES iterations(id) ON DELETE CASCADE,
    name VARCHAR(255) NOT NULL,
    description TEXT,
    status VARCHAR(50) NOT NULL DEFAULT 'pending',
    estimated_hours DECIMAL(10,2),
    actual_hours DECIMAL(10,2),
    assigned_to UUID REFERENCES users(id) ON DELETE SET NULL,
    start_date TIMESTAMP,
    end_date TIMESTAMP,
    priority INTEGER NOT NULL DEFAULT 3,
    blocker_description TEXT,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Índices para iteration_tasks
CREATE INDEX idx_iteration_tasks_iteration_id ON iteration_tasks(iteration_id);
CREATE INDEX idx_iteration_tasks_assigned_to ON iteration_tasks(assigned_to);
CREATE INDEX idx_iteration_tasks_status ON iteration_tasks(status);

-- Tabla: iteration_progress
CREATE TABLE IF NOT EXISTS iteration_progress (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    iteration_id UUID NOT NULL REFERENCES iterations(id) ON DELETE CASCADE,
    record_date TIMESTAMP NOT NULL,
    completion_percentage DECIMAL(5,2) NOT NULL DEFAULT 0,
    total_tasks INTEGER NOT NULL DEFAULT 0,
    completed_tasks INTEGER NOT NULL DEFAULT 0,
    in_progress_tasks INTEGER NOT NULL DEFAULT 0,
    blocked_tasks INTEGER NOT NULL DEFAULT 0,
    blockers TEXT,
    observations TEXT,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Índices para iteration_progress
CREATE INDEX idx_iteration_progress_iteration_id ON iteration_progress(iteration_id);
CREATE INDEX idx_iteration_progress_record_date ON iteration_progress(record_date);

-- Comentarios para documentación
COMMENT ON TABLE iteration_tasks IS 'Tareas específicas de una iteración con seguimiento de progreso';
COMMENT ON TABLE iteration_progress IS 'Registro histórico del progreso de iteraciones';

COMMENT ON COLUMN iteration_tasks.status IS 'Estados: pending, in_progress, completed, blocked';
COMMENT ON COLUMN iteration_tasks.priority IS '1=Alta, 2=Media, 3=Baja';
COMMENT ON COLUMN iteration_progress.completion_percentage IS 'Porcentaje de completitud de la iteración (0-100)';
