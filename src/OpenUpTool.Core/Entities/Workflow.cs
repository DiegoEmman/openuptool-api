namespace OpenUpTool.Core.Entities;

/// <summary>
/// Representa un flujo de trabajo personalizado con estados ordenados
/// </summary>
public class Workflow
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    public Project Project { get; set; } = null!;
    public ICollection<WorkflowState> States { get; set; } = new List<WorkflowState>();
    public ICollection<Artifact> Artifacts { get; set; } = new List<Artifact>();
}
