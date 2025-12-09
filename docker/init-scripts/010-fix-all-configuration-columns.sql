-- ============================================================
-- FIX ALL MISSING COLUMNS FOR CONFIGURATION TEMPLATES
-- HU-018: Redefinir artefactos, flujos, roles y etapas
-- ============================================================

-- ARTIFACT_TYPE_TEMPLATES - Additional fields
ALTER TABLE artifact_type_templates ADD COLUMN IF NOT EXISTS category VARCHAR(100);
ALTER TABLE artifact_type_templates ADD COLUMN IF NOT EXISTS allows_versioning BOOLEAN NOT NULL DEFAULT true;
ALTER TABLE artifact_type_templates ADD COLUMN IF NOT EXISTS requires_approval BOOLEAN NOT NULL DEFAULT false;
ALTER TABLE artifact_type_templates ADD COLUMN IF NOT EXISTS is_system BOOLEAN NOT NULL DEFAULT false;

-- CUSTOM_FIELD_DEFINITIONS - Additional fields
-- (artifact_type_template_id, display_name, order_index already added)
ALTER TABLE custom_field_definitions ADD COLUMN IF NOT EXISTS validation_rules JSONB;

-- WORKFLOW_STATE_TEMPLATES - Fix column names and add missing fields
-- Rename state_name to name if needed
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'workflow_state_templates' AND column_name = 'state_name') THEN
        ALTER TABLE workflow_state_templates RENAME COLUMN state_name TO name;
    END IF;
END $$;

-- Rename is_final to is_final_state if needed
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'workflow_state_templates' AND column_name = 'is_final') THEN
        ALTER TABLE workflow_state_templates RENAME COLUMN is_final TO is_final_state;
    END IF;
END $$;

ALTER TABLE workflow_state_templates ADD COLUMN IF NOT EXISTS is_initial_state BOOLEAN NOT NULL DEFAULT false;
ALTER TABLE workflow_state_templates ADD COLUMN IF NOT EXISTS color VARCHAR(20) DEFAULT '#808080';

-- CONFIGURATION_CHANGE_HISTORY - Fix column names
-- Rename change_date to changed_at if needed
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'configuration_change_history' AND column_name = 'change_date') THEN
        ALTER TABLE configuration_change_history RENAME COLUMN change_date TO changed_at;
    END IF;
END $$;

-- Add indexes for performance
CREATE INDEX IF NOT EXISTS idx_custom_field_definitions_artifact_type ON custom_field_definitions(artifact_type_template_id) WHERE artifact_type_template_id IS NOT NULL;
CREATE INDEX IF NOT EXISTS idx_artifact_type_templates_category ON artifact_type_templates(category) WHERE category IS NOT NULL;
CREATE INDEX IF NOT EXISTS idx_workflow_state_templates_initial ON workflow_state_templates(workflow_template_id, is_initial) WHERE is_initial = true;

-- Update existing OpenUP default configuration if it exists
UPDATE artifact_type_templates SET 
    category = 'Process', 
    allows_versioning = true, 
    requires_approval = false,
    is_system = true
WHERE configuration_id = 'a0000000-0000-0000-0000-000000000001';

-- Set default colors for existing workflow states
UPDATE workflow_state_templates SET color = '#808080' WHERE color IS NULL OR color = '';
UPDATE workflow_state_templates SET is_initial_state = true WHERE state_code IN ('DRAFT', 'NUEVO', 'PENDIENTE', 'BORRADOR', 'DEVELOPMENT') AND is_initial_state = false;

COMMIT;
