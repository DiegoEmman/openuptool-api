namespace OpenUpTool.Core.Entities;

/// <summary>
/// Historial de cambios de estado de artefactos (auditoría)
/// </summary>
public class ArtifactStateHistory
{
    public Guid Id { get; set; }
    public Guid ArtifactId { get; set; }
    public Guid? FromStateId { get; set; } // null si es el primer estado
    public Guid ToStateId { get; set; }
    public Guid ChangedByUserId { get; set; }
    public DateTime ChangedAt { get; set; }
    public string? Comments { get; set; }
    public string? Metadata { get; set; } // JSON: Información adicional del cambio
    
    // Navigation properties
    public Artifact Artifact { get; set; } = null!;
    public WorkflowState? FromState { get; set; }
    public WorkflowState ToState { get; set; } = null!;
    public User ChangedByUser { get; set; } = null!;
}
