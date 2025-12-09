-- =========================================================================
-- HU-019: Templates OpenUP - Additional fields for template versioning
-- =========================================================================
-- This migration adds fields required for:
-- - Tags for template categorization
-- - ParentTemplateId for tracking cloned templates
-- =========================================================================

-- Add Tags column to global_configurations if it doesn't exist
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'global_configurations' 
        AND column_name = 'tags'
    ) THEN
        ALTER TABLE global_configurations ADD COLUMN tags TEXT;
        COMMENT ON COLUMN global_configurations.tags IS 'Comma-separated tags for template categorization (HU-019)';
    END IF;
END $$;

-- Add ParentTemplateId column to global_configurations if it doesn't exist
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'global_configurations' 
        AND column_name = 'parent_template_id'
    ) THEN
        ALTER TABLE global_configurations ADD COLUMN parent_template_id UUID REFERENCES global_configurations(id) ON DELETE SET NULL;
        COMMENT ON COLUMN global_configurations.parent_template_id IS 'Reference to original template if this was cloned (HU-019)';
    END IF;
END $$;

-- Create index for tags search
CREATE INDEX IF NOT EXISTS idx_global_configurations_tags ON global_configurations(tags);

-- Create index for parent template lookup
CREATE INDEX IF NOT EXISTS idx_global_configurations_parent_template ON global_configurations(parent_template_id);

-- Add some sample tags to the default configuration
UPDATE global_configurations 
SET tags = 'default,openup,standard'
WHERE name = 'OpenUP Default Configuration' AND is_default = true;

RAISE NOTICE 'HU-019: Template fields (tags, parent_template_id) added to global_configurations table';
