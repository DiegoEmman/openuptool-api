using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenUpTool.Core.DTOs;
using OpenUpTool.Core.Interfaces;

namespace OpenUpTool.Api.Controllers;

[ApiController]
[Route("api/iterations/{iterationId}/scope")]
[Authorize]
public class IterationScopeController : ControllerBase
{
    private readonly IIterationScopeService _service;
    private readonly ILogger<IterationScopeController> _logger;

    public IterationScopeController(IIterationScopeService service, ILogger<IterationScopeController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<IterationScopeDto>>> GetAll(Guid iterationId)
    {
        try
        {
            var scope = await _service.GetByIterationIdAsync(iterationId);
            return Ok(scope);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener alcance de iteración {IterationId}", iterationId);
            return StatusCode(500, new { message = "Error al obtener alcance" });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager,Scrum Master")]
    public async Task<ActionResult<IterationScopeDto>> AddToScope(Guid iterationId, AddToScopeDto dto)
    {
        try
        {
            if (dto.IterationId != iterationId)
                return BadRequest(new { message = "El iterationId no coincide" });

            var created = await _service.AddToScopeAsync(dto);
            return CreatedAtAction(nameof(GetAll), new { iterationId }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al agregar item al alcance");
            return StatusCode(500, new { message = "Error al agregar al alcance" });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager,Scrum Master,Developer")]
    public async Task<ActionResult<IterationScopeDto>> Update(Guid iterationId, Guid id, UpdateScopeItemDto dto)
    {
        try
        {
            var updated = await _service.UpdateAsync(id, dto);
            if (updated == null)
                return NotFound(new { message = "Item no encontrado" });

            return Ok(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar item del alcance {Id}", id);
            return StatusCode(500, new { message = "Error al actualizar" });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Manager,Scrum Master")]
    public async Task<IActionResult> RemoveFromScope(Guid iterationId, Guid id)
    {
        try
        {
            var deleted = await _service.RemoveFromScopeAsync(id);
            if (!deleted)
                return NotFound(new { message = "Item no encontrado" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar item del alcance {Id}", id);
            return StatusCode(500, new { message = "Error al eliminar" });
        }
    }
}
