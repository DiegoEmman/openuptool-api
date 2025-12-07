namespace OpenUpTool.Core.DTOs;

public record IterationDto(
    Guid Id,
    Guid ProjectId,
    string Name,
    string? Objective,
    string Phase,
    DateTime StartDate,
    DateTime EndDate,
    string Status,
    int? PlannedCapacityHours,
    int? TeamSize,
    int? PlannedPoints,
    int? CompletedPoints,
    DateTime CreatedAt
);

public record CreateIterationDto(
    string Name,
    string? Objective,
    string Phase,
    DateTime StartDate,
    DateTime EndDate,
    int? PlannedCapacityHours = null,
    int? TeamSize = null,
    int? PlannedPoints = null
);

public record UpdateIterationStatusDto(
    string Status
);

// HU-016: DTOs para capacidad y velocidad
public record UpdateIterationCapacityDto(
    int? PlannedCapacityHours,
    int? TeamSize,
    int? PlannedPoints
);

public record UpdateIterationVelocityDto(
    int? PlannedPoints,
    int CompletedPoints
);

public record IterationVelocityDto(
    Guid IterationId,
    string IterationName,
    string Phase,
    DateTime StartDate,
    DateTime EndDate,
    string Status,
    int? PlannedCapacityHours,
    int? TeamSize,
    int? PlannedPoints,
    int? CompletedPoints,
    decimal? VelocityPerHour, // Puntos por hora de capacidad
    decimal? PointsPerMember  // Puntos por miembro del equipo
);

public record ProjectVelocityStatsDto(
    Guid ProjectId,
    string ProjectName,
    int TotalIterations,
    int TotalIterationsWithData,       // Iteraciones con datos de velocidad
    decimal AverageVelocity,           // Promedio de puntos completados por iteración
    decimal AverageCapacityHours,      // Promedio de horas de capacidad
    decimal AverageTeamSize,           // Promedio de tamaño del equipo
    decimal SuggestedPointsNextIteration, // Sugerencia para próxima iteración
    int TotalCompletedPoints,          // Total de puntos completados
    List<IterationVelocityDto> IterationHistory
);

public record PlanningDataDto(
    Guid ProjectId,
    string ProjectName,
    decimal AverageVelocity,
    decimal AverageCapacityHours,
    decimal SuggestedPointsForNextIteration,
    int? LastTeamSize,
    string PlanningRecommendation,
    List<IterationVelocitySummaryDto> LastIterations
);

public record IterationVelocitySummaryDto(
    string IterationName,
    int? PlannedPoints,
    int? CompletedPoints,
    decimal? Accuracy // Porcentaje de precisión de la estimación
);
