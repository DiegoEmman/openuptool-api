namespace OpenUpTool.Core.Entities;

/// <summary>
/// Matriz de permisos para workflows - Define qué roles pueden realizar qué acciones
/// </summary>
public class WorkflowPermission
{
    public Guid Id { get; set; }
    public Guid WorkflowId { get; set; }
    public string Role { get; set; } = string.Empty; // autor, revisor, PO, admin
    public string Action { get; set; } = string.Empty; // crear, editar, aprobar, cambiar_estado
    public bool IsAllowed { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public Workflow? Workflow { get; set; }
}
