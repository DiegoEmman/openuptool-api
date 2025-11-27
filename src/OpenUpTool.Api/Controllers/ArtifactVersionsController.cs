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
    private readonly IFileStorageService _fileStorage;
    private readonly IArtifactVersionRepository _versionRepository;
    private readonly IArtifactRepository _artifactRepository;
    private readonly IProjectService _projectService;
    private readonly ILogger<ArtifactVersionsController> _logger;

    public ArtifactVersionsController(
        IFileStorageService fileStorage,
        IArtifactVersionRepository versionRepository,
        IArtifactRepository artifactRepository,
        IProjectService projectService,
        ILogger<ArtifactVersionsController> logger)
    {
        _fileStorage = fileStorage;
        _versionRepository = versionRepository;
        _artifactRepository = artifactRepository;
        _projectService = projectService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todas las versiones de un artefacto
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ArtifactVersionDto>>> GetVersions(Guid projectId, Guid artifactId)
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

            var versions = await _versionRepository.GetByArtifactIdAsync(artifactId);
            var dtos = versions.Select(v => MapToDto(v));
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener versiones del artefacto {ArtifactId}", artifactId);
            return StatusCode(500, new { message = "Error al obtener versiones" });
        }
    }

    /// <summary>
    /// Crea una nueva versión del artefacto con archivo
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager,Developer,Autor")]
    [Consumes("multipart/form-data")]
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

            // Verificar si el usuario tiene acceso a este proyecto
            var hasAccess = await _projectService.HasUserAccessAsync(userId, projectId);
            if (!hasAccess)
                return Forbid();

            // Verificar que el artefacto existe
            var artifact = await _artifactRepository.GetByIdAsync(artifactId);
            if (artifact == null)
                return NotFound(new { message = "Artefacto no encontrado" });

            if (artifact.ProjectId != projectId)
                return BadRequest(new { message = "El artefacto no pertenece al proyecto especificado" });

            // Obtener el número de la siguiente versión
            var existingVersions = await _versionRepository.GetByArtifactIdAsync(artifactId);
            var nextVersion = existingVersions.Any() ? existingVersions.Max(v => v.VersionNumber) + 1 : 1;

            // Guardar archivo si se proporciona
            string? filePath = null;
            string? fileName = null;
            long? fileSize = null;

            if (request.File != null && request.File.Length > 0)
            {
                using var stream = request.File.OpenReadStream();
                var (savedPath, savedName, savedSize) = await _fileStorage.SaveFileAsync(projectId, artifactId, nextVersion, stream, request.File.FileName);
                filePath = savedPath;
                fileName = savedName;
                fileSize = savedSize;
            }

            // Crear entidad de versión
            var version = new ArtifactVersion
            {
                Id = Guid.NewGuid(),
                ArtifactId = artifactId,
                VersionNumber = nextVersion,
                FilePath = filePath,
                FileName = fileName,
                FileSize = fileSize,
                UploadedBy = request.UploadedBy,
                UploadedAt = DateTime.UtcNow,
                ChangeDescription = request.ChangeDescription,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var created = await _versionRepository.CreateAsync(version);
            _logger.LogInformation("Nueva versión {Version} creada para artefacto {ArtifactId}", nextVersion, artifactId);

            return CreatedAtAction(nameof(GetVersions), new { projectId, artifactId }, MapToDto(created));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear versión del artefacto {ArtifactId}", artifactId);
            return StatusCode(500, new { message = "Error al crear versión", error = ex.Message });
        }
    }

    /// <summary>
    /// Descarga el archivo de una versión específica
    /// </summary>
    [HttpGet("{versionId}/download")]
    public async Task<IActionResult> DownloadFile(Guid projectId, Guid artifactId, Guid versionId)
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

            var version = await _versionRepository.GetByIdAsync(versionId);
            if (version == null)
                return NotFound(new { message = "Versión no encontrada" });

            if (version.ArtifactId != artifactId)
                return BadRequest(new { message = "La versión no pertenece al artefacto especificado" });

            if (string.IsNullOrEmpty(version.FilePath))
                return NotFound(new { message = "Esta versión no tiene archivo asociado" });

            var stream = await _fileStorage.GetFileStreamAsync(version.FilePath);
            if (stream == null)
                return NotFound(new { message = "Archivo no encontrado en el servidor" });

            var contentType = GetContentType(version.FileName ?? "file");
            return File(stream, contentType, version.FileName ?? $"artifact_v{version.VersionNumber}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al descargar archivo de versión {VersionId}", versionId);
            return StatusCode(500, new { message = "Error al descargar archivo" });
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
