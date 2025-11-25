namespace OpenUpTool.Core.Entities;

public class Iteration
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Objective { get; set; }
    public string Phase { get; set; } = string.Empty; // PhaseCode: INCEPTION, ELABORATION, CONSTRUCTION, TRANSITION
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = "Planeada"; // Planeada, En curso, Finalizada
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public Project Project { get; set; } = null!;
}
