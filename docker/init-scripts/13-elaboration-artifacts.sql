-- ==============================================================================
-- MIGRATION: Agregar soporte para archivos adjuntos en artefactos
-- ==============================================================================

-- Agregar nuevas columnas a la tabla artifacts para soportar archivos
ALTER TABLE artifacts
ADD COLUMN IF NOT EXISTS file_path VARCHAR(500),
ADD COLUMN IF NOT EXISTS file_name VARCHAR(255),
ADD COLUMN IF NOT EXISTS file_size BIGINT,
ADD COLUMN IF NOT EXISTS mime_type VARCHAR(100),
ADD COLUMN IF NOT EXISTS file_category VARCHAR(50);

-- Crear índice para búsquedas por categoría de archivo
CREATE INDEX IF NOT EXISTS idx_artifacts_file_category ON artifacts(file_category);

-- Comentarios para documentación
COMMENT ON COLUMN artifacts.file_path IS 'Ruta relativa del archivo desde la carpeta uploads';
COMMENT ON COLUMN artifacts.file_name IS 'Nombre original del archivo subido';
COMMENT ON COLUMN artifacts.file_size IS 'Tamaño del archivo en bytes';
COMMENT ON COLUMN artifacts.mime_type IS 'Tipo MIME del archivo (image/png, application/pdf, etc.)';
COMMENT ON COLUMN artifacts.file_category IS 'Categoría del archivo: DIAGRAM, PROTOTYPE, DOCUMENT';

-- ==============================================================================
-- SEED DATA: Artifact Types para Elaboration
-- ==============================================================================
INSERT INTO artifact_types (id, phase, code, name, description, is_mandatory, default_format, created_at, updated_at)
VALUES
    (gen_random_uuid(), 'ELABORATION', 'DETAILED_USE_CASES', 'Modelo de Casos de Uso Detallado', 'Especificación completa de casos de uso con flujos principales y alternativos.', TRUE, 'TEXT', NOW(), NOW()),
    (gen_random_uuid(), 'ELABORATION', 'DOMAIN_MODEL', 'Modelo de Dominio', 'Diagrama conceptual del dominio del problema.', TRUE, 'FILE', NOW(), NOW()),
    (gen_random_uuid(), 'ELABORATION', 'SUPPLEMENTARY_SPEC', 'Especificación de Requerimientos Suplementarios', 'Requerimientos que no se capturan en casos de uso.', TRUE, 'TEXT', NOW(), NOW()),
    (gen_random_uuid(), 'ELABORATION', 'NON_FUNCTIONAL_REQ', 'Requerimientos No Funcionales', 'Especificación de rendimiento, seguridad, usabilidad, etc.', TRUE, 'TEXT', NOW(), NOW()),
    (gen_random_uuid(), 'ELABORATION', 'ARCHITECTURE_DOC', 'Documento de Arquitectura', 'Descripción de la arquitectura del sistema.', TRUE, 'MIXED', NOW(), NOW()),
    (gen_random_uuid(), 'ELABORATION', 'TECHNICAL_DIAGRAMS', 'Diagramas Técnicos', 'Diagramas de componentes, despliegue, secuencia, etc.', FALSE, 'FILE', NOW(), NOW()),
    (gen_random_uuid(), 'ELABORATION', 'ITERATION_PLAN', 'Plan de Iteraciones', 'Planificación detallada de iteraciones de la fase.', TRUE, 'TEXT', NOW(), NOW()),
    (gen_random_uuid(), 'ELABORATION', 'UI_PROTOTYPE', 'Prototipo de UI', 'Mockups o prototipos de interfaz de usuario.', FALSE, 'FILE', NOW(), NOW())
ON CONFLICT (phase, code) DO NOTHING;

-- ==============================================================================
-- MENSAJE DE CONFIRMACIÓN
-- ==============================================================================
DO $$ 
BEGIN 
    RAISE NOTICE 'Elaboration artifacts migration completed successfully!';
    RAISE NOTICE '- Added file support columns to artifacts table';
    RAISE NOTICE '- Added 8 new artifact types for ELABORATION phase';
END $$;
