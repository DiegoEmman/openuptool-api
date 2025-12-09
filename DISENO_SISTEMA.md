# 🏗️ Documento de Diseño de Sistema - OpenUpTool

## Tabla de Contenidos

1. [Visión General del Sistema](#visión-general-del-sistema)
2. [Arquitectura del Sistema](#arquitectura-del-sistema)
3. [Diseño de la Base de Datos](#diseño-de-la-base-de-datos)
4. [Diseño del Backend (API)](#diseño-del-backend-api)
5. [Diseño del Frontend](#diseño-del-frontend)
6. [Integraciones y APIs](#integraciones-y-apis)
7. [Seguridad](#seguridad)
8. [Escalabilidad y Rendimiento](#escalabilidad-y-rendimiento)
9. [Patrones de Diseño](#patrones-de-diseño)
10. [Diagramas del Sistema](#diagramas-del-sistema)

---

## Visión General del Sistema

### Propósito

OpenUpTool es una aplicación web completa para la gestión de proyectos de desarrollo de software siguiendo la metodología **OpenUP** (Open Unified Process). Proporciona herramientas para gestionar las cuatro fases principales del ciclo de vida del proyecto: Inicio, Elaboración, Construcción y Transición.

### Objetivos del Diseño

1. **Modularidad**: Arquitectura en capas con separación clara de responsabilidades
2. **Escalabilidad**: Capacidad de soportar múltiples proyectos y usuarios concurrentes
3. **Mantenibilidad**: Código limpio, documentado y siguiendo principios SOLID
4. **Seguridad**: Autenticación robusta, autorización basada en roles, auditoría completa
5. **Flexibilidad**: Configuración personalizable y extensible
6. **Usabilidad**: Interfaz intuitiva y responsive

### Stack Tecnológico

#### Backend

- **Framework**: .NET 9.0 / ASP.NET Core
- **ORM**: Entity Framework Core 9.0
- **Base de Datos**: PostgreSQL 15/16
- **Autenticación**: JWT Bearer Tokens
- **Documentación**: Swagger/OpenAPI 7.2
- **Validación**: FluentValidation 11.3
- **Patterns**: MediatR 12.4 (CQRS)
- **Testing**: xUnit 2.9

#### Frontend

- **Framework**: React 19.1
- **Router**: React Router 7.9
- **UI Library**: Material-UI (MUI) 5.18
- **State Management**: React Query (TanStack Query) 5.62
- **Gráficos**: Recharts 3.5
- **Build Tool**: Vite 7.1
- **Lenguaje**: TypeScript 5.9

#### Infraestructura

- **Contenedores**: Docker & Docker Compose
- **Proxy Reverso**: Nginx (opcional)
- **Variables de Entorno**: DotNetEnv 3.1

---

## Arquitectura del Sistema

### Arquitectura de Alto Nivel

```
┌─────────────────────────────────────────────────────────────┐
│                        Internet / Usuarios                   │
└──────────────────────────┬──────────────────────────────────┘
                           │
                           ▼
                  ┌─────────────────┐
                  │   Load Balancer  │ (Opcional)
                  │     (Nginx)      │
                  └────────┬─────────┘
                           │
        ┌──────────────────┴──────────────────┐
        │                                     │
        ▼                                     ▼
┌──────────────────┐               ┌──────────────────┐
│  Frontend (SPA)  │               │  Frontend (SPA)  │
│   React + Vite   │               │   React + Vite   │
│   Puerto: 3000   │               │   Puerto: 3001   │
└────────┬─────────┘               └────────┬─────────┘
         │                                   │
         └───────────────┬───────────────────┘
                         │
                         │ REST API (HTTP/HTTPS)
                         ▼
              ┌─────────────────────┐
              │   API REST (.NET)   │
              │   ASP.NET Core 9    │
              │   Puerto: 5000/5001 │
              └──────────┬──────────┘
                         │
         ┌───────────────┼───────────────┐
         │               │               │
         ▼               ▼               ▼
┌─────────────┐ ┌─────────────┐ ┌─────────────┐
│ PostgreSQL  │ │File Storage │ │SMTP Server  │
│  DB: 5432   │ │ (Uploads)   │ │(Email)      │
└─────────────┘ └─────────────┘ └─────────────┘
```

### Arquitectura en Capas (Backend)

```
┌─────────────────────────────────────────────────────────┐
│                  Presentation Layer                      │
│  (OpenUpTool.Api - Controllers, Middleware, Filters)    │
└──────────────────────┬──────────────────────────────────┘
                       │
                       │ HTTP/REST
                       ▼
┌─────────────────────────────────────────────────────────┐
│                   Application Layer                      │
│     (Services, Command Handlers, Query Handlers)        │
└──────────────────────┬──────────────────────────────────┘
                       │
                       │ Business Logic
                       ▼
┌─────────────────────────────────────────────────────────┐
│                    Domain Layer                          │
│  (OpenUpTool.Core - Entities, DTOs, Interfaces, Enums)  │
└──────────────────────┬──────────────────────────────────┘
                       │
                       │ Data Access
                       ▼
┌─────────────────────────────────────────────────────────┐
│                 Infrastructure Layer                     │
│ (OpenUpTool.Infrastructure - Repositories, DbContext)   │
└──────────────────────┬──────────────────────────────────┘
                       │
                       ▼
                 [PostgreSQL DB]
```

### Flujo de Petición

```
1. User Action (Frontend)
        ↓
2. HTTP Request → API Controller
        ↓
3. Authentication Middleware (JWT Validation)
        ↓
4. Authorization Filter (Role/Permission Check)
        ↓
5. Input Validation (FluentValidation)
        ↓
6. Service Layer (Business Logic)
        ↓
7. Repository Layer (Data Access)
        ↓
8. DbContext → PostgreSQL
        ↓
9. Entity Mapping → DTO
        ↓
10. HTTP Response → Frontend
        ↓
11. State Update (React Query)
        ↓
12. UI Render (React Components)
```

---

## Diseño de la Base de Datos

### Modelo Entidad-Relación (Principales Entidades)

#### Dominio de Proyectos

```
┌─────────────────┐
│    Project      │
├─────────────────┤
│ Id (PK)         │
│ Name            │
│ Identifier      │
│ StartDate       │
│ Status          │
│ Owner           │
│ Description     │
│ IsArchived      │
│ PlanId (FK)     │
└────────┬────────┘
         │
         │ 1:1
         ▼
┌─────────────────┐
│  ProjectPlan    │
├─────────────────┤
│ Id (PK)         │
│ ProjectId (FK)  │
│ Objectives      │
│ Scope           │
│ Resources       │
│ Risks           │
└─────────────────┘
```

#### Dominio de Fases e Iteraciones

```
┌─────────────────┐
│     Phase       │
├─────────────────┤
│ Id (PK)         │
│ ProjectId (FK)  │
│ Name            │
│ Description     │
│ StartDate       │
│ EndDate         │
│ Status          │
└─────────────────┘

┌─────────────────┐
│   Iteration     │
├─────────────────┤
│ Id (PK)         │
│ ProjectId (FK)  │
│ Name            │
│ Number          │
│ StartDate       │
│ EndDate         │
│ Goals           │
│ Status          │
└────────┬────────┘
         │
         │ 1:N
         ▼
┌─────────────────┐
│IterationProgress│
├─────────────────┤
│ Id (PK)         │
│ IterationId(FK) │
│ Date            │
│ HoursWorked     │
│ PercentComplete │
│ Notes           │
└─────────────────┘
```

#### Dominio de Artefactos

```
┌─────────────────┐        ┌─────────────────┐
│  ArtifactType   │        │    Artifact     │
├─────────────────┤        ├─────────────────┤
│ Id (PK)         │   N:1  │ Id (PK)         │
│ Name            │◄───────│ TypeId (FK)     │
│ Phase           │        │ ProjectId (FK)  │
│ Description     │        │ Name            │
│ IsRequired      │        │ Description     │
└─────────────────┘        │ IsRequired      │
                           │ WorkflowId (FK) │
                           │ CurrentState    │
                           └────────┬────────┘
                                    │
                                    │ 1:N
                                    ▼
                           ┌─────────────────┐
                           │ArtifactVersion  │
                           ├─────────────────┤
                           │ Id (PK)         │
                           │ ArtifactId (FK) │
                           │ VersionNumber   │
                           │ FilePath        │
                           │ ChangeLog       │
                           │ CreatedBy       │
                           │ CreatedAt       │
                           └─────────────────┘
```

#### Dominio de Flujos de Trabajo

```
┌─────────────────┐
│    Workflow     │
├─────────────────┤
│ Id (PK)         │
│ ProjectId (FK)  │
│ Name            │
│ Description     │
└────────┬────────┘
         │
         │ 1:N
         ▼
┌─────────────────┐
│ WorkflowState   │
├─────────────────┤
│ Id (PK)         │
│ WorkflowId (FK) │
│ Name            │
│ Description     │
│ Order           │
│ IsFinal         │
│ Color           │
└────────┬────────┘
         │
         │ M:N
         ▼
┌──────────────────────┐
│WorkflowStateResp.    │
├──────────────────────┤
│ Id (PK)              │
│ WorkflowStateId (FK) │
│ RoleId (FK)          │
└──────────────────────┘
```

#### Dominio de Pruebas y Defectos

```
┌─────────────────┐
│ TestExecution   │
├─────────────────┤
│ Id (PK)         │
│ ProjectId (FK)  │
│ TestName        │
│ Description     │
│ Type            │
│ ExecutedAt      │
│ Result          │
│ Evidence        │
└────────┬────────┘
         │
         │ 1:N
         ▼
┌─────────────────┐
│     Defect      │
├─────────────────┤
│ Id (PK)         │
│ ProjectId (FK)  │
│ TestExecutionId │
│ Title           │
│ Description     │
│ Severity        │
│ Priority        │
│ Status          │
│ Type            │
│ AssignedTo      │
│ ReportedBy      │
│ ResolvedBy      │
└─────────────────┘
```

#### Dominio de Usuarios y Seguridad

```
┌─────────────────┐
│      User       │
├─────────────────┤
│ Id (PK)         │
│ Email           │
│ PasswordHash    │
│ Name            │
│ IsActive        │
│ CreatedAt       │
└────────┬────────┘
         │
         │ M:N (a través de ProjectUserRole)
         │
         ▼
┌─────────────────┐        ┌─────────────────┐
│ProjectUserRole  │   N:1  │      Role       │
├─────────────────┤◄───────├─────────────────┤
│ Id (PK)         │        │ Id (PK)         │
│ ProjectId (FK)  │        │ Name            │
│ UserId (FK)     │        │ Description     │
│ RoleId (FK)     │        │ Permissions     │
└─────────────────┘        └─────────────────┘
```

#### Dominio de Auditoría

```
┌─────────────────┐
│   AuditLog      │
├─────────────────┤
│ Id (PK)         │
│ UserId (FK)     │
│ ProjectId (FK)  │
│ EntityType      │
│ EntityId        │
│ Action          │
│ Changes         │
│ Timestamp       │
│ IpAddress       │
└─────────────────┘
```

### Índices y Optimización

**Índices Principales**:

- `IX_Projects_Status` en Projects(Status)
- `IX_Projects_IsArchived` en Projects(IsArchived)
- `IX_Artifacts_ProjectId_Phase` en Artifacts(ProjectId, Phase)
- `IX_AuditLogs_ProjectId_Timestamp` en AuditLogs(ProjectId, Timestamp)
- `IX_Users_Email` (UNIQUE) en Users(Email)
- `IX_Iterations_ProjectId_Status` en Iterations(ProjectId, Status)

**Claves Foráneas con Cascada**:

- Project → ProjectPlan (CASCADE DELETE)
- Project → Phases (CASCADE DELETE)
- Artifact → ArtifactVersions (CASCADE DELETE)
- Workflow → WorkflowStates (CASCADE DELETE)

### Tipos de Datos Especiales

- **JSONB**: Para campos como `Tags` en Project, `Changes` en AuditLog
- **TIMESTAMP**: Con zona horaria deshabilitada (EnableLegacyTimestampBehavior)
- **UUID**: Para todos los IDs (Guid en C#)

---

## Diseño del Backend (API)

### Estructura de Proyectos

```
OpenUpTool.Api/
├── Controllers/
│   ├── ProjectsController.cs
│   ├── ArtifactsController.cs
│   ├── IterationsController.cs
│   ├── WorkflowsController.cs
│   ├── NotificationsController.cs
│   ├── AuthController.cs
│   └── ...
├── Middleware/
│   ├── ExceptionHandlingMiddleware.cs
│   └── AuditLoggingMiddleware.cs
├── Filters/
│   ├── AuthorizePermissionAttribute.cs
│   └── ValidateModelAttribute.cs
├── Extensions/
│   └── ServiceCollectionExtensions.cs
├── Program.cs
└── appsettings.json

OpenUpTool.Core/
├── Entities/
│   ├── Project.cs
│   ├── Artifact.cs
│   ├── User.cs
│   └── ...
├── DTOs/
│   ├── ProjectDtos.cs
│   ├── ArtifactDtos.cs
│   └── ...
├── Interfaces/
│   ├── IProjectRepository.cs
│   ├── IProjectService.cs
│   └── ...
└── Services/
    ├── ProjectService.cs
    ├── ArtifactService.cs
    └── ...

OpenUpTool.Infrastructure/
├── Data/
│   ├── OpenUpToolDbContext.cs
│   └── Migrations/
├── Repositories/
│   ├── ProjectRepository.cs
│   ├── ArtifactRepository.cs
│   └── ...
└── Services/
    ├── EmailService.cs
    └── FileStorageService.cs
```

### Controllers y Endpoints

#### ProjectsController

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProjectsController : ControllerBase
{
    // GET api/projects
    [HttpGet]
    public Task<ActionResult<IEnumerable<ProjectDto>>> GetAll();

    // GET api/projects/{id}
    [HttpGet("{id}")]
    public Task<ActionResult<ProjectDto>> GetById(Guid id);

    // POST api/projects
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public Task<ActionResult<ProjectDto>> Create([FromBody] CreateProjectDto dto);

    // PUT api/projects/{id}
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public Task<ActionResult<ProjectDto>> Update(Guid id, [FromBody] UpdateProjectDto dto);

    // DELETE api/projects/{id}
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public Task<ActionResult> Delete(Guid id);

    // POST api/projects/{id}/archive
    [HttpPost("{id}/archive")]
    [Authorize(Roles = "Admin,Manager")]
    public Task<ActionResult> Archive(Guid id);

    // POST api/projects/{id}/restore
    [HttpPost("{id}/restore")]
    [Authorize(Roles = "Admin,Manager")]
    public Task<ActionResult> Restore(Guid id);

    // GET api/projects/{id}/permissions/{permissionAction}
    [HttpGet("{id}/permissions/{permissionAction}")]
    public Task<ActionResult> CheckPermission(Guid id, string permissionAction);

    // GET api/projects/{id}/users
    [HttpGet("{id}/users")]
    public Task<ActionResult<IEnumerable<UserDto>>> GetProjectUsers(Guid id);
}
```

#### ArtifactsController

```csharp
[ApiController]
[Route("api/projects/{projectId}/artifacts")]
[Authorize]
public class ArtifactsController : ControllerBase
{
    // GET api/projects/{projectId}/artifacts
    [HttpGet]
    public Task<ActionResult<IEnumerable<ArtifactDto>>> GetAll(Guid projectId, [FromQuery] string? phase);

    // GET api/projects/{projectId}/artifacts/{id}
    [HttpGet("{id}")]
    public Task<ActionResult<ArtifactDto>> GetById(Guid projectId, Guid id);

    // POST api/projects/{projectId}/artifacts
    [HttpPost]
    public Task<ActionResult<ArtifactDto>> Create(Guid projectId, [FromForm] CreateArtifactDto dto);

    // PUT api/projects/{projectId}/artifacts/{id}
    [HttpPut("{id}")]
    public Task<ActionResult<ArtifactDto>> Update(Guid projectId, Guid id, [FromBody] UpdateArtifactDto dto);

    // DELETE api/projects/{projectId}/artifacts/{id}
    [HttpDelete("{id}")]
    public Task<ActionResult> Delete(Guid projectId, Guid id);

    // POST api/projects/{projectId}/artifacts/{id}/versions
    [HttpPost("{id}/versions")]
    public Task<ActionResult<ArtifactVersionDto>> CreateVersion(Guid projectId, Guid id, [FromForm] CreateVersionDto dto);

    // GET api/projects/{projectId}/artifacts/{id}/versions
    [HttpGet("{id}/versions")]
    public Task<ActionResult<IEnumerable<ArtifactVersionDto>>> GetVersions(Guid projectId, Guid id);

    // POST api/projects/{projectId}/artifacts/{id}/state
    [HttpPost("{id}/state")]
    public Task<ActionResult> ChangeState(Guid projectId, Guid id, [FromBody] ChangeStateDto dto);

    // GET api/projects/{projectId}/artifacts/validate-phase
    [HttpGet("validate-phase")]
    public Task<ActionResult<PhaseValidationResult>> ValidatePhase(Guid projectId, [FromQuery] string phase);
}
```

#### AuthController

```csharp
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    // POST api/auth/login
    [HttpPost("login")]
    public Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto dto);

    // POST api/auth/register
    [HttpPost("register")]
    public Task<ActionResult<UserDto>> Register([FromBody] RegisterDto dto);

    // POST api/auth/refresh
    [HttpPost("refresh")]
    public Task<ActionResult<LoginResponseDto>> RefreshToken([FromBody] RefreshTokenDto dto);

    // POST api/auth/logout
    [HttpPost("logout")]
    [Authorize]
    public Task<ActionResult> Logout();

    // GET api/auth/me
    [HttpGet("me")]
    [Authorize]
    public Task<ActionResult<UserDto>> GetCurrentUser();
}
```

### Servicios Principales

#### IProjectService

```csharp
public interface IProjectService
{
    Task<IEnumerable<ProjectDto>> GetProjectsForUserAsync(Guid userId);
    Task<ProjectDto?> GetProjectByIdAsync(Guid projectId);
    Task<ProjectDto> CreateProjectAsync(CreateProjectDto dto, Guid createdBy);
    Task<ProjectDto> UpdateProjectAsync(Guid projectId, UpdateProjectDto dto, Guid updatedBy);
    Task DeleteProjectAsync(Guid projectId, Guid deletedBy);
    Task ArchiveProjectAsync(Guid projectId, Guid archivedBy);
    Task RestoreProjectAsync(Guid projectId, Guid restoredBy);
    Task<bool> HasUserAccessAsync(Guid userId, Guid projectId);
    Task<IEnumerable<UserDto>> GetProjectUsersAsync(Guid projectId);
}
```

#### IArtifactService

```csharp
public interface IArtifactService
{
    Task<IEnumerable<ArtifactDto>> GetArtifactsAsync(Guid projectId, string? phase);
    Task<ArtifactDto?> GetArtifactByIdAsync(Guid artifactId);
    Task<ArtifactDto> CreateArtifactAsync(Guid projectId, CreateArtifactDto dto, Guid createdBy);
    Task<ArtifactDto> UpdateArtifactAsync(Guid artifactId, UpdateArtifactDto dto, Guid updatedBy);
    Task DeleteArtifactAsync(Guid artifactId, Guid deletedBy);
    Task<ArtifactVersionDto> CreateVersionAsync(Guid artifactId, CreateVersionDto dto, Guid createdBy);
    Task<IEnumerable<ArtifactVersionDto>> GetVersionsAsync(Guid artifactId);
    Task ChangeStateAsync(Guid artifactId, string newState, Guid changedBy);
    Task<PhaseValidationResult> ValidatePhaseAsync(Guid projectId, string phase);
}
```

### Middleware

#### ExceptionHandlingMiddleware

Captura excepciones globales y retorna respuestas HTTP apropiadas:

```csharp
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (NotFoundException ex)
        {
            await HandleExceptionAsync(context, ex, StatusCodes.Status404NotFound);
        }
        catch (UnauthorizedException ex)
        {
            await HandleExceptionAsync(context, ex, StatusCodes.Status401Unauthorized);
        }
        catch (ValidationException ex)
        {
            await HandleExceptionAsync(context, ex, StatusCodes.Status400BadRequest);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex, StatusCodes.Status500InternalServerError);
        }
    }
}
```

#### AuditLoggingMiddleware

Registra automáticamente acciones en AuditLog:

```csharp
public class AuditLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IAuditLogService _auditLogService;

    public async Task InvokeAsync(HttpContext context)
    {
        var startTime = DateTime.UtcNow;

        await _next(context);

        if (ShouldLogRequest(context))
        {
            await LogRequestAsync(context, startTime);
        }
    }

    private bool ShouldLogRequest(HttpContext context)
    {
        return context.Request.Method != "GET" &&
               context.User.Identity?.IsAuthenticated == true;
    }
}
```

### Autenticación y Autorización

#### JWT Configuration

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSecret)
            )
        };
    });
```

#### Role-Based Authorization

```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("ManagerOrAdmin", policy => policy.RequireRole("Manager", "Admin"));
    options.AddPolicy("Developer", policy => policy.RequireRole("Developer", "Manager", "Admin"));
});
```

---

## Diseño del Frontend

### Estructura de Componentes

```
src/
├── app/
│   ├── router.tsx              # Configuración de rutas
│   ├── AppProviders.tsx        # Providers globales
│   └── layout/
│       ├── MainLayout.tsx
│       └── Navbar.tsx
├── pages/
│   ├── Home/
│   │   └── HomePage.tsx
│   ├── Projects/
│   │   ├── ProjectsListPage.tsx
│   │   ├── ProjectDetailPage.tsx
│   │   └── NewProjectPage.tsx
│   ├── Elaboration/
│   │   └── ElaborationPhasePage.tsx
│   ├── Construction/
│   │   └── ConstructionPhasePage.tsx
│   ├── Iterations/
│   │   └── IterationTrackingPage.tsx
│   ├── Workflows/
│   │   └── WorkflowsPage.tsx
│   ├── Testing/
│   │   └── TestingPage.tsx
│   ├── Configuration/
│   │   ├── GlobalConfigPage.tsx
│   │   └── TemplateManagementPage.tsx
│   └── Login/
│       └── LoginPage.tsx
├── components/
│   ├── common/
│   │   ├── Button.tsx
│   │   ├── Modal.tsx
│   │   └── DataTable.tsx
│   ├── artifacts/
│   │   ├── ArtifactCreateForm.tsx
│   │   ├── ArtifactList.tsx
│   │   └── PhaseValidationDialog.tsx
│   ├── iterations/
│   │   ├── IterationProgressPanel.tsx
│   │   ├── IterationTaskList.tsx
│   │   └── BurndownChart.tsx
│   ├── workflow/
│   │   ├── WorkflowForm.tsx
│   │   ├── WorkflowStatesView.tsx
│   │   └── PermissionMatrixView.tsx
│   └── testing/
│       ├── TestExecutionForm.tsx
│       └── DefectForm.tsx
├── services/
│   ├── api.ts                  # Axios instance
│   ├── authService.ts
│   ├── projectService.ts
│   ├── artifactService.ts
│   ├── iterationService.ts
│   └── ...
├── contexts/
│   └── AuthContext.tsx         # Context de autenticación
├── hooks/
│   ├── useAuth.ts
│   └── useElaborationArtifacts.ts
├── types/
│   ├── project.ts
│   ├── artifact.ts
│   ├── iteration.ts
│   └── ...
└── utils/
    ├── formatters.ts
    └── validators.ts
```

### Arquitectura de Componentes

#### Jerarquía de Componentes

```
App
└── AppProviders (Auth, ReactQuery, Theme)
    └── Router
        ├── MainLayout
        │   ├── Navbar
        │   └── Outlet (páginas)
        └── LoginPage (sin layout)
```

#### Ejemplo de Componente de Página

```tsx
// ProjectsListPage.tsx
export function ProjectsListPage() {
  const [projects, setProjects] = useState<Project[]>([]);
  const [loading, setLoading] = useState(true);
  const { hasRole } = useAuth();

  useEffect(() => {
    loadProjects();
  }, []);

  const loadProjects = async () => {
    setLoading(true);
    try {
      const data = await projectService.list();
      setProjects(data);
    } catch (error) {
      console.error(error);
    } finally {
      setLoading(false);
    }
  };

  return (
    <>
      <Navbar />
      <Container sx={{ py: 4 }}>
        <Box display="flex" justifyContent="space-between" mb={3}>
          <Typography variant="h4">Proyectos</Typography>
          {hasRole(["Admin", "Manager"]) && (
            <Button component={Link} to="/projects/new" variant="contained">
              + Nuevo Proyecto
            </Button>
          )}
        </Box>
        {loading ? (
          <CircularProgress />
        ) : (
          <ProjectsTable projects={projects} onRefresh={loadProjects} />
        )}
      </Container>
    </>
  );
}
```

### Gestión de Estado

#### React Query para Data Fetching

```tsx
// useProjects hook
export function useProjects() {
  return useQuery({
    queryKey: ["projects"],
    queryFn: () => projectService.list(),
    staleTime: 5 * 60 * 1000, // 5 minutos
  });
}

// useCreateProject mutation
export function useCreateProject() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateProjectDto) => projectService.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["projects"] });
    },
  });
}
```

#### Context API para Autenticación

```tsx
// AuthContext.tsx
interface AuthContextType {
  user: User | null;
  login: (credentials: LoginCredentials) => Promise<void>;
  logout: () => void;
  isAuthenticated: boolean;
  hasRole: (roles: string[]) => boolean;
}

export const AuthContext = createContext<AuthContextType | undefined>(
  undefined
);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null);

  const login = async (credentials: LoginCredentials) => {
    const response = await authService.login(credentials);
    setUser(response.user);
    localStorage.setItem("token", response.token);
  };

  const logout = () => {
    setUser(null);
    localStorage.removeItem("token");
  };

  const hasRole = (roles: string[]) => {
    return user ? roles.includes(user.role) : false;
  };

  return (
    <AuthContext.Provider
      value={{ user, login, logout, isAuthenticated: !!user, hasRole }}
    >
      {children}
    </AuthContext.Provider>
  );
}
```

### Servicios API

```typescript
// api.ts - Axios instance
const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
  headers: {
    "Content-Type": "application/json",
  },
});

api.interceptors.request.use((config) => {
  const token = localStorage.getItem("token");
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// projectService.ts
export const projectService = {
  list: async (): Promise<Project[]> => {
    const { data } = await api.get("/projects");
    return data;
  },

  getById: async (id: string): Promise<Project> => {
    const { data } = await api.get(`/projects/${id}`);
    return data;
  },

  create: async (dto: CreateProjectDto): Promise<Project> => {
    const { data } = await api.post("/projects", dto);
    return data;
  },

  update: async (id: string, dto: UpdateProjectDto): Promise<Project> => {
    const { data } = await api.put(`/projects/${id}`, dto);
    return data;
  },

  archive: async (id: string): Promise<void> => {
    await api.post(`/projects/${id}/archive`);
  },
};
```

### Enrutamiento

```tsx
// router.tsx
export const router = createBrowserRouter([
  {
    path: "/login",
    element: <LoginPage />,
  },
  {
    element: (
      <ProtectedRoute>
        <MainLayout />
      </ProtectedRoute>
    ),
    children: [
      {
        path: "/",
        element: <HomePage />,
      },
      {
        path: "/projects",
        element: <ProjectsListPage />,
      },
      {
        path: "/projects/new",
        element: <NewProjectPage />,
      },
      {
        path: "/projects/:id",
        element: <ProjectDetailPage />,
      },
      {
        path: "/projects/:id/elaboration",
        element: <ElaborationPhasePage />,
      },
      {
        path: "/projects/:id/construction",
        element: <ConstructionPhasePage />,
      },
      {
        path: "/projects/:id/iterations/:iterationId",
        element: <IterationTrackingPage />,
      },
      {
        path: "/projects/:id/workflows",
        element: <WorkflowsPage />,
      },
      {
        path: "/configuration",
        element: <GlobalConfigPage />,
      },
    ],
  },
]);
```

---

## Integraciones y APIs

### API REST

#### Formato de Respuesta

**Respuesta Exitosa**:

```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "Mi Proyecto",
  "status": "En curso",
  "createdAt": "2024-01-15T10:30:00Z"
}
```

**Respuesta de Error**:

```json
{
  "error": "NotFound",
  "message": "Proyecto no encontrado",
  "statusCode": 404
}
```

#### Códigos de Estado HTTP

- `200 OK`: Operación exitosa
- `201 Created`: Recurso creado exitosamente
- `204 No Content`: Operación exitosa sin contenido de respuesta
- `400 Bad Request`: Datos de entrada inválidos
- `401 Unauthorized`: No autenticado
- `403 Forbidden`: No autorizado (sin permisos)
- `404 Not Found`: Recurso no encontrado
- `500 Internal Server Error`: Error interno del servidor

### Swagger/OpenAPI

Documentación interactiva de la API disponible en:

- Desarrollo: http://localhost:5000/swagger
- Producción: Deshabilitado

### Integraciones Futuras

- **Git**: Integración con repositorios (GitHub, GitLab, Bitbucket)
- **CI/CD**: Webhooks para notificar builds
- **Jira**: Sincronización de issues y tareas
- **Slack/Teams**: Notificaciones en tiempo real
- **Azure Blob Storage / AWS S3**: Almacenamiento de archivos en la nube

---

## Seguridad

### Autenticación

#### JWT (JSON Web Tokens)

**Estructura del Token**:

```json
{
  "sub": "user-id",
  "email": "user@example.com",
  "name": "John Doe",
  "role": "Developer",
  "exp": 1640000000,
  "iss": "OpenUpTool",
  "aud": "OpenUpTool-Users"
}
```

**Flujo de Autenticación**:

1. Usuario envía credenciales al endpoint `/api/auth/login`
2. Backend valida credenciales con BCrypt
3. Genera JWT firmado con secreto
4. Frontend almacena token en localStorage
5. Token se envía en header `Authorization: Bearer <token>` en cada petición
6. Backend valida token en cada petición

### Autorización

#### Matriz de Permisos por Rol

| Acción                  | Admin | Manager | Developer | Tester | Viewer |
| ----------------------- | ----- | ------- | --------- | ------ | ------ |
| Crear Proyecto          | ✅    | ✅      | ❌        | ❌     | ❌     |
| Editar Proyecto         | ✅    | ✅      | ❌        | ❌     | ❌     |
| Archivar Proyecto       | ✅    | ✅      | ❌        | ❌     | ❌     |
| Ver Proyecto            | ✅    | ✅      | ✅        | ✅     | ✅     |
| Crear Artefacto         | ✅    | ✅      | ✅        | ❌     | ❌     |
| Editar Artefacto        | ✅    | ✅      | ✅        | ❌     | ❌     |
| Eliminar Artefacto      | ✅    | ✅      | ❌        | ❌     | ❌     |
| Ejecutar Pruebas        | ✅    | ✅      | ✅        | ✅     | ❌     |
| Reportar Defectos       | ✅    | ✅      | ✅        | ✅     | ❌     |
| Gestionar Configuración | ✅    | ✅      | ❌        | ❌     | ❌     |
| Invitar Usuarios        | ✅    | ✅      | ❌        | ❌     | ❌     |

### Hashing de Contraseñas

```csharp
// Usando BCrypt
public string HashPassword(string password)
{
    return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
}

public bool VerifyPassword(string password, string hash)
{
    return BCrypt.Net.BCrypt.Verify(password, hash);
}
```

### Validación de Entrada

```csharp
// FluentValidation
public class CreateProjectDtoValidator : AbstractValidator<CreateProjectDto>
{
    public CreateProjectDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es obligatorio")
            .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres");

        RuleFor(x => x.Identifier)
            .NotEmpty().WithMessage("El identificador es obligatorio")
            .Matches(@"^[A-Z0-9-]+$").WithMessage("El identificador solo puede contener letras mayúsculas, números y guiones");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("La fecha de inicio es obligatoria")
            .GreaterThanOrEqualTo(DateTime.Today).WithMessage("La fecha de inicio debe ser hoy o futura");
    }
}
```

### CORS

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(corsOrigins.Split(','))
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});
```

### Protección contra Ataques

- **SQL Injection**: Uso de Entity Framework con parámetros parametrizados
- **XSS**: Sanitización de inputs en frontend, Content Security Policy
- **CSRF**: Tokens CSRF en formularios (implementación futura)
- **Rate Limiting**: Limitación de peticiones por IP (configurablecon `ENABLE_RATE_LIMITING`)
- **Clickjacking**: Header `X-Frame-Options: DENY`

---

## Escalabilidad y Rendimiento

### Estrategias de Escalabilidad

#### Escalado Horizontal

- **Frontend**: Múltiples instancias detrás de un load balancer
- **API**: Stateless, puede escalar horizontalmente sin problemas
- **Base de Datos**: Replicación master-slave para lectura

#### Caching

```csharp
// Implementación futura con Redis
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = Configuration.GetConnectionString("Redis");
});

// Cache en memoria para datos estáticos
builder.Services.AddMemoryCache();
```

#### Optimización de Consultas

```csharp
// Eager Loading para evitar N+1
var projects = await _context.Projects
    .Include(p => p.Plan)
    .Include(p => p.Phases)
    .Include(p => p.Iterations)
    .Where(p => !p.IsArchived)
    .ToListAsync();

// Paginación
var projects = await _context.Projects
    .OrderBy(p => p.Name)
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync();
```

#### Compresión

```csharp
// Comprimir respuestas HTTP
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<GzipCompressionProvider>();
});
```

### Métricas de Rendimiento

- **Tiempo de respuesta API**: < 200ms (95th percentile)
- **Tiempo de carga inicial (Frontend)**: < 3s
- **Tamaño del bundle JS**: < 500KB (gzipped)
- **Consultas a BD**: < 50ms (promedio)

---

## Patrones de Diseño

### Repository Pattern

Abstrae el acceso a datos:

```csharp
public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(Guid id);
    Task<IEnumerable<Project>> GetAllAsync();
    Task<Project> AddAsync(Project project);
    Task UpdateAsync(Project project);
    Task DeleteAsync(Guid id);
}
```

### Unit of Work Pattern

Coordina múltiples repositorios en una transacción:

```csharp
public interface IUnitOfWork : IDisposable
{
    IProjectRepository Projects { get; }
    IArtifactRepository Artifacts { get; }
    IUserRepository Users { get; }

    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
```

### Service Layer Pattern

Encapsula lógica de negocio:

```csharp
public class ProjectService : IProjectService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogService _auditLog;

    public async Task<ProjectDto> CreateProjectAsync(CreateProjectDto dto, Guid createdBy)
    {
        // Validación de negocio
        if (await _unitOfWork.Projects.ExistsByIdentifierAsync(dto.Identifier))
            throw new BusinessException("El identificador ya existe");

        // Crear entidad
        var project = new Project
        {
            Name = dto.Name,
            Identifier = dto.Identifier,
            StartDate = dto.StartDate,
            Owner = dto.Owner,
            CreatedAt = DateTime.UtcNow
        };

        // Guardar en BD
        await _unitOfWork.Projects.AddAsync(project);
        await _unitOfWork.SaveChangesAsync();

        // Auditoría
        await _auditLog.LogAsync("ProjectCreated", project.Id, createdBy);

        // Mapear a DTO
        return MapToDto(project);
    }
}
```

### DTO (Data Transfer Object) Pattern

Separa las entidades de dominio de las representaciones API:

```csharp
// Entity (Domain)
public class Project
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }
    // Propiedades de navegación
    public ICollection<Phase> Phases { get; set; }
}

// DTO (API)
public class ProjectDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string CreatedAt { get; set; } // Formateado como string
    // Sin propiedades de navegación
}
```

### CQRS Lite (Command Query Responsibility Segregation)

Separación de operaciones de lectura y escritura:

```csharp
// Command
public class CreateProjectCommand : IRequest<ProjectDto>
{
    public string Name { get; set; }
    public string Identifier { get; set; }
}

// Command Handler
public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, ProjectDto>
{
    public async Task<ProjectDto> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        // Lógica de creación
    }
}

// Query
public class GetProjectByIdQuery : IRequest<ProjectDto>
{
    public Guid Id { get; set; }
}

// Query Handler
public class GetProjectByIdQueryHandler : IRequestHandler<GetProjectByIdQuery, ProjectDto>
{
    public async Task<ProjectDto> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
    {
        // Lógica de consulta
    }
}
```

---

## Diagramas del Sistema

### Diagrama de Componentes

```
┌─────────────────────────────────────────────────────────────┐
│                        Cliente (Navegador)                   │
│  ┌─────────────────────────────────────────────────────┐    │
│  │              React Application (SPA)                 │    │
│  │  ┌──────────────┐  ┌──────────────┐  ┌───────────┐ │    │
│  │  │   Pages      │  │  Components  │  │  Services │ │    │
│  │  └──────────────┘  └──────────────┘  └───────────┘ │    │
│  │  ┌──────────────┐  ┌──────────────┐  ┌───────────┐ │    │
│  │  │    Hooks     │  │   Contexts   │  │   Utils   │ │    │
│  │  └──────────────┘  └──────────────┘  └───────────┘ │    │
│  └─────────────────────────────────────────────────────┘    │
└────────────────────────┬────────────────────────────────────┘
                         │ HTTPS/REST
                         ▼
┌─────────────────────────────────────────────────────────────┐
│                     API Server (.NET)                        │
│  ┌─────────────────────────────────────────────────────┐    │
│  │                   Controllers                        │    │
│  └──────────────────────┬──────────────────────────────┘    │
│  ┌──────────────────────┴──────────────────────────────┐    │
│  │              Middleware & Filters                    │    │
│  │  (Auth, Logging, Exception Handling, Validation)    │    │
│  └──────────────────────┬──────────────────────────────┘    │
│  ┌──────────────────────┴──────────────────────────────┐    │
│  │                  Services Layer                      │    │
│  │  (Business Logic, Validation, Orchestration)        │    │
│  └──────────────────────┬──────────────────────────────┘    │
│  ┌──────────────────────┴──────────────────────────────┐    │
│  │              Repositories Layer                      │    │
│  │  (Data Access, EF Core, LINQ)                       │    │
│  └──────────────────────┬──────────────────────────────┘    │
└─────────────────────────┼────────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────────┐
│                   PostgreSQL Database                        │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐         │
│  │   Tables    │  │   Indexes   │  │  Relations  │         │
│  └─────────────┘  └─────────────┘  └─────────────┘         │
└─────────────────────────────────────────────────────────────┘
```

### Diagrama de Secuencia: Crear Proyecto

```
Usuario → Frontend → API → Service → Repository → DB

1. Usuario: Click "Crear Proyecto"
2. Frontend: Muestra formulario
3. Usuario: Completa formulario y envía
4. Frontend → API: POST /api/projects
                    { name, identifier, ... }
5. API → Middleware: Valida JWT
6. Middleware → Controller: Autoriza (Manager/Admin)
7. Controller → Validator: Valida DTO
8. Controller → ProjectService: CreateProjectAsync(dto, userId)
9. ProjectService → Repository: AddAsync(project)
10. Repository → DB: INSERT INTO projects ...
11. DB → Repository: Proyecto creado (con Id)
12. Repository → ProjectService: Retorna entidad
13. ProjectService → AuditLog: Log "ProjectCreated"
14. ProjectService → Controller: Retorna ProjectDto
15. Controller → Frontend: 201 Created + ProjectDto
16. Frontend: Actualiza lista de proyectos
17. Frontend → Usuario: Muestra mensaje de éxito
```

### Diagrama de Flujo: Validación de Fase

```
┌─────────────────────────┐
│  Usuario solicita       │
│  validar fase           │
└───────────┬─────────────┘
            ▼
┌─────────────────────────┐
│  API recibe petición    │
│  GET /api/projects/{id} │
│  /artifacts/validate-   │
│  phase?phase=X          │
└───────────┬─────────────┘
            ▼
┌─────────────────────────┐
│  Obtener artefactos     │
│  obligatorios de fase   │
└───────────┬─────────────┘
            ▼
┌─────────────────────────┐
│  Para cada artefacto    │
│  obligatorio:           │
└───────────┬─────────────┘
            ▼
      ┌─────────┐
      │¿Existe? │
      └────┬────┘
           │
    ┌──────┴──────┐
    │NO           │SÍ
    ▼             ▼
┌────────┐   ┌────────────┐
│Agregar │   │¿Tiene al   │
│a lista │   │menos 1     │
│faltante│   │versión?    │
└───┬────┘   └─────┬──────┘
    │              │
    │         ┌────┴────┐
    │         │NO     │SÍ
    │         ▼         ▼
    │     ┌────────┐ ┌────────┐
    │     │Agregar │ │Validar │
    │     │a lista │ │estado  │
    │     │faltante│ └───┬────┘
    │     └───┬────┘     │
    │         │      ┌───┴────┐
    │         │      │Aprobado│
    │         │      └───┬────┘
    └─────────┴──────────┤
                         ▼
              ┌──────────────────┐
              │ Retornar resultado│
              │ - isValid         │
              │ - missingArtifacts│
              │ - recommendations │
              └──────────────────┘
```

---

## Conclusión

Este documento proporciona una visión completa del diseño arquitectónico de OpenUpTool, cubriendo:

- ✅ Arquitectura en capas con separación de responsabilidades
- ✅ Diseño de base de datos normalizado y optimizado
- ✅ API REST bien estructurada con autenticación JWT
- ✅ Frontend modular con React y TypeScript
- ✅ Patrones de diseño aplicados (Repository, Service, DTO, CQRS)
- ✅ Consideraciones de seguridad y escalabilidad
- ✅ Diagramas y flujos del sistema

Este diseño permite:

- **Mantenibilidad**: Código limpio y organizado
- **Escalabilidad**: Fácil de escalar horizontal y verticalmente
- **Testabilidad**: Capas desacopladas facilitan pruebas unitarias
- **Extensibilidad**: Nuevas funcionalidades se agregan sin afectar el código existente
- **Flexibilidad**: Configuración personalizable según necesidades

---

**OpenUpTool v1.0**  
© 2024 - Diseño de Sistema
