-- ==============================================================================
-- Migration: 14-construction-artifacts
-- Descripción: Agrega soporte para artefactos de la fase de Construcción
--              con campos estructurados para código fuente, pruebas e iteraciones
-- Fecha: 2025-12-02
-- ==============================================================================

-- Agregar columnas para repositorio de código fuente
ALTER TABLE artifacts ADD COLUMN IF NOT EXISTS repository_url VARCHAR(500);
ALTER TABLE artifacts ADD COLUMN IF NOT EXISTS repository_version VARCHAR(100);
ALTER TABLE artifacts ADD COLUMN IF NOT EXISTS build_number VARCHAR(50);

-- Agregar columnas para datos estructurados (JSON)
ALTER TABLE artifacts ADD COLUMN IF NOT EXISTS test_data TEXT;
ALTER TABLE artifacts ADD COLUMN IF NOT EXISTS iteration_data TEXT;

-- Comentarios para documentación
COMMENT ON COLUMN artifacts.repository_url IS 'URL del repositorio externo (GitHub, GitLab, etc.)';
COMMENT ON COLUMN artifacts.repository_version IS 'Versión, tag, commit o branch del repositorio';
COMMENT ON COLUMN artifacts.build_number IS 'Número de build asociado al artefacto';
COMMENT ON COLUMN artifacts.test_data IS 'Datos estructurados de casos de prueba y resultados en formato JSON';
COMMENT ON COLUMN artifacts.iteration_data IS 'Actividades y comentarios de iteraciones en formato JSON';

-- Índices para mejorar rendimiento en búsquedas
CREATE INDEX IF NOT EXISTS idx_artifacts_repository_url ON artifacts(repository_url);
CREATE INDEX IF NOT EXISTS idx_artifacts_build_number ON artifacts(build_number);

-- ==============================================================================
-- SEED DATA: Artifact Types para Construction
-- ==============================================================================
INSERT INTO artifact_types (id, phase, code, name, description, is_mandatory, default_format)
VALUES
    (gen_random_uuid(), 'CONSTRUCTION', 'DETAILED_DESIGN_MODEL', 'Modelo de Diseño Detallado', 'Diseño completo de clases, componentes y estructura del sistema.', TRUE, 'MIXED'),
    (gen_random_uuid(), 'CONSTRUCTION', 'SOURCE_CODE', 'Código Fuente', 'Código fuente o enlace al repositorio con versión/build.', TRUE, 'MIXED'),
    (gen_random_uuid(), 'CONSTRUCTION', 'TEST_CASES', 'Casos de Prueba', 'Definición de casos de prueba con ID, pasos y criterios de aceptación.', TRUE, 'MIXED'),
    (gen_random_uuid(), 'CONSTRUCTION', 'TEST_RESULTS', 'Resultados de Pruebas', 'Registro de ejecución de pruebas con resultados y defectos encontrados.', TRUE, 'MIXED'),
    (gen_random_uuid(), 'CONSTRUCTION', 'ITERATION_LOG', 'Registro de Iteraciones', 'Actividades realizadas, decisiones tomadas y comentarios de cada iteración.', TRUE, 'MIXED')
ON CONFLICT (phase, code) DO NOTHING;

-- ==============================================================================
-- CONFIRMACIÓN
-- ==============================================================================
DO $$ 
BEGIN 
    RAISE NOTICE '✅ Migration 14-construction-artifacts completada exitosamente';
    RAISE NOTICE '📋 Columnas agregadas: repository_url, repository_version, build_number, test_data, iteration_data';
    RAISE NOTICE '🏗️ 5 tipos de artefactos de CONSTRUCTION insertados';
END $$;
