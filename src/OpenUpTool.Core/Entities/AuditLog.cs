namespace OpenUpTool.Core.Entities;

public class AuditLog
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Action { get; set; } = string.Empty; // ProjectArchived, ProjectDeleted, ProjectCreated, etc.
    public string EntityType { get; set; } = string.Empty; // Project, Artifact, etc.
    public Guid? EntityId { get; set; }
    public string? Details { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Navigation
    public User? User { get; set; }
}
