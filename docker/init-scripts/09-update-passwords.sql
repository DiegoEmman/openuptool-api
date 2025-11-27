-- Actualizar contraseñas de todos los usuarios con el hash correcto de BCrypt
-- Contraseña: Password123!
-- Hash generado: $2a$11$MCFHcsU/WBZH30BHuIaIFeXU6aauvkfnsQhK.bps5UiTP8vfntBe.

UPDATE users 
SET password_hash = '$2a$11$MCFHcsU/WBZH30BHuIaIFeXU6aauvkfnsQhK.bps5UiTP8vfntBe.'
WHERE email IN (
    'admin@openuptool.com',
    'maria.gonzalez@openuptool.com',
    'roberto.sanchez@openuptool.com',
    'carlos.ramirez@openuptool.com',
    'ana.martinez@openuptool.com',
    'luis.torres@openuptool.com',
    'laura.fernandez@openuptool.com',
    'jorge.hernandez@openuptool.com',
    'patricia.lopez@openuptool.com',
    'viewer@openuptool.com'
);

-- Verificar la actualización
SELECT email, first_name, last_name, 
       substring(password_hash, 1, 20) || '...' as hash_preview
FROM users 
WHERE email LIKE '%@openuptool.com'
ORDER BY email;
