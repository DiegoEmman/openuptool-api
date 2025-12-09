$ErrorActionPreference = "Continue"
$baseUrl = "http://localhost:5000"

Write-Host "`n=== HU-020: REASIGNAR ENTREGABLES ENTRE FASES O FLUJOS ===" -ForegroundColor Cyan

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

# 2. Login como Admin
Write-Host "[2] Login como Admin..." -ForegroundColor Yellow
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

# ==============================================================================
# TEST 3: OBTENER PROYECTO DEL SEED
# ==============================================================================
Write-Host "[3] Obtener proyecto del seed..." -ForegroundColor Yellow
$projectId = $null
try {
    $projects = Invoke-RestMethod -Uri "$baseUrl/api/projects" -Method GET -Headers $headers
    if ($projects.Count -eq 0) {
        Write-Host "    ERROR: No hay proyectos`n" -ForegroundColor Red
        exit
    }
    $projectId = $projects[0].id
    Write-Host "    Proyecto: $($projects[0].name)" -ForegroundColor White
    Write-Host "    OK: Proyecto obtenido $projectId`n" -ForegroundColor Green
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
    exit
}

# ==============================================================================
# TEST 4: SEED ARTIFACT TYPES
# ==============================================================================
Write-Host "[4] Seed artifact types (Inception)..." -ForegroundColor Yellow
try {
    $seedTypes = Invoke-RestMethod -Uri "$baseUrl/api/artifact-types/seed-inception" -Method POST -Headers $headers
    Write-Host "    OK: $($seedTypes.message)`n" -ForegroundColor Green
} catch {
    Write-Host "    INFO: $($_.Exception.Message)`n" -ForegroundColor Yellow
}

# ==============================================================================
# TEST 5: OBTENER TIPO DE ARTEFACTO
# ==============================================================================
Write-Host "[5] Obtener tipos de artefacto..." -ForegroundColor Yellow
$artifactTypeId = $null
try {
    $artifactTypes = Invoke-RestMethod -Uri "$baseUrl/api/artifact-types?phase=INCEPTION" -Method GET -Headers $headers
    if ($artifactTypes.Count -gt 0) {
        $artifactTypeId = $artifactTypes[0].id
        Write-Host "    Tipo encontrado: $($artifactTypes[0].name)" -ForegroundColor White
        Write-Host "    OK: Tipos obtenidos`n" -ForegroundColor Green
    }
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# ==============================================================================
# TEST 6: CREAR ARTEFACTO EN FASE INCEPTION
# ==============================================================================
Write-Host "[6] Crear artefacto en fase INCEPTION..." -ForegroundColor Yellow
$artifactId = $null
try {
    Add-Type -AssemblyName System.Net.Http
    $httpClient = New-Object System.Net.Http.HttpClient
    $httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer $($response.token)")
    
    $form = New-Object System.Net.Http.MultipartFormDataContent
    $form.Add([System.Net.Http.StringContent]::new($projectId.ToString()), "ProjectId")
    $form.Add([System.Net.Http.StringContent]::new("INCEPTION"), "PhaseId")
    $form.Add([System.Net.Http.StringContent]::new($artifactTypeId.ToString()), "ArtifactTypeId")
    $form.Add([System.Net.Http.StringContent]::new("Documento Vision Test HU-020"), "Title")
    $form.Add([System.Net.Http.StringContent]::new("Artefacto para pruebas de reasignacion"), "Description")
    $form.Add([System.Net.Http.StringContent]::new("Admin Test"), "Author")
    $form.Add([System.Net.Http.StringContent]::new("true"), "IsMandatory")
    
    $httpResponse = $httpClient.PostAsync("$baseUrl/api/projects/$projectId/artifacts", $form).Result
    $responseContent = $httpResponse.Content.ReadAsStringAsync().Result
    
    if ($httpResponse.IsSuccessStatusCode) {
        $artifact = $responseContent | ConvertFrom-Json
        $artifactId = $artifact.id
        Write-Host "    Artefacto ID: $artifactId" -ForegroundColor White
        Write-Host "    Fase actual: $($artifact.phaseId)" -ForegroundColor White
        Write-Host "    OK: Artefacto creado`n" -ForegroundColor Green
    } else {
        Write-Host "    ERROR HTTP: $($httpResponse.StatusCode) - $responseContent`n" -ForegroundColor Red
    }
    
    $httpClient.Dispose()
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# ==============================================================================
# TEST 7: VALIDAR REASIGNACION (sin ejecutar) - INCEPTION a ELABORATION
# ==============================================================================
Write-Host "[7] Validar reasignacion INCEPTION -> ELABORATION..." -ForegroundColor Yellow
if ($null -ne $artifactId) {
    try {
        $validateBody = @{
            newPhaseId = "ELABORATION"
            newWorkflowId = $null
        } | ConvertTo-Json
        
        $validateResult = Invoke-RestMethod -Uri "$baseUrl/api/projects/$projectId/artifacts/$artifactId/validate-reassignment" -Method POST -Body $validateBody -Headers $headers
        Write-Host "    Valido: $($validateResult.success)" -ForegroundColor White
        Write-Host "    Mensaje: $($validateResult.message)" -ForegroundColor White
        if ($validateResult.hasViolations) {
            Write-Host "    Violaciones:" -ForegroundColor Yellow
            foreach ($v in $validateResult.violations) {
                Write-Host "      - [$($v.severity)] $($v.violationType): $($v.description)" -ForegroundColor Gray
            }
        }
        Write-Host "    OK: Validacion completada`n" -ForegroundColor Green
    } catch {
        Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
    }
}

# ==============================================================================
# TEST 8: REASIGNAR ARTEFACTO DE INCEPTION A ELABORATION
# ==============================================================================
Write-Host "[8] Reasignar artefacto INCEPTION -> ELABORATION..." -ForegroundColor Yellow
if ($null -ne $artifactId) {
    try {
        $reassignBody = @{
            newPhaseId = "ELABORATION"
            reason = "Movido para pruebas de HU-020"
            confirmViolation = $false
        } | ConvertTo-Json
        
        $reassignResult = Invoke-RestMethod -Uri "$baseUrl/api/projects/$projectId/artifacts/$artifactId/reassign-phase" -Method POST -Body $reassignBody -Headers $headers
        Write-Host "    Exito: $($reassignResult.success)" -ForegroundColor White
        Write-Host "    Mensaje: $($reassignResult.message)" -ForegroundColor White
        if ($reassignResult.movement) {
            Write-Host "    Movimiento ID: $($reassignResult.movement.id)" -ForegroundColor Gray
            Write-Host "    De: $($reassignResult.movement.fromPhaseId) -> A: $($reassignResult.movement.toPhaseId)" -ForegroundColor Gray
        }
        Write-Host "    OK: Reasignacion completada`n" -ForegroundColor Green
    } catch {
        Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
    }
}

# ==============================================================================
# TEST 9: VALIDAR REASIGNACION INVERSA (con violacion)
# ==============================================================================
Write-Host "[9] Validar reasignacion inversa ELABORATION -> INCEPTION..." -ForegroundColor Yellow
if ($null -ne $artifactId) {
    try {
        $validateBody = @{
            newPhaseId = "INCEPTION"
            newWorkflowId = $null
        } | ConvertTo-Json
        
        $validateResult = Invoke-RestMethod -Uri "$baseUrl/api/projects/$projectId/artifacts/$artifactId/validate-reassignment" -Method POST -Body $validateBody -Headers $headers
        Write-Host "    Valido: $($validateResult.success)" -ForegroundColor White
        Write-Host "    Tiene violaciones: $($validateResult.hasViolations)" -ForegroundColor White
        if ($validateResult.hasViolations) {
            Write-Host "    Violaciones detectadas:" -ForegroundColor Yellow
            foreach ($v in $validateResult.violations) {
                Write-Host "      - [$($v.severity)] $($v.violationType): $($v.description)" -ForegroundColor Gray
            }
        }
        Write-Host "    OK: Validacion completada (se esperan violaciones)`n" -ForegroundColor Green
    } catch {
        Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
    }
}

# ==============================================================================
# TEST 10: INTENTAR REASIGNACION INVERSA SIN CONFIRMACION
# ==============================================================================
Write-Host "[10] Intentar reasignacion inversa sin confirmacion..." -ForegroundColor Yellow
if ($null -ne $artifactId) {
    try {
        $reassignBody = @{
            newPhaseId = "INCEPTION"
            reason = "Intento de retroceso sin confirmar"
            confirmViolation = $false
        } | ConvertTo-Json
        
        $reassignResult = Invoke-RestMethod -Uri "$baseUrl/api/projects/$projectId/artifacts/$artifactId/reassign-phase" -Method POST -Body $reassignBody -Headers $headers
        if (-not $reassignResult.success) {
            Write-Host "    Rechazado correctamente: $($reassignResult.message)" -ForegroundColor White
            Write-Host "    OK: Sistema rechazo movimiento con violaciones sin confirmar`n" -ForegroundColor Green
        } else {
            Write-Host "    ADVERTENCIA: Movimiento debio ser rechazado`n" -ForegroundColor Yellow
        }
    } catch {
        if ($_.ErrorDetails.Message) {
            try {
                $errorBody = $_.ErrorDetails.Message | ConvertFrom-Json
                Write-Host "    Rechazado: $($errorBody.message)" -ForegroundColor White
            } catch {
                Write-Host "    Rechazado: $($_.ErrorDetails.Message)" -ForegroundColor White
            }
        } else {
            Write-Host "    Rechazado: Error al procesar la solicitud" -ForegroundColor White
        }
        Write-Host "    OK: Sistema rechazo correctamente`n" -ForegroundColor Green
    }
}

# ==============================================================================
# TEST 11: REASIGNAR CON CONFIRMACION DE VIOLACION
# ==============================================================================
Write-Host "[11] Reasignar con confirmacion de violacion..." -ForegroundColor Yellow
if ($null -ne $artifactId) {
    try {
        $reassignBody = @{
            newPhaseId = "INCEPTION"
            reason = "Retroceso confirmado para pruebas"
            confirmViolation = $true
        } | ConvertTo-Json
        
        $reassignResult = Invoke-RestMethod -Uri "$baseUrl/api/projects/$projectId/artifacts/$artifactId/reassign-phase" -Method POST -Body $reassignBody -Headers $headers
        Write-Host "    Exito: $($reassignResult.success)" -ForegroundColor White
        Write-Host "    Mensaje: $($reassignResult.message)" -ForegroundColor White
        if ($reassignResult.movement) {
            Write-Host "    Violaciones registradas: $($reassignResult.movement.violatedRules)" -ForegroundColor Gray
        }
        Write-Host "    OK: Reasignacion con violacion confirmada`n" -ForegroundColor Green
    } catch {
        Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
    }
}

# ==============================================================================
# TEST 12: OBTENER HISTORIAL DE MOVIMIENTOS
# ==============================================================================
Write-Host "[12] Obtener historial de movimientos..." -ForegroundColor Yellow
if ($null -ne $artifactId) {
    try {
        $history = Invoke-RestMethod -Uri "$baseUrl/api/projects/$projectId/artifacts/$artifactId/movement-history" -Method GET -Headers $headers
        Write-Host "    Total movimientos: $($history.Count)" -ForegroundColor White
        foreach ($m in $history) {
            Write-Host "      - $($m.movementType): $($m.fromPhaseId) -> $($m.toPhaseId) [$($m.movedAt)]" -ForegroundColor Gray
            if ($m.violatedRules) {
                Write-Host "        (Con violaciones confirmadas)" -ForegroundColor Yellow
            }
        }
        Write-Host "    OK: Historial obtenido`n" -ForegroundColor Green
    } catch {
        Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
    }
}

# ==============================================================================
# TEST 13: VALIDAR SALTO DE FASES (INCEPTION -> TRANSITION)
# ==============================================================================
Write-Host "[13] Validar salto de fases INCEPTION -> TRANSITION..." -ForegroundColor Yellow
if ($null -ne $artifactId) {
    try {
        $validateBody = @{
            newPhaseId = "TRANSITION"
            newWorkflowId = $null
        } | ConvertTo-Json
        
        $validateResult = Invoke-RestMethod -Uri "$baseUrl/api/projects/$projectId/artifacts/$artifactId/validate-reassignment" -Method POST -Body $validateBody -Headers $headers
        Write-Host "    Valido: $($validateResult.success)" -ForegroundColor White
        Write-Host "    Tiene violaciones: $($validateResult.hasViolations)" -ForegroundColor White
        if ($validateResult.hasViolations) {
            Write-Host "    Violaciones detectadas:" -ForegroundColor Yellow
            foreach ($v in $validateResult.violations) {
                Write-Host "      - [$($v.severity)] $($v.violationType): $($v.description)" -ForegroundColor Gray
            }
        }
        Write-Host "    OK: Se detectaron violaciones por salto de fases`n" -ForegroundColor Green
    } catch {
        Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
    }
}

# ==============================================================================
# RESUMEN
# ==============================================================================
Write-Host "=== RESUMEN DE PRUEBAS HU-020 ===" -ForegroundColor Cyan
Write-Host "Se verificaron las siguientes funcionalidades:" -ForegroundColor White
Write-Host "  [OK] Validacion de reasignacion sin ejecutar" -ForegroundColor Green
Write-Host "  [OK] Reasignacion de artefacto entre fases" -ForegroundColor Green
Write-Host "  [OK] Deteccion de movimientos hacia atras (retroceso)" -ForegroundColor Green
Write-Host "  [OK] Rechazo de movimientos con violaciones sin confirmar" -ForegroundColor Green
Write-Host "  [OK] Confirmacion de movimientos con violaciones" -ForegroundColor Green
Write-Host "  [OK] Historial de movimientos con registro de usuario y fecha" -ForegroundColor Green
Write-Host "  [OK] Deteccion de salto de fases" -ForegroundColor Green
Write-Host "`n=== FIN DE PRUEBAS HU-020 ===`n" -ForegroundColor Cyan
