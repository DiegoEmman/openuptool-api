using System.Text.Json.Serialization;

namespace OpenUpTool.Core.DTOs;

// ========== User Stories DTOs ==========
public record UserStoryDto(
    Guid Id,
    Guid ProjectId,
    string Title,
    string? Description,
    string? AcceptanceCriteria,
    string Priority,
    int? StoryPoints,
    string Status,
    Guid? CreatedBy,
    DateTime CreatedAt
);

public record CreateUserStoryDto(
    Guid ProjectId,
    string Title,
    string? Description,
    string? AcceptanceCriteria,
    string Priority,
    int? StoryPoints
);

public record UpdateUserStoryDto(
    string? Title,
    string? Description,
    string? AcceptanceCriteria,
    string? Priority,
    int? StoryPoints,
    string? Status
);

// ========== Iteration Scope DTOs ==========
public record IterationScopeDto(
    Guid Id,
    Guid IterationId,
    string ItemType,
    Guid ItemId,
    string? ItemTitle, // Para mostrar en el UI
    string? Description,
    decimal? EstimatedHours,
    string Status,
    Guid? AssignedTo,
    string? AssignedToName,
    DateTime CreatedAt
);

public record AddToScopeDto(
    Guid IterationId,
    string ItemType, // "story" o "artifact"
    Guid ItemId,
    string? Description,
    decimal? EstimatedHours,
    Guid? AssignedTo
);

public record UpdateScopeItemDto(
    string? Description,
    decimal? EstimatedHours,
    string? Status,
    Guid? AssignedTo
);

// ========== Project Invitations DTOs ==========
public record ProjectInvitationDto(
    Guid Id,
    Guid ProjectId,
    string ProjectName,
    string InvitedEmail,
    Guid RoleId,
    string RoleName,
    Guid InvitedBy,
    string InviterName,
    string Status,
    [property: JsonPropertyName("token")] string InvitationToken,
    DateTime ExpiresAt,
    DateTime CreatedAt
);

public record CreateInvitationDto(
    Guid ProjectId,
    [property: JsonPropertyName("email")] string InvitedEmail,
    Guid RoleId
);

public record AcceptInvitationDto(
    string Token
);

// ========== Notifications DTOs ==========
public record NotificationDto(
    Guid Id,
    string Type,
    string Title,
    string Message,
    string? RelatedEntityType,
    Guid? RelatedEntityId,
    bool IsRead,
    string? ActionUrl,
    DateTime CreatedAt
);

public record MarkAsReadDto(
    Guid NotificationId
);

// ========== HU-021: Notification Preferences DTOs ==========

/// <summary>
/// DTO para preferencia de notificación
/// </summary>
public record NotificationPreferenceDto(
    Guid Id,
    string NotificationType,
    string NotificationTypeDescription,
    bool InAppEnabled,
    bool EmailEnabled
);

/// <summary>
/// DTO para actualizar preferencias de notificación
/// </summary>
public record UpdateNotificationPreferenceDto(
    string NotificationType,
    bool? InAppEnabled,
    bool? EmailEnabled
);

/// <summary>
/// DTO para actualizar múltiples preferencias a la vez
/// </summary>
public record UpdateNotificationPreferencesDto(
    List<UpdateNotificationPreferenceDto> Preferences
);

/// <summary>
/// DTO para crear una notificación
/// </summary>
public record CreateNotificationDto(
    Guid UserId,
    string Type,
    string Title,
    string Message,
    string? RelatedEntityType = null,
    Guid? RelatedEntityId = null,
    string? ActionUrl = null
);

/// <summary>
/// DTO para enviar notificación a múltiples usuarios de un proyecto
/// </summary>
public record SendProjectNotificationDto(
    Guid ProjectId,
    string Type,
    string Title,
    string Message,
    string? RelatedEntityType = null,
    Guid? RelatedEntityId = null,
    string? ActionUrl = null,
    bool ExcludeSender = true
);

/// <summary>
/// DTO con los tipos de notificación disponibles
/// </summary>
public record NotificationTypeInfoDto(
    string Type,
    string Description,
    string Category
);

/// <summary>
/// DTO de resumen de preferencias del usuario
/// </summary>
public record NotificationPreferencesSummaryDto(
    Guid UserId,
    int TotalTypes,
    int InAppEnabledCount,
    int EmailEnabledCount,
    List<NotificationPreferenceDto> Preferences
);

// ==================== HU-021: DTOs adicionales para Notificaciones ====================

/// <summary>
/// DTO completo de preferencia de notificación (con más campos)
/// </summary>
public record NotificationPreferenceFullDto(
    Guid Id,
    Guid UserId,
    string NotificationType,
    bool EmailEnabled,
    bool PlatformEnabled,
    bool IsEnabled,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

/// <summary>
/// DTO para configuración completa del usuario
/// </summary>
public record UserNotificationSettingsDto(
    Guid UserId,
    List<NotificationPreferenceFullDto> Preferences,
    bool GlobalEmailEnabled,
    bool GlobalPlatformEnabled
);

/// <summary>
/// DTO para actualización masiva de preferencias
/// </summary>
public record BulkUpdatePreferencesDto(
    bool? EnableAllEmail,
    bool? EnableAllPlatform,
    List<BulkPreferenceItemDto>? Preferences
);

/// <summary>
/// Item individual para actualización masiva
/// </summary>
public record BulkPreferenceItemDto(
    string NotificationType,
    bool? EmailEnabled,
    bool? PlatformEnabled,
    bool? IsEnabled
);

/// <summary>
/// DTO para disparar notificación de prueba
/// </summary>
public record TriggerNotificationDto(
    string Type,
    string Title,
    string Message,
    string? RelatedEntityType = null,
    Guid? RelatedEntityId = null,
    string? ActionUrl = null
);

// ==================== HU-023: Export/Import DTOs ====================

/// <summary>
/// DTO para exportación completa de proyecto
/// </summary>
public record ProjectExportDto(
    string ExportVersion,
    DateTime ExportedAt,
    string ExportedBy,
    ProjectExportDataDto? Project,
    List<PhaseExportDto> Phases,
    List<IterationExportDto> Iterations,
    List<ArtifactExportDto> Artifacts,
    List<ArtifactVersionExportDto> ArtifactVersions,
    ProjectPlanExportDto? Plan
);

/// <summary>
/// Datos básicos del proyecto para exportación
/// </summary>
public record ProjectExportDataDto(
    Guid Id,
    string Name,
    string Identifier,
    DateTime StartDate,
    string Status,
    string? Owner,
    string? Description,
    List<string> Tags,
    DateTime CreatedAt
);

/// <summary>
/// Fase para exportación (ajustado a entidad real)
/// </summary>
public record PhaseExportDto(
    Guid Id,
    string PhaseCode,
    string Name,
    int OrderIndex,
    DateTime? StartDate,
    DateTime? EndDate,
    string Status
);

/// <summary>
/// Iteración para exportación (ajustado a entidad real)
/// </summary>
public record IterationExportDto(
    Guid Id,
    string Name,
    string? Objective,
    string Phase,
    DateTime StartDate,
    DateTime EndDate,
    string Status,
    int? PlannedCapacityHours,
    int? TeamSize,
    int? PlannedPoints,
    int? CompletedPoints
);

/// <summary>
/// Artefacto para exportación (ajustado a entidad real)
/// </summary>
public record ArtifactExportDto(
    Guid Id,
    string Title,
    Guid ArtifactTypeId,
    string? Description,
    string? ContentText,
    string Status,
    string PhaseId,
    string? Author,
    bool IsMandatory,
    DateTime CreatedAt
);

/// <summary>
/// Versión de artefacto para exportación (ajustado a entidad real)
/// </summary>
public record ArtifactVersionExportDto(
    Guid Id,
    Guid ArtifactId,
    int VersionNumber,
    string? FilePath,
    string? FileName,
    string? UploadedBy,
    string? ChangeDescription,
    DateTime CreatedAt
);

/// <summary>
/// Plan de proyecto para exportación (ajustado a entidad real)
/// </summary>
public record ProjectPlanExportDto(
    Guid Id,
    string Objectives,
    string Scope,
    string? Observations,
    int Version,
    bool IsActive
);

/// <summary>
/// DTO para importar proyecto desde JSON
/// </summary>
public record ProjectImportDto(
    string Name,
    string? Description,
    List<string>? Tags,
    bool ImportPhases,
    bool ImportIterations,
    bool ImportArtifacts,
    bool ImportVersionHistory,
    ProjectExportDto? SourceData
);

/// <summary>
/// DTO para importar plantilla y crear proyecto
/// </summary>
public record TemplateProjectImportDto(
    string ProjectName,
    string? ProjectDescription,
    DateTime? StartDate,
    TemplateDefinitionDto Template
);

/// <summary>
/// Definición de plantilla para importación
/// </summary>
public record TemplateDefinitionDto(
    string TemplateName,
    string? TemplateDescription,
    List<TemplatePhaseDto>? Phases,
    List<TemplateArtifactTypeDto>? ArtifactTypes,
    List<TemplateIterationDto>? DefaultIterations
);

/// <summary>
/// Fase en plantilla
/// </summary>
public record TemplatePhaseDto(
    string Name,
    string? Description,
    int OrderIndex
);

/// <summary>
/// Tipo de artefacto en plantilla
/// </summary>
public record TemplateArtifactTypeDto(
    string Name,
    string Code,
    string? Description,
    string Phase,
    bool IsMandatory
);

/// <summary>
/// Iteración por defecto en plantilla
/// </summary>
public record TemplateIterationDto(
    string Name,
    int Number,
    string PhaseName,
    int DurationDays
);

/// <summary>
/// Resultado de exportación
/// </summary>
public record ExportResultDto(
    bool Success,
    string Format,
    string? FileName,
    string? ContentBase64,
    string? DownloadUrl,
    int ItemsExported,
    string Message
);

/// <summary>
/// Resultado de importación
/// </summary>
public record ImportResultDto(
    bool Success,
    Guid? ProjectId,
    string? ProjectName,
    int PhasesImported,
    int IterationsImported,
    int ArtifactsImported,
    int VersionsImported,
    List<string> Warnings,
    string Message
);

