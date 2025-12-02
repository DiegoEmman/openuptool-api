using Microsoft.EntityFrameworkCore;
using OpenUpTool.Core.Entities;
using OpenUpTool.Core.Interfaces;
using OpenUpTool.Infrastructure.Data;

namespace OpenUpTool.Infrastructure.Repositories;

public class IterationTaskRepository : IIterationTaskRepository
{
    private readonly OpenUpToolDbContext _context;

    public IterationTaskRepository(OpenUpToolDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<IterationTask>> GetByIterationIdAsync(Guid iterationId)
    {
        return await _context.IterationTasks
            .Include(t => t.AssignedUser)
            .Where(t => t.IterationId == iterationId)
            .OrderBy(t => t.Priority)
            .ThenBy(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<IterationTask?> GetByIdAsync(Guid id)
    {
        return await _context.IterationTasks
            .Include(t => t.AssignedUser)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IterationTask> CreateAsync(IterationTask task)
    {
        task.CreatedAt = DateTime.UtcNow;
        task.UpdatedAt = DateTime.UtcNow;
        _context.IterationTasks.Add(task);
        await _context.SaveChangesAsync();
        return task;
    }

    public async Task<IterationTask> UpdateAsync(IterationTask task)
    {
        task.UpdatedAt = DateTime.UtcNow;
        _context.IterationTasks.Update(task);
        await _context.SaveChangesAsync();
        return task;
    }

    public async Task DeleteAsync(Guid id)
    {
        var task = await _context.IterationTasks.FindAsync(id);
        if (task != null)
        {
            _context.IterationTasks.Remove(task);
            await _context.SaveChangesAsync();
        }
    }
}

public class IterationProgressRepository : IIterationProgressRepository
{
    private readonly OpenUpToolDbContext _context;

    public IterationProgressRepository(OpenUpToolDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<IterationProgress>> GetByIterationIdAsync(Guid iterationId)
    {
        return await _context.IterationProgresses
            .Where(p => p.IterationId == iterationId)
            .OrderByDescending(p => p.RecordDate)
            .ToListAsync();
    }

    public async Task<IterationProgress?> GetByIdAsync(Guid id)
    {
        return await _context.IterationProgresses
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IterationProgress?> GetLatestByIterationIdAsync(Guid iterationId)
    {
        return await _context.IterationProgresses
            .Where(p => p.IterationId == iterationId)
            .OrderByDescending(p => p.RecordDate)
            .FirstOrDefaultAsync();
    }

    public async Task<IterationProgress> CreateAsync(IterationProgress progress)
    {
        progress.CreatedAt = DateTime.UtcNow;
        progress.UpdatedAt = DateTime.UtcNow;
        _context.IterationProgresses.Add(progress);
        await _context.SaveChangesAsync();
        return progress;
    }

    public async Task<IterationProgress> UpdateAsync(IterationProgress progress)
    {
        progress.UpdatedAt = DateTime.UtcNow;
        _context.IterationProgresses.Update(progress);
        await _context.SaveChangesAsync();
        return progress;
    }
}
