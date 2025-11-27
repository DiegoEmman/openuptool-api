namespace OpenUpTool.Core.Entities;

public class ProjectUserRole
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public Guid? InvitedBy { get; set; }
    public DateTime InvitedAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Accepted, Rejected
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public Project Project { get; set; } = null!;
    public User User { get; set; } = null!;
    public Role Role { get; set; } = null!;
    public User? Inviter { get; set; }
}
