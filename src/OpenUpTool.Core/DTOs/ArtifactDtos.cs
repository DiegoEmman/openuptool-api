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

// ========== HU-020: DTOs para Reasignacion de Entregables ==========

/// <summary>
/// DTO para solicitar la reasignacion de un entregable a otra fase
/// </summary>
public record ReassignArtifactDto(
    string NewPhaseId,
    string? Reason,
    bool ConfirmViolation = false  // Si es true, confirma la reasignacion aunque viole reglas
);

/// <summary>
/// DTO para solicitar la reasignacion de un entregable a otro workflow
/// </summary>
public record ReassignWorkflowDto(
    Guid NewWorkflowId,
    Guid? NewStateId,  // Estado inicial en el nuevo workflow (opcional, usa el inicial por defecto)
    string? Reason
);

/// <summary>
/// DTO para el historial de movimientos de un artefacto
/// </summary>
public record ArtifactMovementHistoryDto(
    Guid Id,
    Guid ArtifactId,
    string MovementType,  // PHASE_CHANGE, WORKFLOW_CHANGE
    string? FromPhaseId,
    string? ToPhaseId,
    Guid? FromWorkflowId,
    Guid? ToWorkflowId,
    Guid? FromStateId,
    Guid? ToStateId,
    string? Reason,
    string MovedBy,
    DateTime MovedAt,
    bool ViolatedRules,
    string? ViolationDetails
);

/// <summary>
/// DTO para la respuesta de reasignacion con validaciones
/// </summary>
public record ReassignmentResultDto(
    bool Success,
    ArtifactDto? Artifact,
    ArtifactMovementHistoryDto? Movement,
    bool HasViolations,
    List<ReassignmentViolationDto>? Violations,
    string Message
);

/// <summary>
/// DTO para una violacion de regla en la reasignacion
/// </summary>
public record ReassignmentViolationDto(
    string ViolationType,  // MISSING_MANDATORY_ARTIFACTS, INVALID_PHASE_TRANSITION, WORKFLOW_INCOMPATIBLE
    string Description,
    string Severity  // WARNING, ERROR
);

/// <summary>
/// DTO para validar si un movimiento es posible sin ejecutarlo
/// </summary>
public record ValidateReassignmentDto(
    string? NewPhaseId,
    Guid? NewWorkflowId
);

/// <summary>
/// DTO con el resumen de historial de movimientos de un artefacto
/// </summary>
public record ArtifactMovementSummaryDto(
    Guid ArtifactId,
    string ArtifactTitle,
    string CurrentPhaseId,
    Guid? CurrentWorkflowId,
    int TotalMovements,
    List<ArtifactMovementHistoryDto> Movements
);
