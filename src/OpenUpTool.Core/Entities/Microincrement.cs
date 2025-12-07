namespace OpenUpTool.Core.Entities;

public class Microincrement
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime Date { get; set; }
    public string Author { get; set; } = string.Empty;
    
    // HU-017: Tipo de microincremento (técnico o funcional)
    public string Type { get; set; } = "funcional"; // "tecnico" o "funcional"
    
    // HU-017: Evidencia (archivo o enlace)
    public string? EvidenceUrl { get; set; }
    public string? EvidenceFilePath { get; set; }
    
    // Relaciones
    public Guid? IterationId { get; set; }
    public Guid ArtifactId { get; set; }
    
    // Metadata
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    public Iteration? Iteration { get; set; }
    public Artifact Artifact { get; set; } = null!;
}
