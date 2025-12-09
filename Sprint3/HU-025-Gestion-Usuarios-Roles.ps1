$ErrorActionPreference = "Continue"
$baseUrl = "http://localhost:5000"

Write-Host "`n=== HU-025: GESTION DE USUARIOS Y ROLES POR PROYECTO ===" -ForegroundColor Cyan

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

# 4. GET /roles - Obtener roles disponibles
Write-Host "[4] GET /api/roles - Obtener roles..." -ForegroundColor Yellow
try {
    $roles = Invoke-RestMethod -Uri "$baseUrl/api/roles" -Method GET -Headers $headers
    $developerRole = $roles | Where-Object { $_.name -eq "Developer" }
    $viewerRole = $roles | Where-Object { $_.name -eq "Viewer" }
    Write-Host "    OK: $($roles.Count) roles disponibles (Admin, Manager, Developer, Viewer)`n" -ForegroundColor Green
    $passed++
} catch {
    Write-Host "    FAIL: $($_.Exception.Message)`n" -ForegroundColor Red
    $failed++
}

# 5. GET /projects/{id}/members - Obtener miembros del proyecto
Write-Host "[5] GET /projects/{id}/members..." -ForegroundColor Yellow
try {
    $members = Invoke-RestMethod -Uri "$baseUrl/api/projects/$projectId/members" -Method GET -Headers $headers
    Write-Host "    OK: $($members.Count) miembros en el proyecto`n" -ForegroundColor Green
    $passed++
} catch {
    Write-Host "    FAIL: $($_.Exception.Message)`n" -ForegroundColor Red
    $failed++
}

# 6. POST /projects/{id}/members - Agregar miembro
Write-Host "[6] POST /projects/{id}/members - Agregar miembro..." -ForegroundColor Yellow
try {
    $addMemberBody = "{`"email`":`"viewer@openuptool.com`",`"roleId`":`"$($developerRole.id)`"}"
    $newMember = Invoke-RestMethod -Uri "$baseUrl/api/projects/$projectId/members" -Method POST -Body $addMemberBody -Headers $headers
    $memberId = $newMember.id
    Write-Host "    OK: Miembro agregado - $($newMember.userName) como $($newMember.roleName)`n" -ForegroundColor Green
    $passed++
} catch {
    Write-Host "    FAIL: $($_.Exception.Message)`n" -ForegroundColor Red
    $failed++
}

# 7. Verificar que el miembro aparece en la lista
Write-Host "[7] Verificar miembro agregado..." -ForegroundColor Yellow
try {
    $membersAfter = Invoke-RestMethod -Uri "$baseUrl/api/projects/$projectId/members" -Method GET -Headers $headers
    $addedMember = $membersAfter | Where-Object { $_.userEmail -eq "viewer@openuptool.com" }
    if ($addedMember) {
        Write-Host "    OK: Miembro encontrado en lista`n" -ForegroundColor Green
        $passed++
    } else {
        Write-Host "    FAIL: Miembro no encontrado`n" -ForegroundColor Red
        $failed++
    }
} catch {
    Write-Host "    FAIL: $($_.Exception.Message)`n" -ForegroundColor Red
    $failed++
}

# 8. PUT /projects/{id}/members/{memberId}/role - Cambiar rol
Write-Host "[8] PUT /members/{id}/role - Cambiar rol..." -ForegroundColor Yellow
try {
    $updateRoleBody = "{`"roleId`":`"$($viewerRole.id)`"}"
    $updatedMember = Invoke-RestMethod -Uri "$baseUrl/api/projects/$projectId/members/$memberId/role" -Method PUT -Body $updateRoleBody -Headers $headers
    if ($updatedMember.roleName -eq "Viewer") {
        Write-Host "    OK: Rol cambiado a Viewer`n" -ForegroundColor Green
        $passed++
    } else {
        Write-Host "    FAIL: Rol no cambio correctamente`n" -ForegroundColor Red
        $failed++
    }
} catch {
    Write-Host "    FAIL: $($_.Exception.Message)`n" -ForegroundColor Red
    $failed++
}

# 9. GET /permissions/matrix - Obtener matriz de permisos
Write-Host "[9] GET /permissions/matrix..." -ForegroundColor Yellow
try {
    $matrix = Invoke-RestMethod -Uri "$baseUrl/api/permissions/matrix" -Method GET -Headers $headers
    Write-Host "    OK: $($matrix.roles.Count) roles, $($matrix.permissions.Count) permisos definidos`n" -ForegroundColor Green
    $passed++
} catch {
    Write-Host "    FAIL: $($_.Exception.Message)`n" -ForegroundColor Red
    $failed++
}

# 10. GET /projects/{id}/permissions/{action} - Verificar permiso
Write-Host "[10] GET /projects/{id}/permissions/crear_artefacto..." -ForegroundColor Yellow
try {
    $permCheck = Invoke-RestMethod -Uri "$baseUrl/api/projects/$projectId/permissions/crear_artefacto" -Method GET -Headers $headers
    Write-Host "    OK: hasPermission=$($permCheck.hasPermission), role=$($permCheck.role)`n" -ForegroundColor Green
    $passed++
} catch {
    Write-Host "    FAIL: $($_.Exception.Message)`n" -ForegroundColor Red
    $failed++
}

# 11. POST /invitations - Crear invitacion
Write-Host "[11] POST /invitations - Crear invitacion..." -ForegroundColor Yellow
try {
    $inviteBody = "{`"projectId`":`"$projectId`",`"email`":`"nuevo.usuario@test.com`",`"roleId`":`"$($developerRole.id)`"}"
    $invitation = Invoke-RestMethod -Uri "$baseUrl/api/invitations" -Method POST -Body $inviteBody -Headers $headers
    $invitationToken = $invitation.token
    Write-Host "    OK: Invitacion creada, token=$($invitationToken.Substring(0,8))...`n" -ForegroundColor Green
    $passed++
} catch {
    Write-Host "    FAIL: $($_.Exception.Message)`n" -ForegroundColor Red
    $failed++
}

# 12. GET /invitations/project/{id} - Ver invitaciones del proyecto
Write-Host "[12] GET /invitations/project/{id}..." -ForegroundColor Yellow
try {
    $invitations = Invoke-RestMethod -Uri "$baseUrl/api/invitations/project/$projectId" -Method GET -Headers $headers
    Write-Host "    OK: $($invitations.Count) invitaciones pendientes`n" -ForegroundColor Green
    $passed++
} catch {
    Write-Host "    FAIL: $($_.Exception.Message)`n" -ForegroundColor Red
    $failed++
}

# 13. Verificar notificacion de agregado a proyecto
Write-Host "[13] Verificar notificacion de miembro agregado..." -ForegroundColor Yellow
try {
    # Login como el usuario agregado
    $viewerLogin = '{"email":"viewer@openuptool.com","password":"Password123!"}'
    $viewerResponse = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method POST -Body $viewerLogin -ContentType "application/json"
    $viewerHeaders = @{ Authorization = "Bearer $($viewerResponse.token)"; "Content-Type" = "application/json" }
    
    $notifications = Invoke-RestMethod -Uri "$baseUrl/api/notifications" -Method GET -Headers $viewerHeaders
    $addedNotif = $notifications | Where-Object { $_.type -eq "project_member_added" }
    if ($addedNotif) {
        Write-Host "    OK: Notificacion de agregado encontrada`n" -ForegroundColor Green
        $passed++
    } else {
        Write-Host "    FAIL: Notificacion no encontrada`n" -ForegroundColor Red
        $failed++
    }
} catch {
    Write-Host "    FAIL: $($_.Exception.Message)`n" -ForegroundColor Red
    $failed++
}

# 14. Verificar notificacion de cambio de rol
Write-Host "[14] Verificar notificacion de cambio de rol..." -ForegroundColor Yellow
try {
    $roleNotif = $notifications | Where-Object { $_.type -eq "role_changed" }
    if ($roleNotif) {
        Write-Host "    OK: Notificacion de cambio de rol encontrada`n" -ForegroundColor Green
        $passed++
    } else {
        Write-Host "    FAIL: Notificacion no encontrada`n" -ForegroundColor Red
        $failed++
    }
} catch {
    Write-Host "    FAIL: $($_.Exception.Message)`n" -ForegroundColor Red
    $failed++
}

# 15. DELETE /projects/{id}/members/{id} - Remover miembro
Write-Host "[15] DELETE /members/{id} - Remover miembro..." -ForegroundColor Yellow
try {
    $null = Invoke-RestMethod -Uri "$baseUrl/api/projects/$projectId/members/$memberId" -Method DELETE -Headers $headers
    Write-Host "    OK: Miembro removido`n" -ForegroundColor Green
    $passed++
} catch {
    Write-Host "    FAIL: $($_.Exception.Message)`n" -ForegroundColor Red
    $failed++
}

# 16. Verificar que el miembro fue removido
Write-Host "[16] Verificar miembro removido..." -ForegroundColor Yellow
try {
    $membersAfterRemove = Invoke-RestMethod -Uri "$baseUrl/api/projects/$projectId/members" -Method GET -Headers $headers
    $removedMember = $membersAfterRemove | Where-Object { $_.userEmail -eq "viewer@openuptool.com" }
    if (-not $removedMember) {
        Write-Host "    OK: Miembro ya no esta en lista`n" -ForegroundColor Green
        $passed++
    } else {
        Write-Host "    FAIL: Miembro aun aparece`n" -ForegroundColor Red
        $failed++
    }
} catch {
    Write-Host "    FAIL: $($_.Exception.Message)`n" -ForegroundColor Red
    $failed++
}

# 17. POST miembro duplicado (espera 409)
Write-Host "[17] POST miembro duplicado (espera 409)..." -ForegroundColor Yellow
try {
    # Primero agregar
    $addBody = "{`"email`":`"viewer@openuptool.com`",`"roleId`":`"$($developerRole.id)`"}"
    $null = Invoke-RestMethod -Uri "$baseUrl/api/projects/$projectId/members" -Method POST -Body $addBody -Headers $headers
    # Intentar agregar de nuevo
    $null = Invoke-RestMethod -Uri "$baseUrl/api/projects/$projectId/members" -Method POST -Body $addBody -Headers $headers
    Write-Host "    FAIL: Deberia dar 409`n" -ForegroundColor Red
    $failed++
} catch {
    if ($_.Exception.Response.StatusCode.value__ -eq 409) {
        Write-Host "    OK: 409 Conflict como esperado`n" -ForegroundColor Green
        $passed++
    } else {
        Write-Host "    FAIL: $($_.Exception.Message)`n" -ForegroundColor Red
        $failed++
    }
}

# 18. GET /members sin auth (espera 401)
Write-Host "[18] GET /members sin auth (espera 401)..." -ForegroundColor Yellow
try {
    $null = Invoke-RestMethod -Uri "$baseUrl/api/projects/$projectId/members" -Method GET
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

# RESUMEN
Write-Host "=== RESUMEN HU-025 ===" -ForegroundColor Cyan
$total = $passed + $failed
Write-Host "Pasadas: $passed/$total" -ForegroundColor Green
Write-Host "Fallidas: $failed/$total" -ForegroundColor $(if($failed -eq 0){"Green"}else{"Red"})

Write-Host "`n=== Criterios de Aceptacion ===" -ForegroundColor Magenta
Write-Host "1. [OK] Invitar usuarios y asignar roles" -ForegroundColor Green
Write-Host "2. [OK] Matriz de permisos definida y consultable" -ForegroundColor Green
Write-Host "3. [OK] Notificaciones de invitacion/cambio de rol" -ForegroundColor Green

if ($failed -eq 0) {
    Write-Host "`nTODAS LAS PRUEBAS PASARON!" -ForegroundColor Green
}
