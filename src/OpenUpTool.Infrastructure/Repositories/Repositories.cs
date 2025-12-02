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
            .Include(a => a.Versions)
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

    public async Task<IEnumerable<ArtifactVersion>> GetVersionsByArtifactIdAsync(Guid artifactId)
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

    public async Task AddAsync(ArtifactVersion version)
    {
        version.CreatedAt = DateTime.UtcNow;
        version.UpdatedAt = DateTime.UtcNow;
        _context.ArtifactVersions.Add(version);
        await _context.SaveChangesAsync();
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

public class TestExecutionRepository : ITestExecutionRepository
{
    private readonly OpenUpToolDbContext _context;

    public TestExecutionRepository(OpenUpToolDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TestExecution>> GetByArtifactIdAsync(Guid artifactId)
    {
        return await _context.TestExecutions
            .Include(te => te.Artifact)
            .Include(te => te.ArtifactVersion)
            .Include(te => te.RelatedDefects)
            .Where(te => te.ArtifactId == artifactId)
            .OrderByDescending(te => te.ExecutedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<TestExecution>> GetByTestCaseIdAsync(Guid artifactId, string testCaseId)
    {
        return await _context.TestExecutions
            .Include(te => te.Artifact)
            .Include(te => te.ArtifactVersion)
            .Include(te => te.RelatedDefects)
            .Where(te => te.ArtifactId == artifactId && te.TestCaseId == testCaseId)
            .OrderByDescending(te => te.ExecutedAt)
            .ToListAsync();
    }

    public async Task<TestExecution?> GetByIdAsync(Guid id)
    {
        return await _context.TestExecutions
            .Include(te => te.Artifact)
            .Include(te => te.ArtifactVersion)
            .Include(te => te.RelatedDefects)
            .FirstOrDefaultAsync(te => te.Id == id);
    }

    public async Task<TestExecution> CreateAsync(TestExecution testExecution)
    {
        _context.TestExecutions.Add(testExecution);
        await _context.SaveChangesAsync();
        return testExecution;
    }

    public async Task<TestExecution> UpdateAsync(TestExecution testExecution)
    {
        _context.TestExecutions.Update(testExecution);
        await _context.SaveChangesAsync();
        return testExecution;
    }

    public async Task DeleteAsync(Guid id)
    {
        var execution = await _context.TestExecutions.FindAsync(id);
        if (execution != null)
        {
            _context.TestExecutions.Remove(execution);
            await _context.SaveChangesAsync();
        }
    }
}

public class DefectRepository : IDefectRepository
{
    private readonly OpenUpToolDbContext _context;

    public DefectRepository(OpenUpToolDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Defect>> GetByProjectIdAsync(Guid projectId)
    {
        return await _context.Defects
            .Include(d => d.Project)
            .Include(d => d.Artifact)
            .Include(d => d.ArtifactVersion)
            .Include(d => d.TestExecution)
            .Where(d => d.ProjectId == projectId)
            .OrderByDescending(d => d.ReportedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Defect>> GetByArtifactIdAsync(Guid artifactId)
    {
        return await _context.Defects
            .Include(d => d.Project)
            .Include(d => d.Artifact)
            .Include(d => d.ArtifactVersion)
            .Include(d => d.TestExecution)
            .Where(d => d.ArtifactId == artifactId)
            .OrderByDescending(d => d.ReportedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Defect>> GetByTestExecutionIdAsync(Guid testExecutionId)
    {
        return await _context.Defects
            .Include(d => d.Project)
            .Include(d => d.Artifact)
            .Include(d => d.ArtifactVersion)
            .Include(d => d.TestExecution)
            .Where(d => d.TestExecutionId == testExecutionId)
            .OrderByDescending(d => d.ReportedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Defect>> GetByStatusAsync(Guid projectId, string status)
    {
        return await _context.Defects
            .Include(d => d.Project)
            .Include(d => d.Artifact)
            .Include(d => d.ArtifactVersion)
            .Include(d => d.TestExecution)
            .Where(d => d.ProjectId == projectId && d.Status == status)
            .OrderByDescending(d => d.ReportedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Defect>> GetByAssigneeAsync(Guid assigneeId)
    {
        return await _context.Defects
            .Include(d => d.Project)
            .Include(d => d.Artifact)
            .Include(d => d.ArtifactVersion)
            .Include(d => d.TestExecution)
            .Where(d => d.AssignedTo == assigneeId)
            .OrderByDescending(d => d.ReportedAt)
            .ToListAsync();
    }

    public async Task<Defect?> GetByIdAsync(Guid id)
    {
        return await _context.Defects
            .Include(d => d.Project)
            .Include(d => d.Artifact)
            .Include(d => d.ArtifactVersion)
            .Include(d => d.TestExecution)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<Defect?> GetByDefectNumberAsync(Guid projectId, string defectNumber)
    {
        return await _context.Defects
            .Include(d => d.Project)
            .Include(d => d.Artifact)
            .Include(d => d.ArtifactVersion)
            .Include(d => d.TestExecution)
            .FirstOrDefaultAsync(d => d.ProjectId == projectId && d.DefectNumber == defectNumber);
    }

    public async Task<string> GenerateNextDefectNumberAsync(Guid projectId)
    {
        var lastDefect = await _context.Defects
            .Where(d => d.ProjectId == projectId)
            .OrderByDescending(d => d.DefectNumber)
            .FirstOrDefaultAsync();

        if (lastDefect == null)
        {
            return "DEF-001";
        }

        // Extract number from format DEF-XXX
        var parts = lastDefect.DefectNumber.Split('-');
        if (parts.Length == 2 && int.TryParse(parts[1], out int lastNumber))
        {
            var nextNumber = lastNumber + 1;
            return $"DEF-{nextNumber:D3}";
        }

        return "DEF-001";
    }

    public async Task<Defect> CreateAsync(Defect defect)
    {
        _context.Defects.Add(defect);
        await _context.SaveChangesAsync();
        return defect;
    }

    public async Task<Defect> UpdateAsync(Defect defect)
    {
        _context.Defects.Update(defect);
        await _context.SaveChangesAsync();
        return defect;
    }

    public async Task DeleteAsync(Guid id)
    {
        var defect = await _context.Defects.FindAsync(id);
        if (defect != null)
        {
            _context.Defects.Remove(defect);
            await _context.SaveChangesAsync();
        }
    }
}

