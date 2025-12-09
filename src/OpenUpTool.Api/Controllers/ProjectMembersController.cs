using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenUpTool.Core.DTOs;
using OpenUpTool.Core.Entities;
using OpenUpTool.Core.Interfaces;
using OpenUpTool.Infrastructure.Data;
using System.Security.Claims;

namespace OpenUpTool.Api.Controllers;

[ApiController]
[Route("api/projects/{projectId}/members")]
[Authorize]
public class ProjectMembersController : ControllerBase
{
    private readonly OpenUpToolDbContext _context;
    private readonly INotificationService _notificationService;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<ProjectMembersController> _logger;

    public ProjectMembersController(
        OpenUpToolDbContext context,
        INotificationService notificationService,
        IAuditLogService auditLogService,
        ILogger<ProjectMembersController> logger)
    {
        _context = context;
        _notificationService = notificationService;
        _auditLogService = auditLogService;
        _logger = logger;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    /// <summary>
    /// Obtiene todos los roles disponibles para asignar
    /// </summary>
    [HttpGet("/api/roles")]
    public async Task<ActionResult<IEnumerable<RoleDto>>> GetAllRoles()
    {
        try
        {
            var roles = await _context.Roles
                .Select(r => new RoleDto(r.Id, r.Name, r.Description))
                .ToListAsync();

            return Ok(roles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener roles");
            return StatusCode(500, new { message = "Error al obtener roles" });
        }
    }

    /// <summary>
    /// Obtiene todos los miembros de un proyecto
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProjectMemberDto>>> GetProjectMembers(Guid projectId)
    {
        try
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null)
                return NotFound(new { message = "Proyecto no encontrado" });

            var purList = await _context.ProjectUserRoles
                .Include(pur => pur.User)
                .Include(pur => pur.Role)
                .Include(pur => pur.Inviter)
                .Where(pur => pur.ProjectId == projectId)
                .ToListAsync();

            var members = purList
                .Select(pur => new ProjectMemberDto
                {
                    Id = pur.Id,
                    UserId = pur.UserId,
                    UserEmail = pur.User?.Email,
                    UserName = string.IsNullOrWhiteSpace(pur.User?.FirstName) && string.IsNullOrWhiteSpace(pur.User?.LastName)
                        ? pur.User?.Email
                        : $"{pur.User?.FirstName} {pur.User?.LastName}".Trim(),
                    RoleId = pur.RoleId,
                    RoleName = pur.Role?.Name,
                    Status = pur.Status,
                    InvitedBy = pur.Inviter != null ? $"{pur.Inviter.FirstName} {pur.Inviter.LastName}".Trim() : null,
                    InvitedAt = pur.InvitedAt,
                    AcceptedAt = pur.AcceptedAt
                })
                .OrderBy(m => m.UserName)
                .ToList();

            if (!members.Any())
            {
                _logger.LogWarning("No se encontraron miembros para el proyecto {ProjectId}", projectId);
                return Ok(new List<ProjectMemberDto>());
            }

            return Ok(members);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener miembros del proyecto {ProjectId}", projectId);
            return StatusCode(500, new { message = "Error al obtener miembros del proyecto" });
        }
    }

    /// <summary>
    /// Agrega un usuario existente al proyecto con un rol específico
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ProjectMemberDto>> AddMember(Guid projectId, [FromBody] AddMemberDto dto)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == Guid.Empty)
                return Unauthorized();

            // Verificar que el proyecto existe
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null)
                return NotFound(new { message = "Proyecto no encontrado" });

            // Verificar que el usuario existe
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null)
                return NotFound(new { message = "Usuario no encontrado" });

            // Verificar que el rol existe
            var role = await _context.Roles.FindAsync(dto.RoleId);
            if (role == null)
                return BadRequest(new { message = "Rol no válido" });

            // Verificar que el usuario no es ya miembro del proyecto
            var existingMember = await _context.ProjectUserRoles
                .FirstOrDefaultAsync(pur => pur.ProjectId == projectId && pur.UserId == user.Id);
            if (existingMember != null)
                return Conflict(new { message = "El usuario ya es miembro del proyecto" });

            // Crear la relación
            var projectUserRole = new ProjectUserRole
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                UserId = user.Id,
                RoleId = dto.RoleId,
                InvitedBy = currentUserId,
                InvitedAt = DateTime.UtcNow,
                AcceptedAt = DateTime.UtcNow, // Auto-aceptado cuando se agrega directamente
                Status = "active",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.ProjectUserRoles.Add(projectUserRole);
            await _context.SaveChangesAsync();

            // Registrar en auditoría
            await _auditLogService.LogActionAsync(
                currentUserId,
                "UserAddedToProject",
                "ProjectUserRole",
                projectUserRole.Id,
                $"Usuario {user.Email} agregado al proyecto {project.Name} con rol {role.Name}"
            );

            // Crear notificación para el usuario agregado
            await _notificationService.CreateNotificationAsync(
                user.Id,
                "project_member_added",
                "Agregado a proyecto",
                $"Has sido agregado al proyecto '{project.Name}' con el rol de {role.Name}",
                "Project",
                projectId
            );

            return CreatedAtAction(nameof(GetProjectMembers), new { projectId }, new ProjectMemberDto
            {
                Id = projectUserRole.Id,
                UserId = user.Id,
                UserEmail = user.Email,
                UserName = $"{user.FirstName} {user.LastName}".Trim(),
                RoleId = role.Id,
                RoleName = role.Name,
                Status = "active",
                InvitedAt = projectUserRole.InvitedAt,
                AcceptedAt = projectUserRole.AcceptedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al agregar miembro al proyecto {ProjectId}", projectId);
            return StatusCode(500, new { message = "Error al agregar miembro" });
        }
    }

    /// <summary>
    /// Cambia el rol de un miembro del proyecto
    /// </summary>
    [HttpPut("{memberId}/role")]
    public async Task<ActionResult<ProjectMemberDto>> UpdateMemberRole(Guid projectId, Guid memberId, [FromBody] UpdateMemberRoleDto dto)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == Guid.Empty)
                return Unauthorized();

            var member = await _context.ProjectUserRoles
                .Include(pur => pur.User)
                .Include(pur => pur.Role)
                .FirstOrDefaultAsync(pur => pur.Id == memberId && pur.ProjectId == projectId);

            if (member == null)
                return NotFound(new { message = "Miembro no encontrado" });

            var newRole = await _context.Roles.FindAsync(dto.RoleId);
            if (newRole == null)
                return BadRequest(new { message = "Rol no válido" });

            if (member.RoleId == dto.RoleId)
            {
                _logger.LogInformation("El rol del miembro ya es {RoleName}", newRole.Name);
                return BadRequest(new { message = "El miembro ya tiene este rol" });
            }

            var oldRoleName = member.Role.Name;
            member.RoleId = dto.RoleId;
            member.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Recargar el rol
            await _context.Entry(member).Reference(m => m.Role).LoadAsync();

            // Registrar en auditoría
            await _auditLogService.LogActionAsync(
                currentUserId,
                "UserRoleChanged",
                "ProjectUserRole",
                memberId,
                $"Rol de {member.User.Email} cambiado de {oldRoleName} a {newRole.Name}"
            );

            // Notificar al usuario
            var project = await _context.Projects.FindAsync(projectId);
            await _notificationService.CreateNotificationAsync(
                member.UserId,
                "role_changed",
                "Rol actualizado",
                $"Tu rol en el proyecto '{project?.Name}' ha sido cambiado a {newRole.Name}",
                "Project",
                projectId
            );

            return Ok(new ProjectMemberDto
            {
                Id = member.Id,
                UserId = member.UserId,
                UserEmail = member.User.Email,
                UserName = $"{member.User.FirstName} {member.User.LastName}".Trim(),
                RoleId = member.RoleId,
                RoleName = member.Role.Name,
                Status = member.Status,
                InvitedAt = member.InvitedAt,
                AcceptedAt = member.AcceptedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cambiar el rol del miembro {MemberId} en el proyecto {ProjectId}", memberId, projectId);
            return StatusCode(500, new { message = "Error al cambiar el rol del miembro" });
        }
    }

    /// <summary>
    /// Remueve un miembro del proyecto
    /// </summary>
    [HttpDelete("{memberId}")]
    public async Task<ActionResult> RemoveMember(Guid projectId, Guid memberId)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == Guid.Empty)
                return Unauthorized();

            var member = await _context.ProjectUserRoles
                .Include(pur => pur.User)
                .Include(pur => pur.Role)
                .FirstOrDefaultAsync(pur => pur.Id == memberId && pur.ProjectId == projectId);

            if (member == null)
                return NotFound(new { message = "Miembro no encontrado" });

            var userEmail = member.User.Email;
            var userName = $"{member.User.FirstName} {member.User.LastName}".Trim();
            var userId = member.UserId;

            _context.ProjectUserRoles.Remove(member);
            await _context.SaveChangesAsync();

            // Registrar en auditoría
            var project = await _context.Projects.FindAsync(projectId);
            await _auditLogService.LogActionAsync(
                currentUserId,
                "UserRemovedFromProject",
                "ProjectUserRole",
                memberId,
                $"Usuario {userEmail} removido del proyecto {project?.Name}"
            );

            // Notificar al usuario removido
            await _notificationService.CreateNotificationAsync(
                userId,
                "removed_from_project",
                "Removido del proyecto",
                $"Has sido removido del proyecto '{project?.Name}'",
                "Project",
                projectId
            );

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al remover miembro {MemberId} del proyecto {ProjectId}", memberId, projectId);
            return StatusCode(500, new { message = "Error al remover miembro" });
        }
    }

    /// <summary>
    /// Obtiene la matriz de permisos por rol
    /// </summary>
    [HttpGet("permissions/matrix")]
    public ActionResult<PermissionMatrixDto> GetPermissionMatrix()
    {
        try
        {
            var matrix = new PermissionMatrixDto
            {
                Permissions = new List<PermissionDefinitionDto>
                {
                    new() { Action = "crear_proyecto", Description = "Crear nuevos proyectos", Admin = true, Manager = true, Developer = false, Viewer = false },
                    new() { Action = "editar_proyecto", Description = "Editar configuración del proyecto", Admin = true, Manager = true, Developer = false, Viewer = false },
                    new() { Action = "eliminar_proyecto", Description = "Eliminar/archivar proyectos", Admin = true, Manager = false, Developer = false, Viewer = false },
                    new() { Action = "crear_artefacto", Description = "Crear artefactos", Admin = true, Manager = true, Developer = true, Viewer = false },
                    new() { Action = "editar_artefacto", Description = "Editar artefactos", Admin = true, Manager = true, Developer = true, Viewer = false }
                }
            };

            return Ok(matrix);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener la matriz de permisos");
            return StatusCode(500, new { message = "Error al obtener la matriz de permisos" });
        }
    }
}
