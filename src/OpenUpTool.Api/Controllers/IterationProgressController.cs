using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenUpTool.Core.DTOs;
using OpenUpTool.Core.Interfaces;
using System.Security.Claims;

namespace OpenUpTool.Api.Controllers;

[ApiController]
[Route("api/iterations/{iterationId}/tasks")]
[Authorize]
public class IterationTasksController : ControllerBase
{
    private readonly IIterationTaskService _taskService;
    private readonly IIterationService _iterationService;
    private readonly IProjectService _projectService;
    private readonly ILogger<IterationTasksController> _logger;

    public IterationTasksController(
        IIterationTaskService taskService,
        IIterationService iterationService,
        IProjectService projectService,
        ILogger<IterationTasksController> logger)
    {
        _taskService = taskService;
        _iterationService = iterationService;
        _projectService = projectService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todas las tareas de una iteración
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<IterationTaskDto>>> GetAll(Guid iterationId)
    {
        try
        {
            var tasks = await _taskService.GetTasksByIterationAsync(iterationId);
            return Ok(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener tareas de la iteración {IterationId}", iterationId);
            return StatusCode(500, new { message = "Error al obtener tareas" });
        }
    }

    /// <summary>
    /// Crea una nueva tarea en una iteración
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager,Scrum Master,Developer")]
    public async Task<ActionResult<IterationTaskDto>> Create(Guid iterationId, [FromBody] CreateIterationTaskDto dto)
    {
        try
        {
            var task = await _taskService.CreateTaskAsync(iterationId, dto);
            return CreatedAtAction(nameof(GetAll), new { iterationId }, task);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear tarea en iteración {IterationId}", iterationId);
            return StatusCode(500, new { message = "Error al crear tarea" });
        }
    }

    /// <summary>
    /// Actualiza una tarea de una iteración
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager,Scrum Master,Developer")]
    public async Task<ActionResult<IterationTaskDto>> Update(Guid iterationId, Guid id, [FromBody] UpdateIterationTaskDto dto)
    {
        try
        {
            var task = await _taskService.UpdateTaskAsync(id, dto);
            if (task == null)
                return NotFound(new { message = "Tarea no encontrada" });

            return Ok(task);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar tarea {Id}", id);
            return StatusCode(500, new { message = "Error al actualizar tarea" });
        }
    }

    /// <summary>
    /// Elimina una tarea de una iteración
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Manager,Scrum Master")]
    public async Task<IActionResult> Delete(Guid iterationId, Guid id)
    {
        try
        {
            var deleted = await _taskService.DeleteTaskAsync(id);
            if (!deleted)
                return NotFound(new { message = "Tarea no encontrada" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar tarea {Id}", id);
            return StatusCode(500, new { message = "Error al eliminar tarea" });
        }
    }
}

[ApiController]
[Route("api/iterations/{iterationId}/progress")]
[Authorize]
public class IterationProgressController : ControllerBase
{
    private readonly IIterationProgressService _progressService;
    private readonly ILogger<IterationProgressController> _logger;

    public IterationProgressController(
        IIterationProgressService progressService,
        ILogger<IterationProgressController> logger)
    {
        _progressService = progressService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los registros de progreso de una iteración
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<IterationProgressDto>>> GetAll(Guid iterationId)
    {
        try
        {
            var progress = await _progressService.GetProgressByIterationAsync(iterationId);
            return Ok(progress);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener progreso de iteración {IterationId}", iterationId);
            return StatusCode(500, new { message = "Error al obtener progreso" });
        }
    }

    /// <summary>
    /// Obtiene el último registro de progreso de una iteración
    /// </summary>
    [HttpGet("latest")]
    public async Task<ActionResult<IterationProgressDto>> GetLatest(Guid iterationId)
    {
        try
        {
            var progress = await _progressService.GetLatestProgressAsync(iterationId);
            if (progress == null)
                return NotFound(new { message = "No hay registros de progreso" });

            return Ok(progress);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener último progreso de iteración {IterationId}", iterationId);
            return StatusCode(500, new { message = "Error al obtener progreso" });
        }
    }

    /// <summary>
    /// Obtiene el resumen de una iteración con su progreso
    /// </summary>
    [HttpGet("summary")]
    public async Task<ActionResult<IterationSummaryDto>> GetSummary(Guid iterationId)
    {
        try
        {
            var summary = await _progressService.GetIterationSummaryAsync(iterationId);
            if (summary == null)
                return NotFound(new { message = "Iteración no encontrada" });

            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener resumen de iteración {IterationId}", iterationId);
            return StatusCode(500, new { message = "Error al obtener resumen" });
        }
    }

    /// <summary>
    /// Obtiene los datos para el gráfico de burndown de una iteración
    /// </summary>
    [HttpGet("burndown")]
    public async Task<ActionResult<BurndownDataDto>> GetBurndownData(Guid iterationId)
    {
        try
        {
            var burndownData = await _progressService.GetBurndownDataAsync(iterationId);
            if (burndownData == null)
                return NotFound(new { message = "Iteración no encontrada" });

            return Ok(burndownData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener datos de burndown para iteración {IterationId}", iterationId);
            return StatusCode(500, new { message = "Error al obtener datos de burndown" });
        }
    }

    /// <summary>
    /// Crea un nuevo registro de progreso para una iteración
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager,Scrum Master")]
    public async Task<ActionResult<IterationProgressDto>> Create(Guid iterationId, [FromBody] CreateIterationProgressDto dto)
    {
        try
        {
            var progress = await _progressService.CreateProgressRecordAsync(iterationId, dto);
            return CreatedAtAction(nameof(GetAll), new { iterationId }, progress);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear registro de progreso en iteración {IterationId}", iterationId);
            return StatusCode(500, new { message = "Error al crear registro de progreso" });
        }
    }

    /// <summary>
    /// Actualiza un registro de progreso existente
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager,Scrum Master")]
    public async Task<ActionResult<IterationProgressDto>> Update(Guid iterationId, Guid id, [FromBody] UpdateIterationProgressDto dto)
    {
        try
        {
            var progress = await _progressService.UpdateProgressRecordAsync(id, dto);
            if (progress == null)
                return NotFound(new { message = "Registro de progreso no encontrado" });

            return Ok(progress);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar registro de progreso {Id}", id);
            return StatusCode(500, new { message = "Error al actualizar registro de progreso" });
        }
    }
}
