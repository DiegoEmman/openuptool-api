-- =====================================================
-- SCRIPT: 07-invitations-notifications.sql
-- DESCRIPCIÓN: Sistema de invitaciones y notificaciones
-- =====================================================

-- Tabla: project_invitations (invitaciones a proyectos)
CREATE TABLE IF NOT EXISTS project_invitations (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    project_id UUID NOT NULL REFERENCES projects(id) ON DELETE CASCADE,
    invited_email VARCHAR(255) NOT NULL,
    role_id UUID NOT NULL REFERENCES roles(id) ON DELETE CASCADE,
    invited_by UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    status VARCHAR(20) DEFAULT 'pending' CHECK (status IN ('pending', 'accepted', 'rejected', 'expired')),
    invitation_token VARCHAR(100) UNIQUE NOT NULL,
    expires_at TIMESTAMP NOT NULL,
    accepted_at TIMESTAMP,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW()
);

CREATE INDEX idx_invitations_project ON project_invitations(project_id);
CREATE INDEX idx_invitations_email ON project_invitations(invited_email);
CREATE INDEX idx_invitations_token ON project_invitations(invitation_token);
CREATE INDEX idx_invitations_status ON project_invitations(status);

-- Tabla: notifications (notificaciones de usuarios)
CREATE TABLE IF NOT EXISTS notifications (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    type VARCHAR(50) NOT NULL CHECK (type IN ('invitation', 'invitation_accepted', 'artifact_uploaded', 'plan_updated', 'iteration_created', 'comment_added', 'mention')),
    title VARCHAR(200) NOT NULL,
    message TEXT NOT NULL,
    related_entity_type VARCHAR(50), -- 'project', 'artifact', 'iteration', etc.
    related_entity_id UUID,
    is_read BOOLEAN DEFAULT FALSE,
    action_url VARCHAR(500),
    created_at TIMESTAMP DEFAULT NOW(),
    read_at TIMESTAMP
);

CREATE INDEX idx_notifications_user ON notifications(user_id);
CREATE INDEX idx_notifications_read ON notifications(user_id, is_read);
CREATE INDEX idx_notifications_created ON notifications(created_at DESC);

COMMENT ON TABLE project_invitations IS 'Invitaciones a usuarios para unirse a proyectos con roles específicos';
COMMENT ON TABLE notifications IS 'Notificaciones in-app para usuarios (invitaciones, menciones, actualizaciones)';
