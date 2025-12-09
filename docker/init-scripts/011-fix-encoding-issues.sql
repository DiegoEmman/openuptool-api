-- ============================================================
-- FIX ENCODING ISSUES IN CONFIGURATION DATA
-- HU-018: Corregir caracteres mal codificados
-- ============================================================

-- Fix workflow templates descriptions
UPDATE workflow_templates 
SET description = 'Flujo de revisión de documentos'
WHERE id = 'e0000000-0000-0000-0000-000000000001';

UPDATE workflow_templates 
SET description = 'Flujo de revisión de código'
WHERE id = 'e0000000-0000-0000-0000-000000000002';

-- Fix workflow state templates descriptions  
UPDATE workflow_state_templates 
SET description = 'En revisión'
WHERE id = 'f0000000-0000-0000-0000-000000000002';

UPDATE workflow_state_templates 
SET description = 'En revisión de código'
WHERE id = 'f0000000-0000-0000-0000-000000000006';

-- Fix artifact type templates descriptions
UPDATE artifact_type_templates
SET description = 'Documento de visión del proyecto'
WHERE id = 'd0000000-0000-0000-0000-000000000001';

UPDATE artifact_type_templates
SET description = 'Código fuente'
WHERE id = 'd0000000-0000-0000-0000-000000000004';

-- Fix phase templates descriptions
UPDATE phase_templates
SET description = 'Fase de concepción e inicio del proyecto'
WHERE phase_code = 'INCEPTION' AND configuration_id = 'a0000000-0000-0000-0000-000000000001';

UPDATE phase_templates
SET description = 'Fase de definición y elaboración de requisitos'
WHERE phase_code = 'ELABORATION' AND configuration_id = 'a0000000-0000-0000-0000-000000000001';

UPDATE phase_templates
SET description = 'Fase de construcción e implementación'
WHERE phase_code = 'CONSTRUCTION' AND configuration_id = 'a0000000-0000-0000-0000-000000000001';

UPDATE phase_templates
SET description = 'Fase de transición y despliegue'
WHERE phase_code = 'TRANSITION' AND configuration_id = 'a0000000-0000-0000-0000-000000000001';

-- Fix global configuration description
UPDATE global_configurations
SET description = 'Configuración estándar de OpenUP con roles, fases, tipos de artefactos y flujos predefinidos'
WHERE id = 'a0000000-0000-0000-0000-000000000001';

-- Fix configuration change history
UPDATE configuration_change_history
SET change_description = 'Configuración inicial de OpenUP creada con roles, fases, tipos de artefactos y flujos predefinidos'
WHERE configuration_id = 'a0000000-0000-0000-0000-000000000001' AND change_type = 'creation';

COMMIT;
