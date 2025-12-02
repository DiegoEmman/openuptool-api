public class TestCase
{
    public int Id { get; set; }
    public int ArtifactId { get; set; }
    public string Description { get; set; }
    public string Expected { get; set; }
    public string Result { get; set; } = "Not Run";
    public string EvidenceUrl { get; set; }

    public Artifact Artifact { get; set; }
}
