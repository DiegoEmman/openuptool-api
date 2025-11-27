using Microsoft.EntityFrameworkCore;
using OpenUpTool.Core.Entities;
using OpenUpTool.Core.Interfaces;
using OpenUpTool.Infrastructure.Data;

namespace OpenUpTool.Infrastructure.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly OpenUpToolDbContext _context;

    public ProjectRepository(OpenUpToolDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Project>> GetAllAsync()
    {
        return await _context.Projects
            .Include(p => p.Phases.OrderBy(ph => ph.OrderIndex))
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Project>> GetProjectsForUserAsync(Guid userId)
    {
        // Obtener proyectos donde el usuario tiene un rol asignado
        var projectIds = await _context.ProjectUserRoles
            .Where(pur => pur.UserId == userId && pur.Status == "active")
            .Select(pur => pur.ProjectId)
            .Distinct()
            .ToListAsync();

        return await _context.Projects
            .Where(p => projectIds.Contains(p.Id))
            .Include(p => p.Phases.OrderBy(ph => ph.OrderIndex))
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<Project?> GetByIdAsync(Guid id)
    {
        return await _context.Projects
            .Include(p => p.Phases.OrderBy(ph => ph.OrderIndex))
            .Include(p => p.Plan)
            .ThenInclude(pl => pl!.Milestones)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Project> CreateAsync(Project project)
    {
        // No establecer CreatedAt/UpdatedAt - la BD los maneja con DEFAULT NOW()
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();
        return project;
    }

    public async Task<Project> UpdateAsync(Project project)
    {
        // No establecer UpdatedAt - el trigger de la BD lo maneja
        _context.Projects.Update(project);
        await _context.SaveChangesAsync();
        return project;
    }

    public async Task DeleteAsync(Guid id)
    {
        var project = await _context.Projects.FindAsync(id);
        if (project != null)
        {
            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
        }
    }
}

public class PhaseRepository : IPhaseRepository
{
    private readonly OpenUpToolDbContext _context;

    public PhaseRepository(OpenUpToolDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Phase>> GetByProjectIdAsync(Guid projectId)
    {
        return await _context.Phases
            .Where(p => p.ProjectId == projectId)
            .OrderBy(p => p.OrderIndex)
            .ToListAsync();
    }

    public async Task<Phase?> GetByIdAsync(Guid id)
    {
        return await _context.Phases.FindAsync(id);
    }

    public async Task<Phase?> GetByProjectAndCodeAsync(Guid projectId, string phaseCode)
    {
        return await _context.Phases
            .FirstOrDefaultAsync(p => p.ProjectId == projectId && p.PhaseCode == phaseCode);
    }

    public async Task<IEnumerable<Phase>> CreateManyAsync(IEnumerable<Phase> phases)
    {
        // No establecer CreatedAt/UpdatedAt - la BD los maneja
        await _context.Phases.AddRangeAsync(phases);
        await _context.SaveChangesAsync();
        return phases;
    }

    public async Task<Phase> UpdateAsync(Phase phase)
    {
        phase.UpdatedAt = DateTime.UtcNow;
        _context.Phases.Update(phase);
        await _context.SaveChangesAsync();
        return phase;
    }
}

public class ProjectPlanRepository : IProjectPlanRepository
{
    private readonly OpenUpToolDbContext _context;

    public ProjectPlanRepository(OpenUpToolDbContext context)
    {
        _context = context;
    }

    public async Task<ProjectPlan?> GetByProjectIdAsync(Guid projectId)
    {
        return await _context.ProjectPlans
            .Include(p => p.Milestones)
            .Where(p => p.ProjectId == projectId && p.IsActive)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<ProjectPlan>> GetAllVersionsByProjectIdAsync(Guid projectId)
    {
        return await _context.ProjectPlans
            .Include(p => p.Milestones)
            .Where(p => p.ProjectId == projectId)
            .OrderByDescending(p => p.Version)
            .ToListAsync();
    }

    public async Task<ProjectPlan?> GetByIdAsync(Guid id)
    {
        return await _context.ProjectPlans
            .Include(p => p.Milestones)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<ProjectPlan> CreateAsync(ProjectPlan plan)
    {
        plan.CreatedAt = DateTime.UtcNow;
        plan.UpdatedAt = DateTime.UtcNow;
        
        foreach (var milestone in plan.Milestones)
        {
            milestone.CreatedAt = DateTime.UtcNow;
            milestone.UpdatedAt = DateTime.UtcNow;
        }

        _context.ProjectPlans.Add(plan);
        await _context.SaveChangesAsync();
        return plan;
    }

    public async Task<ProjectPlan> UpdateAsync(ProjectPlan plan)
    {
        plan.UpdatedAt = DateTime.UtcNow;
        _context.ProjectPlans.Update(plan);
        await _context.SaveChangesAsync();
        return plan;
    }
}

public class IterationRepository : IIterationRepository
{
    private readonly OpenUpToolDbContext _context;

    public IterationRepository(OpenUpToolDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Iteration>> GetByProjectIdAsync(Guid projectId)
    {
        return await _context.Iterations
            .Where(i => i.ProjectId == projectId)
            .OrderBy(i => i.StartDate)
            .ToListAsync();
    }

    public async Task<Iteration?> GetByIdAsync(Guid id)
    {
        return await _context.Iterations.FindAsync(id);
    }

    public async Task<Iteration> CreateAsync(Iteration iteration)
    {
        iteration.CreatedAt = DateTime.UtcNow;
        iteration.UpdatedAt = DateTime.UtcNow;
        _context.Iterations.Add(iteration);
        await _context.SaveChangesAsync();
        return iteration;
    }

    public async Task<Iteration> UpdateAsync(Iteration iteration)
    {
        iteration.UpdatedAt = DateTime.UtcNow;
        _context.Iterations.Update(iteration);
        await _context.SaveChangesAsync();
        return iteration;
    }
}

public class ArtifactRepository : IArtifactRepository
{
    private readonly OpenUpToolDbContext _context;

    public ArtifactRepository(OpenUpToolDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Artifact>> GetByProjectAndPhaseAsync(Guid projectId, string phaseId)
    {
        return await _context.Artifacts
            .Include(a => a.ArtifactType)
            .Where(a => a.ProjectId == projectId && a.PhaseId == phaseId)
            .OrderBy(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<Artifact?> GetByIdAsync(Guid id)
    {
        return await _context.Artifacts
            .Include(a => a.ArtifactType)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Artifact> CreateAsync(Artifact artifact)
    {
        artifact.CreatedAt = DateTime.UtcNow;
        artifact.UpdatedAt = DateTime.UtcNow;
        _context.Artifacts.Add(artifact);
        await _context.SaveChangesAsync();
        return artifact;
    }

    public async Task<Artifact> UpdateAsync(Artifact artifact)
    {
        artifact.UpdatedAt = DateTime.UtcNow;
        _context.Artifacts.Update(artifact);
        await _context.SaveChangesAsync();
        return artifact;
    }
}

public class ArtifactTypeRepository : IArtifactTypeRepository
{
    private readonly OpenUpToolDbContext _context;

    public ArtifactTypeRepository(OpenUpToolDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ArtifactType>> GetAllAsync()
    {
        return await _context.ArtifactTypes.ToListAsync();
    }

    public async Task<IEnumerable<ArtifactType>> GetByPhaseAsync(string phase)
    {
        return await _context.ArtifactTypes
            .Where(at => at.Phase == phase)
            .ToListAsync();
    }

    public async Task<ArtifactType?> GetByIdAsync(Guid id)
    {
        return await _context.ArtifactTypes.FindAsync(id);
    }

    public async Task<ArtifactType> CreateAsync(ArtifactType artifactType)
    {
        artifactType.CreatedAt = DateTime.UtcNow;
        artifactType.UpdatedAt = DateTime.UtcNow;
        _context.ArtifactTypes.Add(artifactType);
        await _context.SaveChangesAsync();
        return artifactType;
    }

    public async Task<ArtifactType> UpdateAsync(ArtifactType artifactType)
    {
        artifactType.UpdatedAt = DateTime.UtcNow;
        _context.ArtifactTypes.Update(artifactType);
        await _context.SaveChangesAsync();
        return artifactType;
    }
}

public class ArtifactVersionRepository : IArtifactVersionRepository
{
    private readonly OpenUpToolDbContext _context;

    public ArtifactVersionRepository(OpenUpToolDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ArtifactVersion>> GetByArtifactIdAsync(Guid artifactId)
    {
        return await _context.ArtifactVersions
            .Where(v => v.ArtifactId == artifactId)
            .OrderByDescending(v => v.VersionNumber)
            .ToListAsync();
    }

    public async Task<ArtifactVersion?> GetByIdAsync(Guid id)
    {
        return await _context.ArtifactVersions.FindAsync(id);
    }

    public async Task<ArtifactVersion> CreateAsync(ArtifactVersion version)
    {
        version.CreatedAt = DateTime.UtcNow;
        version.UpdatedAt = DateTime.UtcNow;
        _context.ArtifactVersions.Add(version);
        await _context.SaveChangesAsync();
        return version;
    }

    public async Task<ArtifactVersion> UpdateAsync(ArtifactVersion version)
    {
        version.UpdatedAt = DateTime.UtcNow;
        _context.ArtifactVersions.Update(version);
        await _context.SaveChangesAsync();
        return version;
    }

    public async Task DeleteAsync(Guid id)
    {
        var version = await _context.ArtifactVersions.FindAsync(id);
        if (version != null)
        {
            _context.ArtifactVersions.Remove(version);
            await _context.SaveChangesAsync();
        }
    }
}
