$ErrorActionPreference = "Continue"
$baseUrl = "http://localhost:5000"

Write-Host "`n=== TEST MICROINCREMENTOS ===" -ForegroundColor Cyan

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
    Write-Host "    OK: $projectId`n" -ForegroundColor Green
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
    exit
}

# 4. Obtener iteraciones del seed
Write-Host "[4] Obtener iteraciones..." -ForegroundColor Yellow
try {
    $iterations = Invoke-RestMethod -Uri "$baseUrl/api/projects/$projectId/iterations" -Method GET -Headers $headers
    if ($iterations.Count -eq 0) {
        Write-Host "    ERROR: No hay iteraciones`n" -ForegroundColor Red
        exit
    }
    $iterationId = $iterations[0].id
    Write-Host "    OK: $iterationId`n" -ForegroundColor Green
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
    exit
}

# 5. Usar artefacto del seed (ID fijo)
Write-Host "[5] Usando artefacto del seed..." -ForegroundColor Yellow
$artifactId = "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"
Write-Host "    OK: $artifactId`n" -ForegroundColor Green

# 6. TEST 1: Crear Microincremento
Write-Host "[6] TEST 1: Crear Microincremento" -ForegroundColor Yellow
$microBody = @{
    title = "Implementar autenticacion JWT"
    description = "Desarrollo del sistema de autenticacion con tokens JWT"
    artifactId = $artifactId
    date = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
    author = "Admin Sistema"
} | ConvertTo-Json

try {
    $micro1 = Invoke-RestMethod -Uri "$baseUrl/api/microincrements" -Method POST -Headers $headers -Body $microBody -ContentType "application/json"
    Write-Host "    OK: Microincremento creado - $($micro1.title)`n" -ForegroundColor Green
    $microId1 = $micro1.id
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
    $microId1 = $null
}

# 7. TEST 2: Crear segundo Microincremento
Write-Host "[7] TEST 2: Crear segundo Microincremento" -ForegroundColor Yellow
$microBody2 = @{
    title = "Crear endpoints de usuario"
    description = "Implementacion de CRUD de usuarios"
    artifactId = $artifactId
    date = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
    author = "Admin Sistema"
} | ConvertTo-Json

try {
    $micro2 = Invoke-RestMethod -Uri "$baseUrl/api/microincrements" -Method POST -Headers $headers -Body $microBody2 -ContentType "application/json"
    Write-Host "    OK: Microincremento creado - $($micro2.title)`n" -ForegroundColor Green
    $microId2 = $micro2.id
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
    $microId2 = $null
}

# 8. TEST 3: GET todos los microincrementos
Write-Host "[8] TEST 3: GET todos los microincrementos" -ForegroundColor Yellow
try {
    $allMicros = Invoke-RestMethod -Uri "$baseUrl/api/microincrements" -Method GET -Headers $headers
    Write-Host "    OK: Total microincrementos: $($allMicros.Count)`n" -ForegroundColor Green
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# 9. TEST 4: GET microincremento por ID
if ($microId1) {
    Write-Host "[9] TEST 4: GET microincremento por ID" -ForegroundColor Yellow
    try {
        $microById = Invoke-RestMethod -Uri "$baseUrl/api/microincrements/$microId1" -Method GET -Headers $headers
        Write-Host "    OK: $($microById.title)`n" -ForegroundColor Green
    } catch {
        Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
    }
}

# 10. TEST 5: Filtrar por iteración
Write-Host "[10] TEST 5: Filtrar por iteración" -ForegroundColor Yellow
try {
    $byIteration = Invoke-RestMethod -Uri "$baseUrl/api/microincrements/iteration/$iterationId" -Method GET -Headers $headers
    Write-Host "    OK: Microincrementos en iteración: $($byIteration.Count)`n" -ForegroundColor Green
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# 11. TEST 6: Filtrar por artefacto
Write-Host "[11] TEST 6: Filtrar por artefacto" -ForegroundColor Yellow
try {
    $byArtifact = Invoke-RestMethod -Uri "$baseUrl/api/microincrements/artifact/$artifactId" -Method GET -Headers $headers
    Write-Host "    OK: Microincrementos en artefacto: $($byArtifact.Count)`n" -ForegroundColor Green
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# 12. TEST 7: Filtrar por autor
Write-Host "[12] TEST 7: Filtrar por autor" -ForegroundColor Yellow
try {
    $author = "Admin Sistema"
    $byAuthor = Invoke-RestMethod -Uri "$baseUrl/api/microincrements/author/$([System.Web.HttpUtility]::UrlEncode($author))" -Method GET -Headers $headers
    Write-Host "    OK: Microincrementos del autor: $($byAuthor.Count)`n" -ForegroundColor Green
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# 13. TEST 8: UPDATE microincremento
if ($microId1) {
    Write-Host "[13] TEST 8: UPDATE microincremento" -ForegroundColor Yellow
    $updateBody = @{
        title = "Implementar autenticacion JWT - ACTUALIZADO"
        description = "Sistema de autenticacion completado y testeado"
    } | ConvertTo-Json
    
    try {
        $updated = Invoke-RestMethod -Uri "$baseUrl/api/microincrements/$microId1" -Method PUT -Headers $headers -Body $updateBody -ContentType "application/json"
        Write-Host "    OK: Microincremento actualizado`n" -ForegroundColor Green
    } catch {
        Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
    }
}

# 14. TEST 9: DELETE microincremento
if ($microId2) {
    Write-Host "[14] TEST 9: DELETE microincremento" -ForegroundColor Yellow
    try {
        Invoke-RestMethod -Uri "$baseUrl/api/microincrements/$microId2" -Method DELETE -Headers $headers
        Write-Host "    OK: Microincremento eliminado`n" -ForegroundColor Green
    } catch {
        Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
    }
}

Write-Host "=== COMPLETADO ===`n" -ForegroundColor Cyan
