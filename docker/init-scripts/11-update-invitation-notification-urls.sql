-- Script para actualizar las URLs de las notificaciones de invitación
-- para incluir el token de invitación en lugar del ID del proyecto

-- Actualizar las notificaciones existentes de tipo 'invitation' 
-- para que apunten al token de invitación en lugar del proyecto
UPDATE notifications n
SET action_url = '/invitations/accept/' || pi.invitation_token
FROM project_invitations pi
WHERE n.type = 'invitation'
  AND n.related_entity_type = 'project'
  AND n.related_entity_id = pi.project_id
  AND pi.status = 'pending'
  AND n.action_url LIKE '/projects/%';

-- Verificar las notificaciones actualizadas
SELECT 
    n.id,
    n.type,
    n.title,
    n.action_url,
    pi.invitation_token,
    pi.invited_email,
    pi.status as invitation_status
FROM notifications n
LEFT JOIN project_invitations pi ON n.related_entity_id = pi.project_id
WHERE n.type = 'invitation'
ORDER BY n.created_at DESC;
