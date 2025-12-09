using OpenUpTool.Core.Entities;

namespace OpenUpTool.Core.Interfaces;

public interface IProjectRepository
{
    Task<IEnumerable<Project>> GetAllAsync();
    Task<IEnumerable<Project>> GetProjectsForUserAsync(Guid userId);
    Task<IEnumerable<Project>> GetArchivedProjectsForUserAsync(Guid userId);
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

public interface IMicroincrementRepository
{
    Task<IEnumerable<Microincrement>> GetAllAsync();
    Task<IEnumerable<Microincrement>> GetByIterationIdAsync(Guid iterationId);
    Task<IEnumerable<Microincrement>> GetByArtifactIdAsync(Guid artifactId);
    Task<IEnumerable<Microincrement>> GetByAuthorAsync(string author);
    Task<IEnumerable<Microincrement>> GetByTypeAsync(string type);
    Task<IEnumerable<Microincrement>> GetFilteredAsync(Guid? iterationId, Guid? artifactId, string? author, string? type);
    Task<Microincrement?> GetByIdAsync(Guid id);
    Task<Microincrement> CreateAsync(Microincrement microincrement);
    Task<Microincrement> UpdateAsync(Microincrement microincrement);
    Task DeleteAsync(Guid id);
}

public interface IProjectClosureRepository
{
    Task<IEnumerable<ProjectClosure>> GetAllAsync();
    Task<ProjectClosure?> GetByIdAsync(Guid id);
    Task<ProjectClosure?> GetByProjectIdAsync(Guid projectId);
    Task<ProjectClosure> CreateAsync(ProjectClosure closure);
    Task<ProjectClosure> UpdateAsync(ProjectClosure closure);
    Task DeleteAsync(Guid id);
}

public interface IFinalBuildRepository
{
    Task<IEnumerable<FinalBuild>> GetAllAsync();
    Task<IEnumerable<FinalBuild>> GetByProjectIdAsync(Guid projectId);
    Task<FinalBuild?> GetByIdAsync(Guid id);
    Task<FinalBuild?> GetByBuildNumberAsync(Guid projectId, string buildNumber);
    Task<FinalBuild> CreateAsync(FinalBuild build);
    Task<FinalBuild> UpdateAsync(FinalBuild build);
    Task DeleteAsync(Guid id);
}

public interface IWorkflowRepository
{
    Task<IEnumerable<Workflow>> GetAllAsync();
    Task<IEnumerable<Workflow>> GetByProjectIdAsync(Guid projectId);
    Task<Workflow?> GetByIdAsync(Guid id);
    Task<Workflow?> GetByIdWithStatesAsync(Guid id);
    Task<Workflow> CreateAsync(Workflow workflow);
    Task<Workflow> UpdateAsync(Workflow workflow);
    Task DeleteAsync(Guid id);
}

public interface IWorkflowStateRepository
{
    Task<IEnumerable<WorkflowState>> GetByWorkflowIdAsync(Guid workflowId);
    Task<WorkflowState?> GetByIdAsync(Guid id);
    Task<WorkflowState?> GetByIdWithResponsiblesAsync(Guid id);
    Task<WorkflowState> CreateAsync(WorkflowState state);
    Task<WorkflowState> UpdateAsync(WorkflowState state);
    Task DeleteAsync(Guid id);
}

public interface IWorkflowStateResponsibleRepository
{
    Task<IEnumerable<WorkflowStateResponsible>> GetByStateIdAsync(Guid stateId);
    Task<WorkflowStateResponsible?> GetByIdAsync(Guid id);
    Task<WorkflowStateResponsible> CreateAsync(WorkflowStateResponsible responsible);
    Task DeleteAsync(Guid id);
}

public interface IArtifactStateHistoryRepository
{
    Task<IEnumerable<ArtifactStateHistory>> GetByArtifactIdAsync(Guid artifactId);
    Task<ArtifactStateHistory?> GetByIdAsync(Guid id);
    Task<ArtifactStateHistory> CreateAsync(ArtifactStateHistory history);
}

public interface IWorkflowPermissionRepository
{
    Task<IEnumerable<WorkflowPermission>> GetByWorkflowIdAsync(Guid workflowId);
    Task<WorkflowPermission?> GetByIdAsync(Guid id);
    Task<WorkflowPermission?> GetByWorkflowRoleActionAsync(Guid workflowId, string role, string action);
    Task<WorkflowPermission> CreateAsync(WorkflowPermission permission);
    Task<WorkflowPermission> UpdateAsync(WorkflowPermission permission);
    Task DeleteAsync(Guid id);
}

// ========== HU-018: Global Configuration Repositories ==========

public interface IGlobalConfigurationRepository
{
    Task<IEnumerable<GlobalConfiguration>> GetAllAsync();
    Task<GlobalConfiguration?> GetByIdAsync(Guid id);
    Task<GlobalConfiguration?> GetByIdWithDetailsAsync(Guid id);
    Task<GlobalConfiguration?> GetDefaultAsync();
    Task<GlobalConfiguration> CreateAsync(GlobalConfiguration configuration);
    Task<GlobalConfiguration> UpdateAsync(GlobalConfiguration configuration);
    Task DeleteAsync(Guid id);
}

public interface IRoleTemplateRepository
{
    Task<IEnumerable<RoleTemplate>> GetByConfigurationIdAsync(Guid configurationId);
    Task<RoleTemplate?> GetByIdAsync(Guid id);
    Task<RoleTemplate> CreateAsync(RoleTemplate roleTemplate);
    Task<RoleTemplate> UpdateAsync(RoleTemplate roleTemplate);
    Task DeleteAsync(Guid id);
}

public interface IPhaseTemplateRepository
{
    Task<IEnumerable<PhaseTemplate>> GetByConfigurationIdAsync(Guid configurationId);
    Task<PhaseTemplate?> GetByIdAsync(Guid id);
    Task<PhaseTemplate> CreateAsync(PhaseTemplate phaseTemplate);
    Task<PhaseTemplate> UpdateAsync(PhaseTemplate phaseTemplate);
    Task DeleteAsync(Guid id);
}

public interface IArtifactTypeTemplateRepository
{
    Task<IEnumerable<ArtifactTypeTemplate>> GetByConfigurationIdAsync(Guid configurationId);
    Task<IEnumerable<ArtifactTypeTemplate>> GetByPhaseCodeAsync(Guid configurationId, string phaseCode);
    Task<ArtifactTypeTemplate?> GetByIdAsync(Guid id);
    Task<ArtifactTypeTemplate?> GetByIdWithCustomFieldsAsync(Guid id);
    Task<ArtifactTypeTemplate> CreateAsync(ArtifactTypeTemplate artifactTypeTemplate);
    Task<ArtifactTypeTemplate> UpdateAsync(ArtifactTypeTemplate artifactTypeTemplate);
    Task DeleteAsync(Guid id);
}

public interface IWorkflowTemplateRepository
{
    Task<IEnumerable<WorkflowTemplate>> GetByConfigurationIdAsync(Guid configurationId);
    Task<WorkflowTemplate?> GetByIdAsync(Guid id);
    Task<WorkflowTemplate?> GetByIdWithStatesAsync(Guid id);
    Task<WorkflowTemplate> CreateAsync(WorkflowTemplate workflowTemplate);
    Task<WorkflowTemplate> UpdateAsync(WorkflowTemplate workflowTemplate);
    Task DeleteAsync(Guid id);
}

public interface IWorkflowStateTemplateRepository
{
    Task<IEnumerable<WorkflowStateTemplate>> GetByWorkflowTemplateIdAsync(Guid workflowTemplateId);
    Task<WorkflowStateTemplate?> GetByIdAsync(Guid id);
    Task<WorkflowStateTemplate> CreateAsync(WorkflowStateTemplate stateTemplate);
    Task<WorkflowStateTemplate> UpdateAsync(WorkflowStateTemplate stateTemplate);
    Task DeleteAsync(Guid id);
}

public interface ICustomFieldDefinitionRepository
{
    Task<IEnumerable<CustomFieldDefinition>> GetByArtifactTypeTemplateIdAsync(Guid artifactTypeTemplateId);
    Task<IEnumerable<CustomFieldDefinition>> GetByConfigurationIdAsync(Guid configurationId);
    Task<CustomFieldDefinition?> GetByIdAsync(Guid id);
    Task<CustomFieldDefinition> CreateAsync(CustomFieldDefinition customField);
    Task<CustomFieldDefinition> UpdateAsync(CustomFieldDefinition customField);
    Task DeleteAsync(Guid id);
}

public interface IConfigurationChangeHistoryRepository
{
    Task<IEnumerable<ConfigurationChangeHistory>> GetByConfigurationIdAsync(Guid configurationId);
    Task<IEnumerable<ConfigurationChangeHistory>> GetByConfigurationIdAndVersionAsync(Guid configurationId, int version);
    Task<ConfigurationChangeHistory?> GetByIdAsync(Guid id);
    Task<ConfigurationChangeHistory> CreateAsync(ConfigurationChangeHistory history);
}

public interface IProjectConfigurationRepository
{
    Task<IEnumerable<ProjectConfiguration>> GetAllAsync();
    Task<IEnumerable<ProjectConfiguration>> GetByConfigurationIdAsync(Guid configurationId);
    Task<ProjectConfiguration?> GetByProjectIdAsync(Guid projectId);
    Task<ProjectConfiguration?> GetByIdAsync(Guid id);
    Task<ProjectConfiguration> CreateAsync(ProjectConfiguration projectConfiguration);
    Task<ProjectConfiguration> UpdateAsync(ProjectConfiguration projectConfiguration);
    Task DeleteAsync(Guid id);
}

public interface IArtifactCustomFieldValueRepository
{
    Task<IEnumerable<ArtifactCustomFieldValue>> GetByArtifactIdAsync(Guid artifactId);
    Task<ArtifactCustomFieldValue?> GetByIdAsync(Guid id);
    Task<ArtifactCustomFieldValue?> GetByArtifactAndFieldAsync(Guid artifactId, Guid customFieldDefinitionId);
    Task<ArtifactCustomFieldValue> CreateAsync(ArtifactCustomFieldValue value);
    Task<ArtifactCustomFieldValue> UpdateAsync(ArtifactCustomFieldValue value);
    Task DeleteAsync(Guid id);
    Task DeleteByArtifactIdAsync(Guid artifactId);
}

// HU-020: Repositorio para historial de movimientos de artefactos
public interface IArtifactMovementHistoryRepository
{
    Task<IEnumerable<ArtifactMovementHistory>> GetByArtifactIdAsync(Guid artifactId);
    Task<ArtifactMovementHistory?> GetByIdAsync(Guid id);
    Task<ArtifactMovementHistory> CreateAsync(ArtifactMovementHistory history);
}
