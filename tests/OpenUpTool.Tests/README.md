# OpenUpTool Tests

Suite de pruebas unitarias y de integración para OpenUpTool API.

## 🎯 Estado Actual

✅ **14 pruebas en total**  
✅ **100% de éxito**  
✅ **0 pruebas fallidas**

## 🚀 Ejecución Rápida

```powershell
# Ejecutar todas las pruebas
dotnet test

# Con salida detallada
dotnet test --verbosity detailed

# Solo este proyecto
dotnet test tests/OpenUpTool.Tests/OpenUpTool.Tests.csproj
```

## 📦 Tecnologías

- **xUnit 2.9.2** - Framework de testing
- **Moq 4.20.72** - Mocking de dependencias
- **FluentAssertions 7.0.0** - Assertions expresivas
- **Microsoft.AspNetCore.Mvc.Testing** - Testing de integración
- **EntityFrameworkCore.InMemory** - Base de datos en memoria

## 📁 Estructura

```
tests/OpenUpTool.Tests/
├── Controllers/           # Pruebas de controladores (9 tests)
│   ├── AuthControllerTests.cs
│   └── ProjectsControllerTests.cs
├── Services/             # Pruebas de servicios (5 tests)
│   └── ProjectServiceTests.cs
└── Helpers/              # Utilidades de prueba
    ├── CustomWebApplicationFactory.cs
    ├── TestDataSeeder.cs
    └── JwtTokenHelper.cs
```

## 📖 Documentación Completa

Para documentación detallada de todas las pruebas, patrones implementados y guías de mantenimiento, consulta:

**[TESTING_DOCUMENTATION.md](../../TESTING_DOCUMENTATION.md)**

## 🧪 Cobertura

### Controllers
- ✅ AuthController (4 tests)
  - Login con credenciales válidas/inválidas
  - Registro de usuarios
  - Consulta de usuarios
  
- ✅ ProjectsController (5 tests)
  - CRUD completo
  - Control de acceso
  - Autorización por roles

### Services
- ✅ ProjectService (5 tests)
  - Consultas y filtrado
  - Creación con fases automáticas
  - Validación de acceso
  - Eliminación

## 🔧 Helpers Disponibles

### TestDataSeeder
Datos de prueba pre-configurados:
- 3 roles (Admin, Manager, Developer)
- 3 usuarios con passwords conocidos (Test123!)
- 1 proyecto de prueba con 4 fases OpenUP

### JwtTokenHelper
Generación de tokens JWT para pruebas:
```csharp
var token = JwtTokenHelper.GenerateAdminToken();
var token = JwtTokenHelper.GenerateManagerToken();
var token = JwtTokenHelper.GenerateDeveloperToken();
```

### CustomWebApplicationFactory
Factory para pruebas de integración con base de datos en memoria.

## 📊 Resultados de Última Ejecución

```
Test summary: total: 14, failed: 0, succeeded: 14, skipped: 0, duration: 0.7s
Build succeeded in 2.0s
```

## 🎓 Patrones Implementados

- ✅ **AAA Pattern** (Arrange-Act-Assert)
- ✅ **Mocking** con Moq para aislar dependencias
- ✅ **FluentAssertions** para assertions legibles
- ✅ **Test Data Builders** con helpers
- ✅ **Nomenclatura clara**: `MethodName_Scenario_ExpectedResult`

## 🔍 Troubleshooting

### Error: "Cannot find program"
```powershell
# Asegúrate de estar en el directorio raíz del repositorio
cd E:\Escritorio\openuptool-api
```

### Error: "Authentication failed"
Verifica que `JwtTokenHelper` esté configurado con los mismos parámetros que la aplicación.

## 📝 Agregar Nuevas Pruebas

1. Crear archivo en el directorio correspondiente
2. Heredar de la clase base si aplica
3. Usar el patrón AAA
4. Mockear dependencias necesarias
5. Ejecutar y verificar: `dotnet test`

## 🤝 Contribuir

Al agregar nuevas funcionalidades al API:
1. ✅ Crea pruebas ANTES de implementar (TDD)
2. ✅ Asegúrate que todas las pruebas pasen
3. ✅ Documenta pruebas complejas en TESTING_DOCUMENTATION.md
4. ✅ Mantén cobertura alta

---

**Última actualización**: 26 de Noviembre de 2025  
**Mantenido por**: Equipo OpenUpTool
