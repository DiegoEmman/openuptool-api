-- HU-020: Reasignar entregables entre fases o flujos
-- Tabla para registrar historial de movimientos de artefactos

-- Crear tabla de historial de movimientos
CREATE TABLE IF NOT EXISTS artifact_movement_histories (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    artifact_id UUID NOT NULL REFERENCES artifacts(id) ON DELETE CASCADE,
    movement_type VARCHAR(20) NOT NULL, -- PHASE_CHANGE, WORKFLOW_CHANGE
    from_phase_id VARCHAR(50),
    to_phase_id VARCHAR(50),
    from_workflow_id UUID REFERENCES workflows(id) ON DELETE SET NULL,
    to_workflow_id UUID REFERENCES workflows(id) ON DELETE SET NULL,
    from_state_id UUID,
    to_state_id UUID,
    reason VARCHAR(2000),
    moved_by VARCHAR(255) NOT NULL,
    moved_at TIMESTAMP DEFAULT NOW(),
    violated_rules BOOLEAN DEFAULT FALSE,
    violation_details VARCHAR(4000),
    CONSTRAINT chk_movement_type CHECK (movement_type IN ('PHASE_CHANGE', 'WORKFLOW_CHANGE'))
);

-- Indices para mejorar rendimiento
CREATE INDEX IF NOT EXISTS idx_artifact_movement_histories_artifact_id 
    ON artifact_movement_histories(artifact_id);
CREATE INDEX IF NOT EXISTS idx_artifact_movement_histories_moved_at 
    ON artifact_movement_histories(moved_at);
CREATE INDEX IF NOT EXISTS idx_artifact_movement_histories_movement_type 
    ON artifact_movement_histories(movement_type);

-- Trigger para actualizar updated_at en artifacts cuando se mueve
CREATE OR REPLACE FUNCTION update_artifact_on_movement()
RETURNS TRIGGER AS $$
BEGIN
    UPDATE artifacts 
    SET updated_at = NOW() 
    WHERE id = NEW.artifact_id;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS artifact_movement_update_trigger ON artifact_movement_histories;
CREATE TRIGGER artifact_movement_update_trigger
    AFTER INSERT ON artifact_movement_histories
    FOR EACH ROW
    EXECUTE FUNCTION update_artifact_on_movement();

-- Insertar datos de ejemplo para pruebas (se limpiarán después)
-- Comentado porque necesitamos IDs reales
-- INSERT INTO artifact_movement_histories (artifact_id, movement_type, from_phase_id, to_phase_id, reason, moved_by)
-- VALUES ('artifact-uuid', 'PHASE_CHANGE', 'INCEPTION', 'ELABORATION', 'Prueba', 'admin@openuptool.com');
