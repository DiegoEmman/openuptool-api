-- =============================================
-- Script: 18-microincrements.sql
-- Descripción: Crear tabla de microincrementos
-- Fecha: 2025-12-07
-- =============================================

-- Tabla de microincrementos
CREATE TABLE microincrements (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    title VARCHAR(500) NOT NULL,
    description TEXT,
    date TIMESTAMP NOT NULL,
    author VARCHAR(255) NOT NULL,
    
    -- Relaciones
    iteration_id UUID REFERENCES iterations(id) ON DELETE SET NULL,
    artifact_id UUID NOT NULL REFERENCES artifacts(id) ON DELETE CASCADE,
    
    -- Metadata
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Índices para mejorar el rendimiento de las consultas
CREATE INDEX idx_microincrements_iteration_id ON microincrements(iteration_id);
CREATE INDEX idx_microincrements_artifact_id ON microincrements(artifact_id);
CREATE INDEX idx_microincrements_author ON microincrements(author);
CREATE INDEX idx_microincrements_date ON microincrements(date DESC);

-- Comentarios descriptivos
COMMENT ON TABLE microincrements IS 'Microincrementos vinculados a entregables e iteraciones';
COMMENT ON COLUMN microincrements.id IS 'Identificador único del microincremento';
COMMENT ON COLUMN microincrements.title IS 'Título del microincremento';
COMMENT ON COLUMN microincrements.description IS 'Descripción detallada del microincremento';
COMMENT ON COLUMN microincrements.date IS 'Fecha del microincremento';
COMMENT ON COLUMN microincrements.author IS 'Autor del microincremento';
COMMENT ON COLUMN microincrements.iteration_id IS 'ID de la iteración asociada (opcional)';
COMMENT ON COLUMN microincrements.artifact_id IS 'ID del entregable asociado (obligatorio)';
COMMENT ON COLUMN microincrements.created_at IS 'Fecha de creación del registro';
COMMENT ON COLUMN microincrements.updated_at IS 'Fecha de última actualización';

-- Datos de ejemplo (opcional, para testing)
-- Se pueden insertar después de que existan artifacts e iterations
