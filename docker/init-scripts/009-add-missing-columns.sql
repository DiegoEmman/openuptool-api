-- Add missing columns to configuration tables

-- Add is_system to role_templates
ALTER TABLE role_templates ADD COLUMN IF NOT EXISTS is_system BOOLEAN NOT NULL DEFAULT false;

-- Add missing columns to phase_templates
ALTER TABLE phase_templates ADD COLUMN IF NOT EXISTS default_duration_days INTEGER;
ALTER TABLE phase_templates ADD COLUMN IF NOT EXISTS is_mandatory BOOLEAN NOT NULL DEFAULT true;

-- Rename code to phase_code in phase_templates if needed
DO $$ 
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'phase_templates' 
        AND column_name = 'code'
    ) AND NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'phase_templates' 
        AND column_name = 'phase_code'
    ) THEN
        ALTER TABLE phase_templates RENAME COLUMN code TO phase_code;
    END IF;
END $$;

-- Add phase_code if it doesn't exist
ALTER TABLE phase_templates ADD COLUMN IF NOT EXISTS phase_code VARCHAR(50);

-- Update phase_templates to use phase_code consistently
UPDATE phase_templates SET phase_code = code WHERE phase_code IS NULL AND code IS NOT NULL;

-- Rename is_required to is_mandatory in artifact_type_templates if needed
DO $$ 
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'artifact_type_templates' 
        AND column_name = 'is_required'
    ) AND NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'artifact_type_templates' 
        AND column_name = 'is_mandatory'
    ) THEN
        ALTER TABLE artifact_type_templates RENAME COLUMN is_required TO is_mandatory;
    END IF;
END $$;

-- Add default_format to artifact_type_templates
ALTER TABLE artifact_type_templates ADD COLUMN IF NOT EXISTS default_format VARCHAR(20) DEFAULT 'TEXT';

-- Add version to configuration_change_history
ALTER TABLE configuration_change_history ADD COLUMN IF NOT EXISTS version INTEGER NOT NULL DEFAULT 1;

-- Rename change_date to changed_at if needed
DO $$ 
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'configuration_change_history' 
        AND column_name = 'change_date'
    ) AND NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'configuration_change_history' 
        AND column_name = 'changed_at'
    ) THEN
        ALTER TABLE configuration_change_history RENAME COLUMN change_date TO changed_at;
    END IF;
END $$;

-- Add missing columns to configuration_change_history
ALTER TABLE configuration_change_history ADD COLUMN IF NOT EXISTS from_version INTEGER;
ALTER TABLE configuration_change_history ADD COLUMN IF NOT EXISTS to_version INTEGER;
ALTER TABLE configuration_change_history ADD COLUMN IF NOT EXISTS entity_type VARCHAR(50);
ALTER TABLE configuration_change_history ADD COLUMN IF NOT EXISTS entity_id UUID;
ALTER TABLE configuration_change_history ADD COLUMN IF NOT EXISTS entity_name VARCHAR(255);

-- Update version column from change data if needed
UPDATE configuration_change_history 
SET from_version = version - 1, to_version = version 
WHERE from_version IS NULL AND to_version IS NULL AND version > 1;
