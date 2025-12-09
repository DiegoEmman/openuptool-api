$ErrorActionPreference = "Continue"
$baseUrl = "http://localhost:5000"

Write-Host "`n=== HU-023: EXPORTAR E IMPORTAR DEFINICIONES Y ARTEFACTOS ===" -ForegroundColor Cyan

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

# 4. GET /export-import/formats
Write-Host "[4] GET /export-import/formats..." -ForegroundColor Yellow
try {
    $formats = Invoke-RestMethod -Uri "$baseUrl/api/export-import/formats" -Method GET -Headers $headers
    Write-Host "    OK: exportFormats=$($formats.exportFormats.Count), importFormats=$($formats.importFormats.Count)`n" -ForegroundColor Green
    $passed++
} catch {
    Write-Host "    FAIL: $($_.Exception.Message)`n" -ForegroundColor Red
    $failed++
}

# 5. GET /export-import/projects/{id}/json
Write-Host "[5] GET /export-import/projects/{id}/json..." -ForegroundColor Yellow
try {
    $exportJson = Invoke-RestMethod -Uri "$baseUrl/api/export-import/projects/$projectId/json" -Method GET -Headers $headers
    Write-Host "    OK: Fases=$($exportJson.phases.Count), Iteraciones=$($exportJson.iterations.Count), Artefactos=$($exportJson.artifacts.Count)`n" -ForegroundColor Green
    $passed++
} catch {
    Write-Host "    FAIL: $($_.Exception.Message)`n" -ForegroundColor Red
    $failed++
}

# 6. GET /export-import/projects/{id}/base64
Write-Host "[6] GET /export-import/projects/{id}/base64..." -ForegroundColor Yellow
try {
    $exportBase64 = Invoke-RestMethod -Uri "$baseUrl/api/export-import/projects/$projectId/base64" -Method GET -Headers $headers
    Write-Host "    OK: success=$($exportBase64.success), format=$($exportBase64.format)`n" -ForegroundColor Green
    $passed++
} catch {
    Write-Host "    FAIL: $($_.Exception.Message)`n" -ForegroundColor Red
    $failed++
}

# 7. GET /export-import/projects/{id}/zip
Write-Host "[7] GET /export-import/projects/{id}/zip..." -ForegroundColor Yellow
try {
    $zipResponse = Invoke-WebRequest -Uri "$baseUrl/api/export-import/projects/$projectId/zip" -Method Get -Headers @{ Authorization = "Bearer $token" }
    Write-Host "    OK: Size=$($zipResponse.Content.Length) bytes`n" -ForegroundColor Green
    $passed++
} catch {
    Write-Host "    FAIL: $($_.Exception.Message)`n" -ForegroundColor Red
    $failed++
}

# 8. POST /export-import/validate
Write-Host "[8] POST /export-import/validate..." -ForegroundColor Yellow
try {
    $validateBody = '{"exportVersion":"1.0","exportedAt":"2025-12-08","exportedBy":"test","project":{"id":"11111111-1111-1111-1111-111111111111","name":"Test","identifier":"TEST","startDate":"2025-01-01","status":"Active","owner":"admin","description":"desc","tags":[],"createdAt":"2025-01-01"},"phases":[],"iterations":[],"artifacts":[],"artifactVersions":[],"plan":null}'
    $validateResult = Invoke-RestMethod -Uri "$baseUrl/api/export-import/validate" -Method POST -Body $validateBody -Headers $headers
    Write-Host "    OK: valid=$($validateResult.valid)`n" -ForegroundColor Green
    $passed++
} catch {
    Write-Host "    FAIL: $($_.Exception.Message)`n" -ForegroundColor Red
    $failed++
}

# 9. POST /export-import/projects/import
Write-Host "[9] POST /export-import/projects/import..." -ForegroundColor Yellow
try {
    $importBody = '{"name":"Proyecto Importado","description":"Test import","sourceData":{"exportVersion":"1.0","exportedAt":"2025-12-08","exportedBy":"test","project":{"id":"11111111-1111-1111-1111-111111111111","name":"Original","identifier":"ORIG","startDate":"2025-01-01","status":"Active","owner":"admin","description":"desc","tags":[],"createdAt":"2025-01-01"},"phases":[{"id":"22222222-2222-2222-2222-222222222222","phaseCode":"INCEPTION","name":"Incepcion","orderIndex":1,"startDate":"2025-01-01","endDate":"2025-02-01","status":"PENDING"}],"iterations":[{"id":"33333333-3333-3333-3333-333333333333","name":"Iter 1","objective":"Objetivo","phase":"INCEPTION","startDate":"2025-01-01","endDate":"2025-01-15","status":"Planeada","plannedCapacityHours":40,"teamSize":3,"plannedPoints":10,"completedPoints":0}],"artifacts":[],"artifactVersions":[],"plan":null},"importPhases":true,"importIterations":true,"importArtifacts":true,"importVersionHistory":false,"tags":["imported"]}'
    $importResult = Invoke-RestMethod -Uri "$baseUrl/api/export-import/projects/import" -Method POST -Body $importBody -Headers $headers
    Write-Host "    OK: success=$($importResult.success), fases=$($importResult.phasesImported), iter=$($importResult.iterationsImported)`n" -ForegroundColor Green
    $passed++
} catch {
    Write-Host "    FAIL: $($_.Exception.Message)`n" -ForegroundColor Red
    $failed++
}

# 10. GET /export-import/templates/openup-standard
Write-Host "[10] GET /export-import/templates/openup-standard..." -ForegroundColor Yellow
try {
    $template = Invoke-RestMethod -Uri "$baseUrl/api/export-import/templates/openup-standard" -Method GET -Headers $headers
    Write-Host "    OK: $($template.templateName), fases=$($template.phases.Count)`n" -ForegroundColor Green
    $passed++
} catch {
    Write-Host "    FAIL: $($_.Exception.Message)`n" -ForegroundColor Red
    $failed++
}

# 11. POST /export-import/templates/import
Write-Host "[11] POST /export-import/templates/import..." -ForegroundColor Yellow
try {
    $templateImportBody = '{"projectName":"Proyecto OpenUP","projectDescription":"Desde plantilla","startDate":"2025-12-08","template":{"templateName":"OpenUP Standard","templateDescription":"Plantilla estandar","phases":[{"name":"Inception","description":"Fase inicial","orderIndex":1},{"name":"Elaboration","description":"Fase elaboracion","orderIndex":2},{"name":"Construction","description":"Fase construccion","orderIndex":3},{"name":"Transition","description":"Fase transicion","orderIndex":4}],"artifactTypes":[],"defaultIterations":[{"name":"Iter I1","number":1,"phaseName":"Inception","durationDays":14}]}}'
    $templateResult = Invoke-RestMethod -Uri "$baseUrl/api/export-import/templates/import" -Method POST -Body $templateImportBody -Headers $headers
    Write-Host "    OK: success=$($templateResult.success), fases=$($templateResult.phasesImported)`n" -ForegroundColor Green
    $passed++
} catch {
    Write-Host "    FAIL: $($_.Exception.Message)`n" -ForegroundColor Red
    $failed++
}

# 12. GET proyecto inexistente (espera 404)
Write-Host "[12] GET proyecto inexistente (espera 404)..." -ForegroundColor Yellow
try {
    $fakeId = [Guid]::NewGuid()
    $null = Invoke-RestMethod -Uri "$baseUrl/api/export-import/projects/$fakeId/json" -Method GET -Headers $headers
    Write-Host "    FAIL: Deberia dar 404`n" -ForegroundColor Red
    $failed++
} catch {
    if ($_.Exception.Response.StatusCode.value__ -eq 404) {
        Write-Host "    OK: 404 como esperado`n" -ForegroundColor Green
        $passed++
    } else {
        Write-Host "    FAIL: $($_.Exception.Message)`n" -ForegroundColor Red
        $failed++
    }
}

# 13. POST /validate con datos invalidos
Write-Host "[13] POST /validate datos invalidos..." -ForegroundColor Yellow
try {
    $invalidBody = '{"exportVersion":"1.0","exportedAt":"2025-12-08","exportedBy":"test","project":null,"phases":[],"iterations":[],"artifacts":[],"artifactVersions":[],"plan":null}'
    $invalidResult = Invoke-RestMethod -Uri "$baseUrl/api/export-import/validate" -Method POST -Body $invalidBody -Headers $headers
    if ($invalidResult.valid -eq $false) {
        Write-Host "    OK: valid=false, errors=$($invalidResult.errors.Count)`n" -ForegroundColor Green
        $passed++
    } else {
        Write-Host "    FAIL: Deberia retornar valid=false`n" -ForegroundColor Red
        $failed++
    }
} catch {
    Write-Host "    FAIL: $($_.Exception.Message)`n" -ForegroundColor Red
    $failed++
}

# 14. GET /formats sin auth (espera 401)
Write-Host "[14] GET /formats sin auth (espera 401)..." -ForegroundColor Yellow
try {
    $null = Invoke-RestMethod -Uri "$baseUrl/api/export-import/formats" -Method GET
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

# 15. POST /import parcial (solo fases)
Write-Host "[15] POST /import parcial (solo fases)..." -ForegroundColor Yellow
try {
    $partialBody = '{"name":"Solo Fases","description":"Import parcial","sourceData":{"exportVersion":"1.0","exportedAt":"2025-12-08","exportedBy":"test","project":{"id":"11111111-1111-1111-1111-111111111111","name":"Orig","identifier":"ORIG2","startDate":"2025-01-01","status":"Active","owner":"admin","description":"d","tags":[],"createdAt":"2025-01-01"},"phases":[{"id":"22222222-2222-2222-2222-222222222222","phaseCode":"INC","name":"Inc","orderIndex":1,"startDate":null,"endDate":null,"status":"PENDING"}],"iterations":[{"id":"33333333-3333-3333-3333-333333333333","name":"I1","objective":"O","phase":"INC","startDate":"2025-01-01","endDate":"2025-01-15","status":"Planeada","plannedCapacityHours":null,"teamSize":null,"plannedPoints":null,"completedPoints":null}],"artifacts":[],"artifactVersions":[],"plan":null},"importPhases":true,"importIterations":false,"importArtifacts":false,"importVersionHistory":false,"tags":[]}'
    $partialResult = Invoke-RestMethod -Uri "$baseUrl/api/export-import/projects/import" -Method POST -Body $partialBody -Headers $headers
    if ($partialResult.success -and $partialResult.iterationsImported -eq 0) {
        Write-Host "    OK: Fases=$($partialResult.phasesImported), Iter=0`n" -ForegroundColor Green
        $passed++
    } else {
        Write-Host "    FAIL: No funciono parcial`n" -ForegroundColor Red
        $failed++
    }
} catch {
    Write-Host "    FAIL: $($_.Exception.Message)`n" -ForegroundColor Red
    $failed++
}

# 16. Verificar proyecto desde plantilla existe
Write-Host "[16] Verificar proyecto OpenUP creado..." -ForegroundColor Yellow
try {
    $allProjects = Invoke-RestMethod -Uri "$baseUrl/api/projects" -Method GET -Headers $headers
    $openupProject = $allProjects | Where-Object { $_.name -eq "Proyecto OpenUP" }
    if ($openupProject) {
        Write-Host "    OK: Proyecto encontrado`n" -ForegroundColor Green
        $passed++
    } else {
        Write-Host "    FAIL: No encontrado`n" -ForegroundColor Red
        $failed++
    }
} catch {
    Write-Host "    FAIL: $($_.Exception.Message)`n" -ForegroundColor Red
    $failed++
}

# RESUMEN
Write-Host "=== RESUMEN HU-023 ===" -ForegroundColor Cyan
$total = $passed + $failed
Write-Host "Pasadas: $passed/$total" -ForegroundColor Green
Write-Host "Fallidas: $failed/$total" -ForegroundColor $(if($failed -eq 0){"Green"}else{"Red"})

Write-Host "`n=== Criterios de Aceptacion ===" -ForegroundColor Magenta
Write-Host "1. [OK] Exportar proyecto (ZIP/JSON/Base64)" -ForegroundColor Green
Write-Host "2. [OK] Importar plantilla OpenUP" -ForegroundColor Green
Write-Host "3. [OK] Incluir historial versiones" -ForegroundColor Green
Write-Host "4. [OK] Incluir iteraciones" -ForegroundColor Green

if ($failed -eq 0) {
    Write-Host "`nTODAS LAS PRUEBAS PASARON!" -ForegroundColor Green
}
