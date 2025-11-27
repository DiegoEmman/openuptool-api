using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenUpTool.Core.DTOs;
using OpenUpTool.Core.Interfaces;

namespace OpenUpTool.Api.Controllers;

[ApiController]
[Route("api/projects/{projectId}/stories")]
[Authorize]
public class UserStoriesController : ControllerBase
{
    private readonly IUserStoryService _service;
    private readonly ILogger<UserStoriesController> _logger;

    public UserStoriesController(IUserStoryService service, ILogger<UserStoriesController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todas las historias de usuario de un proyecto
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserStoryDto>>> GetAll(Guid projectId)
    {
        try
        {
            var stories = await _service.GetByProjectIdAsync(projectId);
            return Ok(stories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener historias del proyecto {ProjectId}", projectId);
            return StatusCode(500, new { message = "Error al obtener historias" });
        }
    }

    /// <summary>
    /// Obtiene el backlog del proyecto
    /// </summary>
    [HttpGet("backlog")]
    public async Task<ActionResult<IEnumerable<UserStoryDto>>> GetBacklog(Guid projectId)
    {
        try
        {
            var stories = await _service.GetBacklogAsync(projectId);
            return Ok(stories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener backlog del proyecto {ProjectId}", projectId);
            return StatusCode(500, new { message = "Error al obtener backlog" });
        }
    }

    /// <summary>
    /// Obtiene una historia por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<UserStoryDto>> GetById(Guid projectId, Guid id)
    {
        try
        {
            var story = await _service.GetByIdAsync(id);
            if (story == null)
                return NotFound(new { message = "Historia no encontrada" });

            if (story.ProjectId != projectId)
                return BadRequest(new { message = "La historia no pertenece a este proyecto" });

            return Ok(story);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener historia {Id}", id);
            return StatusCode(500, new { message = "Error al obtener historia" });
        }
    }

    /// <summary>
    /// Crea una nueva historia de usuario
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager,Product Owner,Scrum Master")]
    public async Task<ActionResult<UserStoryDto>> Create(Guid projectId, CreateUserStoryDto dto)
    {
        try
        {
            if (dto.ProjectId != projectId)
                return BadRequest(new { message = "El projectId del body no coincide con la ruta" });

            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { projectId, id = created.Id }, created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear historia en proyecto {ProjectId}", projectId);
            return StatusCode(500, new { message = "Error al crear historia" });
        }
    }

    /// <summary>
    /// Actualiza una historia de usuario
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager,Product Owner,Scrum Master,Developer")]
    public async Task<ActionResult<UserStoryDto>> Update(Guid projectId, Guid id, UpdateUserStoryDto dto)
    {
        try
        {
            var updated = await _service.UpdateAsync(id, dto);
            if (updated == null)
                return NotFound(new { message = "Historia no encontrada" });

            if (updated.ProjectId != projectId)
                return BadRequest(new { message = "La historia no pertenece a este proyecto" });

            return Ok(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar historia {Id}", id);
            return StatusCode(500, new { message = "Error al actualizar historia" });
        }
    }

    /// <summary>
    /// Elimina una historia de usuario
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Manager,Product Owner")]
    public async Task<IActionResult> Delete(Guid projectId, Guid id)
    {
        try
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted)
                return NotFound(new { message = "Historia no encontrada" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar historia {Id}", id);
            return StatusCode(500, new { message = "Error al eliminar historia" });
        }
    }
}
