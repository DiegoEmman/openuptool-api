namespace OpenUpTool.Core.DTOs;

// DTOs para IterationTask
public record IterationTaskDto(
    Guid Id,
    Guid IterationId,
    string Name,
    string? Description,
    string Status,
    decimal? EstimatedHours,
    decimal? ActualHours,
    Guid? AssignedTo,
    string? AssignedToName,
    DateTime? StartDate,
    DateTime? EndDate,
    int Priority,
    string? BlockerDescription,
    DateTime CreatedAt
);

public record CreateIterationTaskDto(
    string Name,
    string? Description,
    decimal? EstimatedHours,
    Guid? AssignedTo,
    DateTime? StartDate,
    DateTime? EndDate,
    int Priority = 3
);

public record UpdateIterationTaskDto(
    string? Name,
    string? Description,
    string? Status,
    decimal? EstimatedHours,
    decimal? ActualHours,
    Guid? AssignedTo,
    DateTime? StartDate,
    DateTime? EndDate,
    int? Priority,
    string? BlockerDescription
);

// DTOs para IterationProgress
public record IterationProgressDto(
    Guid Id,
    Guid IterationId,
    DateTime RecordDate,
    decimal CompletionPercentage,
    int TotalTasks,
    int CompletedTasks,
    int InProgressTasks,
    int BlockedTasks,
    string? Blockers,
    string? Observations,
    DateTime CreatedAt
);

public record CreateIterationProgressDto(
    DateTime RecordDate,
    decimal CompletionPercentage,
    int TotalTasks,
    int CompletedTasks,
    int InProgressTasks,
    int BlockedTasks,
    string? Blockers,
    string? Observations
);

public record UpdateIterationProgressDto(
    decimal? CompletionPercentage,
    int? TotalTasks,
    int? CompletedTasks,
    int? InProgressTasks,
    int? BlockedTasks,
    string? Blockers,
    string? Observations
);

// DTOs para resumen y dashboard
public record IterationSummaryDto(
    Guid IterationId,
    string IterationName,
    string Phase,
    DateTime StartDate,
    DateTime EndDate,
    string Status,
    decimal CompletionPercentage,
    int TotalTasks,
    int CompletedTasks,
    int InProgressTasks,
    int BlockedTasks,
    IterationProgressDto? LatestProgress
);

public record ProjectDashboardDto(
    Guid ProjectId,
    string ProjectName,
    decimal OverallCompletionPercentage,
    List<PhaseProgressDto> PhaseProgress,
    List<IterationSummaryDto> RecentIterations
);

public record PhaseProgressDto(
    string PhaseCode,
    string PhaseName,
    decimal CompletionPercentage,
    int TotalIterations,
    int CompletedIterations
);

// DTOs para gráficos de burndown
public record BurndownDataDto(
    Guid IterationId,
    string IterationName,
    DateTime StartDate,
    DateTime EndDate,
    List<BurndownPointDto> DataPoints
);

public record BurndownPointDto(
    DateTime Date,
    int RemainingTasks,
    int IdealRemaining,
    int CompletedTasks
);
