namespace OpenUpTool.Core.Entities;

public class IterationScope
{
    public Guid Id { get; set; }
    public Guid IterationId { get; set; }
    public string ItemType { get; set; } = string.Empty; // "story" o "artifact"
    public Guid ItemId { get; set; } // UserStory.Id o Artifact.Id
    public string? Description { get; set; }
    public decimal? EstimatedHours { get; set; }
    public string Status { get; set; } = "pending"; // pending, in_progress, completed, blocked
    public Guid? AssignedTo { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public Iteration Iteration { get; set; } = null!;
    public User? AssignedUser { get; set; }
}
