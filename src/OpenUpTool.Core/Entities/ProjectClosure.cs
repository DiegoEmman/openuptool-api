namespace OpenUpTool.Core.Entities;

/// <summary>
/// Documento de cierre del proyecto con validación de criterios obligatorios
/// </summary>
public class ProjectClosure
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    
    // Información general
    public string ClosedBy { get; set; } = string.Empty; // Usuario que cerró el proyecto
    public DateTime ClosureDate { get; set; }
    public string? Summary { get; set; } // Resumen ejecutivo del proyecto
    public string? LessonsLearned { get; set; } // Lecciones aprendidas
    public string? Recommendations { get; set; } // Recomendaciones para futuros proyectos
    
    // Checklist de criterios cumplidos (JSON)
    // Estructura: [ { "criteriaId": "string", "name": "string", "isMandatory": bool, "isCompleted": bool, "notes": "string" } ]
    public string ChecklistData { get; set; } = "[]";
    
    // Validación automática
    public bool AllMandatoryCriteriaMet { get; set; } // Auto-calculado
    public int TotalCriteria { get; set; }
    public int CompletedCriteria { get; set; }
    public int MandatoryCriteria { get; set; }
    public int CompletedMandatoryCriteria { get; set; }
    
    // Estado del cierre
    public string Status { get; set; } = "Draft"; // Draft, PendingApproval, Approved, Rejected
    public string? RejectionReason { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedBy { get; set; }
    
    // Metadata
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation
    public Project Project { get; set; } = null!;
}
