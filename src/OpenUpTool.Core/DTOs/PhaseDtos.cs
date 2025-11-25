namespace OpenUpTool.Core.DTOs;

public record PhaseDto(
    Guid Id,
    Guid ProjectId,
    string PhaseCode,
    string Name,
    DateTime? StartDate,
    DateTime? EndDate,
    DateTime? ActualStart,
    DateTime? ActualEnd,
    string Status,
    int OrderIndex
);

public record UpdatePhaseDto(
    string? Name,
    DateTime? StartDate,
    DateTime? EndDate,
    DateTime? ActualStart,
    DateTime? ActualEnd,
    string? Status
);
