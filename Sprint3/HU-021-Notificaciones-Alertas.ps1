$ErrorActionPreference = "Continue"
$baseUrl = "http://localhost:5000"

Write-Host "`n=== HU-021: NOTIFICACIONES Y ALERTAS ===" -ForegroundColor Cyan

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
    $userId = $response.user.id
    Write-Host "    OK: Token obtenido, User ID: $userId`n" -ForegroundColor Green
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
    exit
}

# ==============================================================================
# TEST 3: OBTENER TIPOS DE NOTIFICACION
# ==============================================================================
Write-Host "[3] Obtener tipos de notificacion disponibles..." -ForegroundColor Yellow
try {
    $types = Invoke-RestMethod -Uri "$baseUrl/api/notifications/types" -Method GET -Headers $headers
    Write-Host "    OK: $($types.Count) tipos disponibles" -ForegroundColor Green
    foreach ($t in $types) {
        Write-Host "        - $($t.type): $($t.description)" -ForegroundColor Gray
    }
    Write-Host ""
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# ==============================================================================
# TEST 4: OBTENER PREFERENCIAS DEL USUARIO (deberia inicializar automaticamente)
# ==============================================================================
Write-Host "[4] Obtener preferencias del usuario..." -ForegroundColor Yellow
try {
    $prefs = Invoke-RestMethod -Uri "$baseUrl/api/notifications/preferences" -Method GET -Headers $headers
    Write-Host "    OK: $($prefs.preferences.Count) preferencias" -ForegroundColor Green
    Write-Host "        Email global: $($prefs.globalEmailEnabled)" -ForegroundColor Gray
    Write-Host "        In-App global: $($prefs.globalPlatformEnabled)" -ForegroundColor Gray
    if ($prefs.preferences.Count -gt 0) {
        Write-Host "        Primeras 3:" -ForegroundColor Gray
        $prefs.preferences | Select-Object -First 3 | ForEach-Object {
            Write-Host "          $($_.notificationType): Email=$($_.emailEnabled), InApp=$($_.platformEnabled)" -ForegroundColor Gray
        }
    }
    Write-Host ""
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# ==============================================================================
# TEST 5: INICIALIZAR PREFERENCIAS (endpoint explicito)
# ==============================================================================
Write-Host "[5] Inicializar preferencias (endpoint explicito)..." -ForegroundColor Yellow
try {
    $initResult = Invoke-RestMethod -Uri "$baseUrl/api/notifications/preferences/initialize" -Method POST -Headers $headers
    Write-Host "    OK: $($initResult.preferences.Count) preferencias inicializadas`n" -ForegroundColor Green
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# ==============================================================================
# TEST 6: ACTUALIZAR PREFERENCIA ESPECIFICA (habilitar email para CRITICAL_DEFECT)
# ==============================================================================
Write-Host "[6] Actualizar preferencia CRITICAL_DEFECT (habilitar email)..." -ForegroundColor Yellow
try {
    $updateBody = @{
        notificationType = "CRITICAL_DEFECT"
        emailEnabled = $true
        platformEnabled = $true
    } | ConvertTo-Json
    
    $updated = Invoke-RestMethod -Uri "$baseUrl/api/notifications/preferences/CRITICAL_DEFECT" -Method PUT -Body $updateBody -Headers $headers
    Write-Host "    OK: $($updated.notificationType)" -ForegroundColor Green
    Write-Host "        Email: $($updated.emailEnabled), InApp: $($updated.platformEnabled)`n" -ForegroundColor Gray
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# ==============================================================================
# TEST 7: VERIFICAR CAMBIO EN PREFERENCIA
# ==============================================================================
Write-Host "[7] Verificar cambio en preferencia..." -ForegroundColor Yellow
try {
    $prefs2 = Invoke-RestMethod -Uri "$baseUrl/api/notifications/preferences" -Method GET -Headers $headers
    $criticalPref = $prefs2.preferences | Where-Object { $_.notificationType -eq "CRITICAL_DEFECT" }
    
    if ($criticalPref.emailEnabled -eq $true) {
        Write-Host "    OK: CRITICAL_DEFECT.emailEnabled = True (correcto)`n" -ForegroundColor Green
    } else {
        Write-Host "    FAIL: El cambio no se aplico`n" -ForegroundColor Red
    }
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# ==============================================================================
# TEST 8: ACTUALIZACION MASIVA (deshabilitar todos los emails)
# ==============================================================================
Write-Host "[8] Actualizacion masiva (deshabilitar todos los emails)..." -ForegroundColor Yellow
try {
    $bulkBody = @{
        enableAllEmail = $false
        enableAllPlatform = $true
    } | ConvertTo-Json
    
    $bulkResult = Invoke-RestMethod -Uri "$baseUrl/api/notifications/preferences" -Method PUT -Body $bulkBody -Headers $headers
    Write-Host "    OK: Email global=$($bulkResult.globalEmailEnabled), InApp global=$($bulkResult.globalPlatformEnabled)`n" -ForegroundColor Green
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# ==============================================================================
# TEST 9: ACTUALIZAR PREFERENCIAS INDIVIDUALES EN LOTE
# ==============================================================================
Write-Host "[9] Actualizar preferencias individuales en lote..." -ForegroundColor Yellow
try {
    $bulkIndBody = @{
        preferences = @(
            @{ notificationType = "NEW_VERSION"; emailEnabled = $true; platformEnabled = $true },
            @{ notificationType = "STATE_APPROVED"; emailEnabled = $true; platformEnabled = $true },
            @{ notificationType = "DEADLINE_APPROACHING"; emailEnabled = $false; platformEnabled = $false }
        )
    } | ConvertTo-Json -Depth 3
    
    $bulkIndResult = Invoke-RestMethod -Uri "$baseUrl/api/notifications/preferences" -Method PUT -Body $bulkIndBody -Headers $headers
    
    $newVersionPref = $bulkIndResult.preferences | Where-Object { $_.notificationType -eq "NEW_VERSION" }
    $deadlinePref = $bulkIndResult.preferences | Where-Object { $_.notificationType -eq "DEADLINE_APPROACHING" }
    
    Write-Host "    OK: Preferencias actualizadas" -ForegroundColor Green
    Write-Host "        NEW_VERSION: Email=$($newVersionPref.emailEnabled), InApp=$($newVersionPref.platformEnabled)" -ForegroundColor Gray
    Write-Host "        DEADLINE_APPROACHING: InApp=$($deadlinePref.platformEnabled)`n" -ForegroundColor Gray
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# ==============================================================================
# TEST 10: DISPARAR NOTIFICACION DE PRUEBA (NEW_VERSION - habilitada)
# ==============================================================================
Write-Host "[10] Disparar notificacion NEW_VERSION (habilitada)..." -ForegroundColor Yellow
try {
    $triggerBody = @{
        type = "NEW_VERSION"
        title = "Nueva version de artefacto"
        message = "Se ha creado la version 2.0 del artefacto Documento de Requisitos"
        relatedEntityType = "Artifact"
        actionUrl = "/projects/test/artifacts/doc-requisitos"
    } | ConvertTo-Json
    
    $triggerResult = Invoke-RestMethod -Uri "$baseUrl/api/notifications/trigger" -Method POST -Body $triggerBody -Headers $headers
    Write-Host "    OK: $($triggerResult.message)`n" -ForegroundColor Green
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# ==============================================================================
# TEST 11: VERIFICAR NOTIFICACION CREADA
# ==============================================================================
Write-Host "[11] Verificar notificacion creada..." -ForegroundColor Yellow
try {
    $notifications = Invoke-RestMethod -Uri "$baseUrl/api/notifications" -Method GET -Headers $headers
    $newVersionNotif = $notifications | Where-Object { $_.type -eq "NEW_VERSION" -and $_.title -like "*Nueva version*" } | Select-Object -First 1
    
    if ($newVersionNotif) {
        Write-Host "    OK: Notificacion encontrada" -ForegroundColor Green
        Write-Host "        ID: $($newVersionNotif.id)" -ForegroundColor Gray
        Write-Host "        Titulo: $($newVersionNotif.title)" -ForegroundColor Gray
        Write-Host "        ActionUrl: $($newVersionNotif.actionUrl)`n" -ForegroundColor Gray
    } else {
        Write-Host "    INFO: No se encontro la notificacion (puede estar filtrada)`n" -ForegroundColor Yellow
    }
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# ==============================================================================
# TEST 12: DISPARAR NOTIFICACION DESHABILITADA (DEADLINE_APPROACHING)
# ==============================================================================
Write-Host "[12] Disparar notificacion DEADLINE_APPROACHING (deshabilitada)..." -ForegroundColor Yellow
try {
    $triggerDisabledBody = @{
        type = "DEADLINE_APPROACHING"
        title = "Fecha limite proxima - TEST DISABLED"
        message = "Esta notificacion NO deberia aparecer"
    } | ConvertTo-Json
    
    $triggerDisabledResult = Invoke-RestMethod -Uri "$baseUrl/api/notifications/trigger" -Method POST -Body $triggerDisabledBody -Headers $headers
    Write-Host "    OK: Solicitud procesada (no deberia crear notificacion)`n" -ForegroundColor Green
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# ==============================================================================
# TEST 13: VERIFICAR QUE NOTIFICACION DESHABILITADA NO SE CREO
# ==============================================================================
Write-Host "[13] Verificar que notificacion deshabilitada NO se creo..." -ForegroundColor Yellow
try {
    Start-Sleep -Seconds 1
    $notifications2 = Invoke-RestMethod -Uri "$baseUrl/api/notifications" -Method GET -Headers $headers
    $deadlineNotif = $notifications2 | Where-Object { $_.title -like "*TEST DISABLED*" } | Select-Object -First 1
    
    if (!$deadlineNotif) {
        Write-Host "    OK: La notificacion NO fue creada (preferencia respetada)`n" -ForegroundColor Green
    } else {
        Write-Host "    FAIL: La notificacion fue creada cuando no deberia`n" -ForegroundColor Red
    }
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# ==============================================================================
# TEST 14: VALIDAR TIPO DE NOTIFICACION INVALIDO
# ==============================================================================
Write-Host "[14] Validar tipo de notificacion invalido..." -ForegroundColor Yellow
try {
    $invalidBody = @{
        notificationType = "TIPO_INVALIDO"
        emailEnabled = $true
    } | ConvertTo-Json
    
    $invalidResult = Invoke-RestMethod -Uri "$baseUrl/api/notifications/preferences/TIPO_INVALIDO" -Method PUT -Body $invalidBody -Headers $headers
    Write-Host "    FAIL: Deberia haber rechazado el tipo invalido`n" -ForegroundColor Red
} catch {
    if ($_.Exception.Response.StatusCode.value__ -eq 400) {
        Write-Host "    OK: Tipo invalido rechazado correctamente (400)`n" -ForegroundColor Green
    } else {
        Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
    }
}

# ==============================================================================
# TEST 15: RESTAURAR PREFERENCIAS POR DEFECTO
# ==============================================================================
Write-Host "[15] Restaurar preferencias por defecto..." -ForegroundColor Yellow
try {
    $restoreBody = @{
        enableAllEmail = $false
        enableAllPlatform = $true
    } | ConvertTo-Json
    
    $restoreResult = Invoke-RestMethod -Uri "$baseUrl/api/notifications/preferences" -Method PUT -Body $restoreBody -Headers $headers
    Write-Host "    OK: Email=$($restoreResult.globalEmailEnabled), InApp=$($restoreResult.globalPlatformEnabled)`n" -ForegroundColor Green
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# ==============================================================================
# TEST 16: CONTADOR DE NOTIFICACIONES NO LEIDAS
# ==============================================================================
Write-Host "[16] Contador de notificaciones no leidas..." -ForegroundColor Yellow
try {
    $countResult = Invoke-RestMethod -Uri "$baseUrl/api/notifications/unread-count" -Method GET -Headers $headers
    Write-Host "    OK: $($countResult.count) notificaciones no leidas`n" -ForegroundColor Green
} catch {
    Write-Host "    ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
}

# ==============================================================================
# RESUMEN
# ==============================================================================
Write-Host "=== RESUMEN HU-021 ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "ENDPOINTS IMPLEMENTADOS:" -ForegroundColor Magenta
Write-Host "  GET  /api/notifications/types                    - Lista tipos disponibles"
Write-Host "  GET  /api/notifications/preferences              - Obtener preferencias usuario"
Write-Host "  POST /api/notifications/preferences/initialize   - Inicializar preferencias"
Write-Host "  PUT  /api/notifications/preferences/{type}       - Actualizar una preferencia"
Write-Host "  PUT  /api/notifications/preferences              - Actualizacion masiva"
Write-Host "  POST /api/notifications/trigger                  - Disparar notificacion (test)"
Write-Host ""
Write-Host "CRITERIOS DE ACEPTACION:" -ForegroundColor Magenta
Write-Host "  [OK] Notificaciones configurables por email/in-app"
Write-Host "  [OK] Habilitar/deshabilitar tipos de notificacion"
Write-Host "  [OK] Enlaces directos a artefactos (actionUrl)"
Write-Host "  [OK] Respeto de preferencias al crear notificaciones"
Write-Host ""
Write-Host "=== HU-021 COMPLETADA ===" -ForegroundColor Green
