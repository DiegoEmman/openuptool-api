$ErrorActionPreference = "Continue"
$baseUrl = "http://localhost:5000"

Write-Host "`n=== TEST CLOSURE & BUILD ===" -ForegroundColor Cyan

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
Write-Host "[3] Obtener proyecto..." -ForegroundColor Yellow
try {
    $projects = Invoke-RestMethod -Uri "$baseUrl/api/projects" -Method GET -Headers $headers
    if ($projects.Count -eq 0) {
        Write-Host "    ERROR: No hay proyectos`n" -ForegroundColor Red
        exit
    }
    $projectId = $projects[0].id
    Write-Host "    OK: $projectId`n" -ForegroundColor Green
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
    exit
}

# 4. TEST 1: Crear FinalBuild
Write-Host "[4] TEST 1: Crear FinalBuild con artefactos" -ForegroundColor Yellow
$buildBody = @{
    projectId = $projectId
    buildNumber = "v1.0.0"
    version = "1.0.0"
    buildEnvironment = "Production"
    isStable = $true
    testsPassed = 150
    testsTotal = 150
    codeCoverage = 85.5
    binaryArtifacts = @(
        @{
            name = "app.exe"
            type = "EXECUTABLE"
            downloadUrl = "https://example.com/app.exe"
            size = 2048000
            checksum = "abc123"
            checksumType = "SHA256"
        }
    )
} | ConvertTo-Json -Depth 10

try {
    $build = Invoke-RestMethod -Uri "$baseUrl/api/finalbuilds" -Method POST -Headers $headers -Body $buildBody -ContentType "application/json"
    Write-Host "    OK: $($build.buildNumber) | Artefactos: $($build.binaryArtifacts.Count)`n" -ForegroundColor Green
    $buildId = $build.id
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
    $buildId = $null
}

# 5. TEST 2: Validar cierre
Write-Host "[5] TEST 2: Validar cierre" -ForegroundColor Yellow
try {
    $validation = Invoke-RestMethod -Uri "$baseUrl/api/projectclosures/validate/$projectId" -Method GET -Headers $headers
    Write-Host "    OK: Puede cerrar=$($validation.canClose)`n" -ForegroundColor Green
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# 6. TEST 3: Crear ProjectClosure con checklist
Write-Host "[6] TEST 3: Crear ProjectClosure" -ForegroundColor Yellow
$closureBody = @{
    projectId = $projectId
    summary = "Proyecto completado exitosamente"
    lessonsLearned = "Leccion 1: La metodologia OpenUP funciono bien. Leccion 2: Las pruebas automatizadas mejoraron la calidad."
    recommendations = "Recomendacion 1: Mantener cobertura de pruebas arriba del 80%."
    checklist = @(
        @{
            criteriaId = "CRIT-001"
            name = "Artefactos entregados"
            isMandatory = $true
            isCompleted = $true
            notes = "Todos los artefactos verificados"
        },
        @{
            criteriaId = "CRIT-002"
            name = "Build final"
            isMandatory = $true
            isCompleted = $true
            notes = "Build generado y probado"
        }
    )
} | ConvertTo-Json -Depth 10

try {
    $closure = Invoke-RestMethod -Uri "$baseUrl/api/projectclosures" -Method POST -Headers $headers -Body $closureBody -ContentType "application/json"
    Write-Host "    OK: $($closure.status) | $($closure.completedCriteria)/$($closure.totalCriteria)`n" -ForegroundColor Green
    $closureId = $closure.id
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
    $closureId = $null
}

# 7. TEST 4: Aprobar cierre
if ($closureId) {
    Write-Host "[7] TEST 4: Aprobar cierre" -ForegroundColor Yellow
    $approvalBody = @{
        isApproved = $true
        comments = "Aprobado"
    } | ConvertTo-Json
    
    try {
        $approved = Invoke-RestMethod -Uri "$baseUrl/api/projectclosures/$closureId/approve" -Method POST -Headers $headers -Body $approvalBody -ContentType "application/json"
        Write-Host "    OK: $($approved.status)`n" -ForegroundColor Green
    } catch {
        Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
    }
}

# 8. TEST 5: GET todos los builds
Write-Host "[8] TEST 5: GET todos los builds" -ForegroundColor Yellow
try {
    $allBuilds = Invoke-RestMethod -Uri "$baseUrl/api/finalbuilds" -Method GET -Headers $headers
    Write-Host "    OK: Total builds: $($allBuilds.Count)`n" -ForegroundColor Green
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# 9. TEST 6: GET builds por proyecto
Write-Host "[9] TEST 6: GET builds por proyecto" -ForegroundColor Yellow
try {
    $projectBuilds = Invoke-RestMethod -Uri "$baseUrl/api/finalbuilds/project/$projectId" -Method GET -Headers $headers
    Write-Host "    OK: Builds del proyecto: $($projectBuilds.Count)`n" -ForegroundColor Green
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# 10. TEST 7: GET build por ID
if ($buildId) {
    Write-Host "[10] TEST 7: GET build por ID" -ForegroundColor Yellow
    try {
        $buildById = Invoke-RestMethod -Uri "$baseUrl/api/finalbuilds/$buildId" -Method GET -Headers $headers
        Write-Host "    OK: Build $($buildById.buildNumber)`n" -ForegroundColor Green
    } catch {
        Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
    }
}

# 11. TEST 8: GET build por numero
Write-Host "[11] TEST 8: GET build por numero" -ForegroundColor Yellow
try {
    $buildByNumber = Invoke-RestMethod -Uri "$baseUrl/api/finalbuilds/project/$projectId/number/v1.0.0" -Method GET -Headers $headers
    Write-Host "    OK: Build $($buildByNumber.version)`n" -ForegroundColor Green
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# 12. TEST 9: UPDATE build
if ($buildId) {
    Write-Host "[12] TEST 9: UPDATE build" -ForegroundColor Yellow
    $updateBuildBody = @{
        releaseNotesUrl = "https://example.com/release-notes"
        testsPassed = 160
        codeCoverage = 90.0
    } | ConvertTo-Json
    
    try {
        $updatedBuild = Invoke-RestMethod -Uri "$baseUrl/api/finalbuilds/$buildId" -Method PUT -Headers $headers -Body $updateBuildBody -ContentType "application/json"
        Write-Host "    OK: Coverage actualizado a $($updatedBuild.codeCoverage)%`n" -ForegroundColor Green
    } catch {
        Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
    }
}

# 13. TEST 10: GET todos los closures
Write-Host "[13] TEST 10: GET todos los closures" -ForegroundColor Yellow
try {
    $allClosures = Invoke-RestMethod -Uri "$baseUrl/api/projectclosures" -Method GET -Headers $headers
    Write-Host "    OK: Total closures: $($allClosures.Count)`n" -ForegroundColor Green
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# 14. TEST 11: GET closure por ID
if ($closureId) {
    Write-Host "[14] TEST 11: GET closure por ID" -ForegroundColor Yellow
    try {
        $closureById = Invoke-RestMethod -Uri "$baseUrl/api/projectclosures/$closureId" -Method GET -Headers $headers
        Write-Host "    OK: Closure status=$($closureById.status)`n" -ForegroundColor Green
    } catch {
        Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
    }
}

# 15. TEST 12: GET closures por proyecto
Write-Host "[15] TEST 12: GET closures por proyecto" -ForegroundColor Yellow
try {
    $projectClosures = Invoke-RestMethod -Uri "$baseUrl/api/projectclosures/project/$projectId" -Method GET -Headers $headers
    Write-Host "    OK: Closures del proyecto: $($projectClosures.Count)`n" -ForegroundColor Green
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# 16. TEST 13: UPDATE closure
if ($closureId) {
    Write-Host "[16] TEST 13: UPDATE closure" -ForegroundColor Yellow
    $updateClosureBody = @{
        summary = "Proyecto completado exitosamente - ACTUALIZADO"
        lessonsLearned = "Lecciones actualizadas"
        recommendations = "Recomendaciones actualizadas"
    } | ConvertTo-Json
    
    try {
        $updatedClosure = Invoke-RestMethod -Uri "$baseUrl/api/projectclosures/$closureId" -Method PUT -Headers $headers -Body $updateClosureBody -ContentType "application/json"
        Write-Host "    OK: Closure actualizado`n" -ForegroundColor Green
    } catch {
        Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
    }
}

# 17. TEST 14: DELETE build
if ($buildId) {
    Write-Host "[17] TEST 14: DELETE build" -ForegroundColor Yellow
    try {
        Invoke-RestMethod -Uri "$baseUrl/api/finalbuilds/$buildId" -Method DELETE -Headers $headers
        Write-Host "    OK: Build eliminado`n" -ForegroundColor Green
    } catch {
        Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
    }
}

Write-Host "=== COMPLETADO ===`n" -ForegroundColor Cyan
