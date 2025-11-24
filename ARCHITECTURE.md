# ARQUITECTURA DEL PROYECTO - OpenUpTool API

## Estructura del Proyecto

```
OpenUpTool/
├── src/
│   ├── OpenUpTool.Api/              # Capa de Presentación (API REST)
│   │   ├── Controllers/             # Controladores REST
│   │   ├── Extensions/              # Métodos de extensión para configuración
│   │   ├── Middleware/              # Middleware personalizado
│   │   ├── Filters/                 # Filtros de acción y excepción
│   │   ├── Program.cs               # Punto de entrada
│   │   └── appsettings.json         # Configuración
│   │
│   ├── OpenUpTool.Core/             # Capa de Dominio (Lógica de negocio)
│   │   ├── Entities/                # Entidades del dominio
│   │   ├── Interfaces/              # Contratos de repositorios y servicios
│   │   ├── DTOs/                    # Data Transfer Objects
│   │   ├── Specifications/          # Especificaciones para queries
│   │   ├── Enums/                   # Enumeraciones
│   │   └── Exceptions/              # Excepciones personalizadas
│   │
│   └── OpenUpTool.Infrastructure/   # Capa de Infraestructura
│       ├── Data/                    # DbContext y Migrations
│       ├── Repositories/            # Implementaciones de repositorios
│       ├── Services/                # Servicios de infraestructura
│       └── Configurations/          # Configuraciones de Entity Framework
│
├── tests/
│   └── OpenUpTool.Tests/            # Pruebas unitarias y de integración
│
├── docker/
│   └── init-scripts/                # Scripts de inicialización de BD
│
├── docker-compose.yml               # Orquestación de contenedores
├── Dockerfile                       # Imagen de la API
├── .env                            # Variables de entorno (no commitear)
├── .env.example                    # Template de variables de entorno
└── OpenUpTool.sln                  # Solución de .NET

```

## Arquitectura por Capas

### 1. **OpenUpTool.Api (Capa de Presentación)**

- **Responsabilidad**: Exponer endpoints REST, validación de entrada, serialización
- **Dependencias**: Core, Infrastructure
- **Tecnologías**: ASP.NET Core 9, Swagger, JWT Authentication

**Estructura propuesta de Controllers basada en EPICs:**

```
Controllers/
├── ProjectsController.cs           # EPIC 1: Gestión de proyectos
├── PlanningController.cs           # EPIC 2: Plan y seguimiento
├── ArtifactsController.cs          # EPIC 3-4: Artefactos y versionado
├── WorkflowsController.cs          # EPIC 5: Flujos de trabajo
├── IterationsController.cs         # EPIC 6: Iteraciones y microincrementos
├── ConfigurationController.cs      # EPIC 7: Flexibilidad y personalización
├── NotificationsController.cs      # EPIC 8: Notificaciones e integraciones
├── UsersController.cs              # EPIC 9: Usuarios y roles
└── ClosureController.cs            # EPIC 10: Cierre de proyecto
```

### 2. **OpenUpTool.Core (Capa de Dominio)**

- **Responsabilidad**: Lógica de negocio, entidades, interfaces
- **Dependencias**: Ninguna (independiente)
- **Principios**: DDD, SOLID

**Entidades principales sugeridas:**

```
Entities/
├── Project.cs                      # Proyecto OpenUP
├── Phase.cs                        # Fase (Inception, Elaboration, etc.)
├── Artifact.cs                     # Artefacto/entregable
├── ArtifactVersion.cs              # Versión de artefacto
├── Iteration.cs                    # Iteración
├── MicroIncrement.cs               # Microincremento
├── Workflow.cs                     # Flujo de trabajo
├── WorkflowState.cs                # Estado en flujo
├── Defect.cs                       # Defecto/bug
├── TestCase.cs                     # Caso de prueba
├── User.cs                         # Usuario
├── Role.cs                         # Rol
├── ProjectMember.cs                # Miembro del proyecto
├── AuditLog.cs                     # Registro de auditoría
├── Notification.cs                 # Notificación
└── OpenUpTemplate.cs               # Plantilla OpenUP personalizable
```

### 3. **OpenUpTool.Infrastructure (Capa de Infraestructura)**

- **Responsabilidad**: Acceso a datos, servicios externos, implementaciones técnicas
- **Dependencias**: Core
- **Tecnologías**: Entity Framework Core 9, PostgreSQL, SMTP

**Servicios sugeridos:**

```
Services/
├── EmailService.cs                 # Envío de correos
├── FileStorageService.cs           # Almacenamiento de archivos
├── AuditService.cs                 # Auditoría de cambios
└── NotificationService.cs          # Gestión de notificaciones
```

## Patrones de Diseño Aplicados

### Repository Pattern

- Abstracción del acceso a datos
- Interfaces en Core, implementaciones en Infrastructure

### Unit of Work Pattern

- Transacciones atómicas
- Coordinación de múltiples repositorios

### CQRS (Command Query Responsibility Segregation)

- Separación de operaciones de lectura y escritura
- Uso de MediatR para commands y queries

### Specification Pattern

- Queries reutilizables y componibles
- Lógica de filtrado encapsulada

## Flujo de Datos

```
Request → Controller → Validator → MediatR Handler → Repository → DbContext → PostgreSQL
                                          ↓
                                    Domain Logic
                                          ↓
Response ← DTO Mapper ← Entity ← Repository ← Handler
```

## Base de Datos PostgreSQL

**Contenedor Docker configurado con:**

- PostgreSQL 16
- Extensiones: uuid-ossp, pgcrypto
- PgAdmin 4 para administración
- Volúmenes persistentes
- Scripts de inicialización

## Seguridad

1. **Autenticación**: JWT Bearer Tokens
2. **Autorización**: Policy-based con roles
3. **Validación**: FluentValidation
4. **CORS**: Configurado por ambiente
5. **HTTPS**: Redirección configurable
6. **Auditoría**: Registro de todas las operaciones críticas

## Variables de Entorno

El proyecto usa `DotNetEnv` para cargar variables desde `.env`:

- Configuración de BD
- Secretos JWT
- SMTP
- Almacenamiento
- Feature flags

## Siguientes Pasos para Desarrollo

1. **Definir las entidades del dominio** basadas en los EPICs
2. **Crear migraciones de EF Core** para la BD
3. **Implementar repositorios** genéricos y específicos
4. **Desarrollar controllers** por epic
5. **Configurar autenticación JWT**
6. **Implementar validaciones con FluentValidation**
7. **Crear tests unitarios** por capa
8. **Documentar APIs** con Swagger/OpenAPI

## Metodologías Aplicadas

### Scrum

- Organización por sprints/iteraciones (modelo reflejado en entidades)
- Product backlog (Historias de Usuario definidas)
- Control de velocidad del equipo

### XP (Extreme Programming)

- TDD (Tests preparados en estructura)
- Continuous Integration (ready para CI/CD)
- Pair Programming (propiciar con Git)
- Refactoring continuo
- Simple Design (YAGNI aplicado)

## Tecnologías Stack

- **.NET 9.0**: Framework principal
- **ASP.NET Core**: Web API
- **Entity Framework Core 9**: ORM
- **PostgreSQL 16**: Base de datos
- **Swagger/OpenAPI**: Documentación
- **MediatR**: CQRS
- **FluentValidation**: Validaciones
- **xUnit**: Testing
- **Docker & Docker Compose**: Contenedores
- **DotNetEnv**: Variables de entorno
