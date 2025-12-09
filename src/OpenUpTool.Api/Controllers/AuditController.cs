using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenUpTool.Core.DTOs;
using OpenUpTool.Core.Interfaces;
using OpenUpTool.Infrastructure.Data;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace OpenUpTool.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AuditController : ControllerBase
{
    private readonly IAuditLogService _auditLogService;
    private readonly OpenUpToolDbContext _context;
    private readonly ILogger<AuditController> _logger;

    public AuditController(
        IAuditLogService auditLogService,
        OpenUpToolDbContext context,
        ILogger<AuditController> logger)
    {
        _auditLogService = auditLogService;
        _context = context;
        _logger = logger;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    /// <summary>
    /// Obtiene los tipos de acciones disponibles para filtrar
    /// </summary>
    [HttpGet("action-types")]
    public ActionResult<IEnumerable<string>> GetActionTypes()
    {
        var actionTypes = new List<string>
        {
            "ProjectCreated",
            "ProjectUpdated",
            "ProjectArchived",
            "ProjectRestored",
            "ProjectDeleted",
            "ArtifactCreated",
            "ArtifactUpdated",
            "ArtifactDeleted",
            "ArtifactMoved",
            "ArtifactStatusChanged",
            "VersionCreated",
            "VersionApproved",
            "VersionRejected",
            "IterationCreated",
            "IterationUpdated",
            "IterationStatusChanged",
            "PhaseCreated",
            "PhaseUpdated",
            "PhaseStatusChanged",
            "UserInvited",
            "UserRoleChanged",
            "UserRemoved",
            "Import",
            "Export"
        };

        return Ok(actionTypes);
    }

    /// <summary>
    /// Obtiene los tipos de entidades disponibles para filtrar
    /// </summary>
    [HttpGet("entity-types")]
    public ActionResult<IEnumerable<string>> GetEntityTypes()
    {
        var entityTypes = new List<string>
        {
            "Project",
            "Artifact",
            "ArtifactVersion",
            "Iteration",
            "Phase",
            "User",
            "ProjectUserRole"
        };

        return Ok(entityTypes);
    }

    /// <summary>
    /// Obtiene el historial de auditoría con filtros opcionales
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<AuditQueryResultDto>> GetAuditLogs(
        [FromQuery] Guid? projectId = null,
        [FromQuery] Guid? userId = null,
        [FromQuery] string? entityType = null,
        [FromQuery] Guid? entityId = null,
        [FromQuery] string? action = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            var query = _context.AuditLogs
                .Include(a => a.User)
                .AsQueryable();

            // Filtrar por proyecto si se especifica
            if (projectId.HasValue)
            {
                // Obtener todas las entidades relacionadas con el proyecto
                var projectEntityIds = new List<Guid> { projectId.Value };
                
                // Agregar IDs de fases del proyecto
                var phaseIds = await _context.Phases
                    .Where(p => p.ProjectId == projectId.Value)
                    .Select(p => p.Id)
                    .ToListAsync();
                projectEntityIds.AddRange(phaseIds);

                // Agregar IDs de iteraciones del proyecto
                var iterationIds = await _context.Iterations
                    .Where(i => i.ProjectId == projectId.Value)
                    .Select(i => i.Id)
                    .ToListAsync();
                projectEntityIds.AddRange(iterationIds);

                // Agregar IDs de artefactos del proyecto
                var artifactIds = await _context.Artifacts
                    .Where(a => a.ProjectId == projectId.Value)
                    .Select(a => a.Id)
                    .ToListAsync();
                projectEntityIds.AddRange(artifactIds);

                // Agregar IDs de versiones de artefactos
                var versionIds = await _context.ArtifactVersions
                    .Where(v => artifactIds.Contains(v.ArtifactId))
                    .Select(v => v.Id)
                    .ToListAsync();
                projectEntityIds.AddRange(versionIds);

                query = query.Where(a => a.EntityId.HasValue && projectEntityIds.Contains(a.EntityId.Value));
            }

            if (userId.HasValue)
                query = query.Where(a => a.UserId == userId.Value);

            if (!string.IsNullOrEmpty(entityType))
                query = query.Where(a => a.EntityType == entityType);

            if (entityId.HasValue)
                query = query.Where(a => a.EntityId == entityId.Value);

            if (!string.IsNullOrEmpty(action))
                query = query.Where(a => a.Action.Contains(action));

            if (fromDate.HasValue)
                query = query.Where(a => a.CreatedAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(a => a.CreatedAt <= toDate.Value.AddDays(1));

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var logs = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new AuditLogDto
                {
                    Id = a.Id,
                    UserId = a.UserId,
                    UserEmail = a.User != null ? a.User.Email : "Unknown",
                    UserName = a.User != null ? $"{a.User.FirstName} {a.User.LastName}".Trim() : "Unknown",
                    Action = a.Action,
                    EntityType = a.EntityType,
                    EntityId = a.EntityId,
                    Details = a.Details,
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync();

            return Ok(new AuditQueryResultDto
            {
                Logs = logs,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener historial de auditoría");
            return StatusCode(500, new { message = "Error al obtener historial de auditoría" });
        }
    }

    /// <summary>
    /// Obtiene el historial de auditoría de un proyecto específico
    /// </summary>
    [HttpGet("projects/{projectId}")]
    public async Task<ActionResult<AuditQueryResultDto>> GetProjectAuditLogs(
        Guid projectId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            // Verificar que el proyecto existe
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null)
                return NotFound(new { message = "Proyecto no encontrado" });

            return await GetAuditLogs(projectId: projectId, page: page, pageSize: pageSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener auditoría del proyecto {ProjectId}", projectId);
            return StatusCode(500, new { message = "Error al obtener auditoría del proyecto" });
        }
    }

    /// <summary>
    /// Obtiene el historial de auditoría de una entidad específica
    /// </summary>
    [HttpGet("entities/{entityType}/{entityId}")]
    public async Task<ActionResult<IEnumerable<AuditLogDto>>> GetEntityAuditLogs(
        string entityType,
        Guid entityId)
    {
        try
        {
            var logs = await _context.AuditLogs
                .Include(a => a.User)
                .Where(a => a.EntityType == entityType && a.EntityId == entityId)
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new AuditLogDto
                {
                    Id = a.Id,
                    UserId = a.UserId,
                    UserEmail = a.User != null ? a.User.Email : "Unknown",
                    UserName = a.User != null ? $"{a.User.FirstName} {a.User.LastName}".Trim() : "Unknown",
                    Action = a.Action,
                    EntityType = a.EntityType,
                    EntityId = a.EntityId,
                    Details = a.Details,
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync();

            return Ok(logs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener auditoría de entidad {EntityType}/{EntityId}", entityType, entityId);
            return StatusCode(500, new { message = "Error al obtener auditoría de la entidad" });
        }
    }

    /// <summary>
    /// Exporta el historial de auditoría en formato JSON
    /// </summary>
    [HttpGet("export/json")]
    public async Task<ActionResult> ExportToJson(
        [FromQuery] Guid? projectId = null,
        [FromQuery] Guid? userId = null,
        [FromQuery] string? entityType = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var query = _context.AuditLogs
                .Include(a => a.User)
                .AsQueryable();

            if (projectId.HasValue)
            {
                var projectEntityIds = await GetProjectEntityIds(projectId.Value);
                query = query.Where(a => a.EntityId.HasValue && projectEntityIds.Contains(a.EntityId.Value));
            }

            if (userId.HasValue)
                query = query.Where(a => a.UserId == userId.Value);

            if (!string.IsNullOrEmpty(entityType))
                query = query.Where(a => a.EntityType == entityType);

            if (fromDate.HasValue)
                query = query.Where(a => a.CreatedAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(a => a.CreatedAt <= toDate.Value.AddDays(1));

            var logs = await query
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new AuditLogExportDto
                {
                    Id = a.Id,
                    UserId = a.UserId,
                    UserEmail = a.User != null ? a.User.Email : "Unknown",
                    UserName = a.User != null ? $"{a.User.FirstName} {a.User.LastName}".Trim() : "Unknown",
                    Action = a.Action,
                    EntityType = a.EntityType,
                    EntityId = a.EntityId,
                    Details = a.Details,
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync();

            var exportData = new AuditExportDto
            {
                ExportedAt = DateTime.UtcNow,
                ExportedBy = User.FindFirstValue(ClaimTypes.Email) ?? "Unknown",
                TotalRecords = logs.Count,
                Filters = new AuditExportFiltersDto
                {
                    ProjectId = projectId,
                    UserId = userId,
                    EntityType = entityType,
                    FromDate = fromDate,
                    ToDate = toDate
                },
                Logs = logs
            };

            var json = JsonSerializer.Serialize(exportData, new JsonSerializerOptions 
            { 
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var fileName = $"audit_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json";
            return File(Encoding.UTF8.GetBytes(json), "application/json", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al exportar auditoría a JSON");
            return StatusCode(500, new { message = "Error al exportar auditoría" });
        }
    }

    /// <summary>
    /// Exporta el historial de auditoría en formato CSV
    /// </summary>
    [HttpGet("export/csv")]
    public async Task<ActionResult> ExportToCsv(
        [FromQuery] Guid? projectId = null,
        [FromQuery] Guid? userId = null,
        [FromQuery] string? entityType = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var query = _context.AuditLogs
                .Include(a => a.User)
                .AsQueryable();

            if (projectId.HasValue)
            {
                var projectEntityIds = await GetProjectEntityIds(projectId.Value);
                query = query.Where(a => a.EntityId.HasValue && projectEntityIds.Contains(a.EntityId.Value));
            }

            if (userId.HasValue)
                query = query.Where(a => a.UserId == userId.Value);

            if (!string.IsNullOrEmpty(entityType))
                query = query.Where(a => a.EntityType == entityType);

            if (fromDate.HasValue)
                query = query.Where(a => a.CreatedAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(a => a.CreatedAt <= toDate.Value.AddDays(1));

            var logs = await query
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            var csv = new StringBuilder();
            csv.AppendLine("Id,UserId,UserEmail,UserName,Action,EntityType,EntityId,Details,CreatedAt");

            foreach (var log in logs)
            {
                var userName = log.User != null ? $"{log.User.FirstName} {log.User.LastName}".Trim() : "Unknown";
                var userEmail = log.User?.Email ?? "Unknown";
                var details = log.Details?.Replace("\"", "\"\"").Replace("\n", " ") ?? "";
                
                csv.AppendLine($"\"{log.Id}\",\"{log.UserId}\",\"{userEmail}\",\"{userName}\",\"{log.Action}\",\"{log.EntityType}\",\"{log.EntityId}\",\"{details}\",\"{log.CreatedAt:yyyy-MM-dd HH:mm:ss}\"");
            }

            var fileName = $"audit_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al exportar auditoría a CSV");
            return StatusCode(500, new { message = "Error al exportar auditoría" });
        }
    }

    /// <summary>
    /// Obtiene estadísticas de auditoría
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult<AuditStatsDto>> GetStats(
        [FromQuery] Guid? projectId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var query = _context.AuditLogs.AsQueryable();

            if (projectId.HasValue)
            {
                var projectEntityIds = await GetProjectEntityIds(projectId.Value);
                query = query.Where(a => a.EntityId.HasValue && projectEntityIds.Contains(a.EntityId.Value));
            }

            if (fromDate.HasValue)
                query = query.Where(a => a.CreatedAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(a => a.CreatedAt <= toDate.Value.AddDays(1));

            var totalActions = await query.CountAsync();

            var actionsByType = await query
                .GroupBy(a => a.Action)
                .Select(g => new ActionCountDto { Action = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToListAsync();

            var actionsByUser = await query
                .Include(a => a.User)
                .GroupBy(a => new { a.UserId, Email = a.User != null ? a.User.Email : "Unknown" })
                .Select(g => new UserActionCountDto 
                { 
                    UserId = g.Key.UserId, 
                    UserEmail = g.Key.Email, 
                    Count = g.Count() 
                })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToListAsync();

            var actionsByEntityType = await query
                .GroupBy(a => a.EntityType)
                .Select(g => new EntityTypeCountDto { EntityType = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            var recentActivity = await query
                .OrderByDescending(a => a.CreatedAt)
                .Take(10)
                .Include(a => a.User)
                .Select(a => new AuditLogDto
                {
                    Id = a.Id,
                    UserId = a.UserId,
                    UserEmail = a.User != null ? a.User.Email : "Unknown",
                    UserName = a.User != null ? $"{a.User.FirstName} {a.User.LastName}".Trim() : "Unknown",
                    Action = a.Action,
                    EntityType = a.EntityType,
                    EntityId = a.EntityId,
                    Details = a.Details,
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync();

            return Ok(new AuditStatsDto
            {
                TotalActions = totalActions,
                ActionsByType = actionsByType,
                ActionsByUser = actionsByUser,
                ActionsByEntityType = actionsByEntityType,
                RecentActivity = recentActivity
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener estadísticas de auditoría");
            return StatusCode(500, new { message = "Error al obtener estadísticas" });
        }
    }

    /// <summary>
    /// Registra una acción de auditoría manual (para testing)
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<AuditLogDto>> LogAction([FromBody] CreateAuditLogDto dto)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == Guid.Empty)
                return Unauthorized();

            await _auditLogService.LogActionAsync(
                currentUserId,
                dto.Action,
                dto.EntityType,
                dto.EntityId,
                dto.Details
            );

            return Ok(new { message = "Acción registrada correctamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar acción de auditoría");
            return StatusCode(500, new { message = "Error al registrar acción" });
        }
    }

    private async Task<List<Guid>> GetProjectEntityIds(Guid projectId)
    {
        var projectEntityIds = new List<Guid> { projectId };
        
        var phaseIds = await _context.Phases
            .Where(p => p.ProjectId == projectId)
            .Select(p => p.Id)
            .ToListAsync();
        projectEntityIds.AddRange(phaseIds);

        var iterationIds = await _context.Iterations
            .Where(i => i.ProjectId == projectId)
            .Select(i => i.Id)
            .ToListAsync();
        projectEntityIds.AddRange(iterationIds);

        var artifactIds = await _context.Artifacts
            .Where(a => a.ProjectId == projectId)
            .Select(a => a.Id)
            .ToListAsync();
        projectEntityIds.AddRange(artifactIds);

        var versionIds = await _context.ArtifactVersions
            .Where(v => artifactIds.Contains(v.ArtifactId))
            .Select(v => v.Id)
            .ToListAsync();
        projectEntityIds.AddRange(versionIds);

        return projectEntityIds;
    }
}

// DTOs para auditoría
public class AuditQueryResultDto
{
    public IEnumerable<AuditLogDto> Logs { get; set; } = new List<AuditLogDto>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class AuditExportDto
{
    public DateTime ExportedAt { get; set; }
    public string ExportedBy { get; set; } = string.Empty;
    public int TotalRecords { get; set; }
    public AuditExportFiltersDto Filters { get; set; } = new();
    public IEnumerable<AuditLogExportDto> Logs { get; set; } = new List<AuditLogExportDto>();
}

public class AuditExportFiltersDto
{
    public Guid? ProjectId { get; set; }
    public Guid? UserId { get; set; }
    public string? EntityType { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

public class AuditLogExportDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public string? Details { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AuditStatsDto
{
    public int TotalActions { get; set; }
    public IEnumerable<ActionCountDto> ActionsByType { get; set; } = new List<ActionCountDto>();
    public IEnumerable<UserActionCountDto> ActionsByUser { get; set; } = new List<UserActionCountDto>();
    public IEnumerable<EntityTypeCountDto> ActionsByEntityType { get; set; } = new List<EntityTypeCountDto>();
    public IEnumerable<AuditLogDto> RecentActivity { get; set; } = new List<AuditLogDto>();
}

public class ActionCountDto
{
    public string Action { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class UserActionCountDto
{
    public Guid UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class EntityTypeCountDto
{
    public string EntityType { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class CreateAuditLogDto
{
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public string? Details { get; set; }
}
