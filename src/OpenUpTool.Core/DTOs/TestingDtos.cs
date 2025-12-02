namespace OpenUpTool.Core.DTOs;

// ===== Test Execution DTOs =====

public record TestExecutionDto(
    Guid Id,
    Guid ArtifactId,
    string TestCaseId,
    string TestCaseName,
    string Result,
    Guid ExecutedBy,
    DateTime ExecutedAt,
    int? DurationSeconds,
    List<EvidenceItemDto>? Evidence,
    string? Notes,
    Guid? ArtifactVersionId,
    string? Environment,
    DateTime CreatedAt
);

public record CreateTestExecutionDto(
    Guid ArtifactId,
    string TestCaseId,
    string TestCaseName,
    string Result,
    string ExecutedBy,
    int? DurationSeconds,
    List<EvidenceItemDto>? Evidence,
    string? Notes,
    Guid? ArtifactVersionId,
    string? Environment
);

public record UpdateTestExecutionDto(
    string? Result,
    int? DurationSeconds,
    List<EvidenceItemDto>? Evidence,
    string? Notes,
    string? Environment
);

public record EvidenceItemDto(
    string Type, // Screenshot, Log, Attachment, Video
    string Name,
    string? Url,
    string? Description
);

// ===== Defect DTOs =====

public record DefectDto(
    Guid Id,
    string DefectNumber,
    string Title,
    string Description,
    string Severity,
    string Status,
    string Priority,
    string Type,
    Guid ProjectId,
    Guid? ArtifactId,
    Guid? ArtifactVersionId,
    Guid? TestExecutionId,
    Guid ReportedBy,
    DateTime ReportedAt,
    Guid? AssignedTo,
    DateTime? AssignedAt,
    DateTime? ResolvedAt,
    Guid? ResolvedBy,
    string? Resolution,
    string? StepsToReproduce,
    string? ExpectedResult,
    string? ActualResult,
    string? Environment,
    List<string>? Tags,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    // Datos relacionados
    string? ArtifactTitle,
    string? TestCaseName
);

public record CreateDefectDto(
    string Title,
    string Description,
    string Severity,
    string Priority,
    string Type,
    Guid ProjectId,
    Guid? ArtifactId,
    Guid? ArtifactVersionId,
    Guid? TestExecutionId,
    string ReportedBy,
    string? AssignedTo,
    string? StepsToReproduce,
    string? ExpectedResult,
    string? ActualResult,
    string? Environment,
    List<string>? Tags
);

public record UpdateDefectDto(
    string? Title,
    string? Description,
    string? Severity,
    string? Status,
    string? Priority,
    string? Type,
    string? AssignedTo,
    string? Resolution,
    string? StepsToReproduce,
    string? ExpectedResult,
    string? ActualResult,
    string? Environment,
    List<string>? Tags
);

public record AttachmentDto(
    string Name,
    string Path,
    string Type,
    long Size
);

// ===== Statistics DTOs =====

public record TestExecutionSummaryDto(
    Guid ArtifactId,
    string ArtifactTitle,
    int TotalExecutions,
    int Passed,
    int Failed,
    int Blocked,
    int Skipped,
    int Pending,
    double PassRate,
    DateTime? LastExecutionDate
);

public record DefectSummaryDto(
    Guid ProjectId,
    int TotalDefects,
    int Open,
    int InProgress,
    int Resolved,
    int Closed,
    int Reopened,
    int Critical,
    int High,
    int Medium,
    int Low,
    double ResolutionRate
);

