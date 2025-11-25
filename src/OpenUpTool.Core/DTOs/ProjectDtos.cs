namespace OpenUpTool.Core.DTOs;

public record CreateProjectDto(
    string Name,
    string Identifier,
    DateTime StartDate,
    string? Owner,
    string? Description,
    List<string> Tags
);

public record UpdateProjectDto(
    string? Name,
    string? Status,
    string? Owner,
    string? Description,
    List<string>? Tags
);

public record ProjectDto(
    Guid Id,
    string Name,
    string Identifier,
    DateTime StartDate,
    string Status,
    string? Owner,
    string? Description,
    List<string> Tags,
    Guid? PlanId,
    List<string> Phases,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
