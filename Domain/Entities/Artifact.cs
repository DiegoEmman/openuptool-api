public class Artifact
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string Phase { get; set; } // "Construction" para HU-008
    public string Type { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public int AuthorId { get; set; }
    public bool Required { get; set; }
    public string Status { get; set; } = "Pending";

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // HU-008
    public string RepositoryUrl { get; set; }
    public List<TestCase> TestCases { get; set; }
    public List<IterationLog> IterationLogs { get; set; }

    // HU-010
    public List<ArtifactVersion> Versions { get; set; }
}
