using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenUpTool.Core.DTOs;
using OpenUpTool.Core.Interfaces;
using System.Security.Claims;

namespace OpenUpTool.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MicroincrementsController : ControllerBase
{
    private readonly IMicroincrementService _microincrementService;
    private readonly ILogger<MicroincrementsController> _logger;

    public MicroincrementsController(
        IMicroincrementService microincrementService,
        ILogger<MicroincrementsController> logger)
    {
        _microincrementService = microincrementService;
        _logger = logger;
    }

    /// <summary>
    /// Obtener todos los microincrementos
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MicroincrementDto>>> GetAll()
    {
        try
        {
            var microincrements = await _microincrementService.GetAllAsync();
            return Ok(microincrements);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener microincrementos");
            return StatusCode(500, new { message = "Error al obtener microincrementos" });
        }
    }

    /// <summary>
    /// Obtener microincrementos filtrados por iteración, entregable, autor y/o tipo
    /// </summary>
    [HttpGet("filter")]
    public async Task<ActionResult<IEnumerable<MicroincrementDto>>> GetFiltered(
        [FromQuery] Guid? iterationId,
        [FromQuery] Guid? artifactId,
        [FromQuery] string? author,
        [FromQuery] string? type)
    {
        try
        {
            var microincrements = await _microincrementService.GetFilteredAsync(iterationId, artifactId, author, type);
            return Ok(microincrements);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al filtrar microincrementos");
            return StatusCode(500, new { message = "Error al filtrar microincrementos" });
        }
    }

    /// <summary>
    /// Obtener microincrementos por tipo (tecnico o funcional)
    /// </summary>
    [HttpGet("type/{type}")]
    public async Task<ActionResult<IEnumerable<MicroincrementDto>>> GetByType(string type)
    {
        try
        {
            if (type != "tecnico" && type != "funcional")
            {
                return BadRequest(new { message = "El tipo debe ser 'tecnico' o 'funcional'" });
            }
            var microincrements = await _microincrementService.GetByTypeAsync(type);
            return Ok(microincrements);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener microincrementos del tipo {Type}", type);
            return StatusCode(500, new { message = "Error al obtener microincrementos" });
        }
    }

    /// <summary>
    /// Obtener microincrementos por iteración
    /// </summary>
    [HttpGet("iteration/{iterationId}")]
    public async Task<ActionResult<IEnumerable<MicroincrementDto>>> GetByIteration(Guid iterationId)
    {
        try
        {
            var microincrements = await _microincrementService.GetByIterationIdAsync(iterationId);
            return Ok(microincrements);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener microincrementos de la iteración {IterationId}", iterationId);
            return StatusCode(500, new { message = "Error al obtener microincrementos" });
        }
    }

    /// <summary>
    /// Obtener microincrementos por entregable (artifact)
    /// </summary>
    [HttpGet("artifact/{artifactId}")]
    public async Task<ActionResult<IEnumerable<MicroincrementDto>>> GetByArtifact(Guid artifactId)
    {
        try
        {
            var microincrements = await _microincrementService.GetByArtifactIdAsync(artifactId);
            return Ok(microincrements);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener microincrementos del artifact {ArtifactId}", artifactId);
            return StatusCode(500, new { message = "Error al obtener microincrementos" });
        }
    }

    /// <summary>
    /// Obtener microincrementos por autor
    /// </summary>
    [HttpGet("author/{author}")]
    public async Task<ActionResult<IEnumerable<MicroincrementDto>>> GetByAuthor(string author)
    {
        try
        {
            var microincrements = await _microincrementService.GetByAuthorAsync(author);
            return Ok(microincrements);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener microincrementos del autor {Author}", author);
            return StatusCode(500, new { message = "Error al obtener microincrementos" });
        }
    }

    /// <summary>
    /// Obtener un microincremento por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<MicroincrementDto>> GetById(Guid id)
    {
        try
        {
            var microincrement = await _microincrementService.GetByIdAsync(id);
            if (microincrement == null)
            {
                return NotFound(new { message = $"Microincremento con ID {id} no encontrado" });
            }
            return Ok(microincrement);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener microincremento {Id}", id);
            return StatusCode(500, new { message = "Error al obtener microincremento" });
        }
    }

    /// <summary>
    /// Crear un nuevo microincremento
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<MicroincrementDto>> Create([FromBody] CreateMicroincrementDto dto)
    {
        try
        {
            var microincrement = await _microincrementService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = microincrement.Id }, microincrement);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear microincremento");
            return StatusCode(500, new { message = "Error al crear microincremento" });
        }
    }

    /// <summary>
    /// Actualizar un microincremento
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<MicroincrementDto>> Update(Guid id, [FromBody] UpdateMicroincrementDto dto)
    {
        try
        {
            var microincrement = await _microincrementService.UpdateAsync(id, dto);
            if (microincrement == null)
            {
                return NotFound(new { message = $"Microincremento con ID {id} no encontrado" });
            }
            return Ok(microincrement);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar microincremento {Id}", id);
            return StatusCode(500, new { message = "Error al actualizar microincremento" });
        }
    }

    /// <summary>
    /// Eliminar un microincremento
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        try
        {
            var deleted = await _microincrementService.DeleteAsync(id);
            if (!deleted)
            {
                return NotFound(new { message = $"Microincremento con ID {id} no encontrado" });
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar microincremento {Id}", id);
            return StatusCode(500, new { message = "Error al eliminar microincremento" });
        }
    }
}
