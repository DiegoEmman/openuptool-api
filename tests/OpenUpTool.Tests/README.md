# OpenUpTool Tests

## Resumen

Este directorio contiene las pruebas unitarias y de integración para el backend de OpenUpTool.

**Total de pruebas:** 9 pruebas unitarias

## Estructura

```
tests/OpenUpTool.Tests/
├── Controllers/              # Pruebas unitarias de controladores
│   ├── AuthControllerTests.cs (5 tests)
│   └── ProjectsControllerTests.cs (4 tests)
└── README.md
```

## Stack de Testing

-   **xUnit 2.9.2**: Framework de testing
-   **Moq 4.20.72**: Librería de mocking
-   **FluentAssertions 7.0.0**: Assertions expresivas
-   **Microsoft.AspNetCore.Mvc.Testing**: Testing de integración con WebApplicationFactory
-   **Microsoft.EntityFrameworkCore.InMemory**: Base de datos en memoria para tests

## Cobertura de Historias de Usuario

Las pruebas actualmente cubren:

-   **HU-001/002/003**: Autenticación y registro (AuthController)
-   **HU-004**: Gestión básica de proyectos (ProjectsController)

## Pruebas Unitarias

### AuthControllerTests (HU-001, HU-002, HU-003) - 5 tests

-   **Register_WithValidData_ReturnsOk**: Verifica registro exitoso
-   **Register_WithExistingEmail_ReturnsBadRequest**: Verifica validación de email duplicado
-   **Login_WithValidCredentials_ReturnsToken**: Verifica login exitoso
-   **Login_WithInvalidCredentials_ReturnsUnauthorized**: Verifica credenciales incorrectas
-   **GetCurrentUser_WithValidToken_ReturnsUser**: Verifica obtención de usuario actual

### ProjectsControllerTests (HU-004) - 4 tests

-   **GetAll_ReturnsOkWithProjects**: Obtiene todos los proyectos del usuario
-   **GetById_WithValidId_ReturnsOkWithProject**: Obtiene proyecto por ID
-   **GetById_WithInvalidId_ReturnsNotFound**: Maneja proyecto inexistente
-   **Create_WithValidData_ReturnsCreatedProject**: Crea nuevo proyecto

## Resultado de Ejecución

```bash
dotnet test --verbosity normal
```

**Resultado:** ✅ 9/9 tests passed (100% success rate)

```
Test summary: total: 9, failed: 0, succeeded: 9, skipped: 0
```

## Ejecución de Pruebas

### Ejecutar todas las pruebas

```bash
cd E:\Escritorio\openuptool-api
dotnet test
```

### Ejecutar con información detallada

```bash
dotnet test --verbosity normal
```

### Ejecutar solo pruebas unitarias

```bash
dotnet test --filter "FullyQualifiedName~Controllers"
```

## Estructura de una Prueba Unitaria

Todas las pruebas siguen el patrón AAA (Arrange-Act-Assert):

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

## Notas Importantes

1. **Mocking**: Se utiliza Moq para simular dependencias en pruebas unitarias
2. **In-Memory Database**: Las pruebas de integración usan EF Core In-Memory
3. **Claims**: Los tests simulan autenticación con ClaimsPrincipal
4. **Isolation**: Cada test es independiente y no comparte estado
5. **Nomenclatura**: `MethodName_Scenario_ExpectedResult`
6. **Program.cs Accessibility**: El archivo Program.cs está marcado como `partial class` para permitir que WebApplicationFactory acceda a él desde los tests de integración

## Cómo Agregar Más Pruebas

Para agregar pruebas de otros controladores:

1. Revisa la interfaz del servicio en `src/OpenUpTool.Core/Interfaces/IServices.cs`
2. Revisa el controlador real en `src/OpenUpTool.Api/Controllers/`
3. Crea un nuevo archivo de test siguiendo el patrón de `ProjectsControllerTests.cs`
4. Asegúrate de mockear todas las dependencias del constructor
5. Usa las firmas de métodos reales de las interfaces

## Próximos Pasos

-   Agregar pruebas para ArtifactsController (HU-005)
-   Agregar pruebas para DefectsController (HU-009)
-   Agregar pruebas para IterationsController (HU-008)
-   Agregar pruebas para TestExecutionsController (HU-009)
-   Implementar pruebas de integración configurando correctamente el test server
-   Agregar cobertura de código con coverlet
