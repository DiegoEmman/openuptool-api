namespace OpenUpTool.Core.Entities;

public class ProjectInvitation
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string InvitedEmail { get; set; } = string.Empty;
    public Guid RoleId { get; set; }
    public Guid InvitedBy { get; set; }
    public string Status { get; set; } = "pending"; // pending, accepted, rejected, expired
    public string InvitationToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public Project Project { get; set; } = null!;
    public Role Role { get; set; } = null!;
    public User Inviter { get; set; } = null!;
}
