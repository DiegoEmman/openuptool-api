namespace OpenUpTool.Core.Entities;

public class ArtifactType
{
    public Guid Id { get; set; }
    public string Phase { get; set; } = string.Empty; // PhaseCode: INCEPTION, ELABORATION, CONSTRUCTION, TRANSITION
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsMandatory { get; set; }
    public string DefaultFormat { get; set; } = "TEXT"; // TEXT, FILE, MIXED
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<Artifact> Artifacts { get; set; } = new List<Artifact>();
}
