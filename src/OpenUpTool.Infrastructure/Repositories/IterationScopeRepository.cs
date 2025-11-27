using Microsoft.EntityFrameworkCore;
using OpenUpTool.Core.Entities;
using OpenUpTool.Core.Interfaces;
using OpenUpTool.Infrastructure.Data;

namespace OpenUpTool.Infrastructure.Repositories;

public class IterationScopeRepository : IIterationScopeRepository
{
    private readonly OpenUpToolDbContext _context;

    public IterationScopeRepository(OpenUpToolDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<IterationScope>> GetByIterationIdAsync(Guid iterationId)
    {
        return await _context.IterationScopes
            .Include(s => s.AssignedUser)
            .Where(s => s.IterationId == iterationId)
            .OrderBy(s => s.ItemType)
            .ThenBy(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<IterationScope?> GetByIdAsync(Guid id)
    {
        return await _context.IterationScopes
            .Include(s => s.AssignedUser)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<IterationScope> CreateAsync(IterationScope scope)
    {
        scope.CreatedAt = DateTime.UtcNow;
        scope.UpdatedAt = DateTime.UtcNow;
        _context.IterationScopes.Add(scope);
        await _context.SaveChangesAsync();
        return scope;
    }

    public async Task<IterationScope> UpdateAsync(IterationScope scope)
    {
        scope.UpdatedAt = DateTime.UtcNow;
        _context.IterationScopes.Update(scope);
        await _context.SaveChangesAsync();
        return scope;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var scope = await _context.IterationScopes.FindAsync(id);
        if (scope == null) return false;

        _context.IterationScopes.Remove(scope);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(Guid iterationId, string itemType, Guid itemId)
    {
        return await _context.IterationScopes
            .AnyAsync(s => s.IterationId == iterationId && s.ItemType == itemType && s.ItemId == itemId);
    }
}
