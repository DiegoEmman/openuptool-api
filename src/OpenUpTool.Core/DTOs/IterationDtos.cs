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
    DateTime CreatedAt
);

public record CreateIterationDto(
    string Name,
    string? Objective,
    string Phase,
    DateTime StartDate,
    DateTime EndDate
);

public record UpdateIterationStatusDto(
    string Status
);
