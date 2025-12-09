-- =============================================
-- HU-018: Redefinir artefactos, flujos, roles y etapas
-- Script de migración para configuración global
-- =============================================

-- Tabla principal de configuraciones globales
CREATE TABLE IF NOT EXISTS global_configurations (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(255) NOT NULL,
    description VARCHAR(1000),
    version INTEGER DEFAULT 1,
    is_active BOOLEAN DEFAULT TRUE,
    is_default BOOLEAN DEFAULT FALSE,
    created_by VARCHAR(255),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_global_configurations_name ON global_configurations(name);
CREATE INDEX IF NOT EXISTS idx_global_configurations_is_default ON global_configurations(is_default);

-- Tabla de templates de roles
CREATE TABLE IF NOT EXISTS role_templates (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    configuration_id UUID NOT NULL REFERENCES global_configurations(id) ON DELETE CASCADE,
    name VARCHAR(100) NOT NULL,
    description VARCHAR(500),
    permissions JSONB,
    order_index INTEGER DEFAULT 0,
    is_system BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_role_templates_configuration_id ON role_templates(configuration_id);

-- Tabla de templates de fases
CREATE TABLE IF NOT EXISTS phase_templates (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    configuration_id UUID NOT NULL REFERENCES global_configurations(id) ON DELETE CASCADE,
    phase_code VARCHAR(50) NOT NULL,
    name VARCHAR(100) NOT NULL,
    description VARCHAR(500),
    order_index INTEGER DEFAULT 0,
    default_duration_days INTEGER,
    is_mandatory BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_phase_templates_configuration_id ON phase_templates(configuration_id);

-- Tabla de templates de tipos de artefacto
CREATE TABLE IF NOT EXISTS artifact_type_templates (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    configuration_id UUID NOT NULL REFERENCES global_configurations(id) ON DELETE CASCADE,
    phase_code VARCHAR(50) NOT NULL,
    code VARCHAR(50) NOT NULL,
    name VARCHAR(255) NOT NULL,
    description VARCHAR(1000),
    is_mandatory BOOLEAN DEFAULT FALSE,
    default_format VARCHAR(20) DEFAULT 'TEXT',
    order_index INTEGER DEFAULT 0,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_artifact_type_templates_configuration_id ON artifact_type_templates(configuration_id);
CREATE INDEX IF NOT EXISTS idx_artifact_type_templates_phase_code ON artifact_type_templates(phase_code);

-- Tabla de templates de workflows
CREATE TABLE IF NOT EXISTS workflow_templates (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    configuration_id UUID NOT NULL REFERENCES global_configurations(id) ON DELETE CASCADE,
    name VARCHAR(255) NOT NULL,
    description VARCHAR(1000),
    is_default BOOLEAN DEFAULT FALSE,
    order_index INTEGER DEFAULT 0,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_workflow_templates_configuration_id ON workflow_templates(configuration_id);

-- Tabla de templates de estados de workflow
CREATE TABLE IF NOT EXISTS workflow_state_templates (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    workflow_template_id UUID NOT NULL REFERENCES workflow_templates(id) ON DELETE CASCADE,
    name VARCHAR(100) NOT NULL,
    description VARCHAR(500),
    order_index INTEGER DEFAULT 0,
    color VARCHAR(20),
    is_initial_state BOOLEAN DEFAULT FALSE,
    is_final_state BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_workflow_state_templates_workflow_template_id ON workflow_state_templates(workflow_template_id);

-- Tabla de definiciones de campos personalizados
CREATE TABLE IF NOT EXISTS custom_field_definitions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    artifact_type_template_id UUID NOT NULL REFERENCES artifact_type_templates(id) ON DELETE CASCADE,
    field_name VARCHAR(100) NOT NULL,
    display_name VARCHAR(255) NOT NULL,
    field_type VARCHAR(20) NOT NULL, -- TEXT, NUMBER, DATE, BOOLEAN, SELECT, MULTISELECT
    is_required BOOLEAN DEFAULT FALSE,
    default_value VARCHAR(1000),
    options JSONB, -- Para SELECT/MULTISELECT
    validation_rules JSONB,
    order_index INTEGER DEFAULT 0,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_custom_field_definitions_artifact_type_template_id ON custom_field_definitions(artifact_type_template_id);

-- Tabla de historial de cambios de configuración
CREATE TABLE IF NOT EXISTS configuration_change_history (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    configuration_id UUID NOT NULL REFERENCES global_configurations(id) ON DELETE CASCADE,
    from_version INTEGER NOT NULL,
    to_version INTEGER NOT NULL,
    change_type VARCHAR(50) NOT NULL, -- CREATE, UPDATE, DELETE, ROLLBACK, VERSION_INCREMENT
    entity_type VARCHAR(50) NOT NULL, -- CONFIGURATION, ROLE, PHASE, ARTIFACT_TYPE, WORKFLOW, WORKFLOW_STATE, CUSTOM_FIELD
    entity_id UUID,
    entity_name VARCHAR(255),
    old_value JSONB,
    new_value JSONB,
    changed_by VARCHAR(255) NOT NULL,
    change_description VARCHAR(1000),
    changed_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_configuration_change_history_configuration_id ON configuration_change_history(configuration_id);
CREATE INDEX IF NOT EXISTS idx_configuration_change_history_changed_at ON configuration_change_history(changed_at);

-- Tabla de configuración por proyecto
CREATE TABLE IF NOT EXISTS project_configurations (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    project_id UUID NOT NULL REFERENCES projects(id) ON DELETE CASCADE,
    configuration_id UUID NOT NULL REFERENCES global_configurations(id) ON DELETE RESTRICT,
    applied_version INTEGER NOT NULL,
    applied_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    applied_by VARCHAR(255),
    auto_update BOOLEAN DEFAULT FALSE,
    UNIQUE(project_id)
);

CREATE INDEX IF NOT EXISTS idx_project_configurations_project_id ON project_configurations(project_id);
CREATE INDEX IF NOT EXISTS idx_project_configurations_configuration_id ON project_configurations(configuration_id);

-- Tabla de valores de campos personalizados para artefactos
CREATE TABLE IF NOT EXISTS artifact_custom_field_values (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    artifact_id UUID NOT NULL REFERENCES artifacts(id) ON DELETE CASCADE,
    custom_field_definition_id UUID NOT NULL REFERENCES custom_field_definitions(id) ON DELETE CASCADE,
    value VARCHAR(5000),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(artifact_id, custom_field_definition_id)
);

CREATE INDEX IF NOT EXISTS idx_artifact_custom_field_values_artifact_id ON artifact_custom_field_values(artifact_id);

-- =============================================
-- Insertar configuración por defecto OpenUP
-- =============================================

DO $$
DECLARE
    v_config_id UUID;
    v_workflow_id UUID;
BEGIN
    -- Crear configuración por defecto si no existe
    IF NOT EXISTS (SELECT 1 FROM global_configurations WHERE is_default = TRUE) THEN
        v_config_id := gen_random_uuid();
        
        INSERT INTO global_configurations (id, name, description, version, is_active, is_default, created_by)
        VALUES (v_config_id, 'OpenUP Default', 'Configuración estándar de OpenUP', 1, TRUE, TRUE, 'system');

        -- Insertar roles por defecto
        INSERT INTO role_templates (configuration_id, name, description, order_index, is_system) VALUES
        (v_config_id, 'Admin', 'Administrador del sistema con acceso completo', 1, TRUE),
        (v_config_id, 'Manager', 'Gestor de proyecto con capacidad de planificación', 2, TRUE),
        (v_config_id, 'Developer', 'Desarrollador con acceso a artefactos y tareas', 3, TRUE),
        (v_config_id, 'Viewer', 'Usuario de solo lectura', 4, TRUE);

        -- Insertar fases por defecto
        INSERT INTO phase_templates (configuration_id, phase_code, name, description, order_index, default_duration_days, is_mandatory) VALUES
        (v_config_id, 'INCEPTION', 'Inception', 'Fase de inicio - Establecer alcance y visión del proyecto', 1, 14, TRUE),
        (v_config_id, 'ELABORATION', 'Elaboration', 'Fase de elaboración - Definir arquitectura y mitigar riesgos', 2, 30, TRUE),
        (v_config_id, 'CONSTRUCTION', 'Construction', 'Fase de construcción - Desarrollar el producto', 3, 60, TRUE),
        (v_config_id, 'TRANSITION', 'Transition', 'Fase de transición - Entregar el producto al usuario', 4, 21, TRUE);

        -- Insertar tipos de artefacto por defecto para INCEPTION
        INSERT INTO artifact_type_templates (configuration_id, phase_code, code, name, description, is_mandatory, default_format, order_index) VALUES
        (v_config_id, 'INCEPTION', 'VISION', 'Vision Document', 'Documento de visión del proyecto', TRUE, 'TEXT', 1),
        (v_config_id, 'INCEPTION', 'GLOSSARY', 'Glossary', 'Glosario de términos del proyecto', FALSE, 'TEXT', 2),
        (v_config_id, 'INCEPTION', 'USE_CASE', 'Use Case Model', 'Modelo de casos de uso', TRUE, 'MIXED', 3),
        (v_config_id, 'INCEPTION', 'RISK_LIST', 'Risk List', 'Lista de riesgos del proyecto', TRUE, 'TEXT', 4);

        -- Insertar tipos de artefacto por defecto para ELABORATION
        INSERT INTO artifact_type_templates (configuration_id, phase_code, code, name, description, is_mandatory, default_format, order_index) VALUES
        (v_config_id, 'ELABORATION', 'ARCHITECTURE', 'Software Architecture Document', 'Documento de arquitectura de software', TRUE, 'MIXED', 1),
        (v_config_id, 'ELABORATION', 'PROTOTYPE', 'Architectural Prototype', 'Prototipo arquitectónico ejecutable', FALSE, 'FILE', 2),
        (v_config_id, 'ELABORATION', 'DATA_MODEL', 'Data Model', 'Modelo de datos', TRUE, 'MIXED', 3),
        (v_config_id, 'ELABORATION', 'UC_SPEC', 'Use Case Specifications', 'Especificaciones detalladas de casos de uso', TRUE, 'TEXT', 4);

        -- Insertar tipos de artefacto por defecto para CONSTRUCTION
        INSERT INTO artifact_type_templates (configuration_id, phase_code, code, name, description, is_mandatory, default_format, order_index) VALUES
        (v_config_id, 'CONSTRUCTION', 'SOURCE_CODE', 'Source Code', 'Código fuente del proyecto', TRUE, 'FILE', 1),
        (v_config_id, 'CONSTRUCTION', 'UNIT_TESTS', 'Unit Tests', 'Pruebas unitarias', TRUE, 'FILE', 2),
        (v_config_id, 'CONSTRUCTION', 'INTEGRATION_BUILD', 'Integration Build', 'Build de integración', FALSE, 'FILE', 3),
        (v_config_id, 'CONSTRUCTION', 'DEV_DOCS', 'Developer Documentation', 'Documentación para desarrolladores', FALSE, 'TEXT', 4);

        -- Insertar tipos de artefacto por defecto para TRANSITION
        INSERT INTO artifact_type_templates (configuration_id, phase_code, code, name, description, is_mandatory, default_format, order_index) VALUES
        (v_config_id, 'TRANSITION', 'RELEASE_NOTES', 'Release Notes', 'Notas de versión', TRUE, 'TEXT', 1),
        (v_config_id, 'TRANSITION', 'USER_MANUAL', 'User Manual', 'Manual de usuario', TRUE, 'FILE', 2),
        (v_config_id, 'TRANSITION', 'DEPLOYMENT_GUIDE', 'Deployment Guide', 'Guía de despliegue', TRUE, 'TEXT', 3),
        (v_config_id, 'TRANSITION', 'TRAINING_MATERIALS', 'Training Materials', 'Materiales de capacitación', FALSE, 'FILE', 4);

        -- Insertar workflow por defecto
        v_workflow_id := gen_random_uuid();
        INSERT INTO workflow_templates (id, configuration_id, name, description, is_default, order_index)
        VALUES (v_workflow_id, v_config_id, 'Standard Workflow', 'Flujo de trabajo estándar para artefactos', TRUE, 1);

        -- Insertar estados del workflow
        INSERT INTO workflow_state_templates (workflow_template_id, name, description, order_index, color, is_initial_state, is_final_state) VALUES
        (v_workflow_id, 'Pendiente', 'Artefacto pendiente de inicio', 1, '#6B7280', TRUE, FALSE),
        (v_workflow_id, 'En Progreso', 'Artefacto en desarrollo activo', 2, '#3B82F6', FALSE, FALSE),
        (v_workflow_id, 'En Revisión', 'Artefacto en proceso de revisión', 3, '#F59E0B', FALSE, FALSE),
        (v_workflow_id, 'Aprobado', 'Artefacto aprobado', 4, '#10B981', FALSE, FALSE),
        (v_workflow_id, 'Completado', 'Artefacto finalizado', 5, '#059669', FALSE, TRUE);

        RAISE NOTICE 'Configuración OpenUP por defecto creada con ID: %', v_config_id;
    ELSE
        RAISE NOTICE 'Ya existe una configuración por defecto';
    END IF;
END $$;

-- Mensaje de finalización
DO $$
BEGIN
    RAISE NOTICE '========================================';
    RAISE NOTICE 'HU-018: Migración completada exitosamente';
    RAISE NOTICE '========================================';
END $$;
