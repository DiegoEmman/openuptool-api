-- Fix column names to match Entity Framework expectations (PascalCase)

-- Check if columns need to be added/renamed
DO $$ 
BEGIN
    -- Add ParentTemplateId if parent_template_id doesn't exist
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'global_configurations' 
        AND column_name = 'parent_template_id'
    ) THEN
        ALTER TABLE global_configurations 
        ADD COLUMN parent_template_id UUID;
    END IF;

    -- Add Tags column if it doesn't exist
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'global_configurations' 
        AND column_name = 'tags'
    ) THEN
        ALTER TABLE global_configurations 
        ADD COLUMN tags TEXT;
    END IF;

    -- Drop old columns if they exist (in case of naming mismatch)
    IF EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'global_configurations' 
        AND column_name = 'ParentTemplateId'
    ) THEN
        ALTER TABLE global_configurations DROP COLUMN "ParentTemplateId";
    END IF;

    IF EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'global_configurations' 
        AND column_name = 'Tags'
    ) THEN
        ALTER TABLE global_configurations DROP COLUMN "Tags";
    END IF;
END $$;
