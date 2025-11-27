-- Asignar admin como Manager a todos los proyectos existentes
INSERT INTO project_user_roles (id, project_id, user_id, role_id, invited_by, invited_at, accepted_at, status, created_at, updated_at)
SELECT 
    gen_random_uuid(),
    p.id,
    'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa'::uuid, -- admin user id
    '22222222-2222-2222-2222-222222222222'::uuid, -- Manager role id
    'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa'::uuid, -- invited by admin
    NOW(),
    NOW(),
    'active',
    NOW(),
    NOW()
FROM projects p
WHERE NOT EXISTS (
    SELECT 1 FROM project_user_roles pur
    WHERE pur.project_id = p.id 
    AND pur.user_id = 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa'::uuid
);

-- Verificar la asignación
SELECT p.name, u.email, r.name as role, pur.status
FROM project_user_roles pur
JOIN projects p ON pur.project_id = p.id
JOIN users u ON pur.user_id = u.id
JOIN roles r ON pur.role_id = r.id
WHERE u.email = 'admin@openuptool.com'
ORDER BY p.created_at DESC;
