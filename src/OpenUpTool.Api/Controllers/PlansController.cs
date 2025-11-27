using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenUpTool.Core.DTOs;
using OpenUpTool.Core.Interfaces;

namespace OpenUpTool.Api.Controllers;

[ApiController]
[Route("api/projects/{projectId}/plan")]
[Authorize]
public class PlansController : ControllerBase
{
    private readonly IProjectPlanService _planService;
    private readonly ILogger<PlansController> _logger;

    public PlansController(IProjectPlanService planService, ILogger<PlansController> logger)
    {
        _planService = planService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene el plan de un proyecto
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ProjectPlanDto>> GetByProject(Guid projectId)
    {
        try
        {
            var plan = await _planService.GetPlanByProjectAsync(projectId);
            if (plan == null)
                return NotFound(new { message = "Plan no encontrado" });

            return Ok(plan);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener plan del proyecto {ProjectId}", projectId);
            return StatusCode(500, new { message = "Error al obtener plan" });
        }
    }

    /// <summary>
    /// Crea el plan inicial de un proyecto (v1)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ProjectPlanDto>> CreateInitialPlan(Guid projectId, [FromBody] CreateProjectPlanDto dto)
    {
        try
        {
            var plan = await _planService.CreateInitialPlanAsync(projectId, dto);
            return CreatedAtAction(nameof(GetByProject), new { projectId }, plan);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear plan para proyecto {ProjectId}", projectId);
            return StatusCode(500, new { message = "Error al crear plan" });
        }
    }

    /// <summary>
    /// Crea una nueva versión del plan (v2, v3, etc.)
    /// </summary>
    [HttpPost("new-version")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ProjectPlanDto>> CreateNewVersion(Guid projectId, [FromBody] CreateProjectPlanDto dto)
    {
        try
        {
            var plan = await _planService.CreateNewPlanVersionAsync(projectId, dto);
            return Ok(plan);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear nueva versión del plan para proyecto {ProjectId}", projectId);
            return StatusCode(500, new { message = "Error al crear nueva versión del plan" });
        }
    }

    /// <summary>
    /// Obtiene el historial de versiones del plan de un proyecto
    /// </summary>
    [HttpGet("history")]
    public async Task<ActionResult<IEnumerable<ProjectPlanDto>>> GetHistory(Guid projectId)
    {
        try
        {
            var plans = await _planService.GetPlanHistoryAsync(projectId);
            return Ok(plans);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener historial del plan para proyecto {ProjectId}", projectId);
            return StatusCode(500, new { message = "Error al obtener historial del plan" });
        }
    }
}
