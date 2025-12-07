-- =============================================
-- Script: 22-microincrements-hu017.sql
-- Descripción: HU-017 - Agregar campos para microincrementos técnicos
-- Fecha: 2025-12-07
-- =============================================

-- Agregar columna type para diferenciar técnico/funcional
ALTER TABLE microincrements 
ADD COLUMN IF NOT EXISTS type VARCHAR(20) DEFAULT 'funcional';

-- Agregar columna para URL de evidencia
ALTER TABLE microincrements 
ADD COLUMN IF NOT EXISTS evidence_url VARCHAR(1000);

-- Agregar columna para archivo de evidencia
ALTER TABLE microincrements 
ADD COLUMN IF NOT EXISTS evidence_file_path VARCHAR(500);

-- Crear índice para filtrar por tipo
CREATE INDEX IF NOT EXISTS idx_microincrements_type ON microincrements(type);

-- Comentarios descriptivos
COMMENT ON COLUMN microincrements.type IS 'Tipo de microincremento: tecnico o funcional';
COMMENT ON COLUMN microincrements.evidence_url IS 'URL de evidencia (enlace externo)';
COMMENT ON COLUMN microincrements.evidence_file_path IS 'Ruta del archivo de evidencia';
