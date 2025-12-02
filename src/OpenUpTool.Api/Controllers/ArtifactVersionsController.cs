using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenUpTool.Api.Models;
using OpenUpTool.Core.DTOs;
using OpenUpTool.Core.Entities;
using OpenUpTool.Core.Interfaces;
using System.Security.Claims;

namespace OpenUpTool.Api.Controllers;

[ApiController]
[Route("api/projects/{projectId}/artifacts/{artifactId}/versions")]
[Authorize]
public class ArtifactVersionsController : ControllerBase
{
    private readonly IArtifactVersionService _versionService;
    private readonly IProjectService _projectService;
    private readonly ILogger<ArtifactVersionsController> _logger;

    public ArtifactVersionsController(
        IArtifactVersionService versionService,
        IProjectService projectService,
        ILogger<ArtifactVersionsController> logger)
    {
        _versionService = versionService;
        _projectService = projectService;
        _logger = logger;
    }

    /// <summary>
    /// Crea una nueva versión de un artefacto
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ArtifactVersionDto>> CreateVersion(
        Guid projectId, 
        Guid artifactId,
        [FromForm] CreateArtifactVersionRequest request)
    {
        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var hasAccess = await _projectService.HasUserAccessAsync(userId, projectId);
            if (!hasAccess)
                return Forbid();

            var dto = new CreateArtifactVersionDto(
                ChangeDescription: request.ChangeDescription,
                UploadedBy: userId.ToString()
            );

            Stream? fileStream = null;
            string? fileName = null;

            if (request.File != null)
            {
                fileStream = request.File.OpenReadStream();
                fileName = request.File.FileName;
            }

            var version = await _versionService.CreateVersionAsync(artifactId, dto, fileStream, fileName);

            return Ok(version);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear versión para artefacto {ArtifactId}", artifactId);
            return StatusCode(500, new { message = "Error al crear versión" });
        }
    }

    /// <summary>
    /// Obtiene el historial completo de versiones de un artefacto
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<VersionHistoryDto>> GetVersionHistory(Guid projectId, Guid artifactId)
    {
        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var hasAccess = await _projectService.HasUserAccessAsync(userId, projectId);
            if (!hasAccess)
                return Forbid();

            var history = await _versionService.GetVersionHistoryAsync(artifactId);
            return Ok(history);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener historial de versiones del artefacto {ArtifactId}", artifactId);
            return StatusCode(500, new { message = "Error al obtener historial" });
        }
    }

    /// <summary>
    /// Obtiene una versión específica por su ID
    /// </summary>
    [HttpGet("{versionId}")]
    public async Task<ActionResult<ArtifactVersionDto>> GetVersion(Guid projectId, Guid artifactId, Guid versionId)
    {
        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var hasAccess = await _projectService.HasUserAccessAsync(userId, projectId);
            if (!hasAccess)
                return Forbid();

            var version = await _versionService.GetVersionByIdAsync(versionId);
            if (version == null)
                return NotFound(new { message = "Versión no encontrada" });

            if (version.ArtifactId != artifactId)
                return BadRequest(new { message = "La versión no pertenece al artefacto especificado" });

            return Ok(version);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener versión {VersionId}", versionId);
            return StatusCode(500, new { message = "Error al obtener versión" });
        }
    }

    /// <summary>
    /// Compara dos versiones de un artefacto (metadatos)
    /// </summary>
    [HttpGet("compare")]
    public async Task<ActionResult<VersionComparisonDto>> CompareVersions(
        Guid projectId, 
        Guid artifactId,
        [FromQuery] Guid v1,
        [FromQuery] Guid v2)
    {
        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var hasAccess = await _projectService.HasUserAccessAsync(userId, projectId);
            if (!hasAccess)
                return Forbid();

            var comparison = await _versionService.CompareVersionsAsync(v1, v2);
            if (comparison == null)
                return NotFound(new { message = "Una o ambas versiones no existen" });

            return Ok(comparison);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al comparar versiones {V1} y {V2}", v1, v2);
            return StatusCode(500, new { message = "Error al comparar versiones" });
        }
    }

    /// <summary>
    /// Descarga el archivo de una versión específica
    /// </summary>
    [HttpGet("{versionId}/download")]
    public async Task<IActionResult> DownloadVersion(Guid projectId, Guid artifactId, Guid versionId)
    {
        try
        {
            var stream = await _versionService.GetVersionFileAsync(versionId);
            if (stream == null)
                return NotFound($"Archivo de versión {versionId} no encontrado");

            var version = await _versionService.GetVersionByIdAsync(versionId);
            if (version == null)
                return NotFound($"Versión {versionId} no encontrada");

            return File(stream, "application/octet-stream", version.FileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al descargar archivo de versión {VersionId}", versionId);
            return StatusCode(500, new { message = "Error al descargar archivo", error = ex.Message });
        }
    }

    private static ArtifactVersionDto MapToDto(ArtifactVersion version)
    {
        return new ArtifactVersionDto(
            version.Id,
            version.ArtifactId,
            version.VersionNumber,
            version.FilePath,
            version.FileName,
            version.FileSize,
            version.UploadedBy,
            version.UploadedAt,
            version.ChangeDescription,
            version.CreatedAt
        );
    }

    private static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".txt" => "text/plain",
            ".zip" => "application/zip",
            _ => "application/octet-stream"
        };
    }
}
