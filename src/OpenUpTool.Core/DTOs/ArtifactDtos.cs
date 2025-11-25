namespace OpenUpTool.Core.DTOs;

public record ArtifactTypeDto(
    Guid Id,
    string Phase,
    string Code,
    string Name,
    string Description,
    bool IsMandatory,
    string DefaultFormat
);

public record ArtifactDto(
    Guid Id,
    Guid ProjectId,
    string PhaseId,
    Guid ArtifactTypeId,
    string Title,
    string? Description,
    string? Author,
    DateTime CreatedAt,
    string Status,
    bool IsMandatory,
    string? ContentText
);

public record CreateArtifactDto(
    Guid ProjectId,
    string PhaseId,
    Guid ArtifactTypeId,
    string Title,
    string? Description,
    string? Author
);

public record UpdateArtifactDto(
    string? Title,
    string? Description,
    string? Author,
    string? Status,
    string? ContentText
);
