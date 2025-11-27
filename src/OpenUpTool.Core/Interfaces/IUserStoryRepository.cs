using OpenUpTool.Core.Entities;

namespace OpenUpTool.Core.Interfaces;

public interface IUserStoryRepository
{
    Task<IEnumerable<UserStory>> GetByProjectIdAsync(Guid projectId);
    Task<UserStory?> GetByIdAsync(Guid id);
    Task<UserStory> CreateAsync(UserStory userStory);
    Task<UserStory> UpdateAsync(UserStory userStory);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<UserStory>> GetByStatusAsync(Guid projectId, string status);
}
