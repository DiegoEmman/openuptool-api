# Sistema de Versionado de Artefactos - Resumen de Implementación

## 📋 Historia de Usuario

**Como** gestor de proyectos  
**Quiero** tener un sistema de versionado para cada artefacto, donde pueda registrar la fecha de entrega y observaciones  
**Para** mantener la trazabilidad de las evoluciones de cada entregable

### Criterios de Aceptación

✅ **AC1:** Cada artefacto permite crear versiones numeradas (v1, v2, …) con fecha, autor y campo observaciones  
✅ **AC2:** Al crear una nueva versión, el sistema solicita la descripción de cambios y guarda la versión anterior intacta  
✅ **AC3:** Se puede comparar metadatos de versiones (no es obligatorio comparar contenido binario)  
✅ **AC4:** El historial de versiones es accesible y descargable

---

## 🏗️ Arquitectura Implementada

### 1. Capa de Entidades (Domain Layer)

**Archivo:** `src/OpenUpTool.Core/Entities/ArtifactVersion.cs`

```csharp
public class ArtifactVersion
{
    public Guid Id { get; set; }
    public Guid ArtifactId { get; set; }
    public int VersionNumber { get; set; }           // Auto-incrementable
    public string? FilePath { get; set; }
    public string? FileName { get; set; }
    public long? FileSize { get; set; }
    public string UploadedBy { get; set; }           // User ID
    public DateTime UploadedAt { get; set; }          // Fecha de entrega
    public string? ChangeDescription { get; set; }    // Observaciones
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public virtual Artifact Artifact { get; set; }
}
```

### 2. DTOs (Data Transfer Objects)

**Archivo:** `src/OpenUpTool.Core/DTOs/ArtifactDtos.cs`

#### ArtifactVersionDto

```csharp
public record ArtifactVersionDto(
    Guid Id,
    Guid ArtifactId,
    int VersionNumber,
    string? FilePath,
    string? FileName,
    long? FileSize,
    string UploadedBy,
    DateTime UploadedAt,
    string? ChangeDescription,
    DateTime CreatedAt
);
```

#### CreateArtifactVersionDto

```csharp
public record CreateArtifactVersionDto(
    string? ChangeDescription,
    string UploadedBy
);
```

#### VersionComparisonDto

```csharp
public record VersionComparisonDto(
    ArtifactVersionDto Version1,
    ArtifactVersionDto Version2,
    VersionDifferencesDto Differences
);
```

#### VersionDifferencesDto

```csharp
public record VersionDifferencesDto(
    bool FileChanged,
    bool FileSizeChanged,
    long? FileSizeDifference,
    bool AuthorChanged,
    TimeSpan TimeDifference,
    string? ChangeDescription
);
```

#### VersionHistoryDto

```csharp
public record VersionHistoryDto(
    Guid ArtifactId,
    string ArtifactTitle,
    int TotalVersions,
    List<ArtifactVersionDto> Versions
);
```

### 3. Interfaces

**Archivo:** `src/OpenUpTool.Core/Interfaces/IServices.cs`

```csharp
public interface IArtifactVersionService
{
    Task<ArtifactVersionDto> CreateVersionAsync(Guid artifactId, CreateArtifactVersionDto dto, Stream? fileStream, string? fileName);
    Task<VersionHistoryDto> GetVersionHistoryAsync(Guid artifactId);
    Task<ArtifactVersionDto?> GetVersionByIdAsync(Guid versionId);
    Task<VersionComparisonDto?> CompareVersionsAsync(Guid versionId1, Guid versionId2);
    Task<Stream?> GetVersionFileAsync(Guid versionId);
}
```

**Archivo:** `src/OpenUpTool.Core/Interfaces/IRepositories.cs`

```csharp
public interface IArtifactVersionRepository
{
    Task<IEnumerable<ArtifactVersion>> GetVersionsByArtifactIdAsync(Guid artifactId);
    Task<ArtifactVersion?> GetByIdAsync(Guid id);
    Task AddAsync(ArtifactVersion version);
    Task<ArtifactVersion> UpdateAsync(ArtifactVersion version);
    Task DeleteAsync(Guid id);
}
```

### 4. Servicio de Negocio

**Archivo:** `src/OpenUpTool.Core/Services/ArtifactVersionService.cs`

**Métodos Implementados:**

1. **CreateVersionAsync**: Crea nueva versión con auto-incremento de número

   - Obtiene max(version_number) + 1 del artefacto
   - Guarda archivo en subcarpeta "versions"
   - Retorna DTO con metadata completa

2. **GetVersionHistoryAsync**: Obtiene historial completo

   - Retorna todas las versiones ordenadas descendente
   - Incluye título del artefacto y conteo total

3. **GetVersionByIdAsync**: Obtiene versión específica

   - Consulta por ID único de versión
   - Retorna null si no existe

4. **CompareVersionsAsync**: Compara dos versiones

   - Calcula diferencias de metadatos
   - Valida que pertenezcan al mismo artefacto
   - Retorna DTO con ambas versiones + diferencias

5. **GetVersionFileAsync**: Obtiene stream del archivo
   - Usado para descarga
   - Retorna null si versión no tiene archivo

### 5. Repositorio

**Archivo:** `src/OpenUpTool.Infrastructure/Repositories/Repositories.cs`

```csharp
public class ArtifactVersionRepository : IArtifactVersionRepository
{
    // GetVersionsByArtifactIdAsync: consulta con OrderByDescending(version_number)
    // GetByIdAsync: FindAsync por ID único
    // AddAsync: establece CreatedAt y UpdatedAt antes de guardar
    // UpdateAsync: actualiza UpdatedAt automáticamente
    // DeleteAsync: elimina por ID
}
```

### 6. Controlador REST API

**Archivo:** `src/OpenUpTool.Api/Controllers/ArtifactVersionsController.cs`

**Endpoints:**

| Método | Ruta                                                                                  | Descripción                  |
| ------ | ------------------------------------------------------------------------------------- | ---------------------------- |
| POST   | `/api/projects/{projectId}/artifacts/{artifactId}/versions`                           | Crear nueva versión          |
| GET    | `/api/projects/{projectId}/artifacts/{artifactId}/versions`                           | Obtener historial completo   |
| GET    | `/api/projects/{projectId}/artifacts/{artifactId}/versions/{versionId}`               | Obtener versión específica   |
| GET    | `/api/projects/{projectId}/artifacts/{artifactId}/versions/compare?v1={id1}&v2={id2}` | Comparar dos versiones       |
| GET    | `/api/projects/{projectId}/artifacts/{artifactId}/versions/{versionId}/download`      | Descargar archivo de versión |

**Seguridad:** Todos los endpoints requieren:

- Autenticación JWT (`[Authorize]`)
- Validación de acceso al proyecto (`HasUserAccessAsync`)

### 7. Base de Datos

**Archivo:** `docker/init-scripts/03-artifact-versions-project-users.sql`

```sql
CREATE TABLE artifact_versions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    artifact_id UUID NOT NULL,
    version_number INT NOT NULL,
    file_path VARCHAR(500),
    file_name VARCHAR(255),
    file_size BIGINT,
    uploaded_by VARCHAR(255),
    uploaded_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    change_description TEXT,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_artifact_versions_artifact
        FOREIGN KEY (artifact_id) REFERENCES artifacts(id) ON DELETE CASCADE,
    CONSTRAINT uq_artifact_versions_artifact_version
        UNIQUE (artifact_id, version_number)
);

CREATE INDEX idx_artifact_versions_artifact_id ON artifact_versions(artifact_id);
```

**Constraints:**

- **UNIQUE (artifact_id, version_number)**: Previene duplicados
- **FOREIGN KEY CASCADE DELETE**: Elimina versiones al borrar artefacto
- **INDEX**: Optimiza consultas por artifact_id

---

## 🔄 Flujo de Uso

### Crear Primera Versión (v1)

```
Usuario → POST /versions con archivo + "Versión inicial"
    ↓
Controlador valida acceso al proyecto
    ↓
Servicio obtiene MAX(version_number) = NULL → asigna 1
    ↓
FileStorageService guarda archivo en /uploads/projects/{projectId}/{artifactId}/versions/
    ↓
Repositorio inserta registro con version_number=1
    ↓
Retorna ArtifactVersionDto con metadata
```

### Crear Segunda Versión (v2)

```
Usuario → POST /versions con archivo + "Agregadas secciones 3 y 4"
    ↓
Servicio obtiene MAX(version_number) = 1 → asigna 2
    ↓
Guarda nuevo archivo (v1 permanece intacto)
    ↓
Inserta nuevo registro con version_number=2
    ↓
Retorna nueva versión
```

### Obtener Historial

```
Usuario → GET /versions
    ↓
Repositorio consulta WHERE artifact_id = {id} ORDER BY version_number DESC
    ↓
Servicio mapea a VersionHistoryDto con lista de versiones
    ↓
Retorna: { artifactTitle, totalVersions, versions[] }
```

### Comparar Versiones

```
Usuario → GET /versions/compare?v1={guid1}&v2={guid2}
    ↓
Repositorio consulta ambas versiones
    ↓
Servicio valida que pertenezcan al mismo artefacto
    ↓
Calcula: fileChanged, fileSizeDiff, authorChanged, timeDiff
    ↓
Retorna VersionComparisonDto con diferencias
```

### Descargar Versión

```
Usuario → GET /versions/{versionId}/download
    ↓
Servicio obtiene metadata de la versión
    ↓
FileStorageService abre stream del archivo
    ↓
Controlador retorna File() con Content-Type apropiado
    ↓
Navegador descarga archivo con nombre original
```

---

## 📊 Diferencias Calculadas en Comparación

| Campo                | Descripción         | Cálculo                                            |
| -------------------- | ------------------- | -------------------------------------------------- |
| `fileChanged`        | ¿Cambió el archivo? | `fileName1 != fileName2 OR fileSize1 != fileSize2` |
| `fileSizeChanged`    | ¿Cambió el tamaño?  | `fileSize1 != fileSize2`                           |
| `fileSizeDifference` | Diferencia en bytes | `fileSize2 - fileSize1`                            |
| `authorChanged`      | ¿Cambió el autor?   | `uploadedBy1 != uploadedBy2`                       |
| `timeDifference`     | Tiempo transcurrido | `uploadedAt2 - uploadedAt1`                        |
| `changeDescription`  | Descripción de v2   | `version2.ChangeDescription`                       |

---

## 🔐 Seguridad

1. **Autenticación JWT**: Todos los endpoints requieren token válido
2. **Autorización por Proyecto**: Valida que usuario tenga acceso al proyecto
3. **Validación de Pertenencia**: Verifica que versión pertenezca al artefacto correcto
4. **Control de Acceso a Archivos**: Solo descarga si usuario tiene acceso

---

## 📦 Archivos Modificados/Creados

### Nuevos Archivos

1. ✅ `src/OpenUpTool.Core/Services/ArtifactVersionService.cs` - Servicio de negocio
2. ✅ `VERSIONING_TESTS.md` - Plan de pruebas completo
3. ✅ `VERSIONING_IMPLEMENTATION.md` - Este documento

### Archivos Modificados

1. ✅ `src/OpenUpTool.Core/Interfaces/IServices.cs` - Agregada interfaz IArtifactVersionService
2. ✅ `src/OpenUpTool.Core/Interfaces/IRepositories.cs` - Renombrado método GetByArtifactIdAsync → GetVersionsByArtifactIdAsync
3. ✅ `src/OpenUpTool.Core/DTOs/ArtifactDtos.cs` - Agregados 3 DTOs de versionado
4. ✅ `src/OpenUpTool.Infrastructure/Repositories/Repositories.cs` - Actualizado ArtifactVersionRepository
5. ✅ `src/OpenUpTool.Api/Controllers/ArtifactVersionsController.cs` - Reescrito con nuevos endpoints
6. ✅ `src/OpenUpTool.Api/Program.cs` - Registrado IArtifactVersionService en DI container

### Base de Datos

1. ✅ `docker/init-scripts/03-artifact-versions-project-users.sql` - Ya existía con schema correcto

---

## ✅ Cumplimiento de Criterios de Aceptación

### AC1: Versiones numeradas con fecha, autor y observaciones

✅ **Implementado**

- `VersionNumber` (int): Auto-incrementa automáticamente (1, 2, 3...)
- `UploadedAt` (DateTime): Fecha/hora de entrega (generada automáticamente)
- `UploadedBy` (string): ID del usuario autor (desde JWT claims)
- `ChangeDescription` (string): Campo de observaciones/comentarios

### AC2: Versión anterior intacta al crear nueva

✅ **Implementado**

- Al crear nueva versión, se calcula `MAX(version_number) + 1`
- No se actualiza registro anterior
- Archivo físico de versión anterior permanece en su ubicación
- Constraint UNIQUE previene sobrescribir versiones

### AC3: Comparar metadatos de versiones

✅ **Implementado**

- Endpoint GET `/versions/compare?v1={id1}&v2={id2}`
- Compara: archivo, tamaño, autor, tiempo transcurrido
- Calcula diferencias numéricas (bytes, timespan)
- No compara contenido binario (fuera de alcance)

### AC4: Historial accesible y descargable

✅ **Implementado**

- GET `/versions`: Retorna historial completo ordenado
- GET `/versions/{id}`: Retorna versión específica
- GET `/versions/{id}/download`: Descarga archivo de cualquier versión
- Formato descargable preserva nombre original del archivo

---

## 🚀 Estado de Implementación

| Componente    | Estado          | Detalles                      |
| ------------- | --------------- | ----------------------------- |
| Entidad       | ✅ Completo     | ArtifactVersion con 11 campos |
| DTOs          | ✅ Completo     | 5 DTOs creados                |
| Servicio      | ✅ Completo     | 5 métodos implementados       |
| Repositorio   | ✅ Completo     | 5 operaciones CRUD            |
| Controlador   | ✅ Completo     | 5 endpoints REST              |
| Base de Datos | ✅ Completo     | Tabla, constraints, índices   |
| Seguridad     | ✅ Completo     | JWT + validación de acceso    |
| Compilación   | ✅ Sin errores  | Build exitoso                 |
| Pruebas       | 📝 Documentadas | Ver VERSIONING_TESTS.md       |

---

## 📝 Próximos Pasos

1. **Ejecutar Pruebas**: Seguir plan en `VERSIONING_TESTS.md`
2. **Validar Funcionalidad**: Verificar 10 escenarios de prueba
3. **Pruebas de Integración**: Testear con frontend
4. **Documentación de API**: Actualizar Swagger/OpenAPI
5. **Optimizaciones** (si necesario):
   - Cache de historial de versiones
   - Compresión de archivos grandes
   - Limpieza de versiones antiguas (política de retención)

---

## 📚 Documentación Relacionada

- **Plan de Pruebas Completo**: `VERSIONING_TESTS.md`
- **Documentación General**: `README.md`
- **Arquitectura**: `ARCHITECTURE.md`
- **Configuración BD**: `DATABASE_MANAGEMENT_GUIDE.md`

---

## 📞 Soporte

Para dudas o problemas con el sistema de versionado:

1. Revisar logs en `/logs/`
2. Verificar estructura de base de datos con `\d artifact_versions`
3. Consultar documentación de pruebas
4. Revisar código fuente en `src/OpenUpTool.Core/Services/ArtifactVersionService.cs`

---

**Fecha de Implementación**: 2025-01-10  
**Versión de API**: 1.0  
**Estado**: ✅ Completado y listo para pruebas
