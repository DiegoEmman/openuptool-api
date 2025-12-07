-- Agregar columnas de archivo y auditoría a la tabla projects
ALTER TABLE projects
ADD COLUMN IF NOT EXISTS is_archived BOOLEAN DEFAULT FALSE,
ADD COLUMN IF NOT EXISTS archived_at TIMESTAMP,
ADD COLUMN IF NOT EXISTS archived_by UUID REFERENCES users(id);

-- Crear tabla de auditoría
CREATE TABLE IF NOT EXISTS audit_logs (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id),
    action VARCHAR(100) NOT NULL,
    entity_type VARCHAR(50) NOT NULL,
    entity_id UUID,
    details TEXT,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Crear índices para mejor rendimiento
CREATE INDEX IF NOT EXISTS idx_audit_logs_user_id ON audit_logs(user_id);
CREATE INDEX IF NOT EXISTS idx_audit_logs_entity ON audit_logs(entity_type, entity_id);
CREATE INDEX IF NOT EXISTS idx_audit_logs_created_at ON audit_logs(created_at DESC);
CREATE INDEX IF NOT EXISTS idx_projects_archived ON projects(is_archived);

-- Comentarios
COMMENT ON TABLE audit_logs IS 'Registro de auditoría de acciones del sistema';
COMMENT ON COLUMN projects.is_archived IS 'Indica si el proyecto está archivado';
COMMENT ON COLUMN projects.archived_at IS 'Fecha en que se archivó el proyecto';
COMMENT ON COLUMN projects.archived_by IS 'Usuario que archivó el proyecto';
