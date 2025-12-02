-- Agregar columnas para repositorio
ALTER TABLE artifacts ADD COLUMN IF NOT EXISTS repository_url VARCHAR(500);
ALTER TABLE artifacts ADD COLUMN IF NOT EXISTS repository_version VARCHAR(100);
ALTER TABLE artifacts ADD COLUMN IF NOT EXISTS build_number VARCHAR(50);
ALTER TABLE artifacts ADD COLUMN IF NOT EXISTS test_data TEXT;
ALTER TABLE artifacts ADD COLUMN IF NOT EXISTS iteration_data TEXT;

-- Crear índices
CREATE INDEX IF NOT EXISTS idx_artifacts_repository_url ON artifacts(repository_url);
CREATE INDEX IF NOT EXISTS idx_artifacts_build_number ON artifacts(build_number);

-- Insertar tipos de artefactos de Construction
INSERT INTO artifact_types (id, phase, code, name, description, is_mandatory, default_format, created_at, updated_at)
VALUES
    (gen_random_uuid(), 'CONSTRUCTION', 'DETAILED_DESIGN_MODEL', 'Modelo de Diseño Detallado', 'Diseño completo de clases, componentes y estructura del sistema.', TRUE, 'MIXED', NOW(), NOW()),
    (gen_random_uuid(), 'CONSTRUCTION', 'SOURCE_CODE', 'Código Fuente', 'Código fuente o enlace al repositorio con versión/build.', TRUE, 'MIXED', NOW(), NOW()),
    (gen_random_uuid(), 'CONSTRUCTION', 'TEST_CASES', 'Casos de Prueba', 'Definición de casos de prueba con ID, pasos y criterios de aceptación.', TRUE, 'MIXED', NOW(), NOW()),
    (gen_random_uuid(), 'CONSTRUCTION', 'TEST_RESULTS', 'Resultados de Pruebas', 'Registro de ejecución de pruebas con resultados y defectos encontrados.', TRUE, 'MIXED', NOW(), NOW()),
    (gen_random_uuid(), 'CONSTRUCTION', 'ITERATION_LOG', 'Registro de Iteraciones', 'Actividades realizadas, decisiones tomadas y comentarios de cada iteración.', TRUE, 'MIXED', NOW(), NOW())
ON CONFLICT (phase, code) DO NOTHING;

SELECT 'Migration completada exitosamente' as status;
