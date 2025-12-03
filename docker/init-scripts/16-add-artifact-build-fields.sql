-- Add build-related columns to artifacts table
-- These columns support CI/CD integration and repository linking

-- Add build_number column if it doesn't exist
DO $$ 
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'artifacts' AND column_name = 'build_number'
    ) THEN
        ALTER TABLE artifacts ADD COLUMN build_number VARCHAR(100);
        COMMENT ON COLUMN artifacts.build_number IS 'Build number from CI/CD system';
    END IF;
END $$;

-- Add repository_version column if it doesn't exist
DO $$ 
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'artifacts' AND column_name = 'repository_version'
    ) THEN
        ALTER TABLE artifacts ADD COLUMN repository_version VARCHAR(100);
        COMMENT ON COLUMN artifacts.repository_version IS 'Git commit hash or version tag';
    END IF;
END $$;

-- Add repository_url column if it doesn't exist
DO $$ 
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'artifacts' AND column_name = 'repository_url'
    ) THEN
        ALTER TABLE artifacts ADD COLUMN repository_url VARCHAR(500);
        COMMENT ON COLUMN artifacts.repository_url IS 'URL to source code repository';
    END IF;
END $$;

-- Create index for repository_url for faster lookups
CREATE INDEX IF NOT EXISTS idx_artifacts_repository_url ON artifacts(repository_url);

COMMENT ON TABLE artifacts IS 'Project artifacts with support for files, repositories, and build information';
