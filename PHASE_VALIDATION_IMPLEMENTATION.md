# Sistema de Validación de Artefactos Obligatorios - Implementación

## 📋 Resumen

Se ha implementado un sistema completo para gestionar artefactos obligatorios/opcionales y validar el avance de fase basado en la completitud de los artefactos obligatorios.

---

## ✅ Criterios de Aceptación Cumplidos

### AC1: Switch Obligatorio/Opcional ✅

- **Implementado:** Campo `IsMandatory` (booleano) en entidad `Artifact`
- **DTOs actualizados:**
  - `CreateArtifactDto`: incluye `bool IsMandatory`
  - `UpdateArtifactDto`: incluye `bool? IsMandatory` (nullable)
  - `ArtifactDto`: ya incluía el campo
- **Base de datos:** Columna `is_mandatory BOOLEAN NOT NULL DEFAULT FALSE`

### AC2: Validación de Avance de Fase ✅

- **Servicio:** `ValidatePhaseCompletionAsync(projectId, phaseId)`
- **Lógica:**
  1. Obtiene artefactos de la fase con `IsMandatory = true`
  2. Verifica que cada uno tenga al menos una versión
  3. Genera lista de artefactos faltantes
  4. Retorna `CanAdvance = true` solo si todos tienen versiones
- **Endpoint:** `GET /api/projects/{projectId}/phases/{phaseId}/validate`

### AC3: Artefactos Opcionales No Bloquean ✅

- La validación filtra `WHERE IsMandatory = true`
- Artefactos con `IsMandatory = false` son ignorados completamente
- No aparecen en `MissingArtifacts` aunque no tengan versiones

---

## 🏗️ Arquitectura

### 1. Capa de Datos

**Entidad Artifact:**

```csharp
public class Artifact
{
    // ... otros campos
    public bool IsMandatory { get; set; }
    public ICollection<ArtifactVersion> Versions { get; set; }
}
```

**Base de Datos:**

```sql
-- Ya existía en 01-init.sql
is_mandatory BOOLEAN NOT NULL DEFAULT FALSE
```

### 2. DTOs

**CreateArtifactDto:**

```csharp
public record CreateArtifactDto(
    Guid ProjectId,
    string PhaseId,
    Guid ArtifactTypeId,
    string Title,
    string? Description,
    string? Author,
    bool IsMandatory,  // ← Nuevo campo
    // ... otros campos
);
```

**UpdateArtifactDto:**

```csharp
public record UpdateArtifactDto(
    string? Title,
    string? Description,
    string? Author,
    string? Status,
    bool? IsMandatory,  // ← Nuevo campo (nullable)
    // ... otros campos
);
```

**PhaseValidationDto:**

```csharp
public record PhaseValidationDto(
    bool CanAdvance,                           // ¿Puede avanzar?
    string Phase,                              // ID de fase
    int TotalMandatoryArtifacts,              // Total obligatorios
    int CompletedMandatoryArtifacts,          // Obligatorios con versiones
    List<MissingArtifactDto> MissingArtifacts, // Artefactos sin versiones
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

### 3. Servicios

**IArtifactService (interfaz):**

```csharp
Task<PhaseValidationDto> ValidatePhaseCompletionAsync(Guid projectId, string phaseId);
```

**ArtifactService (implementación):**

```csharp
public async Task<PhaseValidationDto> ValidatePhaseCompletionAsync(Guid projectId, string phaseId)
{
    // 1. Obtener artefactos de la fase
    var artifacts = await _artifactRepository.GetByProjectAndPhaseAsync(projectId, phaseId);

    // 2. Filtrar solo obligatorios
    var mandatoryArtifacts = artifacts.Where(a => a.IsMandatory).ToList();

    // 3. Identificar faltantes (sin versiones)
    var missingArtifacts = new List<MissingArtifactDto>();
    foreach (var artifact in mandatoryArtifacts)
    {
        var hasVersions = artifact.Versions != null && artifact.Versions.Any();
        if (!hasVersions)
        {
            missingArtifacts.Add(new MissingArtifactDto(
                artifact.Id,
                artifact.Title,
                artifact.ArtifactType?.Name ?? "Desconocido",
                artifact.Status,
                false
            ));
        }
    }

    // 4. Determinar si puede avanzar
    var canAdvance = missingArtifacts.Count == 0;
    var message = canAdvance
        ? "Todos los artefactos obligatorios están completos. El proyecto puede avanzar a la siguiente fase."
        : $"Faltan {missingArtifacts.Count} artefacto(s) obligatorio(s) por completar.";

    return new PhaseValidationDto(
        canAdvance,
        phaseId,
        mandatoryArtifacts.Count,
        mandatoryArtifacts.Count - missingArtifacts.Count,
        missingArtifacts,
        message
    );
}
```

**Cambios en CreateArtifactAsync:**

```csharp
// Antes: IsMandatory = artifactType.IsMandatory
// Ahora: IsMandatory = dto.IsMandatory
```

**Cambios en UpdateArtifactAsync:**

```csharp
if (dto.IsMandatory.HasValue) artifact.IsMandatory = dto.IsMandatory.Value;
```

### 4. Repositorio

**ArtifactRepository:**

```csharp
public async Task<IEnumerable<Artifact>> GetByProjectAndPhaseAsync(Guid projectId, string phaseId)
{
    return await _context.Artifacts
        .Include(a => a.ArtifactType)
        .Include(a => a.Versions)  // ← Agregado para validación
        .Where(a => a.ProjectId == projectId && a.PhaseId == phaseId)
        .OrderBy(a => a.CreatedAt)
        .ToListAsync();
}
```

### 5. Controlador

**ArtifactsController:**

```csharp
/// <summary>
/// Valida si un proyecto puede avanzar de fase verificando artefactos obligatorios
/// </summary>
[HttpGet("{projectId}/phases/{phaseId}/validate")]
public async Task<ActionResult<PhaseValidationDto>> ValidatePhaseCompletion(
    Guid projectId,
    string phaseId)
{
    try
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var hasAccess = await _projectService.HasUserAccessAsync(userId, projectId);
        if (!hasAccess)
            return Forbid();

        var validation = await _artifactService.ValidatePhaseCompletionAsync(projectId, phaseId);

        return Ok(validation);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error al validar finalización de fase {PhaseId} del proyecto {ProjectId}", phaseId, projectId);
        return StatusCode(500, new { message = "Error al validar fase" });
    }
}
```

---

## 🔄 Flujo de Uso

### Escenario 1: Crear Artefacto Obligatorio

```
Frontend → POST /api/projects/{id}/artifacts
Body: { "isMandatory": true, "title": "Documento de Visión", ... }
    ↓
Controlador valida acceso al proyecto
    ↓
Servicio crea Artifact con IsMandatory = true
    ↓
Repositorio guarda en DB con is_mandatory = true
    ↓
Retorna ArtifactDto con isMandatory: true
```

### Escenario 2: Validar Fase (Incompleta)

```
Frontend → GET /api/projects/{id}/phases/INCEPTION/validate
    ↓
Servicio consulta artefactos obligatorios de INCEPTION
    ↓
Encuentra 3 obligatorios: 2 con versiones, 1 sin versiones
    ↓
Genera MissingArtifacts con el artefacto sin versión
    ↓
Retorna { canAdvance: false, missingArtifacts: [1 item] }
```

### Escenario 3: Completar Artefacto

```
Frontend → POST /api/projects/{id}/artifacts/{aid}/versions
Body: FormData con archivo + ChangeDescription
    ↓
Servicio crea ArtifactVersion con version_number = 1
    ↓
Repositorio guarda versión vinculada al artefacto
```

### Escenario 4: Validar Fase (Completa)

```
Frontend → GET /api/projects/{id}/phases/INCEPTION/validate
    ↓
Servicio consulta artefactos obligatorios de INCEPTION
    ↓
Encuentra 3 obligatorios: todos con versiones
    ↓
missingArtifacts = []
    ↓
Retorna { canAdvance: true, message: "Todos completos..." }
```

### Escenario 5: Artefactos Opcionales

```
Proyecto tiene:
- 3 artefactos con IsMandatory = true (todos con versiones)
- 2 artefactos con IsMandatory = false (sin versiones)
    ↓
Validación filtra WHERE IsMandatory = true
    ↓
Solo evalúa los 3 obligatorios
    ↓
Retorna { canAdvance: true } ← Los opcionales no bloquean
```

---

## 📊 Reglas de Validación

| Condición                            | CanAdvance | Acción                |
| ------------------------------------ | ---------- | --------------------- |
| Todos los obligatorios con versiones | ✅ true    | Permitir avance       |
| Al menos 1 obligatorio sin versión   | ❌ false   | Bloquear avance       |
| Opcionales sin versiones             | ✅ -       | Ignorar (no bloquean) |

**Fórmula:**

```
CanAdvance = (Count(MandatoryArtifacts.WithVersions) == Count(MandatoryArtifacts))
```

---

## 🔐 Seguridad

1. **Autenticación:** Todos los endpoints requieren JWT Bearer Token
2. **Autorización:** Valida acceso del usuario al proyecto (`HasUserAccessAsync`)
3. **Respuestas:**
   - 200 OK: Validación exitosa
   - 401 Unauthorized: Sin token o token inválido
   - 403 Forbidden: Usuario sin acceso al proyecto
   - 500 Internal Server Error: Error del servidor

---

## 📦 Archivos Modificados

### Core Layer

1. ✅ `src/OpenUpTool.Core/DTOs/ArtifactDtos.cs`

   - Agregado `IsMandatory` a `CreateArtifactDto`
   - Agregado `IsMandatory?` a `UpdateArtifactDto`
   - Creados `PhaseValidationDto` y `MissingArtifactDto`

2. ✅ `src/OpenUpTool.Core/Interfaces/IServices.cs`

   - Agregado método `ValidatePhaseCompletionAsync` a `IArtifactService`

3. ✅ `src/OpenUpTool.Core/Services/ArtifactServices.cs`
   - Actualizado `CreateArtifactAsync`: usa `dto.IsMandatory`
   - Actualizado `UpdateArtifactAsync`: actualiza `IsMandatory` si se proporciona
   - Implementado `ValidatePhaseCompletionAsync` con lógica de validación

### Infrastructure Layer

4. ✅ `src/OpenUpTool.Infrastructure/Repositories/Repositories.cs`
   - Agregado `.Include(a => a.Versions)` en `GetByProjectAndPhaseAsync`

### API Layer

5. ✅ `src/OpenUpTool.Api/Controllers/ArtifactsController.cs`
   - Agregado endpoint `GET /{projectId}/phases/{phaseId}/validate`

### Documentación

6. ✅ `PHASE_VALIDATION_TESTS.md` - Plan de pruebas completo (10 escenarios)
7. ✅ `PHASE_VALIDATION_IMPLEMENTATION.md` - Este documento

### Base de Datos

- ✅ Campo `is_mandatory` ya existía en `01-init.sql`

---

## ✅ Estado de Implementación

| Componente      | Estado          | Detalles                          |
| --------------- | --------------- | --------------------------------- |
| Modelo de Datos | ✅ Completo     | Campo `IsMandatory` en Artifact   |
| DTOs            | ✅ Completo     | 4 DTOs creados/actualizados       |
| Servicio        | ✅ Completo     | Método de validación implementado |
| Repositorio     | ✅ Completo     | Include de Versions agregado      |
| Controlador     | ✅ Completo     | Endpoint REST con seguridad       |
| Base de Datos   | ✅ Completo     | Columna ya existía                |
| Compilación     | ✅ Sin errores  | Build exitoso                     |
| Pruebas         | 📝 Documentadas | Ver PHASE_VALIDATION_TESTS.md     |

---

## 🧪 Próximos Pasos

1. **Ejecutar Pruebas:** Seguir plan en `PHASE_VALIDATION_TESTS.md`
2. **Validar Funcionalidad:**
   - Crear artefactos obligatorios y opcionales
   - Crear versiones para artefactos
   - Probar endpoint de validación
3. **Integración Frontend:**
   - Switch toggle para IsMandatory en formulario de creación
   - Botón "Validar Fase" que llame al endpoint
   - Dashboard con indicador de progreso de artefactos obligatorios
4. **Mejoras Futuras (opcional):**
   - Validación automática al intentar cambiar fase del proyecto
   - Notificaciones cuando faltan artefactos obligatorios
   - Dashboard visual con % de completitud por fase

---

## 📚 Ejemplos de Respuestas

### Fase Incompleta

```json
{
  "canAdvance": false,
  "phase": "INCEPTION",
  "totalMandatoryArtifacts": 4,
  "completedMandatoryArtifacts": 2,
  "missingArtifacts": [
    {
      "artifactId": "123e4567-e89b-12d3-a456-426614174000",
      "title": "Documento de Visión",
      "artifactType": "Documento de Visión",
      "status": "Pendiente",
      "hasVersions": false
    },
    {
      "artifactId": "123e4567-e89b-12d3-a456-426614174001",
      "title": "Lista de Stakeholders",
      "artifactType": "Lista de Stakeholders",
      "status": "En revisión",
      "hasVersions": false
    }
  ],
  "message": "Faltan 2 artefacto(s) obligatorio(s) por completar."
}
```

### Fase Completa

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

---

## 💡 Notas Técnicas

### Decisiones de Diseño

1. **Versiones como indicador de completitud:**

   - Un artefacto está "completo" si tiene al menos 1 versión
   - No se valida el estado del artefacto (puede estar "Pendiente" pero tener versión)
   - Esto permite iterar sobre entregas (v1, v2, v3...)

2. **Artefactos opcionales:**

   - Se ignoran completamente en la validación
   - No importa su estado ni si tienen versiones
   - Útil para documentación de referencia o anexos

3. **Campo IsMandatory en DTO:**

   - En `CreateArtifactDto`: obligatorio (no nullable)
   - En `UpdateArtifactDto`: opcional (nullable)
   - Permite cambiar un artefacto de opcional a obligatorio y viceversa

4. **Include de Versions:**
   - Se agrega en el repositorio para evitar N+1 queries
   - Solo se hace en `GetByProjectAndPhaseAsync` (usado por validación)
   - Otros métodos no incluyen Versions a menos que sea necesario

---

## 🚀 Resumen Ejecutivo

✅ **Implementación Completa** - Sistema de validación de artefactos obligatorios funcional y listo para pruebas.

**Funcionalidades:**

- Switch Obligatorio/Opcional en artefactos
- Validación de fase basada en completitud de obligatorios
- Artefactos opcionales no bloquean avance
- API REST con seguridad JWT
- Mensajes descriptivos y detallados

**Estado:** Compilado sin errores, listo para integración con frontend.

**Próximo paso:** Ejecutar plan de pruebas documentado en `PHASE_VALIDATION_TESTS.md`
