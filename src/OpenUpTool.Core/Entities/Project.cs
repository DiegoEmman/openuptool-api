namespace OpenUpTool.Core.Entities;

public class Project
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Identifier { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public string Status { get; set; } = "Creado"; // Creado, Planificado, En curso, Cerrado
    public string? Owner { get; set; }
    public string? Description { get; set; }
    public List<string> Tags { get; set; } = new();
    public Guid? PlanId { get; set; }
    public bool IsArchived { get; set; } = false;
    public DateTime? ArchivedAt { get; set; }
    public Guid? ArchivedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public ProjectPlan? Plan { get; set; }
    public ICollection<Phase> Phases { get; set; } = new List<Phase>();
    public ICollection<Iteration> Iterations { get; set; } = new List<Iteration>();
    public ICollection<Artifact> Artifacts { get; set; } = new List<Artifact>();
}
