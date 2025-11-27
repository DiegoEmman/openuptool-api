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
    string InvitationToken,
    DateTime ExpiresAt,
    DateTime CreatedAt
);

public record CreateInvitationDto(
    Guid ProjectId,
    string InvitedEmail,
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
