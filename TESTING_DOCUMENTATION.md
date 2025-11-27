# Documentación de Pruebas - OpenUpTool API

## Índice

1. [Resumen](#resumen)
2. [Infraestructura de Pruebas](#infraestructura-de-pruebas)
3. [Pruebas Unitarias de Controladores](#pruebas-unitarias-de-controladores)
4. [Pruebas Unitarias de Servicios](#pruebas-unitarias-de-servicios)
5. [Helpers de Prueba](#helpers-de-prueba)
6. [Ejecución de Pruebas](#ejecución-de-pruebas)
7. [Cobertura y Estadísticas](#cobertura-y-estadísticas)

---

## Resumen

Este documento describe la suite de pruebas implementada para el proyecto OpenUpTool API. Las pruebas están diseñadas usando **xUnit** como framework de testing, **Moq** para crear mocks de dependencias, y **FluentAssertions** para assertions más legibles y expresivas.

### Tecnologías Utilizadas

-   **xUnit 2.9.2**: Framework de pruebas para .NET
-   **Moq 4.20.72**: Librería de mocking para simular dependencias
-   **FluentAssertions 7.0.0**: Librería para assertions expresivas
-   **Microsoft.AspNetCore.Mvc.Testing 9.0.0**: Para pruebas de integración con WebApplicationFactory
-   **Microsoft.EntityFrameworkCore.InMemory 9.0.0**: Base de datos en memoria para pruebas de integración

### Resultados Actuales

-   **Total de pruebas**: 14
-   **Pruebas exitosas**: 14 ✅
-   **Pruebas fallidas**: 0
-   **Tasa de éxito**: 100%

---

## Infraestructura de Pruebas

### Estructura del Proyecto de Pruebas

```
tests/OpenUpTool.Tests/
├── Controllers/
│   ├── AuthControllerTests.cs
│   └── ProjectsControllerTests.cs
├── Services/
│   └── ProjectServiceTests.cs
└── Helpers/
    ├── CustomWebApplicationFactory.cs
    ├── TestDataSeeder.cs
    └── JwtTokenHelper.cs
```

### Configuración del Proyecto

El proyecto de pruebas está configurado con las siguientes características:

**OpenUpTool.Tests.csproj**:

-   Target Framework: **.NET 9.0**
-   SDK: **Microsoft.NET.Sdk.Web** (necesario para pruebas de integración)
-   Paquetes principales instalados
-   Referencias a todos los proyectos de la solución

---

## Pruebas Unitarias de Controladores

### AuthControllerTests

**Ubicación**: `tests/OpenUpTool.Tests/Controllers/AuthControllerTests.cs`

**Propósito**: Verificar el comportamiento del controlador de autenticación, incluyendo login, registro y consulta de usuarios.

#### Pruebas Implementadas

##### 1. `Login_WithValidCredentials_ReturnsOkWithToken`

**Objetivo**: Verificar que un usuario con credenciales válidas recibe un token JWT.

**Escenario**:

-   Se proporciona email y contraseña correctos
-   El servicio de autenticación retorna un LoginResponseDto válido

**Resultado Esperado**:

-   Status Code: 200 OK
-   Body contiene token JWT y datos del usuario

**Código Clave**:

```csharp
_authServiceMock.Setup(s => s.LoginAsync(It.IsAny<LoginDto>()))
    .ReturnsAsync(expectedResponse);
```

##### 2. `Login_WithInvalidCredentials_ReturnsUnauthorized`

**Objetivo**: Verificar que credenciales inválidas resultan en un error 401.

**Escenario**:

-   Se proporcionan credenciales incorrectas
-   El servicio retorna null indicando fallo de autenticación

**Resultado Esperado**:

-   Status Code: 401 Unauthorized
-   Sin token en la respuesta

##### 3. `Register_WithValidData_ReturnsCreatedAtAction`

**Objetivo**: Verificar que un administrador puede registrar nuevos usuarios.

**Escenario**:

-   Usuario autenticado con rol "Admin"
-   Datos de registro válidos (email, password, nombres, rol)

**Resultado Esperado**:

-   Status Code: 201 Created
-   Header Location apunta al endpoint GetUser
-   Body contiene datos del usuario creado

**Características Importantes**:

-   Requiere autorización con rol Admin
-   Simula el contexto de seguridad con ClaimsPrincipal

##### 4. `GetUser_WithValidId_ReturnsUser`

**Objetivo**: Verificar que se puede consultar información de un usuario por su ID.

**Escenario**:

-   Usuario autenticado
-   ID de usuario válido

**Resultado Esperado**:

-   Status Code: 200 OK
-   Body contiene datos completos del usuario

---

### ProjectsControllerTests

**Ubicación**: `tests/OpenUpTool.Tests/Controllers/ProjectsControllerTests.cs`

**Propósito**: Verificar todas las operaciones CRUD de proyectos y el control de acceso basado en roles.

#### Configuración de Pruebas

Cada prueba utiliza un usuario mock autenticado por defecto con rol "Manager":

```csharp
private void SetupAuthenticatedUser()
{
    var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
    {
        new Claim(ClaimTypes.NameIdentifier, _testUserId.ToString()),
        new Claim(ClaimTypes.Role, "Manager")
    }, "mock"));
}
```

#### Pruebas Implementadas

##### 1. `GetAll_ReturnsUserProjects`

**Objetivo**: Verificar que un usuario obtiene solo sus proyectos asignados.

**Escenario**:

-   Usuario autenticado solicita todos sus proyectos
-   El servicio retorna proyectos filtrados por userId

**Resultado Esperado**:

-   Status Code: 200 OK
-   Lista de proyectos del usuario

**Lógica de Negocio Validada**:

-   Filtrado automático por usuario
-   Extracción de userId de claims JWT

##### 2. `GetById_WithValidId_ReturnsProject`

**Objetivo**: Verificar que se puede obtener un proyecto específico si el usuario tiene acceso.

**Escenario**:

-   Usuario autenticado con acceso al proyecto
-   ID de proyecto válido

**Resultado Esperado**:

-   Status Code: 200 OK
-   Detalles completos del proyecto

**Validaciones**:

-   Verificación de acceso del usuario al proyecto
-   Retorno de datos completos incluyendo fases

##### 3. `Create_WithValidData_ReturnsCreatedProject`

**Objetivo**: Verificar la creación de un nuevo proyecto.

**Escenario**:

-   Usuario con rol Manager o Admin
-   Datos completos del proyecto (nombre, identificador, fecha inicio, etc.)

**Resultado Esperado**:

-   Status Code: 201 Created
-   Header Location apunta al nuevo proyecto
-   Proyecto incluye 4 fases de OpenUP creadas automáticamente

**Datos de Entrada**:

```csharp
var createDto = new CreateProjectDto(
    "New Project",      // Name
    "NEWPROJ",         // Identifier
    DateTime.UtcNow,   // StartDate
    "Test Owner",      // Owner (opcional)
    "Description",     // Description (opcional)
    new List<string>() // Tags
);
```

##### 4. `Update_WithValidData_ReturnsUpdatedProject`

**Objetivo**: Verificar la actualización de un proyecto existente.

**Escenario**:

-   Usuario con acceso al proyecto
-   Datos parciales a actualizar

**Resultado Esperado**:

-   Status Code: 200 OK
-   Proyecto actualizado con nuevos valores

**Validaciones**:

-   Control de acceso antes de actualizar
-   Actualización parcial permitida

##### 5. `Delete_WithValidId_ReturnsNoContent`

**Objetivo**: Verificar la eliminación de un proyecto.

**Escenario**:

-   Usuario con rol Admin
-   Usuario con acceso al proyecto

**Resultado Esperado**:

-   Status Code: 204 No Content
-   Proyecto eliminado del sistema

**Seguridad**:

-   Solo usuarios Admin pueden eliminar
-   Verificación adicional de acceso al proyecto

---

## Pruebas Unitarias de Servicios

### ProjectServiceTests

**Ubicación**: `tests/OpenUpTool.Tests/Services/ProjectServiceTests.cs`

**Propósito**: Verificar la lógica de negocio de la capa de servicios para gestión de proyectos.

#### Configuración

Utiliza mocks de los repositorios para aislar la lógica de negocio:

```csharp
private readonly Mock<IProjectRepository> _projectRepositoryMock;
private readonly Mock<IPhaseRepository> _phaseRepositoryMock;
private readonly Mock<IProjectUserRoleRepository> _projectUserRoleRepositoryMock;
```

#### Pruebas Implementadas

##### 1. `GetAllProjectsAsync_ReturnsAllProjects`

**Objetivo**: Verificar que se obtienen todos los proyectos del repositorio.

**Lógica Probada**:

-   Llamada al repositorio
-   Mapeo de entidades a DTOs
-   Retorno de colección

**Verificación**:

```csharp
_projectRepositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
```

##### 2. `GetProjectByIdAsync_WithValidId_ReturnsProject`

**Objetivo**: Verificar la consulta de un proyecto específico.

**Lógica Probada**:

-   Consulta por ID
-   Mapeo de entidad a DTO
-   Manejo de proyecto no encontrado (retorna null)

##### 3. `HasUserAccessAsync_WithAccess_ReturnsTrue`

**Objetivo**: Verificar la lógica de control de acceso a proyectos.

**Lógica Probada**:

-   Consulta de relación usuario-proyecto
-   Retorno booleano indicando acceso

**Importancia**: Crítico para seguridad y autorización

##### 4. `CreateProjectAsync_CreatesProjectWithPhases`

**Objetivo**: Verificar la creación completa de un proyecto con todas sus dependencias.

**Lógica Compleja Probada**:

1. Creación del proyecto
2. Creación automática de 4 fases OpenUP:
    - Incepción (INCEPTION)
    - Elaboración (ELABORATION)
    - Construcción (CONSTRUCTION)
    - Transición (TRANSITION)
3. Asignación automática del creador como Manager
4. Recarga del proyecto con fases

**Verificaciones Múltiples**:

```csharp
// Verifica creación de 4 fases
_phaseRepositoryMock.Verify(r => r.CreateManyAsync(
    It.Is<IEnumerable<Phase>>(phases => phases.Count() == 4)),
    Times.Once);

// Verifica asignación de rol
_projectUserRoleRepositoryMock.Verify(r => r.CreateAsync(
    It.Is<ProjectUserRole>(pur => pur.UserId == userId)),
    Times.Once);
```

##### 5. `DeleteProjectAsync_CallsRepositoryDelete`

**Objetivo**: Verificar que la eliminación llama correctamente al repositorio.

**Lógica Probada**:

-   Delegación al repositorio
-   Propagación de ID correcto

---

## Helpers de Prueba

### CustomWebApplicationFactory

**Ubicación**: `tests/OpenUpTool.Tests/Helpers/CustomWebApplicationFactory.cs`

**Propósito**: Proporcionar una factory personalizada para pruebas de integración con base de datos en memoria.

**Características**:

-   Hereda de `WebApplicationFactory<Program>`
-   Reemplaza el DbContext real con uno en memoria
-   Permite pruebas end-to-end sin base de datos física

**Código Clave**:

```csharp
services.RemoveAll(typeof(DbContextOptions<OpenUpToolDbContext>));
services.AddDbContext<OpenUpToolDbContext>(options =>
{
    options.UseInMemoryDatabase("InMemoryTestDb");
});
```

**Uso**:

```csharp
public class MyIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public MyIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }
}
```

### TestDataSeeder

**Ubicación**: `tests/OpenUpTool.Tests/Helpers/TestDataSeeder.cs`

**Propósito**: Sembrar datos de prueba consistentes en la base de datos en memoria.

**Datos Creados**:

#### Roles

-   **Admin** (ID: 11111111-1111-1111-1111-111111111111)
-   **Manager** (ID: 22222222-2222-2222-2222-222222222222)
-   **Developer** (ID: 33333333-3333-3333-3333-333333333333)

#### Usuarios de Prueba

-   **admin@test.com** - Password: Test123!
-   **manager@test.com** - Password: Test123!
-   **developer@test.com** - Password: Test123!

#### Proyecto de Prueba

-   **Test Project** (ID: 44444444-4444-4444-4444-444444444444)
    -   Identifier: TEST
    -   Con 4 fases OpenUP creadas
    -   Manager asignado al proyecto

**Uso**:

```csharp
using var scope = factory.Services.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<OpenUpToolDbContext>();
TestDataSeeder.SeedTestData(context);
```

### JwtTokenHelper

**Ubicación**: `tests/OpenUpTool.Tests/Helpers/JwtTokenHelper.cs`

**Propósito**: Generar tokens JWT válidos para pruebas de endpoints autenticados.

**Métodos Principales**:

#### `GenerateTestToken(userId, email, role)`

Genera un token JWT personalizado con claims específicos.

#### `GenerateAdminToken()`

Token pre-configurado para usuario Admin.

#### `GenerateManagerToken()`

Token pre-configurado para usuario Manager.

#### `GenerateDeveloperToken()`

Token pre-configurado para usuario Developer.

**Ejemplo de Uso**:

```csharp
var token = JwtTokenHelper.GenerateAdminToken();
_client.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", token);
```

---

## Ejecución de Pruebas

### Comandos Disponibles

#### Ejecutar todas las pruebas

```powershell
dotnet test
```

#### Ejecutar pruebas con salida detallada

```powershell
dotnet test --verbosity detailed
```

#### Ejecutar pruebas de un proyecto específico

```powershell
dotnet test tests/OpenUpTool.Tests/OpenUpTool.Tests.csproj
```

#### Ejecutar pruebas con cobertura de código

```powershell
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

#### Filtrar pruebas por categoría

```powershell
# Ejecutar solo pruebas de controladores
dotnet test --filter FullyQualifiedName~Controllers

# Ejecutar solo pruebas de servicios
dotnet test --filter FullyQualifiedName~Services
```

### Salida Esperada

```
Test summary: total: 14, failed: 0, succeeded: 14, skipped: 0
Build succeeded in 2.4s
```

---

## Cobertura y Estadísticas

### Distribución de Pruebas

| Categoría   | Cantidad | Descripción                            |
| ----------- | -------- | -------------------------------------- |
| Controllers | 9        | Pruebas de endpoints HTTP y respuestas |
| Services    | 5        | Pruebas de lógica de negocio           |
| **Total**   | **14**   | **Todas las pruebas**                  |

### Controladores Cubiertos

| Controlador        | Pruebas | Cobertura                    |
| ------------------ | ------- | ---------------------------- |
| AuthController     | 4       | Login, Register, GetUser     |
| ProjectsController | 5       | CRUD completo + autorización |

### Servicios Cubiertos

| Servicio       | Pruebas | Cobertura                                    |
| -------------- | ------- | -------------------------------------------- |
| ProjectService | 5       | Operaciones principales y creación con fases |

### Escenarios de Prueba

#### ✅ Casos Exitosos (Happy Path)

-   Login con credenciales válidas
-   Creación de proyectos
-   Consulta de proyectos con acceso
-   Actualización de proyectos
-   Eliminación con permisos adecuados

#### ❌ Casos de Error (Negative Cases)

-   Login con credenciales inválidas
-   Acceso sin autenticación
-   Operaciones sin permisos adecuados
-   Consulta de recursos inexistentes

#### 🔒 Casos de Seguridad

-   Verificación de roles (Admin, Manager, Developer)
-   Control de acceso a proyectos
-   Validación de tokens JWT
-   Autorización multinivel

---

## Patrones y Mejores Prácticas Implementadas

### 1. Patrón AAA (Arrange-Act-Assert)

Todas las pruebas siguen este patrón claro:

```csharp
[Fact]
public async Task Example_Test()
{
    // Arrange - Configuración
    var input = CreateTestData();

    // Act - Ejecución
    var result = await _controller.Method(input);

    // Assert - Verificación
    result.Should().BeOfType<OkObjectResult>();
}
```

### 2. Mocking de Dependencias

Se aísla el código bajo prueba usando Moq:

```csharp
_serviceMock.Setup(s => s.Method(It.IsAny<Type>()))
    .ReturnsAsync(expectedResult);
```

### 3. Assertions Expresivas

Uso de FluentAssertions para mayor legibilidad:

```csharp
result.Should().NotBeNull();
result.Should().BeOfType<OkObjectResult>();
projects.Should().HaveCount(2);
```

### 4. Datos de Prueba Consistentes

Uso de helpers y seeders para datos reproducibles.

### 5. Nomenclatura Clara

Convención: `MethodName_Scenario_ExpectedResult`

-   ✅ `Login_WithValidCredentials_ReturnsOkWithToken`
-   ✅ `Create_WithValidData_ReturnsCreatedProject`

### 6. Independencia de Pruebas

Cada prueba es independiente y puede ejecutarse en cualquier orden.

### 7. Verificación de Interacciones

Uso de `Verify` para asegurar llamadas correctas a dependencias:

```csharp
_repositoryMock.Verify(r => r.CreateAsync(It.IsAny<Entity>()), Times.Once);
```

---

## Mantenimiento y Extensión

### Agregar Nuevas Pruebas

#### Para un nuevo controlador:

1. Crear archivo `NombreControllerTests.cs` en `tests/OpenUpTool.Tests/Controllers/`
2. Mockear dependencias (servicios, logger)
3. Configurar autenticación si es necesario
4. Implementar pruebas siguiendo patrón AAA

#### Para un nuevo servicio:

1. Crear archivo `NombreServiceTests.cs` en `tests/OpenUpTool.Tests/Services/`
2. Mockear repositorios y dependencias
3. Probar lógica de negocio aislada
4. Verificar interacciones con repositorios

### Actualizar Datos de Prueba

Modificar `TestDataSeeder.cs` para agregar:

-   Nuevos usuarios
-   Nuevos proyectos
-   Nuevas relaciones

---

## Troubleshooting

### Problema: Pruebas fallan con error de DbContext

**Solución**: Verificar que `CustomWebApplicationFactory` esté configurando correctamente la base de datos en memoria.

### Problema: Pruebas de autenticación fallan

**Solución**: Verificar que `JwtTokenHelper` use la misma clave secreta que la aplicación.

### Problema: Errores de compilación con DTOs

**Solución**: Los DTOs son records posicionales. Asegurarse de pasar todos los parámetros en el orden correcto.

---

## Conclusión

La suite de pruebas implementada proporciona:

✅ **Cobertura funcional** de endpoints principales  
✅ **Validación de lógica de negocio** en servicios  
✅ **Pruebas de seguridad** y autorización  
✅ **Base sólida** para agregar más pruebas  
✅ **Confianza** en cambios y refactorings

### Próximos Pasos Recomendados

1. **Aumentar cobertura** agregando pruebas para:

    - PhasesController
    - IterationsController
    - ArtifactsController
    - NotificationsController
    - InvitationsController

2. **Pruebas de integración** end-to-end usando `CustomWebApplicationFactory`

3. **Pruebas de rendimiento** para endpoints críticos

4. **Integración con CI/CD** para ejecución automática

5. **Reportes de cobertura** usando herramientas como Coverlet y ReportGenerator

---

## Referencias

-   [xUnit Documentation](https://xunit.net/)
-   [Moq Documentation](https://github.com/moq/moq4)
-   [FluentAssertions Documentation](https://fluentassertions.com/)
-   [ASP.NET Core Testing](https://docs.microsoft.com/en-us/aspnet/core/test/)

---

**Fecha de creación**: 26 de Noviembre de 2025  
**Versión**: 1.0  
**Estado**: ✅ Todas las pruebas pasando (14/14)
