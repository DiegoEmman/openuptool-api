-- Script para agregar usuarios de prueba al sistema OpenUpTool
-- Las contraseñas están hasheadas con BCrypt (contraseña: Password123!)

-- Usuario 1: Manager
INSERT INTO users (id, first_name, last_name, email, password_hash, role_id, is_active, created_at)
VALUES (
    gen_random_uuid(),
    'María',
    'González',
    'maria.gonzalez@openuptool.com',
    '$2a$11$XqC3z8gR7LGPxJ5R7J5z8uVYmBF.9QKE1Y3Z5qH8kL9M2nP4rS6Oa', -- Password123!
    (SELECT id FROM roles WHERE name = 'Manager'),
    true,
    NOW()
);

-- Usuario 2: Developer
INSERT INTO users (id, first_name, last_name, email, password_hash, role_id, is_active, created_at)
VALUES (
    gen_random_uuid(),
    'Carlos',
    'Ramírez',
    'carlos.ramirez@openuptool.com',
    '$2a$11$XqC3z8gR7LGPxJ5R7J5z8uVYmBF.9QKE1Y3Z5qH8kL9M2nP4rS6Oa', -- Password123!
    (SELECT id FROM roles WHERE name = 'Developer'),
    true,
    NOW()
);

-- Usuario 3: Developer
INSERT INTO users (id, first_name, last_name, email, password_hash, role_id, is_active, created_at)
VALUES (
    gen_random_uuid(),
    'Ana',
    'Martínez',
    'ana.martinez@openuptool.com',
    '$2a$11$XqC3z8gR7LGPxJ5R7J5z8uVYmBF.9QKE1Y3Z5qH8kL9M2nP4rS6Oa', -- Password123!
    (SELECT id FROM roles WHERE name = 'Developer'),
    true,
    NOW()
);

-- Usuario 4: Developer
INSERT INTO users (id, first_name, last_name, email, password_hash, role_id, is_active, created_at)
VALUES (
    gen_random_uuid(),
    'Luis',
    'Torres',
    'luis.torres@openuptool.com',
    '$2a$11$XqC3z8gR7LGPxJ5R7J5z8uVYmBF.9QKE1Y3Z5qH8kL9M2nP4rS6Oa', -- Password123!
    (SELECT id FROM roles WHERE name = 'Developer'),
    true,
    NOW()
);

-- Usuario 5: Viewer
INSERT INTO users (id, first_name, last_name, email, password_hash, role_id, is_active, created_at)
VALUES (
    gen_random_uuid(),
    'Patricia',
    'López',
    'patricia.lopez@openuptool.com',
    '$2a$11$XqC3z8gR7LGPxJ5R7J5z8uVYmBF.9QKE1Y3Z5qH8kL9M2nP4rS6Oa', -- Password123!
    (SELECT id FROM roles WHERE name = 'Viewer'),
    true,
    NOW()
);

-- Usuario 6: Manager
INSERT INTO users (id, first_name, last_name, email, password_hash, role_id, is_active, created_at)
VALUES (
    gen_random_uuid(),
    'Roberto',
    'Sánchez',
    'roberto.sanchez@openuptool.com',
    '$2a$11$XqC3z8gR7LGPxJ5R7J5z8uVYmBF.9QKE1Y3Z5qH8kL9M2nP4rS6Oa', -- Password123!
    (SELECT id FROM roles WHERE name = 'Manager'),
    true,
    NOW()
);

-- Usuario 7: Developer
INSERT INTO users (id, first_name, last_name, email, password_hash, role_id, is_active, created_at)
VALUES (
    gen_random_uuid(),
    'Laura',
    'Fernández',
    'laura.fernandez@openuptool.com',
    '$2a$11$XqC3z8gR7LGPxJ5R7J5z8uVYmBF.9QKE1Y3Z5qH8kL9M2nP4rS6Oa', -- Password123!
    (SELECT id FROM roles WHERE name = 'Developer'),
    true,
    NOW()
);

-- Usuario 8: Developer
INSERT INTO users (id, first_name, last_name, email, password_hash, role_id, is_active, created_at)
VALUES (
    gen_random_uuid(),
    'Jorge',
    'Hernández',
    'jorge.hernandez@openuptool.com',
    '$2a$11$XqC3z8gR7LGPxJ5R7J5z8uVYmBF.9QKE1Y3Z5qH8kL9M2nP4rS6Oa', -- Password123!
    (SELECT id FROM roles WHERE name = 'Developer'),
    true,
    NOW()
);
