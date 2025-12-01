using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenUpTool.Core.Entities;
using OpenUpTool.Infrastructure.Data;

namespace OpenUpTool.Api.Controllers;

/// <summary>
/// Controlador para gestión de base de datos (solo desarrollo)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous] // Permitir acceso sin autenticación para facilitar desarrollo
public class DatabaseManagementController : ControllerBase
{
    private readonly OpenUpToolDbContext _context;
    private readonly ILogger<DatabaseManagementController> _logger;

    public DatabaseManagementController(OpenUpToolDbContext context, ILogger<DatabaseManagementController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Recrea todas las tablas de la base de datos (ADVERTENCIA: Elimina todos los datos)
    /// </summary>
    [HttpPost("recreate-database")]
    public async Task<IActionResult> RecreateDatabase()
    {
        try
        {
            _logger.LogWarning("🗑️ Eliminando base de datos...");
            await _context.Database.EnsureDeletedAsync();
            
            _logger.LogInformation("🔨 Creando base de datos...");
            await _context.Database.EnsureCreatedAsync();
            
            _logger.LogInformation("✅ Base de datos recreada exitosamente");
            
            return Ok(new 
            { 
                message = "Base de datos recreada exitosamente. Todas las tablas han sido creadas.",
                tablas = new[]
                {
                    "Roles",
                    "Users",
                    "Projects",
                    "Phases",
                    "ProjectPlans",
                    "Milestones",
                    "Iterations",
                    "ArtifactTypes",
                    "Artifacts",
                    "ArtifactVersions",
                    "UserStories",
                    "IterationScope",
                    "ProjectUserRoles",
                    "ProjectInvitations",
                    "Notifications"
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al recrear base de datos");
            return StatusCode(500, new { message = "Error al recrear base de datos", error = ex.Message });
        }
    }

    /// <summary>
    /// Siembra datos de prueba en la base de datos
    /// </summary>
    [HttpPost("seed-data")]
    public async Task<IActionResult> SeedData()
    {
        try
        {
            _logger.LogInformation("🌱 Iniciando siembra de datos de prueba...");

            // Verificar si ya existen datos
            if (await _context.Users.AnyAsync())
            {
                return BadRequest(new { message = "La base de datos ya contiene datos. Use recreate-database primero si desea limpiarla." });
            }

            // 1. Crear Roles
            var roles = new[]
            {
                new Role 
                { 
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Name = "Admin", 
                    Description = "Administrador del sistema con acceso total",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Role 
                { 
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Name = "Manager", 
                    Description = "Gerente de proyecto con permisos de gestión",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Role 
                { 
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    Name = "Developer", 
                    Description = "Desarrollador con permisos limitados",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Role 
                { 
                    Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                    Name = "Tester", 
                    Description = "Tester con acceso a pruebas y reportes",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };
            await _context.Roles.AddRangeAsync(roles);
            await _context.SaveChangesAsync();
            _logger.LogInformation("✅ Roles creados");

            // 2. Crear Usuarios (password: Password123! para todos)
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword("Password123!");
            var users = new[]
            {
                // Admin
                new User
                {
                    Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                    Email = "admin@openuptool.com",
                    PasswordHash = hashedPassword,
                    FirstName = "Admin",
                    LastName = "Sistema",
                    RoleId = roles[0].Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                // Managers
                new User
                {
                    Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                    Email = "maria.gonzalez@openuptool.com",
                    PasswordHash = hashedPassword,
                    FirstName = "María",
                    LastName = "González",
                    RoleId = roles[1].Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Email = "roberto.sanchez@openuptool.com",
                    PasswordHash = hashedPassword,
                    FirstName = "Roberto",
                    LastName = "Sánchez",
                    RoleId = roles[1].Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                // Developers
                new User
                {
                    Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                    Email = "carlos.ramirez@openuptool.com",
                    PasswordHash = hashedPassword,
                    FirstName = "Carlos",
                    LastName = "Ramírez",
                    RoleId = roles[2].Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Email = "ana.martinez@openuptool.com",
                    PasswordHash = hashedPassword,
                    FirstName = "Ana",
                    LastName = "Martínez",
                    RoleId = roles[2].Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Email = "luis.torres@openuptool.com",
                    PasswordHash = hashedPassword,
                    FirstName = "Luis",
                    LastName = "Torres",
                    RoleId = roles[2].Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Email = "laura.fernandez@openuptool.com",
                    PasswordHash = hashedPassword,
                    FirstName = "Laura",
                    LastName = "Fernández",
                    RoleId = roles[2].Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Email = "jorge.hernandez@openuptool.com",
                    PasswordHash = hashedPassword,
                    FirstName = "Jorge",
                    LastName = "Hernández",
                    RoleId = roles[2].Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                // Testers
                new User
                {
                    Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                    Email = "patricia.lopez@openuptool.com",
                    PasswordHash = hashedPassword,
                    FirstName = "Patricia",
                    LastName = "López",
                    RoleId = roles[3].Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Email = "viewer@openuptool.com",
                    PasswordHash = hashedPassword,
                    FirstName = "Usuario",
                    LastName = "Viewer",
                    RoleId = roles[3].Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };
            await _context.Users.AddRangeAsync(users);
            await _context.SaveChangesAsync();
            _logger.LogInformation("✅ Usuarios creados (10 usuarios)");

            // 3. Crear Proyectos
            var now = DateTime.UtcNow;
            var projects = new[]
            {
                new Project
                {
                    Id = Guid.Parse("11111111-2222-3333-4444-555555555555"),
                    Name = "Sistema de Gestión Empresarial",
                    Identifier = "SGE",
                    StartDate = now.AddMonths(-2),
                    Status = "En Progreso",
                    Owner = "Carlos Gerente",
                    Description = "Sistema integral para gestión de recursos empresariales",
                    Tags = new List<string> { "ERP", "Enterprise", "Gestión" },
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Project
                {
                    Id = Guid.Parse("22222222-3333-4444-5555-666666666666"),
                    Name = "Aplicación Mobile Banking",
                    Identifier = "AMB",
                    StartDate = now.AddMonths(-1),
                    Status = "Creado",
                    Owner = "Carlos Gerente",
                    Description = "Aplicación móvil para banca en línea",
                    Tags = new List<string> { "Mobile", "Banking", "Fintech" },
                    CreatedAt = now,
                    UpdatedAt = now
                }
            };
            await _context.Projects.AddRangeAsync(projects);
            await _context.SaveChangesAsync();
            _logger.LogInformation("✅ Proyectos creados");

            // 4. Crear Fases para cada proyecto
            var phases = new List<Phase>();
            foreach (var project in projects)
            {
                phases.AddRange(new[]
                {
                    new Phase 
                    { 
                        Id = Guid.NewGuid(), 
                        ProjectId = project.Id, 
                        PhaseCode = "INCEPTION", 
                        Name = "Incepción", 
                        Status = project.Id == projects[0].Id ? "COMPLETED" : "PENDING",
                        StartDate = project.Id == projects[0].Id ? project.StartDate : null,
                        EndDate = project.Id == projects[0].Id ? project.StartDate.AddDays(14) : null,
                        OrderIndex = 1, 
                        CreatedAt = now, 
                        UpdatedAt = now 
                    },
                    new Phase 
                    { 
                        Id = Guid.NewGuid(), 
                        ProjectId = project.Id, 
                        PhaseCode = "ELABORATION", 
                        Name = "Elaboración", 
                        Status = project.Id == projects[0].Id ? "IN_PROGRESS" : "PENDING",
                        StartDate = project.Id == projects[0].Id ? project.StartDate.AddDays(15) : null,
                        OrderIndex = 2, 
                        CreatedAt = now, 
                        UpdatedAt = now 
                    },
                    new Phase 
                    { 
                        Id = Guid.NewGuid(), 
                        ProjectId = project.Id, 
                        PhaseCode = "CONSTRUCTION", 
                        Name = "Construcción", 
                        Status = "PENDING", 
                        OrderIndex = 3, 
                        CreatedAt = now, 
                        UpdatedAt = now 
                    },
                    new Phase 
                    { 
                        Id = Guid.NewGuid(), 
                        ProjectId = project.Id, 
                        PhaseCode = "TRANSITION", 
                        Name = "Transición", 
                        Status = "PENDING", 
                        OrderIndex = 4, 
                        CreatedAt = now, 
                        UpdatedAt = now 
                    }
                });
            }
            await _context.Phases.AddRangeAsync(phases);
            await _context.SaveChangesAsync();
            _logger.LogInformation("✅ Fases creadas");

            // 5. Asignar usuarios a proyectos
            var projectUserRoles = new List<ProjectUserRole>
            {
                // Proyecto 1 - SGE - Equipo completo
                new ProjectUserRole
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projects[0].Id,
                    UserId = users[0].Id, // Admin
                    RoleId = roles[0].Id,
                    InvitedBy = users[0].Id,
                    InvitedAt = now,
                    AcceptedAt = now,
                    Status = "active",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new ProjectUserRole
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projects[0].Id,
                    UserId = users[1].Id, // María González (Manager)
                    RoleId = roles[1].Id,
                    InvitedBy = users[0].Id,
                    InvitedAt = now,
                    AcceptedAt = now,
                    Status = "active",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new ProjectUserRole
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projects[0].Id,
                    UserId = users[3].Id, // Carlos Ramírez (Developer)
                    RoleId = roles[2].Id,
                    InvitedBy = users[0].Id,
                    InvitedAt = now,
                    AcceptedAt = now,
                    Status = "active",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new ProjectUserRole
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projects[0].Id,
                    UserId = users[4].Id, // Ana Martínez (Developer)
                    RoleId = roles[2].Id,
                    InvitedBy = users[0].Id,
                    InvitedAt = now,
                    AcceptedAt = now,
                    Status = "active",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new ProjectUserRole
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projects[0].Id,
                    UserId = users[5].Id, // Luis Torres (Developer)
                    RoleId = roles[2].Id,
                    InvitedBy = users[0].Id,
                    InvitedAt = now,
                    AcceptedAt = now,
                    Status = "active",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new ProjectUserRole
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projects[0].Id,
                    UserId = users[8].Id, // Patricia López (Tester)
                    RoleId = roles[3].Id,
                    InvitedBy = users[0].Id,
                    InvitedAt = now,
                    AcceptedAt = now,
                    Status = "active",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                // Proyecto 2 - AMB - Equipo reducido
                new ProjectUserRole
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projects[1].Id,
                    UserId = users[0].Id, // Admin
                    RoleId = roles[0].Id,
                    InvitedBy = users[0].Id,
                    InvitedAt = now,
                    AcceptedAt = now,
                    Status = "active",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new ProjectUserRole
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projects[1].Id,
                    UserId = users[2].Id, // Roberto Sánchez (Manager)
                    RoleId = roles[1].Id,
                    InvitedBy = users[0].Id,
                    InvitedAt = now,
                    AcceptedAt = now,
                    Status = "active",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new ProjectUserRole
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projects[1].Id,
                    UserId = users[6].Id, // Laura Fernández (Developer)
                    RoleId = roles[2].Id,
                    InvitedBy = users[0].Id,
                    InvitedAt = now,
                    AcceptedAt = now,
                    Status = "active",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new ProjectUserRole
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projects[1].Id,
                    UserId = users[7].Id, // Jorge Hernández (Developer)
                    RoleId = roles[2].Id,
                    InvitedBy = users[0].Id,
                    InvitedAt = now,
                    AcceptedAt = now,
                    Status = "active",
                    CreatedAt = now,
                    UpdatedAt = now
                }
            };
            await _context.ProjectUserRoles.AddRangeAsync(projectUserRoles);
            await _context.SaveChangesAsync();
            _logger.LogInformation("✅ Usuarios asignados a proyectos (11 asignaciones)");

            // 6. Crear tipos de artefactos (OpenUP completo)
            var artifactTypes = new[]
            {
                // INCEPTION - Fase de Inicio
                new ArtifactType { Id = Guid.NewGuid(), Name = "Vision Document", Phase = "INCEPTION", Code = "VIS", Description = "Documento de visión del proyecto que establece el alcance y objetivos", IsMandatory = true, DefaultFormat = "TEXT", CreatedAt = now, UpdatedAt = now },
                new ArtifactType { Id = Guid.NewGuid(), Name = "Project Plan", Phase = "INCEPTION", Code = "PLAN", Description = "Plan general del proyecto con cronograma y recursos", IsMandatory = true, DefaultFormat = "TEXT", CreatedAt = now, UpdatedAt = now },
                new ArtifactType { Id = Guid.NewGuid(), Name = "Risk List", Phase = "INCEPTION", Code = "RISK", Description = "Lista de riesgos identificados y estrategias de mitigación", IsMandatory = true, DefaultFormat = "TEXT", CreatedAt = now, UpdatedAt = now },
                new ArtifactType { Id = Guid.NewGuid(), Name = "Glossary", Phase = "INCEPTION", Code = "GLOS", Description = "Glosario de términos del proyecto", IsMandatory = false, DefaultFormat = "TEXT", CreatedAt = now, UpdatedAt = now },
                new ArtifactType { Id = Guid.NewGuid(), Name = "Stakeholder Requests", Phase = "INCEPTION", Code = "STAKE", Description = "Solicitudes y necesidades de los stakeholders", IsMandatory = true, DefaultFormat = "TEXT", CreatedAt = now, UpdatedAt = now },
                
                // ELABORATION - Fase de Elaboración
                new ArtifactType { Id = Guid.NewGuid(), Name = "Use Case Model", Phase = "ELABORATION", Code = "UCM", Description = "Modelo de casos de uso del sistema", IsMandatory = true, DefaultFormat = "TEXT", CreatedAt = now, UpdatedAt = now },
                new ArtifactType { Id = Guid.NewGuid(), Name = "Architecture Notebook", Phase = "ELABORATION", Code = "ARCH", Description = "Documento de arquitectura del sistema", IsMandatory = true, DefaultFormat = "MIXED", CreatedAt = now, UpdatedAt = now },
                new ArtifactType { Id = Guid.NewGuid(), Name = "System Requirements", Phase = "ELABORATION", Code = "SYSREQ", Description = "Requisitos funcionales y no funcionales del sistema", IsMandatory = true, DefaultFormat = "TEXT", CreatedAt = now, UpdatedAt = now },
                new ArtifactType { Id = Guid.NewGuid(), Name = "Domain Model", Phase = "ELABORATION", Code = "DOM", Description = "Modelo conceptual del dominio del negocio", IsMandatory = false, DefaultFormat = "FILE", CreatedAt = now, UpdatedAt = now },
                new ArtifactType { Id = Guid.NewGuid(), Name = "Use Case Specification", Phase = "ELABORATION", Code = "UCSPEC", Description = "Especificación detallada de casos de uso", IsMandatory = true, DefaultFormat = "TEXT", CreatedAt = now, UpdatedAt = now },
                new ArtifactType { Id = Guid.NewGuid(), Name = "Supplementary Specification", Phase = "ELABORATION", Code = "SUPPL", Description = "Especificaciones suplementarias y requisitos no funcionales", IsMandatory = true, DefaultFormat = "TEXT", CreatedAt = now, UpdatedAt = now },
                new ArtifactType { Id = Guid.NewGuid(), Name = "User Interface Prototype", Phase = "ELABORATION", Code = "UIPROT", Description = "Prototipo de interfaz de usuario", IsMandatory = false, DefaultFormat = "FILE", CreatedAt = now, UpdatedAt = now },
                new ArtifactType { Id = Guid.NewGuid(), Name = "Data Model", Phase = "ELABORATION", Code = "DATAM", Description = "Modelo de datos del sistema", IsMandatory = true, DefaultFormat = "FILE", CreatedAt = now, UpdatedAt = now },
                
                // CONSTRUCTION - Fase de Construcción
                new ArtifactType { Id = Guid.NewGuid(), Name = "Design Model", Phase = "CONSTRUCTION", Code = "DES", Description = "Modelo de diseño detallado del sistema", IsMandatory = true, DefaultFormat = "FILE", CreatedAt = now, UpdatedAt = now },
                new ArtifactType { Id = Guid.NewGuid(), Name = "Implementation", Phase = "CONSTRUCTION", Code = "IMP", Description = "Código fuente implementado", IsMandatory = true, DefaultFormat = "FILE", CreatedAt = now, UpdatedAt = now },
                new ArtifactType { Id = Guid.NewGuid(), Name = "Test Case", Phase = "CONSTRUCTION", Code = "TST", Description = "Casos de prueba del sistema", IsMandatory = true, DefaultFormat = "TEXT", CreatedAt = now, UpdatedAt = now },
                new ArtifactType { Id = Guid.NewGuid(), Name = "Test Scripts", Phase = "CONSTRUCTION", Code = "TSTSCR", Description = "Scripts automatizados de pruebas", IsMandatory = false, DefaultFormat = "FILE", CreatedAt = now, UpdatedAt = now },
                new ArtifactType { Id = Guid.NewGuid(), Name = "Build", Phase = "CONSTRUCTION", Code = "BUILD", Description = "Build ejecutable del sistema", IsMandatory = true, DefaultFormat = "FILE", CreatedAt = now, UpdatedAt = now },
                new ArtifactType { Id = Guid.NewGuid(), Name = "Developer Guide", Phase = "CONSTRUCTION", Code = "DEVG", Description = "Guía para desarrolladores del proyecto", IsMandatory = false, DefaultFormat = "TEXT", CreatedAt = now, UpdatedAt = now },
                new ArtifactType { Id = Guid.NewGuid(), Name = "API Documentation", Phase = "CONSTRUCTION", Code = "APIDOC", Description = "Documentación de APIs del sistema", IsMandatory = true, DefaultFormat = "MIXED", CreatedAt = now, UpdatedAt = now },
                new ArtifactType { Id = Guid.NewGuid(), Name = "Unit Tests", Phase = "CONSTRUCTION", Code = "UNIT", Description = "Pruebas unitarias del código", IsMandatory = true, DefaultFormat = "FILE", CreatedAt = now, UpdatedAt = now },
                new ArtifactType { Id = Guid.NewGuid(), Name = "Integration Tests", Phase = "CONSTRUCTION", Code = "INTEG", Description = "Pruebas de integración", IsMandatory = true, DefaultFormat = "FILE", CreatedAt = now, UpdatedAt = now },
                
                // TRANSITION - Fase de Transición
                new ArtifactType { Id = Guid.NewGuid(), Name = "User Manual", Phase = "TRANSITION", Code = "UMAN", Description = "Manual de usuario del sistema", IsMandatory = true, DefaultFormat = "TEXT", CreatedAt = now, UpdatedAt = now },
                new ArtifactType { Id = Guid.NewGuid(), Name = "Deployment Plan", Phase = "TRANSITION", Code = "DEPLOY", Description = "Plan de despliegue del sistema", IsMandatory = true, DefaultFormat = "TEXT", CreatedAt = now, UpdatedAt = now },
                new ArtifactType { Id = Guid.NewGuid(), Name = "Training Materials", Phase = "TRANSITION", Code = "TRAIN", Description = "Material de capacitación para usuarios", IsMandatory = false, DefaultFormat = "MIXED", CreatedAt = now, UpdatedAt = now },
                new ArtifactType { Id = Guid.NewGuid(), Name = "Release Notes", Phase = "TRANSITION", Code = "RELNOTE", Description = "Notas de la versión liberada", IsMandatory = true, DefaultFormat = "TEXT", CreatedAt = now, UpdatedAt = now },
                new ArtifactType { Id = Guid.NewGuid(), Name = "Installation Guide", Phase = "TRANSITION", Code = "INSTG", Description = "Guía de instalación del sistema", IsMandatory = true, DefaultFormat = "TEXT", CreatedAt = now, UpdatedAt = now },
                new ArtifactType { Id = Guid.NewGuid(), Name = "System Administration Guide", Phase = "TRANSITION", Code = "SYSADM", Description = "Guía de administración del sistema", IsMandatory = true, DefaultFormat = "TEXT", CreatedAt = now, UpdatedAt = now },
                new ArtifactType { Id = Guid.NewGuid(), Name = "Backup and Recovery Plan", Phase = "TRANSITION", Code = "BACKUP", Description = "Plan de respaldo y recuperación", IsMandatory = true, DefaultFormat = "TEXT", CreatedAt = now, UpdatedAt = now },
                new ArtifactType { Id = Guid.NewGuid(), Name = "Product Acceptance", Phase = "TRANSITION", Code = "ACCEPT", Description = "Documento de aceptación del producto", IsMandatory = true, DefaultFormat = "TEXT", CreatedAt = now, UpdatedAt = now }
            };
            await _context.ArtifactTypes.AddRangeAsync(artifactTypes);
            await _context.SaveChangesAsync();
            _logger.LogInformation("✅ Tipos de artefactos creados (30 tipos)");

            // 7. Crear iteraciones para el primer proyecto
            var elaborationPhase = phases.FirstOrDefault(p => p.ProjectId == projects[0].Id && p.PhaseCode == "ELABORATION");
            if (elaborationPhase != null)
            {
                var iterations = new[]
                {
                    new Iteration
                    {
                        Id = Guid.NewGuid(),
                        ProjectId = projects[0].Id,
                        Phase = "ELABORATION",
                        Name = "Iteración 1 - Elaboración",
                        Objective = "Definir arquitectura base y casos de uso principales",
                        Status = "En curso",
                        StartDate = now.AddDays(-7),
                        EndDate = now.AddDays(7),
                        CreatedAt = now,
                        UpdatedAt = now
                    },
                    new Iteration
                    {
                        Id = Guid.NewGuid(),
                        ProjectId = projects[0].Id,
                        Phase = "ELABORATION",
                        Name = "Iteración 2 - Elaboración",
                        Objective = "Completar diseño detallado y prototipos",
                        Status = "Planeada",
                        StartDate = now.AddDays(8),
                        EndDate = now.AddDays(21),
                        CreatedAt = now,
                        UpdatedAt = now
                    }
                };
                await _context.Iterations.AddRangeAsync(iterations);
                await _context.SaveChangesAsync();
                _logger.LogInformation("✅ Iteraciones creadas");

                // 8. Crear User Stories
                var userStories = new[]
                {
                    new UserStory
                    {
                        Id = Guid.NewGuid(),
                        ProjectId = projects[0].Id,
                        Title = "Login de usuarios",
                        Description = "Como usuario, quiero poder iniciar sesión en el sistema",
                        AcceptanceCriteria = "- El usuario puede ingresar email y contraseña\n- Se validan las credenciales\n- Se muestra error si son incorrectas",
                        Priority = "high",
                        Status = "in_progress",
                        StoryPoints = 5,
                        CreatedBy = users[1].Id,
                        CreatedAt = now,
                        UpdatedAt = now
                    },
                    new UserStory
                    {
                        Id = Guid.NewGuid(),
                        ProjectId = projects[0].Id,
                        Title = "Gestión de usuarios",
                        Description = "Como administrador, quiero gestionar los usuarios del sistema",
                        AcceptanceCriteria = "- Crear nuevos usuarios\n- Editar usuarios existentes\n- Desactivar usuarios",
                        Priority = "medium",
                        Status = "ready",
                        StoryPoints = 8,
                        CreatedBy = users[1].Id,
                        CreatedAt = now,
                        UpdatedAt = now
                    },
                    new UserStory
                    {
                        Id = Guid.NewGuid(),
                        ProjectId = projects[0].Id,
                        Title = "Dashboard principal",
                        Description = "Como usuario, quiero ver un dashboard con información relevante",
                        AcceptanceCriteria = "- Mostrar métricas principales\n- Gráficos interactivos\n- Filtros por fecha",
                        Priority = "medium",
                        Status = "backlog",
                        StoryPoints = 13,
                        CreatedBy = users[1].Id,
                        CreatedAt = now,
                        UpdatedAt = now
                    }
                };
                await _context.UserStories.AddRangeAsync(userStories);
                await _context.SaveChangesAsync();
                _logger.LogInformation("✅ User Stories creadas");

                // 9. Agregar user stories a la iteración actual
                var iterationScopes = new[]
                {
                    new IterationScope
                    {
                        Id = Guid.NewGuid(),
                        IterationId = iterations[0].Id,
                        ItemType = "story",
                        ItemId = userStories[0].Id,
                        Status = "in_progress",
                        AssignedTo = users[2].Id,
                        EstimatedHours = 20,
                        CreatedAt = now,
                        UpdatedAt = now
                    },
                    new IterationScope
                    {
                        Id = Guid.NewGuid(),
                        IterationId = iterations[0].Id,
                        ItemType = "story",
                        ItemId = userStories[1].Id,
                        Status = "pending",
                        AssignedTo = users[2].Id,
                        EstimatedHours = 32,
                        CreatedAt = now,
                        UpdatedAt = now
                    }
                };
                await _context.IterationScopes.AddRangeAsync(iterationScopes);
                await _context.SaveChangesAsync();
                _logger.LogInformation("✅ Alcance de iteración definido");
            }

            // 10. Crear planes de proyecto
            var projectPlans = new[]
            {
                new ProjectPlan
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projects[0].Id,
                    Objectives = "Desarrollar un sistema integral de gestión empresarial que permita administrar recursos, proyectos y personal de manera eficiente",
                    Scope = "El sistema incluirá módulos de gestión de proyectos, recursos humanos, finanzas y reportería",
                    InitialSchedule = new List<PhaseScheduleItem>
                    {
                        new PhaseScheduleItem { PhaseName = "Incepción", StartDate = now.AddMonths(-2), EndDate = now.AddMonths(-2).AddDays(14), Responsible = "Carlos Gerente" },
                        new PhaseScheduleItem { PhaseName = "Elaboración", StartDate = now.AddMonths(-2).AddDays(15), EndDate = now.AddDays(21), Responsible = "Ana Desarrolladora" },
                        new PhaseScheduleItem { PhaseName = "Construcción", StartDate = now.AddDays(22), EndDate = now.AddMonths(2), Responsible = "Ana Desarrolladora" },
                        new PhaseScheduleItem { PhaseName = "Transición", StartDate = now.AddMonths(2).AddDays(1), EndDate = now.AddMonths(3), Responsible = "Luis Tester" }
                    },
                    Version = 1,
                    IsActive = true,
                    Observations = "Plan inicial del proyecto, sujeto a ajustes según avance",
                    CreatedAt = now.AddMonths(-2),
                    UpdatedAt = now
                },
                new ProjectPlan
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projects[1].Id,
                    Objectives = "Desarrollar una aplicación móvil de banca que permita a los usuarios realizar operaciones bancarias desde sus dispositivos móviles",
                    Scope = "Aplicación para iOS y Android con funcionalidades de consulta de saldo, transferencias, pago de servicios y soporte",
                    InitialSchedule = new List<PhaseScheduleItem>
                    {
                        new PhaseScheduleItem { PhaseName = "Incepción", StartDate = now.AddMonths(-1), EndDate = now.AddMonths(-1).AddDays(10), Responsible = "Carlos Gerente" },
                        new PhaseScheduleItem { PhaseName = "Elaboración", StartDate = now.AddMonths(-1).AddDays(11), EndDate = now.AddDays(14), Responsible = "Ana Desarrolladora" },
                        new PhaseScheduleItem { PhaseName = "Construcción", StartDate = now.AddDays(15), EndDate = now.AddMonths(2), Responsible = "Ana Desarrolladora" },
                        new PhaseScheduleItem { PhaseName = "Transición", StartDate = now.AddMonths(2).AddDays(1), EndDate = now.AddMonths(3), Responsible = "Luis Tester" }
                    },
                    Version = 1,
                    IsActive = true,
                    Observations = "Enfoque en experiencia de usuario y seguridad",
                    CreatedAt = now.AddMonths(-1),
                    UpdatedAt = now
                }
            };
            await _context.ProjectPlans.AddRangeAsync(projectPlans);
            await _context.SaveChangesAsync();
            _logger.LogInformation("✅ Planes de proyecto creados");

            // 11. Actualizar proyectos con sus planes
            projects[0].PlanId = projectPlans[0].Id;
            projects[1].PlanId = projectPlans[1].Id;
            _context.Projects.UpdateRange(projects);
            await _context.SaveChangesAsync();
            _logger.LogInformation("✅ Proyectos vinculados a sus planes");

            // 12. Crear milestones (hitos) para los planes
            var milestones = new[]
            {
                // Milestones para SGE
                new Milestone
                {
                    Id = Guid.NewGuid(),
                    PlanId = projectPlans[0].Id,
                    Name = "Aprobación de Visión",
                    Date = now.AddMonths(-2).AddDays(7),
                    Description = "Aprobación del documento de visión por stakeholders",
                    CreatedAt = now.AddMonths(-2),
                    UpdatedAt = now.AddMonths(-2)
                },
                new Milestone
                {
                    Id = Guid.NewGuid(),
                    PlanId = projectPlans[0].Id,
                    Name = "Arquitectura Definida",
                    Date = now.AddDays(14),
                    Description = "Arquitectura del sistema completamente definida y aprobada",
                    CreatedAt = now.AddMonths(-2),
                    UpdatedAt = now
                },
                new Milestone
                {
                    Id = Guid.NewGuid(),
                    PlanId = projectPlans[0].Id,
                    Name = "Prototipo Funcional",
                    Date = now.AddMonths(1),
                    Description = "Primer prototipo funcional del sistema",
                    CreatedAt = now.AddMonths(-2),
                    UpdatedAt = now
                },
                // Milestones para AMB
                new Milestone
                {
                    Id = Guid.NewGuid(),
                    PlanId = projectPlans[1].Id,
                    Name = "Definición de Requisitos",
                    Date = now.AddMonths(-1).AddDays(5),
                    Description = "Requisitos funcionales y no funcionales definidos",
                    CreatedAt = now.AddMonths(-1),
                    UpdatedAt = now.AddMonths(-1)
                },
                new Milestone
                {
                    Id = Guid.NewGuid(),
                    PlanId = projectPlans[1].Id,
                    Name = "Diseño UX Aprobado",
                    Date = now.AddDays(7),
                    Description = "Diseño de experiencia de usuario aprobado",
                    CreatedAt = now.AddMonths(-1),
                    UpdatedAt = now
                }
            };
            await _context.Milestones.AddRangeAsync(milestones);
            await _context.SaveChangesAsync();
            _logger.LogInformation("✅ Milestones creados");

            // 13. Crear artefactos para los proyectos
            var artifacts = new[]
            {
                // Artefactos para SGE - Fase Incepción (completada)
                new Artifact
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projects[0].Id,
                    PhaseId = "INCEPTION",
                    ArtifactTypeId = artifactTypes[0].Id, // Vision Document
                    Title = "Documento de Visión - SGE",
                    Description = "Visión general del Sistema de Gestión Empresarial",
                    Author = users[1].Email,
                    Status = "Aprobado",
                    IsMandatory = true,
                    ContentText = "# Visión del Sistema\n\nEl SGE será un sistema integral que permitirá...\n\n## Objetivos\n- Centralizar información\n- Mejorar eficiencia\n- Facilitar toma de decisiones",
                    CreatedAt = now.AddMonths(-2).AddDays(3),
                    UpdatedAt = now.AddMonths(-2).AddDays(10)
                },
                // Artefactos para SGE - Fase Elaboración (en progreso)
                new Artifact
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projects[0].Id,
                    PhaseId = "ELABORATION",
                    ArtifactTypeId = artifactTypes[1].Id, // Use Case Model
                    Title = "Modelo de Casos de Uso - SGE",
                    Description = "Casos de uso principales del sistema",
                    Author = users[2].Email,
                    Status = "En revisión",
                    IsMandatory = true,
                    ContentText = "# Casos de Uso\n\n## CU-01: Gestionar Proyectos\n**Actor:** Gerente\n**Descripción:** Permite crear y administrar proyectos...",
                    CreatedAt = now.AddDays(-5),
                    UpdatedAt = now.AddDays(-1)
                },
                new Artifact
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projects[0].Id,
                    PhaseId = "ELABORATION",
                    ArtifactTypeId = artifactTypes[2].Id, // Architecture Notebook
                    Title = "Documento de Arquitectura - SGE",
                    Description = "Arquitectura del sistema y decisiones de diseño",
                    Author = users[2].Email,
                    Status = "Pendiente",
                    IsMandatory = true,
                    ContentText = "# Arquitectura del Sistema\n\n## Estilo Arquitectónico\nArquitectura en capas:\n- Capa de Presentación\n- Capa de Negocio\n- Capa de Datos",
                    CreatedAt = now.AddDays(-3),
                    UpdatedAt = now.AddDays(-1)
                },
                // Artefactos para AMB - Fase Incepción
                new Artifact
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projects[1].Id,
                    PhaseId = "INCEPTION",
                    ArtifactTypeId = artifactTypes[0].Id, // Vision Document
                    Title = "Documento de Visión - AMB",
                    Description = "Visión de la aplicación mobile banking",
                    Author = users[1].Email,
                    Status = "En revisión",
                    IsMandatory = true,
                    ContentText = "# Visión Mobile Banking\n\n## Problema\nLos usuarios necesitan realizar operaciones bancarias de forma rápida y segura desde sus dispositivos móviles...",
                    CreatedAt = now.AddMonths(-1).AddDays(2),
                    UpdatedAt = now.AddDays(-2)
                }
            };
            await _context.Artifacts.AddRangeAsync(artifacts);
            await _context.SaveChangesAsync();
            _logger.LogInformation("✅ Artefactos creados");

            // 14. Crear versiones de artefactos
            var artifactVersions = new[]
            {
                // Versiones para Visión SGE
                new ArtifactVersion
                {
                    Id = Guid.NewGuid(),
                    ArtifactId = artifacts[0].Id,
                    VersionNumber = 1,
                    FileName = "vision_sge_v1.pdf",
                    FilePath = "/artifacts/sge/vision_sge_v1.pdf",
                    FileSize = 245678,
                    UploadedBy = users[1].Email,
                    UploadedAt = now.AddMonths(-2).AddDays(3),
                    ChangeDescription = "Versión inicial del documento de visión",
                    CreatedAt = now.AddMonths(-2).AddDays(3),
                    UpdatedAt = now.AddMonths(-2).AddDays(3)
                },
                new ArtifactVersion
                {
                    Id = Guid.NewGuid(),
                    ArtifactId = artifacts[0].Id,
                    VersionNumber = 2,
                    FileName = "vision_sge_v2.pdf",
                    FilePath = "/artifacts/sge/vision_sge_v2.pdf",
                    FileSize = 268934,
                    UploadedBy = users[1].Email,
                    UploadedAt = now.AddMonths(-2).AddDays(7),
                    ChangeDescription = "Incorporación de feedback de stakeholders",
                    CreatedAt = now.AddMonths(-2).AddDays(7),
                    UpdatedAt = now.AddMonths(-2).AddDays(7)
                },
                new ArtifactVersion
                {
                    Id = Guid.NewGuid(),
                    ArtifactId = artifacts[0].Id,
                    VersionNumber = 3,
                    FileName = "vision_sge_v3_final.pdf",
                    FilePath = "/artifacts/sge/vision_sge_v3_final.pdf",
                    FileSize = 275120,
                    UploadedBy = users[0].Email,
                    UploadedAt = now.AddMonths(-2).AddDays(10),
                    ChangeDescription = "Versión final aprobada",
                    CreatedAt = now.AddMonths(-2).AddDays(10),
                    UpdatedAt = now.AddMonths(-2).AddDays(10)
                },
                // Versiones para Casos de Uso SGE
                new ArtifactVersion
                {
                    Id = Guid.NewGuid(),
                    ArtifactId = artifacts[1].Id,
                    VersionNumber = 1,
                    FileName = "casos_uso_sge_v1.docx",
                    FilePath = "/artifacts/sge/casos_uso_sge_v1.docx",
                    FileSize = 89456,
                    UploadedBy = users[2].Email,
                    UploadedAt = now.AddDays(-5),
                    ChangeDescription = "Primera versión del modelo de casos de uso",
                    CreatedAt = now.AddDays(-5),
                    UpdatedAt = now.AddDays(-5)
                },
                new ArtifactVersion
                {
                    Id = Guid.NewGuid(),
                    ArtifactId = artifacts[1].Id,
                    VersionNumber = 2,
                    FileName = "casos_uso_sge_v2.docx",
                    FilePath = "/artifacts/sge/casos_uso_sge_v2.docx",
                    FileSize = 124789,
                    UploadedBy = users[2].Email,
                    UploadedAt = now.AddDays(-1),
                    ChangeDescription = "Agregados casos de uso de módulo de RRHH",
                    CreatedAt = now.AddDays(-1),
                    UpdatedAt = now.AddDays(-1)
                },
                // Versiones para Arquitectura SGE
                new ArtifactVersion
                {
                    Id = Guid.NewGuid(),
                    ArtifactId = artifacts[2].Id,
                    VersionNumber = 1,
                    FileName = "arquitectura_sge_draft.pdf",
                    FilePath = "/artifacts/sge/arquitectura_sge_draft.pdf",
                    FileSize = 456789,
                    UploadedBy = users[2].Email,
                    UploadedAt = now.AddDays(-3),
                    ChangeDescription = "Borrador inicial de arquitectura",
                    CreatedAt = now.AddDays(-3),
                    UpdatedAt = now.AddDays(-3)
                },
                // Versiones para Visión AMB
                new ArtifactVersion
                {
                    Id = Guid.NewGuid(),
                    ArtifactId = artifacts[3].Id,
                    VersionNumber = 1,
                    FileName = "vision_amb_v1.pdf",
                    FilePath = "/artifacts/amb/vision_amb_v1.pdf",
                    FileSize = 198765,
                    UploadedBy = users[1].Email,
                    UploadedAt = now.AddMonths(-1).AddDays(2),
                    ChangeDescription = "Versión inicial para revisión",
                    CreatedAt = now.AddMonths(-1).AddDays(2),
                    UpdatedAt = now.AddMonths(-1).AddDays(2)
                }
            };
            await _context.ArtifactVersions.AddRangeAsync(artifactVersions);
            await _context.SaveChangesAsync();
            _logger.LogInformation("✅ Versiones de artefactos creadas");

            // 15. Crear notificaciones de ejemplo
            var notifications = new[]
            {
                new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = users[2].Id,
                    Type = "ASSIGNMENT",
                    Title = "Nueva asignación",
                    Message = "Se te ha asignado la historia de usuario: Login de usuarios",
                    RelatedEntityType = "UserStory",
                    RelatedEntityId = Guid.NewGuid(),
                    IsRead = false,
                    CreatedAt = now,
                    ReadAt = null
                },
                new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = users[1].Id,
                    Type = "PROJECT_UPDATE",
                    Title = "Progreso del proyecto",
                    Message = "El proyecto SGE ha completado la fase de Incepción",
                    RelatedEntityType = "Project",
                    RelatedEntityId = projects[0].Id,
                    IsRead = true,
                    CreatedAt = now.AddDays(-5),
                    ReadAt = now.AddDays(-4)
                }
            };
            await _context.Notifications.AddRangeAsync(notifications);
            await _context.SaveChangesAsync();
            _logger.LogInformation("✅ Notificaciones creadas");

            _logger.LogInformation("🎉 Siembra de datos completada exitosamente");

            return Ok(new
            {
                message = "Datos de prueba sembrados exitosamente",
                resumen = new
                {
                    roles = roles.Length,
                    usuarios = 10,
                    proyectos = projects.Length,
                    fases = phases.Count,
                    planesProyecto = projectPlans.Length,
                    milestones = milestones.Length,
                    tiposArtefactos = 30,
                    artefactos = artifacts.Length,
                    versionesArtefactos = artifactVersions.Length,
                    iteraciones = 2,
                    userStories = 3,
                    alcanceIteracion = 2,
                    asignacionesProyecto = 11,
                    notificaciones = notifications.Length
                },
                credenciales = new
                {
                    password = "Password123!",
                    totalUsuarios = 10,
                    admin = "admin@openuptool.com",
                    managers = new[] { "maria.gonzalez@openuptool.com", "roberto.sanchez@openuptool.com" },
                    developers = new[] { "carlos.ramirez@openuptool.com", "ana.martinez@openuptool.com", "luis.torres@openuptool.com", "laura.fernandez@openuptool.com", "jorge.hernandez@openuptool.com" },
                    testers = new[] { "patricia.lopez@openuptool.com", "viewer@openuptool.com" }
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al sembrar datos de prueba");
            return StatusCode(500, new { message = "Error al sembrar datos de prueba", error = ex.Message });
        }
    }

    /// <summary>
    /// Verifica el estado de la base de datos
    /// </summary>
    [HttpGet("status")]
    public async Task<IActionResult> GetDatabaseStatus()
    {
        try
        {
            var canConnect = await _context.Database.CanConnectAsync();
            
            if (!canConnect)
            {
                return Ok(new { status = "disconnected", message = "No se puede conectar a la base de datos" });
            }

            var counts = new
            {
                roles = await _context.Roles.CountAsync(),
                usuarios = await _context.Users.CountAsync(),
                proyectos = await _context.Projects.CountAsync(),
                fases = await _context.Phases.CountAsync(),
                planesProyecto = await _context.ProjectPlans.CountAsync(),
                milestones = await _context.Milestones.CountAsync(),
                tiposArtefactos = await _context.ArtifactTypes.CountAsync(),
                artefactos = await _context.Artifacts.CountAsync(),
                versionesArtefactos = await _context.ArtifactVersions.CountAsync(),
                iteraciones = await _context.Iterations.CountAsync(),
                userStories = await _context.UserStories.CountAsync(),
                alcanceIteracion = await _context.IterationScopes.CountAsync(),
                notificaciones = await _context.Notifications.CountAsync()
            };

            var isEmpty = counts.roles == 0 && counts.usuarios == 0 && counts.proyectos == 0;

            return Ok(new
            {
                status = "connected",
                isEmpty,
                message = isEmpty ? "Base de datos vacía" : "Base de datos con datos",
                registros = counts
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar estado de base de datos");
            return StatusCode(500, new { message = "Error al verificar estado", error = ex.Message });
        }
    }
}
