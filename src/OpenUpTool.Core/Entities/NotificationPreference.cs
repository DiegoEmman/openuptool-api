namespace OpenUpTool.Core.Entities;

/// <summary>
/// HU-021: Preferencias de notificación por usuario
/// </summary>
public class NotificationPreference
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    
    /// <summary>
    /// Tipo de notificación: NEW_VERSION, STATE_APPROVED, CRITICAL_DEFECT, DEADLINE_APPROACHING, 
    /// ARTIFACT_BLOCKED, INVITATION, MENTION, COMMENT
    /// </summary>
    public string NotificationType { get; set; } = string.Empty;
    
    /// <summary>
    /// Habilitar notificaciones en la plataforma
    /// </summary>
    public bool InAppEnabled { get; set; } = true;
    
    /// <summary>
    /// Habilitar notificaciones por correo electrónico
    /// </summary>
    public bool EmailEnabled { get; set; } = false;
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    public User User { get; set; } = null!;
}

/// <summary>
/// HU-021: Tipos de notificación disponibles
/// </summary>
public static class NotificationTypes
{
    public const string NewVersion = "NEW_VERSION";
    public const string StateApproved = "STATE_APPROVED";
    public const string CriticalDefect = "CRITICAL_DEFECT";
    public const string DeadlineApproaching = "DEADLINE_APPROACHING";
    public const string ArtifactBlocked = "ARTIFACT_BLOCKED";
    public const string Invitation = "INVITATION";
    public const string InvitationAccepted = "INVITATION_ACCEPTED";
    public const string Mention = "MENTION";
    public const string Comment = "COMMENT";
    public const string PhaseChange = "PHASE_CHANGE";
    public const string WorkflowChange = "WORKFLOW_CHANGE";
    
    public static readonly string[] All = new[]
    {
        NewVersion,
        StateApproved,
        CriticalDefect,
        DeadlineApproaching,
        ArtifactBlocked,
        Invitation,
        InvitationAccepted,
        Mention,
        Comment,
        PhaseChange,
        WorkflowChange
    };
}
