using OpenUpTool.Core.DTOs;

namespace OpenUpTool.Core.Interfaces;

public interface IProjectInvitationService
{
    Task<ProjectInvitationDto> CreateInvitationAsync(CreateInvitationDto dto, Guid invitedBy);
    Task<IEnumerable<ProjectInvitationDto>> GetByProjectIdAsync(Guid projectId);
    Task<IEnumerable<ProjectInvitationDto>> GetPendingByEmailAsync(string email);
    Task<bool> AcceptInvitationAsync(string token, Guid userId);
    Task<bool> RejectInvitationAsync(string token);
    Task<bool> CancelInvitationAsync(Guid invitationId);
}
