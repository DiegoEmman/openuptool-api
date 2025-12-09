# HU-026 - Cierre del proyecto - Acceptance script
# Usage: Run in PowerShell. Ensure API is running and .env configured.

$ErrorActionPreference = "Continue"
$baseUrl = "http://localhost:5000"

Write-Host "`n=== HU-026: CIERRE DEL PROYECTO (Acceptance) ===" -ForegroundColor Cyan

# 0. Recreate Database
Write-Host "[0] Recreate Database..." -ForegroundColor Yellow
try {
    $recreateHeaders = @{ "Content-Type" = "application/json" }
    $recreateResult = Invoke-RestMethod -Uri "$baseUrl/api/DatabaseManagement/recreate-database" -Method POST -Headers $recreateHeaders -ErrorAction Stop
    Write-Host "    OK: $($recreateResult.message)`n" -ForegroundColor Green
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
    exit 1
}

# 1. Seed Data
Write-Host "[1] Seed Data..." -ForegroundColor Yellow
try {
    $seedResult = Invoke-RestMethod -Uri "$baseUrl/api/DatabaseManagement/seed-data" -Method POST -Headers $recreateHeaders -ErrorAction Stop
    Write-Host "    OK: $($seedResult.message)`n" -ForegroundColor Green
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
    exit 1
}

# 2. Login as Admin
Write-Host "[2] Login as Admin..." -ForegroundColor Yellow
try {
    $loginBody = '{"email":"admin@openuptool.com","password":"Password123!"}'
    $loginResponse = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method POST -Body $loginBody -ContentType "application/json" -ErrorAction Stop
    $adminToken = $loginResponse.token
    $adminHeaders = @{ Authorization = "Bearer $($adminToken)"; "Content-Type" = "application/json" }
    Write-Host "    OK: Admin token obtenido`n" -ForegroundColor Green
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
    exit 1
}

# 3. Login as Manager (seeded user)
Write-Host "[3] Login as Manager..." -ForegroundColor Yellow
try {
    $mgrBody = '{"email":"maria.gonzalez@openuptool.com","password":"Password123!"}'
    $mgrResp = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method POST -Body $mgrBody -ContentType "application/json" -ErrorAction Stop
    $managerToken = $mgrResp.token
    $managerHeaders = @{ Authorization = "Bearer $($managerToken)"; "Content-Type" = "application/json" }
    Write-Host "    OK: Manager token obtenido`n" -ForegroundColor Green
} catch {
    Write-Warning "Manager login failed, reusing admin token for manager actions"
    $managerHeaders = $adminHeaders
}

# 4. Get or create project
Write-Host "[4] Obtener o crear proyecto de prueba..." -ForegroundColor Yellow
try {
    $projects = Invoke-RestMethod -Uri "$baseUrl/api/projects" -Method GET -Headers $managerHeaders -ErrorAction Stop
    $projectId = $projects[0].id
    Write-Host "    OK: Usando proyecto existente: $projectId`n" -ForegroundColor Green
} catch {
    Write-Host "    WARN: No fue posible listar proyectos, intentando crear uno..." -ForegroundColor Yellow
    try {
        $createBody = @{ Name='HU026 Test Project'; Identifier='HU026'; StartDate = (Get-Date).ToString('o'); Owner = 'Test Owner'; Description = 'Acceptance project for HU-026' } | ConvertTo-Json
        $proj = Invoke-RestMethod -Uri "$baseUrl/api/projects" -Method POST -Headers $managerHeaders -Body $createBody -ContentType "application/json" -ErrorAction Stop
        $projectId = $proj.id
        Write-Host "    OK: Proyecto creado: $projectId`n" -ForegroundColor Green
    } catch {
        Write-Host "    ERROR: No se pudo crear proyecto: $($_.Exception.Message)`n" -ForegroundColor Red
        exit 1
    }
}

# 5. Ensure artifact types exist (use admin)
Write-Host "[5] Verificar tipos de artefactos..." -ForegroundColor Yellow
try {
    $types = Invoke-RestMethod -Uri "$baseUrl/api/artifact-types" -Method GET -Headers $adminHeaders -ErrorAction Stop
    Write-Host "    OK: $($types.Count) tipos de artefactos encontrados`n" -ForegroundColor Green
} catch {
    Write-Host "    WARN: No se pudo obtener tipos de artefactos: $($_.Exception.Message)`n" -ForegroundColor Yellow
}

# 6. Validate closure
Write-Host "[6] Validar cierre del proyecto..." -ForegroundColor Yellow
try {
    $validation = Invoke-RestMethod -Uri "$baseUrl/api/projectclosures/validate/$projectId" -Method GET -Headers $managerHeaders -ErrorAction Stop
    Write-Host "    OK: CanClose=$($validation.canClose); Missing=$($validation.missingMandatoryCriteria.Count)`n" -ForegroundColor Green
} catch {
    Write-Host "    ERROR: Validacion fallida: $($_.Exception.Message)`n" -ForegroundColor Red
    exit 1
}

# 7. Attempt non-forced close (expect 400 when missing)
Write-Host "[7] Intentar cierre sin force (debe fallar si faltan requisitos)..." -ForegroundColor Yellow
try {
    $body = @{ force = $false } | ConvertTo-Json
    $null = Invoke-RestMethod -Uri "$baseUrl/api/projectclosures/project/$projectId/close" -Method POST -Headers $managerHeaders -Body $body -ContentType "application/json" -ErrorAction Stop
    Write-Host "    FAIL: Cierre sin force tuvo exito inesperado`n" -ForegroundColor Red
} catch {
    Write-Host "    OK: Error esperado al cerrar sin force: $($_.Exception.Message)`n" -ForegroundColor Green
}

# 8. Attempt forced close as manager (expect 403)
Write-Host "[8] Intentar cierre forzado como manager (debe ser 403)..." -ForegroundColor Yellow
try {
    $body = @{ force = $true; justification = 'Testing forced close' } | ConvertTo-Json
    $null = Invoke-RestMethod -Uri "$baseUrl/api/projectclosures/project/$projectId/close" -Method POST -Headers $managerHeaders -Body $body -ContentType "application/json" -ErrorAction Stop
    Write-Host "    FAIL: Manager pudo forzar cierre (no esperado)`n" -ForegroundColor Red
} catch {
    Write-Host "    OK: Manager forzado prohibido: $($_.Exception.Message)`n" -ForegroundColor Green
}

# 9. Forced close as admin (should succeed)
Write-Host "[9] Intentar cierre forzado como admin..." -ForegroundColor Yellow
try {
    $body = @{ force = $true; justification = 'Admin forced close for acceptance test' } | ConvertTo-Json
    $created = Invoke-RestMethod -Uri "$baseUrl/api/projectclosures/project/$projectId/close" -Method POST -Headers $adminHeaders -Body $body -ContentType "application/json" -ErrorAction Stop
    Write-Host "    OK: Cierre creado: $($created.id)`n" -ForegroundColor Green
} catch {
    Write-Host "    ERROR: Cierre forzado admin fallo: $($_.Exception.Message)`n" -ForegroundColor Red
    exit 1
}

# 10. Check project archived
Write-Host "[10] Verificar proyecto archivado..." -ForegroundColor Yellow
try {
    $proj = Invoke-RestMethod -Uri "$baseUrl/api/projects/$projectId" -Method GET -Headers $adminHeaders -ErrorAction Stop
    Write-Host "    OK: IsArchived=$($proj.isArchived) ArchivedAt=$($proj.archivedAt)`n" -ForegroundColor Green
} catch {
    Write-Host "    WARN: No se pudo obtener proyecto: $($_.Exception.Message)`n" -ForegroundColor Yellow
}

Write-Host "=== HU-026 acceptance script finished ===`n" -ForegroundColor Cyan
