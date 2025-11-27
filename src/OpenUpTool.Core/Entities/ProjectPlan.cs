namespace OpenUpTool.Core.Entities;

public class ProjectPlan
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Objectives { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
    public List<PhaseScheduleItem> InitialSchedule { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public int Version { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Observations { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public Project Project { get; set; } = null!;
    public ICollection<Milestone> Milestones { get; set; } = new List<Milestone>();
}

public class PhaseScheduleItem
{
    public string PhaseName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Responsible { get; set; }
}
