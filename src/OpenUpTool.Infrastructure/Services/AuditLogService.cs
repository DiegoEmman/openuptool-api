using Microsoft.EntityFrameworkCore;
using OpenUpTool.Core.DTOs;
using OpenUpTool.Core.Entities;
using OpenUpTool.Core.Interfaces;
using OpenUpTool.Infrastructure.Data;

namespace OpenUpTool.Infrastructure.Services;

public class AuditLogService : IAuditLogService
{
    private readonly OpenUpToolDbContext _context;

    public AuditLogService(OpenUpToolDbContext context)
    {
        _context = context;
    }

    public async Task LogActionAsync(Guid userId, string action, string entityType, Guid? entityId, string? details = null)
    {
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Details = details,
            CreatedAt = DateTime.UtcNow
        };

        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<AuditLogDto>> GetAuditLogsAsync(Guid? userId = null, string? entityType = null, Guid? entityId = null)
    {
        var query = _context.AuditLogs
            .Include(a => a.User)
            .AsQueryable();

        if (userId.HasValue)
            query = query.Where(a => a.UserId == userId);

        if (!string.IsNullOrEmpty(entityType))
            query = query.Where(a => a.EntityType == entityType);

        if (entityId.HasValue)
            query = query.Where(a => a.EntityId == entityId);

        var logs = await query
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        return logs.Select(a => new AuditLogDto
        {
            Id = a.Id,
            UserId = a.UserId,
            UserEmail = a.User?.Email ?? "Unknown",
            UserName = $"{a.User?.FirstName} {a.User?.LastName}".Trim(),
            Action = a.Action,
            EntityType = a.EntityType,
            EntityId = a.EntityId,
            Details = a.Details,
            CreatedAt = a.CreatedAt
        });
    }
}
