namespace OpenUpTool.Core.Entities;

public class Phase
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string PhaseCode { get; set; } = string.Empty; // INCEPTION, ELABORATION, CONSTRUCTION, TRANSITION
    public string Name { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? ActualStart { get; set; }
    public DateTime? ActualEnd { get; set; }
    public string Status { get; set; } = "PENDING"; // PENDING, IN_PROGRESS, COMPLETED
    public int OrderIndex { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public Project Project { get; set; } = null!;
}
