using Microsoft.EntityFrameworkCore;
using OpenUpTool.Core.Entities;
using OpenUpTool.Core.Interfaces;
using OpenUpTool.Infrastructure.Data;

namespace OpenUpTool.Infrastructure.Repositories;

public class ProjectUserRoleRepository : IProjectUserRoleRepository
{
    private readonly OpenUpToolDbContext _context;

    public ProjectUserRoleRepository(OpenUpToolDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ProjectUserRole>> GetByProjectIdAsync(Guid projectId)
    {
        return await _context.ProjectUserRoles
            .Include(pur => pur.User)
            .Include(pur => pur.Role)
            .Where(pur => pur.ProjectId == projectId)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProjectUserRole>> GetByUserIdAsync(Guid userId)
    {
        return await _context.ProjectUserRoles
            .Include(pur => pur.Project)
            .Include(pur => pur.Role)
            .Where(pur => pur.UserId == userId)
            .ToListAsync();
    }

    public async Task<ProjectUserRole?> GetByIdAsync(Guid id)
    {
        return await _context.ProjectUserRoles
            .Include(pur => pur.User)
            .Include(pur => pur.Role)
            .Include(pur => pur.Project)
            .FirstOrDefaultAsync(pur => pur.Id == id);
    }

    public async Task<bool> HasUserAccessToProjectAsync(Guid userId, Guid projectId)
    {
        return await _context.ProjectUserRoles
            .AnyAsync(pur => pur.UserId == userId && pur.ProjectId == projectId && pur.Status == "active");
    }

    public async Task<ProjectUserRole> CreateAsync(ProjectUserRole projectUserRole)
    {
        projectUserRole.CreatedAt = DateTime.UtcNow;
        projectUserRole.UpdatedAt = DateTime.UtcNow;
        _context.ProjectUserRoles.Add(projectUserRole);
        await _context.SaveChangesAsync();
        return projectUserRole;
    }

    public async Task<ProjectUserRole> UpdateAsync(ProjectUserRole projectUserRole)
    {
        projectUserRole.UpdatedAt = DateTime.UtcNow;
        _context.ProjectUserRoles.Update(projectUserRole);
        await _context.SaveChangesAsync();
        return projectUserRole;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var projectUserRole = await _context.ProjectUserRoles.FindAsync(id);
        if (projectUserRole == null) return false;

        _context.ProjectUserRoles.Remove(projectUserRole);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UserHasRoleInProjectAsync(Guid userId, Guid projectId, string roleName)
    {
        return await _context.ProjectUserRoles
            .Include(pur => pur.Role)
            .AnyAsync(pur => pur.UserId == userId 
                          && pur.ProjectId == projectId 
                          && pur.Role.Name == roleName 
                          && pur.Status == "active");
    }
}
