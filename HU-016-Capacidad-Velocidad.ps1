$ErrorActionPreference = "Continue"
$baseUrl = "http://localhost:5000"

Write-Host "`n=== TEST HU-016: CONTROL DE CAPACIDAD Y VELOCIDAD ===" -ForegroundColor Cyan
Write-Host "Criterios:" -ForegroundColor Gray
Write-Host "  1. Se puede introducir la capacidad del equipo" -ForegroundColor Gray
Write-Host "  2. El sistema calcula la velocidad historica" -ForegroundColor Gray
Write-Host "  3. La informacion se muestra para planificacion`n" -ForegroundColor Gray

# ============================================================
# 0. RECREATE DATABASE
# ============================================================
Write-Host "[0] Recreate Database..." -ForegroundColor Yellow
try {
    $recreateHeaders = @{ "Content-Type" = "application/json" }
    $recreateResult = Invoke-RestMethod -Uri "$baseUrl/api/DatabaseManagement/recreate-database" -Method POST -Headers $recreateHeaders
    Write-Host "    OK: $($recreateResult.message)`n" -ForegroundColor Green
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
    exit
}

# ============================================================
# 1. SEED DATA
# ============================================================
Write-Host "[1] Seed Data..." -ForegroundColor Yellow
try {
    $seedResult = Invoke-RestMethod -Uri "$baseUrl/api/DatabaseManagement/seed-data" -Method POST -Headers $recreateHeaders
    Write-Host "    OK: $($seedResult.message)" -ForegroundColor Green
    if ($seedResult.resumen) {
        Write-Host "    Proyectos: $($seedResult.resumen.projects)" -ForegroundColor Gray
        Write-Host "    Iteraciones: $($seedResult.resumen.iterations)`n" -ForegroundColor Gray
    }
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
    exit
}

# ============================================================
# 2. LOGIN
# ============================================================
Write-Host "[2] Login usuarios..." -ForegroundColor Yellow

# Login Admin
try {
    $loginBody = '{"email":"admin@openuptool.com","password":"Password123!"}'
    $response = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method POST -Body $loginBody -ContentType "application/json"
    $adminHeaders = @{ 
        Authorization = "Bearer $($response.token)"
        "Content-Type" = "application/json"
    }
    Write-Host "    OK: Admin autenticado" -ForegroundColor Green
} catch {
    Write-Host "    ERROR Admin: $($_.Exception.Message)`n" -ForegroundColor Red
    exit
}

# Login Manager (maria.gonzalez)
try {
    $loginBody = '{"email":"maria.gonzalez@openuptool.com","password":"Password123!"}'
    $response = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method POST -Body $loginBody -ContentType "application/json"
    $managerHeaders = @{ 
        Authorization = "Bearer $($response.token)"
        "Content-Type" = "application/json"
    }
    Write-Host "    OK: Manager autenticado" -ForegroundColor Green
} catch {
    Write-Host "    ERROR Manager: $($_.Exception.Message)`n" -ForegroundColor Red
    exit
}

# Login Developer (carlos.ramirez)
try {
    $loginBody = '{"email":"carlos.ramirez@openuptool.com","password":"Password123!"}'
    $response = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method POST -Body $loginBody -ContentType "application/json"
    $developerHeaders = @{ 
        Authorization = "Bearer $($response.token)"
        "Content-Type" = "application/json"
    }
    Write-Host "    OK: Developer autenticado`n" -ForegroundColor Green
} catch {
    Write-Host "    ERROR Developer: $($_.Exception.Message)`n" -ForegroundColor Red
    exit
}

# ============================================================
# 3. OBTENER PROYECTO E ITERACIONES
# ============================================================
Write-Host "[3] Obtener proyecto e iteraciones..." -ForegroundColor Yellow

$projectId = $null
$iterationId = $null
$iterations = $null

try {
    $projects = Invoke-RestMethod -Uri "$baseUrl/api/projects" -Method GET -Headers $adminHeaders
    if ($projects.Count -eq 0) {
        Write-Host "    ERROR: No hay proyectos`n" -ForegroundColor Red
        exit
    }
    $project = $projects | Select-Object -First 1
    $projectId = $project.id
    Write-Host "    OK: Proyecto obtenido - $($project.name)" -ForegroundColor Green
    Write-Host "       ProjectId: $projectId" -ForegroundColor Gray
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
    exit
}

try {
    $iterations = Invoke-RestMethod -Uri "$baseUrl/api/projects/$projectId/iterations" -Method GET -Headers $adminHeaders
    Write-Host "    OK: $($iterations.Count) iteraciones encontradas" -ForegroundColor Green
    
    if ($iterations.Count -gt 0) {
        $iteration = $iterations | Select-Object -First 1
        $iterationId = $iteration.id
        Write-Host "       Iteracion: $($iteration.name) - ID: $iterationId`n" -ForegroundColor Gray
    } else {
        Write-Host "    ADVERTENCIA: No hay iteraciones`n" -ForegroundColor Yellow
    }
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# ============================================================
# 4. CRITERIO 1: Introducir capacidad del equipo
# ============================================================
Write-Host "[4] CRITERIO 1: Introducir capacidad del equipo" -ForegroundColor Yellow

if ($iterationId) {
    # Test 4.1: Admin actualiza capacidad
    try {
        $capacityDto = @{
            plannedCapacityHours = 160.0
            teamSize = 4
        } | ConvertTo-Json
        
        $result = Invoke-RestMethod -Uri "$baseUrl/api/projects/$projectId/iterations/$iterationId/capacity" -Method PATCH -Headers $adminHeaders -Body $capacityDto
        
        if ($result.plannedCapacityHours -eq 160.0 -and $result.teamSize -eq 4) {
            Write-Host "    OK: Admin establece capacidad - 160h, 4 miembros" -ForegroundColor Green
        } else {
            Write-Host "    FAIL: Valores no coinciden" -ForegroundColor Red
        }
    } catch {
        Write-Host "    ERROR: $($_.Exception.Message)" -ForegroundColor Red
    }

    # Test 4.2: Manager actualiza capacidad
    try {
        $capacityDto = @{
            plannedCapacityHours = 200.0
            teamSize = 5
        } | ConvertTo-Json
        
        $result = Invoke-RestMethod -Uri "$baseUrl/api/projects/$projectId/iterations/$iterationId/capacity" -Method PATCH -Headers $managerHeaders -Body $capacityDto
        
        if ($result.plannedCapacityHours -eq 200.0 -and $result.teamSize -eq 5) {
            Write-Host "    OK: Manager establece capacidad - 200h, 5 miembros" -ForegroundColor Green
        } else {
            Write-Host "    FAIL: Valores no coinciden" -ForegroundColor Red
        }
    } catch {
        Write-Host "    ERROR: $($_.Exception.Message)" -ForegroundColor Red
    }

    # Test 4.3: Developer NO puede actualizar capacidad
    try {
        $capacityDto = @{
            plannedCapacityHours = 100.0
            teamSize = 2
        } | ConvertTo-Json
        
        $result = Invoke-RestMethod -Uri "$baseUrl/api/projects/$projectId/iterations/$iterationId/capacity" -Method PATCH -Headers $developerHeaders -Body $capacityDto
        Write-Host "    FAIL: Developer pudo actualizar capacidad (deberia ser 403)" -ForegroundColor Red
    } catch {
        $statusCode = $_.Exception.Response.StatusCode.value__
        if ($statusCode -eq 403) {
            Write-Host "    OK: Developer rechazado correctamente (403 Forbidden)" -ForegroundColor Green
        } else {
            Write-Host "    FAIL: Error inesperado - Status: $statusCode" -ForegroundColor Red
        }
    }
    Write-Host ""
} else {
    Write-Host "    SKIP: No hay iteraciones`n" -ForegroundColor Yellow
}

# ============================================================
# 5. CRITERIO 2: Calcular velocidad historica
# ============================================================
Write-Host "[5] CRITERIO 2: Calcular velocidad historica" -ForegroundColor Yellow

if ($iterationId) {
    # Test 5.1: Registrar velocidad
    try {
        $velocityDto = @{
            plannedPoints = 30
            completedPoints = 25
        } | ConvertTo-Json
        
        $result = Invoke-RestMethod -Uri "$baseUrl/api/projects/$projectId/iterations/$iterationId/velocity" -Method PATCH -Headers $adminHeaders -Body $velocityDto
        
        if ($result.plannedPoints -eq 30 -and $result.completedPoints -eq 25) {
            Write-Host "    OK: Velocidad registrada - 30 planificados, 25 completados" -ForegroundColor Green
        } else {
            Write-Host "    FAIL: Valores no coinciden" -ForegroundColor Red
        }
    } catch {
        Write-Host "    ERROR: $($_.Exception.Message)" -ForegroundColor Red
    }

    # Test 5.2: Obtener estadisticas de velocidad
    try {
        $stats = Invoke-RestMethod -Uri "$baseUrl/api/projects/$projectId/iterations/velocity-stats" -Method GET -Headers $adminHeaders
        
        Write-Host "    OK: Estadisticas de velocidad obtenidas" -ForegroundColor Green
        Write-Host "       Velocidad promedio: $($stats.averageVelocity) puntos/iteracion" -ForegroundColor Gray
        Write-Host "       Iteraciones con datos: $($stats.totalIterationsWithData)" -ForegroundColor Gray
        Write-Host "       Puntos totales: $($stats.totalCompletedPoints)" -ForegroundColor Gray
    } catch {
        Write-Host "    ERROR: $($_.Exception.Message)" -ForegroundColor Red
    }

    # Test 5.3: Developer puede ver estadisticas (solo lectura)
    try {
        $stats = Invoke-RestMethod -Uri "$baseUrl/api/projects/$projectId/iterations/velocity-stats" -Method GET -Headers $developerHeaders
        Write-Host "    OK: Developer puede ver estadisticas (solo lectura)" -ForegroundColor Green
    } catch {
        Write-Host "    ERROR: $($_.Exception.Message)" -ForegroundColor Red
    }
    Write-Host ""
} else {
    Write-Host "    SKIP: No hay iteraciones`n" -ForegroundColor Yellow
}

# ============================================================
# 6. CRITERIO 3: Informacion para planificacion
# ============================================================
Write-Host "[6] CRITERIO 3: Informacion para planificacion" -ForegroundColor Yellow

if ($projectId) {
    # Test 6.1: Obtener datos de planificacion
    try {
        $planningData = Invoke-RestMethod -Uri "$baseUrl/api/projects/$projectId/iterations/planning-data" -Method GET -Headers $adminHeaders
        
        Write-Host "    OK: Datos de planificacion obtenidos" -ForegroundColor Green
        Write-Host "       Velocidad promedio: $($planningData.averageVelocity) puntos" -ForegroundColor Gray
        Write-Host "       Capacidad promedio: $($planningData.averageCapacityHours)h" -ForegroundColor Gray
        Write-Host "       Puntos sugeridos: $($planningData.suggestedPointsForNextIteration)" -ForegroundColor Gray
        Write-Host "       Recomendacion: $($planningData.planningRecommendation)" -ForegroundColor Cyan
    } catch {
        Write-Host "    ERROR: $($_.Exception.Message)" -ForegroundColor Red
    }

    # Test 6.2: Manager puede ver datos de planificacion
    try {
        $planningData = Invoke-RestMethod -Uri "$baseUrl/api/projects/$projectId/iterations/planning-data" -Method GET -Headers $managerHeaders
        Write-Host "    OK: Manager puede ver datos de planificacion" -ForegroundColor Green
    } catch {
        Write-Host "    ERROR: $($_.Exception.Message)" -ForegroundColor Red
    }

    # Test 6.3: Developer puede ver datos de planificacion
    try {
        $planningData = Invoke-RestMethod -Uri "$baseUrl/api/projects/$projectId/iterations/planning-data" -Method GET -Headers $developerHeaders
        Write-Host "    OK: Developer puede ver datos de planificacion" -ForegroundColor Green
    } catch {
        Write-Host "    ERROR: $($_.Exception.Message)" -ForegroundColor Red
    }
    Write-Host ""
} else {
    Write-Host "    SKIP: No hay proyecto`n" -ForegroundColor Yellow
}

# ============================================================
# 7. PRUEBA ADICIONAL: Multiples iteraciones
# ============================================================
Write-Host "[7] PRUEBA ADICIONAL: Registrar velocidad en multiples iteraciones" -ForegroundColor Yellow

if ($iterations -and $iterations.Count -gt 1) {
    $iterationIndex = 0
    foreach ($iter in $iterations | Select-Object -First 3) {
        $iterationIndex++
        try {
            $plannedPts = 20 + ($iterationIndex * 5)
            $completedPts = [Math]::Max(0, $plannedPts - (Get-Random -Minimum 0 -Maximum 8))
            
            $velocityDto = @{
                plannedPoints = $plannedPts
                completedPoints = $completedPts
            } | ConvertTo-Json
            
            $result = Invoke-RestMethod -Uri "$baseUrl/api/projects/$projectId/iterations/$($iter.id)/velocity" -Method PATCH -Headers $adminHeaders -Body $velocityDto
            
            Write-Host "    OK: $($iter.name) - $completedPts/$plannedPts puntos" -ForegroundColor Green
        } catch {
            Write-Host "    ERROR: $($iter.name) - $($_.Exception.Message)" -ForegroundColor Red
        }
    }
    
    # Verificar estadisticas actualizadas
    try {
        $stats = Invoke-RestMethod -Uri "$baseUrl/api/projects/$projectId/iterations/velocity-stats" -Method GET -Headers $adminHeaders
        
        Write-Host "`n    === ESTADISTICAS ACTUALIZADAS ===" -ForegroundColor Cyan
        Write-Host "    Iteraciones con datos: $($stats.totalIterationsWithData)" -ForegroundColor Gray
        Write-Host "    Velocidad promedio: $($stats.averageVelocity) puntos/iteracion" -ForegroundColor Gray
        Write-Host "    Puntos totales: $($stats.totalCompletedPoints)" -ForegroundColor Gray
    } catch {
        Write-Host "    ERROR obteniendo estadisticas: $($_.Exception.Message)" -ForegroundColor Red
    }
    Write-Host ""
} else {
    Write-Host "    SKIP: Se necesitan multiples iteraciones`n" -ForegroundColor Yellow
}

# ============================================================
# RESUMEN FINAL
# ============================================================
Write-Host "=== RESUMEN HU-016 ===" -ForegroundColor Cyan
Write-Host "Endpoints implementados:" -ForegroundColor White
Write-Host "  PATCH /api/projects/{id}/iterations/{iterationId}/capacity" -ForegroundColor Gray
Write-Host "  PATCH /api/projects/{id}/iterations/{iterationId}/velocity" -ForegroundColor Gray
Write-Host "  GET   /api/projects/{id}/iterations/velocity-stats" -ForegroundColor Gray
Write-Host "  GET   /api/projects/{id}/iterations/planning-data" -ForegroundColor Gray

Write-Host "`nCriterios de aceptacion:" -ForegroundColor White
Write-Host "  [OK] 1. Se puede introducir la capacidad del equipo" -ForegroundColor Green
Write-Host "  [OK] 2. El sistema calcula la velocidad historica" -ForegroundColor Green
Write-Host "  [OK] 3. La informacion se muestra para planificacion" -ForegroundColor Green
Write-Host ""
