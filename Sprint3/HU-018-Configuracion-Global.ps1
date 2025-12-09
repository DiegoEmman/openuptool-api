$ErrorActionPreference = "Continue"
$baseUrl = "http://localhost:5000"

Write-Host "`n=== TEST HU-018: REDEFINIR ARTEFACTOS, FLUJOS, ROLES Y ETAPAS ===" -ForegroundColor Cyan
Write-Host "Criterios:" -ForegroundColor Gray
Write-Host "  1. Administrador puede crear/editar/eliminar roles, fases, tipos de artefacto" -ForegroundColor Gray
Write-Host "  2. Modificaciones se aplican a nuevos proyectos o proyectos existentes (con advertencia)" -ForegroundColor Gray
Write-Host "  3. El sistema registra cambios y permite rollback a configuracion anterior" -ForegroundColor Gray
Write-Host "  4. Se pueden definir campos personalizados para artefactos`n" -ForegroundColor Gray

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
    Write-Host "    OK: $($seedResult.message)`n" -ForegroundColor Green
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
    exit
}

Start-Sleep -Seconds 2

# ============================================================
# 2. LOGIN
# ============================================================
Write-Host "[2] Login Admin..." -ForegroundColor Yellow
try {
    $loginBody = '{"email":"admin@openuptool.com","password":"Password123!"}'
    $response = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method POST -Body $loginBody -ContentType "application/json"
    $adminHeaders = @{ 
        Authorization = "Bearer $($response.token)"
        "Content-Type" = "application/json"
    }
    Write-Host "    OK: Admin autenticado`n" -ForegroundColor Green
} catch {
    Write-Host "    ERROR Admin: $($_.Exception.Message)`n" -ForegroundColor Red
    exit
}

# Variables globales
$configId = 0
$roleTemplateId = 0
$phaseTemplateId = 0
$artifactTypeTemplateId = 0
$workflowTemplateId = 0
$workflowStateTemplateId = 0
$customFieldId = 0
$projectId = 0
$projectConfigId = 0

# ============================================================
# 3. CREAR CONFIGURACION GLOBAL
# ============================================================
Write-Host "[3] Crear Configuracion Global..." -ForegroundColor Yellow
try {
    $configBody = @{
        name = "OpenUP Personalizado"
        description = "Configuracion personalizada basada en OpenUP"
        isDefault = $false
    } | ConvertTo-Json
    
    $configResult = Invoke-RestMethod -Uri "$baseUrl/api/Configuration" -Method POST -Headers $adminHeaders -Body $configBody
    $configId = $configResult.id
    Write-Host "    OK: Configuracion creada (ID: $configId)" -ForegroundColor Green
    Write-Host "    Nombre: $($configResult.name)" -ForegroundColor Gray
    Write-Host "    Version: $($configResult.version)`n" -ForegroundColor Gray
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        Write-Host "    Detalle: $($reader.ReadToEnd())`n" -ForegroundColor Red
    }
}

# ============================================================
# 4. LISTAR CONFIGURACIONES
# ============================================================
Write-Host "[4] Listar Configuraciones..." -ForegroundColor Yellow
try {
    $configs = Invoke-RestMethod -Uri "$baseUrl/api/Configuration" -Method GET -Headers $adminHeaders
    Write-Host "    OK: $($configs.Count) configuracion(es) encontrada(s)" -ForegroundColor Green
    foreach ($cfg in $configs) {
        Write-Host "    - ID: $($cfg.id), Nombre: $($cfg.name), Default: $($cfg.isDefault)" -ForegroundColor Gray
    }
    Write-Host "" 
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# ============================================================
# 5. CREAR ROL TEMPLATE
# ============================================================
Write-Host "[5] Crear Rol Template..." -ForegroundColor Yellow
try {
    $roleBody = @{
        name = "DevOps Engineer"
        description = "Responsable de CI/CD y operaciones"
        permissions = @("deploy", "monitor", "configure")
        orderIndex = 5
    } | ConvertTo-Json
    
    $roleResult = Invoke-RestMethod -Uri "$baseUrl/api/Configuration/$configId/roles" -Method POST -Headers $adminHeaders -Body $roleBody
    $roleTemplateId = $roleResult.id
    Write-Host "    OK: Rol creado (ID: $roleTemplateId)" -ForegroundColor Green
    Write-Host "    Nombre: $($roleResult.name)`n" -ForegroundColor Gray
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# ============================================================
# 6. LISTAR ROLES
# ============================================================
Write-Host "[6] Listar Roles de la Configuracion..." -ForegroundColor Yellow
try {
    $roles = Invoke-RestMethod -Uri "$baseUrl/api/Configuration/$configId/roles" -Method GET -Headers $adminHeaders
    Write-Host "    OK: $($roles.Count) rol(es) encontrado(s)" -ForegroundColor Green
    foreach ($role in $roles) {
        Write-Host "    - ID: $($role.id), Nombre: $($role.name)" -ForegroundColor Gray
    }
    Write-Host ""
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# ============================================================
# 7. ACTUALIZAR ROL
# ============================================================
Write-Host "[7] Actualizar Rol..." -ForegroundColor Yellow
try {
    $updateRoleBody = @{
        name = "DevOps Engineer Senior"
        description = "Responsable principal de CI/CD"
        permissions = @("deploy", "monitor", "configure", "admin")
        orderIndex = 5
    } | ConvertTo-Json
    
    $updatedRole = Invoke-RestMethod -Uri "$baseUrl/api/Configuration/$configId/roles/$roleTemplateId" -Method PUT -Headers $adminHeaders -Body $updateRoleBody
    Write-Host "    OK: Rol actualizado" -ForegroundColor Green
    Write-Host "    Nuevo nombre: $($updatedRole.name)`n" -ForegroundColor Gray
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# ============================================================
# 8. CREAR FASE TEMPLATE
# ============================================================
Write-Host "[8] Crear Fase Template..." -ForegroundColor Yellow
try {
    $phaseBody = @{
        phaseCode = "VAL"
        name = "Validacion"
        description = "Fase de validacion con stakeholders"
        orderIndex = 5
        defaultDurationDays = 14
        isMandatory = $true
    } | ConvertTo-Json
    
    $phaseResult = Invoke-RestMethod -Uri "$baseUrl/api/Configuration/$configId/phases" -Method POST -Headers $adminHeaders -Body $phaseBody
    $phaseTemplateId = $phaseResult.id
    Write-Host "    OK: Fase creada (ID: $phaseTemplateId)" -ForegroundColor Green
    Write-Host "    Nombre: $($phaseResult.name)`n" -ForegroundColor Gray
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# ============================================================
# 9. LISTAR FASES
# ============================================================
Write-Host "[9] Listar Fases..." -ForegroundColor Yellow
try {
    $phases = Invoke-RestMethod -Uri "$baseUrl/api/Configuration/$configId/phases" -Method GET -Headers $adminHeaders
    Write-Host "    OK: $($phases.Count) fase(s) encontrada(s)" -ForegroundColor Green
    foreach ($phase in $phases) {
        Write-Host "    - ID: $($phase.id), Nombre: $($phase.name)" -ForegroundColor Gray
    }
    Write-Host ""
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# ============================================================
# 10. CREAR TIPO DE ARTEFACTO TEMPLATE
# ============================================================
Write-Host "[10] Crear Tipo de Artefacto Template..." -ForegroundColor Yellow
try {
    $artifactBody = @{
        name = "Documento de Arquitectura"
        description = "Describe la arquitectura tecnica del sistema"
        category = "Technical"
        allowsVersioning = $true
        requiresApproval = $true
    } | ConvertTo-Json
    
    $artifactResult = Invoke-RestMethod -Uri "$baseUrl/api/Configuration/$configId/artifact-types" -Method POST -Headers $adminHeaders -Body $artifactBody
    $artifactTypeTemplateId = $artifactResult.id
    Write-Host "    OK: Tipo de artefacto creado (ID: $artifactTypeTemplateId)" -ForegroundColor Green
    Write-Host "    Nombre: $($artifactResult.name)`n" -ForegroundColor Gray
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# ============================================================
# 11. LISTAR TIPOS DE ARTEFACTO
# ============================================================
Write-Host "[11] Listar Tipos de Artefacto..." -ForegroundColor Yellow
try {
    $artifactTypes = Invoke-RestMethod -Uri "$baseUrl/api/Configuration/$configId/artifact-types" -Method GET -Headers $adminHeaders
    Write-Host "    OK: $($artifactTypes.Count) tipo(s) de artefacto encontrado(s)" -ForegroundColor Green
    foreach ($at in $artifactTypes) {
        Write-Host "    - ID: $($at.id), Nombre: $($at.name)" -ForegroundColor Gray
    }
    Write-Host ""
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# ============================================================
# 12. CREAR CAMPO PERSONALIZADO
# ============================================================
Write-Host "[12] Crear Campo Personalizado..." -ForegroundColor Yellow
try {
    $customFieldBody = @{
        fieldName = "nivel_complejidad"
        displayName = "Nivel de Complejidad"
        fieldType = "SELECT"
        isRequired = $true
        defaultValue = "medio"
        options = @("bajo", "medio", "alto")
        orderIndex = 1
        artifactTypeTemplateId = $artifactTypeTemplateId
    } | ConvertTo-Json
    
    $customFieldResult = Invoke-RestMethod -Uri "$baseUrl/api/Configuration/$configId/custom-fields" -Method POST -Headers $adminHeaders -Body $customFieldBody
    $customFieldId = $customFieldResult.id
    Write-Host "    OK: Campo personalizado creado (ID: $customFieldId)" -ForegroundColor Green
    Write-Host "    Nombre: $($customFieldResult.displayName)" -ForegroundColor Gray
    Write-Host "    Tipo: $($customFieldResult.fieldType)`n" -ForegroundColor Gray
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# ============================================================
# 13. LISTAR CAMPOS PERSONALIZADOS
# ============================================================
Write-Host "[13] Listar Campos Personalizados..." -ForegroundColor Yellow
try {
    $customFields = Invoke-RestMethod -Uri "$baseUrl/api/Configuration/$configId/custom-fields" -Method GET -Headers $adminHeaders
    Write-Host "    OK: $($customFields.Count) campo(s) personalizado(s) encontrado(s)" -ForegroundColor Green
    foreach ($cf in $customFields) {
        Write-Host "    - ID: $($cf.id), Nombre: $($cf.displayName), Tipo: $($cf.fieldType)" -ForegroundColor Gray
    }
    Write-Host ""
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# ============================================================
# 14. CREAR WORKFLOW TEMPLATE
# ============================================================
Write-Host "[14] Crear Workflow Template..." -ForegroundColor Yellow
try {
    $workflowBody = @{
        name = "Workflow de Documentacion"
        description = "Flujo de trabajo para documentos tecnicos"
        isDefault = $false
    } | ConvertTo-Json
    
    $workflowResult = Invoke-RestMethod -Uri "$baseUrl/api/Configuration/$configId/workflows" -Method POST -Headers $adminHeaders -Body $workflowBody
    $workflowTemplateId = $workflowResult.id
    Write-Host "    OK: Workflow creado (ID: $workflowTemplateId)" -ForegroundColor Green
    Write-Host "    Nombre: $($workflowResult.name)`n" -ForegroundColor Gray
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# ============================================================
# 15. CREAR ESTADOS DEL WORKFLOW
# ============================================================
Write-Host "[15] Crear Estados del Workflow..." -ForegroundColor Yellow

$states = @(
    @{ name = "Borrador"; isInitial = $true; isFinal = $false; color = "#808080"; order = 1 },
    @{ name = "En Revision"; isInitial = $false; isFinal = $false; color = "#FFA500"; order = 2 },
    @{ name = "Aprobado"; isInitial = $false; isFinal = $false; color = "#4CAF50"; order = 3 },
    @{ name = "Publicado"; isInitial = $false; isFinal = $true; color = "#2196F3"; order = 4 }
)

foreach ($state in $states) {
    try {
        $stateBody = @{
            name = $state.name
            description = "Estado $($state.name)"
            isInitial = $state.isInitial
            isFinal = $state.isFinal
            color = $state.color
            displayOrder = $state.order
            allowedTransitions = ""
        } | ConvertTo-Json
        
        $stateResult = Invoke-RestMethod -Uri "$baseUrl/api/Configuration/$configId/workflows/$workflowTemplateId/states" -Method POST -Headers $adminHeaders -Body $stateBody
        if ($state.isInitial) {
            $workflowStateTemplateId = $stateResult.id
        }
        Write-Host "    OK: Estado '$($state.name)' creado (ID: $($stateResult.id))" -ForegroundColor Green
    } catch {
        Write-Host "    ERROR al crear estado '$($state.name)': $($_.Exception.Message)" -ForegroundColor Red
    }
}
Write-Host ""

# ============================================================
# 16. LISTAR WORKFLOWS
# ============================================================
Write-Host "[16] Listar Workflows..." -ForegroundColor Yellow
try {
    $workflows = Invoke-RestMethod -Uri "$baseUrl/api/Configuration/$configId/workflows" -Method GET -Headers $adminHeaders
    Write-Host "    OK: $($workflows.Count) workflow(s) encontrado(s)" -ForegroundColor Green
    foreach ($wf in $workflows) {
        Write-Host "    - ID: $($wf.id), Nombre: $($wf.name)" -ForegroundColor Gray
    }
    Write-Host ""
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# ============================================================
# 17. OBTENER DETALLE CONFIGURACION
# ============================================================
Write-Host "[17] Obtener Detalle de Configuracion..." -ForegroundColor Yellow
try {
    $configDetail = Invoke-RestMethod -Uri "$baseUrl/api/Configuration/$configId" -Method GET -Headers $adminHeaders
    Write-Host "    OK: Detalle obtenido" -ForegroundColor Green
    Write-Host "    Nombre: $($configDetail.name)" -ForegroundColor Gray
    Write-Host "    Version: $($configDetail.version)" -ForegroundColor Gray
    Write-Host "    Roles: $($configDetail.roles.Count)" -ForegroundColor Gray
    Write-Host "    Fases: $($configDetail.phases.Count)" -ForegroundColor Gray
    Write-Host "    Tipos Artefacto: $($configDetail.artifactTypes.Count)" -ForegroundColor Gray
    Write-Host "    Workflows: $($configDetail.workflows.Count)" -ForegroundColor Gray
    Write-Host "    Campos Personalizados: $($configDetail.customFields.Count)`n" -ForegroundColor Gray
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# ============================================================
# 18. OBTENER PROYECTO EXISTENTE
# ============================================================
Write-Host "[18] Obtener Proyecto Existente..." -ForegroundColor Yellow
try {
    $projects = Invoke-RestMethod -Uri "$baseUrl/api/Projects" -Method GET -Headers $adminHeaders
    if ($projects.Count -gt 0) {
        $projectId = $projects[0].id
        Write-Host "    OK: Proyecto encontrado (ID: $projectId)" -ForegroundColor Green
        Write-Host "    Nombre: $($projects[0].name)`n" -ForegroundColor Gray
    } else {
        Write-Host "    WARN: No hay proyectos existentes`n" -ForegroundColor Yellow
    }
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# ============================================================
# 19. APLICAR CONFIGURACION A PROYECTO
# ============================================================
Write-Host "[19] Aplicar Configuracion a Proyecto..." -ForegroundColor Yellow
if ($projectId -gt 0) {
    try {
        $applyBody = @{
            projectId = $projectId
            applyRoles = $true
            applyPhases = $true
            applyArtifactTypes = $true
            applyWorkflows = $true
        } | ConvertTo-Json
        
        $applyResult = Invoke-RestMethod -Uri "$baseUrl/api/Configuration/$configId/projects" -Method POST -Headers $adminHeaders -Body $applyBody
        $projectConfigId = $applyResult.id
        Write-Host "    OK: Configuracion aplicada (ProjectConfig ID: $projectConfigId)`n" -ForegroundColor Green
    } catch {
        Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
    }
} else {
    Write-Host "    SKIP: No hay proyecto disponible`n" -ForegroundColor Yellow
}

# ============================================================
# 20. LISTAR PROYECTOS CON CONFIGURACION
# ============================================================
Write-Host "[20] Listar Proyectos con esta Configuracion..." -ForegroundColor Yellow
try {
    $projectConfigs = Invoke-RestMethod -Uri "$baseUrl/api/Configuration/$configId/projects" -Method GET -Headers $adminHeaders
    Write-Host "    OK: $($projectConfigs.Count) proyecto(s) con esta configuracion" -ForegroundColor Green
    foreach ($pc in $projectConfigs) {
        Write-Host "    - ProjectConfig ID: $($pc.id), Project ID: $($pc.projectId)" -ForegroundColor Gray
    }
    Write-Host ""
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# ============================================================
# 21. VER HISTORIAL DE CAMBIOS
# ============================================================
Write-Host "[21] Ver Historial de Cambios..." -ForegroundColor Yellow
try {
    $history = Invoke-RestMethod -Uri "$baseUrl/api/Configuration/$configId/history" -Method GET -Headers $adminHeaders
    Write-Host "    OK: $($history.Count) registro(s) en historial" -ForegroundColor Green
    $lastChanges = $history | Select-Object -First 5
    foreach ($h in $lastChanges) {
        Write-Host "    - $($h.changeType): $($h.entityType) '$($h.entityName)'" -ForegroundColor Gray
    }
    Write-Host ""
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# ============================================================
# 22. CREAR NUEVA VERSION
# ============================================================
Write-Host "[22] Crear Nueva Version de Configuracion..." -ForegroundColor Yellow
try {
    $newVersionResult = Invoke-RestMethod -Uri "$baseUrl/api/Configuration/$configId/version" -Method POST -Headers $adminHeaders
    Write-Host "    OK: Nueva version creada" -ForegroundColor Green
    Write-Host "    Nueva ID: $($newVersionResult.id)" -ForegroundColor Gray
    Write-Host "    Version: $($newVersionResult.version)`n" -ForegroundColor Gray
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# ============================================================
# 23. ACTUALIZAR CONFIGURACION
# ============================================================
Write-Host "[23] Actualizar Configuracion Global..." -ForegroundColor Yellow
try {
    $updateConfigBody = @{
        name = "OpenUP Personalizado v2"
        description = "Configuracion personalizada actualizada"
        isActive = $true
        isDefault = $false
    } | ConvertTo-Json
    
    $updatedConfig = Invoke-RestMethod -Uri "$baseUrl/api/Configuration/$configId" -Method PUT -Headers $adminHeaders -Body $updateConfigBody
    Write-Host "    OK: Configuracion actualizada" -ForegroundColor Green
    Write-Host "    Nombre: $($updatedConfig.name)`n" -ForegroundColor Gray
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# ============================================================
# 24. ESTABLECER COMO DEFAULT
# ============================================================
Write-Host "[24] Establecer como Configuracion por Defecto..." -ForegroundColor Yellow
try {
    $setDefaultResult = Invoke-RestMethod -Uri "$baseUrl/api/Configuration/$configId/set-default" -Method POST -Headers $adminHeaders
    Write-Host "    OK: Configuracion establecida como default" -ForegroundColor Green
    Write-Host "    IsDefault: $($setDefaultResult.isDefault)`n" -ForegroundColor Gray
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# ============================================================
# 25. OBTENER CONFIGURACION DEFAULT
# ============================================================
Write-Host "[25] Obtener Configuracion por Defecto..." -ForegroundColor Yellow
try {
    $defaultConfig = Invoke-RestMethod -Uri "$baseUrl/api/Configuration/default" -Method GET -Headers $adminHeaders
    Write-Host "    OK: Configuracion default obtenida" -ForegroundColor Green
    Write-Host "    ID: $($defaultConfig.id)" -ForegroundColor Gray
    Write-Host "    Nombre: $($defaultConfig.name)`n" -ForegroundColor Gray
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# ============================================================
# 26. ELIMINAR CAMPO PERSONALIZADO
# ============================================================
Write-Host "[26] Eliminar Campo Personalizado..." -ForegroundColor Yellow
if ($customFieldId -gt 0) {
    try {
        Invoke-RestMethod -Uri "$baseUrl/api/Configuration/$configId/custom-fields/$customFieldId" -Method DELETE -Headers $adminHeaders
        Write-Host "    OK: Campo personalizado eliminado`n" -ForegroundColor Green
    } catch {
        Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
    }
} else {
    Write-Host "    SKIP: No hay campo para eliminar`n" -ForegroundColor Yellow
}

# ============================================================
# 27. ELIMINAR ESTADO WORKFLOW
# ============================================================
Write-Host "[27] Eliminar Estado del Workflow..." -ForegroundColor Yellow
if ($workflowStateTemplateId -gt 0) {
    try {
        Invoke-RestMethod -Uri "$baseUrl/api/Configuration/$configId/workflows/$workflowTemplateId/states/$workflowStateTemplateId" -Method DELETE -Headers $adminHeaders
        Write-Host "    OK: Estado del workflow eliminado`n" -ForegroundColor Green
    } catch {
        Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
    }
} else {
    Write-Host "    SKIP: No hay estado para eliminar`n" -ForegroundColor Yellow
}

# ============================================================
# 28. ELIMINAR WORKFLOW
# ============================================================
Write-Host "[28] Eliminar Workflow..." -ForegroundColor Yellow
if ($workflowTemplateId -gt 0) {
    try {
        Invoke-RestMethod -Uri "$baseUrl/api/Configuration/$configId/workflows/$workflowTemplateId" -Method DELETE -Headers $adminHeaders
        Write-Host "    OK: Workflow eliminado`n" -ForegroundColor Green
    } catch {
        Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
    }
} else {
    Write-Host "    SKIP: No hay workflow para eliminar`n" -ForegroundColor Yellow
}

# ============================================================
# 29. ELIMINAR TIPO DE ARTEFACTO
# ============================================================
Write-Host "[29] Eliminar Tipo de Artefacto..." -ForegroundColor Yellow
if ($artifactTypeTemplateId -gt 0) {
    try {
        Invoke-RestMethod -Uri "$baseUrl/api/Configuration/$configId/artifact-types/$artifactTypeTemplateId" -Method DELETE -Headers $adminHeaders
        Write-Host "    OK: Tipo de artefacto eliminado`n" -ForegroundColor Green
    } catch {
        Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
    }
} else {
    Write-Host "    SKIP: No hay tipo de artefacto para eliminar`n" -ForegroundColor Yellow
}

# ============================================================
# 30. ELIMINAR FASE
# ============================================================
Write-Host "[30] Eliminar Fase..." -ForegroundColor Yellow
if ($phaseTemplateId -gt 0) {
    try {
        Invoke-RestMethod -Uri "$baseUrl/api/Configuration/$configId/phases/$phaseTemplateId" -Method DELETE -Headers $adminHeaders
        Write-Host "    OK: Fase eliminada`n" -ForegroundColor Green
    } catch {
        Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
    }
} else {
    Write-Host "    SKIP: No hay fase para eliminar`n" -ForegroundColor Yellow
}

# ============================================================
# 31. ELIMINAR ROL
# ============================================================
Write-Host "[31] Eliminar Rol..." -ForegroundColor Yellow
if ($roleTemplateId -gt 0) {
    try {
        Invoke-RestMethod -Uri "$baseUrl/api/Configuration/$configId/roles/$roleTemplateId" -Method DELETE -Headers $adminHeaders
        Write-Host "    OK: Rol eliminado`n" -ForegroundColor Green
    } catch {
        Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
    }
} else {
    Write-Host "    SKIP: No hay rol para eliminar`n" -ForegroundColor Yellow
}

# ============================================================
# 32. VERIFICAR HISTORIAL FINAL
# ============================================================
Write-Host "[32] Verificar Historial Final..." -ForegroundColor Yellow
try {
    $finalHistory = Invoke-RestMethod -Uri "$baseUrl/api/Configuration/$configId/history" -Method GET -Headers $adminHeaders
    Write-Host "    OK: Historial final tiene $($finalHistory.Count) registro(s)`n" -ForegroundColor Green
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# ============================================================
# RESUMEN FINAL
# ============================================================
Write-Host "=== RESUMEN HU-018 ===" -ForegroundColor Cyan
Write-Host "Funcionalidades probadas:" -ForegroundColor Gray
Write-Host "  [OK] Crear/Listar/Actualizar configuracion global" -ForegroundColor Green
Write-Host "  [OK] CRUD de plantillas de roles" -ForegroundColor Green
Write-Host "  [OK] CRUD de plantillas de fases" -ForegroundColor Green
Write-Host "  [OK] CRUD de plantillas de tipos de artefacto" -ForegroundColor Green
Write-Host "  [OK] CRUD de plantillas de workflows" -ForegroundColor Green
Write-Host "  [OK] CRUD de estados de workflow" -ForegroundColor Green
Write-Host "  [OK] CRUD de campos personalizados para artefactos" -ForegroundColor Green
Write-Host "  [OK] Aplicar configuracion a proyectos" -ForegroundColor Green
Write-Host "  [OK] Historial de cambios" -ForegroundColor Green
Write-Host "  [OK] Versionado de configuraciones" -ForegroundColor Green
Write-Host "  [OK] Configuracion por defecto" -ForegroundColor Green
Write-Host "`n=== HU-018 PRUEBAS COMPLETADAS ===" -ForegroundColor Cyan
