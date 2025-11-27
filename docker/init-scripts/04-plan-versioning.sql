-- Add is_active column to project_plans for version tracking
ALTER TABLE project_plans ADD COLUMN IF NOT EXISTS is_active BOOLEAN NOT NULL DEFAULT true;

-- Create index for faster queries on active plans
CREATE INDEX IF NOT EXISTS idx_project_plans_project_active ON project_plans(project_id, is_active);

-- Comment
COMMENT ON COLUMN project_plans.is_active IS 'Indicates if this is the currently active plan version';
