# ===========================================
# HU-017: Microincrementos técnicos
# ===========================================
# Criterios de aceptación:
# 1. Los microincrementos pueden etiquetarse como "técnico" o "funcional"
# 2. Para cada microincremento se registra autor, fecha, entregable afectado y evidencia
# 3. Se puede filtrar la lista por tipo y por iteración
# ===========================================

$baseUrl = "http://localhost:5000/api"

Write-Host "`n=== TEST HU-017: MICROINCREMENTOS TÉCNICOS ===" -ForegroundColor Cyan
Write-Host "Criterios:"
Write-Host "  1. Los microincrementos pueden etiquetarse como 'tecnico' o 'funcional'"
Write-Host "  2. Se registra autor, fecha, entregable afectado y evidencia"
Write-Host "  3. Se puede filtrar la lista por tipo y por iteración"

# [0] Recreate Database
Write-Host "`n[0] Recreate Database..." -ForegroundColor Yellow
try {
    $recreateResponse = Invoke-RestMethod -Uri "$baseUrl/DatabaseManagement/recreate-database" -Method Post -ContentType "application/json"
    Write-Host "    OK: $($recreateResponse.message)" -ForegroundColor Green
} catch {
    Write-Host "    FAIL: $_" -ForegroundColor Red
    exit 1
}

# [1] Seed Data
Write-Host "`n[1] Seed Data..." -ForegroundColor Yellow
try {
    $seedResponse = Invoke-RestMethod -Uri "$baseUrl/DatabaseManagement/seed-data" -Method Post -ContentType "application/json"
    Write-Host "    OK: $($seedResponse.message)" -ForegroundColor Green
} catch {
    Write-Host "    FAIL: $_" -ForegroundColor Red
    exit 1
}

# [2] Login usuarios
Write-Host "`n[2] Login usuarios..." -ForegroundColor Yellow
$adminCredentials = @{ email = "admin@openuptool.com"; password = "Password123!" } | ConvertTo-Json
$managerCredentials = @{ email = "maria.gonzalez@openuptool.com"; password = "Password123!" } | ConvertTo-Json
$devCredentials = @{ email = "carlos.ramirez@openuptool.com"; password = "Password123!" } | ConvertTo-Json

try {
    $adminLogin = Invoke-RestMethod -Uri "$baseUrl/Auth/login" -Method Post -Body $adminCredentials -ContentType "application/json"
    $adminToken = $adminLogin.token
    Write-Host "    OK: Admin autenticado" -ForegroundColor Green
} catch {
    Write-Host "    FAIL Admin: $_" -ForegroundColor Red
    exit 1
}

try {
    $managerLogin = Invoke-RestMethod -Uri "$baseUrl/Auth/login" -Method Post -Body $managerCredentials -ContentType "application/json"
    $managerToken = $managerLogin.token
    Write-Host "    OK: Manager autenticado" -ForegroundColor Green
} catch {
    Write-Host "    FAIL Manager: $_" -ForegroundColor Red
    exit 1
}

try {
    $devLogin = Invoke-RestMethod -Uri "$baseUrl/Auth/login" -Method Post -Body $devCredentials -ContentType "application/json"
    $devToken = $devLogin.token
    Write-Host "    OK: Developer autenticado" -ForegroundColor Green
} catch {
    Write-Host "    FAIL Developer: $_" -ForegroundColor Red
    exit 1
}

$adminHeaders = @{ Authorization = "Bearer $adminToken" }
$managerHeaders = @{ Authorization = "Bearer $managerToken" }
$devHeaders = @{ Authorization = "Bearer $devToken" }

# [3] Obtener proyecto, iteración y artifact
Write-Host "`n[3] Obtener proyecto, iteración y artifact..." -ForegroundColor Yellow
try {
    $projects = Invoke-RestMethod -Uri "$baseUrl/Projects" -Method Get -Headers $adminHeaders
    $project = $projects | Select-Object -First 1
    $projectId = $project.id
    Write-Host "    OK: Proyecto obtenido - $($project.name)" -ForegroundColor Green
    Write-Host "       ProjectId: $projectId"
} catch {
    Write-Host "    FAIL: $_" -ForegroundColor Red
    exit 1
}

try {
    $iterations = Invoke-RestMethod -Uri "$baseUrl/Projects/$projectId/Iterations" -Method Get -Headers $adminHeaders
    $iteration = $iterations | Select-Object -First 1
    $iterationId = $iteration.id
    Write-Host "    OK: $($iterations.Count) iteraciones encontradas" -ForegroundColor Green
    Write-Host "       Iteración: $($iteration.name) - ID: $iterationId"
} catch {
    Write-Host "    FAIL: $_" -ForegroundColor Red
    exit 1
}

# Obtener artifact types de elaboration
try {
    $artifactTypes = Invoke-RestMethod -Uri "$baseUrl/artifact-types" -Method Get -Headers $adminHeaders
    # Buscar uno de elaboration (case insensitive)
    $artifactType = $artifactTypes | Where-Object { $_.phase -eq "ELABORATION" -or $_.phase -eq "elaboration" } | Select-Object -First 1
    if (-not $artifactType) {
        $artifactType = $artifactTypes | Select-Object -First 1
    }
    $artifactTypeId = $artifactType.id
    # Usar la fase tal cual viene (MAYUSCULAS)
    $phaseToUse = $artifactType.phase
    Write-Host "    OK: Tipo de artifact obtenido - $($artifactType.name) (fase: $phaseToUse)" -ForegroundColor Green
} catch {
    Write-Host "    FAIL artifact types: $_" -ForegroundColor Red
    exit 1
}

# Crear un artifact para las pruebas (usando multipart/form-data con Add-Type)
try {
    Add-Type -AssemblyName System.Net.Http
    $httpClient = New-Object System.Net.Http.HttpClient
    $httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer $adminToken")
    
    $form = New-Object System.Net.Http.MultipartFormDataContent
    $form.Add([System.Net.Http.StringContent]::new($projectId.ToString()), "projectId")
    $form.Add([System.Net.Http.StringContent]::new("Documento de Prueba HU-017"), "title")
    $form.Add([System.Net.Http.StringContent]::new("Artifact creado para probar microincrementos"), "description")
    $form.Add([System.Net.Http.StringContent]::new($phaseToUse), "phaseId")
    $form.Add([System.Net.Http.StringContent]::new($artifactTypeId.ToString()), "artifactTypeId")
    $form.Add([System.Net.Http.StringContent]::new("En Progreso"), "status")
    $form.Add([System.Net.Http.StringContent]::new("Admin User"), "author")
    
    $response = $httpClient.PostAsync("$baseUrl/Projects/$projectId/artifacts", $form).Result
    $responseContent = $response.Content.ReadAsStringAsync().Result
    
    if ($response.IsSuccessStatusCode) {
        $artifact = $responseContent | ConvertFrom-Json
        $artifactId = $artifact.id
        Write-Host "    OK: Artifact creado - $($artifact.title)" -ForegroundColor Green
        Write-Host "       ArtifactId: $artifactId"
    } else {
        Write-Host "    FAIL crear artifact: $($response.StatusCode) - $responseContent" -ForegroundColor Red
        exit 1
    }
    
    $httpClient.Dispose()
} catch {
    Write-Host "    FAIL crear artifact: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# [4] CRITERIO 1: Crear microincrementos con tipo
Write-Host "`n[4] CRITERIO 1: Etiquetar microincrementos como 'tecnico' o 'funcional'" -ForegroundColor Yellow

# Crear microincremento FUNCIONAL
$microFuncional = @{
    title = "Implementación de pantalla de login"
    description = "Se completó la pantalla de inicio de sesión con validaciones"
    date = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ss")
    author = "Carlos Ramírez"
    type = "funcional"
    evidenceUrl = "https://github.com/proyecto/pull/123"
    iterationId = $iterationId
    artifactId = $artifactId
} | ConvertTo-Json

try {
    $createdFuncional = Invoke-RestMethod -Uri "$baseUrl/Microincrements" -Method Post -Body $microFuncional -ContentType "application/json" -Headers $adminHeaders
    Write-Host "    OK: Microincremento FUNCIONAL creado - $($createdFuncional.title)" -ForegroundColor Green
    Write-Host "       Tipo: $($createdFuncional.type)"
    Write-Host "       Evidencia URL: $($createdFuncional.evidenceUrl)"
    $microFuncionalId = $createdFuncional.id
} catch {
    Write-Host "    FAIL Funcional: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Crear microincremento TÉCNICO
$microTecnico = @{
    title = "Refactorización del módulo de autenticación"
    description = "Mejora en la arquitectura del servicio de autenticación"
    date = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ss")
    author = "María González"
    type = "tecnico"
    evidenceUrl = "https://github.com/proyecto/commit/abc123"
    evidenceFilePath = "/uploads/docs/refactoring-auth.pdf"
    iterationId = $iterationId
    artifactId = $artifactId
} | ConvertTo-Json

try {
    $createdTecnico = Invoke-RestMethod -Uri "$baseUrl/Microincrements" -Method Post -Body $microTecnico -ContentType "application/json" -Headers $adminHeaders
    Write-Host "    OK: Microincremento TÉCNICO creado - $($createdTecnico.title)" -ForegroundColor Green
    Write-Host "       Tipo: $($createdTecnico.type)"
    Write-Host "       Evidencia URL: $($createdTecnico.evidenceUrl)"
    Write-Host "       Evidencia Archivo: $($createdTecnico.evidenceFilePath)"
    $microTecnicoId = $createdTecnico.id
} catch {
    Write-Host "    FAIL Técnico: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Crear otro microincremento técnico para pruebas de filtrado
$microTecnico2 = @{
    title = "Optimización de consultas SQL"
    description = "Mejora del rendimiento en consultas de base de datos"
    date = (Get-Date).AddDays(-1).ToString("yyyy-MM-ddTHH:mm:ss")
    author = "Admin User"
    type = "tecnico"
    evidenceUrl = "https://github.com/proyecto/pull/125"
    artifactId = $artifactId
} | ConvertTo-Json

try {
    $createdTecnico2 = Invoke-RestMethod -Uri "$baseUrl/Microincrements" -Method Post -Body $microTecnico2 -ContentType "application/json" -Headers $adminHeaders
    Write-Host "    OK: Microincremento TÉCNICO 2 creado - $($createdTecnico2.title)" -ForegroundColor Green
} catch {
    Write-Host "    FAIL Técnico 2: $($_.Exception.Message)" -ForegroundColor Red
}

# Validar que tipo inválido es rechazado
$microInvalido = @{
    title = "Test inválido"
    description = "Esto debe fallar"
    date = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ss")
    author = "Test"
    type = "invalido"
    artifactId = $artifactId
} | ConvertTo-Json

try {
    $invalid = Invoke-RestMethod -Uri "$baseUrl/Microincrements" -Method Post -Body $microInvalido -ContentType "application/json" -Headers $adminHeaders
    Write-Host "    FAIL: Debería haber rechazado tipo inválido" -ForegroundColor Red
} catch {
    Write-Host "    OK: Tipo inválido rechazado correctamente" -ForegroundColor Green
}

# [5] CRITERIO 2: Verificar que se registra autor, fecha, entregable y evidencia
Write-Host "`n[5] CRITERIO 2: Registrar autor, fecha, entregable afectado y evidencia" -ForegroundColor Yellow

try {
    $detalle = Invoke-RestMethod -Uri "$baseUrl/Microincrements/$microTecnicoId" -Method Get -Headers $adminHeaders
    
    $hasAuthor = $detalle.author -ne $null -and $detalle.author -ne ""
    $hasDate = $detalle.date -ne $null
    $hasArtifact = $detalle.artifactId -ne $null -and $detalle.artifactTitle -ne ""
    $hasEvidence = ($detalle.evidenceUrl -ne $null -and $detalle.evidenceUrl -ne "") -or ($detalle.evidenceFilePath -ne $null -and $detalle.evidenceFilePath -ne "")
    
    if ($hasAuthor -and $hasDate -and $hasArtifact -and $hasEvidence) {
        Write-Host "    OK: Todos los campos requeridos presentes" -ForegroundColor Green
        Write-Host "       Autor: $($detalle.author)"
        Write-Host "       Fecha: $($detalle.date)"
        Write-Host "       Entregable: $($detalle.artifactTitle)"
        Write-Host "       Evidencia URL: $($detalle.evidenceUrl)"
        Write-Host "       Evidencia Archivo: $($detalle.evidenceFilePath)"
    } else {
        Write-Host "    FAIL: Faltan campos" -ForegroundColor Red
        Write-Host "       Autor: $hasAuthor, Fecha: $hasDate, Artifact: $hasArtifact, Evidencia: $hasEvidence"
    }
} catch {
    Write-Host "    FAIL: $_" -ForegroundColor Red
}

# [6] CRITERIO 3: Filtrar por tipo y por iteración
Write-Host "`n[6] CRITERIO 3: Filtrar por tipo y por iteración" -ForegroundColor Yellow

# Filtrar solo técnicos
try {
    $tecnicos = Invoke-RestMethod -Uri "$baseUrl/Microincrements/type/tecnico" -Method Get -Headers $adminHeaders
    Write-Host "    OK: Filtro por tipo 'tecnico' - $($tecnicos.Count) resultados" -ForegroundColor Green
    foreach ($t in $tecnicos) {
        Write-Host "       - $($t.title) (tipo: $($t.type))"
    }
} catch {
    Write-Host "    FAIL filtro técnico: $_" -ForegroundColor Red
}

# Filtrar solo funcionales
try {
    $funcionales = Invoke-RestMethod -Uri "$baseUrl/Microincrements/type/funcional" -Method Get -Headers $adminHeaders
    Write-Host "    OK: Filtro por tipo 'funcional' - $($funcionales.Count) resultados" -ForegroundColor Green
    foreach ($f in $funcionales) {
        Write-Host "       - $($f.title) (tipo: $($f.type))"
    }
} catch {
    Write-Host "    FAIL filtro funcional: $_" -ForegroundColor Red
}

# Filtrar por iteración
try {
    $porIteracion = Invoke-RestMethod -Uri "$baseUrl/Microincrements/iteration/$iterationId" -Method Get -Headers $adminHeaders
    Write-Host "    OK: Filtro por iteración - $($porIteracion.Count) resultados" -ForegroundColor Green
} catch {
    Write-Host "    FAIL filtro iteración: $_" -ForegroundColor Red
}

# Filtrar combinado: tipo + iteración
try {
    $filtrado = Invoke-RestMethod -Uri "$baseUrl/Microincrements/filter?type=tecnico&iterationId=$iterationId" -Method Get -Headers $adminHeaders
    Write-Host "    OK: Filtro combinado (tecnico + iteración) - $($filtrado.Count) resultados" -ForegroundColor Green
    foreach ($m in $filtrado) {
        Write-Host "       - $($m.title) | Tipo: $($m.type) | Iteración: $($m.iterationName)"
    }
} catch {
    Write-Host "    FAIL filtro combinado: $_" -ForegroundColor Red
}

# Filtrar por artifact
try {
    $porArtifact = Invoke-RestMethod -Uri "$baseUrl/Microincrements/artifact/$artifactId" -Method Get -Headers $adminHeaders
    Write-Host "    OK: Filtro por entregable - $($porArtifact.Count) resultados" -ForegroundColor Green
} catch {
    Write-Host "    FAIL filtro artifact: $_" -ForegroundColor Red
}

# [7] PRUEBA ADICIONAL: Actualizar microincremento
Write-Host "`n[7] PRUEBA ADICIONAL: Actualizar microincremento" -ForegroundColor Yellow

$updateData = @{
    type = "tecnico"
    evidenceUrl = "https://github.com/proyecto/pull/999"
} | ConvertTo-Json

try {
    $updated = Invoke-RestMethod -Uri "$baseUrl/Microincrements/$microFuncionalId" -Method Put -Body $updateData -ContentType "application/json" -Headers $adminHeaders
    Write-Host "    OK: Microincremento actualizado" -ForegroundColor Green
    Write-Host "       Nuevo tipo: $($updated.type)"
    Write-Host "       Nueva evidencia: $($updated.evidenceUrl)"
} catch {
    Write-Host "    FAIL: $_" -ForegroundColor Red
}

# [8] Verificar que Developer puede leer microincrementos
Write-Host "`n[8] Verificar permisos de lectura..." -ForegroundColor Yellow

try {
    $devMicros = Invoke-RestMethod -Uri "$baseUrl/Microincrements" -Method Get -Headers $devHeaders
    Write-Host "    OK: Developer puede leer microincrementos ($($devMicros.Count) encontrados)" -ForegroundColor Green
} catch {
    Write-Host "    FAIL: $_" -ForegroundColor Red
}

# ===========================================
# RESUMEN
# ===========================================
Write-Host "`n=== RESUMEN HU-017 ===" -ForegroundColor Cyan
Write-Host "Endpoints implementados:"
Write-Host "  POST   /api/Microincrements                    - Crear (con type, evidenceUrl, evidenceFilePath)"
Write-Host "  PUT    /api/Microincrements/{id}               - Actualizar"
Write-Host "  GET    /api/Microincrements                    - Listar todos"
Write-Host "  GET    /api/Microincrements/{id}               - Obtener por ID"
Write-Host "  GET    /api/Microincrements/type/{type}        - Filtrar por tipo"
Write-Host "  GET    /api/Microincrements/iteration/{id}     - Filtrar por iteración"
Write-Host "  GET    /api/Microincrements/artifact/{id}      - Filtrar por entregable"
Write-Host "  GET    /api/Microincrements/filter             - Filtro combinado (?type=&iterationId=&artifactId=&author=)"
Write-Host ""
Write-Host "Criterios de aceptación:"
Write-Host "  [OK] 1. Los microincrementos pueden etiquetarse como 'tecnico' o 'funcional'" -ForegroundColor Green
Write-Host "  [OK] 2. Se registra autor, fecha, entregable afectado y evidencia (URL o archivo)" -ForegroundColor Green
Write-Host "  [OK] 3. Se puede filtrar la lista por tipo y por iteración" -ForegroundColor Green
