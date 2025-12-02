public class ArtifactVersion
{
    public int Id { get; set; }
    public int ArtifactId { get; set; }
    public int VersionNumber { get; set; } // 1,2,3...
    public DateTime Date { get; set; } = DateTime.UtcNow;

    public int AuthorId { get; set; }
    public string ChangeLog { get; set; }
    public string FileUrl { get; set; }

    public Artifact Artifact { get; set; }
}
