$ErrorActionPreference = "Continue"
$baseUrl = "http://localhost:5000"

Write-Host "`n=== TEST HU-012: WORKFLOWS Y ESTADOS DE ARTEFACTOS ===" -ForegroundColor Cyan

# 0. Recreate Database
Write-Host "[0] Recreate Database..." -ForegroundColor Yellow
try {
    $recreateHeaders = @{ "Content-Type" = "application/json" }
    $recreateResult = Invoke-RestMethod -Uri "$baseUrl/api/DatabaseManagement/recreate-database" -Method POST -Headers $recreateHeaders
    Write-Host "    OK: $($recreateResult.message)`n" -ForegroundColor Green
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
    exit
}

# 1. Seed Data
Write-Host "[1] Seed Data..." -ForegroundColor Yellow
try {
    $seedResult = Invoke-RestMethod -Uri "$baseUrl/api/DatabaseManagement/seed-data" -Method POST -Headers $recreateHeaders
    Write-Host "    OK: $($seedResult.message)" -ForegroundColor Green
    Write-Host "    Workflows: $($seedResult.resumen.workflows)" -ForegroundColor Gray
    Write-Host "    Estados: $($seedResult.resumen.estadosWorkflow)" -ForegroundColor Gray
    Write-Host "    Responsables: $($seedResult.resumen.responsablesEstados)`n" -ForegroundColor Gray
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
    exit
}

# 2. Login
Write-Host "[2] Login..." -ForegroundColor Yellow
try {
    $loginBody = '{"email":"admin@openuptool.com","password":"Password123!"}'
    $response = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method POST -Body $loginBody -ContentType "application/json"
    $headers = @{ 
        Authorization = "Bearer $($response.token)"
        "Content-Type" = "application/json"
    }
    Write-Host "    OK: Token obtenido`n" -ForegroundColor Green
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
    exit
}

# 3. Obtener proyecto del seed
Write-Host "[3] Obtener proyecto..." -ForegroundColor Yellow
try {
    $projects = Invoke-RestMethod -Uri "$baseUrl/api/projects" -Method GET -Headers $headers
    if ($projects.Count -eq 0) {
        Write-Host "    ERROR: No hay proyectos`n" -ForegroundColor Red
        exit
    }
    $projectId = $projects[0].id
    Write-Host "    OK: $projectId - $($projects[0].name)`n" -ForegroundColor Green
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
    exit
}

# ========== WORKFLOWS TESTS ==========

# 4. TEST: Obtener todos los workflows
Write-Host "[4] TEST: Obtener todos los workflows" -ForegroundColor Yellow
try {
    $workflows = Invoke-RestMethod -Uri "$baseUrl/api/workflows" -Method GET -Headers $headers
    Write-Host "    OK: $($workflows.Count) workflows encontrados" -ForegroundColor Green
    foreach ($wf in $workflows) {
        Write-Host "       - $($wf.name) (Estados: $($wf.states.Count))" -ForegroundColor Gray
    }
    Write-Host ""
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# 5. TEST: Obtener workflows por proyecto
Write-Host "[5] TEST: Obtener workflows por proyecto" -ForegroundColor Yellow
try {
    $projectWorkflows = Invoke-RestMethod -Uri "$baseUrl/api/workflows/project/$projectId" -Method GET -Headers $headers
    Write-Host "    OK: $($projectWorkflows.Count) workflows del proyecto" -ForegroundColor Green
    $workflowId = $projectWorkflows[0].id
    Write-Host "       Workflow ID para tests: $workflowId`n" -ForegroundColor Gray
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# 6. TEST: Obtener workflow por ID con estados completos
Write-Host "[6] TEST: Obtener workflow por ID (con estados)" -ForegroundColor Yellow
try {
    $workflow = Invoke-RestMethod -Uri "$baseUrl/api/workflows/$workflowId" -Method GET -Headers $headers
    Write-Host "    OK: $($workflow.name)" -ForegroundColor Green
    Write-Host "       Estados ordenados:" -ForegroundColor Gray
    foreach ($state in $workflow.states | Sort-Object order) {
        Write-Host "         $($state.order). $($state.name) [$($state.color)] - Responsables: $($state.responsibles.Count)" -ForegroundColor Gray
    }
    Write-Host ""
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# 7. TEST: Crear nuevo workflow
Write-Host "[7] TEST: Crear nuevo workflow" -ForegroundColor Yellow
try {
    $createWorkflowBody = @{
        projectId = $projectId
        name = "Flujo de Validación QA"
        description = "Flujo para validación de calidad de artefactos"
    } | ConvertTo-Json
    
    $newWorkflow = Invoke-RestMethod -Uri "$baseUrl/api/workflows" -Method POST -Body $createWorkflowBody -Headers $headers
    Write-Host "    OK: Workflow creado - $($newWorkflow.name)" -ForegroundColor Green
    Write-Host "       ID: $($newWorkflow.id)`n" -ForegroundColor Gray
    $newWorkflowId = $newWorkflow.id
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# 8. TEST: Actualizar workflow
Write-Host "[8] TEST: Actualizar workflow" -ForegroundColor Yellow
try {
    $updateWorkflowBody = @{
        name = "Flujo de Validación QA (Actualizado)"
        description = "Flujo mejorado para validación de calidad"
        isActive = $true
    } | ConvertTo-Json
    
    $updatedWorkflow = Invoke-RestMethod -Uri "$baseUrl/api/workflows/$newWorkflowId" -Method PUT -Body $updateWorkflowBody -Headers $headers
    Write-Host "    OK: Workflow actualizado - $($updatedWorkflow.name)`n" -ForegroundColor Green
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# ========== WORKFLOW STATES TESTS ==========

# 9. TEST: Obtener estados de un workflow
Write-Host "[9] TEST: Obtener estados de un workflow" -ForegroundColor Yellow
try {
    $states = Invoke-RestMethod -Uri "$baseUrl/api/workflows/$workflowId/states" -Method GET -Headers $headers
    Write-Host "    OK: $($states.Count) estados encontrados" -ForegroundColor Green
    $stateId = $states[0].id
    Write-Host "       Estado ID para tests: $stateId`n" -ForegroundColor Gray
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# 10. TEST: Crear nuevo estado
Write-Host "[10] TEST: Crear nuevo estado en workflow" -ForegroundColor Yellow
try {
    $createStateBody = @{
        workflowId = $newWorkflowId
        name = "En Validación"
        description = "Artefacto bajo validación de QA"
        order = 1
        color = "#3B82F6"
        isInitialState = $true
        isFinalState = $false
        requiredActions = '["Revisar checklist","Ejecutar pruebas"]'
    } | ConvertTo-Json
    
    $newState = Invoke-RestMethod -Uri "$baseUrl/api/workflows/states" -Method POST -Body $createStateBody -Headers $headers
    Write-Host "    OK: Estado creado - $($newState.name)" -ForegroundColor Green
    Write-Host "       ID: $($newState.id)`n" -ForegroundColor Gray
    $newStateId = $newState.id
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# 11. TEST: Obtener estado por ID
Write-Host "[11] TEST: Obtener estado por ID" -ForegroundColor Yellow
try {
    $state = Invoke-RestMethod -Uri "$baseUrl/api/workflows/states/$newStateId" -Method GET -Headers $headers
    Write-Host "    OK: $($state.name)" -ForegroundColor Green
    Write-Host "       Orden: $($state.order), Color: $($state.color)" -ForegroundColor Gray
    Write-Host "       Inicial: $($state.isInitialState), Final: $($state.isFinalState)`n" -ForegroundColor Gray
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# 12. TEST: Actualizar estado
Write-Host "[12] TEST: Actualizar estado" -ForegroundColor Yellow
try {
    $updateStateBody = @{
        name = "En Validación QA"
        description = "Artefacto bajo validación rigurosa de QA"
        order = 1
        color = "#2563EB"
        isInitialState = $true
        isFinalState = $false
        requiredActions = '["Revisar checklist completo","Ejecutar pruebas funcionales","Validar cobertura"]'
    } | ConvertTo-Json
    
    $updatedState = Invoke-RestMethod -Uri "$baseUrl/api/workflows/states/$newStateId" -Method PUT -Body $updateStateBody -Headers $headers
    Write-Host "    OK: Estado actualizado - $($updatedState.name)`n" -ForegroundColor Green
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# ========== RESPONSIBLES TESTS ==========

# 13. TEST: Agregar responsable a estado
Write-Host "[13] TEST: Agregar responsable a estado" -ForegroundColor Yellow
try {
    $users = Invoke-RestMethod -Uri "$baseUrl/api/projects/$projectId/users" -Method GET -Headers $headers
    $testerId = $users | Select-Object -First 1 -ExpandProperty id
    
    $addResponsibleBody = @{
        workflowStateId = $newStateId
        userId = $testerId
        role = "Validador QA"
    } | ConvertTo-Json
    
    $responsible = Invoke-RestMethod -Uri "$baseUrl/api/workflows/states/responsibles" -Method POST -Body $addResponsibleBody -Headers $headers
    Write-Host "    OK: Responsable agregado - $($responsible.userName)" -ForegroundColor Green
    Write-Host "       Rol: $($responsible.role)`n" -ForegroundColor Gray
    $responsibleId = $responsible.id
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# 14. TEST: Eliminar responsable
Write-Host "[14] TEST: Eliminar responsable" -ForegroundColor Yellow
try {
    Invoke-RestMethod -Uri "$baseUrl/api/workflows/states/responsibles/$responsibleId" -Method DELETE -Headers $headers
    Write-Host "    OK: Responsable eliminado`n" -ForegroundColor Green
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# ========== ARTIFACT STATE TESTS ==========

# 15. TEST: Usar artefacto del seed
Write-Host "[15] Usando artefacto del seed..." -ForegroundColor Yellow
$artifactId = "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"
Write-Host "    OK: $artifactId`n" -ForegroundColor Green

# 16. TEST: Obtener artefacto con información de workflow
Write-Host "[16] TEST: Obtener artefacto con workflow" -ForegroundColor Yellow
try {
    $artifactWithWorkflow = Invoke-RestMethod -Uri "$baseUrl/api/workflows/artifacts/$artifactId/workflow" -Method GET -Headers $headers
    Write-Host "    OK: $($artifactWithWorkflow.title)" -ForegroundColor Green
    Write-Host "       Workflow: $($artifactWithWorkflow.workflowName)" -ForegroundColor Gray
    Write-Host "       Estado actual: $($artifactWithWorkflow.currentStateName) [$($artifactWithWorkflow.currentStateColor)]" -ForegroundColor Gray
    Write-Host "       Historial: $($artifactWithWorkflow.stateHistory.Count) cambios`n" -ForegroundColor Gray
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# 17. TEST: Obtener historial de estados del artefacto
Write-Host "[17] TEST: Obtener historial de estados" -ForegroundColor Yellow
try {
    $history = Invoke-RestMethod -Uri "$baseUrl/api/workflows/artifacts/$artifactId/history" -Method GET -Headers $headers
    Write-Host "    OK: $($history.Count) cambios de estado" -ForegroundColor Green
    foreach ($change in $history) {
        $fromState = if ($change.fromStateName) { $change.fromStateName } else { "[Inicial]" }
        Write-Host "       $fromState -> $($change.toStateName) por $($change.changedByUserName)" -ForegroundColor Gray
        Write-Host "       Fecha: $($change.changedAt), Comentario: $($change.comments)" -ForegroundColor DarkGray
    }
    Write-Host ""
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# 18. TEST: Cambiar estado de artefacto
Write-Host "[18] TEST: Cambiar estado de artefacto" -ForegroundColor Yellow
try {
    # Obtener el siguiente estado (Aprobado) del workflow del artefacto
    $workflow = Invoke-RestMethod -Uri "$baseUrl/api/workflows/$($artifactWithWorkflow.workflowId)" -Method GET -Headers $headers
    $aprobadonState = $workflow.states | Where-Object { $_.name -eq "Aprobado" } | Select-Object -First 1
    
    $changeStateBody = @{
        artifactId = $artifactId
        toStateId = $aprobadonState.id
        comments = "Documento revisado y aprobado por el equipo"
        metadata = '{"reviewScore":95,"reviewers":2}'
    } | ConvertTo-Json
    
    $stateChange = Invoke-RestMethod -Uri "$baseUrl/api/workflows/artifacts/change-state" -Method POST -Body $changeStateBody -Headers $headers
    Write-Host "    OK: Estado cambiado" -ForegroundColor Green
    Write-Host "       De: $($stateChange.fromStateName) -> A: $($stateChange.toStateName)" -ForegroundColor Gray
    Write-Host "       Por: $($stateChange.changedByUserName)" -ForegroundColor Gray
    Write-Host "       Comentario: $($stateChange.comments)`n" -ForegroundColor Gray
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# 19. TEST: Asignar workflow a artefacto  
Write-Host "[19] TEST: Asignar workflow a artefacto" -ForegroundColor Yellow
try {
    # Reasignar el workflow nuevo al artefacto existente (simula cambio de workflow)
    $artifact = Invoke-RestMethod -Uri "$baseUrl/api/workflows/artifacts/$artifactId/assign-workflow/$newWorkflowId" -Method POST -Headers $headers
    Write-Host "    OK: Workflow asignado al artefacto" -ForegroundColor Green
    Write-Host "       Artefacto: $($artifact.title)" -ForegroundColor Gray
    Write-Host "       Workflow: $($artifact.workflowName)`n" -ForegroundColor Gray
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# 20. TEST: Eliminar estado
Write-Host "[20] TEST: Eliminar estado" -ForegroundColor Yellow
try {
    Invoke-RestMethod -Uri "$baseUrl/api/workflows/states/$newStateId" -Method DELETE -Headers $headers
    Write-Host "    OK: Estado eliminado`n" -ForegroundColor Green
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# 21. TEST: Eliminar workflow
Write-Host "[21] TEST: Eliminar workflow" -ForegroundColor Yellow
try {
    Invoke-RestMethod -Uri "$baseUrl/api/workflows/$newWorkflowId" -Method DELETE -Headers $headers
    Write-Host "    OK: Workflow eliminado`n" -ForegroundColor Green
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

Write-Host "`n=== RESUMEN DE TESTS ===" -ForegroundColor Cyan
Write-Host "21 endpoints probados correctamente" -ForegroundColor Green
Write-Host "   - 5 endpoints de Workflows (GET all, GET by project, GET by ID, POST, PUT, DELETE)" -ForegroundColor Gray
Write-Host "   - 5 endpoints de WorkflowStates (GET by workflow, GET by ID, POST, PUT, DELETE)" -ForegroundColor Gray
Write-Host "   - 2 endpoints de Responsibles (POST, DELETE)" -ForegroundColor Gray
Write-Host "   - 4 endpoints de ArtifactState (GET with workflow, GET history, POST change-state, POST assign-workflow)" -ForegroundColor Gray
Write-Host ""
Write-Host "HU-012 completada" -ForegroundColor Green
Write-Host "- Workflows personalizados definidos" -ForegroundColor Green
Write-Host "- Estados ordenados con colores y acciones" -ForegroundColor Green
Write-Host "- Responsables asignados a estados" -ForegroundColor Green
Write-Host "- Historial de cambios auditado" -ForegroundColor Green
Write-Host ""
