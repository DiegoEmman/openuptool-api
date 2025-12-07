namespace OpenUpTool.Core.DTOs;

public class MicroincrementDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime Date { get; set; }
    public string Author { get; set; } = string.Empty;
    
    // HU-017: Tipo y evidencia
    public string Type { get; set; } = "funcional";
    public string? EvidenceUrl { get; set; }
    public string? EvidenceFilePath { get; set; }
    
    // Relaciones
    public Guid? IterationId { get; set; }
    public string? IterationName { get; set; }
    public Guid ArtifactId { get; set; }
    public string ArtifactTitle { get; set; } = string.Empty;
    
    // Metadata
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateMicroincrementDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime Date { get; set; }
    public string Author { get; set; } = string.Empty;
    
    // HU-017: Tipo y evidencia
    public string Type { get; set; } = "funcional"; // "tecnico" o "funcional"
    public string? EvidenceUrl { get; set; }
    public string? EvidenceFilePath { get; set; }
    
    public Guid? IterationId { get; set; }
    public Guid ArtifactId { get; set; }
}

public class UpdateMicroincrementDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime? Date { get; set; }
    public string? Author { get; set; }
    
    // HU-017: Tipo y evidencia
    public string? Type { get; set; }
    public string? EvidenceUrl { get; set; }
    public string? EvidenceFilePath { get; set; }
    
    public Guid? IterationId { get; set; }
    public Guid? ArtifactId { get; set; }
}
