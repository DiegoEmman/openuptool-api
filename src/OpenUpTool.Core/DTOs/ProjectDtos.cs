using System.Text.Json.Serialization;

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
    DateTime UpdatedAt,
    bool IsArchived,
    DateTime? ArchivedAt,
    Guid? ArchivedBy
);

public class ProjectMemberDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? UserEmail { get; set; }
    public string? UserName { get; set; }
    public Guid RoleId { get; set; }
    public string? RoleName { get; set; }
    public string? Status { get; set; }
    public string? InvitedBy { get; set; }
    public DateTime? InvitedAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
}

public class UpdateMemberRoleDto
{
    [property: JsonPropertyName("roleId")]
    public Guid RoleId { get; set; }
}

public class PermissionMatrixDto
{
    public List<PermissionDefinitionDto> Permissions { get; set; } = new List<PermissionDefinitionDto>();
}

public class PermissionDefinitionDto
{
    public string? Action { get; set; }
    public string? Description { get; set; }
    public bool Admin { get; set; }
    public bool Manager { get; set; }
    public bool Developer { get; set; }
    public bool Viewer { get; set; }
}

public class AddMemberDto
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public string? Email { get; set; }
}
