-- HU-018: Global Configuration Tables
-- This script creates the tables needed for global configuration management

-- Global Configurations table
CREATE TABLE IF NOT EXISTS global_configurations (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(255) NOT NULL,
    description VARCHAR(1000),
    version INTEGER NOT NULL DEFAULT 1,
    is_active BOOLEAN NOT NULL DEFAULT true,
    is_default BOOLEAN NOT NULL DEFAULT false,
    tags TEXT,
    parent_template_id UUID,
    created_at TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW(),
    created_by VARCHAR(255) NOT NULL
);

-- Role Templates table
CREATE TABLE IF NOT EXISTS role_templates (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    configuration_id UUID NOT NULL,
    name VARCHAR(255) NOT NULL,
    description VARCHAR(1000),
    permissions TEXT, -- JSON array
    order_index INTEGER NOT NULL,
    created_at TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW(),
    FOREIGN KEY (configuration_id) REFERENCES global_configurations(id) ON DELETE CASCADE
);

-- Phase Templates table
CREATE TABLE IF NOT EXISTS phase_templates (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    configuration_id UUID NOT NULL,
    code VARCHAR(50) NOT NULL,
    name VARCHAR(255) NOT NULL,
    description VARCHAR(1000),
    order_index INTEGER NOT NULL,
    created_at TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW(),
    FOREIGN KEY (configuration_id) REFERENCES global_configurations(id) ON DELETE CASCADE,
    UNIQUE (configuration_id, code)
);

-- Artifact Type Templates table
CREATE TABLE IF NOT EXISTS artifact_type_templates (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    configuration_id UUID NOT NULL,
    phase_code VARCHAR(50) NOT NULL,
    code VARCHAR(50) NOT NULL,
    name VARCHAR(255) NOT NULL,
    description VARCHAR(1000),
    is_required BOOLEAN NOT NULL DEFAULT false,
    order_index INTEGER NOT NULL,
    created_at TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW(),
    FOREIGN KEY (configuration_id) REFERENCES global_configurations(id) ON DELETE CASCADE,
    UNIQUE (configuration_id, code)
);

-- Workflow Templates table
CREATE TABLE IF NOT EXISTS workflow_templates (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    configuration_id UUID NOT NULL,
    name VARCHAR(255) NOT NULL,
    description VARCHAR(1000),
    applicable_phases TEXT, -- JSON array
    created_at TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW(),
    FOREIGN KEY (configuration_id) REFERENCES global_configurations(id) ON DELETE CASCADE
);

-- Workflow State Templates table
CREATE TABLE IF NOT EXISTS workflow_state_templates (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    workflow_template_id UUID NOT NULL,
    state_name VARCHAR(255) NOT NULL,
    state_code VARCHAR(50) NOT NULL,
    description VARCHAR(1000),
    order_index INTEGER NOT NULL,
    is_final BOOLEAN NOT NULL DEFAULT false,
    created_at TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW(),
    FOREIGN KEY (workflow_template_id) REFERENCES workflow_templates(id) ON DELETE CASCADE,
    UNIQUE (workflow_template_id, state_code)
);

-- Custom Field Definitions table
CREATE TABLE IF NOT EXISTS custom_field_definitions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    configuration_id UUID NOT NULL,
    field_name VARCHAR(255) NOT NULL,
    field_type VARCHAR(20) NOT NULL CHECK (field_type IN ('text', 'number', 'date', 'boolean', 'select')),
    default_value TEXT,
    options TEXT, -- JSON array for select type
    applicable_artifact_types TEXT, -- JSON array
    is_required BOOLEAN NOT NULL DEFAULT false,
    created_at TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW(),
    FOREIGN KEY (configuration_id) REFERENCES global_configurations(id) ON DELETE CASCADE
);

-- Configuration Change History table
CREATE TABLE IF NOT EXISTS configuration_change_history (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    configuration_id UUID NOT NULL,
    version INTEGER NOT NULL,
    change_type VARCHAR(50) NOT NULL,
    change_description VARCHAR(1000),
    old_value JSONB,
    new_value JSONB,
    changed_by VARCHAR(255) NOT NULL,
    change_date TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW(),
    FOREIGN KEY (configuration_id) REFERENCES global_configurations(id) ON DELETE CASCADE
);

-- Project Configurations table (links projects to configurations)
CREATE TABLE IF NOT EXISTS project_configurations (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    project_id UUID NOT NULL,
    configuration_id UUID NOT NULL,
    applied_at TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW(),
    applied_by VARCHAR(255) NOT NULL,
    configuration_snapshot JSONB, -- Full snapshot of configuration at time of application
    FOREIGN KEY (project_id) REFERENCES projects(id) ON DELETE CASCADE,
    FOREIGN KEY (configuration_id) REFERENCES global_configurations(id) ON DELETE RESTRICT,
    UNIQUE (project_id)
);

-- Artifact Custom Field Values table
CREATE TABLE IF NOT EXISTS artifact_custom_field_values (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    artifact_id UUID NOT NULL,
    field_definition_id UUID NOT NULL,
    field_value TEXT,
    created_at TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW(),
    FOREIGN KEY (artifact_id) REFERENCES artifacts(id) ON DELETE CASCADE,
    FOREIGN KEY (field_definition_id) REFERENCES custom_field_definitions(id) ON DELETE CASCADE,
    UNIQUE (artifact_id, field_definition_id)
);

-- Indexes for better performance
CREATE INDEX IF NOT EXISTS idx_global_configurations_is_default ON global_configurations(is_default) WHERE is_default = true;
CREATE INDEX IF NOT EXISTS idx_global_configurations_is_active ON global_configurations(is_active);
CREATE INDEX IF NOT EXISTS idx_role_templates_configuration_id ON role_templates(configuration_id);
CREATE INDEX IF NOT EXISTS idx_phase_templates_configuration_id ON phase_templates(configuration_id);
CREATE INDEX IF NOT EXISTS idx_artifact_type_templates_configuration_id ON artifact_type_templates(configuration_id);
CREATE INDEX IF NOT EXISTS idx_artifact_type_templates_phase_code ON artifact_type_templates(phase_code);
CREATE INDEX IF NOT EXISTS idx_workflow_templates_configuration_id ON workflow_templates(configuration_id);
CREATE INDEX IF NOT EXISTS idx_workflow_state_templates_workflow_id ON workflow_state_templates(workflow_template_id);
CREATE INDEX IF NOT EXISTS idx_custom_field_definitions_configuration_id ON custom_field_definitions(configuration_id);
CREATE INDEX IF NOT EXISTS idx_configuration_change_history_configuration_id ON configuration_change_history(configuration_id);
CREATE INDEX IF NOT EXISTS idx_configuration_change_history_version ON configuration_change_history(configuration_id, version);
CREATE INDEX IF NOT EXISTS idx_project_configurations_project_id ON project_configurations(project_id);
CREATE INDEX IF NOT EXISTS idx_artifact_custom_field_values_artifact_id ON artifact_custom_field_values(artifact_id);
CREATE INDEX IF NOT EXISTS idx_artifact_custom_field_values_field_definition_id ON artifact_custom_field_values(field_definition_id);

-- Insert a default OpenUP configuration
INSERT INTO global_configurations (id, name, description, version, is_active, is_default, created_by)
VALUES (
    'a0000000-0000-0000-0000-000000000001',
    'OpenUP Standard',
    'Configuración estándar de OpenUP con roles, fases, tipos de artefactos y flujos predefinidos',
    1,
    true,
    true,
    'System'
) ON CONFLICT DO NOTHING;

-- Insert default phases for OpenUP
INSERT INTO phase_templates (id, configuration_id, code, name, description, order_index) VALUES
('b0000000-0000-0000-0000-000000000001', 'a0000000-0000-0000-0000-000000000001', 'INCEPTION', 'Inicio', 'Fase de concepción e inicio del proyecto', 1),
('b0000000-0000-0000-0000-000000000002', 'a0000000-0000-0000-0000-000000000001', 'ELABORATION', 'Elaboración', 'Fase de definición y elaboración de requisitos', 2),
('b0000000-0000-0000-0000-000000000003', 'a0000000-0000-0000-0000-000000000001', 'CONSTRUCTION', 'Construcción', 'Fase de construcción e implementación', 3),
('b0000000-0000-0000-0000-000000000004', 'a0000000-0000-0000-0000-000000000001', 'TRANSITION', 'Transición', 'Fase de transición y despliegue', 4)
ON CONFLICT DO NOTHING;

-- Insert default roles for OpenUP
INSERT INTO role_templates (id, configuration_id, name, description, permissions, order_index) VALUES
('c0000000-0000-0000-0000-000000000001', 'a0000000-0000-0000-0000-000000000001', 'Project Manager', 'Líder del proyecto', '["manage_project", "manage_users", "view_all"]', 1),
('c0000000-0000-0000-0000-000000000002', 'a0000000-0000-0000-0000-000000000001', 'Developer', 'Desarrollador', '["create_artifacts", "edit_artifacts", "view_artifacts"]', 2),
('c0000000-0000-0000-0000-000000000003', 'a0000000-0000-0000-0000-000000000001', 'Analyst', 'Analista', '["create_artifacts", "edit_artifacts", "view_artifacts", "manage_requirements"]', 3),
('c0000000-0000-0000-0000-000000000004', 'a0000000-0000-0000-0000-000000000001', 'Tester', 'Tester', '["create_tests", "execute_tests", "view_artifacts", "create_defects"]', 4),
('c0000000-0000-0000-0000-000000000005', 'a0000000-0000-0000-0000-000000000001', 'Stakeholder', 'Interesado', '["view_artifacts", "comment"]', 5)
ON CONFLICT DO NOTHING;

-- Insert default artifact types for OpenUP
INSERT INTO artifact_type_templates (id, configuration_id, phase_code, code, name, description, is_required, order_index) VALUES
('d0000000-0000-0000-0000-000000000001', 'a0000000-0000-0000-0000-000000000001', 'INCEPTION', 'VISION_DOC', 'Vision Document', 'Documento de visión del proyecto', true, 1),
('d0000000-0000-0000-0000-000000000002', 'a0000000-0000-0000-0000-000000000001', 'ELABORATION', 'USE_CASE_MODEL', 'Use Case Model', 'Modelo de casos de uso', true, 2),
('d0000000-0000-0000-0000-000000000003', 'a0000000-0000-0000-0000-000000000001', 'ELABORATION', 'ARCHITECTURE_DOC', 'Architecture Document', 'Documento de arquitectura', true, 3),
('d0000000-0000-0000-0000-000000000004', 'a0000000-0000-0000-0000-000000000001', 'CONSTRUCTION', 'SOURCE_CODE', 'Source Code', 'Código fuente', true, 4),
('d0000000-0000-0000-0000-000000000005', 'a0000000-0000-0000-0000-000000000001', 'CONSTRUCTION', 'BUILD', 'Build', 'Build del sistema', true, 5),
('d0000000-0000-0000-0000-000000000006', 'a0000000-0000-0000-0000-000000000001', 'TRANSITION', 'DEPLOYMENT_PLAN', 'Deployment Plan', 'Plan de despliegue', true, 6),
('d0000000-0000-0000-0000-000000000007', 'a0000000-0000-0000-0000-000000000001', 'TRANSITION', 'USER_MANUAL', 'User Manual', 'Manual de usuario', true, 7)
ON CONFLICT DO NOTHING;

-- Insert default workflows
INSERT INTO workflow_templates (id, configuration_id, name, description, applicable_phases) VALUES
('e0000000-0000-0000-0000-000000000001', 'a0000000-0000-0000-0000-000000000001', 'Document Review Workflow', 'Flujo de revisión de documentos', '["INCEPTION", "ELABORATION", "TRANSITION"]'),
('e0000000-0000-0000-0000-000000000002', 'a0000000-0000-0000-0000-000000000001', 'Code Review Workflow', 'Flujo de revisión de código', '["CONSTRUCTION"]')
ON CONFLICT DO NOTHING;

-- Insert workflow states for Document Review
INSERT INTO workflow_state_templates (id, workflow_template_id, state_name, state_code, description, order_index, is_final) VALUES
('f0000000-0000-0000-0000-000000000001', 'e0000000-0000-0000-0000-000000000001', 'Draft', 'DRAFT', 'Borrador inicial', 1, false),
('f0000000-0000-0000-0000-000000000002', 'e0000000-0000-0000-0000-000000000001', 'In Review', 'IN_REVIEW', 'En revisión', 2, false),
('f0000000-0000-0000-0000-000000000003', 'e0000000-0000-0000-0000-000000000001', 'Approved', 'APPROVED', 'Aprobado', 3, true),
('f0000000-0000-0000-0000-000000000004', 'e0000000-0000-0000-0000-000000000001', 'Rejected', 'REJECTED', 'Rechazado', 4, true)
ON CONFLICT DO NOTHING;

-- Insert workflow states for Code Review
INSERT INTO workflow_state_templates (id, workflow_template_id, state_name, state_code, description, order_index, is_final) VALUES
('f0000000-0000-0000-0000-000000000005', 'e0000000-0000-0000-0000-000000000002', 'Development', 'DEVELOPMENT', 'En desarrollo', 1, false),
('f0000000-0000-0000-0000-000000000006', 'e0000000-0000-0000-0000-000000000002', 'Code Review', 'CODE_REVIEW', 'En revisión de código', 2, false),
('f0000000-0000-0000-0000-000000000007', 'e0000000-0000-0000-0000-000000000002', 'Testing', 'TESTING', 'En pruebas', 3, false),
('f0000000-0000-0000-0000-000000000008', 'e0000000-0000-0000-0000-000000000002', 'Merged', 'MERGED', 'Integrado', 4, true)
ON CONFLICT DO NOTHING;

-- Log initial configuration creation
INSERT INTO configuration_change_history (configuration_id, version, change_type, change_description, changed_by) VALUES
('a0000000-0000-0000-0000-000000000001', 1, 'creation', 'Configuración inicial de OpenUP creada con roles, fases, tipos de artefactos y flujos predefinidos', 'System')
ON CONFLICT DO NOTHING;

-- Grant permissions (if needed)
-- GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO your_user;
