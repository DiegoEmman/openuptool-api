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
    
    // HU-016: Capacidad y velocidad
    public int? PlannedCapacityHours { get; set; } // Capacidad planificada en horas
    public int? TeamSize { get; set; } // Número de miembros del equipo
    public int? PlannedPoints { get; set; } // Puntos planificados para la iteración
    public int? CompletedPoints { get; set; } // Puntos completados (velocidad real)
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public Project Project { get; set; } = null!;
}
