using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenUpTool.Core.DTOs;
using OpenUpTool.Core.Interfaces;
using System.Security.Claims;

namespace OpenUpTool.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FinalBuildsController : ControllerBase
{
    private readonly IFinalBuildService _buildService;
    private readonly ILogger<FinalBuildsController> _logger;

    public FinalBuildsController(
        IFinalBuildService buildService,
        ILogger<FinalBuildsController> logger)
    {
        _buildService = buildService;
        _logger = logger;
    }

    /// <summary>
    /// Obtener todos los builds finales
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<FinalBuildDto>>> GetAll()
    {
        try
        {
            var builds = await _buildService.GetAllBuildsAsync();
            return Ok(builds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener builds");
            return StatusCode(500, new { message = "Error al obtener builds" });
        }
    }

    /// <summary>
    /// Obtener builds por proyecto
    /// </summary>
    [HttpGet("project/{projectId}")]
    public async Task<ActionResult<IEnumerable<FinalBuildDto>>> GetByProject(Guid projectId)
    {
        try
        {
            var builds = await _buildService.GetBuildsByProjectIdAsync(projectId);
            return Ok(builds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener builds del proyecto {ProjectId}", projectId);
            return StatusCode(500, new { message = "Error al obtener builds" });
        }
    }

    /// <summary>
    /// Obtener build por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<FinalBuildDto>> GetById(Guid id)
    {
        try
        {
            var build = await _buildService.GetBuildByIdAsync(id);
            if (build == null)
                return NotFound(new { message = $"Build con ID {id} no encontrado" });

            return Ok(build);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener build {Id}", id);
            return StatusCode(500, new { message = "Error al obtener build" });
        }
    }

    /// <summary>
    /// Obtener build por número
    /// </summary>
    [HttpGet("project/{projectId}/number/{buildNumber}")]
    public async Task<ActionResult<FinalBuildDto>> GetByNumber(Guid projectId, string buildNumber)
    {
        try
        {
            var build = await _buildService.GetBuildByNumberAsync(projectId, buildNumber);
            if (build == null)
                return NotFound(new { message = $"Build {buildNumber} no encontrado para el proyecto {projectId}" });

            return Ok(build);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener build {BuildNumber} del proyecto {ProjectId}", buildNumber, projectId);
            return StatusCode(500, new { message = "Error al obtener build" });
        }
    }

    /// <summary>
    /// Registrar build final
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<FinalBuildDto>> Create([FromBody] CreateFinalBuildDto dto)
    {
        try
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "CI/CD System";
            var build = await _buildService.CreateBuildAsync(dto, userEmail);
            return CreatedAtAction(nameof(GetById), new { id = build.Id }, build);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear build");
            return StatusCode(500, new { message = "Error al crear build" });
        }
    }

    /// <summary>
    /// Actualizar build final
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<FinalBuildDto>> Update(Guid id, [FromBody] UpdateFinalBuildDto dto)
    {
        try
        {
            var build = await _buildService.UpdateBuildAsync(id, dto);
            if (build == null)
                return NotFound(new { message = $"Build con ID {id} no encontrado" });

            return Ok(build);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar build {Id}", id);
            return StatusCode(500, new { message = "Error al actualizar build" });
        }
    }

    /// <summary>
    /// Eliminar build final
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        try
        {
            var deleted = await _buildService.DeleteBuildAsync(id);
            if (!deleted)
                return NotFound(new { message = $"Build con ID {id} no encontrado" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar build {Id}", id);
            return StatusCode(500, new { message = "Error al eliminar build" });
        }
    }
}
