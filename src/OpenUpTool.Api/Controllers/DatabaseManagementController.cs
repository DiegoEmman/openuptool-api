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
            _context.Database.SetCommandTimeout(300); // Aumentar el timeout a 5 minutos
            await _context.Database.EnsureDeletedAsync();
            
            _logger.LogInformation("🔨 Creando base de datos...");
            _context.Database.SetCommandTimeout(300); // Aumentar el timeout a 5 minutos
            await _context.Database.EnsureCreatedAsync();
            
            _logger.LogInformation("✅ Base de datos recreada exitosamente");
            
            return Ok(new 
            { 
                message = "Base de datos recreada exitosamente. Todas las tablas han sido creadas.",
                totalTablas = 40,
                tablas = new[]
                {
                    // Configuración global y templates
                    "global_configurations",
                    "artifact_type_templates",
                    "configuration_change_history",
                    "phase_templates",
                    "role_templates",
                    "workflow_templates",
                    "workflow_state_templates",
                    "custom_field_definitions",
                    
                    // Usuarios y roles
                    "roles",
                    "users",
                    
                    // Proyectos
                    "projects",
                    "project_configurations",
                    "project_plans",
                    "project_closures",
                    "project_user_roles",
                    "project_invitations",
                    
                    // Fases e iteraciones
                    "phases",
                    "iterations",
                    "iteration_progress",
                    "iteration_tasks",
                    "iteration_scope",
                    "milestones",
                    
                    // Workflows
                    "workflows",
                    "workflow_states",
                    "workflow_permissions",
                    "workflow_state_responsibles",
                    
                    // Artefactos
                    "artifact_types",
                    "artifacts",
                    "artifact_versions",
                    "artifact_custom_field_values",
                    "artifact_movement_histories",
                    "artifact_state_history",
                    
                    // Historias de usuario y construcción
                    "user_stories",
                    "microincrements",
                    "final_builds",
                    
                    // Testing y defectos
                    "test_executions",
                    "defects",
                    
                    // Notificaciones y auditoría
                    "notifications",
                    "notification_preferences",
                    "AuditLogs"
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
                    Name = "Viewer", 
                    Description = "Usuario con permisos de solo lectura (viewer)",
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

                // 7.5. Crear algunos artefactos de ejemplo
                var visionType = artifactTypes.FirstOrDefault(at => at.Code == "VIS");
                var planType = artifactTypes.FirstOrDefault(at => at.Code == "PLAN");
                var ucmType = artifactTypes.FirstOrDefault(at => at.Code == "UCM");
                var inceptionPhaseForArtifacts = phases.FirstOrDefault(p => p.ProjectId == projects[0].Id && p.PhaseCode == "INCEPTION");
                var elaborationPhaseForArtifacts = phases.FirstOrDefault(p => p.ProjectId == projects[0].Id && p.PhaseCode == "ELABORATION");
                
                var projectArtifacts = new[]
                {
                    new Artifact
                    {
                        Id = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"),
                        ProjectId = projects[0].Id,
                        PhaseId = inceptionPhaseForArtifacts?.PhaseCode ?? "INCEPTION",
                        ArtifactTypeId = visionType?.Id ?? Guid.NewGuid(),
                        Title = "Documento de Visión - SGE",
                        Description = "Documento que describe la visión general del Sistema de Gestión Empresarial",
                        Status = "Aprobado",
                        Author = "Admin Sistema",
                        IsMandatory = true,
                        CreatedAt = now.AddDays(-10)
                    },
                    new Artifact
                    {
                        Id = Guid.NewGuid(),
                        ProjectId = projects[0].Id,
                        PhaseId = inceptionPhaseForArtifacts?.PhaseCode ?? "INCEPTION",
                        ArtifactTypeId = planType?.Id ?? Guid.NewGuid(),
                        Title = "Plan de Proyecto - SGE",
                        Description = "Plan detallado con cronograma y asignación de recursos",
                        Status = "En revisión",
                        Author = "Admin Sistema",
                        IsMandatory = true,
                        CreatedAt = now.AddDays(-8)
                    },
                    new Artifact
                    {
                        Id = Guid.NewGuid(),
                        ProjectId = projects[0].Id,
                        PhaseId = elaborationPhaseForArtifacts?.PhaseCode ?? "ELABORATION",
                        ArtifactTypeId = ucmType?.Id ?? Guid.NewGuid(),
                        Title = "Modelo de Casos de Uso",
                        Description = "Casos de uso principales del sistema",
                        Status = "Pendiente",
                        Author = "Carlos Ramírez",
                        IsMandatory = true,
                        CreatedAt = now.AddDays(-5)
                    }
                };
                await _context.Artifacts.AddRangeAsync(projectArtifacts);
                await _context.SaveChangesAsync();
                _logger.LogInformation("✅ Artefactos de ejemplo creados (3 artefactos)");

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

            // 15. Crear tareas de iteración
            var iterationTasks = new[]
            {
                new IterationTask
                {
                    Id = Guid.NewGuid(),
                    IterationId = elaborationPhase != null ? await _context.Iterations.Where(i => i.ProjectId == projects[0].Id).Select(i => i.Id).FirstOrDefaultAsync() : Guid.NewGuid(),
                    Name = "Diseñar base de datos",
                    Description = "Crear el modelo de datos y definir las tablas principales",
                    Status = "Completada",
                    EstimatedHours = 8,
                    ActualHours = 7.5m,
                    AssignedTo = users[3].Id, // Carlos Ramírez
                    StartDate = now.AddDays(-6),
                    EndDate = now.AddDays(-5),
                    Priority = 1,
                    CreatedAt = now.AddDays(-7),
                    UpdatedAt = now.AddDays(-5)
                },
                new IterationTask
                {
                    Id = Guid.NewGuid(),
                    IterationId = elaborationPhase != null ? await _context.Iterations.Where(i => i.ProjectId == projects[0].Id).Select(i => i.Id).FirstOrDefaultAsync() : Guid.NewGuid(),
                    Name = "Implementar autenticación",
                    Description = "Desarrollar sistema de login con JWT",
                    Status = "En Progreso",
                    EstimatedHours = 12,
                    ActualHours = 8,
                    AssignedTo = users[4].Id, // Ana Martínez
                    StartDate = now.AddDays(-4),
                    Priority = 1,
                    CreatedAt = now.AddDays(-5),
                    UpdatedAt = now
                },
                new IterationTask
                {
                    Id = Guid.NewGuid(),
                    IterationId = elaborationPhase != null ? await _context.Iterations.Where(i => i.ProjectId == projects[0].Id).Select(i => i.Id).FirstOrDefaultAsync() : Guid.NewGuid(),
                    Name = "Crear componentes de UI",
                    Description = "Desarrollar los componentes base del frontend",
                    Status = "Pendiente",
                    EstimatedHours = 16,
                    AssignedTo = users[5].Id, // Luis Torres
                    Priority = 2,
                    CreatedAt = now.AddDays(-3),
                    UpdatedAt = now
                },
                new IterationTask
                {
                    Id = Guid.NewGuid(),
                    IterationId = elaborationPhase != null ? await _context.Iterations.Where(i => i.ProjectId == projects[0].Id).Select(i => i.Id).FirstOrDefaultAsync() : Guid.NewGuid(),
                    Name = "Configurar CI/CD",
                    Description = "Implementar pipeline de integración continua",
                    Status = "Bloqueada",
                    EstimatedHours = 6,
                    AssignedTo = users[3].Id, // Carlos Ramírez
                    Priority = 3,
                    BlockerDescription = "Esperando acceso al servidor de CI/CD",
                    CreatedAt = now.AddDays(-2),
                    UpdatedAt = now
                }
            };
            await _context.IterationTasks.AddRangeAsync(iterationTasks);
            await _context.SaveChangesAsync();
            _logger.LogInformation("✅ Tareas de iteración creadas");

            // 16. Crear registros de progreso de iteración
            if (elaborationPhase != null)
            {
                var iterationId = await _context.Iterations
                    .Where(i => i.ProjectId == projects[0].Id && i.Status == "En curso")
                    .Select(i => i.Id)
                    .FirstOrDefaultAsync();

                if (iterationId != Guid.Empty)
                {
                    var iterationProgress = new[]
                    {
                        new IterationProgress
                        {
                            Id = Guid.NewGuid(),
                            IterationId = iterationId,
                            RecordDate = now.AddDays(-7),
                            CompletionPercentage = 10,
                            TotalTasks = 4,
                            CompletedTasks = 0,
                            InProgressTasks = 1,
                            BlockedTasks = 0,
                            Observations = "Inicio de iteración, tareas asignadas",
                            CreatedAt = now.AddDays(-7),
                            UpdatedAt = now.AddDays(-7)
                        },
                        new IterationProgress
                        {
                            Id = Guid.NewGuid(),
                            IterationId = iterationId,
                            RecordDate = now.AddDays(-4),
                            CompletionPercentage = 35,
                            TotalTasks = 4,
                            CompletedTasks = 1,
                            InProgressTasks = 2,
                            BlockedTasks = 0,
                            Observations = "Diseño de BD completado. Auth y UI en progreso",
                            CreatedAt = now.AddDays(-4),
                            UpdatedAt = now.AddDays(-4)
                        },
                        new IterationProgress
                        {
                            Id = Guid.NewGuid(),
                            IterationId = iterationId,
                            RecordDate = now,
                            CompletionPercentage = 50,
                            TotalTasks = 4,
                            CompletedTasks = 1,
                            InProgressTasks = 1,
                            BlockedTasks = 1,
                            Blockers = "Tarea de CI/CD bloqueada por falta de acceso al servidor",
                            Observations = "Buen avance general. Necesitamos resolver el bloqueo de CI/CD",
                            CreatedAt = now,
                            UpdatedAt = now
                        }
                    };
                    await _context.IterationProgresses.AddRangeAsync(iterationProgress);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("✅ Progreso de iteración registrado");
                }
            }

            // 17. Crear test executions
            var testExecutions = new[]
            {
                new TestExecution
                {
                    Id = Guid.NewGuid(),
                    ArtifactId = artifacts[1].Id, // Casos de Uso SGE
                    TestCaseId = "TC-001",
                    TestCaseName = "Validar login con credenciales correctas",
                    Result = "Passed",
                    ExecutedBy = users[8].Id, // Patricia López (Tester)
                    ExecutedAt = now.AddDays(-2),
                    DurationSeconds = 45,
                    ArtifactVersionId = artifactVersions[4].Id,
                    Environment = "Windows 11, Chrome 120",
                    Notes = "Prueba exitosa. Login funciona correctamente.",
                    CreatedAt = now.AddDays(-2),
                    UpdatedAt = now.AddDays(-2)
                },
                new TestExecution
                {
                    Id = Guid.NewGuid(),
                    ArtifactId = artifacts[1].Id,
                    TestCaseId = "TC-002",
                    TestCaseName = "Validar login con credenciales incorrectas",
                    Result = "Passed",
                    ExecutedBy = users[8].Id,
                    ExecutedAt = now.AddDays(-2),
                    DurationSeconds = 30,
                    ArtifactVersionId = artifactVersions[4].Id,
                    Environment = "Windows 11, Chrome 120",
                    Notes = "Se muestra el mensaje de error apropiado.",
                    CreatedAt = now.AddDays(-2),
                    UpdatedAt = now.AddDays(-2)
                },
                new TestExecution
                {
                    Id = Guid.NewGuid(),
                    ArtifactId = artifacts[1].Id,
                    TestCaseId = "TC-003",
                    TestCaseName = "Validar recuperación de contraseña",
                    Result = "Failed",
                    ExecutedBy = users[8].Id,
                    ExecutedAt = now.AddDays(-1),
                    DurationSeconds = 120,
                    ArtifactVersionId = artifactVersions[4].Id,
                    Environment = "Windows 11, Firefox 115",
                    Notes = "El email de recuperación no se está enviando.",
                    CreatedAt = now.AddDays(-1),
                    UpdatedAt = now.AddDays(-1)
                },
                new TestExecution
                {
                    Id = Guid.NewGuid(),
                    ArtifactId = artifacts[2].Id, // Arquitectura SGE
                    TestCaseId = "TC-010",
                    TestCaseName = "Validar conectividad con base de datos",
                    Result = "Passed",
                    ExecutedBy = users[8].Id,
                    ExecutedAt = now.AddDays(-1),
                    DurationSeconds = 15,
                    ArtifactVersionId = artifactVersions[5].Id,
                    Environment = "PostgreSQL 16",
                    Notes = "Conexión establecida exitosamente.",
                    CreatedAt = now.AddDays(-1),
                    UpdatedAt = now.AddDays(-1)
                }
            };
            await _context.TestExecutions.AddRangeAsync(testExecutions);
            await _context.SaveChangesAsync();
            _logger.LogInformation("✅ Ejecuciones de pruebas creadas");

            // 18. Crear defectos
            var defects = new[]
            {
                new Defect
                {
                    Id = Guid.NewGuid(),
                    DefectNumber = "DEF-001",
                    Title = "Email de recuperación no se envía",
                    Description = "Cuando un usuario solicita recuperar su contraseña, el sistema no envía el email con el enlace de recuperación.",
                    Severity = "High",
                    Status = "Open",
                    Priority = "High",
                    Type = "Functional",
                    ProjectId = projects[0].Id,
                    ArtifactId = artifacts[1].Id,
                    ArtifactVersionId = artifactVersions[4].Id,
                    TestExecutionId = testExecutions[2].Id,
                    ReportedBy = users[8].Id, // Patricia López
                    ReportedAt = now.AddDays(-1).AddHours(-2),
                    AssignedTo = users[4].Id, // Ana Martínez
                    AssignedAt = now.AddDays(-1).AddHours(-1),
                    StepsToReproduce = "1. Ir a la página de login\n2. Click en 'Olvidé mi contraseña'\n3. Ingresar email registrado\n4. Click en 'Enviar'\n5. Verificar bandeja de entrada",
                    ExpectedResult = "El usuario debe recibir un email con enlace de recuperación en menos de 2 minutos.",
                    ActualResult = "No se recibe ningún email. La respuesta del sistema indica éxito pero el email no llega.",
                    Environment = "Windows 11, Firefox 115, PostgreSQL 16",
                    Tags = "login,authentication,email",
                    CreatedAt = now.AddDays(-1).AddHours(-2),
                    UpdatedAt = now.AddDays(-1).AddHours(-1)
                },
                new Defect
                {
                    Id = Guid.NewGuid(),
                    DefectNumber = "DEF-002",
                    Title = "Error de validación en campo de email",
                    Description = "El campo de email no valida correctamente el formato y permite ingresar textos sin @",
                    Severity = "Medium",
                    Status = "In Progress",
                    Priority = "Medium",
                    Type = "Functional",
                    ProjectId = projects[0].Id,
                    ArtifactId = artifacts[1].Id,
                    ArtifactVersionId = artifactVersions[4].Id,
                    ReportedBy = users[8].Id,
                    ReportedAt = now.AddDays(-3),
                    AssignedTo = users[5].Id, // Luis Torres
                    AssignedAt = now.AddDays(-3).AddHours(2),
                    StepsToReproduce = "1. Ir al formulario de registro\n2. En el campo email escribir 'usuario'\n3. Intentar enviar el formulario",
                    ExpectedResult = "Debe mostrar error: 'Por favor ingrese un email válido'",
                    ActualResult = "El formulario se envía sin mostrar error de validación.",
                    Environment = "Windows 11, Chrome 120",
                    Tags = "validation,form,email",
                    CreatedAt = now.AddDays(-3),
                    UpdatedAt = now.AddHours(-2)
                },
                new Defect
                {
                    Id = Guid.NewGuid(),
                    DefectNumber = "DEF-003",
                    Title = "Timeout en consultas complejas",
                    Description = "Las consultas que involucran múltiples JOINs tardan más de 30 segundos y algunas terminan en timeout.",
                    Severity = "Critical",
                    Status = "Resolved",
                    Priority = "Urgent",
                    Type = "Performance",
                    ProjectId = projects[0].Id,
                    ArtifactId = artifacts[2].Id,
                    ArtifactVersionId = artifactVersions[5].Id,
                    ReportedBy = users[3].Id, // Carlos Ramírez
                    ReportedAt = now.AddDays(-5),
                    AssignedTo = users[3].Id,
                    AssignedAt = now.AddDays(-5).AddHours(1),
                    ResolvedAt = now.AddDays(-4),
                    ResolvedBy = users[3].Id,
                    Resolution = "Se agregaron índices a las columnas más utilizadas en los JOINs. Se redujo el tiempo promedio de consulta a 2 segundos.",
                    StepsToReproduce = "1. Acceder al dashboard principal\n2. Aplicar filtros de fecha del último año\n3. Seleccionar todos los proyectos\n4. Generar reporte",
                    ExpectedResult = "El reporte debe generarse en menos de 5 segundos.",
                    ActualResult = "El reporte tarda más de 30 segundos o termina en timeout.",
                    Environment = "PostgreSQL 16, 1000 registros en BD",
                    Tags = "performance,database,query",
                    CreatedAt = now.AddDays(-5),
                    UpdatedAt = now.AddDays(-4)
                },
                new Defect
                {
                    Id = Guid.NewGuid(),
                    DefectNumber = "DEF-004",
                    Title = "Botón de guardar no responde en formularios largos",
                    Description = "En formularios con más de 20 campos, el botón de guardar no responde al primer click.",
                    Severity = "Low",
                    Status = "Closed",
                    Priority = "Low",
                    Type = "UI",
                    ProjectId = projects[0].Id,
                    ReportedBy = users[9].Id, // Viewer
                    ReportedAt = now.AddDays(-10),
                    AssignedTo = users[5].Id,
                    AssignedAt = now.AddDays(-10).AddHours(3),
                    ResolvedAt = now.AddDays(-8),
                    ResolvedBy = users[5].Id,
                    Resolution = "Se agregó feedback visual mientras se procesa el guardado. Se mejoró la experiencia de usuario.",
                    StepsToReproduce = "1. Abrir formulario de proyecto nuevo\n2. Llenar todos los campos\n3. Click en botón Guardar\n4. Observar que no hay respuesta inmediata",
                    ExpectedResult = "El botón debe mostrar algún indicador de que está procesando.",
                    ActualResult = "El botón no muestra ninguna retroalimentación inmediata.",
                    Environment = "Windows 11, Chrome 120",
                    Tags = "ui,ux,feedback",
                    CreatedAt = now.AddDays(-10),
                    UpdatedAt = now.AddDays(-7)
                },
                new Defect
                {
                    Id = Guid.NewGuid(),
                    DefectNumber = "DEF-005",
                    Title = "Inconsistencia en formato de fechas",
                    Description = "Las fechas se muestran en diferentes formatos en distintas secciones de la aplicación.",
                    Severity = "Low",
                    Status = "Reopened",
                    Priority = "Medium",
                    Type = "UI",
                    ProjectId = projects[0].Id,
                    ReportedBy = users[1].Id, // María González
                    ReportedAt = now.AddDays(-8),
                    AssignedTo = users[5].Id,
                    AssignedAt = now.AddDays(-8).AddHours(1),
                    ResolvedAt = now.AddDays(-6),
                    ResolvedBy = users[5].Id,
                    Resolution = "Se estandarizó el formato a DD/MM/YYYY en toda la aplicación.",
                    StepsToReproduce = "1. Ver lista de proyectos (formato MM/DD/YYYY)\n2. Ver detalle de proyecto (formato DD-MM-YYYY)\n3. Ver iteraciones (formato YYYY/MM/DD)",
                    ExpectedResult = "Todas las fechas deben tener el mismo formato.",
                    ActualResult = "Cada sección muestra las fechas en formato diferente.",
                    Environment = "Windows 11, Chrome 120",
                    Tags = "ui,format,consistency",
                    CreatedAt = now.AddDays(-8),
                    UpdatedAt = now.AddHours(-3)
                }
            };
            await _context.Defects.AddRangeAsync(defects);
            await _context.SaveChangesAsync();
            _logger.LogInformation("✅ Defectos creados");

            // 19. Crear notificaciones de ejemplo
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

            // 20. Crear Workflows de ejemplo
            var workflows = new[]
            {
                new Workflow
                {
                    Id = Guid.Parse("ffffffff-1111-2222-3333-444444444444"),
                    ProjectId = projects[0].Id, // SGE
                    Name = "Flujo de Revisión de Documentos",
                    Description = "Flujo estándar para revisión y aprobación de documentos",
                    IsActive = true,
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Workflow
                {
                    Id = Guid.Parse("ffffffff-5555-6666-7777-888888888888"),
                    ProjectId = projects[0].Id, // SGE
                    Name = "Flujo de Desarrollo de Código",
                    Description = "Flujo para gestión de código fuente desde desarrollo hasta producción",
                    IsActive = true,
                    CreatedAt = now,
                    UpdatedAt = now
                }
            };
            await _context.Workflows.AddRangeAsync(workflows);
            await _context.SaveChangesAsync();
            _logger.LogInformation("✅ Workflows creados");

            // 21. Crear Estados para los Workflows
            var workflowStates = new[]
            {
                // Estados para Flujo de Revisión de Documentos
                new WorkflowState
                {
                    Id = Guid.Parse("eeeeeeee-1111-1111-1111-111111111111"),
                    WorkflowId = workflows[0].Id,
                    Name = "Borrador",
                    Description = "Documento en elaboración inicial",
                    Order = 1,
                    Color = "#9CA3AF",
                    IsInitialState = true,
                    IsFinalState = false,
                    RequiredActions = "[\"Completar contenido\",\"Revisar formato\"]",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new WorkflowState
                {
                    Id = Guid.Parse("eeeeeeee-2222-2222-2222-222222222222"),
                    WorkflowId = workflows[0].Id,
                    Name = "En Revisión",
                    Description = "Documento bajo revisión por pares",
                    Order = 2,
                    Color = "#F59E0B",
                    IsInitialState = false,
                    IsFinalState = false,
                    RequiredActions = "[\"Revisar contenido\",\"Validar formato\",\"Agregar comentarios\"]",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new WorkflowState
                {
                    Id = Guid.Parse("eeeeeeee-3333-3333-3333-333333333333"),
                    WorkflowId = workflows[0].Id,
                    Name = "Aprobado",
                    Description = "Documento aprobado para uso",
                    Order = 3,
                    Color = "#10B981",
                    IsInitialState = false,
                    IsFinalState = true,
                    RequiredActions = "[\"Publicar documento\"]",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                // Estados para Flujo de Desarrollo de Código
                new WorkflowState
                {
                    Id = Guid.Parse("eeeeeeee-4444-4444-4444-444444444444"),
                    WorkflowId = workflows[1].Id,
                    Name = "Desarrollo",
                    Description = "Código en desarrollo activo",
                    Order = 1,
                    Color = "#6366F1",
                    IsInitialState = true,
                    IsFinalState = false,
                    RequiredActions = "[\"Implementar funcionalidad\",\"Escribir tests\"]",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new WorkflowState
                {
                    Id = Guid.Parse("eeeeeeee-5555-5555-5555-555555555555"),
                    WorkflowId = workflows[1].Id,
                    Name = "Testing",
                    Description = "Código en fase de pruebas",
                    Order = 2,
                    Color = "#8B5CF6",
                    IsInitialState = false,
                    IsFinalState = false,
                    RequiredActions = "[\"Ejecutar tests unitarios\",\"Ejecutar tests de integración\",\"Validar cobertura\"]",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new WorkflowState
                {
                    Id = Guid.Parse("eeeeeeee-6666-6666-6666-666666666666"),
                    WorkflowId = workflows[1].Id,
                    Name = "Producción",
                    Description = "Código desplegado en producción",
                    Order = 3,
                    Color = "#059669",
                    IsInitialState = false,
                    IsFinalState = true,
                    RequiredActions = "[\"Monitorear métricas\"]",
                    CreatedAt = now,
                    UpdatedAt = now
                }
            };
            await _context.WorkflowStates.AddRangeAsync(workflowStates);
            await _context.SaveChangesAsync();
            _logger.LogInformation("✅ Estados de workflows creados");

            // 22. Asignar responsables a estados
            var stateResponsibles = new[]
            {
                // Revisores para estado "En Revisión"
                new WorkflowStateResponsible
                {
                    Id = Guid.NewGuid(),
                    WorkflowStateId = workflowStates[1].Id, // En Revisión
                    UserId = users[1].Id, // Manager
                    Role = "Revisor Principal",
                    AssignedAt = now
                },
                new WorkflowStateResponsible
                {
                    Id = Guid.NewGuid(),
                    WorkflowStateId = workflowStates[1].Id, // En Revisión
                    UserId = users[4].Id, // Developer
                    Role = "Revisor Técnico",
                    AssignedAt = now
                },
                // Aprobador para estado "Aprobado"
                new WorkflowStateResponsible
                {
                    Id = Guid.NewGuid(),
                    WorkflowStateId = workflowStates[2].Id, // Aprobado
                    UserId = users[1].Id, // Manager
                    Role = "Aprobador",
                    AssignedAt = now
                },
                // Responsables para Testing
                new WorkflowStateResponsible
                {
                    Id = Guid.NewGuid(),
                    WorkflowStateId = workflowStates[4].Id, // Testing
                    UserId = users[6].Id, // Tester
                    Role = "QA Lead",
                    AssignedAt = now
                }
            };
            await _context.WorkflowStateResponsibles.AddRangeAsync(stateResponsibles);
            await _context.SaveChangesAsync();
            _logger.LogInformation("✅ Responsables de estados asignados");

            // 23. Crear permisos de workflows (HU-013)
            var workflowPermissions = new[]
            {
                // Permisos para "Flujo de Desarrollo de Código" (workflows[1])
                // Rol: autor
                new WorkflowPermission { Id = Guid.NewGuid(), WorkflowId = workflows[1].Id, Role = "autor", Action = "crear", IsAllowed = true, CreatedAt = now },
                new WorkflowPermission { Id = Guid.NewGuid(), WorkflowId = workflows[1].Id, Role = "autor", Action = "editar", IsAllowed = true, CreatedAt = now },
                new WorkflowPermission { Id = Guid.NewGuid(), WorkflowId = workflows[1].Id, Role = "autor", Action = "aprobar", IsAllowed = false, CreatedAt = now },
                new WorkflowPermission { Id = Guid.NewGuid(), WorkflowId = workflows[1].Id, Role = "autor", Action = "cambiar_estado", IsAllowed = true, CreatedAt = now },
                
                // Rol: revisor
                new WorkflowPermission { Id = Guid.NewGuid(), WorkflowId = workflows[1].Id, Role = "revisor", Action = "crear", IsAllowed = false, CreatedAt = now },
                new WorkflowPermission { Id = Guid.NewGuid(), WorkflowId = workflows[1].Id, Role = "revisor", Action = "editar", IsAllowed = true, CreatedAt = now },
                new WorkflowPermission { Id = Guid.NewGuid(), WorkflowId = workflows[1].Id, Role = "revisor", Action = "aprobar", IsAllowed = true, CreatedAt = now },
                new WorkflowPermission { Id = Guid.NewGuid(), WorkflowId = workflows[1].Id, Role = "revisor", Action = "cambiar_estado", IsAllowed = true, CreatedAt = now },
                
                // Rol: PO (Product Owner)
                new WorkflowPermission { Id = Guid.NewGuid(), WorkflowId = workflows[1].Id, Role = "PO", Action = "crear", IsAllowed = true, CreatedAt = now },
                new WorkflowPermission { Id = Guid.NewGuid(), WorkflowId = workflows[1].Id, Role = "PO", Action = "editar", IsAllowed = true, CreatedAt = now },
                new WorkflowPermission { Id = Guid.NewGuid(), WorkflowId = workflows[1].Id, Role = "PO", Action = "aprobar", IsAllowed = true, CreatedAt = now },
                new WorkflowPermission { Id = Guid.NewGuid(), WorkflowId = workflows[1].Id, Role = "PO", Action = "cambiar_estado", IsAllowed = true, CreatedAt = now },
                
                // Rol: admin
                new WorkflowPermission { Id = Guid.NewGuid(), WorkflowId = workflows[1].Id, Role = "admin", Action = "crear", IsAllowed = true, CreatedAt = now },
                new WorkflowPermission { Id = Guid.NewGuid(), WorkflowId = workflows[1].Id, Role = "admin", Action = "editar", IsAllowed = true, CreatedAt = now },
                new WorkflowPermission { Id = Guid.NewGuid(), WorkflowId = workflows[1].Id, Role = "admin", Action = "aprobar", IsAllowed = true, CreatedAt = now },
                new WorkflowPermission { Id = Guid.NewGuid(), WorkflowId = workflows[1].Id, Role = "admin", Action = "cambiar_estado", IsAllowed = true, CreatedAt = now },
                
                // Permisos para "Flujo de Revisión de Documentos" (workflows[0])
                // Rol: autor
                new WorkflowPermission { Id = Guid.NewGuid(), WorkflowId = workflows[0].Id, Role = "autor", Action = "crear", IsAllowed = true, CreatedAt = now },
                new WorkflowPermission { Id = Guid.NewGuid(), WorkflowId = workflows[0].Id, Role = "autor", Action = "editar", IsAllowed = true, CreatedAt = now },
                new WorkflowPermission { Id = Guid.NewGuid(), WorkflowId = workflows[0].Id, Role = "autor", Action = "aprobar", IsAllowed = false, CreatedAt = now },
                new WorkflowPermission { Id = Guid.NewGuid(), WorkflowId = workflows[0].Id, Role = "autor", Action = "cambiar_estado", IsAllowed = false, CreatedAt = now },
                
                // Rol: revisor
                new WorkflowPermission { Id = Guid.NewGuid(), WorkflowId = workflows[0].Id, Role = "revisor", Action = "crear", IsAllowed = false, CreatedAt = now },
                new WorkflowPermission { Id = Guid.NewGuid(), WorkflowId = workflows[0].Id, Role = "revisor", Action = "editar", IsAllowed = false, CreatedAt = now },
                new WorkflowPermission { Id = Guid.NewGuid(), WorkflowId = workflows[0].Id, Role = "revisor", Action = "aprobar", IsAllowed = true, CreatedAt = now },
                new WorkflowPermission { Id = Guid.NewGuid(), WorkflowId = workflows[0].Id, Role = "revisor", Action = "cambiar_estado", IsAllowed = true, CreatedAt = now },
                
                // Rol: PO
                new WorkflowPermission { Id = Guid.NewGuid(), WorkflowId = workflows[0].Id, Role = "PO", Action = "crear", IsAllowed = true, CreatedAt = now },
                new WorkflowPermission { Id = Guid.NewGuid(), WorkflowId = workflows[0].Id, Role = "PO", Action = "editar", IsAllowed = true, CreatedAt = now },
                new WorkflowPermission { Id = Guid.NewGuid(), WorkflowId = workflows[0].Id, Role = "PO", Action = "aprobar", IsAllowed = true, CreatedAt = now },
                new WorkflowPermission { Id = Guid.NewGuid(), WorkflowId = workflows[0].Id, Role = "PO", Action = "cambiar_estado", IsAllowed = true, CreatedAt = now },
                
                // Rol: admin
                new WorkflowPermission { Id = Guid.NewGuid(), WorkflowId = workflows[0].Id, Role = "admin", Action = "crear", IsAllowed = true, CreatedAt = now },
                new WorkflowPermission { Id = Guid.NewGuid(), WorkflowId = workflows[0].Id, Role = "admin", Action = "editar", IsAllowed = true, CreatedAt = now },
                new WorkflowPermission { Id = Guid.NewGuid(), WorkflowId = workflows[0].Id, Role = "admin", Action = "aprobar", IsAllowed = true, CreatedAt = now },
                new WorkflowPermission { Id = Guid.NewGuid(), WorkflowId = workflows[0].Id, Role = "admin", Action = "cambiar_estado", IsAllowed = true, CreatedAt = now }
            };
            await _context.WorkflowPermissions.AddRangeAsync(workflowPermissions);
            await _context.SaveChangesAsync();
            _logger.LogInformation("✅ Permisos de workflows creados");

            // 24. Asignar workflow al artefacto de prueba y crear historial
            var artifactVision = await _context.Artifacts.FindAsync(Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"));
            if (artifactVision != null)
            {
                artifactVision.WorkflowId = workflows[0].Id;
                artifactVision.CurrentStateId = workflowStates[1].Id; // En Revisión
                
                var stateHistory = new[]
                {
                    new ArtifactStateHistory
                    {
                        Id = Guid.NewGuid(),
                        ArtifactId = artifactVision.Id,
                        FromStateId = null,
                        ToStateId = workflowStates[0].Id, // Borrador
                        ChangedByUserId = users[0].Id, // Admin
                        ChangedAt = now.AddDays(-7),
                        Comments = "Creación inicial del documento Visión",
                        Metadata = null
                    },
                    new ArtifactStateHistory
                    {
                        Id = Guid.NewGuid(),
                        ArtifactId = artifactVision.Id,
                        FromStateId = workflowStates[0].Id, // Borrador
                        ToStateId = workflowStates[1].Id, // En Revisión
                        ChangedByUserId = users[2].Id, // Developer
                        ChangedAt = now.AddDays(-2),
                        Comments = "Documento completado, listo para revisión",
                        Metadata = "{\"version\":\"1.0\",\"reviewers\":[\"" + users[1].Id + "\",\"" + users[4].Id + "\"]}"
                    }
                };
                await _context.ArtifactStateHistories.AddRangeAsync(stateHistory);
                await _context.SaveChangesAsync();
                _logger.LogInformation("✅ Workflow asignado a artefacto con historial");
            }

            // 25. Crear Configuración Global
            var globalConfiguration = new GlobalConfiguration
            {
                Id = Guid.NewGuid(),
                Name = "OpenUP Standard Configuration",
                Description = "Configuración estándar OpenUP con roles, fases, tipos de artefactos y workflows predefinidos",
                Version = 1,
                IsActive = true,
                IsDefault = true,
                CreatedAt = now,
                UpdatedAt = now,
                CreatedBy = "System"
            };
            await _context.GlobalConfigurations.AddAsync(globalConfiguration);
            await _context.SaveChangesAsync();
            _logger.LogInformation("✅ Configuración global creada");

            // 26. Crear Plantillas de Tipos de Artefactos
            var artifactTypeTemplates = new[]
            {
                new ArtifactTypeTemplate
                {
                    Id = Guid.NewGuid(),
                    ConfigurationId = globalConfiguration.Id,
                    Name = "Vision Document Template",
                    Description = "Plantilla estándar para documentos de visión según OpenUP",
                    PhaseCode = "INCEPTION",
                    Code = "VIS-TPL",
                    DefaultFormat = "TEXT",
                    IsMandatory = true,
                    OrderIndex = 1,
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new ArtifactTypeTemplate
                {
                    Id = Guid.NewGuid(),
                    ConfigurationId = globalConfiguration.Id,
                    Name = "Architecture Notebook Template",
                    Description = "Plantilla para cuaderno de arquitectura",
                    PhaseCode = "ELABORATION",
                    Code = "ARCH-TPL",
                    DefaultFormat = "MIXED",
                    IsMandatory = true,
                    OrderIndex = 2,
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new ArtifactTypeTemplate
                {
                    Id = Guid.NewGuid(),
                    ConfigurationId = globalConfiguration.Id,
                    Name = "Test Case Template",
                    Description = "Plantilla para casos de prueba",
                    PhaseCode = "CONSTRUCTION",
                    Code = "TST-TPL",
                    DefaultFormat = "TEXT",
                    IsMandatory = true,
                    OrderIndex = 3,
                    CreatedAt = now,
                    UpdatedAt = now
                }
            };
            await _context.ArtifactTypeTemplates.AddRangeAsync(artifactTypeTemplates);
            await _context.SaveChangesAsync();
            _logger.LogInformation("✅ Plantillas de tipos de artefactos creadas");

            // 27. Crear Plantillas de Fases
            var phaseTemplates = new[]
            {
                new PhaseTemplate
                {
                    Id = Guid.NewGuid(),
                    ConfigurationId = globalConfiguration.Id,
                    Name = "Inception Phase Template",
                    Description = "Plantilla estándar para fase de Inicio según OpenUP",
                    PhaseCode = "INCEPTION",
                    DefaultDurationDays = 14,
                    IsMandatory = true,
                    OrderIndex = 1,
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new PhaseTemplate
                {
                    Id = Guid.NewGuid(),
                    ConfigurationId = globalConfiguration.Id,
                    Name = "Elaboration Phase Template",
                    Description = "Plantilla para fase de Elaboración",
                    PhaseCode = "ELABORATION",
                    DefaultDurationDays = 28,
                    IsMandatory = true,
                    OrderIndex = 2,
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new PhaseTemplate
                {
                    Id = Guid.NewGuid(),
                    ConfigurationId = globalConfiguration.Id,
                    Name = "Construction Phase Template",
                    Description = "Plantilla para fase de Construcción",
                    PhaseCode = "CONSTRUCTION",
                    DefaultDurationDays = 56,
                    IsMandatory = true,
                    OrderIndex = 3,
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new PhaseTemplate
                {
                    Id = Guid.NewGuid(),
                    ConfigurationId = globalConfiguration.Id,
                    Name = "Transition Phase Template",
                    Description = "Plantilla para fase de Transición",
                    PhaseCode = "TRANSITION",
                    DefaultDurationDays = 14,
                    IsMandatory = true,
                    OrderIndex = 4,
                    CreatedAt = now,
                    UpdatedAt = now
                }
            };
            await _context.PhaseTemplates.AddRangeAsync(phaseTemplates);
            await _context.SaveChangesAsync();
            _logger.LogInformation("✅ Plantillas de fases creadas");

            // 28. Crear Plantillas de Roles
            var roleTemplates = new[]
            {
                new RoleTemplate
                {
                    Id = Guid.NewGuid(),
                    ConfigurationId = globalConfiguration.Id,
                    Name = "Product Owner",
                    Description = "Responsable de definir y priorizar requisitos",
                    Permissions = "[\"CREATE_PROJECT\",\"EDIT_REQUIREMENTS\",\"APPROVE_DELIVERABLES\"]",
                    OrderIndex = 1,
                    IsSystem = false,
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new RoleTemplate
                {
                    Id = Guid.NewGuid(),
                    ConfigurationId = globalConfiguration.Id,
                    Name = "Scrum Master",
                    Description = "Facilita el proceso y elimina impedimentos",
                    Permissions = "[\"VIEW_PROJECTS\",\"MANAGE_SPRINTS\",\"REMOVE_IMPEDIMENTS\"]",
                    OrderIndex = 2,
                    IsSystem = false,
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new RoleTemplate
                {
                    Id = Guid.NewGuid(),
                    ConfigurationId = globalConfiguration.Id,
                    Name = "Software Architect",
                    Description = "Define la arquitectura técnica del sistema",
                    Permissions = "[\"VIEW_PROJECTS\",\"EDIT_ARCHITECTURE\",\"REVIEW_CODE\",\"APPROVE_DESIGN\"]",
                    OrderIndex = 3,
                    IsSystem = false,
                    CreatedAt = now,
                    UpdatedAt = now
                }
            };
            await _context.RoleTemplates.AddRangeAsync(roleTemplates);
            await _context.SaveChangesAsync();
            _logger.LogInformation("✅ Plantillas de roles creadas");

            // 29. Crear Plantillas de Workflows
            var workflowTemplates = new[]
            {
                new WorkflowTemplate
                {
                    Id = Guid.NewGuid(),
                    ConfigurationId = globalConfiguration.Id,
                    Name = "Document Review Workflow",
                    Description = "Flujo estándar para revisión de documentos",
                    IsDefault = true,
                    OrderIndex = 1,
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new WorkflowTemplate
                {
                    Id = Guid.NewGuid(),
                    ConfigurationId = globalConfiguration.Id,
                    Name = "Code Development Workflow",
                    Description = "Flujo para desarrollo de código fuente",
                    IsDefault = false,
                    OrderIndex = 2,
                    CreatedAt = now,
                    UpdatedAt = now
                }
            };
            await _context.WorkflowTemplates.AddRangeAsync(workflowTemplates);
            await _context.SaveChangesAsync();
            _logger.LogInformation("✅ Plantillas de workflows creadas");

            // 30. Crear Configuraciones de Proyecto
            var projectConfigurations = new[]
            {
                new ProjectConfiguration
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projects[0].Id, // SGE
                    ConfigurationId = globalConfiguration.Id,
                    AppliedVersion = 1,
                    AppliedAt = now,
                    AppliedBy = "admin@openuptool.com",
                    AutoUpdate = true
                },
                new ProjectConfiguration
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projects[1].Id, // AMB
                    ConfigurationId = globalConfiguration.Id,
                    AppliedVersion = 1,
                    AppliedAt = now,
                    AppliedBy = "admin@openuptool.com",
                    AutoUpdate = false
                }
            };
            await _context.ProjectConfigurations.AddRangeAsync(projectConfigurations);
            await _context.SaveChangesAsync();
            _logger.LogInformation("✅ Configuraciones de proyecto creadas");

            // 31. Crear Microincrementos
            var firstIterationId = await _context.Iterations.Where(i => i.ProjectId == projects[0].Id).Select(i => i.Id).FirstOrDefaultAsync();
            var microincrements = new[]
            {
                new Microincrement
                {
                    Id = Guid.NewGuid(),
                    IterationId = firstIterationId,
                    ArtifactId = artifacts[0].Id,
                    Title = "API de Autenticación",
                    Description = "Implementación completa del módulo de autenticación JWT",
                    Type = "tecnico",
                    Date = now.AddDays(-10),
                    Author = "Ana Martínez",
                    EvidenceUrl = "https://github.com/example/commit/abc123",
                    CreatedAt = now.AddDays(-12),
                    UpdatedAt = now.AddDays(-6)
                },
                new Microincrement
                {
                    Id = Guid.NewGuid(),
                    IterationId = firstIterationId,
                    ArtifactId = artifacts[1].Id,
                    Title = "Dashboard Principal",
                    Description = "Pantalla principal con KPIs y gráficos",
                    Type = "funcional",
                    Date = now.AddDays(-5),
                    Author = "Luis Torres",
                    EvidenceFilePath = "/uploads/dashboard-screenshot.png",
                    CreatedAt = now.AddDays(-7),
                    UpdatedAt = now
                }
            };
            await _context.Microincrements.AddRangeAsync(microincrements);
            await _context.SaveChangesAsync();
            _logger.LogInformation("✅ Microincrementos creados");

            // 32. Crear Final Builds
            var finalBuilds = new[]
            {
                new FinalBuild
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projects[0].Id,
                    Version = "1.0.0-alpha",
                    BuildNumber = "20241209.1",
                    BuildTag = "v1.0.0-alpha",
                    CommitHash = "a1b2c3d4e5f6",
                    BuildDate = now.AddDays(-3),
                    BuiltBy = "Carlos Ramírez",
                    BuildEnvironment = "GitHub Actions",
                    BuildConfiguration = "Release",
                    BinaryArtifacts = "[{\"name\":\"sge-1.0.0-alpha.zip\",\"type\":\"PACKAGE\",\"filePath\":\"/builds/sge/1.0.0-alpha/sge-1.0.0-alpha.zip\",\"size\":25600000,\"checksum\":\"abc123def456\",\"checksumType\":\"SHA256\"}]",
                    MainDownloadUrl = "https://releases.openuptool.com/sge/v1.0.0-alpha/sge-1.0.0-alpha.zip",
                    ReleaseNotesUrl = "https://releases.openuptool.com/sge/v1.0.0-alpha/notes.html",
                    TargetPlatform = "Web",
                    IsStable = false,
                    TestsPassed = 127,
                    TestsTotal = 127,
                    CodeCoverage = 82.5,
                    QualityGateStatus = "Passed",
                    CreatedAt = now.AddDays(-3),
                    UpdatedAt = now.AddDays(-3)
                }
            };
            await _context.FinalBuilds.AddRangeAsync(finalBuilds);
            await _context.SaveChangesAsync();
            _logger.LogInformation("✅ Final Builds creados");

            // 33. Crear Estados de Workflow Templates
            var workflowStateTemplates = new[]
            {
                // Estados para "Document Review Workflow"
                new WorkflowStateTemplate
                {
                    Id = Guid.NewGuid(),
                    WorkflowTemplateId = workflowTemplates[0].Id,
                    Name = "Borrador",
                    Description = "Documento en elaboración",
                    OrderIndex = 1,
                    Color = "#9E9E9E",
                    IsInitialState = true,
                    IsFinalState = false,
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new WorkflowStateTemplate
                {
                    Id = Guid.NewGuid(),
                    WorkflowTemplateId = workflowTemplates[0].Id,
                    Name = "En Revisión",
                    Description = "Documento siendo revisado",
                    OrderIndex = 2,
                    Color = "#FF9800",
                    IsInitialState = false,
                    IsFinalState = false,
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new WorkflowStateTemplate
                {
                    Id = Guid.NewGuid(),
                    WorkflowTemplateId = workflowTemplates[0].Id,
                    Name = "Aprobado",
                    Description = "Documento aprobado",
                    OrderIndex = 3,
                    Color = "#4CAF50",
                    IsInitialState = false,
                    IsFinalState = true,
                    CreatedAt = now,
                    UpdatedAt = now
                },
                // Estados para "Code Development Workflow"
                new WorkflowStateTemplate
                {
                    Id = Guid.NewGuid(),
                    WorkflowTemplateId = workflowTemplates[1].Id,
                    Name = "En Desarrollo",
                    Description = "Código en desarrollo",
                    OrderIndex = 1,
                    Color = "#2196F3",
                    IsInitialState = true,
                    IsFinalState = false,
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new WorkflowStateTemplate
                {
                    Id = Guid.NewGuid(),
                    WorkflowTemplateId = workflowTemplates[1].Id,
                    Name = "Code Review",
                    Description = "Revisión de código",
                    OrderIndex = 2,
                    Color = "#9C27B0",
                    IsInitialState = false,
                    IsFinalState = false,
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new WorkflowStateTemplate
                {
                    Id = Guid.NewGuid(),
                    WorkflowTemplateId = workflowTemplates[1].Id,
                    Name = "Testing",
                    Description = "En pruebas",
                    OrderIndex = 3,
                    Color = "#FF5722",
                    IsInitialState = false,
                    IsFinalState = false,
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new WorkflowStateTemplate
                {
                    Id = Guid.NewGuid(),
                    WorkflowTemplateId = workflowTemplates[1].Id,
                    Name = "Completado",
                    Description = "Código completado y desplegado",
                    OrderIndex = 4,
                    Color = "#4CAF50",
                    IsInitialState = false,
                    IsFinalState = true,
                    CreatedAt = now,
                    UpdatedAt = now
                }
            };
            await _context.WorkflowStateTemplates.AddRangeAsync(workflowStateTemplates);
            await _context.SaveChangesAsync();
            _logger.LogInformation("✅ Estados de workflow templates creados");

            // 34. Crear Campos Personalizados para Artifact Types
            var customFieldDefinitions = new[]
            {
                // Campos para Vision Document Template
                new CustomFieldDefinition
                {
                    Id = Guid.NewGuid(),
                    ConfigurationId = globalConfiguration.Id,
                    ArtifactTypeTemplateId = artifactTypeTemplates[0].Id,
                    FieldName = "budget",
                    DisplayName = "Presupuesto Estimado",
                    FieldType = "NUMBER",
                    IsRequired = true,
                    DefaultValue = "0",
                    Options = null,
                    ValidationRules = "{\"min\":0,\"max\":10000000}",
                    OrderIndex = 1,
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new CustomFieldDefinition
                {
                    Id = Guid.NewGuid(),
                    ConfigurationId = globalConfiguration.Id,
                    ArtifactTypeTemplateId = artifactTypeTemplates[0].Id,
                    FieldName = "stakeholders",
                    DisplayName = "Stakeholders Principales",
                    FieldType = "TEXT",
                    IsRequired = true,
                    DefaultValue = null,
                    Options = null,
                    ValidationRules = "{\"maxLength\":500}",
                    OrderIndex = 2,
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new CustomFieldDefinition
                {
                    Id = Guid.NewGuid(),
                    ConfigurationId = globalConfiguration.Id,
                    ArtifactTypeTemplateId = artifactTypeTemplates[0].Id,
                    FieldName = "priority",
                    DisplayName = "Prioridad",
                    FieldType = "SELECT",
                    IsRequired = true,
                    DefaultValue = "MEDIUM",
                    Options = "[\"HIGH\",\"MEDIUM\",\"LOW\"]",
                    ValidationRules = null,
                    OrderIndex = 3,
                    CreatedAt = now,
                    UpdatedAt = now
                },
                // Campos para Architecture Notebook
                new CustomFieldDefinition
                {
                    Id = Guid.NewGuid(),
                    ConfigurationId = globalConfiguration.Id,
                    ArtifactTypeTemplateId = artifactTypeTemplates[1].Id,
                    FieldName = "architecture_pattern",
                    DisplayName = "Patrón Arquitectónico",
                    FieldType = "SELECT",
                    IsRequired = true,
                    DefaultValue = "LAYERED",
                    Options = "[\"LAYERED\",\"MICROSERVICES\",\"MVC\",\"HEXAGONAL\",\"EVENT_DRIVEN\"]",
                    ValidationRules = null,
                    OrderIndex = 1,
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new CustomFieldDefinition
                {
                    Id = Guid.NewGuid(),
                    ConfigurationId = globalConfiguration.Id,
                    ArtifactTypeTemplateId = artifactTypeTemplates[1].Id,
                    FieldName = "technologies",
                    DisplayName = "Tecnologías Utilizadas",
                    FieldType = "MULTISELECT",
                    IsRequired = false,
                    DefaultValue = null,
                    Options = "[\"React\",\"Angular\",\"Vue\",\".NET\",\"Java\",\"Python\",\"Node.js\",\"PostgreSQL\",\"MongoDB\",\"Redis\"]",
                    ValidationRules = null,
                    OrderIndex = 2,
                    CreatedAt = now,
                    UpdatedAt = now
                },
                // Campos para Test Case Template
                new CustomFieldDefinition
                {
                    Id = Guid.NewGuid(),
                    ConfigurationId = globalConfiguration.Id,
                    ArtifactTypeTemplateId = artifactTypeTemplates[2].Id,
                    FieldName = "test_priority",
                    DisplayName = "Prioridad de Prueba",
                    FieldType = "NUMBER",
                    IsRequired = true,
                    DefaultValue = "3",
                    Options = null,
                    ValidationRules = "{\"min\":1,\"max\":5}",
                    OrderIndex = 1,
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new CustomFieldDefinition
                {
                    Id = Guid.NewGuid(),
                    ConfigurationId = globalConfiguration.Id,
                    ArtifactTypeTemplateId = artifactTypeTemplates[2].Id,
                    FieldName = "automated",
                    DisplayName = "Test Automatizado",
                    FieldType = "BOOLEAN",
                    IsRequired = false,
                    DefaultValue = "false",
                    Options = null,
                    ValidationRules = null,
                    OrderIndex = 2,
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new CustomFieldDefinition
                {
                    Id = Guid.NewGuid(),
                    ConfigurationId = globalConfiguration.Id,
                    ArtifactTypeTemplateId = artifactTypeTemplates[2].Id,
                    FieldName = "test_type",
                    DisplayName = "Tipo de Prueba",
                    FieldType = "SELECT",
                    IsRequired = true,
                    DefaultValue = "FUNCTIONAL",
                    Options = "[\"FUNCTIONAL\",\"INTEGRATION\",\"UNIT\",\"E2E\",\"PERFORMANCE\",\"SECURITY\"]",
                    ValidationRules = null,
                    OrderIndex = 3,
                    CreatedAt = now,
                    UpdatedAt = now
                }
            };
            await _context.CustomFieldDefinitions.AddRangeAsync(customFieldDefinitions);
            await _context.SaveChangesAsync();
            _logger.LogInformation("✅ Campos personalizados creados");

            // 35. Crear más artefactos de CONSTRUCTION
            var constructionPhase = phases.FirstOrDefault(p => p.PhaseCode == "CONSTRUCTION");
            if (constructionPhase != null)
            {
                var constructionArtifactTypes = await _context.ArtifactTypes
                    .Where(at => at.Phase == "CONSTRUCTION")
                    .ToListAsync();

                var constructionArtifacts = new List<Artifact>();
                
                foreach (var artifactType in constructionArtifactTypes.Take(5))
                {
                    constructionArtifacts.Add(new Artifact
                    {
                        Id = Guid.NewGuid(),
                        ProjectId = projects[0].Id,
                        PhaseId = "CONSTRUCTION",
                        ArtifactTypeId = artifactType.Id,
                        Title = $"{artifactType.Name} - SGE",
                        Description = $"Implementación de {artifactType.Name.ToLower()} para el proyecto SGE",
                        ContentText = $"# {artifactType.Name}\n\nContenido del artefacto para la fase de construcción.\n\n## Detalles de Implementación\n\nEste documento detalla la implementación realizada durante la fase de construcción.",
                        IsMandatory = artifactType.IsMandatory,
                        Status = "En Progreso",
                        Author = $"{users[4].FirstName} {users[4].LastName}",
                        CreatedAt = now.AddDays(-15),
                        UpdatedAt = now.AddDays(-2)
                    });
                }

                await _context.Artifacts.AddRangeAsync(constructionArtifacts);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"✅ {constructionArtifacts.Count} artefactos de construcción creados");
            }

            // 36. Crear Audit Logs
            var auditLogs = new[]
            {
                // Logs de proyectos
                new AuditLog
                {
                    Id = Guid.NewGuid(),
                    UserId = users[0].Id,
                    Action = "PROJECT_CREATED",
                    EntityType = "Project",
                    EntityId = projects[0].Id,
                    Details = "{\"projectName\":\"Sistema de Gestión Educativa\",\"phase\":\"INCEPTION\"}",
                    CreatedAt = now.AddDays(-30)
                },
                new AuditLog
                {
                    Id = Guid.NewGuid(),
                    UserId = users[1].Id,
                    Action = "PROJECT_UPDATED",
                    EntityType = "Project",
                    EntityId = projects[0].Id,
                    Details = "{\"field\":\"description\",\"oldValue\":\"Sistema inicial\",\"newValue\":\"Sistema completo de gestión educativa\"}",
                    CreatedAt = now.AddDays(-28)
                },
                new AuditLog
                {
                    Id = Guid.NewGuid(),
                    UserId = users[0].Id,
                    Action = "PROJECT_CREATED",
                    EntityType = "Project",
                    EntityId = projects[1].Id,
                    Details = "{\"projectName\":\"App Móvil Bancaria\",\"phase\":\"INCEPTION\"}",
                    CreatedAt = now.AddDays(-25)
                },
                // Logs de artefactos
                new AuditLog
                {
                    Id = Guid.NewGuid(),
                    UserId = users[4].Id,
                    Action = "ARTIFACT_CREATED",
                    EntityType = "Artifact",
                    EntityId = artifacts[0].Id,
                    Details = "{\"artifactName\":\"Documento de Visión\",\"phase\":\"INCEPTION\"}",
                    CreatedAt = now.AddDays(-22)
                },
                new AuditLog
                {
                    Id = Guid.NewGuid(),
                    UserId = users[3].Id,
                    Action = "ARTIFACT_UPDATED",
                    EntityType = "Artifact",
                    EntityId = artifacts[0].Id,
                    Details = "{\"field\":\"content\",\"version\":2}",
                    CreatedAt = now.AddDays(-20)
                },
                new AuditLog
                {
                    Id = Guid.NewGuid(),
                    UserId = users[5].Id,
                    Action = "ARTIFACT_VERSION_CREATED",
                    EntityType = "Artifact",
                    EntityId = artifacts[0].Id,
                    Details = "{\"version\":3,\"changes\":\"Actualización de requisitos\"}",
                    CreatedAt = now.AddDays(-18)
                },
                // Logs de usuarios
                new AuditLog
                {
                    Id = Guid.NewGuid(),
                    UserId = users[0].Id,
                    Action = "USER_ROLE_ASSIGNED",
                    EntityType = "User",
                    EntityId = users[4].Id,
                    Details = "{\"role\":\"Developer\",\"project\":\"SGE\"}",
                    CreatedAt = now.AddDays(-27)
                },
                new AuditLog
                {
                    Id = Guid.NewGuid(),
                    UserId = users[1].Id,
                    Action = "USER_INVITED",
                    EntityType = "User",
                    EntityId = users[6].Id,
                    Details = "{\"email\":\"laura.fernandez@openuptool.com\",\"role\":\"Developer\",\"project\":\"SGE\"}",
                    CreatedAt = now.AddDays(-24)
                },
                // Logs de iteraciones
                new AuditLog
                {
                    Id = Guid.NewGuid(),
                    UserId = users[1].Id,
                    Action = "ITERATION_CREATED",
                    EntityType = "Iteration",
                    EntityId = null,
                    Details = "{\"iterationName\":\"Sprint 1\",\"duration\":14}",
                    CreatedAt = now.AddDays(-21)
                },
                new AuditLog
                {
                    Id = Guid.NewGuid(),
                    UserId = users[1].Id,
                    Action = "ITERATION_COMPLETED",
                    EntityType = "Iteration",
                    EntityId = null,
                    Details = "{\"iterationName\":\"Sprint 1\",\"completedStories\":5}",
                    CreatedAt = now.AddDays(-7)
                },
                // Logs de workflows
                new AuditLog
                {
                    Id = Guid.NewGuid(),
                    UserId = users[3].Id,
                    Action = "WORKFLOW_ASSIGNED",
                    EntityType = "Artifact",
                    EntityId = artifacts[0].Id,
                    Details = "{\"workflowName\":\"Flujo de Revisión de Documentos\"}",
                    CreatedAt = now.AddDays(-19)
                },
                new AuditLog
                {
                    Id = Guid.NewGuid(),
                    UserId = users[4].Id,
                    Action = "ARTIFACT_STATE_CHANGED",
                    EntityType = "Artifact",
                    EntityId = artifacts[0].Id,
                    Details = "{\"fromState\":\"Borrador\",\"toState\":\"En Revisión\",\"workflow\":\"Flujo de Revisión de Documentos\"}",
                    CreatedAt = now.AddDays(-17)
                },
                new AuditLog
                {
                    Id = Guid.NewGuid(),
                    UserId = users[1].Id,
                    Action = "ARTIFACT_STATE_CHANGED",
                    EntityType = "Artifact",
                    EntityId = artifacts[0].Id,
                    Details = "{\"fromState\":\"En Revisión\",\"toState\":\"Aprobado\",\"workflow\":\"Flujo de Revisión de Documentos\"}",
                    CreatedAt = now.AddDays(-15)
                },
                // Logs de defectos
                new AuditLog
                {
                    Id = Guid.NewGuid(),
                    UserId = users[7].Id,
                    Action = "DEFECT_CREATED",
                    EntityType = "Defect",
                    EntityId = null,
                    Details = "{\"severity\":\"CRITICAL\",\"title\":\"Error en autenticación\"}",
                    CreatedAt = now.AddDays(-10)
                },
                new AuditLog
                {
                    Id = Guid.NewGuid(),
                    UserId = users[4].Id,
                    Action = "DEFECT_RESOLVED",
                    EntityType = "Defect",
                    EntityId = null,
                    Details = "{\"defectId\":\"DEF-001\",\"resolution\":\"Corregido en commit abc123\"}",
                    CreatedAt = now.AddDays(-8)
                },
                // Logs de builds
                new AuditLog
                {
                    Id = Guid.NewGuid(),
                    UserId = users[3].Id,
                    Action = "BUILD_CREATED",
                    EntityType = "FinalBuild",
                    EntityId = finalBuilds[0].Id,
                    Details = "{\"version\":\"1.0.0-alpha\",\"buildNumber\":\"20241209.1\",\"status\":\"Success\"}",
                    CreatedAt = now.AddDays(-3)
                },
                // Logs de configuración
                new AuditLog
                {
                    Id = Guid.NewGuid(),
                    UserId = users[0].Id,
                    Action = "CONFIGURATION_APPLIED",
                    EntityType = "ProjectConfiguration",
                    EntityId = projectConfigurations[0].Id,
                    Details = "{\"configurationName\":\"OpenUP Standard Configuration\",\"version\":1}",
                    CreatedAt = now.AddDays(-26)
                },
                new AuditLog
                {
                    Id = Guid.NewGuid(),
                    UserId = users[1].Id,
                    Action = "WORKFLOW_CREATED",
                    EntityType = "Workflow",
                    EntityId = workflows[0].Id,
                    Details = "{\"workflowName\":\"Flujo de Revisión de Documentos\",\"statesCount\":3}",
                    CreatedAt = now.AddDays(-23)
                },
                new AuditLog
                {
                    Id = Guid.NewGuid(),
                    UserId = users[1].Id,
                    Action = "WORKFLOW_CREATED",
                    EntityType = "Workflow",
                    EntityId = workflows[1].Id,
                    Details = "{\"workflowName\":\"Flujo de Desarrollo de Código\",\"statesCount\":4}",
                    CreatedAt = now.AddDays(-23)
                },
                // Logs de milestones
                new AuditLog
                {
                    Id = Guid.NewGuid(),
                    UserId = users[1].Id,
                    Action = "MILESTONE_COMPLETED",
                    EntityType = "Milestone",
                    EntityId = milestones[0].Id,
                    Details = "{\"milestoneName\":\"Entrega Alpha\",\"completionDate\":\"2024-12-06\"}",
                    CreatedAt = now.AddDays(-3)
                }
            };
            await _context.AuditLogs.AddRangeAsync(auditLogs);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"✅ {auditLogs.Length} audit logs creados");

            _logger.LogInformation("🎉 Siembra de datos completada exitosamente");

            return Ok(new
            {
                message = "Datos de prueba sembrados exitosamente",
                resumen = new
                {
                    roles = roles.Length,
                    usuarios = 10,
                    proyectos = projects.Length,
                    globalConfigurations = 1,
                    artifactTypeTemplates = artifactTypeTemplates.Length,
                    phaseTemplates = phaseTemplates.Length,
                    roleTemplates = roleTemplates.Length,
                    workflowTemplates = workflowTemplates.Length,
                    workflowStateTemplates = workflowStateTemplates.Length,
                    customFieldDefinitions = customFieldDefinitions.Length,
                    projectConfigurations = projectConfigurations.Length,
                    fases = phases.Count,
                    planesProyecto = projectPlans.Length,
                    milestones = milestones.Length,
                    tiposArtefactos = 30,
                    artefactos = artifacts.Length + 5, // includes construction artifacts
                    versionesArtefactos = artifactVersions.Length,
                    iteraciones = 2,
                    tareasIteracion = 4,
                    progresoIteracion = 3,
                    userStories = 3,
                    alcanceIteracion = 2,
                    microincrements = microincrements.Length,
                    finalBuilds = finalBuilds.Length,
                    asignacionesProyecto = 11,
                    ejecucionesPrueba = 4,
                    defectos = 5,
                    notificaciones = notifications.Length,
                    workflows = workflows.Length,
                    estadosWorkflow = workflowStates.Length,
                    responsablesEstados = stateResponsibles.Length,
                    historialEstadosArtefacto = 2,
                    permisosWorkflow = workflowPermissions.Length,
                    auditLogs = auditLogs.Length
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
                // Tablas de configuración y templates
                globalConfigurations = await _context.GlobalConfigurations.CountAsync(),
                artifactTypeTemplates = await _context.ArtifactTypeTemplates.CountAsync(),
                phaseTemplates = await _context.PhaseTemplates.CountAsync(),
                roleTemplates = await _context.RoleTemplates.CountAsync(),
                workflowTemplates = await _context.WorkflowTemplates.CountAsync(),
                
                // Usuarios y roles
                roles = await _context.Roles.CountAsync(),
                usuarios = await _context.Users.CountAsync(),
                
                // Proyectos
                proyectos = await _context.Projects.CountAsync(),
                projectConfigurations = await _context.ProjectConfigurations.CountAsync(),
                planesProyecto = await _context.ProjectPlans.CountAsync(),
                projectClosures = await _context.ProjectClosures.CountAsync(),
                projectUserRoles = await _context.ProjectUserRoles.CountAsync(),
                projectInvitations = await _context.ProjectInvitations.CountAsync(),
                
                // Fases e iteraciones
                fases = await _context.Phases.CountAsync(),
                iteraciones = await _context.Iterations.CountAsync(),
                iterationProgress = await _context.IterationProgresses.CountAsync(),
                tareasIteracion = await _context.IterationTasks.CountAsync(),
                alcanceIteracion = await _context.IterationScopes.CountAsync(),
                milestones = await _context.Milestones.CountAsync(),
                
                // Workflows
                workflows = await _context.Workflows.CountAsync(),
                workflowStates = await _context.WorkflowStates.CountAsync(),
                workflowPermissions = await _context.WorkflowPermissions.CountAsync(),
                workflowStateResponsibles = await _context.WorkflowStateResponsibles.CountAsync(),
                
                // Artefactos
                tiposArtefactos = await _context.ArtifactTypes.CountAsync(),
                artefactos = await _context.Artifacts.CountAsync(),
                versionesArtefactos = await _context.ArtifactVersions.CountAsync(),
                artifactCustomFieldValues = await _context.ArtifactCustomFieldValues.CountAsync(),
                artifactMovementHistories = await _context.ArtifactMovementHistories.CountAsync(),
                artifactStateHistory = await _context.ArtifactStateHistories.CountAsync(),
                
                // User stories y construcción
                userStories = await _context.UserStories.CountAsync(),
                microincrements = await _context.Microincrements.CountAsync(),
                finalBuilds = await _context.FinalBuilds.CountAsync(),
                
                // Testing y defectos
                ejecucionesPrueba = await _context.TestExecutions.CountAsync(),
                defectos = await _context.Defects.CountAsync(),
                
                // Notificaciones y auditoría
                notificaciones = await _context.Notifications.CountAsync(),
                notificationPreferences = await _context.NotificationPreferences.CountAsync(),
                auditLogs = await _context.AuditLogs.CountAsync()
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
