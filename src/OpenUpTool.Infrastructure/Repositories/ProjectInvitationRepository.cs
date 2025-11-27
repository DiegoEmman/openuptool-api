using Microsoft.EntityFrameworkCore;
using OpenUpTool.Core.Entities;
using OpenUpTool.Core.Interfaces;
using OpenUpTool.Infrastructure.Data;

namespace OpenUpTool.Infrastructure.Repositories;

public class ProjectInvitationRepository : IProjectInvitationRepository
{
    private readonly OpenUpToolDbContext _context;

    public ProjectInvitationRepository(OpenUpToolDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ProjectInvitation>> GetByProjectIdAsync(Guid projectId)
    {
        return await _context.ProjectInvitations
            .Include(i => i.Project)
            .Include(i => i.Role)
            .Include(i => i.Inviter)
            .Where(i => i.ProjectId == projectId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProjectInvitation>> GetByEmailAsync(string email)
    {
        return await _context.ProjectInvitations
            .Include(i => i.Project)
            .Include(i => i.Role)
            .Include(i => i.Inviter)
            .Where(i => i.InvitedEmail.ToLower() == email.ToLower())
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();
    }

    public async Task<ProjectInvitation?> GetByIdAsync(Guid id)
    {
        return await _context.ProjectInvitations
            .Include(i => i.Project)
            .Include(i => i.Role)
            .Include(i => i.Inviter)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<ProjectInvitation?> GetByTokenAsync(string token)
    {
        return await _context.ProjectInvitations
            .Include(i => i.Project)
            .Include(i => i.Role)
            .Include(i => i.Inviter)
            .FirstOrDefaultAsync(i => i.InvitationToken == token);
    }

    public async Task<ProjectInvitation> CreateAsync(ProjectInvitation invitation)
    {
        invitation.CreatedAt = DateTime.UtcNow;
        invitation.UpdatedAt = DateTime.UtcNow;
        _context.ProjectInvitations.Add(invitation);
        await _context.SaveChangesAsync();
        return invitation;
    }

    public async Task<ProjectInvitation> UpdateAsync(ProjectInvitation invitation)
    {
        invitation.UpdatedAt = DateTime.UtcNow;
        _context.ProjectInvitations.Update(invitation);
        await _context.SaveChangesAsync();
        return invitation;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var invitation = await _context.ProjectInvitations.FindAsync(id);
        if (invitation == null) return false;

        _context.ProjectInvitations.Remove(invitation);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<ProjectInvitation>> GetPendingByEmailAsync(string email)
    {
        return await _context.ProjectInvitations
            .Include(i => i.Project)
            .Include(i => i.Role)
            .Include(i => i.Inviter)
            .Where(i => i.InvitedEmail.ToLower() == email.ToLower() 
                     && i.Status == "pending" 
                     && i.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();
    }
}
