namespace OpenUpTool.Core.Entities;

public class IterationProgress
{
    public Guid Id { get; set; }
    public Guid IterationId { get; set; }
    public DateTime RecordDate { get; set; }
    public decimal CompletionPercentage { get; set; } // % completado de la iteración
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int InProgressTasks { get; set; }
    public int BlockedTasks { get; set; }
    public string? Blockers { get; set; } // Bloqueos identificados
    public string? Observations { get; set; } // Observaciones sobre el avance
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public Iteration Iteration { get; set; } = null!;
}

public class IterationTask
{
    public Guid Id { get; set; }
    public Guid IterationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = "pending"; // pending, in_progress, completed, blocked
    public decimal? EstimatedHours { get; set; }
    public decimal? ActualHours { get; set; }
    public Guid? AssignedTo { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int Priority { get; set; } = 3; // 1=Alta, 2=Media, 3=Baja
    public string? BlockerDescription { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public Iteration Iteration { get; set; } = null!;
    public User? AssignedUser { get; set; }
}
