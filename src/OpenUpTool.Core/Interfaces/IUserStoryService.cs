using OpenUpTool.Core.DTOs;

namespace OpenUpTool.Core.Interfaces;

public interface IUserStoryService
{
    Task<IEnumerable<UserStoryDto>> GetByProjectIdAsync(Guid projectId);
    Task<UserStoryDto?> GetByIdAsync(Guid id);
    Task<UserStoryDto> CreateAsync(CreateUserStoryDto dto);
    Task<UserStoryDto?> UpdateAsync(Guid id, UpdateUserStoryDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<UserStoryDto>> GetBacklogAsync(Guid projectId);
}
