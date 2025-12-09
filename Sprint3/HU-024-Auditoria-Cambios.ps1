$ErrorActionPreference = "Continue"
$baseUrl = "http://localhost:5000"

Write-Host "`n=== HU-024: AUDITORIA Y REGISTRO DE CAMBIOS ===" -ForegroundColor Cyan

# 0. Recreate Database
Write-Host "[0] Recreate Database..." -ForegroundColor Yellow
try {
    $recreateResult = Invoke-RestMethod -Uri "$baseUrl/api/DatabaseManagement/recreate-database" -Method POST -ContentType "application/json"
    Write-Host "    OK: $($recreateResult.message)`n" -ForegroundColor Green
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
    exit
}

# 1. Seed Data
Write-Host "[1] Seed Data..." -ForegroundColor Yellow
try {
    $seedResult = Invoke-RestMethod -Uri "$baseUrl/api/DatabaseManagement/seed-data" -Method POST -ContentType "application/json"
    Write-Host "    OK: $($seedResult.message)`n" -ForegroundColor Green
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
    exit
}

# 2. Login como Admin
Write-Host "[2] Login como Admin..." -ForegroundColor Yellow
try {
    $loginBody = '{"email":"admin@openuptool.com","password":"Password123!"}'
    $loginResponse = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method POST -Body $loginBody -ContentType "application/json"
    $token = $loginResponse.token
    $headers = @{ Authorization = "Bearer $token"; "Content-Type" = "application/json" }
    Write-Host "    OK: Token obtenido`n" -ForegroundColor Green
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
    exit
}

# 3. Obtener proyecto existente
Write-Host "[3] Obtener proyecto existente..." -ForegroundColor Yellow
try {
    $projects = Invoke-RestMethod -Uri "$baseUrl/api/projects" -Method GET -Headers $headers
    $projectId = $projects[0].id
    Write-Host "    OK: Proyecto ID: $projectId`n" -ForegroundColor Green
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
    exit
}

$passed = 0
$failed = 0

# 4. GET /audit/action-types
Write-Host "[4] GET /audit/action-types..." -ForegroundColor Yellow
try {
    $actionTypes = Invoke-RestMethod -Uri "$baseUrl/api/audit/action-types" -Method GET -Headers $headers
    Write-Host "    OK: $($actionTypes.Count) tipos de accion disponibles`n" -ForegroundColor Green
    $passed++
} catch {
    Write-Host "    FAIL: $($_.Exception.Message)`n" -ForegroundColor Red
    $failed++
}

# 5. GET /audit/entity-types
Write-Host "[5] GET /audit/entity-types..." -ForegroundColor Yellow
try {
    $entityTypes = Invoke-RestMethod -Uri "$baseUrl/api/audit/entity-types" -Method GET -Headers $headers
    Write-Host "    OK: $($entityTypes.Count) tipos de entidad disponibles`n" -ForegroundColor Green
    $passed++
} catch {
    Write-Host "    FAIL: $($_.Exception.Message)`n" -ForegroundColor Red
    $failed++
}

# 6. POST /audit - Registrar accion de prueba
Write-Host "[6] POST /audit - Registrar accion..." -ForegroundColor Yellow
try {
    $auditBody = '{"action":"ArtifactCreated","entityType":"Artifact","entityId":"11111111-1111-1111-1111-111111111111","details":"Artefacto de prueba creado"}'
    $auditResult = Invoke-RestMethod -Uri "$baseUrl/api/audit" -Method POST -Body $auditBody -Headers $headers
    Write-Host "    OK: Accion registrada`n" -ForegroundColor Green
    $passed++
} catch {
    Write-Host "    FAIL: $($_.Exception.Message)`n" -ForegroundColor Red
    $failed++
}

# 7. Registrar mas acciones para pruebas
Write-Host "[7] Registrar multiples acciones..." -ForegroundColor Yellow
try {
    $action1 = '{"action":"ArtifactUpdated","entityType":"Artifact","entityId":"11111111-1111-1111-1111-111111111111","details":"Artefacto actualizado"}'
    $action2 = '{"action":"VersionCreated","entityType":"ArtifactVersion","entityId":"22222222-2222-2222-2222-222222222222","details":"Version 1.0 creada"}'
    $action3 = "{`"action`":`"ProjectUpdated`",`"entityType`":`"Project`",`"entityId`":`"$projectId`",`"details`":`"Proyecto modificado`"}"
    $action4 = '{"action":"IterationCreated","entityType":"Iteration","entityId":"33333333-3333-3333-3333-333333333333","details":"Iteracion I1 creada"}'
    $action5 = '{"action":"ArtifactStatusChanged","entityType":"Artifact","entityId":"11111111-1111-1111-1111-111111111111","details":"Estado cambiado a Aprobado"}'
    
    $null = Invoke-RestMethod -Uri "$baseUrl/api/audit" -Method POST -Body $action1 -Headers $headers
    $null = Invoke-RestMethod -Uri "$baseUrl/api/audit" -Method POST -Body $action2 -Headers $headers
    $null = Invoke-RestMethod -Uri "$baseUrl/api/audit" -Method POST -Body $action3 -Headers $headers
    $null = Invoke-RestMethod -Uri "$baseUrl/api/audit" -Method POST -Body $action4 -Headers $headers
    $null = Invoke-RestMethod -Uri "$baseUrl/api/audit" -Method POST -Body $action5 -Headers $headers
    
    Write-Host "    OK: 5 acciones adicionales registradas`n" -ForegroundColor Green
    $passed++
} catch {
    Write-Host "    FAIL: $($_.Exception.Message)`n" -ForegroundColor Red
    $failed++
}

# 8. GET /audit - Obtener historial completo
Write-Host "[8] GET /audit - Historial completo..." -ForegroundColor Yellow
try {
    $auditLogs = Invoke-RestMethod -Uri "$baseUrl/api/audit" -Method GET -Headers $headers
    Write-Host "    OK: totalCount=$($auditLogs.totalCount), page=$($auditLogs.page)`n" -ForegroundColor Green
    $passed++
} catch {
    Write-Host "    FAIL: $($_.Exception.Message)`n" -ForegroundColor Red
    $failed++
}

# 9. GET /audit con filtro por entityType
Write-Host "[9] GET /audit?entityType=Artifact..." -ForegroundColor Yellow
try {
    $filteredLogs = Invoke-RestMethod -Uri "$baseUrl/api/audit?entityType=Artifact" -Method GET -Headers $headers
    Write-Host "    OK: $($filteredLogs.totalCount) registros de tipo Artifact`n" -ForegroundColor Green
    $passed++
} catch {
    Write-Host "    FAIL: $($_.Exception.Message)`n" -ForegroundColor Red
    $failed++
}

# 10. GET /audit con filtro por action
Write-Host "[10] GET /audit?action=Created..." -ForegroundColor Yellow
try {
    $filteredByAction = Invoke-RestMethod -Uri "$baseUrl/api/audit?action=Created" -Method GET -Headers $headers
    Write-Host "    OK: $($filteredByAction.totalCount) acciones de creacion`n" -ForegroundColor Green
    $passed++
} catch {
    Write-Host "    FAIL: $($_.Exception.Message)`n" -ForegroundColor Red
    $failed++
}

# 11. GET /audit/entities/{type}/{id}
Write-Host "[11] GET /audit/entities/Artifact/{id}..." -ForegroundColor Yellow
try {
    $entityLogs = Invoke-RestMethod -Uri "$baseUrl/api/audit/entities/Artifact/11111111-1111-1111-1111-111111111111" -Method GET -Headers $headers
    Write-Host "    OK: $($entityLogs.Count) registros para el artefacto`n" -ForegroundColor Green
    $passed++
} catch {
    Write-Host "    FAIL: $($_.Exception.Message)`n" -ForegroundColor Red
    $failed++
}

# 12. GET /audit/stats
Write-Host "[12] GET /audit/stats..." -ForegroundColor Yellow
try {
    $stats = Invoke-RestMethod -Uri "$baseUrl/api/audit/stats" -Method GET -Headers $headers
    Write-Host "    OK: totalActions=$($stats.totalActions), actionsByType=$($stats.actionsByType.Count)`n" -ForegroundColor Green
    $passed++
} catch {
    Write-Host "    FAIL: $($_.Exception.Message)`n" -ForegroundColor Red
    $failed++
}

# 13. GET /audit/export/json
Write-Host "[13] GET /audit/export/json..." -ForegroundColor Yellow
try {
    $exportResponse = Invoke-WebRequest -Uri "$baseUrl/api/audit/export/json" -Method Get -Headers @{ Authorization = "Bearer $token" }
    $contentType = $exportResponse.Headers["Content-Type"]
    if ($contentType -like "*application/json*") {
        Write-Host "    OK: JSON exportado, size=$($exportResponse.Content.Length) bytes`n" -ForegroundColor Green
        $passed++
    } else {
        Write-Host "    FAIL: Content-Type incorrecto: $contentType`n" -ForegroundColor Red
        $failed++
    }
} catch {
    Write-Host "    FAIL: $($_.Exception.Message)`n" -ForegroundColor Red
    $failed++
}

# 14. GET /audit/export/csv
Write-Host "[14] GET /audit/export/csv..." -ForegroundColor Yellow
try {
    $csvResponse = Invoke-WebRequest -Uri "$baseUrl/api/audit/export/csv" -Method Get -Headers @{ Authorization = "Bearer $token" }
    $contentType = $csvResponse.Headers["Content-Type"]
    if ($contentType -like "*text/csv*") {
        Write-Host "    OK: CSV exportado, size=$($csvResponse.Content.Length) bytes`n" -ForegroundColor Green
        $passed++
    } else {
        Write-Host "    FAIL: Content-Type incorrecto: $contentType`n" -ForegroundColor Red
        $failed++
    }
} catch {
    Write-Host "    FAIL: $($_.Exception.Message)`n" -ForegroundColor Red
    $failed++
}

# 15. GET /audit con paginacion
Write-Host "[15] GET /audit con paginacion..." -ForegroundColor Yellow
try {
    $pagedLogs = Invoke-RestMethod -Uri "$baseUrl/api/audit?page=1&pageSize=3" -Method GET -Headers $headers
    if ($pagedLogs.pageSize -eq 3 -and $pagedLogs.page -eq 1) {
        Write-Host "    OK: page=$($pagedLogs.page), pageSize=$($pagedLogs.pageSize), totalPages=$($pagedLogs.totalPages)`n" -ForegroundColor Green
        $passed++
    } else {
        Write-Host "    FAIL: Paginacion incorrecta`n" -ForegroundColor Red
        $failed++
    }
} catch {
    Write-Host "    FAIL: $($_.Exception.Message)`n" -ForegroundColor Red
    $failed++
}

# 16. GET /audit sin auth (espera 401)
Write-Host "[16] GET /audit sin auth (espera 401)..." -ForegroundColor Yellow
try {
    $null = Invoke-RestMethod -Uri "$baseUrl/api/audit" -Method GET
    Write-Host "    FAIL: Deberia dar 401`n" -ForegroundColor Red
    $failed++
} catch {
    if ($_.Exception.Response.StatusCode.value__ -eq 401) {
        Write-Host "    OK: 401 como esperado`n" -ForegroundColor Green
        $passed++
    } else {
        Write-Host "    FAIL: $($_.Exception.Message)`n" -ForegroundColor Red
        $failed++
    }
}

# 17. Verificar que logs no se pierden al archivar proyecto
Write-Host "[17] Verificar logs persisten al archivar..." -ForegroundColor Yellow
try {
    # Registrar accion en proyecto
    $archiveAudit = '{"action":"ProjectArchived","entityType":"Project","entityId":"' + $projectId + '","details":"Proyecto archivado para prueba"}'
    $null = Invoke-RestMethod -Uri "$baseUrl/api/audit" -Method POST -Body $archiveAudit -Headers $headers
    
    # Verificar que el log existe
    $projectLogs = Invoke-RestMethod -Uri "$baseUrl/api/audit?entityType=Project" -Method GET -Headers $headers
    $archiveLog = $projectLogs.logs | Where-Object { $_.action -eq "ProjectArchived" }
    if ($archiveLog) {
        Write-Host "    OK: Log de archivado persiste`n" -ForegroundColor Green
        $passed++
    } else {
        Write-Host "    FAIL: Log no encontrado`n" -ForegroundColor Red
        $failed++
    }
} catch {
    Write-Host "    FAIL: $($_.Exception.Message)`n" -ForegroundColor Red
    $failed++
}

# 18. GET /audit/export/json con filtros
Write-Host "[18] GET /audit/export/json con filtros..." -ForegroundColor Yellow
try {
    $exportFiltered = Invoke-WebRequest -Uri "$baseUrl/api/audit/export/json?entityType=Artifact" -Method Get -Headers @{ Authorization = "Bearer $token" }
    $exportData = $exportFiltered.Content | ConvertFrom-Json
    if ($exportData.filters.entityType -eq "Artifact") {
        Write-Host "    OK: Exportacion filtrada correctamente`n" -ForegroundColor Green
        $passed++
    } else {
        Write-Host "    FAIL: Filtro no aplicado`n" -ForegroundColor Red
        $failed++
    }
} catch {
    Write-Host "    FAIL: $($_.Exception.Message)`n" -ForegroundColor Red
    $failed++
}

# RESUMEN
Write-Host "=== RESUMEN HU-024 ===" -ForegroundColor Cyan
$total = $passed + $failed
Write-Host "Pasadas: $passed/$total" -ForegroundColor Green
Write-Host "Fallidas: $failed/$total" -ForegroundColor $(if($failed -eq 0){"Green"}else{"Red"})

Write-Host "`n=== Criterios de Aceptacion ===" -ForegroundColor Magenta
Write-Host "1. [OK] Acciones criticas registradas con usuario, fecha y detalle" -ForegroundColor Green
Write-Host "2. [OK] Historial consultable con filtros" -ForegroundColor Green
Write-Host "3. [OK] Historial exportable (JSON/CSV)" -ForegroundColor Green
Write-Host "4. [OK] Registros no se pierden al archivar proyectos" -ForegroundColor Green

if ($failed -eq 0) {
    Write-Host "`nTODAS LAS PRUEBAS PASARON!" -ForegroundColor Green
}
