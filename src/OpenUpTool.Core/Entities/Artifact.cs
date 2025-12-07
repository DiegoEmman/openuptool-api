namespace OpenUpTool.Core.Entities;

public class Artifact
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string PhaseId { get; set; } = string.Empty; // PhaseCode usado como identificador
    public Guid ArtifactTypeId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Author { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = "Pendiente"; // Pendiente, En revisión, Aprobado
    public bool IsMandatory { get; set; }
    public string? ContentText { get; set; }
    
    // Campos para archivos adjuntos
    public string? FilePath { get; set; } // Ruta física del archivo en el servidor
    public string? FileName { get; set; } // Nombre original del archivo
    public long? FileSize { get; set; } // Tamaño en bytes
    public string? MimeType { get; set; } // Tipo MIME del archivo (image/png, application/pdf, etc.)
    public string? FileCategory { get; set; } // DIAGRAM, PROTOTYPE, DOCUMENT, etc.
    
    // Campos para código fuente y repositorio
    public string? RepositoryUrl { get; set; } // URL del repositorio (GitHub, GitLab, etc.)
    public string? RepositoryVersion { get; set; } // Tag, commit, branch
    public string? BuildNumber { get; set; } // Número de build asociado
    
    // Campos estructurados para pruebas e iteraciones (almacenados como JSON)
    public string? TestData { get; set; } // JSON: { testCases: [], results: [] }
    public string? IterationData { get; set; } // JSON: { activities: [], comments: [] }
    
    // Campos para Workflow
    public Guid? WorkflowId { get; set; } // Flujo de trabajo asociado
    public Guid? CurrentStateId { get; set; } // Estado actual en el flujo
    
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public Project Project { get; set; } = null!;
    public ArtifactType ArtifactType { get; set; } = null!;
    public Workflow? Workflow { get; set; }
    public WorkflowState? CurrentState { get; set; }
    public ICollection<ArtifactVersion> Versions { get; set; } = new List<ArtifactVersion>();
    public ICollection<ArtifactStateHistory> StateHistories { get; set; } = new List<ArtifactStateHistory>();
}
