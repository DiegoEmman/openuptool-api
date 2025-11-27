-- =====================================================
-- SCRIPT: 06-iteration-scope.sql
-- DESCRIPCIÓN: Tabla para alcance de iteraciones
-- =====================================================

-- Tabla: iteration_scope (alcance de iteraciones - historias y artefactos)
CREATE TABLE IF NOT EXISTS iteration_scope (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    iteration_id UUID NOT NULL REFERENCES iterations(id) ON DELETE CASCADE,
    item_type VARCHAR(20) NOT NULL CHECK (item_type IN ('story', 'artifact')),
    item_id UUID NOT NULL, -- Referencia a user_story.id o artifact.id
    description TEXT,
    estimated_hours DECIMAL(10, 2),
    status VARCHAR(20) DEFAULT 'pending' CHECK (status IN ('pending', 'in_progress', 'completed', 'blocked')),
    assigned_to UUID REFERENCES users(id) ON DELETE SET NULL,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),
    UNIQUE(iteration_id, item_type, item_id)
);

CREATE INDEX idx_iteration_scope_iteration ON iteration_scope(iteration_id);
CREATE INDEX idx_iteration_scope_item ON iteration_scope(item_type, item_id);
CREATE INDEX idx_iteration_scope_assigned ON iteration_scope(assigned_to);

-- Tabla: user_stories (historias de usuario para el alcance)
CREATE TABLE IF NOT EXISTS user_stories (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    project_id UUID NOT NULL REFERENCES projects(id) ON DELETE CASCADE,
    title VARCHAR(200) NOT NULL,
    description TEXT,
    acceptance_criteria TEXT,
    priority VARCHAR(20) DEFAULT 'medium' CHECK (priority IN ('low', 'medium', 'high', 'critical')),
    story_points INTEGER,
    status VARCHAR(20) DEFAULT 'backlog' CHECK (status IN ('backlog', 'ready', 'in_progress', 'done', 'blocked')),
    created_by UUID REFERENCES users(id) ON DELETE SET NULL,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW()
);

CREATE INDEX idx_user_stories_project ON user_stories(project_id);
CREATE INDEX idx_user_stories_status ON user_stories(status);

COMMENT ON TABLE iteration_scope IS 'Alcance de cada iteración: historias de usuario y artefactos asociados';
COMMENT ON TABLE user_stories IS 'Historias de usuario del proyecto (backlog)';
