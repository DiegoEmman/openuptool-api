using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenUpTool.Core.DTOs;
using OpenUpTool.Core.Entities;
using OpenUpTool.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using System.IO.Compression;
using System.Text;

namespace OpenUpTool.Api.Controllers;

/// <summary>
/// HU-023: Controlador para exportar e importar proyectos y plantillas
/// </summary>
[ApiController]
[Route("api/export-import")]
[Authorize]
public class ExportImportController : ControllerBase
{
    private readonly OpenUpToolDbContext _context;
    private readonly ILogger<ExportImportController> _logger;

    public ExportImportController(OpenUpToolDbContext context, ILogger<ExportImportController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // ==================== EXPORT ENDPOINTS ====================

    /// <summary>
    /// Exporta un proyecto completo a JSON
    /// </summary>
    [HttpGet("projects/{projectId}/json")]
    public async Task<ActionResult<ProjectExportDto>> ExportProjectToJson(Guid projectId)
    {
        try
        {
            var export = await BuildProjectExport(projectId);
            if (export == null)
                return NotFound(new { message = "Proyecto no encontrado" });

            return Ok(export);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al exportar proyecto {ProjectId} a JSON", projectId);
            return StatusCode(500, new { message = "Error al exportar proyecto" });
        }
    }

    /// <summary>
    /// Exporta un proyecto completo a archivo ZIP (JSON + metadatos)
    /// </summary>
    [HttpGet("projects/{projectId}/zip")]
    public async Task<IActionResult> ExportProjectToZip(Guid projectId)
    {
        try
        {
            var export = await BuildProjectExport(projectId);
            if (export == null)
                return NotFound(new { message = "Proyecto no encontrado" });

            // Crear ZIP en memoria
            using var memoryStream = new MemoryStream();
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                // Archivo principal JSON
                var jsonEntry = archive.CreateEntry("project.json");
                using (var entryStream = jsonEntry.Open())
                using (var writer = new StreamWriter(entryStream))
                {
                    var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
                    var json = JsonSerializer.Serialize(export, jsonOptions);
                    await writer.WriteAsync(json);
                }

                // Archivo de metadatos
                var metaEntry = archive.CreateEntry("metadata.txt");
                using (var entryStream = metaEntry.Open())
                using (var writer = new StreamWriter(entryStream))
                {
                    await writer.WriteLineAsync($"OpenUpTool Project Export");
                    await writer.WriteLineAsync($"========================");
                    await writer.WriteLineAsync($"Project: {export.Project?.Name ?? "N/A"}");
                    await writer.WriteLineAsync($"Identifier: {export.Project?.Identifier ?? "N/A"}");
                    await writer.WriteLineAsync($"Exported At: {export.ExportedAt:yyyy-MM-dd HH:mm:ss} UTC");
                    await writer.WriteLineAsync($"Exported By: {export.ExportedBy}");
                    await writer.WriteLineAsync($"Phases: {export.Phases.Count}");
                    await writer.WriteLineAsync($"Iterations: {export.Iterations.Count}");
                    await writer.WriteLineAsync($"Artifacts: {export.Artifacts.Count}");
                    await writer.WriteLineAsync($"Versions: {export.ArtifactVersions.Count}");
                }

                // README
                var readmeEntry = archive.CreateEntry("README.md");
                using (var entryStream = readmeEntry.Open())
                using (var writer = new StreamWriter(entryStream))
                {
                    await writer.WriteLineAsync($"# {export.Project?.Name ?? "Proyecto"}");
                    await writer.WriteLineAsync("");
                    await writer.WriteLineAsync($"**Descripción:** {export.Project?.Description ?? "Sin descripción"}");
                    await writer.WriteLineAsync("");
                    await writer.WriteLineAsync($"## Información del Proyecto");
                    await writer.WriteLineAsync($"- **Identificador:** {export.Project?.Identifier ?? "N/A"}");
                    await writer.WriteLineAsync($"- **Estado:** {export.Project?.Status ?? "N/A"}");
                    await writer.WriteLineAsync($"- **Fecha Inicio:** {export.Project?.StartDate:yyyy-MM-dd}");
                    await writer.WriteLineAsync($"- **Tags:** {string.Join(", ", export.Project?.Tags ?? new List<string>())}");
                    await writer.WriteLineAsync("");
                    await writer.WriteLineAsync($"## Contenido");
                    await writer.WriteLineAsync($"- {export.Phases.Count} fases");
                    await writer.WriteLineAsync($"- {export.Iterations.Count} iteraciones");
                    await writer.WriteLineAsync($"- {export.Artifacts.Count} artefactos");
                    await writer.WriteLineAsync($"- {export.ArtifactVersions.Count} versiones de artefactos");
                }
            }

            memoryStream.Position = 0;
            var fileName = $"{export.Project?.Identifier ?? "project"}_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.zip";
            
            return File(memoryStream.ToArray(), "application/zip", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al exportar proyecto {ProjectId} a ZIP", projectId);
            return StatusCode(500, new { message = "Error al exportar proyecto" });
        }
    }

    /// <summary>
    /// Exporta un proyecto a formato base64 (para transferencia)
    /// </summary>
    [HttpGet("projects/{projectId}/base64")]
    public async Task<ActionResult<ExportResultDto>> ExportProjectToBase64(Guid projectId)
    {
        try
        {
            var export = await BuildProjectExport(projectId);
            if (export == null)
                return NotFound(new { message = "Proyecto no encontrado" });

            var jsonOptions = new JsonSerializerOptions { WriteIndented = false };
            var json = JsonSerializer.Serialize(export, jsonOptions);
            var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

            return Ok(new ExportResultDto(
                Success: true,
                Format: "base64",
                FileName: $"{export.Project?.Identifier ?? "project"}_export.json",
                ContentBase64: base64,
                DownloadUrl: null,
                ItemsExported: export.Artifacts.Count + export.Iterations.Count + export.Phases.Count,
                Message: "Proyecto exportado correctamente"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al exportar proyecto {ProjectId} a base64", projectId);
            return StatusCode(500, new { message = "Error al exportar proyecto" });
        }
    }

    /// <summary>
    /// Lista los formatos de exportación disponibles
    /// </summary>
    [HttpGet("formats")]
    public ActionResult<object> GetExportFormats()
    {
        return Ok(new
        {
            exportFormats = new[]
            {
                new { format = "json", description = "JSON estructurado", contentType = "application/json" },
                new { format = "zip", description = "Archivo ZIP con JSON y metadatos", contentType = "application/zip" },
                new { format = "base64", description = "JSON codificado en Base64", contentType = "text/plain" }
            },
            importFormats = new[]
            {
                new { format = "json", description = "JSON de proyecto exportado" },
                new { format = "template", description = "Plantilla OpenUP en JSON" }
            }
        });
    }

    // ==================== IMPORT ENDPOINTS ====================

    /// <summary>
    /// Importa un proyecto desde JSON exportado
    /// </summary>
    [HttpPost("projects/import")]
    public async Task<ActionResult<ImportResultDto>> ImportProject([FromBody] ProjectImportDto dto)
    {
        try
        {
            if (dto.SourceData == null)
                return BadRequest(new { message = "Se requiere SourceData con los datos del proyecto" });

            var warnings = new List<string>();
            var userId = GetCurrentUserEmail();

            // Crear nuevo proyecto
            var project = new Project
            {
                Id = Guid.NewGuid(),
                Name = dto.Name ?? dto.SourceData.Project?.Name ?? "Imported Project",
                Identifier = GenerateUniqueIdentifier(dto.SourceData.Project?.Identifier ?? "IMPORTED"),
                Description = dto.Description ?? dto.SourceData.Project?.Description,
                StartDate = DateTime.UtcNow,
                Status = "Creado",
                Tags = dto.Tags ?? dto.SourceData.Project?.Tags ?? new List<string>(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            // Asignar el proyecto al usuario que lo importa
            var currentUserId = GetCurrentUserId();
            var managerRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Manager");
            if (managerRole != null && currentUserId != Guid.Empty)
            {
                var projectUserRole = new ProjectUserRole
                {
                    Id = Guid.NewGuid(),
                    ProjectId = project.Id,
                    UserId = currentUserId,
                    RoleId = managerRole.Id,
                    Status = "active",
                    AcceptedAt = DateTime.UtcNow,
                    InvitedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.ProjectUserRoles.Add(projectUserRole);
                await _context.SaveChangesAsync();
            }

            int phasesImported = 0, iterationsImported = 0, artifactsImported = 0, versionsImported = 0;

            // Importar fases
            if (dto.ImportPhases && dto.SourceData.Phases.Any())
            {
                foreach (var phaseDto in dto.SourceData.Phases.OrderBy(p => p.OrderIndex))
                {
                    var phase = new Phase
                    {
                        Id = Guid.NewGuid(),
                        ProjectId = project.Id,
                        PhaseCode = phaseDto.PhaseCode,
                        Name = phaseDto.Name,
                        OrderIndex = phaseDto.OrderIndex,
                        Status = "PENDING",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.Phases.Add(phase);
                    phasesImported++;
                }
                await _context.SaveChangesAsync();
            }

            // Importar iteraciones
            if (dto.ImportIterations && dto.SourceData.Iterations.Any())
            {
                int iterNumber = 1;
                foreach (var iterDto in dto.SourceData.Iterations.OrderBy(i => i.StartDate))
                {
                    var iteration = new Iteration
                    {
                        Id = Guid.NewGuid(),
                        ProjectId = project.Id,
                        Name = iterDto.Name,
                        Phase = iterDto.Phase,
                        Objective = iterDto.Objective,
                        StartDate = DateTime.UtcNow.AddDays((iterNumber - 1) * 14),
                        EndDate = DateTime.UtcNow.AddDays(iterNumber * 14),
                        Status = "Planeada",
                        PlannedCapacityHours = iterDto.PlannedCapacityHours,
                        TeamSize = iterDto.TeamSize,
                        PlannedPoints = iterDto.PlannedPoints,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.Iterations.Add(iteration);
                    iterNumber++;
                    iterationsImported++;
                }
                await _context.SaveChangesAsync();
            }

            // Importar artefactos
            if (dto.ImportArtifacts && dto.SourceData.Artifacts.Any())
            {
                foreach (var artDto in dto.SourceData.Artifacts)
                {
                    var artifact = new Artifact
                    {
                        Id = Guid.NewGuid(),
                        ProjectId = project.Id,
                        PhaseId = artDto.PhaseId,
                        ArtifactTypeId = artDto.ArtifactTypeId,
                        Title = artDto.Title,
                        Description = artDto.Description,
                        ContentText = artDto.ContentText,
                        Status = "Pendiente",
                        Author = userId,
                        IsMandatory = artDto.IsMandatory,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.Artifacts.Add(artifact);
                    artifactsImported++;
                }
                await _context.SaveChangesAsync();
            }

            // Nota: El historial de versiones no se importa porque depende de archivos físicos
            if (dto.ImportVersionHistory && dto.SourceData.ArtifactVersions.Any())
            {
                warnings.Add($"El historial de versiones ({dto.SourceData.ArtifactVersions.Count} versiones) no se importó porque requiere archivos físicos");
            }

            return Ok(new ImportResultDto(
                Success: true,
                ProjectId: project.Id,
                ProjectName: project.Name,
                PhasesImported: phasesImported,
                IterationsImported: iterationsImported,
                ArtifactsImported: artifactsImported,
                VersionsImported: versionsImported,
                Warnings: warnings,
                Message: "Proyecto importado correctamente"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al importar proyecto");
            return StatusCode(500, new { message = $"Error al importar: {ex.Message}" });
        }
    }

    /// <summary>
    /// Importa una plantilla OpenUP y crea un proyecto
    /// </summary>
    [HttpPost("templates/import")]
    public async Task<ActionResult<ImportResultDto>> ImportTemplate([FromBody] TemplateProjectImportDto dto)
    {
        try
        {
            var warnings = new List<string>();

            // Crear proyecto desde plantilla
            var project = new Project
            {
                Id = Guid.NewGuid(),
                Name = dto.ProjectName,
                Identifier = GenerateUniqueIdentifier(dto.ProjectName.Replace(" ", "_").ToUpper()),
                Description = dto.ProjectDescription,
                StartDate = dto.StartDate ?? DateTime.UtcNow,
                Status = "Creado",
                Tags = new List<string> { "imported", "template" },
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            // Asignar el proyecto al usuario que lo importa
            var currentUserId = GetCurrentUserId();
            var managerRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Manager");
            if (managerRole != null && currentUserId != Guid.Empty)
            {
                var projectUserRole = new ProjectUserRole
                {
                    Id = Guid.NewGuid(),
                    ProjectId = project.Id,
                    UserId = currentUserId,
                    RoleId = managerRole.Id,
                    Status = "active",
                    AcceptedAt = DateTime.UtcNow,
                    InvitedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.ProjectUserRoles.Add(projectUserRole);
                await _context.SaveChangesAsync();
            }

            int phasesImported = 0, iterationsImported = 0, artifactsImported = 0;
            var phaseCodeMap = new Dictionary<string, string>();

            // Importar fases de la plantilla
            if (dto.Template.Phases?.Any() == true)
            {
                foreach (var phaseDto in dto.Template.Phases.OrderBy(p => p.OrderIndex))
                {
                    var phaseCode = phaseDto.Name.ToUpper().Replace(" ", "_");
                    var phase = new Phase
                    {
                        Id = Guid.NewGuid(),
                        ProjectId = project.Id,
                        PhaseCode = phaseCode,
                        Name = phaseDto.Name,
                        OrderIndex = phaseDto.OrderIndex,
                        Status = "PENDING",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.Phases.Add(phase);
                    phaseCodeMap[phaseDto.Name] = phaseCode;
                    phasesImported++;
                }
                await _context.SaveChangesAsync();
            }

            // Importar iteraciones por defecto
            if (dto.Template.DefaultIterations?.Any() == true)
            {
                var startDate = project.StartDate;
                foreach (var iterDto in dto.Template.DefaultIterations.OrderBy(i => i.Number))
                {
                    var phaseCode = phaseCodeMap.ContainsKey(iterDto.PhaseName) 
                        ? phaseCodeMap[iterDto.PhaseName] 
                        : iterDto.PhaseName.ToUpper();

                    var iteration = new Iteration
                    {
                        Id = Guid.NewGuid(),
                        ProjectId = project.Id,
                        Name = iterDto.Name,
                        Phase = phaseCode,
                        StartDate = startDate,
                        EndDate = startDate.AddDays(iterDto.DurationDays),
                        Status = "Planeada",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.Iterations.Add(iteration);
                    startDate = startDate.AddDays(iterDto.DurationDays);
                    iterationsImported++;
                }
                await _context.SaveChangesAsync();
            }

            // Crear artefactos vacíos para tipos obligatorios
            if (dto.Template.ArtifactTypes?.Any() == true)
            {
                var existingTypes = await _context.ArtifactTypes.ToListAsync();
                
                foreach (var artType in dto.Template.ArtifactTypes.Where(a => a.IsMandatory))
                {
                    var artifactType = existingTypes.FirstOrDefault(t => 
                        t.Code == artType.Code || t.Name.Contains(artType.Name, StringComparison.OrdinalIgnoreCase));
                    
                    if (artifactType == null)
                    {
                        warnings.Add($"Tipo de artefacto '{artType.Code}' no encontrado, se omitió");
                        continue;
                    }

                    var phaseCode = phaseCodeMap.ContainsKey(artType.Phase) 
                        ? phaseCodeMap[artType.Phase] 
                        : artType.Phase.ToUpper();

                    var artifact = new Artifact
                    {
                        Id = Guid.NewGuid(),
                        ProjectId = project.Id,
                        PhaseId = phaseCode,
                        ArtifactTypeId = artifactType.Id,
                        Title = artType.Name,
                        Description = artType.Description,
                        Status = "Pendiente",
                        IsMandatory = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.Artifacts.Add(artifact);
                    artifactsImported++;
                }
                await _context.SaveChangesAsync();
            }

            return Ok(new ImportResultDto(
                Success: true,
                ProjectId: project.Id,
                ProjectName: project.Name,
                PhasesImported: phasesImported,
                IterationsImported: iterationsImported,
                ArtifactsImported: artifactsImported,
                VersionsImported: 0,
                Warnings: warnings,
                Message: $"Proyecto creado desde plantilla '{dto.Template.TemplateName}'"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al importar plantilla");
            return StatusCode(500, new { message = $"Error al importar plantilla: {ex.Message}" });
        }
    }

    /// <summary>
    /// Valida un archivo JSON de importación sin importarlo
    /// </summary>
    [HttpPost("validate")]
    public ActionResult<object> ValidateImportData([FromBody] ProjectExportDto data)
    {
        try
        {
            var errors = new List<string>();
            var warnings = new List<string>();

            // Validar estructura básica
            if (data.Project == null)
                errors.Add("Falta información del proyecto");
            else
            {
                if (string.IsNullOrEmpty(data.Project.Name))
                    errors.Add("El proyecto debe tener nombre");
                if (string.IsNullOrEmpty(data.Project.Identifier))
                    warnings.Add("El identificador será generado automáticamente");
            }

            // Validar versión de exportación
            if (string.IsNullOrEmpty(data.ExportVersion))
                warnings.Add("No se especifica versión de exportación");

            // Validar referencias
            var phaseCodes = data.Phases?.Select(p => p.PhaseCode).ToHashSet() ?? new HashSet<string>();
            var artifactIds = data.Artifacts?.Select(a => a.Id).ToHashSet() ?? new HashSet<Guid>();

            // Verificar referencias de iteraciones a fases
            foreach (var iter in data.Iterations ?? Enumerable.Empty<IterationExportDto>())
            {
                if (!string.IsNullOrEmpty(iter.Phase) && !phaseCodes.Contains(iter.Phase))
                    warnings.Add($"Iteración '{iter.Name}' referencia fase inexistente: {iter.Phase}");
            }

            // Verificar referencias de artefactos a fases
            foreach (var art in data.Artifacts ?? Enumerable.Empty<ArtifactExportDto>())
            {
                if (!string.IsNullOrEmpty(art.PhaseId) && !phaseCodes.Contains(art.PhaseId))
                    warnings.Add($"Artefacto '{art.Title}' referencia fase inexistente: {art.PhaseId}");
            }

            // Verificar versiones de artefactos
            foreach (var ver in data.ArtifactVersions ?? Enumerable.Empty<ArtifactVersionExportDto>())
            {
                if (!artifactIds.Contains(ver.ArtifactId))
                    warnings.Add($"Versión {ver.VersionNumber} referencia artefacto inexistente");
            }

            return Ok(new
            {
                valid = !errors.Any(),
                errors,
                warnings,
                summary = new
                {
                    projectName = data.Project?.Name,
                    phases = data.Phases?.Count ?? 0,
                    iterations = data.Iterations?.Count ?? 0,
                    artifacts = data.Artifacts?.Count ?? 0,
                    versions = data.ArtifactVersions?.Count ?? 0
                }
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { valid = false, errors = new[] { $"Error al validar: {ex.Message}" } });
        }
    }

    /// <summary>
    /// Obtiene una plantilla OpenUP estándar
    /// </summary>
    [HttpGet("templates/openup-standard")]
    public ActionResult<TemplateDefinitionDto> GetOpenUpStandardTemplate()
    {
        var template = new TemplateDefinitionDto(
            TemplateName: "OpenUP Standard",
            TemplateDescription: "Plantilla estándar de OpenUP con las 4 fases y artefactos obligatorios",
            Phases: new List<TemplatePhaseDto>
            {
                new("Inception", "Fase de inicio - Definir alcance y visión", 1),
                new("Elaboration", "Fase de elaboración - Arquitectura y riesgos", 2),
                new("Construction", "Fase de construcción - Desarrollo iterativo", 3),
                new("Transition", "Fase de transición - Entrega y despliegue", 4)
            },
            ArtifactTypes: new List<TemplateArtifactTypeDto>
            {
                new("Vision Document", "VISION", "Documento de visión del proyecto", "Inception", true),
                new("Project Plan", "PLAN", "Plan de proyecto", "Inception", true),
                new("Risk List", "RISKS", "Lista de riesgos", "Inception", true),
                new("Use Case Model", "UCM", "Modelo de casos de uso", "Elaboration", true),
                new("Architecture Notebook", "ARCH", "Cuaderno de arquitectura", "Elaboration", true),
                new("Test Plan", "TEST", "Plan de pruebas", "Construction", true),
                new("User Manual", "MANUAL", "Manual de usuario", "Transition", true),
                new("Release Notes", "RELEASE", "Notas de versión", "Transition", true)
            },
            DefaultIterations: new List<TemplateIterationDto>
            {
                new("Inception 1", 1, "Inception", 14),
                new("Elaboration 1", 2, "Elaboration", 14),
                new("Elaboration 2", 3, "Elaboration", 14),
                new("Construction 1", 4, "Construction", 14),
                new("Construction 2", 5, "Construction", 14),
                new("Construction 3", 6, "Construction", 14),
                new("Transition 1", 7, "Transition", 14)
            }
        );

        return Ok(template);
    }

    // ==================== HELPER METHODS ====================

    private async Task<ProjectExportDto?> BuildProjectExport(Guid projectId)
    {
        var project = await _context.Projects
            .Include(p => p.Plan)
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project == null)
            return null;

        var phases = await _context.Phases
            .Where(p => p.ProjectId == projectId)
            .OrderBy(p => p.OrderIndex)
            .ToListAsync();

        var iterations = await _context.Iterations
            .Where(i => i.ProjectId == projectId)
            .OrderBy(i => i.StartDate)
            .ToListAsync();

        var artifacts = await _context.Artifacts
            .Where(a => a.ProjectId == projectId)
            .ToListAsync();

        var artifactIds = artifacts.Select(a => a.Id).ToList();
        var versions = await _context.ArtifactVersions
            .Where(v => artifactIds.Contains(v.ArtifactId))
            .OrderBy(v => v.CreatedAt)
            .ToListAsync();

        return new ProjectExportDto(
            ExportVersion: "1.0",
            ExportedAt: DateTime.UtcNow,
            ExportedBy: GetCurrentUserEmail(),
            Project: new ProjectExportDataDto(
                project.Id,
                project.Name,
                project.Identifier,
                project.StartDate,
                project.Status,
                project.Owner,
                project.Description,
                project.Tags,
                project.CreatedAt
            ),
            Phases: phases.Select(p => new PhaseExportDto(
                p.Id,
                p.PhaseCode,
                p.Name,
                p.OrderIndex,
                p.StartDate,
                p.EndDate,
                p.Status
            )).ToList(),
            Iterations: iterations.Select(i => new IterationExportDto(
                i.Id,
                i.Name,
                i.Objective,
                i.Phase,
                i.StartDate,
                i.EndDate,
                i.Status,
                i.PlannedCapacityHours,
                i.TeamSize,
                i.PlannedPoints,
                i.CompletedPoints
            )).ToList(),
            Artifacts: artifacts.Select(a => new ArtifactExportDto(
                a.Id,
                a.Title,
                a.ArtifactTypeId,
                a.Description,
                a.ContentText,
                a.Status,
                a.PhaseId,
                a.Author,
                a.IsMandatory,
                a.CreatedAt
            )).ToList(),
            ArtifactVersions: versions.Select(v => new ArtifactVersionExportDto(
                v.Id,
                v.ArtifactId,
                v.VersionNumber,
                v.FilePath,
                v.FileName,
                v.UploadedBy,
                v.ChangeDescription,
                v.CreatedAt
            )).ToList(),
            Plan: project.Plan != null ? new ProjectPlanExportDto(
                project.Plan.Id,
                project.Plan.Objectives,
                project.Plan.Scope,
                project.Plan.Observations,
                project.Plan.Version,
                project.Plan.IsActive
            ) : null
        );
    }

    private string GenerateUniqueIdentifier(string baseIdentifier)
    {
        var clean = new string(baseIdentifier.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());
        var suffix = DateTime.UtcNow.ToString("yyyyMMddHHmm");
        return $"{clean}_{suffix}";
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }

    private string GetCurrentUserEmail()
    {
        return User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue(ClaimTypes.Name) ?? "unknown";
    }
}
