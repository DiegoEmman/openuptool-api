using Microsoft.AspNetCore.Mvc;
using OpenUpTool.Core.DTOs;
using OpenUpTool.Core.Interfaces;

namespace OpenUpTool.Api.Controllers;

[ApiController]
[Route("api/projects/{projectId}/plan")]
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
    /// Crea el plan inicial de un proyecto
    /// </summary>
    [HttpPost]
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
}
