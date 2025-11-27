warn: Microsoft.AspNetCore.HttpsPolicy.HttpsRedirectionMiddleware[3]
      Failed to determine the https port for redirect.
OPTIONS /api/auth/login                          204 (1ms)
POST   /api/auth/login                          401 (1255ms)
-- Crear tablas de autenticación
CREATE TABLE IF NOT EXISTS roles (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(50) NOT NULL UNIQUE,
    description VARCHAR(255),
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    email VARCHAR(255) NOT NULL UNIQUE,
    password_hash TEXT NOT NULL,
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    role_id UUID NOT NULL REFERENCES roles(id) ON DELETE RESTRICT,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    last_login_at TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_users_email ON users(email);
CREATE INDEX IF NOT EXISTS idx_users_role_id ON users(role_id);

-- Insertar roles predefinidos
INSERT INTO roles (id, name, description, created_at, updated_at) VALUES
    ('11111111-1111-1111-1111-111111111111', 'Admin', 'Acceso completo al sistema. Puede crear, editar, eliminar y gestionar usuarios y roles.', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('22222222-2222-2222-2222-222222222222', 'Manager', 'Puede crear proyectos, modificar estados, asignar responsables y gestionar planes.', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('33333333-3333-3333-3333-333333333333', 'Developer', 'Puede agregar artefactos, iteraciones y actualizar su progreso. No puede modificar proyectos.', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('44444444-4444-4444-4444-444444444444', 'Viewer', 'Solo puede ver información de proyectos. Sin permisos de edición.', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)
ON CONFLICT (name) DO NOTHING;

-- Crear usuarios por defecto
-- Password: Admin123! (hasheado con BCrypt WorkFactor 11)
-- Hash generado con BCrypt.Net-Next 4.0.3
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
     CURRENT_TIMESTAMP)
ON CONFLICT (email) DO NOTHING;

-- Credenciales de prueba:
-- Admin: admin@openuptool.com / Admin123!
-- Viewer: viewer@openuptool.com / Admin123!
