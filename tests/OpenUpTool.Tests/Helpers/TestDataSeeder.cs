using OpenUpTool.Core.Entities;
using OpenUpTool.Infrastructure.Data;

namespace OpenUpTool.Tests.Helpers;

/// <summary>
/// Clase helper para sembrar datos de prueba en la base de datos
/// </summary>
public static class TestDataSeeder
{
    public static Guid AdminUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static Guid ManagerUserId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static Guid DeveloperUserId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    
    public static Guid AdminRoleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static Guid ManagerRoleId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static Guid DeveloperRoleId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    
    public static Guid TestProjectId = Guid.Parse("44444444-4444-4444-4444-444444444444");

    /// <summary>
    /// Siembra datos básicos de prueba
    /// </summary>
    public static void SeedTestData(OpenUpToolDbContext context)
    {
        // Limpiar datos existentes
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        // Roles
        var roles = new[]
        {
            new Role { Id = AdminRoleId, Name = "Admin", Description = "Administrador del sistema", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Role { Id = ManagerRoleId, Name = "Manager", Description = "Gerente de proyecto", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Role { Id = DeveloperRoleId, Name = "Developer", Description = "Desarrollador", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };
        context.Roles.AddRange(roles);

        // Usuarios de prueba (password: Test123!)
        var users = new[]
        {
            new User 
            { 
                Id = AdminUserId, 
                Email = "admin@test.com", 
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test123!"),
                FirstName = "Admin",
                LastName = "User",
                RoleId = AdminRoleId,
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow 
            },
            new User 
            { 
                Id = ManagerUserId, 
                Email = "manager@test.com", 
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test123!"),
                FirstName = "Manager",
                LastName = "User",
                RoleId = ManagerRoleId,
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow 
            },
            new User 
            { 
                Id = DeveloperUserId, 
                Email = "developer@test.com", 
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test123!"),
                FirstName = "Developer",
                LastName = "User",
                RoleId = DeveloperRoleId,
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow 
            }
        };
        context.Users.AddRange(users);

        // Proyecto de prueba
        var project = new Project
        {
            Id = TestProjectId,
            Name = "Test Project",
            Identifier = "TEST",
            StartDate = DateTime.UtcNow,
            Status = "Creado",
            Owner = "Test Owner",
            Description = "Test project for unit tests",
            Tags = new List<string> { "test", "unit-test" },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.Projects.Add(project);

        // Fases del proyecto
        var phases = new[]
        {
            new Phase { Id = Guid.NewGuid(), ProjectId = TestProjectId, PhaseCode = "INCEPTION", Name = "Incepción", Status = "PENDING", OrderIndex = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Phase { Id = Guid.NewGuid(), ProjectId = TestProjectId, PhaseCode = "ELABORATION", Name = "Elaboración", Status = "PENDING", OrderIndex = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Phase { Id = Guid.NewGuid(), ProjectId = TestProjectId, PhaseCode = "CONSTRUCTION", Name = "Construcción", Status = "PENDING", OrderIndex = 3, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Phase { Id = Guid.NewGuid(), ProjectId = TestProjectId, PhaseCode = "TRANSITION", Name = "Transición", Status = "PENDING", OrderIndex = 4, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };
        context.Phases.AddRange(phases);

        // Asignar manager al proyecto
        var projectUserRole = new ProjectUserRole
        {
            Id = Guid.NewGuid(),
            ProjectId = TestProjectId,
            UserId = ManagerUserId,
            RoleId = ManagerRoleId,
            InvitedBy = AdminUserId,
            InvitedAt = DateTime.UtcNow,
            AcceptedAt = DateTime.UtcNow,
            Status = "active",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.ProjectUserRoles.Add(projectUserRole);

        context.SaveChanges();
    }
}
