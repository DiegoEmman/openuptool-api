using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenUpTool.Core.DTOs;
using OpenUpTool.Core.Interfaces;

namespace OpenUpTool.Api.Controllers;

[ApiController]
[Route("api/projects/{projectId}/phases")]
[Authorize]
public class PhasesController : ControllerBase
{
    private readonly IPhaseService _phaseService;
    private readonly ILogger<PhasesController> _logger;

    public PhasesController(IPhaseService phaseService, ILogger<PhasesController> logger)
    {
        _phaseService = phaseService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todas las fases de un proyecto
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PhaseDto>>> GetByProject(Guid projectId)
    {
        try
        {
            var phases = await _phaseService.GetPhasesByProjectAsync(projectId);
            return Ok(phases);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener fases del proyecto {ProjectId}", projectId);
            return StatusCode(500, new { message = "Error al obtener fases" });
        }
    }

    /// <summary>
    /// Obtiene una fase específica por código
    /// </summary>
    [HttpGet("{phaseCode}")]
    public async Task<ActionResult<PhaseDto>> GetByCode(Guid projectId, string phaseCode)
    {
        try
        {
            var phase = await _phaseService.GetPhaseByCodeAsync(projectId, phaseCode);
            if (phase == null)
                return NotFound(new { message = "Fase no encontrada" });

            return Ok(phase);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener fase {PhaseCode} del proyecto {ProjectId}", phaseCode, projectId);
            return StatusCode(500, new { message = "Error al obtener fase" });
        }
    }

    /// <summary>
    /// Actualiza una fase
    /// </summary>
    [HttpPatch("{phaseId}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<PhaseDto>> Update(Guid projectId, Guid phaseId, [FromBody] UpdatePhaseDto dto)
    {
        try
        {
            var phase = await _phaseService.UpdatePhaseAsync(phaseId, dto);
            if (phase == null)
                return NotFound(new { message = "Fase no encontrada" });

            return Ok(phase);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar fase {PhaseId}", phaseId);
            return StatusCode(500, new { message = "Error al actualizar fase" });
        }
    }

    /// <summary>
    /// Inicia una fase
    /// </summary>
    [HttpPost("{phaseId}/start")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<PhaseDto>> Start(Guid projectId, Guid phaseId)
    {
        try
        {
            var phase = await _phaseService.StartPhaseAsync(phaseId);
            if (phase == null)
                return NotFound(new { message = "Fase no encontrada" });

            return Ok(phase);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al iniciar fase {PhaseId}", phaseId);
            return StatusCode(500, new { message = "Error al iniciar fase" });
        }
    }

    /// <summary>
    /// Completa una fase
    /// </summary>
    [HttpPost("{phaseId}/complete")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<PhaseDto>> Complete(Guid projectId, Guid phaseId)
    {
        try
        {
            var phase = await _phaseService.CompletePhaseAsync(phaseId);
            if (phase == null)
                return NotFound(new { message = "Fase no encontrada" });

            return Ok(phase);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al completar fase {PhaseId}", phaseId);
            return StatusCode(500, new { message = "Error al completar fase" });
        }
    }
}
