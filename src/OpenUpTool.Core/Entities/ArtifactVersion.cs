namespace OpenUpTool.Core.Entities;

public class ArtifactVersion
{
    public Guid Id { get; set; }
    public Guid ArtifactId { get; set; }
    public int VersionNumber { get; set; }
    public string? FilePath { get; set; }
    public string? FileName { get; set; }
    public long? FileSize { get; set; }
    public string? UploadedBy { get; set; }
    public DateTime UploadedAt { get; set; }
    public string? ChangeDescription { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public Artifact Artifact { get; set; } = null!;
}
