-- Script de inicialización de la base de datos
-- Este script se ejecuta automáticamente al iniciar el contenedor PostgreSQL

-- Crear extensiones útiles
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- Configuraciones adicionales
ALTER DATABASE openuptool SET timezone TO 'UTC';

-- Mensaje de confirmación
DO $$ 
BEGIN 
    RAISE NOTICE 'OpenUpTool Database initialized successfully!';
END $$;
