using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenUpTool.Core.DTOs;
using OpenUpTool.Core.Interfaces;
using System.Security.Claims;

namespace OpenUpTool.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;
    private readonly IIterationProgressService _progressService;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<ProjectsController> _logger;

    public ProjectsController(
        IProjectService projectService,
        IIterationProgressService progressService,
        IAuditLogService auditLogService,
        ILogger<ProjectsController> logger)
    {
        _projectService = projectService;
        _progressService = progressService;
        _auditLogService = auditLogService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los proyectos del usuario autenticado
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProjectDto>>> GetAll()
    {
        try
        {
            var userIdClaim = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var projects = await _projectService.GetProjectsForUserAsync(userId);
            return Ok(projects);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener proyectos");
            return StatusCode(500, new { message = "Error al obtener proyectos" });
        }
    }

    /// <summary>
    /// Obtiene un proyecto por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ProjectDto>> GetById(Guid id)
    {
        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var project = await _projectService.GetProjectByIdAsync(id);
            if (project == null)
                return NotFound(new { message = "Proyecto no encontrado" });

            // Verificar si el usuario tiene acceso a este proyecto
            var hasAccess = await _projectService.HasUserAccessAsync(userId, id);
            if (!hasAccess)
                return Forbid();

            return Ok(project);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener proyecto {ProjectId}", id);
            return StatusCode(500, new { message = "Error al obtener proyecto" });
        }
    }

    /// <summary>
    /// Crea un nuevo proyecto
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ProjectDto>> Create([FromBody] CreateProjectDto dto)
    {
        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            _logger.LogInformation("Creando proyecto: {ProjectName}", dto.Name);
            var project = await _projectService.CreateProjectAsync(dto, userId);
            _logger.LogInformation("Proyecto creado exitosamente: {ProjectId}", project.Id);
            return CreatedAtAction(nameof(GetById), new { id = project.Id }, project);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear proyecto: {Message}", ex.Message);
            return StatusCode(500, new { message = "Error al crear proyecto", error = ex.Message, stackTrace = ex.StackTrace });
        }
    }

    /// <summary>
    /// Actualiza un proyecto existente
    /// </summary>
    [HttpPatch("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ProjectDto>> Update(Guid id, [FromBody] UpdateProjectDto dto)
    {
        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            // Verificar si el usuario tiene acceso a este proyecto
            var hasAccess = await _projectService.HasUserAccessAsync(userId, id);
            if (!hasAccess)
                return Forbid();

            var project = await _projectService.UpdateProjectAsync(id, dto);
            if (project == null)
                return NotFound(new { message = "Proyecto no encontrado" });

            return Ok(project);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar proyecto {ProjectId}", id);
            return StatusCode(500, new { message = "Error al actualizar proyecto" });
        }
    }

    /// <summary>
    /// Elimina un proyecto
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            // Verificar si el usuario tiene acceso a este proyecto
            var hasAccess = await _projectService.HasUserAccessAsync(userId, id);
            if (!hasAccess)
                return Forbid();

            await _projectService.DeleteProjectAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar proyecto {ProjectId}", id);
            return StatusCode(500, new { message = "Error al eliminar proyecto" });
        }
    }

    /// <summary>
    /// Obtiene el dashboard del proyecto con el avance total y por fase
    /// </summary>
    [HttpGet("{id}/dashboard")]
    public async Task<ActionResult<ProjectDashboardDto>> GetDashboard(Guid id)
    {
        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            // Verificar si el usuario tiene acceso a este proyecto
            var hasAccess = await _projectService.HasUserAccessAsync(userId, id);
            if (!hasAccess)
                return Forbid();

            var dashboard = await _progressService.GetProjectDashboardAsync(id);
            if (dashboard == null)
                return NotFound(new { message = "Proyecto no encontrado" });

            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener dashboard del proyecto {ProjectId}", id);
            return StatusCode(500, new { message = "Error al obtener dashboard" });
        }
    }

    /// <summary>
    /// Obtiene los usuarios asignados a un proyecto
    /// </summary>
    [HttpGet("{id}/users")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetProjectUsers(Guid id)
    {
        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            // Verificar si el usuario tiene acceso a este proyecto
            var hasAccess = await _projectService.HasUserAccessAsync(userId, id);
            if (!hasAccess)
                return Forbid();

            var users = await _projectService.GetProjectUsersAsync(id);
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuarios del proyecto {ProjectId}", id);
            return StatusCode(500, new { message = "Error al obtener usuarios del proyecto" });
        }
    }

    /// <summary>
    /// Archiva un proyecto (conserva todo el historial)
    /// </summary>
    [HttpPost("{id}/archive")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ProjectDto>> ArchiveProject(Guid id)
    {
        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            // Verificar si el usuario tiene acceso a este proyecto
            var hasAccess = await _projectService.HasUserAccessAsync(userId, id);
            if (!hasAccess)
                return Forbid();

            var project = await _projectService.ArchiveProjectAsync(id, userId);
            if (project == null)
                return NotFound(new { message = "Proyecto no encontrado" });

            // Registrar en auditoría
            await _auditLogService.LogActionAsync(
                userId,
                "ProjectArchived",
                "Project",
                id,
                $"Proyecto '{project.Name}' archivado"
            );

            return Ok(project);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al archivar proyecto {ProjectId}", id);
            return StatusCode(500, new { message = "Error al archivar proyecto" });
        }
    }

    /// <summary>
    /// Desarchivar un proyecto
    /// </summary>
    [HttpPost("{id}/unarchive")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ProjectDto>> UnarchiveProject(Guid id)
    {
        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            // Verificar si el usuario tiene acceso a este proyecto
            var hasAccess = await _projectService.HasUserAccessAsync(userId, id);
            if (!hasAccess)
                return Forbid();

            var project = await _projectService.UnarchiveProjectAsync(id);
            if (project == null)
                return NotFound(new { message = "Proyecto no encontrado" });

            // Registrar en auditoría
            await _auditLogService.LogActionAsync(
                userId,
                "ProjectUnarchived",
                "Project",
                id,
                $"Proyecto '{project.Name}' desarchivado"
            );

            return Ok(project);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al desarchivar proyecto {ProjectId}", id);
            return StatusCode(500, new { message = "Error al desarchivar proyecto" });
        }
    }

    /// <summary>
    /// Obtiene todos los proyectos archivados del usuario
    /// </summary>
    [HttpGet("archived")]
    public async Task<ActionResult<IEnumerable<ProjectDto>>> GetArchivedProjects()
    {
        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var projects = await _projectService.GetArchivedProjectsForUserAsync(userId);
            return Ok(projects);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener proyectos archivados");
            return StatusCode(500, new { message = "Error al obtener proyectos archivados" });
        }
    }

    /// <summary>
    /// Elimina un proyecto permanentemente (requiere confirmación)
    /// </summary>
    [HttpDelete("{id}/permanent")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeletePermanently(Guid id, [FromQuery] bool confirm = false)
    {
        try
        {
            if (!confirm)
            {
                return BadRequest(new
                {
                    message = "Se requiere confirmación para eliminar permanentemente",
                    hint = "Agregue '?confirm=true' a la URL para confirmar la eliminación"
                });
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            // Verificar si el usuario tiene acceso a este proyecto
            var hasAccess = await _projectService.HasUserAccessAsync(userId, id);
            if (!hasAccess)
                return Forbid();

            // Obtener info del proyecto antes de eliminarlo
            var project = await _projectService.GetProjectByIdAsync(id);
            if (project == null)
                return NotFound(new { message = "Proyecto no encontrado" });

            var deleted = await _projectService.DeleteProjectPermanentlyAsync(id, userId);
            if (!deleted)
                return NotFound(new { message = "Proyecto no encontrado" });

            // Registrar en auditoría
            await _auditLogService.LogActionAsync(
                userId,
                "ProjectDeletedPermanently",
                "Project",
                id,
                $"Proyecto '{project.Name}' eliminado permanentemente"
            );

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar permanentemente proyecto {ProjectId}", id);
            return StatusCode(500, new { message = "Error al eliminar proyecto" });
        }
    }

    /// <summary>
    /// Obtiene el registro de auditoría de un proyecto
    /// </summary>
    [HttpGet("{id}/audit-logs")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<IEnumerable<AuditLogDto>>> GetAuditLogs(Guid id)
    {
        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            // Verificar si el usuario tiene acceso a este proyecto
            var hasAccess = await _projectService.HasUserAccessAsync(userId, id);
            if (!hasAccess)
                return Forbid();

            var logs = await _auditLogService.GetAuditLogsAsync(
                entityType: "Project",
                entityId: id
            );

            return Ok(logs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener logs de auditoría del proyecto {ProjectId}", id);
            return StatusCode(500, new { message = "Error al obtener logs de auditoría" });
        }
    }
}
