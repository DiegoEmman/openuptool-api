using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenUpTool.Api.Models;
using OpenUpTool.Core.DTOs;
using OpenUpTool.Core.Interfaces;
using System.Security.Claims;

namespace OpenUpTool.Api.Controllers;

[ApiController]
[Route("api/artifact-types")]
[Authorize]
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
    [Authorize(Roles = "Admin")]
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
[Authorize]
public class ArtifactsController : ControllerBase
{
    private readonly IArtifactService _artifactService;
    private readonly IProjectService _projectService;
    private readonly ILogger<ArtifactsController> _logger;

    public ArtifactsController(IArtifactService artifactService, IProjectService projectService, ILogger<ArtifactsController> logger)
    {
        _artifactService = artifactService;
        _projectService = projectService;
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
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            // Verificar si el usuario tiene acceso a este proyecto
            var hasAccess = await _projectService.HasUserAccessAsync(userId, projectId);
            if (!hasAccess)
                return Forbid();

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
    [Consumes("multipart/form-data")]
    [Authorize(Roles = "Admin,Manager,Developer")]
    public async Task<ActionResult<ArtifactDto>> Create(Guid projectId, [FromForm] CreateArtifactRequest request)
    {
        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            // Verificar si el usuario tiene acceso a este proyecto
            var hasAccess = await _projectService.HasUserAccessAsync(userId, projectId);
            if (!hasAccess)
                return Forbid();

            // Validar que el projectId del request coincida con el de la ruta
            if (request.ProjectId != projectId)
                return BadRequest(new { message = "El projectId del cuerpo debe coincidir con el de la URL" });

            Stream? fileStream = null;
            string? fileName = null;

            if (request.File != null && request.File.Length > 0)
            {
                if (string.IsNullOrEmpty(request.FileCategory))
                {
                    return BadRequest(new { message = "FileCategory es requerida cuando se adjunta un archivo" });
                }

                fileStream = request.File.OpenReadStream();
                fileName = request.File.FileName;
            }

            // Convertir el request a DTO
            var dto = new CreateArtifactDto(
                request.ProjectId,
                request.PhaseId,
                request.ArtifactTypeId,
                request.Title,
                request.Description,
                request.Author,
                request.IsMandatory,
                request.ContentText,
                request.FileCategory,
                request.RepositoryUrl,
                request.RepositoryVersion,
                request.BuildNumber
            );

            var artifact = await _artifactService.CreateArtifactAsync(dto, fileStream, fileName);
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
    [Consumes("multipart/form-data")]
    [Authorize(Roles = "Admin,Manager,Developer")]
    public async Task<ActionResult<ArtifactDto>> Update(Guid projectId, Guid artifactId, [FromForm] UpdateArtifactRequest request)
    {
        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            // Verificar si el usuario tiene acceso a este proyecto
            var hasAccess = await _projectService.HasUserAccessAsync(userId, projectId);
            if (!hasAccess)
                return Forbid();

            Stream? fileStream = null;
            string? fileName = null;

            if (request.File != null && request.File.Length > 0)
            {
                if (string.IsNullOrEmpty(request.FileCategory))
                {
                    return BadRequest(new { message = "FileCategory es requerida cuando se adjunta un archivo" });
                }

                fileStream = request.File.OpenReadStream();
                fileName = request.File.FileName;
            }

            // Convertir el request a DTO
            var dto = new UpdateArtifactDto(
                request.Title,
                request.Description,
                request.Author,
                request.Status,
                request.IsMandatory,
                request.ContentText,
                request.FileCategory,
                request.RepositoryUrl,
                request.RepositoryVersion,
                request.BuildNumber
            );

            var artifact = await _artifactService.UpdateArtifactAsync(artifactId, dto, fileStream, fileName);
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

    /// <summary>
    /// Descarga el archivo adjunto de un artefacto
    /// </summary>
    [HttpGet("{artifactId}/file")]
    public async Task<IActionResult> DownloadFile(Guid projectId, Guid artifactId)
    {
        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            // Verificar si el usuario tiene acceso a este proyecto
            var hasAccess = await _projectService.HasUserAccessAsync(userId, projectId);
            if (!hasAccess)
                return Forbid();

            var fileStream = await _artifactService.GetArtifactFileAsync(artifactId);
            if (fileStream == null)
                return NotFound(new { message = "Archivo no encontrado" });

            // Obtener el artefacto para conocer el nombre y tipo de archivo
            var artifacts = await _artifactService.GetArtifactsByProjectAndPhaseAsync(projectId, "");
            var artifact = artifacts.FirstOrDefault(a => a.Id == artifactId);
            
            if (artifact == null)
                return NotFound(new { message = "Artefacto no encontrado" });

            var contentType = artifact.MimeType ?? "application/octet-stream";
            var fileName = artifact.FileName ?? "download";

            return File(fileStream, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al descargar archivo del artefacto {ArtifactId}", artifactId);
            return StatusCode(500, new { message = "Error al descargar archivo" });
        }
    }

    /// <summary>
    /// Obtiene los formatos de archivo permitidos por categoría
    /// </summary>
    [HttpGet("allowed-formats")]
    public async Task<ActionResult<List<AllowedFileFormatsDto>>> GetAllowedFormats()
    {
        try
        {
            var formats = await _artifactService.GetAllowedFileFormatsAsync();
            return Ok(formats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener formatos permitidos");
            return StatusCode(500, new { message = "Error al obtener formatos permitidos" });
        }
    }

    /// <summary>
    /// Vincula un repositorio externo al artefacto
    /// </summary>
    [HttpPost("{artifactId}/link-repository")]
    public async Task<ActionResult<ArtifactDto>> LinkRepository(
        Guid projectId, 
        Guid artifactId, 
        [FromBody] LinkRepositoryRequest request)
    {
        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var hasAccess = await _projectService.HasUserAccessAsync(userId, projectId);
            if (!hasAccess)
                return Forbid();

            var artifact = await _artifactService.LinkRepositoryAsync(artifactId, request);
            if (artifact == null)
                return NotFound(new { message = "Artefacto no encontrado" });

            return Ok(artifact);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al vincular repositorio al artefacto {ArtifactId}", artifactId);
            return StatusCode(500, new { message = "Error al vincular repositorio" });
        }
    }

    /// <summary>
    /// Agrega un caso de prueba al artefacto
    /// </summary>
    [HttpPost("{artifactId}/test-cases")]
    public async Task<ActionResult<ArtifactDto>> AddTestCase(
        Guid projectId, 
        Guid artifactId, 
        [FromBody] AddTestCaseRequest request)
    {
        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var hasAccess = await _projectService.HasUserAccessAsync(userId, projectId);
            if (!hasAccess)
                return Forbid();

            var artifact = await _artifactService.AddTestCaseAsync(artifactId, request);
            if (artifact == null)
                return NotFound(new { message = "Artefacto no encontrado" });

            return Ok(artifact);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al agregar caso de prueba al artefacto {ArtifactId}", artifactId);
            return StatusCode(500, new { message = "Error al agregar caso de prueba" });
        }
    }

    /// <summary>
    /// Agrega un resultado de prueba al artefacto
    /// </summary>
    [HttpPost("{artifactId}/test-results")]
    public async Task<ActionResult<ArtifactDto>> AddTestResult(
        Guid projectId, 
        Guid artifactId, 
        [FromBody] AddTestResultRequest request)
    {
        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var hasAccess = await _projectService.HasUserAccessAsync(userId, projectId);
            if (!hasAccess)
                return Forbid();

            var artifact = await _artifactService.AddTestResultAsync(artifactId, request);
            if (artifact == null)
                return NotFound(new { message = "Artefacto no encontrado" });

            return Ok(artifact);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al agregar resultado de prueba al artefacto {ArtifactId}", artifactId);
            return StatusCode(500, new { message = "Error al agregar resultado de prueba" });
        }
    }

    /// <summary>
    /// Agrega una actividad de iteración al artefacto
    /// </summary>
    [HttpPost("{artifactId}/iteration-activities")]
    public async Task<ActionResult<ArtifactDto>> AddIterationActivity(
        Guid projectId, 
        Guid artifactId, 
        [FromBody] AddIterationActivityRequest request)
    {
        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var hasAccess = await _projectService.HasUserAccessAsync(userId, projectId);
            if (!hasAccess)
                return Forbid();

            var artifact = await _artifactService.AddIterationActivityAsync(artifactId, request);
            if (artifact == null)
                return NotFound(new { message = "Artefacto no encontrado" });

            return Ok(artifact);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al agregar actividad de iteración al artefacto {ArtifactId}", artifactId);
            return StatusCode(500, new { message = "Error al agregar actividad de iteración" });
        }
    }

    /// <summary>
    /// Valida si un proyecto puede avanzar de fase verificando artefactos obligatorios
    /// </summary>
    [HttpGet("~/api/projects/{projectId}/phases/{phaseId}/validate")]
    public async Task<ActionResult<PhaseValidationDto>> ValidatePhaseCompletion(
        Guid projectId,
        string phaseId)
    {
        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var hasAccess = await _projectService.HasUserAccessAsync(userId, projectId);
            if (!hasAccess)
                return Forbid();

            var validation = await _artifactService.ValidatePhaseCompletionAsync(projectId, phaseId);
            
            return Ok(validation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al validar finalización de fase {PhaseId} del proyecto {ProjectId}", phaseId, projectId);
            return StatusCode(500, new { message = "Error al validar fase" });
        }
    }
}

