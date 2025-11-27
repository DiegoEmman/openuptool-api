using OpenUpTool.Core.Entities;

namespace OpenUpTool.Core.Interfaces;

public interface IProjectRepository
{
    Task<IEnumerable<Project>> GetAllAsync();
    Task<Project?> GetByIdAsync(Guid id);
    Task<Project> CreateAsync(Project project);
    Task<Project> UpdateAsync(Project project);
    Task DeleteAsync(Guid id);
}

public interface IPhaseRepository
{
    Task<IEnumerable<Phase>> GetByProjectIdAsync(Guid projectId);
    Task<Phase?> GetByIdAsync(Guid id);
    Task<Phase?> GetByProjectAndCodeAsync(Guid projectId, string phaseCode);
    Task<IEnumerable<Phase>> CreateManyAsync(IEnumerable<Phase> phases);
    Task<Phase> UpdateAsync(Phase phase);
}

public interface IProjectPlanRepository
{
    Task<ProjectPlan?> GetByProjectIdAsync(Guid projectId);
    Task<IEnumerable<ProjectPlan>> GetAllVersionsByProjectIdAsync(Guid projectId);
    Task<ProjectPlan?> GetByIdAsync(Guid id);
    Task<ProjectPlan> CreateAsync(ProjectPlan plan);
    Task<ProjectPlan> UpdateAsync(ProjectPlan plan);
}

public interface IIterationRepository
{
    Task<IEnumerable<Iteration>> GetByProjectIdAsync(Guid projectId);
    Task<Iteration?> GetByIdAsync(Guid id);
    Task<Iteration> CreateAsync(Iteration iteration);
    Task<Iteration> UpdateAsync(Iteration iteration);
}

public interface IArtifactRepository
{
    Task<IEnumerable<Artifact>> GetByProjectAndPhaseAsync(Guid projectId, string phaseId);
    Task<Artifact?> GetByIdAsync(Guid id);
    Task<Artifact> CreateAsync(Artifact artifact);
    Task<Artifact> UpdateAsync(Artifact artifact);
}

public interface IArtifactTypeRepository
{
    Task<IEnumerable<ArtifactType>> GetAllAsync();
    Task<IEnumerable<ArtifactType>> GetByPhaseAsync(string phase);
    Task<ArtifactType?> GetByIdAsync(Guid id);
    Task<ArtifactType> CreateAsync(ArtifactType artifactType);
    Task<ArtifactType> UpdateAsync(ArtifactType artifactType);
}

public interface IArtifactVersionRepository
{
    Task<IEnumerable<ArtifactVersion>> GetByArtifactIdAsync(Guid artifactId);
    Task<ArtifactVersion?> GetByIdAsync(Guid id);
    Task<ArtifactVersion> CreateAsync(ArtifactVersion version);
    Task<ArtifactVersion> UpdateAsync(ArtifactVersion version);
    Task DeleteAsync(Guid id);
}
