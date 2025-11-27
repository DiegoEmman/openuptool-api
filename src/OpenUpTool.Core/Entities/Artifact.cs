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
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public Project Project { get; set; } = null!;
    public ArtifactType ArtifactType { get; set; } = null!;
    public ICollection<ArtifactVersion> Versions { get; set; } = new List<ArtifactVersion>();
}
