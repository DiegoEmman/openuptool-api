using OpenUpTool.Core.Entities;

namespace OpenUpTool.Core.Interfaces;

public interface IIterationScopeRepository
{
    Task<IEnumerable<IterationScope>> GetByIterationIdAsync(Guid iterationId);
    Task<IterationScope?> GetByIdAsync(Guid id);
    Task<IterationScope> CreateAsync(IterationScope scope);
    Task<IterationScope> UpdateAsync(IterationScope scope);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid iterationId, string itemType, Guid itemId);
}
