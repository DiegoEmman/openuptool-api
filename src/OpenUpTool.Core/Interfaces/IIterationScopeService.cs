using OpenUpTool.Core.DTOs;

namespace OpenUpTool.Core.Interfaces;

public interface IIterationScopeService
{
    Task<IEnumerable<IterationScopeDto>> GetByIterationIdAsync(Guid iterationId);
    Task<IterationScopeDto?> GetByIdAsync(Guid id);
    Task<IterationScopeDto> AddToScopeAsync(AddToScopeDto dto);
    Task<IterationScopeDto?> UpdateAsync(Guid id, UpdateScopeItemDto dto);
    Task<bool> RemoveFromScopeAsync(Guid id);
}
