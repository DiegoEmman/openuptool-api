-- Actualizar hashes de contraseñas para Admin123!
-- Hash generado con BCrypt.Net-Next 4.0.3, WorkFactor 12
UPDATE users 
SET password_hash = '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5jtXmPGhqwfQm'
WHERE email IN ('admin@openuptool.com', 'viewer@openuptool.com');
