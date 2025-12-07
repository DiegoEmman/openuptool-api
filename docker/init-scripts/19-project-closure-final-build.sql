-- Migration: Add ProjectClosure and FinalBuild tables
-- Description: Tablas para cierre de proyecto con checklist y registro de builds finales
-- Date: 2025-12-07

-- ProjectClosure Table
CREATE TABLE IF NOT EXISTS project_closures (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    project_id UUID NOT NULL,
    closed_by VARCHAR(200) NOT NULL,
    closure_date TIMESTAMP NOT NULL DEFAULT NOW(),
    summary TEXT,
    lessons_learned TEXT,
    recommendations TEXT,
    checklist_data JSONB NOT NULL DEFAULT '[]'::jsonb,
    all_mandatory_criteria_met BOOLEAN NOT NULL DEFAULT FALSE,
    total_criteria INTEGER NOT NULL DEFAULT 0,
    completed_criteria INTEGER NOT NULL DEFAULT 0,
    mandatory_criteria INTEGER NOT NULL DEFAULT 0,
    completed_mandatory_criteria INTEGER NOT NULL DEFAULT 0,
    status VARCHAR(50) NOT NULL DEFAULT 'Draft', -- Draft, PendingApproval, Approved, Rejected
    rejection_reason TEXT,
    approved_at TIMESTAMP,
    approved_by VARCHAR(200),
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW(),
    
    CONSTRAINT fk_closure_project FOREIGN KEY (project_id) 
        REFERENCES projects(id) ON DELETE CASCADE,
    CONSTRAINT unique_project_closure UNIQUE (project_id)
);

-- Indexes for ProjectClosure
CREATE INDEX IF NOT EXISTS idx_project_closures_project_id ON project_closures(project_id);
CREATE INDEX IF NOT EXISTS idx_project_closures_status ON project_closures(status);
CREATE INDEX IF NOT EXISTS idx_project_closures_created_at ON project_closures(created_at);

-- Comments for ProjectClosure
COMMENT ON TABLE project_closures IS 'Documentos de cierre de proyecto con checklist de criterios';
COMMENT ON COLUMN project_closures.checklist_data IS 'JSON array con criterios: [{ criteriaId, name, isMandatory, isCompleted, notes }]';
COMMENT ON COLUMN project_closures.all_mandatory_criteria_met IS 'Indica si todos los criterios obligatorios están cumplidos';
COMMENT ON COLUMN project_closures.status IS 'Estado del cierre: Draft, PendingApproval, Approved, Rejected';

-- FinalBuild Table
CREATE TABLE IF NOT EXISTS final_builds (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    project_id UUID NOT NULL,
    build_number VARCHAR(100) NOT NULL,
    version VARCHAR(50) NOT NULL,
    build_tag VARCHAR(100),
    commit_hash VARCHAR(100),
    build_date TIMESTAMP NOT NULL DEFAULT NOW(),
    built_by VARCHAR(200) NOT NULL,
    build_environment VARCHAR(100), -- Jenkins, GitHub Actions, Azure DevOps, etc.
    build_configuration VARCHAR(50), -- Release, Debug, Production
    binary_artifacts JSONB NOT NULL DEFAULT '[]'::jsonb,
    main_download_url VARCHAR(1000),
    documentation_url VARCHAR(1000),
    release_notes_url VARCHAR(1000),
    target_platform VARCHAR(100), -- Windows, Linux, MacOS, Web, Android, iOS
    dependencies TEXT,
    system_requirements TEXT,
    is_stable BOOLEAN NOT NULL DEFAULT TRUE,
    tests_passed INTEGER,
    tests_total INTEGER,
    code_coverage DECIMAL(5,2),
    quality_gate_status VARCHAR(50), -- Passed, Failed, Warning
    closure_id UUID,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW(),
    
    CONSTRAINT fk_build_project FOREIGN KEY (project_id) 
        REFERENCES projects(id) ON DELETE CASCADE,
    CONSTRAINT fk_build_closure FOREIGN KEY (closure_id) 
        REFERENCES project_closures(id) ON DELETE SET NULL,
    CONSTRAINT unique_project_build_number UNIQUE (project_id, build_number)
);

-- Indexes for FinalBuild
CREATE INDEX IF NOT EXISTS idx_final_builds_project_id ON final_builds(project_id);
CREATE INDEX IF NOT EXISTS idx_final_builds_build_date ON final_builds(build_date);
CREATE INDEX IF NOT EXISTS idx_final_builds_version ON final_builds(version);
CREATE INDEX IF NOT EXISTS idx_final_builds_is_stable ON final_builds(is_stable);
CREATE INDEX IF NOT EXISTS idx_final_builds_closure_id ON final_builds(closure_id);

-- Comments for FinalBuild
COMMENT ON TABLE final_builds IS 'Registro de builds finales con artefactos binarios y métricas de calidad';
COMMENT ON COLUMN final_builds.binary_artifacts IS 'JSON array con artefactos: [{ name, type, filePath, downloadUrl, size, checksum, checksumType }]';
COMMENT ON COLUMN final_builds.build_number IS 'Identificador único del build (ej: v1.0.0-build.123)';
COMMENT ON COLUMN final_builds.is_stable IS 'true = Stable Release, false = Beta/RC';
COMMENT ON COLUMN final_builds.code_coverage IS 'Porcentaje de cobertura de código (0-100)';
