using OpenUpTool.Core.Entities;

namespace OpenUpTool.Core.Interfaces;

public interface IProjectInvitationRepository
{
    Task<IEnumerable<ProjectInvitation>> GetByProjectIdAsync(Guid projectId);
    Task<IEnumerable<ProjectInvitation>> GetByEmailAsync(string email);
    Task<ProjectInvitation?> GetByIdAsync(Guid id);
    Task<ProjectInvitation?> GetByTokenAsync(string token);
    Task<ProjectInvitation> CreateAsync(ProjectInvitation invitation);
    Task<ProjectInvitation> UpdateAsync(ProjectInvitation invitation);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<ProjectInvitation>> GetPendingByEmailAsync(string email);
}
