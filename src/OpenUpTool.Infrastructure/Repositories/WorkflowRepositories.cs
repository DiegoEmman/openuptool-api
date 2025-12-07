using Microsoft.EntityFrameworkCore;
using OpenUpTool.Core.Entities;
using OpenUpTool.Core.Interfaces;
using OpenUpTool.Infrastructure.Data;

namespace OpenUpTool.Infrastructure.Repositories;

public class WorkflowRepository : IWorkflowRepository
{
    private readonly OpenUpToolDbContext _context;

    public WorkflowRepository(OpenUpToolDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Workflow>> GetAllAsync()
    {
        return await _context.Workflows
            .Include(w => w.Project)
            .Include(w => w.States)
            .OrderBy(w => w.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Workflow>> GetByProjectIdAsync(Guid projectId)
    {
        return await _context.Workflows
            .Include(w => w.States)
            .Where(w => w.ProjectId == projectId)
            .OrderBy(w => w.Name)
            .ToListAsync();
    }

    public async Task<Workflow?> GetByIdAsync(Guid id)
    {
        return await _context.Workflows
            .Include(w => w.Project)
            .FirstOrDefaultAsync(w => w.Id == id);
    }

    public async Task<Workflow?> GetByIdWithStatesAsync(Guid id)
    {
        return await _context.Workflows
            .Include(w => w.Project)
            .Include(w => w.States.OrderBy(s => s.Order))
                .ThenInclude(s => s.Responsibles)
                    .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(w => w.Id == id);
    }

    public async Task<Workflow> CreateAsync(Workflow workflow)
    {
        workflow.CreatedAt = DateTime.UtcNow;
        workflow.UpdatedAt = DateTime.UtcNow;
        _context.Workflows.Add(workflow);
        await _context.SaveChangesAsync();
        return workflow;
    }

    public async Task<Workflow> UpdateAsync(Workflow workflow)
    {
        workflow.UpdatedAt = DateTime.UtcNow;
        _context.Workflows.Update(workflow);
        await _context.SaveChangesAsync();
        return workflow;
    }

    public async Task DeleteAsync(Guid id)
    {
        var workflow = await _context.Workflows.FindAsync(id);
        if (workflow != null)
        {
            _context.Workflows.Remove(workflow);
            await _context.SaveChangesAsync();
        }
    }
}

public class WorkflowStateRepository : IWorkflowStateRepository
{
    private readonly OpenUpToolDbContext _context;

    public WorkflowStateRepository(OpenUpToolDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<WorkflowState>> GetByWorkflowIdAsync(Guid workflowId)
    {
        return await _context.WorkflowStates
            .Include(s => s.Responsibles)
                .ThenInclude(r => r.User)
            .Where(s => s.WorkflowId == workflowId)
            .OrderBy(s => s.Order)
            .ToListAsync();
    }

    public async Task<WorkflowState?> GetByIdAsync(Guid id)
    {
        return await _context.WorkflowStates
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<WorkflowState?> GetByIdWithResponsiblesAsync(Guid id)
    {
        return await _context.WorkflowStates
            .Include(s => s.Responsibles)
                .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<WorkflowState> CreateAsync(WorkflowState state)
    {
        state.CreatedAt = DateTime.UtcNow;
        state.UpdatedAt = DateTime.UtcNow;
        _context.WorkflowStates.Add(state);
        await _context.SaveChangesAsync();
        return state;
    }

    public async Task<WorkflowState> UpdateAsync(WorkflowState state)
    {
        state.UpdatedAt = DateTime.UtcNow;
        _context.WorkflowStates.Update(state);
        await _context.SaveChangesAsync();
        return state;
    }

    public async Task DeleteAsync(Guid id)
    {
        var state = await _context.WorkflowStates.FindAsync(id);
        if (state != null)
        {
            _context.WorkflowStates.Remove(state);
            await _context.SaveChangesAsync();
        }
    }
}

public class WorkflowStateResponsibleRepository : IWorkflowStateResponsibleRepository
{
    private readonly OpenUpToolDbContext _context;

    public WorkflowStateResponsibleRepository(OpenUpToolDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<WorkflowStateResponsible>> GetByStateIdAsync(Guid stateId)
    {
        return await _context.WorkflowStateResponsibles
            .Include(r => r.User)
            .Where(r => r.WorkflowStateId == stateId)
            .ToListAsync();
    }

    public async Task<WorkflowStateResponsible?> GetByIdAsync(Guid id)
    {
        return await _context.WorkflowStateResponsibles
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<WorkflowStateResponsible> CreateAsync(WorkflowStateResponsible responsible)
    {
        responsible.AssignedAt = DateTime.UtcNow;
        _context.WorkflowStateResponsibles.Add(responsible);
        await _context.SaveChangesAsync();
        
        // Reload with User navigation property
        return await _context.WorkflowStateResponsibles
            .Include(r => r.User)
            .FirstAsync(r => r.Id == responsible.Id);
    }

    public async Task DeleteAsync(Guid id)
    {
        var responsible = await _context.WorkflowStateResponsibles.FindAsync(id);
        if (responsible != null)
        {
            _context.WorkflowStateResponsibles.Remove(responsible);
            await _context.SaveChangesAsync();
        }
    }
}

public class ArtifactStateHistoryRepository : IArtifactStateHistoryRepository
{
    private readonly OpenUpToolDbContext _context;

    public ArtifactStateHistoryRepository(OpenUpToolDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ArtifactStateHistory>> GetByArtifactIdAsync(Guid artifactId)
    {
        return await _context.ArtifactStateHistories
            .Include(h => h.FromState)
            .Include(h => h.ToState)
            .Include(h => h.ChangedByUser)
            .Where(h => h.ArtifactId == artifactId)
            .OrderByDescending(h => h.ChangedAt)
            .ToListAsync();
    }

    public async Task<ArtifactStateHistory?> GetByIdAsync(Guid id)
    {
        return await _context.ArtifactStateHistories
            .Include(h => h.FromState)
            .Include(h => h.ToState)
            .Include(h => h.ChangedByUser)
            .FirstOrDefaultAsync(h => h.Id == id);
    }

    public async Task<ArtifactStateHistory> CreateAsync(ArtifactStateHistory history)
    {
        history.ChangedAt = DateTime.UtcNow;
        _context.ArtifactStateHistories.Add(history);
        await _context.SaveChangesAsync();
        return history;
    }
}

public class WorkflowPermissionRepository : IWorkflowPermissionRepository
{
    private readonly OpenUpToolDbContext _context;

    public WorkflowPermissionRepository(OpenUpToolDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<WorkflowPermission>> GetByWorkflowIdAsync(Guid workflowId)
    {
        return await _context.WorkflowPermissions
            .Where(p => p.WorkflowId == workflowId)
            .OrderBy(p => p.Role)
            .ThenBy(p => p.Action)
            .ToListAsync();
    }

    public async Task<WorkflowPermission?> GetByIdAsync(Guid id)
    {
        return await _context.WorkflowPermissions.FindAsync(id);
    }

    public async Task<WorkflowPermission?> GetByWorkflowRoleActionAsync(Guid workflowId, string role, string action)
    {
        return await _context.WorkflowPermissions
            .FirstOrDefaultAsync(p => p.WorkflowId == workflowId && p.Role == role && p.Action == action);
    }

    public async Task<WorkflowPermission> CreateAsync(WorkflowPermission permission)
    {
        permission.CreatedAt = DateTime.UtcNow;
        _context.WorkflowPermissions.Add(permission);
        await _context.SaveChangesAsync();
        return permission;
    }

    public async Task<WorkflowPermission> UpdateAsync(WorkflowPermission permission)
    {
        permission.UpdatedAt = DateTime.UtcNow;
        _context.WorkflowPermissions.Update(permission);
        await _context.SaveChangesAsync();
        return permission;
    }

    public async Task DeleteAsync(Guid id)
    {
        var permission = await _context.WorkflowPermissions.FindAsync(id);
        if (permission != null)
        {
            _context.WorkflowPermissions.Remove(permission);
            await _context.SaveChangesAsync();
        }
    }
}
