namespace OpenUpTool.Core.Entities;

/// <summary>
/// HU-020: Entidad para registrar el historial de movimientos de artefactos entre fases o workflows
/// </summary>
public class ArtifactMovementHistory
{
    public Guid Id { get; set; }
    public Guid ArtifactId { get; set; }
    
    /// <summary>
    /// Tipo de movimiento: PHASE_CHANGE, WORKFLOW_CHANGE
    /// </summary>
    public string MovementType { get; set; } = string.Empty;
    
    // Campos para cambio de fase
    public string? FromPhaseId { get; set; }
    public string? ToPhaseId { get; set; }
    
    // Campos para cambio de workflow
    public Guid? FromWorkflowId { get; set; }
    public Guid? ToWorkflowId { get; set; }
    public Guid? FromStateId { get; set; }
    public Guid? ToStateId { get; set; }
    
    /// <summary>
    /// Razon o justificacion del movimiento
    /// </summary>
    public string? Reason { get; set; }
    
    /// <summary>
    /// Usuario que realizo el movimiento
    /// </summary>
    public string MovedBy { get; set; } = string.Empty;
    
    /// <summary>
    /// Fecha y hora del movimiento
    /// </summary>
    public DateTime MovedAt { get; set; }
    
    /// <summary>
    /// Indica si el movimiento violo alguna regla de negocio
    /// </summary>
    public bool ViolatedRules { get; set; }
    
    /// <summary>
    /// Detalles de las reglas violadas (JSON)
    /// </summary>
    public string? ViolationDetails { get; set; }
    
    // Navigation properties
    public Artifact Artifact { get; set; } = null!;
    public Workflow? FromWorkflow { get; set; }
    public Workflow? ToWorkflow { get; set; }
    public WorkflowState? FromState { get; set; }
    public WorkflowState? ToState { get; set; }
}
