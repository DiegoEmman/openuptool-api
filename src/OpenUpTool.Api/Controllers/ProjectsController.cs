using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenUpTool.Core.DTOs;
using OpenUpTool.Core.Interfaces;

namespace OpenUpTool.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;
    private readonly ILogger<ProjectsController> _logger;

    public ProjectsController(IProjectService projectService, ILogger<ProjectsController> logger)
    {
        _projectService = projectService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los proyectos
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProjectDto>>> GetAll()
    {
        try
        {
            var projects = await _projectService.GetAllProjectsAsync();
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
            var project = await _projectService.GetProjectByIdAsync(id);
            if (project == null)
                return NotFound(new { message = "Proyecto no encontrado" });

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
            _logger.LogInformation("Creando proyecto: {ProjectName}", dto.Name);
            var project = await _projectService.CreateProjectAsync(dto);
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
            await _projectService.DeleteProjectAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar proyecto {ProjectId}", id);
            return StatusCode(500, new { message = "Error al eliminar proyecto" });
        }
    }
}
