-- HU-013: Roles y permisos por flujo
-- Crear tabla workflow_permissions

CREATE TABLE IF NOT EXISTS workflow_permissions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    workflow_id UUID NOT NULL,
    role VARCHAR(50) NOT NULL, -- autor, revisor, PO, admin
    action VARCHAR(50) NOT NULL, -- crear, editar, aprobar, cambiar_estado
    is_allowed BOOLEAN NOT NULL DEFAULT true,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP,
    
    CONSTRAINT fk_workflow_permissions_workflow FOREIGN KEY (workflow_id) REFERENCES workflows(id) ON DELETE CASCADE,
    CONSTRAINT uq_workflow_permissions_workflow_role_action UNIQUE (workflow_id, role, action)
);

CREATE INDEX IF NOT EXISTS idx_workflow_permissions_workflow_id ON workflow_permissions(workflow_id);
CREATE INDEX IF NOT EXISTS idx_workflow_permissions_role ON workflow_permissions(role);
CREATE INDEX IF NOT EXISTS idx_workflow_permissions_action ON workflow_permissions(action);

COMMENT ON TABLE workflow_permissions IS 'Matriz de permisos para workflows - define qué roles pueden realizar qué acciones';
COMMENT ON COLUMN workflow_permissions.role IS 'Rol del usuario: autor, revisor, PO, admin';
COMMENT ON COLUMN workflow_permissions.action IS 'Acción permitida: crear, editar, aprobar, cambiar_estado';
COMMENT ON COLUMN workflow_permissions.is_allowed IS 'Indica si el permiso está permitido o denegado';

-- Nota: Los permisos iniciales se crean en el seed de C# (DatabaseManagementController.SeedData)
-- Esto evita duplicados y mantiene la lógica de seed centralizada
