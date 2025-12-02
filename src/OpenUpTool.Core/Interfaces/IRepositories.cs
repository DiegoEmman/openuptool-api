using OpenUpTool.Core.Entities;

namespace OpenUpTool.Core.Interfaces;

public interface IProjectRepository
{
    Task<IEnumerable<Project>> GetAllAsync();
    Task<IEnumerable<Project>> GetProjectsForUserAsync(Guid userId);
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

public interface IIterationTaskRepository
{
    Task<IEnumerable<IterationTask>> GetByIterationIdAsync(Guid iterationId);
    Task<IterationTask?> GetByIdAsync(Guid id);
    Task<IterationTask> CreateAsync(IterationTask task);
    Task<IterationTask> UpdateAsync(IterationTask task);
    Task DeleteAsync(Guid id);
}

public interface IIterationProgressRepository
{
    Task<IEnumerable<IterationProgress>> GetByIterationIdAsync(Guid iterationId);
    Task<IterationProgress?> GetByIdAsync(Guid id);
    Task<IterationProgress?> GetLatestByIterationIdAsync(Guid iterationId);
    Task<IterationProgress> CreateAsync(IterationProgress progress);
    Task<IterationProgress> UpdateAsync(IterationProgress progress);
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
    Task<IEnumerable<ArtifactVersion>> GetVersionsByArtifactIdAsync(Guid artifactId);
    Task<ArtifactVersion?> GetByIdAsync(Guid id);
    Task AddAsync(ArtifactVersion version);
    Task<ArtifactVersion> UpdateAsync(ArtifactVersion version);
    Task DeleteAsync(Guid id);
}

public interface ITestExecutionRepository
{
    Task<IEnumerable<TestExecution>> GetByArtifactIdAsync(Guid artifactId);
    Task<IEnumerable<TestExecution>> GetByTestCaseIdAsync(Guid artifactId, string testCaseId);
    Task<TestExecution?> GetByIdAsync(Guid id);
    Task<TestExecution> CreateAsync(TestExecution execution);
    Task<TestExecution> UpdateAsync(TestExecution execution);
    Task DeleteAsync(Guid id);
}

public interface IDefectRepository
{
    Task<IEnumerable<Defect>> GetByProjectIdAsync(Guid projectId);
    Task<IEnumerable<Defect>> GetByArtifactIdAsync(Guid artifactId);
    Task<IEnumerable<Defect>> GetByTestExecutionIdAsync(Guid testExecutionId);
    Task<IEnumerable<Defect>> GetByStatusAsync(Guid projectId, string status);
    Task<IEnumerable<Defect>> GetByAssigneeAsync(Guid assignedTo);
    Task<Defect?> GetByIdAsync(Guid id);
    Task<Defect?> GetByDefectNumberAsync(Guid projectId, string defectNumber);
    Task<string> GenerateNextDefectNumberAsync(Guid projectId);
    Task<Defect> CreateAsync(Defect defect);
    Task<Defect> UpdateAsync(Defect defect);
    Task DeleteAsync(Guid id);
}
