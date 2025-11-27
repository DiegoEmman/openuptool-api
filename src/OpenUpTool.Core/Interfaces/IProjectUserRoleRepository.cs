using OpenUpTool.Core.Entities;

namespace OpenUpTool.Core.Interfaces;

public interface IProjectUserRoleRepository
{
    Task<IEnumerable<ProjectUserRole>> GetByProjectIdAsync(Guid projectId);
    Task<IEnumerable<ProjectUserRole>> GetByUserIdAsync(Guid userId);
    Task<ProjectUserRole?> GetByIdAsync(Guid id);
    Task<bool> HasUserAccessToProjectAsync(Guid userId, Guid projectId);
    Task<ProjectUserRole> CreateAsync(ProjectUserRole projectUserRole);
    Task<ProjectUserRole> UpdateAsync(ProjectUserRole projectUserRole);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> UserHasRoleInProjectAsync(Guid userId, Guid projectId, string roleName);
}
