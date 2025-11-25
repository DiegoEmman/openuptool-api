namespace OpenUpTool.Core.DTOs;

public record PhaseScheduleItemDto(
    string PhaseName,
    DateTime StartDate,
    DateTime EndDate,
    string? Responsible
);

public record MilestoneDto(
    Guid Id,
    string Name,
    DateTime Date,
    string? Description
);

public record CreateMilestoneDto(
    string Name,
    DateTime Date,
    string? Description
);

public record ProjectPlanDto(
    Guid Id,
    Guid ProjectId,
    string Objectives,
    string Scope,
    List<PhaseScheduleItemDto> InitialSchedule,
    List<MilestoneDto> Milestones,
    DateTime CreatedAt,
    int Version,
    string? Observations
);

public record CreateProjectPlanDto(
    string Objectives,
    string Scope,
    List<PhaseScheduleItemDto> InitialSchedule,
    List<CreateMilestoneDto> Milestones,
    string? Observations
);
