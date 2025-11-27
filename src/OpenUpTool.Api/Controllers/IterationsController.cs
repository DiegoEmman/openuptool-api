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
}
