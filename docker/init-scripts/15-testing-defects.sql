-- 15-testing-defects.sql
-- Testing and Defect Tracking System
-- Implements test execution results and defect management for quality assurance

-- Test Executions Table
-- Tracks individual test case execution results with evidence
CREATE TABLE IF NOT EXISTS test_executions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    artifact_id UUID NOT NULL REFERENCES artifacts(id) ON DELETE CASCADE,
    test_case_id VARCHAR(100) NOT NULL,
    test_case_name VARCHAR(500) NOT NULL,
    result VARCHAR(50) NOT NULL DEFAULT 'Pending',
    executed_by UUID NOT NULL REFERENCES users(id),
    executed_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    duration_seconds INTEGER,
    evidence TEXT,
    notes TEXT,
    artifact_version_id UUID REFERENCES artifact_versions(id) ON DELETE SET NULL,
    environment VARCHAR(100),
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT test_executions_result_check CHECK (result IN ('Pending', 'Passed', 'Failed', 'Blocked', 'Skipped'))
);

-- Indexes for test_executions
CREATE INDEX IF NOT EXISTS idx_test_executions_artifact_id ON test_executions(artifact_id);
CREATE INDEX IF NOT EXISTS idx_test_executions_artifact_testcase ON test_executions(artifact_id, test_case_id);
CREATE INDEX IF NOT EXISTS idx_test_executions_executed_at ON test_executions(executed_at DESC);
CREATE INDEX IF NOT EXISTS idx_test_executions_result ON test_executions(result);

-- Defects Table
-- Comprehensive defect tracking with lifecycle management
CREATE TABLE IF NOT EXISTS defects (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    defect_number VARCHAR(50) NOT NULL,
    title VARCHAR(500) NOT NULL,
    description TEXT,
    severity VARCHAR(50) NOT NULL DEFAULT 'Medium',
    status VARCHAR(50) NOT NULL DEFAULT 'Open',
    priority VARCHAR(50),
    type VARCHAR(50),
    project_id UUID NOT NULL REFERENCES projects(id) ON DELETE CASCADE,
    artifact_id UUID REFERENCES artifacts(id) ON DELETE SET NULL,
    artifact_version_id UUID REFERENCES artifact_versions(id) ON DELETE SET NULL,
    test_execution_id UUID REFERENCES test_executions(id) ON DELETE SET NULL,
    reported_by UUID NOT NULL REFERENCES users(id),
    reported_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    assigned_to UUID REFERENCES users(id),
    assigned_at TIMESTAMP,
    resolved_at TIMESTAMP,
    resolved_by UUID REFERENCES users(id),
    resolution TEXT,
    steps_to_reproduce TEXT,
    expected_result TEXT,
    actual_result TEXT,
    environment VARCHAR(100),
    tags TEXT,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT defects_severity_check CHECK (severity IN ('Critical', 'High', 'Medium', 'Low')),
    CONSTRAINT defects_status_check CHECK (status IN ('Open', 'InProgress', 'Resolved', 'Closed', 'Reopened')),
    CONSTRAINT defects_priority_check CHECK (priority IS NULL OR priority IN ('Critical', 'High', 'Medium', 'Low')),
    CONSTRAINT defects_type_check CHECK (type IS NULL OR type IN ('Bug', 'Regression', 'Performance', 'Security', 'UI', 'Other')),
    CONSTRAINT defects_unique_number UNIQUE (project_id, defect_number)
);

-- Indexes for defects
CREATE INDEX IF NOT EXISTS idx_defects_project_id ON defects(project_id);
CREATE INDEX IF NOT EXISTS idx_defects_artifact_id ON defects(artifact_id);
CREATE INDEX IF NOT EXISTS idx_defects_test_execution_id ON defects(test_execution_id);
CREATE INDEX IF NOT EXISTS idx_defects_status ON defects(status);
CREATE INDEX IF NOT EXISTS idx_defects_assigned_to ON defects(assigned_to);
CREATE INDEX IF NOT EXISTS idx_defects_reported_at ON defects(reported_at DESC);
CREATE INDEX IF NOT EXISTS idx_defects_defect_number ON defects(project_id, defect_number);

-- Comments
COMMENT ON TABLE test_executions IS 'Stores test case execution results with evidence and metrics';
COMMENT ON TABLE defects IS 'Tracks defects/bugs with comprehensive lifecycle and assignment workflow';
COMMENT ON COLUMN test_executions.evidence IS 'JSON array of evidence items (screenshots, logs, etc.)';
COMMENT ON COLUMN test_executions.result IS 'Test execution result: Pending, Passed, Failed, Blocked, or Skipped';
COMMENT ON COLUMN defects.defect_number IS 'Human-readable sequential ID (e.g., DEF-001)';
COMMENT ON COLUMN defects.tags IS 'JSON array of tags for categorization';

