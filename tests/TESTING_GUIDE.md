# Guía de Testing - OpenUpTool Backend

## ✅ Estado Actual

**Tests implementados:** 9 pruebas unitarias  
**Tests pasando:** 9/9 (100% success rate)  
**Última ejecución:** Build succeeded, 0 failures

## 📁 Estructura de Archivos

```
tests/OpenUpTool.Tests/
├── Controllers/
│   ├── AuthControllerTests.cs     (5 tests) ✅
│   └── ProjectsControllerTests.cs (4 tests) ✅
├── OpenUpTool.Tests.csproj
└── README.md
```

## 🧪 Cobertura de Pruebas

### AuthControllerTests (5 tests) - HU-001, HU-002, HU-003

-   ✅ Register_WithValidData_ReturnsOk
-   ✅ Register_WithExistingEmail_ReturnsBadRequest
-   ✅ Login_WithValidCredentials_ReturnsToken
-   ✅ Login_WithInvalidCredentials_ReturnsUnauthorized
-   ✅ GetCurrentUser_WithValidToken_ReturnsUser

### ProjectsControllerTests (4 tests) - HU-004

-   ✅ GetAll_ReturnsOkWithProjects
-   ✅ GetById_WithValidId_ReturnsOkWithProject
-   ✅ GetById_WithInvalidId_ReturnsNotFound
-   ✅ Create_WithValidData_ReturnsCreatedProject

## 🚀 Cómo Correr las Pruebas

### Opción 1: Desde el directorio raíz

```powershell
cd E:\Escritorio\openuptool-api
dotnet test
```

### Opción 2: Con output detallado

```powershell
dotnet test --verbosity normal
```

### Opción 3: Solo un controlador específico

```powershell
dotnet test --filter "FullyQualifiedName~AuthControllerTests"
```

## 📋 Output Esperado

```
Restore complete (0.5s)
  OpenUpTool.Core succeeded
  OpenUpTool.Infrastructure succeeded
  OpenUpTool.Api succeeded
  OpenUpTool.Tests succeeded
[xUnit.net 00:00:00.00] xUnit.net VSTest Adapter v2.8.2+699d445a1a
[xUnit.net 00:00:00.09]   Discovered:  OpenUpTool.Tests
[xUnit.net 00:00:00.09]   Starting:    OpenUpTool.Tests
[xUnit.net 00:00:00.19]   Finished:    OpenUpTool.Tests

Test summary: total: 9, failed: 0, succeeded: 9, skipped: 0

Build succeeded
```

## 🔍 Anatomía de un Test

```csharp
[Fact]
public async Task MethodName_Scenario_ExpectedResult()
{
    // Arrange: Configurar mocks y datos de prueba
    var mockService = new Mock<IService>();
    mockService.Setup(s => s.Method()).ReturnsAsync(expectedValue);
    var controller = new Controller(mockService.Object);

    // Act: Ejecutar la acción a probar
    var result = await controller.Action();

    // Assert: Verificar el resultado
    var okResult = Assert.IsType<OkObjectResult>(result.Result);
    var value = Assert.IsType<ExpectedType>(okResult.Value);
    Assert.Equal(expectedValue, value);
}
```

## 🛠️ Stack Tecnológico

| Librería                               | Versión | Propósito                |
| -------------------------------------- | ------- | ------------------------ |
| xUnit                                  | 2.9.2   | Framework de testing     |
| Moq                                    | 4.20.72 | Mocking de dependencias  |
| FluentAssertions                       | 7.0.0   | Assertions expresivas    |
| Microsoft.AspNetCore.Mvc.Testing       | 9.0.1   | Testing de integración   |
| Microsoft.EntityFrameworkCore.InMemory | 9.0.1   | BD en memoria para tests |

## 🎯 Puntos Importantes

1. **Patrón AAA**: Todas las pruebas siguen Arrange-Act-Assert
2. **Mocking con Moq**: Se mockan todas las dependencias de servicios
3. **Claims Simulados**: Tests de controladores configuran ClaimsPrincipal para autenticación
4. **Aislamiento**: Cada test es independiente y no comparte estado
5. **Nomenclatura**: `MethodName_Scenario_ExpectedResult`
6. **Program.cs**: Marcado como `partial class` para tests de integración

## 📝 Cómo Agregar Más Tests

### Paso 1: Revisa las interfaces reales

```powershell
# Ver las firmas de métodos reales
cat src/OpenUpTool.Core/Interfaces/IServices.cs
```

### Paso 2: Revisa el controlador

```powershell
# Ver el controlador que quieres probar
cat src/OpenUpTool.Api/Controllers/NombreController.cs
```

### Paso 3: Crea el archivo de test

Usa `ProjectsControllerTests.cs` como plantilla:

-   Mockea todas las dependencias del constructor
-   Setup de ClaimsPrincipal para autenticación
-   Usa las firmas reales de las interfaces (no inventes métodos)
-   Usa los DTOs reales (records con constructores posicionales)

### Paso 4: Ejecuta y verifica

```powershell
dotnet test
```

## ⚠️ Errores Comunes

### Error: Constructor no existe

```
'Controller' does not contain a constructor that takes X arguments
```

**Solución:** Verifica las dependencias reales del controlador

### Error: Método no existe en interfaz

```
'IService' does not contain a definition for 'MethodAsync'
```

**Solución:** Usa el nombre de método exacto de `IServices.cs`

### Error: DTO con constructor posicional

```
'CreateDto' does not contain a definition for 'Property'
```

**Solución:** Los DTOs son records, usa constructores posicionales:

```csharp
var dto = new CreateProjectDto(
    "Name",
    "Identifier",
    DateTime.UtcNow,
    "Owner",
    "Description",
    new List<string> { "tag1" }
);
```

## 📚 Recursos

-   [Documentación xUnit](https://xunit.net/)
-   [Documentación Moq](https://github.com/moq/moq4)
-   [ASP.NET Core Testing](https://learn.microsoft.com/en-us/aspnet/core/test/)

## 🔮 Próximos Pasos

-   [ ] Agregar tests para ArtifactsController (HU-005)
-   [ ] Agregar tests para DefectsController (HU-009)
-   [ ] Agregar tests para IterationsController (HU-008)
-   [ ] Agregar tests para TestExecutionsController
-   [ ] Configurar tests de integración con múltiples proveedores de BD
-   [ ] Implementar cobertura de código con Coverlet
