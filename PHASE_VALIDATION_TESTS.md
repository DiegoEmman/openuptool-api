# Sistema de Validación de Artefactos Obligatorios

## 📋 Historia de Usuario

**Como:** Product Owner  
**Quiero:** Indicar si un entregable es opcional o obligatorio dentro de la fase  
**Para:** Que la herramienta valide requisitos mínimos para pasar de fase  
**Prioridad:** Alta  
**Estimación:** 3 puntos

### Criterios de Aceptación

✅ **AC1:** En la configuración del artefacto hay un switch Obligatorio/Opcional  
✅ **AC2:** El flujo de avance de fase verifica que los artefactos obligatorios estén con versión "entregada" antes de permitir pasar de fase  
✅ **AC3:** Los artefactos opcionales no bloquean el avance

---

## 🏗️ Implementación

### 1. Modelo de Datos

**Campo en Artifact Entity:**

```csharp
public bool IsMandatory { get; set; }
```

**Base de Datos:**

```sql
-- Columna en tabla artifacts
is_mandatory BOOLEAN NOT NULL DEFAULT FALSE
```

### 2. DTOs Actualizados

**CreateArtifactDto:**

```csharp
public record CreateArtifactDto(
    Guid ProjectId,
    string PhaseId,
    Guid ArtifactTypeId,
    string Title,
    string? Description,
    string? Author,
    bool IsMandatory,  // ← Campo agregado
    string? ContentText,
    string? FileCategory,
    string? RepositoryUrl,
    string? RepositoryVersion,
    string? BuildNumber
);
```

**UpdateArtifactDto:**

```csharp
public record UpdateArtifactDto(
    string? Title,
    string? Description,
    string? Author,
    string? Status,
    bool? IsMandatory,  // ← Campo agregado (nullable)
    string? ContentText,
    string? FileCategory,
    string? RepositoryUrl,
    string? RepositoryVersion,
    string? BuildNumber
);
```

**ArtifactDto:** Ya incluía el campo `IsMandatory`

### 3. DTOs de Validación

**PhaseValidationDto:**

```csharp
public record PhaseValidationDto(
    bool CanAdvance,                           // ¿Puede avanzar?
    string Phase,                              // ID de la fase
    int TotalMandatoryArtifacts,              // Total de obligatorios
    int CompletedMandatoryArtifacts,          // Obligatorios completados
    List<MissingArtifactDto> MissingArtifacts, // Artefactos faltantes
    string? Message                            // Mensaje descriptivo
);
```

**MissingArtifactDto:**

```csharp
public record MissingArtifactDto(
    Guid ArtifactId,
    string Title,
    string ArtifactType,
    string Status,
    bool HasVersions
);
```

### 4. Servicio de Validación

**Método:** `ValidatePhaseCompletionAsync(Guid projectId, string phaseId)`

**Lógica:**

1. Obtiene todos los artefactos de la fase del proyecto
2. Filtra solo los artefactos con `IsMandatory = true`
3. Para cada artefacto obligatorio, verifica si tiene al menos una versión
4. Genera lista de artefactos faltantes (sin versiones)
5. Retorna `CanAdvance = true` solo si todos los obligatorios tienen versiones

### 5. Endpoint REST

```
GET /api/projects/{projectId}/phases/{phaseId}/validate
```

**Autenticación:** JWT Bearer Token  
**Autorización:** Usuario debe tener acceso al proyecto

**Response 200 OK:**

```json
{
  "canAdvance": true,
  "phase": "INCEPTION",
  "totalMandatoryArtifacts": 4,
  "completedMandatoryArtifacts": 4,
  "missingArtifacts": [],
  "message": "Todos los artefactos obligatorios están completos. El proyecto puede avanzar a la siguiente fase."
}
```

**Response 200 OK (con artefactos faltantes):**

```json
{
  "canAdvance": false,
  "phase": "INCEPTION",
  "totalMandatoryArtifacts": 4,
  "completedMandatoryArtifacts": 2,
  "missingArtifacts": [
    {
      "artifactId": "guid-1",
      "title": "Documento de Visión",
      "artifactType": "Documento de Visión",
      "status": "Pendiente",
      "hasVersions": false
    },
    {
      "artifactId": "guid-2",
      "title": "Lista de Stakeholders",
      "artifactType": "Lista de Stakeholders",
      "status": "En revisión",
      "hasVersions": false
    }
  ],
  "message": "Faltan 2 artefacto(s) obligatorio(s) por completar."
}
```

---

## 🧪 Plan de Pruebas

### Prerequisitos

1. Servidor API ejecutándose
2. Base de datos PostgreSQL con datos de prueba
3. Token JWT válido
4. Proyecto con artefactos de prueba en fase INCEPTION

### Variables de Prueba

```powershell
$projectId = "guid-del-proyecto"
$phaseId = "INCEPTION"
$token = "Bearer eyJhbG..."
```

---

## Test 1: Crear Artefacto Obligatorio

### Objetivo

Verificar que se puede crear un artefacto marcado como obligatorio.

### Request (PowerShell)

```powershell
$headers = @{
    "Authorization" = $token
    "Content-Type" = "application/json"
}

$body = @{
    projectId = $projectId
    phaseId = "INCEPTION"
    artifactTypeId = "guid-tipo-vision-doc"
    title = "Documento de Visión v1"
    description = "Visión inicial del producto"
    author = "Product Owner"
    isMandatory = $true
    contentText = "Contenido del documento..."
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri "https://localhost:7061/api/projects/$projectId/artifacts" `
    -Method Post `
    -Headers $headers `
    -Body $body

$response | ConvertTo-Json
```

### Resultado Esperado

```json
{
  "id": "guid-artefacto",
  "projectId": "guid-proyecto",
  "phaseId": "INCEPTION",
  "title": "Documento de Visión v1",
  "isMandatory": true,
  "status": "Pendiente",
  ...
}
```

### Criterios de Aceptación

- ✅ Status code: 200 OK
- ✅ `isMandatory` es `true`
- ✅ Artefacto creado en base de datos

---

## Test 2: Crear Artefacto Opcional

### Objetivo

Verificar que se puede crear un artefacto marcado como opcional.

### Request (PowerShell)

```powershell
$body = @{
    projectId = $projectId
    phaseId = "INCEPTION"
    artifactTypeId = "guid-tipo-hl-use-cases"
    title = "Casos de Uso Alto Nivel"
    description = "Opcional - solo para referencia"
    author = "Analista"
    isMandatory = $false
    contentText = "Casos de uso..."
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri "https://localhost:7061/api/projects/$projectId/artifacts" `
    -Method Post `
    -Headers $headers `
    -Body $body

$response | ConvertTo-Json
```

### Resultado Esperado

```json
{
  "id": "guid-artefacto-2",
  "isMandatory": false,
  ...
}
```

### Criterios de Aceptación

- ✅ `isMandatory` es `false`

---

## Test 3: Actualizar Estado Obligatorio

### Objetivo

Verificar que se puede cambiar un artefacto de opcional a obligatorio.

### Request (PowerShell)

```powershell
$artifactId = "guid-artefacto-2"

$body = @{
    isMandatory = $true
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri "https://localhost:7061/api/projects/$projectId/artifacts/$artifactId" `
    -Method Put `
    -Headers $headers `
    -Body $body

$response.isMandatory
```

### Resultado Esperado

```
True
```

### Criterios de Aceptación

- ✅ Campo `isMandatory` actualizado correctamente

---

## Test 4: Validar Fase SIN Artefactos Completados

### Objetivo

Verificar que la validación falla cuando los artefactos obligatorios no tienen versiones.

### Setup

- Proyecto con 3 artefactos obligatorios
- Ninguno tiene versiones creadas

### Request (PowerShell)

```powershell
$response = Invoke-RestMethod -Uri "https://localhost:7061/api/projects/$projectId/phases/$phaseId/validate" `
    -Method Get `
    -Headers $headers

$response | ConvertTo-Json -Depth 5
```

### Resultado Esperado

```json
{
  "canAdvance": false,
  "phase": "INCEPTION",
  "totalMandatoryArtifacts": 3,
  "completedMandatoryArtifacts": 0,
  "missingArtifacts": [
    {
      "artifactId": "guid-1",
      "title": "Documento de Visión v1",
      "artifactType": "Documento de Visión",
      "status": "Pendiente",
      "hasVersions": false
    },
    {
      "artifactId": "guid-2",
      "title": "Lista de Stakeholders",
      "artifactType": "Lista de Stakeholders",
      "status": "Pendiente",
      "hasVersions": false
    },
    {
      "artifactId": "guid-3",
      "title": "Plan de Proyecto v1",
      "artifactType": "Plan de Proyecto",
      "status": "Pendiente",
      "hasVersions": false
    }
  ],
  "message": "Faltan 3 artefacto(s) obligatorio(s) por completar."
}
```

### Criterios de Aceptación

- ✅ `canAdvance` es `false`
- ✅ `totalMandatoryArtifacts` es 3
- ✅ `completedMandatoryArtifacts` es 0
- ✅ `missingArtifacts` contiene 3 elementos
- ✅ Cada elemento en `missingArtifacts` tiene `hasVersions: false`

---

## Test 5: Crear Versión de Artefacto Obligatorio

### Objetivo

Completar un artefacto obligatorio creando una versión.

### Request (PowerShell)

```powershell
$artifactId = "guid-1"

$fileContent = [System.IO.File]::ReadAllBytes("C:\temp\vision_v1.pdf")
$fileName = "vision_v1.pdf"

$boundary = [System.Guid]::NewGuid().ToString()
$bodyLines = @(
    "--$boundary",
    "Content-Disposition: form-data; name=`"ChangeDescription`"",
    "",
    "Versión inicial del documento de visión",
    "--$boundary",
    "Content-Disposition: form-data; name=`"File`"; filename=`"$fileName`"",
    "Content-Type: application/pdf",
    "",
    [System.Text.Encoding]::Latin1.GetString($fileContent),
    "--$boundary--"
)

$body = $bodyLines -join "`r`n"

$response = Invoke-RestMethod -Uri "https://localhost:7061/api/projects/$projectId/artifacts/$artifactId/versions" `
    -Method Post `
    -Headers @{ "Authorization" = $token } `
    -ContentType "multipart/form-data; boundary=$boundary" `
    -Body ([System.Text.Encoding]::Latin1.GetBytes($body))

$response | ConvertTo-Json
```

### Resultado Esperado

```json
{
  "id": "guid-version",
  "artifactId": "guid-1",
  "versionNumber": 1,
  "fileName": "vision_v1.pdf",
  "changeDescription": "Versión inicial del documento de visión"
}
```

### Criterios de Aceptación

- ✅ Versión creada exitosamente
- ✅ `versionNumber` es 1

---

## Test 6: Validar Fase CON 1 Artefacto Completado

### Objetivo

Verificar que la validación detecta correctamente artefactos completados vs pendientes.

### Setup

- 3 artefactos obligatorios
- 1 tiene versión (completado)
- 2 sin versiones (pendientes)

### Request (PowerShell)

```powershell
$response = Invoke-RestMethod -Uri "https://localhost:7061/api/projects/$projectId/phases/$phaseId/validate" `
    -Method Get `
    -Headers $headers

$response | ConvertTo-Json -Depth 5
```

### Resultado Esperado

```json
{
  "canAdvance": false,
  "phase": "INCEPTION",
  "totalMandatoryArtifacts": 3,
  "completedMandatoryArtifacts": 1,
  "missingArtifacts": [
    {
      "artifactId": "guid-2",
      "title": "Lista de Stakeholders",
      "artifactType": "Lista de Stakeholders",
      "status": "Pendiente",
      "hasVersions": false
    },
    {
      "artifactId": "guid-3",
      "title": "Plan de Proyecto v1",
      "artifactType": "Plan de Proyecto",
      "status": "Pendiente",
      "hasVersions": false
    }
  ],
  "message": "Faltan 2 artefacto(s) obligatorio(s) por completar."
}
```

### Criterios de Aceptación

- ✅ `canAdvance` es `false` (aún faltan 2)
- ✅ `completedMandatoryArtifacts` es 1
- ✅ `missingArtifacts` contiene solo 2 elementos (no incluye el completado)

---

## Test 7: Completar Todos los Artefactos Obligatorios

### Objetivo

Crear versiones para todos los artefactos obligatorios restantes.

### Request (PowerShell)

```powershell
# Completar artefacto 2
$artifactId2 = "guid-2"
# ... (similar al Test 5, crear versión)

# Completar artefacto 3
$artifactId3 = "guid-3"
# ... (similar al Test 5, crear versión)
```

### Criterios de Aceptación

- ✅ Ambos artefactos tienen al menos una versión

---

## Test 8: Validar Fase COMPLETA

### Objetivo

Verificar que cuando todos los artefactos obligatorios tienen versiones, la fase puede avanzar.

### Setup

- 3 artefactos obligatorios
- Todos tienen al menos 1 versión

### Request (PowerShell)

```powershell
$response = Invoke-RestMethod -Uri "https://localhost:7061/api/projects/$projectId/phases/$phaseId/validate" `
    -Method Get `
    -Headers $headers

$response | ConvertTo-Json -Depth 5
```

### Resultado Esperado

```json
{
  "canAdvance": true,
  "phase": "INCEPTION",
  "totalMandatoryArtifacts": 3,
  "completedMandatoryArtifacts": 3,
  "missingArtifacts": [],
  "message": "Todos los artefactos obligatorios están completos. El proyecto puede avanzar a la siguiente fase."
}
```

### Criterios de Aceptación

- ✅ `canAdvance` es `true`
- ✅ `completedMandatoryArtifacts` == `totalMandatoryArtifacts`
- ✅ `missingArtifacts` es array vacío
- ✅ Mensaje confirma que puede avanzar

---

## Test 9: Validar con Artefactos Opcionales

### Objetivo

Verificar que los artefactos opcionales NO bloquean el avance de fase.

### Setup

- 3 artefactos obligatorios (todos con versiones)
- 2 artefactos opcionales (sin versiones)

### Request (PowerShell)

```powershell
$response = Invoke-RestMethod -Uri "https://localhost:7061/api/projects/$projectId/phases/$phaseId/validate" `
    -Method Get `
    -Headers $headers

$response.canAdvance
$response.missingArtifacts.Count
```

### Resultado Esperado

```
canAdvance: True
missingArtifacts.Count: 0
```

### Criterios de Aceptación

- ✅ `canAdvance` es `true` (opcionales no bloquean)
- ✅ Los artefactos opcionales NO aparecen en `missingArtifacts`
- ✅ Solo se validan artefactos con `isMandatory = true`

---

## Test 10: Validación Sin Acceso al Proyecto

### Objetivo

Verificar seguridad - usuarios sin acceso no pueden validar.

### Request (PowerShell)

```powershell
$unauthorizedToken = "Bearer token-otro-usuario"

try {
    $response = Invoke-RestMethod -Uri "https://localhost:7061/api/projects/$projectId/phases/$phaseId/validate" `
        -Method Get `
        -Headers @{ "Authorization" = $unauthorizedToken }
} catch {
    $_.Exception.Response.StatusCode.value__
}
```

### Resultado Esperado

```
403
```

### Criterios de Aceptación

- ✅ Status code: 403 Forbidden
- ✅ No se expone información del proyecto

---

## 📊 Matriz de Validación

| Obligatorios | Con Versión | Sin Versión | Opcionales | CanAdvance | Mensaje  |
| ------------ | ----------- | ----------- | ---------- | ---------- | -------- |
| 3            | 0           | 3           | 2          | ❌ false   | Faltan 3 |
| 3            | 1           | 2           | 2          | ❌ false   | Faltan 2 |
| 3            | 2           | 1           | 2          | ❌ false   | Faltan 1 |
| 3            | 3           | 0           | 2          | ✅ true    | Completo |
| 3            | 3           | 0           | 0          | ✅ true    | Completo |

**Nota:** Los artefactos opcionales NUNCA bloquean el avance, sin importar si tienen versiones o no.

---

## ✅ Checklist Final

### Funcionalidad

- [ ] Se puede crear artefacto con `isMandatory = true`
- [ ] Se puede crear artefacto con `isMandatory = false`
- [ ] Se puede actualizar el estado `isMandatory`
- [ ] Validación detecta artefactos obligatorios sin versiones
- [ ] Validación ignora artefactos opcionales
- [ ] `canAdvance = true` solo cuando todos los obligatorios tienen versiones
- [ ] `canAdvance = false` cuando al menos un obligatorio no tiene versión

### API

- [ ] Endpoint `/validate` responde correctamente
- [ ] Respuestas JSON bien formadas
- [ ] Mensajes descriptivos y claros

### Seguridad

- [ ] Validación requiere autenticación JWT
- [ ] Validación requiere acceso al proyecto
- [ ] 403 para usuarios sin acceso

### Base de Datos

- [ ] Campo `is_mandatory` existe y funciona
- [ ] Include de `Versions` trae las versiones del artefacto
- [ ] Consultas optimizadas con índices

---

## 🎯 Resultado Esperado

Después de ejecutar todos los tests:

1. ✅ Switch Obligatorio/Opcional funciona en CREATE y UPDATE
2. ✅ Validación de fase verifica artefactos obligatorios
3. ✅ Solo artefactos con `isMandatory = true` y sin versiones bloquean avance
4. ✅ Artefactos opcionales nunca bloquean avance
5. ✅ Endpoint retorna información clara sobre estado de completitud
6. ✅ Seguridad validada en todos los endpoints

**Sistema listo para integración con frontend.**
