-- Script de inicialización de la base de datos OpenUpTool
-- Este script se ejecuta automáticamente al iniciar el contenedor PostgreSQL

-- Crear extensiones útiles
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- Configuraciones adicionales
ALTER DATABASE openuptool SET timezone TO 'UTC';

-- ==============================================================================
-- TABLA: Projects
-- ==============================================================================
CREATE TABLE IF NOT EXISTS projects (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(255) NOT NULL,
    identifier VARCHAR(50) NOT NULL UNIQUE,
    start_date TIMESTAMP NOT NULL,
    status VARCHAR(50) NOT NULL DEFAULT 'Creado',
    owner VARCHAR(255),
    description TEXT,
    tags JSONB DEFAULT '[]'::jsonb,
    plan_id UUID,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_projects_identifier ON projects(identifier);
CREATE INDEX idx_projects_status ON projects(status);
CREATE INDEX idx_projects_created_at ON projects(created_at);

-- ==============================================================================
-- TABLA: Phases
-- ==============================================================================
CREATE TABLE IF NOT EXISTS phases (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    project_id UUID NOT NULL REFERENCES projects(id) ON DELETE CASCADE,
    phase_code VARCHAR(50) NOT NULL,
    name VARCHAR(255) NOT NULL,
    start_date TIMESTAMP,
    end_date TIMESTAMP,
    actual_start TIMESTAMP,
    actual_end TIMESTAMP,
    status VARCHAR(50) NOT NULL DEFAULT 'PENDING',
    order_index INTEGER NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW(),
    CONSTRAINT unique_project_phase UNIQUE (project_id, phase_code)
);

CREATE INDEX idx_phases_project_id ON phases(project_id);
CREATE INDEX idx_phases_phase_code ON phases(phase_code);
CREATE INDEX idx_phases_status ON phases(status);

-- ==============================================================================
-- TABLA: Project Plans
-- ==============================================================================
CREATE TABLE IF NOT EXISTS project_plans (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    project_id UUID NOT NULL UNIQUE REFERENCES projects(id) ON DELETE CASCADE,
    objectives TEXT NOT NULL,
    scope TEXT NOT NULL,
    initial_schedule JSONB NOT NULL DEFAULT '[]'::jsonb,
    version INTEGER NOT NULL DEFAULT 1,
    observations TEXT,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_project_plans_project_id ON project_plans(project_id);

-- ==============================================================================
-- TABLA: Milestones
-- ==============================================================================
CREATE TABLE IF NOT EXISTS milestones (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    plan_id UUID NOT NULL REFERENCES project_plans(id) ON DELETE CASCADE,
    name VARCHAR(255) NOT NULL,
    date TIMESTAMP NOT NULL,
    description TEXT,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_milestones_plan_id ON milestones(plan_id);
CREATE INDEX idx_milestones_date ON milestones(date);

-- ==============================================================================
-- TABLA: Iterations
-- ==============================================================================
CREATE TABLE IF NOT EXISTS iterations (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    project_id UUID NOT NULL REFERENCES projects(id) ON DELETE CASCADE,
    name VARCHAR(255) NOT NULL,
    objective TEXT,
    phase VARCHAR(50) NOT NULL,
    start_date TIMESTAMP NOT NULL,
    end_date TIMESTAMP NOT NULL,
    status VARCHAR(50) NOT NULL DEFAULT 'Planeada',
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_iterations_project_id ON iterations(project_id);
CREATE INDEX idx_iterations_phase ON iterations(phase);
CREATE INDEX idx_iterations_status ON iterations(status);

-- ==============================================================================
-- TABLA: Artifact Types
-- ==============================================================================
CREATE TABLE IF NOT EXISTS artifact_types (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    phase VARCHAR(50) NOT NULL,
    code VARCHAR(100) NOT NULL,
    name VARCHAR(255) NOT NULL,
    description TEXT NOT NULL,
    is_mandatory BOOLEAN NOT NULL DEFAULT FALSE,
    default_format VARCHAR(50) NOT NULL DEFAULT 'TEXT',
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW(),
    CONSTRAINT unique_phase_code UNIQUE (phase, code)
);

CREATE INDEX idx_artifact_types_phase ON artifact_types(phase);
CREATE INDEX idx_artifact_types_code ON artifact_types(code);

-- ==============================================================================
-- TABLA: Artifacts
-- ==============================================================================
CREATE TABLE IF NOT EXISTS artifacts (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    project_id UUID NOT NULL REFERENCES projects(id) ON DELETE CASCADE,
    phase_id VARCHAR(50) NOT NULL,
    artifact_type_id UUID NOT NULL REFERENCES artifact_types(id) ON DELETE RESTRICT,
    title VARCHAR(255) NOT NULL,
    description TEXT,
    author VARCHAR(255),
    status VARCHAR(50) NOT NULL DEFAULT 'Pendiente',
    is_mandatory BOOLEAN NOT NULL DEFAULT FALSE,
    content_text TEXT,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_artifacts_project_id ON artifacts(project_id);
CREATE INDEX idx_artifacts_phase_id ON artifacts(phase_id);
CREATE INDEX idx_artifacts_artifact_type_id ON artifacts(artifact_type_id);
CREATE INDEX idx_artifacts_status ON artifacts(status);

-- ==============================================================================
-- SEED DATA: Artifact Types para Inception
-- ==============================================================================
INSERT INTO artifact_types (phase, code, name, description, is_mandatory, default_format)
VALUES
    ('INCEPTION', 'VISION_DOC', 'Documento de Visión', 'Define la visión del producto.', TRUE, 'TEXT'),
    ('INCEPTION', 'STAKEHOLDERS', 'Lista de Stakeholders', 'Identifica actores clave.', TRUE, 'TEXT'),
    ('INCEPTION', 'INITIAL_RISKS', 'Lista de Riesgos Iniciales', 'Riesgos tempranos.', TRUE, 'TEXT'),
    ('INCEPTION', 'PROJECT_PLAN_V1', 'Plan de Proyecto (v1)', 'Versión inicial del plan.', TRUE, 'TEXT'),
    ('INCEPTION', 'HL_USE_CASES', 'Modelo Casos de Uso Alto Nivel', 'Casos de uso principales.', FALSE, 'TEXT')
ON CONFLICT (phase, code) DO NOTHING;

-- ==============================================================================
-- FUNCIONES: Actualización automática de updated_at
-- ==============================================================================
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Triggers para actualizar updated_at
CREATE TRIGGER update_projects_updated_at BEFORE UPDATE ON projects
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_phases_updated_at BEFORE UPDATE ON phases
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_project_plans_updated_at BEFORE UPDATE ON project_plans
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_milestones_updated_at BEFORE UPDATE ON milestones
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_iterations_updated_at BEFORE UPDATE ON iterations
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_artifact_types_updated_at BEFORE UPDATE ON artifact_types
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_artifacts_updated_at BEFORE UPDATE ON artifacts
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

-- ==============================================================================
-- MENSAJE DE CONFIRMACIÓN
-- ==============================================================================
DO $$ 
BEGIN 
    RAISE NOTICE 'OpenUpTool Database initialized successfully!';
    RAISE NOTICE 'Tables created: projects, phases, project_plans, milestones, iterations, artifact_types, artifacts';
    RAISE NOTICE 'Default artifact types for INCEPTION phase seeded.';
END $$;
