using Microsoft.EntityFrameworkCore;
using OpenUpTool.Core.Entities;
using OpenUpTool.Core.Interfaces;
using OpenUpTool.Infrastructure.Data;

namespace OpenUpTool.Infrastructure.Repositories;

public class MicroincrementRepository : IMicroincrementRepository
{
    private readonly OpenUpToolDbContext _context;

    public MicroincrementRepository(OpenUpToolDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Microincrement>> GetAllAsync()
    {
        return await _context.Microincrements
            .Include(m => m.Artifact)
            .Include(m => m.Iteration)
            .OrderByDescending(m => m.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<Microincrement>> GetByIterationIdAsync(Guid iterationId)
    {
        return await _context.Microincrements
            .Include(m => m.Artifact)
            .Include(m => m.Iteration)
            .Where(m => m.IterationId == iterationId)
            .OrderByDescending(m => m.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<Microincrement>> GetByArtifactIdAsync(Guid artifactId)
    {
        return await _context.Microincrements
            .Include(m => m.Artifact)
            .Include(m => m.Iteration)
            .Where(m => m.ArtifactId == artifactId)
            .OrderByDescending(m => m.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<Microincrement>> GetByAuthorAsync(string author)
    {
        return await _context.Microincrements
            .Include(m => m.Artifact)
            .Include(m => m.Iteration)
            .Where(m => m.Author == author)
            .OrderByDescending(m => m.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<Microincrement>> GetByTypeAsync(string type)
    {
        return await _context.Microincrements
            .Include(m => m.Artifact)
            .Include(m => m.Iteration)
            .Where(m => m.Type == type)
            .OrderByDescending(m => m.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<Microincrement>> GetFilteredAsync(
        Guid? iterationId, 
        Guid? artifactId, 
        string? author,
        string? type)
    {
        var query = _context.Microincrements
            .Include(m => m.Artifact)
            .Include(m => m.Iteration)
            .AsQueryable();

        if (iterationId.HasValue)
        {
            query = query.Where(m => m.IterationId == iterationId.Value);
        }

        if (artifactId.HasValue)
        {
            query = query.Where(m => m.ArtifactId == artifactId.Value);
        }

        if (!string.IsNullOrWhiteSpace(author))
        {
            query = query.Where(m => m.Author.Contains(author));
        }

        if (!string.IsNullOrWhiteSpace(type))
        {
            query = query.Where(m => m.Type == type);
        }

        return await query
            .OrderByDescending(m => m.Date)
            .ToListAsync();
    }

    public async Task<Microincrement?> GetByIdAsync(Guid id)
    {
        return await _context.Microincrements
            .Include(m => m.Artifact)
            .Include(m => m.Iteration)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<Microincrement> CreateAsync(Microincrement microincrement)
    {
        _context.Microincrements.Add(microincrement);
        await _context.SaveChangesAsync();
        return microincrement;
    }

    public async Task<Microincrement> UpdateAsync(Microincrement microincrement)
    {
        _context.Microincrements.Update(microincrement);
        await _context.SaveChangesAsync();
        return microincrement;
    }

    public async Task DeleteAsync(Guid id)
    {
        var microincrement = await _context.Microincrements.FindAsync(id);
        if (microincrement != null)
        {
            _context.Microincrements.Remove(microincrement);
            await _context.SaveChangesAsync();
        }
    }
}
