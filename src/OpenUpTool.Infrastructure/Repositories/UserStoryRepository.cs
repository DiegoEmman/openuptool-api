using Microsoft.EntityFrameworkCore;
using OpenUpTool.Core.Entities;
using OpenUpTool.Core.Interfaces;
using OpenUpTool.Infrastructure.Data;

namespace OpenUpTool.Infrastructure.Repositories;

public class UserStoryRepository : IUserStoryRepository
{
    private readonly OpenUpToolDbContext _context;

    public UserStoryRepository(OpenUpToolDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<UserStory>> GetByProjectIdAsync(Guid projectId)
    {
        return await _context.UserStories
            .Where(us => us.ProjectId == projectId)
            .OrderByDescending(us => us.CreatedAt)
            .ToListAsync();
    }

    public async Task<UserStory?> GetByIdAsync(Guid id)
    {
        return await _context.UserStories.FindAsync(id);
    }

    public async Task<UserStory> CreateAsync(UserStory userStory)
    {
        userStory.CreatedAt = DateTime.UtcNow;
        userStory.UpdatedAt = DateTime.UtcNow;
        _context.UserStories.Add(userStory);
        await _context.SaveChangesAsync();
        return userStory;
    }

    public async Task<UserStory> UpdateAsync(UserStory userStory)
    {
        userStory.UpdatedAt = DateTime.UtcNow;
        _context.UserStories.Update(userStory);
        await _context.SaveChangesAsync();
        return userStory;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var userStory = await _context.UserStories.FindAsync(id);
        if (userStory == null) return false;

        _context.UserStories.Remove(userStory);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<UserStory>> GetByStatusAsync(Guid projectId, string status)
    {
        return await _context.UserStories
            .Where(us => us.ProjectId == projectId && us.Status == status)
            .OrderBy(us => us.Priority)
            .ToListAsync();
    }
}
