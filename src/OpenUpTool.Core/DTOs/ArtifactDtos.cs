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
    string? ContentText,
    string? FilePath,
    string? FileName,
    long? FileSize,
    string? MimeType,
    string? FileCategory,
    string? RepositoryUrl,
    string? RepositoryVersion,
    string? BuildNumber,
    List<TestCaseDto>? TestCases,
    List<TestResultDto>? TestResults,
    List<IterationActivityDto>? IterationActivities
);

public record CreateArtifactDto(
    Guid ProjectId,
    string PhaseId,
    Guid ArtifactTypeId,
    string Title,
    string? Description,
    string? Author,
    bool IsMandatory,
    string? ContentText,
    string? FileCategory, // DIAGRAM, PROTOTYPE, DOCUMENT
    string? RepositoryUrl,
    string? RepositoryVersion,
    string? BuildNumber
);

public record UpdateArtifactDto(
    string? Title,
    string? Description,
    string? Author,
    string? Status,
    bool? IsMandatory,
    string? ContentText,
    string? FileCategory,
    string? RepositoryUrl,
    string? RepositoryVersion,
    string? BuildNumber
);

// DTO para información de archivos permitidos
public record AllowedFileFormatsDto(
    string Category,
    List<string> Extensions,
    List<string> MimeTypes,
    long MaxSizeBytes
);

public record ArtifactVersionDto(
    Guid Id,
    Guid ArtifactId,
    int VersionNumber,
    string? FilePath,
    string? FileName,
    long? FileSize,
    string? UploadedBy,
    DateTime UploadedAt,
    string? ChangeDescription,
    DateTime CreatedAt
);

public record CreateArtifactVersionDto(
    string? ChangeDescription,
    string? UploadedBy
);

public record VersionComparisonDto(
    ArtifactVersionDto Version1,
    ArtifactVersionDto Version2,
    VersionDifferencesDto Differences
);

public record VersionDifferencesDto(
    bool FileChanged,
    bool FileSizeChanged,
    long? FileSizeDifference,
    bool AuthorChanged,
    TimeSpan TimeDifference,
    string? ChangeDescription
);

public record VersionHistoryDto(
    Guid ArtifactId,
    string ArtifactTitle,
    int TotalVersions,
    List<ArtifactVersionDto> Versions
);

// DTOs para datos estructurados de pruebas
public record TestCaseDto(
    string TestId,
    string Title,
    string? Description,
    List<string> Steps,
    string ExpectedResult,
    string Priority,
    DateTime CreatedAt
);

public record TestResultDto(
    string TestCaseId,
    string Result, // PASSED, FAILED, BLOCKED, SKIPPED
    string? ExecutedBy,
    DateTime ExecutedAt,
    string? Notes,
    List<DefectDto>? Defects
);

// DTOs para registro de iteraciones
public record IterationActivityDto(
    string ActivityId,
    string Type, // MEETING, DEVELOPMENT, TESTING, REVIEW, DECISION
    string Description,
    string? Participants,
    DateTime Timestamp,
    List<string>? Tags
);

public record AddTestCaseRequest(
    string TestId,
    string Title,
    string? Description,
    List<string> Steps,
    string ExpectedResult,
    string Priority
);

public record AddTestResultRequest(
    string TestCaseId,
    string Result,
    string? ExecutedBy,
    string? Notes,
    List<DefectDto>? Defects
);

public record AddIterationActivityRequest(
    string Type,
    string Description,
    string? Participants,
    List<string>? Tags
);

public record LinkRepositoryRequest(
    string RepositoryUrl,
    string? RepositoryVersion,
    string? BuildNumber
);

// DTOs para validación de avance de fase
public record PhaseValidationDto(
    bool CanAdvance,
    string Phase,
    int TotalMandatoryArtifacts,
    int CompletedMandatoryArtifacts,
    List<MissingArtifactDto> MissingArtifacts,
    string? Message
);

public record MissingArtifactDto(
    Guid ArtifactId,
    string Title,
    string ArtifactType,
    string Status,
    bool HasVersions
);

