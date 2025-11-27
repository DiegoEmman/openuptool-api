namespace OpenUpTool.Core.Entities;

public class UserStory
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? AcceptanceCriteria { get; set; }
    public string Priority { get; set; } = "medium"; // low, medium, high, critical
    public int? StoryPoints { get; set; }
    public string Status { get; set; } = "backlog"; // backlog, ready, in_progress, done, blocked
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public Project Project { get; set; } = null!;
    public User? Creator { get; set; }
    public ICollection<IterationScope> IterationScopes { get; set; } = new List<IterationScope>();
}
