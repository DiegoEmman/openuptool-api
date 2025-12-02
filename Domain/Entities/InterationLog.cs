public class IterationLog
{
    public int Id { get; set; }
    public int ArtifactId { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public string Comments { get; set; }
    public int Progress { get; set; } // 0-100

    public Artifact Artifact { get; set; }
}
