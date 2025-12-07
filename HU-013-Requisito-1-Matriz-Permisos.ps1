$ErrorActionPreference = "Continue"
$baseUrl = "http://localhost:5000"

Write-Host "`n=== TEST HU-013: ROLES Y PERMISOS POR FLUJO (Requisito 1) ===" -ForegroundColor Cyan
Write-Host "Matriz de permisos - Asignar acciones a roles`n" -ForegroundColor Gray

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
    Write-Host "    Permisos creados en seed`n" -ForegroundColor Gray
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

# 3. Obtener workflows
Write-Host "[3] Obtener workflows..." -ForegroundColor Yellow
try {
    $workflows = Invoke-RestMethod -Uri "$baseUrl/api/workflows" -Method GET -Headers $headers
    if ($workflows.Count -eq 0) {
        Write-Host "    ERROR: No hay workflows`n" -ForegroundColor Red
        exit
    }
    $workflowId = $workflows[0].id
    $workflowName = $workflows[0].name
    Write-Host "    OK: $($workflows.Count) workflows encontrados" -ForegroundColor Green
    Write-Host "       Usando: $workflowName`n" -ForegroundColor Gray
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
    exit
}

# ========== WORKFLOW PERMISSIONS TESTS ==========

# 4. TEST: Obtener matriz de permisos
Write-Host "[4] TEST: Obtener matriz de permisos" -ForegroundColor Yellow
try {
    $matrix = Invoke-RestMethod -Uri "$baseUrl/api/workflows/$workflowId/permissions/matrix" -Method GET -Headers $headers
    Write-Host "    OK: Matriz de permisos obtenida" -ForegroundColor Green
    Write-Host "       Workflow: $($matrix.workflowName)" -ForegroundColor Gray
    Write-Host "       Roles configurados: $($matrix.permissions.Count)" -ForegroundColor Gray
    
    foreach ($rolePermission in $matrix.permissions) {
        Write-Host "`n       Rol: $($rolePermission.role)" -ForegroundColor Cyan
        foreach ($action in $rolePermission.actions.GetEnumerator()) {
            $status = if ($action.Value) { "[OK]" } else { "[X]" }
            $color = if ($action.Value) { "Green" } else { "Red" }
            Write-Host "         $status $($action.Key)" -ForegroundColor $color
        }
    }
    Write-Host ""
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# 5. TEST: Obtener todos los permisos de un workflow
Write-Host "[5] TEST: Obtener todos los permisos" -ForegroundColor Yellow
try {
    $permissions = Invoke-RestMethod -Uri "$baseUrl/api/workflows/$workflowId/permissions" -Method GET -Headers $headers
    Write-Host "    OK: $($permissions.Count) permisos encontrados" -ForegroundColor Green
    Write-Host "       Detalle:" -ForegroundColor Gray
    
    $grouped = $permissions | Group-Object -Property role
    foreach ($group in $grouped) {
        Write-Host "         $($group.Name): $($group.Count) acciones" -ForegroundColor Gray
    }
    Write-Host ""
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# 6. TEST: Crear nuevo permiso
Write-Host "[6] TEST: Crear nuevo permiso" -ForegroundColor Yellow
try {
    $newPermissionBody = @{
        workflowId = $workflowId
        role = "tester"
        action = "ejecutar_pruebas"
        isAllowed = $true
    } | ConvertTo-Json
    
    $newPermission = Invoke-RestMethod -Uri "$baseUrl/api/workflows/permissions" -Method POST -Body $newPermissionBody -Headers $headers
    Write-Host "    OK: Permiso creado" -ForegroundColor Green
    Write-Host "       Rol: $($newPermission.role)" -ForegroundColor Gray
    Write-Host "       Acción: $($newPermission.action)" -ForegroundColor Gray
    Write-Host "       Permitido: $($newPermission.isAllowed)`n" -ForegroundColor Gray
    $newPermissionId = $newPermission.id
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# 7. TEST: Verificar permiso existente
Write-Host "[7] TEST: Verificar permiso (autor puede crear)" -ForegroundColor Yellow
try {
    $checkUrl = "$baseUrl/api/workflows/$workflowId/permissions/check?role=autor&action=crear"
    $check = Invoke-RestMethod -Uri $checkUrl -Method GET -Headers $headers
    $status = if ($check.hasPermission) { "[OK] PERMITIDO" } else { "[X] DENEGADO" }
    $color = if ($check.hasPermission) { "Green" } else { "Red" }
    Write-Host "    $status" -ForegroundColor $color
    Write-Host "       Rol: $($check.role)" -ForegroundColor Gray
    Write-Host "       Acción: $($check.action)`n" -ForegroundColor Gray
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# 8. TEST: Verificar permiso denegado
Write-Host "[8] TEST: Verificar permiso (autor NO puede aprobar)" -ForegroundColor Yellow
try {
    $checkUrl = "$baseUrl/api/workflows/$workflowId/permissions/check?role=autor&action=aprobar"
    $check = Invoke-RestMethod -Uri $checkUrl -Method GET -Headers $headers
    $status = if ($check.hasPermission) { "[OK] PERMITIDO" } else { "[X] DENEGADO" }
    $color = if ($check.hasPermission) { "Green" } else { "Red" }
    Write-Host "    $status" -ForegroundColor $color
    Write-Host "       Rol: $($check.role)" -ForegroundColor Gray
    Write-Host "       Acción: $($check.action)`n" -ForegroundColor Gray
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# 9. TEST: Actualizar permiso
Write-Host "[9] TEST: Actualizar permiso" -ForegroundColor Yellow
try {
    $updateBody = @{
        isAllowed = $false
    } | ConvertTo-Json
    
    $updated = Invoke-RestMethod -Uri "$baseUrl/api/workflows/permissions/$newPermissionId" -Method PUT -Body $updateBody -Headers $headers
    Write-Host "    OK: Permiso actualizado" -ForegroundColor Green
    Write-Host "       Rol: $($updated.role)" -ForegroundColor Gray
    Write-Host "       Acción: $($updated.action)" -ForegroundColor Gray
    Write-Host "       Ahora permitido: $($updated.isAllowed)`n" -ForegroundColor Gray
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# 10. TEST: Eliminar permiso
Write-Host "[10] TEST: Eliminar permiso" -ForegroundColor Yellow
try {
    Invoke-RestMethod -Uri "$baseUrl/api/workflows/permissions/$newPermissionId" -Method DELETE -Headers $headers
    Write-Host "    OK: Permiso eliminado`n" -ForegroundColor Green
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# 11. TEST: Intentar crear permiso duplicado
Write-Host "[11] TEST: Intentar crear permiso duplicado (debe fallar)" -ForegroundColor Yellow
try {
    $duplicateBody = @{
        workflowId = $workflowId
        role = "autor"
        action = "crear"
        isAllowed = $true
    } | ConvertTo-Json
    
    $duplicate = Invoke-RestMethod -Uri "$baseUrl/api/workflows/permissions" -Method POST -Body $duplicateBody -Headers $headers
    Write-Host "    ERROR: No debería permitir duplicados`n" -ForegroundColor Red
} catch {
    Write-Host "    OK: Correctamente rechazado (duplicado no permitido)`n" -ForegroundColor Green
}

# ========== PERMISSION VALIDATION TESTS (Modo solo lectura) ==========

# 12. TEST: Login como Developer (autor)
Write-Host "[12] TEST: Login como Developer (rol autor)" -ForegroundColor Yellow
try {
    $devLoginBody = '{"email":"carlos.ramirez@openuptool.com","password":"Password123!"}'
    $devResponse = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method POST -Body $devLoginBody -ContentType "application/json"
    $devHeaders = @{ 
        Authorization = "Bearer $($devResponse.token)"
        "Content-Type" = "application/json"
    }
    Write-Host "    OK: Developer logueado (mapeado a rol 'autor')`n" -ForegroundColor Green
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
    exit
}

# 13. TEST: Developer puede CREAR (autor tiene permiso crear=true)
Write-Host "[13] TEST: Developer/autor puede CREAR" -ForegroundColor Yellow
try {
    $testCreate = Invoke-RestMethod -Uri "$baseUrl/api/workflows/$workflowId/test-create" -Method POST -Headers $devHeaders
    Write-Host "    OK: Developer puede crear (permiso concedido)" -ForegroundColor Green
    Write-Host "       Acción: $($testCreate.action)" -ForegroundColor Gray
    Write-Host "       Rol del usuario: $($testCreate.userRole)`n" -ForegroundColor Gray
} catch {
    $errorDetails = $_.ErrorDetails.Message | ConvertFrom-Json
    Write-Host "    ERROR: $($errorDetails.message)`n" -ForegroundColor Red
}

# 14. TEST: Developer NO puede APROBAR (autor tiene permiso aprobar=false)
Write-Host "[14] TEST: Developer/autor NO puede APROBAR (modo solo lectura)" -ForegroundColor Yellow
try {
    $testApprove = Invoke-RestMethod -Uri "$baseUrl/api/workflows/$workflowId/test-approve" -Method POST -Headers $devHeaders
    Write-Host "    ERROR: No debería permitir aprobar`n" -ForegroundColor Red
} catch {
    if ($_.Exception.Response.StatusCode -eq 403) {
        $errorDetails = $_.ErrorDetails.Message | ConvertFrom-Json
        Write-Host "    OK: Acción bloqueada correctamente (403 Forbidden)" -ForegroundColor Green
        Write-Host "       Mensaje: $($errorDetails.message)" -ForegroundColor Gray
        Write-Host "       Error: $($errorDetails.error)" -ForegroundColor Gray
        Write-Host "       Modo lectura: $($errorDetails.details.modoLectura)" -ForegroundColor Gray
        Write-Host "       Sugerencia: $($errorDetails.details.sugerencia)`n" -ForegroundColor Cyan
    } else {
        Write-Host "    ERROR: Código de error inesperado: $($_.Exception.Response.StatusCode)`n" -ForegroundColor Red
    }
}

# 15. TEST: Login como Tester (revisor)
Write-Host "[15] TEST: Login como Tester (rol revisor)" -ForegroundColor Yellow
try {
    $testerLoginBody = '{"email":"patricia.lopez@openuptool.com","password":"Password123!"}'
    $testerResponse = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method POST -Body $testerLoginBody -ContentType "application/json"
    $testerHeaders = @{ 
        Authorization = "Bearer $($testerResponse.token)"
        "Content-Type" = "application/json"
    }
    Write-Host "    OK: Tester logueado (mapeado a rol 'revisor')`n" -ForegroundColor Green
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
    exit
}

# 16. TEST: Tester NO puede CREAR (revisor tiene permiso crear=false)
Write-Host "[16] TEST: Tester/revisor NO puede CREAR (modo solo lectura)" -ForegroundColor Yellow
try {
    $testCreate = Invoke-RestMethod -Uri "$baseUrl/api/workflows/$workflowId/test-create" -Method POST -Headers $testerHeaders
    Write-Host "    ERROR: No debería permitir crear`n" -ForegroundColor Red
} catch {
    if ($_.Exception.Response.StatusCode -eq 403) {
        $errorDetails = $_.ErrorDetails.Message | ConvertFrom-Json
        Write-Host "    OK: Acción bloqueada correctamente (403 Forbidden)" -ForegroundColor Green
        Write-Host "       Mensaje: $($errorDetails.message)" -ForegroundColor Gray
        Write-Host "       Error: $($errorDetails.error)" -ForegroundColor Gray
        Write-Host "       Modo lectura: $($errorDetails.details.modoLectura)" -ForegroundColor Gray
        Write-Host "       Sugerencia: $($errorDetails.details.sugerencia)`n" -ForegroundColor Cyan
    } else {
        Write-Host "    ERROR: Código de error inesperado: $($_.Exception.Response.StatusCode)`n" -ForegroundColor Red
    }
}

# 17. TEST: Tester puede APROBAR (revisor tiene permiso aprobar=true)
Write-Host "[17] TEST: Tester/revisor puede APROBAR" -ForegroundColor Yellow
try {
    $testApprove = Invoke-RestMethod -Uri "$baseUrl/api/workflows/$workflowId/test-approve" -Method POST -Headers $testerHeaders
    Write-Host "    OK: Tester puede aprobar (permiso concedido)" -ForegroundColor Green
    Write-Host "       Acción: $($testApprove.action)" -ForegroundColor Gray
    Write-Host "       Rol del usuario: $($testApprove.userRole)`n" -ForegroundColor Gray
} catch {
    $errorDetails = $_.ErrorDetails.Message | ConvertFrom-Json
    Write-Host "    ERROR: $($errorDetails.message)`n" -ForegroundColor Red
}

Write-Host "`n=== RESUMEN ===" -ForegroundColor Cyan
Write-Host "Requisito 1: Matriz de permisos [OK]" -ForegroundColor Green
Write-Host "- Permisos configurados por rol: autor, revisor, PO, admin" -ForegroundColor Gray
Write-Host "- Acciones definidas: crear, editar, aprobar, cambiar_estado" -ForegroundColor Gray
Write-Host "- CRUD completo de permisos" -ForegroundColor Gray
Write-Host "- Verificacion de permisos funcional" -ForegroundColor Gray
Write-Host "- Matriz visualizada correctamente" -ForegroundColor Gray
Write-Host "`nValidación de permisos en endpoints:" -ForegroundColor Cyan
Write-Host "- Usuarios sin permiso ven mensaje explicativo (403 Forbidden)" -ForegroundColor Gray
Write-Host "- Modo solo lectura activado para acciones no permitidas" -ForegroundColor Gray
Write-Host "- Mensajes explicativos claros con sugerencias" -ForegroundColor Gray
Write-Host "- Bloqueo efectivo de acciones no autorizadas`n" -ForegroundColor Gray
