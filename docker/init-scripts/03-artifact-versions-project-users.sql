-- Add artifact_versions table
CREATE TABLE IF NOT EXISTS artifact_versions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    artifact_id UUID NOT NULL,
    version_number INT NOT NULL,
    file_path VARCHAR(500),
    file_name VARCHAR(255),
    file_size BIGINT,
    uploaded_by VARCHAR(255),
    uploaded_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    change_description TEXT,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_artifact_versions_artifact FOREIGN KEY (artifact_id) REFERENCES artifacts(id) ON DELETE CASCADE,
    CONSTRAINT uq_artifact_versions_artifact_version UNIQUE (artifact_id, version_number)
);

-- Add project_user_roles table
CREATE TABLE IF NOT EXISTS project_user_roles (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    project_id UUID NOT NULL,
    user_id UUID NOT NULL,
    role_id UUID NOT NULL,
    invited_by UUID,
    invited_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    accepted_at TIMESTAMP,
    status VARCHAR(50) NOT NULL DEFAULT 'Pending',
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_project_user_roles_project FOREIGN KEY (project_id) REFERENCES projects(id) ON DELETE CASCADE,
    CONSTRAINT fk_project_user_roles_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    CONSTRAINT fk_project_user_roles_role FOREIGN KEY (role_id) REFERENCES roles(id) ON DELETE RESTRICT,
    CONSTRAINT fk_project_user_roles_inviter FOREIGN KEY (invited_by) REFERENCES users(id) ON DELETE SET NULL,
    CONSTRAINT uq_project_user_roles_project_user_role UNIQUE (project_id, user_id, role_id)
);

-- Create indexes for better performance
CREATE INDEX idx_artifact_versions_artifact_id ON artifact_versions(artifact_id);
CREATE INDEX idx_project_user_roles_project_id ON project_user_roles(project_id);
CREATE INDEX idx_project_user_roles_user_id ON project_user_roles(user_id);
CREATE INDEX idx_project_user_roles_role_id ON project_user_roles(role_id);

-- Comments
COMMENT ON TABLE artifact_versions IS 'Stores versions of artifacts with file metadata';
COMMENT ON TABLE project_user_roles IS 'Maps users to projects with specific roles';
