namespace OpenUpTool.Core.Entities;

/// <summary>
/// Representa un defecto encontrado durante pruebas o desarrollo
/// </summary>
public class Defect
{
    public Guid Id { get; set; }
    
    // Número secuencial legible por humanos (ej: DEF-001, DEF-002)
    public string DefectNumber { get; set; } = string.Empty;
    
    // Título descriptivo del defecto
    public string Title { get; set; } = string.Empty;
    
    // Descripción detallada
    public string Description { get; set; } = string.Empty;
    
    // Severidad del defecto
    public string Severity { get; set; } = "Medium"; // Critical, High, Medium, Low
    
    // Estado del defecto
    public string Status { get; set; } = "Open"; // Open, InProgress, Resolved, Closed, Reopened
    
    // Prioridad
    public string Priority { get; set; } = "Medium"; // Critical, High, Medium, Low
    
    // Tipo de defecto
    public string Type { get; set; } = "Bug"; // Bug, Regression, Performance, Security, UI, Other
    
    // Proyecto al que pertenece
    public Guid ProjectId { get; set; }
    
    // Artefacto relacionado (opcional)
    public Guid? ArtifactId { get; set; }
    
    // Versión del artefacto relacionada (opcional)
    public Guid? ArtifactVersionId { get; set; }
    
    // Ejecución de prueba que detectó el defecto (opcional)
    public Guid? TestExecutionId { get; set; }
    
    // Usuario que reportó el defecto
    public Guid ReportedBy { get; set; }
    
    // Fecha de reporte
    public DateTime ReportedAt { get; set; }
    
    // Usuario asignado para resolver el defecto
    public Guid? AssignedTo { get; set; }
    
    // Fecha de asignación
    public DateTime? AssignedAt { get; set; }
    
    // Fecha de resolución
    public DateTime? ResolvedAt { get; set; }
    
    // Usuario que resolvió
    public Guid? ResolvedBy { get; set; }
    
    // Descripción de la resolución
    public string? Resolution { get; set; }
    
    // Pasos para reproducir
    public string? StepsToReproduce { get; set; }
    
    // Resultado esperado
    public string? ExpectedResult { get; set; }
    
    // Resultado actual
    public string? ActualResult { get; set; }
    
    // Entorno donde se encontró
    public string? Environment { get; set; } // Dev, QA, Staging, Production
    
    // Etiquetas (JSON array)
    public string? Tags { get; set; } // JSON: ["ui", "login", "security"]
    
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public Project Project { get; set; } = null!;
    public Artifact? Artifact { get; set; }
    public ArtifactVersion? ArtifactVersion { get; set; }
    public TestExecution? TestExecution { get; set; }
}
