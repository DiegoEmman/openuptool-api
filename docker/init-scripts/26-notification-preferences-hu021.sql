-- =====================================================
-- HU-021: Notificaciones y Alertas - Preferencias
-- =====================================================
-- Permite a los usuarios configurar sus preferencias
-- de notificaciones por tipo (email/in-app)
-- =====================================================

-- Tabla de preferencias de notificación
CREATE TABLE IF NOT EXISTS notification_preferences (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    notification_type VARCHAR(50) NOT NULL,
    in_app_enabled BOOLEAN NOT NULL DEFAULT TRUE,
    email_enabled BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    
    -- Cada usuario solo puede tener una preferencia por tipo
    CONSTRAINT uq_user_notification_type UNIQUE (user_id, notification_type)
);

-- Índices para mejorar rendimiento
CREATE INDEX IF NOT EXISTS idx_notification_preferences_user_id ON notification_preferences(user_id);
CREATE INDEX IF NOT EXISTS idx_notification_preferences_type ON notification_preferences(notification_type);

-- Comentarios de documentación
COMMENT ON TABLE notification_preferences IS 'HU-021: Preferencias de notificación por usuario y tipo';
COMMENT ON COLUMN notification_preferences.user_id IS 'Usuario propietario de la preferencia';
COMMENT ON COLUMN notification_preferences.notification_type IS 'Tipo de notificación (NEW_VERSION, STATE_APPROVED, etc.)';
COMMENT ON COLUMN notification_preferences.in_app_enabled IS 'Si está habilitada la notificación en la plataforma';
COMMENT ON COLUMN notification_preferences.email_enabled IS 'Si está habilitada la notificación por email';

-- Inicializar preferencias por defecto para usuarios existentes
-- Solo para el usuario admin como ejemplo
INSERT INTO notification_preferences (id, user_id, notification_type, in_app_enabled, email_enabled)
SELECT 
    gen_random_uuid(),
    u.id,
    nt.notification_type,
    TRUE,
    FALSE
FROM users u
CROSS JOIN (
    VALUES 
        ('NEW_VERSION'),
        ('STATE_APPROVED'),
        ('CRITICAL_DEFECT'),
        ('DEADLINE_APPROACHING'),
        ('ARTIFACT_BLOCKED'),
        ('INVITATION'),
        ('INVITATION_ACCEPTED'),
        ('MENTION'),
        ('COMMENT'),
        ('PHASE_CHANGE'),
        ('WORKFLOW_CHANGE')
) AS nt(notification_type)
WHERE u.email = 'admin@openuptool.com'
ON CONFLICT (user_id, notification_type) DO NOTHING;

-- Vista para resumen de preferencias por usuario
CREATE OR REPLACE VIEW v_user_notification_settings AS
SELECT 
    u.id AS user_id,
    u.email,
    u.full_name,
    COUNT(np.id) AS total_preferences,
    COUNT(CASE WHEN np.in_app_enabled THEN 1 END) AS in_app_enabled_count,
    COUNT(CASE WHEN np.email_enabled THEN 1 END) AS email_enabled_count
FROM users u
LEFT JOIN notification_preferences np ON u.id = np.user_id
GROUP BY u.id, u.email, u.full_name;

COMMENT ON VIEW v_user_notification_settings IS 'HU-021: Resumen de preferencias de notificación por usuario';

-- Función para inicializar preferencias de un nuevo usuario
CREATE OR REPLACE FUNCTION initialize_user_notification_preferences(p_user_id UUID)
RETURNS VOID AS $$
DECLARE
    notification_types TEXT[] := ARRAY[
        'NEW_VERSION',
        'STATE_APPROVED',
        'CRITICAL_DEFECT',
        'DEADLINE_APPROACHING',
        'ARTIFACT_BLOCKED',
        'INVITATION',
        'INVITATION_ACCEPTED',
        'MENTION',
        'COMMENT',
        'PHASE_CHANGE',
        'WORKFLOW_CHANGE'
    ];
    nt TEXT;
BEGIN
    FOREACH nt IN ARRAY notification_types
    LOOP
        INSERT INTO notification_preferences (
            id, user_id, notification_type, in_app_enabled, email_enabled
        ) VALUES (
            gen_random_uuid(), p_user_id, nt, TRUE, FALSE
        ) ON CONFLICT (user_id, notification_type) DO NOTHING;
    END LOOP;
END;
$$ LANGUAGE plpgsql;

COMMENT ON FUNCTION initialize_user_notification_preferences IS 'HU-021: Inicializa preferencias por defecto para un usuario';

-- Mensaje de confirmación
DO $$
BEGIN
    RAISE NOTICE 'HU-021: Tabla notification_preferences creada correctamente';
    RAISE NOTICE 'HU-021: Preferencias inicializadas para admin@openuptool.com';
END $$;
