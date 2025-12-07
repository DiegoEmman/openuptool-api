$ErrorActionPreference = "Continue"
$baseUrl = "http://localhost:5000"

Write-Host "`n=== HU-002: ELIMINAR/ARCHIVAR PROYECTO ===" -ForegroundColor Cyan

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
    Write-Host "    OK: $($seedResult.message)`n" -ForegroundColor Green
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
    exit
}

# 2. Login
Write-Host "[2] Login..." -ForegroundColor Yellow
try {
    $loginBody = '{"email":"admin@openuptool.com","password":"Password123!"}'
    $response = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method POST -Body $loginBody -ContentType "application/json"
    $headers = @{ Authorization = "Bearer $($response.token)" }
    Write-Host "    OK: Token obtenido`n" -ForegroundColor Green
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
    exit
}

# 3. Obtener proyecto del seed
Write-Host "[3] Obtener proyecto del seed..." -ForegroundColor Yellow
try {
    $projects = Invoke-RestMethod -Uri "$baseUrl/api/projects" -Method GET -Headers $headers
    if ($projects.Count -eq 0) {
        Write-Host "    ERROR: No hay proyectos`n" -ForegroundColor Red
        exit
    }
    $projectId = $projects[0].id
    Write-Host "    OK: Proyecto obtenido $projectId`n" -ForegroundColor Green
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
    exit
}

# 4. TEST 1: Archivar proyecto
Write-Host "[4] TEST 1: Archivar proyecto" -ForegroundColor Yellow
try {
    $archived = Invoke-RestMethod -Uri "$baseUrl/api/projects/$projectId/archive" -Method POST -Headers $headers
    Write-Host "    OK: Archivado=$($archived.isArchived)`n" -ForegroundColor Green
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# 5. TEST 2: GET proyectos archivados
Write-Host "[5] TEST 2: GET proyectos archivados" -ForegroundColor Yellow
try {
    $archivedProjects = Invoke-RestMethod -Uri "$baseUrl/api/projects/archived" -Method GET -Headers $headers
    Write-Host "    OK: Total archivados: $($archivedProjects.Count)`n" -ForegroundColor Green
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# 6. TEST 3: GET audit logs
Write-Host "[6] TEST 3: GET audit logs del proyecto" -ForegroundColor Yellow
try {
    $auditLogs = Invoke-RestMethod -Uri "$baseUrl/api/projects/$projectId/audit-logs" -Method GET -Headers $headers
    Write-Host "    OK: Logs de auditoria: $($auditLogs.Count)`n" -ForegroundColor Green
    foreach ($log in $auditLogs | Select-Object -First 3) {
        Write-Host "       - $($log.action) por $($log.userName)" -ForegroundColor Gray
    }
    Write-Host ""
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# 7. TEST 4: Desarchivar proyecto
Write-Host "[7] TEST 4: Desarchivar proyecto" -ForegroundColor Yellow
try {
    $unarchived = Invoke-RestMethod -Uri "$baseUrl/api/projects/$projectId/unarchive" -Method POST -Headers $headers
    Write-Host "    OK: Desarchivado=$($unarchived.isArchived)`n" -ForegroundColor Green
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# 8. TEST 5: Volver a archivar
Write-Host "[8] TEST 5: Volver a archivar" -ForegroundColor Yellow
try {
    $archived = Invoke-RestMethod -Uri "$baseUrl/api/projects/$projectId/archive" -Method POST -Headers $headers
    Write-Host "    OK: Archivado nuevamente`n" -ForegroundColor Green
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# 9. TEST 6: Intentar eliminar sin confirmacion
Write-Host "[9] TEST 6: Intentar eliminar sin confirmacion" -ForegroundColor Yellow
try {
    Invoke-RestMethod -Uri "$baseUrl/api/projects/$projectId/permanent" -Method DELETE -Headers $headers
    Write-Host "    ERROR: No debio permitir eliminar sin confirmacion`n" -ForegroundColor Red
} catch {
    Write-Host "    OK: Eliminacion bloqueada correctamente`n" -ForegroundColor Green
}

# 10. TEST 7: Eliminar con confirmacion
Write-Host "[10] TEST 7: Eliminar permanentemente con confirmacion" -ForegroundColor Yellow
try {
    Invoke-RestMethod -Uri "$baseUrl/api/projects/$projectId/permanent?confirm=true" -Method DELETE -Headers $headers
    Write-Host "    OK: Proyecto eliminado permanentemente`n" -ForegroundColor Green
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# 11. TEST 8: Verificar que ya no existe
Write-Host "[11] TEST 8: Verificar que proyecto ya no existe" -ForegroundColor Yellow
try {
    Invoke-RestMethod -Uri "$baseUrl/api/projects/$projectId" -Method GET -Headers $headers
    Write-Host "    ERROR: Proyecto todavia existe`n" -ForegroundColor Red
} catch {
    Write-Host "    OK: Proyecto ya no existe (404 esperado)`n" -ForegroundColor Green
}

Write-Host "=== COMPLETADO ===`n" -ForegroundColor Cyan
