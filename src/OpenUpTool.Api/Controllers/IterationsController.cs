using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenUpTool.Core.DTOs;
using OpenUpTool.Core.Interfaces;
using System.Security.Claims;

namespace OpenUpTool.Api.Controllers;

[ApiController]
[Route("api/projects/{projectId}/iterations")]
[Authorize]
public class IterationsController : ControllerBase
{
    private readonly IIterationService _iterationService;
    private readonly IProjectService _projectService;
    private readonly ILogger<IterationsController> _logger;

    public IterationsController(IIterationService iterationService, IProjectService projectService, ILogger<IterationsController> logger)
    {
        _iterationService = iterationService;
        _projectService = projectService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todas las iteraciones de un proyecto
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<IterationDto>>> GetByProject(Guid projectId)
    {
        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            // Verificar si el usuario tiene acceso a este proyecto
            var hasAccess = await _projectService.HasUserAccessAsync(userId, projectId);
            if (!hasAccess)
                return Forbid();

            var iterations = await _iterationService.GetIterationsByProjectAsync(projectId);
            return Ok(iterations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener iteraciones del proyecto {ProjectId}", projectId);
            return StatusCode(500, new { message = "Error al obtener iteraciones" });
        }
    }

    /// <summary>
    /// Crea una nueva iteración
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager,Developer")]
    public async Task<ActionResult<IterationDto>> Create(Guid projectId, [FromBody] CreateIterationDto dto)
    {
        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            // Verificar si el usuario tiene acceso a este proyecto
            var hasAccess = await _projectService.HasUserAccessAsync(userId, projectId);
            if (!hasAccess)
                return Forbid();

            var iteration = await _iterationService.CreateIterationAsync(projectId, dto);
            return CreatedAtAction(nameof(GetByProject), new { projectId }, iteration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear iteración para proyecto {ProjectId}", projectId);
            return StatusCode(500, new { message = "Error al crear iteración" });
        }
    }

    /// <summary>
    /// Actualiza el estado de una iteración
    /// </summary>
    [HttpPatch("{iterationId}/status")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<IterationDto>> UpdateStatus(Guid projectId, Guid iterationId, [FromBody] UpdateIterationStatusDto dto)
    {
        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            // Verificar si el usuario tiene acceso a este proyecto
            var hasAccess = await _projectService.HasUserAccessAsync(userId, projectId);
            if (!hasAccess)
                return Forbid();

            var iteration = await _iterationService.UpdateIterationStatusAsync(iterationId, dto.Status);
            if (iteration == null)
                return NotFound(new { message = "Iteración no encontrada" });

            return Ok(iteration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar estado de iteración {IterationId}", iterationId);
            return StatusCode(500, new { message = "Error al actualizar iteración" });
        }
    }

    // ========== HU-016: CAPACIDAD Y VELOCIDAD ==========

    /// <summary>
    /// Actualiza la capacidad del equipo para una iteración
    /// </summary>
    [HttpPatch("{iterationId}/capacity")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<IterationDto>> UpdateCapacity(Guid projectId, Guid iterationId, [FromBody] UpdateIterationCapacityDto dto)
    {
        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var hasAccess = await _projectService.HasUserAccessAsync(userId, projectId);
            if (!hasAccess)
                return Forbid();

            var iteration = await _iterationService.UpdateIterationCapacityAsync(iterationId, dto);
            if (iteration == null)
                return NotFound(new { message = "Iteración no encontrada" });

            return Ok(iteration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar capacidad de iteración {IterationId}", iterationId);
            return StatusCode(500, new { message = "Error al actualizar capacidad" });
        }
    }

    /// <summary>
    /// Registra la velocidad (puntos completados) de una iteración
    /// </summary>
    [HttpPatch("{iterationId}/velocity")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<IterationDto>> UpdateVelocity(Guid projectId, Guid iterationId, [FromBody] UpdateIterationVelocityDto dto)
    {
        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var hasAccess = await _projectService.HasUserAccessAsync(userId, projectId);
            if (!hasAccess)
                return Forbid();

            var iteration = await _iterationService.UpdateIterationVelocityAsync(iterationId, dto);
            if (iteration == null)
                return NotFound(new { message = "Iteración no encontrada" });

            return Ok(iteration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar velocidad de iteración {IterationId}", iterationId);
            return StatusCode(500, new { message = "Error al registrar velocidad" });
        }
    }

    /// <summary>
    /// Obtiene estadísticas de velocidad histórica del proyecto
    /// </summary>
    [HttpGet("velocity-stats")]
    public async Task<ActionResult<ProjectVelocityStatsDto>> GetVelocityStats(Guid projectId)
    {
        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var hasAccess = await _projectService.HasUserAccessAsync(userId, projectId);
            if (!hasAccess)
                return Forbid();

            var stats = await _iterationService.GetProjectVelocityStatsAsync(projectId);
            if (stats == null)
                return NotFound(new { message = "No hay iteraciones para este proyecto" });

            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener estadísticas de velocidad del proyecto {ProjectId}", projectId);
            return StatusCode(500, new { message = "Error al obtener estadísticas" });
        }
    }

    /// <summary>
    /// Obtiene datos para planificación del siguiente Sprint/iteración
    /// </summary>
    [HttpGet("planning-data")]
    public async Task<ActionResult<PlanningDataDto>> GetPlanningData(Guid projectId)
    {
        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var hasAccess = await _projectService.HasUserAccessAsync(userId, projectId);
            if (!hasAccess)
                return Forbid();

            var data = await _iterationService.GetPlanningDataAsync(projectId);
            if (data == null)
                return NotFound(new { message = "No hay iteraciones para este proyecto" });

            return Ok(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener datos de planificación del proyecto {ProjectId}", projectId);
            return StatusCode(500, new { message = "Error al obtener datos de planificación" });
        }
    }
}
