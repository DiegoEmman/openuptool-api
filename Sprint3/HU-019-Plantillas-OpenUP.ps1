$ErrorActionPreference = "Continue"
$baseUrl = "http://localhost:5000"

Write-Host "`n=== HU-019: GUARDAR Y VERSIONAR PLANTILLAS OPENUP ===" -ForegroundColor Cyan

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

# ==============================================================================
# TEST 2.5: CREAR CONFIGURACION POR DEFECTO (si no existe)
# ==============================================================================
Write-Host "[2.5] Crear configuracion por defecto..." -ForegroundColor Yellow
try {
    $createConfigBody = @{
        name = "OpenUP Default Configuration"
        description = "Configuracion predeterminada de OpenUP"
        isDefault = $true
        copyFromDefault = $false
    } | ConvertTo-Json
    
    $defaultConfig = Invoke-RestMethod -Uri "$baseUrl/api/Configuration" -Method POST -Body $createConfigBody -Headers $headers
    Write-Host "    Configuracion creada: $($defaultConfig.name)" -ForegroundColor White
    Write-Host "    ID: $($defaultConfig.id)" -ForegroundColor Gray
    Write-Host "    OK: Configuracion por defecto creada`n" -ForegroundColor Green
} catch {
    Write-Host "    INFO: $($_.Exception.Message)`n" -ForegroundColor Yellow
}

# ==============================================================================
# TEST 3: LISTAR PLANTILLAS EXISTENTES
# ==============================================================================
Write-Host "[3] Listar plantillas existentes..." -ForegroundColor Yellow
$templateId1 = $null
try {
    $templates = Invoke-RestMethod -Uri "$baseUrl/api/Configuration/templates" -Method GET -Headers $headers
    Write-Host "    Plantillas encontradas: $($templates.Count)" -ForegroundColor White
    foreach ($t in $templates) {
        Write-Host "      - $($t.name) (v$($t.version)) - Roles: $($t.rolesCount), Fases: $($t.phasesCount)" -ForegroundColor Gray
        if ($null -eq $templateId1) {
            $templateId1 = $t.id
        }
    }
    Write-Host "    OK: Listado completado`n" -ForegroundColor Green
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# ==============================================================================
# TEST 4: GUARDAR CONFIGURACION COMO PLANTILLA
# ==============================================================================
Write-Host "[4] Guardar configuracion como plantilla..." -ForegroundColor Yellow
$templateId2 = $null
if ($null -ne $templateId1) {
    try {
        $saveBody = @{
            name = "Mi Plantilla Personalizada $(Get-Date -Format 'HHmmss')"
            description = "Plantilla creada desde la configuracion existente"
            tags = "custom,test,hu019"
        } | ConvertTo-Json
        
        $newTemplate = Invoke-RestMethod -Uri "$baseUrl/api/Configuration/$templateId1/save-as-template" -Method POST -Body $saveBody -Headers $headers
        $templateId2 = $newTemplate.id
        Write-Host "    Nueva plantilla: $($newTemplate.name)" -ForegroundColor White
        Write-Host "    ID: $($newTemplate.id)" -ForegroundColor Gray
        Write-Host "    OK: Plantilla guardada`n" -ForegroundColor Green
    } catch {
        Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
    }
} else {
    Write-Host "    SKIP: No hay plantilla origen`n" -ForegroundColor Yellow
}

# ==============================================================================
# TEST 5: CLONAR PLANTILLA
# ==============================================================================
Write-Host "[5] Clonar plantilla..." -ForegroundColor Yellow
$clonedTemplateId = $null
if ($null -ne $templateId1) {
    try {
        $cloneBody = @{
            newName = "Plantilla Clonada $(Get-Date -Format 'HHmmss')"
            newDescription = "Esta es una copia de la plantilla original"
        } | ConvertTo-Json
        
        $cloned = Invoke-RestMethod -Uri "$baseUrl/api/Configuration/$templateId1/clone" -Method POST -Body $cloneBody -Headers $headers
        $clonedTemplateId = $cloned.id
        Write-Host "    Plantilla clonada: $($cloned.name)" -ForegroundColor White
        Write-Host "    ID: $($cloned.id)" -ForegroundColor Gray
        Write-Host "    OK: Plantilla clonada`n" -ForegroundColor Green
    } catch {
        Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
    }
} else {
    Write-Host "    SKIP: No hay plantilla origen`n" -ForegroundColor Yellow
}

# ==============================================================================
# TEST 6: COMPARAR DOS PLANTILLAS
# ==============================================================================
Write-Host "[6] Comparar dos plantillas..." -ForegroundColor Yellow
if ($null -ne $templateId1 -and $null -ne $templateId2) {
    try {
        $compareUrl = "$baseUrl/api/Configuration/templates/compare?template1Id=$templateId1" + [char]38 + "template2Id=$templateId2"
        $comparison = Invoke-RestMethod -Uri $compareUrl -Method GET -Headers $headers
        
        Write-Host "    Plantilla 1: $($comparison.template1.name) (v$($comparison.template1.version))" -ForegroundColor White
        Write-Host "    Plantilla 2: $($comparison.template2.name) (v$($comparison.template2.version))" -ForegroundColor White
        Write-Host "    Total diferencias: $($comparison.summary.totalDifferences)" -ForegroundColor Gray
        Write-Host "    Son identicas: $($comparison.summary.areIdentical)" -ForegroundColor Gray
        Write-Host "    OK: Comparacion completada`n" -ForegroundColor Green
    } catch {
        Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
    }
} else {
    Write-Host "    SKIP: Se necesitan dos plantillas`n" -ForegroundColor Yellow
}

# ==============================================================================
# TEST 7: EXPORTAR PLANTILLA
# ==============================================================================
Write-Host "[7] Exportar plantilla..." -ForegroundColor Yellow
$exportedTemplate = $null
if ($null -ne $templateId1) {
    try {
        $exported = Invoke-RestMethod -Uri "$baseUrl/api/Configuration/$templateId1/export" -Method GET -Headers $headers
        $exportedTemplate = $exported
        
        Write-Host "    Nombre: $($exported.name)" -ForegroundColor White
        Write-Host "    Version: $($exported.version)" -ForegroundColor Gray
        Write-Host "    Roles: $($exported.roles.Count), Fases: $($exported.phases.Count)" -ForegroundColor Gray
        Write-Host "    Tipos: $($exported.artifactTypes.Count), Workflows: $($exported.workflows.Count)" -ForegroundColor Gray
        Write-Host "    OK: Plantilla exportada`n" -ForegroundColor Green
    } catch {
        Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
    }
} else {
    Write-Host "    SKIP: No hay plantilla para exportar`n" -ForegroundColor Yellow
}

# ==============================================================================
# TEST 8: IMPORTAR PLANTILLA
# ==============================================================================
Write-Host "[8] Importar plantilla..." -ForegroundColor Yellow
if ($null -ne $exportedTemplate) {
    try {
        $importBody = @{
            name = "Plantilla Importada $(Get-Date -Format 'HHmmss')"
            description = "Plantilla importada desde exportacion"
            roles = $exportedTemplate.roles
            phases = $exportedTemplate.phases
            artifactTypes = $exportedTemplate.artifactTypes
            workflows = $exportedTemplate.workflows
        } | ConvertTo-Json -Depth 10
        
        $imported = Invoke-RestMethod -Uri "$baseUrl/api/Configuration/templates/import" -Method POST -Body $importBody -Headers $headers
        
        Write-Host "    Nueva plantilla: $($imported.name)" -ForegroundColor White
        Write-Host "    ID: $($imported.id)" -ForegroundColor Gray
        Write-Host "    Roles: $($imported.roleTemplatesCount), Fases: $($imported.phaseTemplatesCount)" -ForegroundColor Gray
        Write-Host "    OK: Plantilla importada`n" -ForegroundColor Green
    } catch {
        Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
    }
} else {
    Write-Host "    SKIP: No hay plantilla exportada`n" -ForegroundColor Yellow
}

# ==============================================================================
# TEST 9: VER HISTORIAL DE VERSIONES
# ==============================================================================
Write-Host "[9] Ver historial de versiones..." -ForegroundColor Yellow
if ($null -ne $templateId1) {
    try {
        $history = Invoke-RestMethod -Uri "$baseUrl/api/Configuration/$templateId1/versions" -Method GET -Headers $headers
        
        Write-Host "    Total de cambios: $($history.Count)" -ForegroundColor White
        foreach ($change in $history | Select-Object -First 5) {
            Write-Host "      v$($change.version) - $($change.changeDescription) por $($change.changedBy)" -ForegroundColor Gray
        }
        Write-Host "    OK: Historial obtenido`n" -ForegroundColor Green
    } catch {
        Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
    }
} else {
    Write-Host "    SKIP: No hay plantilla`n" -ForegroundColor Yellow
}

# ==============================================================================
# TEST 10: VERIFICAR ESTADO FINAL
# ==============================================================================
Write-Host "[10] Verificar estado final..." -ForegroundColor Yellow
try {
    $finalTemplates = Invoke-RestMethod -Uri "$baseUrl/api/Configuration/templates" -Method GET -Headers $headers
    
    Write-Host "    Total de plantillas: $($finalTemplates.Count)" -ForegroundColor White
    foreach ($t in $finalTemplates) {
        $defaultMark = ""
        if ($t.isDefault) { $defaultMark = "[DEFAULT]" }
        Write-Host "      - $($t.name) $defaultMark" -ForegroundColor Gray
    }
    Write-Host "    OK: Estado final verificado`n" -ForegroundColor Green
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

Write-Host "=== FIN DE PRUEBAS HU-019 ===" -ForegroundColor Cyan
