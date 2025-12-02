namespace OpenUpTool.Core.Entities;

/// <summary>
/// Representa una ejecución de un caso de prueba con su resultado
/// </summary>
public class TestExecution
{
    public Guid Id { get; set; }
    
    // Relación con artefacto (donde están los test cases)
    public Guid ArtifactId { get; set; }
    
    // Identificador del caso de prueba (referencia al JSON TestData)
    public string TestCaseId { get; set; } = string.Empty;
    
    // Nombre del caso de prueba (desnormalizado para consultas rápidas)
    public string TestCaseName { get; set; } = string.Empty;
    
    // Resultado de la ejecución
    public string Result { get; set; } = "Pending"; // Pending, Passed, Failed, Blocked, Skipped
    
    // Usuario que ejecutó la prueba
    public Guid ExecutedBy { get; set; }
    
    // Fecha de ejecución
    public DateTime ExecutedAt { get; set; }
    
    // Duración en segundos
    public int? DurationSeconds { get; set; }
    
    // Evidencia (JSON con screenshots, logs, etc.)
    public string? Evidence { get; set; } // JSON: { screenshots: [], logs: [], attachments: [] }
    
    // Observaciones
    public string? Notes { get; set; }
    
    // Versión del artefacto asociada
    public Guid? ArtifactVersionId { get; set; }
    
    // Entorno de ejecución
    public string? Environment { get; set; } // Dev, QA, Staging, Production
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public Artifact Artifact { get; set; } = null!;
    public ArtifactVersion? ArtifactVersion { get; set; }
    public ICollection<Defect> RelatedDefects { get; set; } = new List<Defect>();
}
