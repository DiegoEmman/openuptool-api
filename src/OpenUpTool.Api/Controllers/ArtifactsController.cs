using Microsoft.AspNetCore.Mvc;
using OpenUpTool.Core.DTOs;
using OpenUpTool.Core.Interfaces;

namespace OpenUpTool.Api.Controllers;

[ApiController]
[Route("api/artifact-types")]
public class ArtifactTypesController : ControllerBase
{
    private readonly IArtifactTypeService _artifactTypeService;
    private readonly ILogger<ArtifactTypesController> _logger;

    public ArtifactTypesController(IArtifactTypeService artifactTypeService, ILogger<ArtifactTypesController> logger)
    {
        _artifactTypeService = artifactTypeService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los tipos de artefactos
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ArtifactTypeDto>>> GetAll([FromQuery] string? phase)
    {
        try
        {
            if (!string.IsNullOrEmpty(phase))
            {
                var typesByPhase = await _artifactTypeService.GetArtifactTypesByPhaseAsync(phase);
                return Ok(typesByPhase);
            }

            var types = await _artifactTypeService.GetAllArtifactTypesAsync();
            return Ok(types);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener tipos de artefactos");
            return StatusCode(500, new { message = "Error al obtener tipos de artefactos" });
        }
    }

    /// <summary>
    /// Inicializa los tipos de artefactos por defecto para Inception
    /// </summary>
    [HttpPost("seed-inception")]
    public async Task<IActionResult> SeedInception()
    {
        try
        {
            await _artifactTypeService.SeedDefaultInceptionTypesAsync();
            return Ok(new { message = "Tipos de artefactos de Inception inicializados" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al inicializar tipos de artefactos");
            return StatusCode(500, new { message = "Error al inicializar tipos de artefactos" });
        }
    }
}

[ApiController]
[Route("api/projects/{projectId}/artifacts")]
public class ArtifactsController : ControllerBase
{
    private readonly IArtifactService _artifactService;
    private readonly ILogger<ArtifactsController> _logger;

    public ArtifactsController(IArtifactService artifactService, ILogger<ArtifactsController> logger)
    {
        _artifactService = artifactService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene artefactos de un proyecto y fase
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ArtifactDto>>> GetByProjectAndPhase(Guid projectId, [FromQuery] string phaseId)
    {
        try
        {
            if (string.IsNullOrEmpty(phaseId))
                return BadRequest(new { message = "phaseId es requerido" });

            var artifacts = await _artifactService.GetArtifactsByProjectAndPhaseAsync(projectId, phaseId);
            return Ok(artifacts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener artefactos del proyecto {ProjectId} y fase {PhaseId}", projectId, phaseId);
            return StatusCode(500, new { message = "Error al obtener artefactos" });
        }
    }

    /// <summary>
    /// Crea un nuevo artefacto
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ArtifactDto>> Create(Guid projectId, [FromBody] CreateArtifactDto dto)
    {
        try
        {
            // Validar que el projectId del DTO coincida con el de la ruta
            if (dto.ProjectId != projectId)
                return BadRequest(new { message = "El projectId del cuerpo debe coincidir con el de la URL" });

            var artifact = await _artifactService.CreateArtifactAsync(dto);
            return CreatedAtAction(nameof(GetByProjectAndPhase), new { projectId, phaseId = dto.PhaseId }, artifact);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear artefacto");
            return StatusCode(500, new { message = "Error al crear artefacto" });
        }
    }

    /// <summary>
    /// Actualiza un artefacto
    /// </summary>
    [HttpPatch("{artifactId}")]
    public async Task<ActionResult<ArtifactDto>> Update(Guid projectId, Guid artifactId, [FromBody] UpdateArtifactDto dto)
    {
        try
        {
            var artifact = await _artifactService.UpdateArtifactAsync(artifactId, dto);
            if (artifact == null)
                return NotFound(new { message = "Artefacto no encontrado" });

            return Ok(artifact);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar artefacto {ArtifactId}", artifactId);
            return StatusCode(500, new { message = "Error al actualizar artefacto" });
        }
    }
}
