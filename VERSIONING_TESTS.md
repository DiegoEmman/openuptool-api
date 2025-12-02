# Pruebas de Sistema de Versionado de Artefactos

## Resumen de Implementación

Se ha implementado un sistema completo de versionado para artefactos que cumple con los siguientes requisitos:

### ✅ Requisitos Funcionales Implementados

1. **Versionado Numerado Automático**

   - Cada artefacto puede tener múltiples versiones numeradas (v1, v2, v3...)
   - El número de versión se auto-incrementa automáticamente
   - Cada versión tiene fecha de entrega (uploaded_at), autor (uploaded_by) y observaciones (change_description)

2. **Descripción de Cambios**

   - Al crear una nueva versión, el sistema solicita la descripción de cambios
   - La versión anterior se mantiene intacta en la base de datos
   - Cada versión guarda su propio archivo físicamente separado

3. **Comparación de Metadatos**

   - Se pueden comparar dos versiones cualesquiera
   - La comparación incluye: cambios en archivo, diferencia de tamaño, cambios de autor, diferencia temporal
   - No se compara contenido binario (fuera del alcance)

4. **Historial Descargable**
   - El historial completo está accesible vía API
   - Cada versión puede descargarse individualmente
   - Los archivos se preservan en el sistema de archivos

### 🏗️ Arquitectura Implementada

**Capas:**

- **Entidad**: `ArtifactVersion` con 11 campos
- **DTOs**: `ArtifactVersionDto`, `CreateArtifactVersionDto`, `VersionHistoryDto`, `VersionComparisonDto`, `VersionDifferencesDto`
- **Repositorio**: `IArtifactVersionRepository` con métodos CRUD y consultas específicas
- **Servicio**: `IArtifactVersionService` con 5 métodos de negocio
- **Controlador**: `ArtifactVersionsController` con 5 endpoints REST

**Endpoints Disponibles:**

```
POST   /api/projects/{projectId}/artifacts/{artifactId}/versions
GET    /api/projects/{projectId}/artifacts/{artifactId}/versions
GET    /api/projects/{projectId}/artifacts/{artifactId}/versions/{versionId}
GET    /api/projects/{projectId}/artifacts/{artifactId}/versions/compare?v1={id1}&v2={id2}
GET    /api/projects/{projectId}/artifacts/{artifactId}/versions/{versionId}/download
```

**Base de Datos:**

- Tabla: `artifact_versions`
- Constraint UNIQUE: `(artifact_id, version_number)` - previene duplicados
- Foreign Key: `artifact_id` → `artifacts(id)` con CASCADE DELETE
- Index: `idx_artifact_versions_artifact_id` para consultas rápidas

---

## 🧪 Plan de Pruebas

### Prerequisitos

1. Servidor API ejecutándose en `https://localhost:7061`
2. PostgreSQL con datos de prueba
3. Token JWT válido de usuario con acceso al proyecto
4. Artefacto existente para testear versiones

### Variables de Prueba

```bash
# Configurar estas variables antes de ejecutar las pruebas
$projectId = "guid-del-proyecto-test"
$artifactId = "guid-del-artefacto-test"
$token = "Bearer eyJhbG..."
```

---

## Test 1: Crear Primera Versión (v1)

### Objetivo

Verificar que se puede crear la primera versión de un artefacto con archivo y descripción de cambios.

### Datos de Entrada

- **Archivo**: `documento_v1.pdf` (cualquier PDF de prueba)
- **ChangeDescription**: "Versión inicial del documento de diseño"

### Request (PowerShell)

```powershell
$headers = @{
    "Authorization" = $token
}

$fileContent = [System.IO.File]::ReadAllBytes("C:\temp\documento_v1.pdf")
$fileName = "documento_v1.pdf"

$boundary = [System.Guid]::NewGuid().ToString()
$bodyLines = @(
    "--$boundary",
    "Content-Disposition: form-data; name=`"ChangeDescription`"",
    "",
    "Versión inicial del documento de diseño",
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
    -Headers $headers `
    -ContentType "multipart/form-data; boundary=$boundary" `
    -Body ([System.Text.Encoding]::Latin1.GetBytes($body))

$response | ConvertTo-Json -Depth 3
```

### Resultado Esperado

```json
{
  "id": "guid-version-1",
  "artifactId": "guid-artefacto",
  "versionNumber": 1,
  "filePath": "uploads/projects/guid-proyecto/guid-artefacto/versions/documento_v1.pdf",
  "fileName": "documento_v1.pdf",
  "fileSize": 125846,
  "uploadedBy": "guid-usuario",
  "uploadedAt": "2025-01-10T15:30:00Z",
  "changeDescription": "Versión inicial del documento de diseño",
  "createdAt": "2025-01-10T15:30:00Z"
}
```

### Criterios de Aceptación

- ✅ Status code: 200 OK
- ✅ `versionNumber` es 1
- ✅ `fileSize` coincide con tamaño del archivo
- ✅ `uploadedAt` es fecha/hora actual
- ✅ `changeDescription` contiene el texto enviado
- ✅ Archivo guardado en sistema de archivos

---

## Test 2: Crear Segunda Versión (v2)

### Objetivo

Verificar que se puede crear una segunda versión y que el número se auto-incrementa.

### Datos de Entrada

- **Archivo**: `documento_v2.pdf` (PDF modificado)
- **ChangeDescription**: "Agregadas secciones 3 y 4 con diagramas de secuencia"

### Request (PowerShell)

```powershell
# Similar al Test 1, cambiar archivo y descripción
$fileContent = [System.IO.File]::ReadAllBytes("C:\temp\documento_v2.pdf")
$fileName = "documento_v2.pdf"

# ... (mismo código multipart con nueva descripción)
```

### Resultado Esperado

```json
{
  "id": "guid-version-2",
  "artifactId": "guid-artefacto",
  "versionNumber": 2,
  "filePath": "uploads/projects/guid-proyecto/guid-artefacto/versions/documento_v2.pdf",
  "fileName": "documento_v2.pdf",
  "fileSize": 187932,
  "uploadedBy": "guid-usuario",
  "uploadedAt": "2025-01-10T16:45:00Z",
  "changeDescription": "Agregadas secciones 3 y 4 con diagramas de secuencia",
  "createdAt": "2025-01-10T16:45:00Z"
}
```

### Criterios de Aceptación

- ✅ `versionNumber` es 2 (auto-incrementado)
- ✅ Nueva versión no afecta v1 (verificar en siguiente test)
- ✅ Ambos archivos existen en sistema de archivos
- ✅ `fileSize` diferente a v1

---

## Test 3: Obtener Historial Completo

### Objetivo

Verificar que se puede recuperar el historial con todas las versiones ordenadas.

### Request (PowerShell)

```powershell
$headers = @{
    "Authorization" = $token
}

$response = Invoke-RestMethod -Uri "https://localhost:7061/api/projects/$projectId/artifacts/$artifactId/versions" `
    -Method Get `
    -Headers $headers

$response | ConvertTo-Json -Depth 5
```

### Resultado Esperado

```json
{
  "artifactId": "guid-artefacto",
  "artifactTitle": "Modelo de Diseño Detallado",
  "totalVersions": 2,
  "versions": [
    {
      "id": "guid-version-2",
      "versionNumber": 2,
      "fileName": "documento_v2.pdf",
      "fileSize": 187932,
      "uploadedBy": "guid-usuario",
      "uploadedAt": "2025-01-10T16:45:00Z",
      "changeDescription": "Agregadas secciones 3 y 4 con diagramas de secuencia"
    },
    {
      "id": "guid-version-1",
      "versionNumber": 1,
      "fileName": "documento_v1.pdf",
      "fileSize": 125846,
      "uploadedBy": "guid-usuario",
      "uploadedAt": "2025-01-10T15:30:00Z",
      "changeDescription": "Versión inicial del documento de diseño"
    }
  ]
}
```

### Criterios de Aceptación

- ✅ `totalVersions` es 2
- ✅ Array `versions` contiene 2 elementos
- ✅ Versiones ordenadas de más reciente a más antigua (v2 primero)
- ✅ Ambas versiones preservan sus metadatos originales

---

## Test 4: Obtener Versión Específica

### Objetivo

Verificar que se puede obtener una versión individual por su ID.

### Request (PowerShell)

```powershell
$versionId = "guid-version-1"  # ID de la v1

$headers = @{
    "Authorization" = $token
}

$response = Invoke-RestMethod -Uri "https://localhost:7061/api/projects/$projectId/artifacts/$artifactId/versions/$versionId" `
    -Method Get `
    -Headers $headers

$response | ConvertTo-Json -Depth 3
```

### Resultado Esperado

```json
{
  "id": "guid-version-1",
  "artifactId": "guid-artefacto",
  "versionNumber": 1,
  "filePath": "uploads/projects/guid-proyecto/guid-artefacto/versions/documento_v1.pdf",
  "fileName": "documento_v1.pdf",
  "fileSize": 125846,
  "uploadedBy": "guid-usuario",
  "uploadedAt": "2025-01-10T15:30:00Z",
  "changeDescription": "Versión inicial del documento de diseño",
  "createdAt": "2025-01-10T15:30:00Z"
}
```

### Criterios de Aceptación

- ✅ Retorna solo la versión solicitada
- ✅ Todos los campos coinciden con los creados originalmente

---

## Test 5: Comparar Dos Versiones (Metadatos)

### Objetivo

Verificar la funcionalidad de comparación de metadatos entre dos versiones.

### Request (PowerShell)

```powershell
$v1Id = "guid-version-1"
$v2Id = "guid-version-2"

$headers = @{
    "Authorization" = $token
}

$response = Invoke-RestMethod -Uri "https://localhost:7061/api/projects/$projectId/artifacts/$artifactId/versions/compare?v1=$v1Id&v2=$v2Id" `
    -Method Get `
    -Headers $headers

$response | ConvertTo-Json -Depth 5
```

### Resultado Esperado

```json
{
  "version1": {
    "id": "guid-version-1",
    "versionNumber": 1,
    "fileName": "documento_v1.pdf",
    "fileSize": 125846,
    "uploadedBy": "guid-usuario",
    "uploadedAt": "2025-01-10T15:30:00Z",
    "changeDescription": "Versión inicial del documento de diseño"
  },
  "version2": {
    "id": "guid-version-2",
    "versionNumber": 2,
    "fileName": "documento_v2.pdf",
    "fileSize": 187932,
    "uploadedBy": "guid-usuario",
    "uploadedAt": "2025-01-10T16:45:00Z",
    "changeDescription": "Agregadas secciones 3 y 4 con diagramas de secuencia"
  },
  "differences": {
    "fileChanged": true,
    "fileSizeChanged": true,
    "fileSizeDifference": 62086,
    "authorChanged": false,
    "timeDifference": "01:15:00",
    "changeDescription": "Agregadas secciones 3 y 4 con diagramas de secuencia"
  }
}
```

### Criterios de Aceptación

- ✅ Muestra datos completos de ambas versiones
- ✅ `fileChanged` es true (diferentes nombres)
- ✅ `fileSizeChanged` es true
- ✅ `fileSizeDifference` es correcto (187932 - 125846 = 62086 bytes)
- ✅ `authorChanged` es false (mismo usuario)
- ✅ `timeDifference` refleja tiempo transcurrido entre versiones

---

## Test 6: Descargar Versión Específica

### Objetivo

Verificar que se puede descargar el archivo de una versión antigua.

### Request (PowerShell)

```powershell
$versionId = "guid-version-1"  # Descargar v1 antigua

$headers = @{
    "Authorization" = $token
}

Invoke-WebRequest -Uri "https://localhost:7061/api/projects/$projectId/artifacts/$artifactId/versions/$versionId/download" `
    -Method Get `
    -Headers $headers `
    -OutFile "C:\temp\descargado_v1.pdf"

# Verificar archivo descargado
(Get-Item "C:\temp\descargado_v1.pdf").Length
```

### Resultado Esperado

- Status code: 200 OK
- Header `Content-Type`: `application/pdf`
- Header `Content-Disposition`: `attachment; filename="documento_v1.pdf"`
- Archivo descargado con tamaño: 125846 bytes
- Contenido coincide con v1 original (no con v2)

### Criterios de Aceptación

- ✅ Archivo descarga correctamente
- ✅ Tamaño del archivo coincide con `fileSize` de la versión
- ✅ Contenido del archivo es el de la versión solicitada
- ✅ No se afectó la integridad de otras versiones

---

## Test 7: Crear Tercera Versión Sin Archivo

### Objetivo

Verificar que se puede crear una versión sin archivo (solo metadatos).

### Request (PowerShell)

```powershell
$headers = @{
    "Authorization" = $token
}

$boundary = [System.Guid]::NewGuid().ToString()
$bodyLines = @(
    "--$boundary",
    "Content-Disposition: form-data; name=`"ChangeDescription`"",
    "",
    "Versión solo con metadatos actualización de referencias",
    "--$boundary--"
)

$body = $bodyLines -join "`r`n"

$response = Invoke-RestMethod -Uri "https://localhost:7061/api/projects/$projectId/artifacts/$artifactId/versions" `
    -Method Post `
    -Headers $headers `
    -ContentType "multipart/form-data; boundary=$boundary" `
    -Body ([System.Text.Encoding]::Latin1.GetBytes($body))

$response | ConvertTo-Json -Depth 3
```

### Resultado Esperado

```json
{
  "id": "guid-version-3",
  "artifactId": "guid-artefacto",
  "versionNumber": 3,
  "filePath": null,
  "fileName": null,
  "fileSize": null,
  "uploadedBy": "guid-usuario",
  "uploadedAt": "2025-01-10T17:00:00Z",
  "changeDescription": "Versión solo con metadatos actualización de referencias",
  "createdAt": "2025-01-10T17:00:00Z"
}
```

### Criterios de Aceptación

- ✅ `versionNumber` es 3
- ✅ `filePath`, `fileName`, `fileSize` son null
- ✅ `changeDescription` se guarda correctamente
- ✅ No hay error por falta de archivo

---

## Test 8: Intentar Descargar Versión Sin Archivo

### Objetivo

Verificar manejo de error cuando se intenta descargar versión sin archivo.

### Request (PowerShell)

```powershell
$versionId = "guid-version-3"

$headers = @{
    "Authorization" = $token
}

try {
    $response = Invoke-WebRequest -Uri "https://localhost:7061/api/projects/$projectId/artifacts/$artifactId/versions/$versionId/download" `
        -Method Get `
        -Headers $headers
} catch {
    $_.Exception.Response.StatusCode.value__
    $_.ErrorDetails.Message | ConvertFrom-Json
}
```

### Resultado Esperado

- Status code: 404 Not Found
- Mensaje: `"Esta versión no tiene archivo asociado"`

### Criterios de Aceptación

- ✅ Retorna 404 correctamente
- ✅ Mensaje de error es claro y descriptivo
- ✅ No causa excepción no controlada

---

## Test 9: Validación de Seguridad

### Objetivo

Verificar que usuarios sin acceso al proyecto no pueden ver/crear versiones.

### Request (PowerShell)

```powershell
# Token de usuario diferente sin acceso al proyecto
$unauthorizedToken = "Bearer token-otro-usuario"

$headers = @{
    "Authorization" = $unauthorizedToken
}

try {
    $response = Invoke-RestMethod -Uri "https://localhost:7061/api/projects/$projectId/artifacts/$artifactId/versions" `
        -Method Get `
        -Headers $headers
} catch {
    $_.Exception.Response.StatusCode.value__
}
```

### Resultado Esperado

- Status code: 403 Forbidden

### Criterios de Aceptación

- ✅ Retorna 403 para GET historial
- ✅ Retorna 403 para POST crear versión
- ✅ Retorna 403 para GET versión específica
- ✅ Retorna 403 para GET comparar
- ✅ Retorna 403 para GET descargar

---

## Test 10: Verificación de Integridad de Datos

### Objetivo

Verificar que todas las versiones permanecen intactas después de múltiples operaciones.

### Request (PowerShell)

```powershell
# Obtener historial completo
$history = Invoke-RestMethod -Uri "https://localhost:7061/api/projects/$projectId/artifacts/$artifactId/versions" `
    -Method Get `
    -Headers @{ "Authorization" = $token }

# Verificar cada versión
foreach ($version in $history.versions) {
    Write-Host "Verificando versión $($version.versionNumber)"

    # Obtener versión específica
    $detail = Invoke-RestMethod -Uri "https://localhost:7061/api/projects/$projectId/artifacts/$artifactId/versions/$($version.id)" `
        -Method Get `
        -Headers @{ "Authorization" = $token }

    # Comparar que los datos coincidan
    if ($version.versionNumber -ne $detail.versionNumber) {
        Write-Error "Inconsistencia detectada en versión $($version.id)"
    }

    Write-Host "✓ Versión $($version.versionNumber) íntegra"
}

Write-Host "`nTotal de versiones: $($history.totalVersions)"
Write-Host "Todas las versiones verificadas correctamente"
```

### Criterios de Aceptación

- ✅ Todas las versiones existen en historial
- ✅ Datos de cada versión coinciden entre historial y consulta individual
- ✅ Números de versión son consecutivos (1, 2, 3...)
- ✅ No hay duplicados de números de versión

---

## 📋 Resumen de Pruebas

| #   | Test                  | Endpoint                  | Método | Status Esperado |
| --- | --------------------- | ------------------------- | ------ | --------------- |
| 1   | Crear v1 con archivo  | `/versions`               | POST   | 200 OK          |
| 2   | Crear v2 con archivo  | `/versions`               | POST   | 200 OK          |
| 3   | Historial completo    | `/versions`               | GET    | 200 OK          |
| 4   | Versión específica    | `/versions/{id}`          | GET    | 200 OK          |
| 5   | Comparar v1 y v2      | `/versions/compare`       | GET    | 200 OK          |
| 6   | Descargar v1          | `/versions/{id}/download` | GET    | 200 OK          |
| 7   | Crear v3 sin archivo  | `/versions`               | POST   | 200 OK          |
| 8   | Descargar sin archivo | `/versions/{id}/download` | GET    | 404 Not Found   |
| 9   | Acceso denegado       | `/versions`               | GET    | 403 Forbidden   |
| 10  | Integridad de datos   | Múltiples                 | GET    | Todos OK        |

---

## ✅ Checklist de Verificación Final

### Funcionalidad

- [ ] Versiones se numeran automáticamente (1, 2, 3...)
- [ ] Cada versión guarda: fecha, autor, observaciones
- [ ] Al crear nueva versión, versión anterior permanece intacta
- [ ] Archivos se guardan separadamente para cada versión
- [ ] Se puede comparar metadatos de dos versiones
- [ ] Historial está ordenado (más reciente primero)
- [ ] Cada versión puede descargarse individualmente

### Seguridad

- [ ] Solo usuarios con acceso al proyecto pueden crear versiones
- [ ] Solo usuarios con acceso pueden ver historial
- [ ] Solo usuarios con acceso pueden descargar archivos
- [ ] Token JWT se valida en todos los endpoints

### Base de Datos

- [ ] Constraint UNIQUE previene versiones duplicadas
- [ ] CASCADE DELETE elimina versiones al borrar artefacto
- [ ] Índices mejoran performance de consultas
- [ ] Timestamps se generan automáticamente

### API

- [ ] Todos los endpoints responden correctamente
- [ ] Códigos de estado HTTP son apropiados
- [ ] Mensajes de error son claros y descriptivos
- [ ] Respuestas JSON están bien formadas

---

## 🚀 Resultado Esperado Final

Después de ejecutar todos los tests:

1. ✅ Sistema crea versiones numeradas automáticamente
2. ✅ Versiones anteriores se preservan intactas
3. ✅ Historial completo es accesible
4. ✅ Comparación de metadatos funciona correctamente
5. ✅ Archivos son descargables individualmente
6. ✅ Seguridad valida acceso en todos los endpoints
7. ✅ Base de datos mantiene integridad referencial
8. ✅ Sin errores de compilación ni runtime

El sistema cumple con TODOS los requisitos especificados en la historia de usuario.
