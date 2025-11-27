-- Eliminar usuarios existentes y recrearlos con hashes correctos
DELETE FROM users WHERE email IN ('admin@openuptool.com', 'viewer@openuptool.com');

-- Password: Admin123!
-- Hash generado correctamente con BCrypt WorkFactor 11
INSERT INTO users (id, email, password_hash, first_name, last_name, role_id, is_active, created_at, updated_at) VALUES
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 
     'admin@openuptool.com', 
     '$2a$11$vYmE3qrq4JlP.zeyKL7uxu.s24KQghqr5zsL3GBQH6bPKz3Pq8tLS', 
     'Admin', 
     'Sistema', 
     '11111111-1111-1111-1111-111111111111', 
     TRUE, 
     CURRENT_TIMESTAMP, 
     CURRENT_TIMESTAMP),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb',
     'viewer@openuptool.com',
     '$2a$11$vYmE3qrq4JlP.zeyKL7uxu.s24KQghqr5zsL3GBQH6bPKz3Pq8tLS',
     'Usuario',
     'Viewer',
     '44444444-4444-4444-4444-444444444444',
     TRUE,
     CURRENT_TIMESTAMP,
     CURRENT_TIMESTAMP);
