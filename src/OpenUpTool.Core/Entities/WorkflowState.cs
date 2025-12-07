namespace OpenUpTool.Core.Entities;

/// <summary>
/// Representa un estado específico dentro de un flujo de trabajo
/// </summary>
public class WorkflowState
{
    public Guid Id { get; set; }
    public Guid WorkflowId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Order { get; set; } // Orden del estado en el flujo
    public string? Color { get; set; } // Color para visualización (hex)
    public bool IsInitialState { get; set; } = false;
    public bool IsFinalState { get; set; } = false;
    public string? RequiredActions { get; set; } // JSON: Lista de acciones requeridas
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    public Workflow Workflow { get; set; } = null!;
    public ICollection<WorkflowStateResponsible> Responsibles { get; set; } = new List<WorkflowStateResponsible>();
    public ICollection<ArtifactStateHistory> ArtifactStateHistories { get; set; } = new List<ArtifactStateHistory>();
}
