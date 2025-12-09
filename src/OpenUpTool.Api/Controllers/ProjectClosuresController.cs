using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenUpTool.Core.DTOs;
using OpenUpTool.Core.Interfaces;
using System.Security.Claims;

namespace OpenUpTool.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProjectClosuresController : ControllerBase
{
    private readonly IProjectClosureService _closureService;
    private readonly IProjectService _projectService;
    private readonly ILogger<ProjectClosuresController> _logger;

    public ProjectClosuresController(
        IProjectClosureService closureService,
        IProjectService projectService,
        ILogger<ProjectClosuresController> logger)
    {
        _closureService = closureService;
        _projectService = projectService;
        _logger = logger;
    }

    /// <summary>
    /// Obtener todos los cierres de proyecto
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProjectClosureDto>>> GetAll()
    {
        try
        {
            var closures = await _closureService.GetAllClosuresAsync();
            return Ok(closures);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cierres de proyecto");
            return StatusCode(500, new { message = "Error al obtener cierres de proyecto" });
        }
    }

    /// <summary>
    /// Obtener cierre por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ProjectClosureDto>> GetById(Guid id)
    {
        try
        {
            var closure = await _closureService.GetClosureByIdAsync(id);
            if (closure == null)
                return NotFound(new { message = $"Cierre con ID {id} no encontrado" });

            return Ok(closure);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cierre {Id}", id);
            return StatusCode(500, new { message = "Error al obtener cierre" });
        }
    }

    /// <summary>
    /// Obtener cierre por proyecto
    /// </summary>
    [HttpGet("project/{projectId}")]
    public async Task<ActionResult<ProjectClosureDto>> GetByProjectId(Guid projectId)
    {
        try
        {
            var closure = await _closureService.GetClosureByProjectIdAsync(projectId);
            if (closure == null)
                return NotFound(new { message = $"No se encontró cierre para el proyecto {projectId}" });

            return Ok(closure);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cierre del proyecto {ProjectId}", projectId);
            return StatusCode(500, new { message = "Error al obtener cierre" });
        }
    }

    /// <summary>
    /// Validar si un proyecto puede cerrarse
    /// </summary>
    [HttpGet("validate/{projectId}")]
    public async Task<ActionResult<ClosureValidationDto>> ValidateClosure(Guid projectId)
    {
        try
        {
            var validation = await _closureService.ValidateClosureAsync(projectId);
            return Ok(validation);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al validar cierre del proyecto {ProjectId}", projectId);
            return StatusCode(500, new { message = "Error al validar cierre" });
        }
    }

    /// <summary>
    /// Crear documento de cierre
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ProjectClosureDto>> Create([FromBody] CreateProjectClosureDto dto)
    {
        try
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "Unknown";
            var closure = await _closureService.CreateClosureAsync(dto, userEmail);
            return CreatedAtAction(nameof(GetById), new { id = closure.Id }, closure);
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
            _logger.LogError(ex, "Error al crear cierre");
            return StatusCode(500, new { message = "Error al crear cierre" });
        }
    }

    /// <summary>
    /// Cerrar proyecto (valida checklist y artefactos). Si `force` es true se requiere rol Admin.
    /// </summary>
    [HttpPost("project/{projectId}/close")]
    public async Task<ActionResult<ProjectClosureDto>> CloseProject(Guid projectId, [FromBody] CloseProjectDto dto)
    {
        try
        {
            var validation = await _closureService.ValidateClosureAsync(projectId);

            if (!dto.Force && !validation.CanClose)
            {
                return BadRequest(validation);
            }

            // If forcing, only Admins can perform
            if (dto.Force)
            {
                var isAdmin = User.IsInRole("Admin") || User.Claims.Any(c => c.Type == System.Security.Claims.ClaimTypes.Role && c.Value == "Admin");
                if (!isAdmin)
                    return Forbid();
            }

            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "Unknown";

            // Use the checklist from validation to build the closure document checklist
            var checklist = validation.ChecklistPreview ?? new List<ClosureCriteriaDto>();

            var createDto = new CreateProjectClosureDto
            {
                ProjectId = projectId,
                Summary = dto.Justification ?? (validation.CanClose ? "Cierre automático al cumplir criterios" : "Cierre forzado"),
                LessonsLearned = string.Empty,
                Recommendations = string.Empty,
                Checklist = checklist
            };

            var created = await _closureService.CreateClosureAsync(createDto, userEmail);

            // Archive project if closure created
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            Guid archivedBy = Guid.Empty;
            if (!string.IsNullOrEmpty(userIdClaim)) Guid.TryParse(userIdClaim, out archivedBy);
            await _projectService.ArchiveProjectAsync(projectId, archivedBy);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
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
            _logger.LogError(ex, "Error al cerrar el proyecto {ProjectId}", projectId);
            return StatusCode(500, new { message = "Error al cerrar el proyecto" });
        }
    }

    /// <summary>
    /// Actualizar documento de cierre
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ProjectClosureDto>> Update(Guid id, [FromBody] UpdateProjectClosureDto dto)
    {
        try
        {
            var closure = await _closureService.UpdateClosureAsync(id, dto);
            if (closure == null)
                return NotFound(new { message = $"Cierre con ID {id} no encontrado" });

            return Ok(closure);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar cierre {Id}", id);
            return StatusCode(500, new { message = "Error al actualizar cierre" });
        }
    }

    /// <summary>
    /// Aprobar o rechazar cierre
    /// </summary>
    [HttpPost("{id}/approve")]
    public async Task<ActionResult<ProjectClosureDto>> Approve(Guid id, [FromBody] ApproveClosureDto dto)
    {
        try
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "Unknown";
            var closure = await _closureService.ApproveClosureAsync(id, userEmail, dto);
            if (closure == null)
                return NotFound(new { message = $"Cierre con ID {id} no encontrado" });

            return Ok(closure);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al aprobar cierre {Id}", id);
            return StatusCode(500, new { message = "Error al aprobar cierre" });
        }
    }

    /// <summary>
    /// Eliminar documento de cierre
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        try
        {
            var deleted = await _closureService.DeleteClosureAsync(id);
            if (!deleted)
                return NotFound(new { message = $"Cierre con ID {id} no encontrado" });

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar cierre {Id}", id);
            return StatusCode(500, new { message = "Error al eliminar cierre" });
        }
    }
}
