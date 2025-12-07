namespace OpenUpTool.Core.Entities;

/// <summary>
/// Representa los responsables asignados a un estado del flujo
/// </summary>
public class WorkflowStateResponsible
{
    public Guid Id { get; set; }
    public Guid WorkflowStateId { get; set; }
    public Guid UserId { get; set; }
    public string? Role { get; set; } // Rol específico en este estado (Revisor, Aprobador, etc.)
    public DateTime AssignedAt { get; set; }
    
    // Navigation properties
    public WorkflowState WorkflowState { get; set; } = null!;
    public User User { get; set; } = null!;
}
