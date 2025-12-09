using OpenUpTool.Core.DTOs;

namespace OpenUpTool.Core.Interfaces;

public interface IProjectService
{
    Task<IEnumerable<ProjectDto>> GetAllProjectsAsync();
    Task<IEnumerable<ProjectDto>> GetProjectsForUserAsync(Guid userId);
    Task<ProjectDto?> GetProjectByIdAsync(Guid id);
    Task<bool> HasUserAccessAsync(Guid userId, Guid projectId);
    Task<ProjectDto> CreateProjectAsync(CreateProjectDto dto, Guid createdBy);
    Task<ProjectDto?> UpdateProjectAsync(Guid id, UpdateProjectDto dto);
    Task DeleteProjectAsync(Guid id);
    Task<ProjectDto?> ArchiveProjectAsync(Guid id, Guid archivedBy);
    Task<ProjectDto?> UnarchiveProjectAsync(Guid id);
    Task<IEnumerable<ProjectDto>> GetArchivedProjectsForUserAsync(Guid userId);
    Task<bool> DeleteProjectPermanentlyAsync(Guid id, Guid deletedBy);
    Task<IEnumerable<UserDto>> GetProjectUsersAsync(Guid projectId);
}

public interface IPhaseService
{
    Task<IEnumerable<PhaseDto>> GetPhasesByProjectAsync(Guid projectId);
    Task<PhaseDto?> GetPhaseByIdAsync(Guid id);
    Task<PhaseDto?> GetPhaseByCodeAsync(Guid projectId, string phaseCode);
    Task<PhaseDto?> UpdatePhaseAsync(Guid id, UpdatePhaseDto dto);
    Task<PhaseDto?> StartPhaseAsync(Guid id);
    Task<PhaseDto?> CompletePhaseAsync(Guid id);
}

public interface IProjectPlanService
{
    Task<ProjectPlanDto?> GetPlanByProjectAsync(Guid projectId);
    Task<ProjectPlanDto> CreateInitialPlanAsync(Guid projectId, CreateProjectPlanDto dto);
    Task<ProjectPlanDto> CreateNewPlanVersionAsync(Guid projectId, CreateProjectPlanDto dto);
    Task<IEnumerable<ProjectPlanDto>> GetPlanHistoryAsync(Guid projectId);
}

public interface IIterationService
{
    Task<IEnumerable<IterationDto>> GetIterationsByProjectAsync(Guid projectId);
    Task<IterationDto> CreateIterationAsync(Guid projectId, CreateIterationDto dto);
    Task<IterationDto?> UpdateIterationStatusAsync(Guid id, string status);
    // HU-016: Capacidad y velocidad
    Task<IterationDto?> UpdateIterationCapacityAsync(Guid id, UpdateIterationCapacityDto dto);
    Task<IterationDto?> UpdateIterationVelocityAsync(Guid id, UpdateIterationVelocityDto dto);
    Task<ProjectVelocityStatsDto?> GetProjectVelocityStatsAsync(Guid projectId);
    Task<PlanningDataDto?> GetPlanningDataAsync(Guid projectId);
}

public interface IIterationTaskService
{
    Task<IEnumerable<IterationTaskDto>> GetTasksByIterationAsync(Guid iterationId);
    Task<IterationTaskDto> CreateTaskAsync(Guid iterationId, CreateIterationTaskDto dto);
    Task<IterationTaskDto?> UpdateTaskAsync(Guid id, UpdateIterationTaskDto dto);
    Task<bool> DeleteTaskAsync(Guid id);
}

public interface IIterationProgressService
{
    Task<IEnumerable<IterationProgressDto>> GetProgressByIterationAsync(Guid iterationId);
    Task<IterationProgressDto?> GetLatestProgressAsync(Guid iterationId);
    Task<IterationProgressDto> CreateProgressRecordAsync(Guid iterationId, CreateIterationProgressDto dto);
    Task<IterationProgressDto?> UpdateProgressRecordAsync(Guid id, UpdateIterationProgressDto dto);
    Task<IterationSummaryDto?> GetIterationSummaryAsync(Guid iterationId);
    Task<ProjectDashboardDto?> GetProjectDashboardAsync(Guid projectId);
    Task<BurndownDataDto?> GetBurndownDataAsync(Guid iterationId);
}

public interface IArtifactService
{
    Task<IEnumerable<ArtifactDto>> GetArtifactsByProjectAndPhaseAsync(Guid projectId, string phaseId);
    Task<ArtifactDto> CreateArtifactAsync(CreateArtifactDto dto, Stream? fileStream = null, string? fileName = null);
    Task<ArtifactDto?> UpdateArtifactAsync(Guid id, UpdateArtifactDto dto, Stream? fileStream = null, string? fileName = null);
    Task<Stream?> GetArtifactFileAsync(Guid artifactId);
    Task<List<AllowedFileFormatsDto>> GetAllowedFileFormatsAsync();
    Task<ArtifactDto?> LinkRepositoryAsync(Guid artifactId, LinkRepositoryRequest request);
    Task<ArtifactDto?> AddTestCaseAsync(Guid artifactId, AddTestCaseRequest request);
    Task<ArtifactDto?> AddTestResultAsync(Guid artifactId, AddTestResultRequest request);
    Task<ArtifactDto?> AddIterationActivityAsync(Guid artifactId, AddIterationActivityRequest request);
    Task<PhaseValidationDto> ValidatePhaseCompletionAsync(Guid projectId, string phaseId);
    
    // HU-020: Reasignación de artefactos
    Task<ReassignmentResultDto> ReassignToPhaseAsync(Guid artifactId, string userId, ReassignArtifactDto dto);
    Task<ReassignmentResultDto> ReassignWorkflowAsync(Guid artifactId, string userId, ReassignWorkflowDto dto);
    Task<ReassignmentResultDto> ValidateReassignmentAsync(Guid artifactId, ValidateReassignmentDto dto);
    Task<IEnumerable<ArtifactMovementHistoryDto>> GetMovementHistoryAsync(Guid artifactId);
}

public interface IArtifactTypeService
{
    Task<IEnumerable<ArtifactTypeDto>> GetAllArtifactTypesAsync();
    Task<IEnumerable<ArtifactTypeDto>> GetArtifactTypesByPhaseAsync(string phase);
    Task SeedDefaultInceptionTypesAsync();
}

public interface IArtifactVersionService
{
    Task<ArtifactVersionDto> CreateVersionAsync(Guid artifactId, CreateArtifactVersionDto dto, Stream? fileStream = null, string? fileName = null);
    Task<VersionHistoryDto> GetVersionHistoryAsync(Guid artifactId);
    Task<ArtifactVersionDto?> GetVersionByIdAsync(Guid versionId);
    Task<VersionComparisonDto?> CompareVersionsAsync(Guid versionId1, Guid versionId2);
    Task<Stream?> GetVersionFileAsync(Guid versionId);
}

public interface ITestExecutionService
{
    Task<TestExecutionDto> CreateExecutionAsync(Guid userId, CreateTestExecutionDto dto);
    Task<IEnumerable<TestExecutionDto>> GetExecutionsByArtifactAsync(Guid artifactId);
    Task<IEnumerable<TestExecutionDto>> GetExecutionsByTestCaseAsync(Guid artifactId, string testCaseId);
    Task<TestExecutionDto?> GetExecutionByIdAsync(Guid executionId);
    Task<TestExecutionDto?> UpdateExecutionAsync(Guid executionId, UpdateTestExecutionDto dto);
    Task<bool> DeleteExecutionAsync(Guid executionId);
    Task<TestExecutionSummaryDto> GetExecutionSummaryAsync(Guid artifactId);
}

public interface IDefectService
{
    Task<DefectDto> CreateDefectAsync(Guid userId, CreateDefectDto dto);
    Task<IEnumerable<DefectDto>> GetDefectsByProjectAsync(Guid projectId);
    Task<IEnumerable<DefectDto>> GetDefectsByArtifactAsync(Guid artifactId);
    Task<IEnumerable<DefectDto>> GetDefectsByTestExecutionAsync(Guid testExecutionId);
    Task<IEnumerable<DefectDto>> GetDefectsByStatusAsync(Guid projectId, string status);
    Task<IEnumerable<DefectDto>> GetDefectsByAssigneeAsync(Guid assigneeId);
    Task<DefectDto?> GetDefectByIdAsync(Guid defectId);
    Task<DefectDto?> GetDefectByNumberAsync(Guid projectId, string defectNumber);
    Task<DefectDto?> UpdateDefectAsync(Guid defectId, UpdateDefectDto dto);
    Task<DefectDto?> AssignDefectAsync(Guid defectId, Guid assignedTo);
    Task<DefectDto?> UpdateStatusAsync(Guid defectId, string newStatus);
    Task<DefectDto?> ResolveDefectAsync(Guid defectId, Guid resolvedBy, string resolution);
    Task<DefectDto?> CloseDefectAsync(Guid defectId);
    Task<DefectDto?> ReopenDefectAsync(Guid defectId);
    Task<bool> DeleteDefectAsync(Guid defectId);
    Task<DefectSummaryDto> GetDefectSummaryAsync(Guid projectId);
}

public interface IFileStorageService
{
    Task<(string FilePath, string FileName, long FileSize)> SaveFileAsync(Guid projectId, Guid artifactId, Stream fileStream, string fileName, string category);
    Task<Stream?> GetFileStreamAsync(string filePath);
    Task<bool> DeleteFileAsync(string filePath);
    Task<string> GetFilePhysicalPathAsync(string filePath);
    bool IsValidFileFormat(string fileName, string category);
    long GetMaxFileSizeForCategory(string category);
}

public interface IAuditLogService
{
    Task LogActionAsync(Guid userId, string action, string entityType, Guid? entityId, string? details = null);
    Task<IEnumerable<AuditLogDto>> GetAuditLogsAsync(Guid? userId = null, string? entityType = null, Guid? entityId = null);
}

public interface IMicroincrementService
{
    Task<IEnumerable<MicroincrementDto>> GetAllAsync();
    Task<IEnumerable<MicroincrementDto>> GetByIterationIdAsync(Guid iterationId);
    Task<IEnumerable<MicroincrementDto>> GetByArtifactIdAsync(Guid artifactId);
    Task<IEnumerable<MicroincrementDto>> GetByAuthorAsync(string author);
    Task<IEnumerable<MicroincrementDto>> GetByTypeAsync(string type);
    Task<IEnumerable<MicroincrementDto>> GetFilteredAsync(Guid? iterationId, Guid? artifactId, string? author, string? type);
    Task<MicroincrementDto?> GetByIdAsync(Guid id);
    Task<MicroincrementDto> CreateAsync(CreateMicroincrementDto dto);
    Task<MicroincrementDto?> UpdateAsync(Guid id, UpdateMicroincrementDto dto);
    Task<bool> DeleteAsync(Guid id);
}

public interface IProjectClosureService
{
    Task<IEnumerable<ProjectClosureDto>> GetAllClosuresAsync();
    Task<ProjectClosureDto?> GetClosureByIdAsync(Guid id);
    Task<ProjectClosureDto?> GetClosureByProjectIdAsync(Guid projectId);
    Task<ProjectClosureDto> CreateClosureAsync(CreateProjectClosureDto dto, string closedBy);
    Task<ProjectClosureDto?> UpdateClosureAsync(Guid id, UpdateProjectClosureDto dto);
    Task<ProjectClosureDto?> ApproveClosureAsync(Guid id, string approvedBy, ApproveClosureDto dto);
    Task<ClosureValidationDto> ValidateClosureAsync(Guid projectId);
    Task<bool> DeleteClosureAsync(Guid id);
}

public interface IFinalBuildService
{
    Task<IEnumerable<FinalBuildDto>> GetAllBuildsAsync();
    Task<IEnumerable<FinalBuildDto>> GetBuildsByProjectIdAsync(Guid projectId);
    Task<FinalBuildDto?> GetBuildByIdAsync(Guid id);
    Task<FinalBuildDto?> GetBuildByNumberAsync(Guid projectId, string buildNumber);
    Task<FinalBuildDto> CreateBuildAsync(CreateFinalBuildDto dto, string builtBy);
    Task<FinalBuildDto?> UpdateBuildAsync(Guid id, UpdateFinalBuildDto dto);
    Task<bool> DeleteBuildAsync(Guid id);
}

public interface IWorkflowService
{
    Task<IEnumerable<WorkflowDto>> GetAllWorkflowsAsync();
    Task<IEnumerable<WorkflowDto>> GetWorkflowsByProjectIdAsync(Guid projectId);
    Task<WorkflowWithStatesDto?> GetWorkflowByIdAsync(Guid id);
    Task<WorkflowDto> CreateWorkflowAsync(CreateWorkflowDto dto, Guid createdBy);
    Task<WorkflowDto?> UpdateWorkflowAsync(Guid id, UpdateWorkflowDto dto);
    Task<bool> DeleteWorkflowAsync(Guid id);
}

public interface IWorkflowStateService
{
    Task<IEnumerable<WorkflowStateDto>> GetStatesByWorkflowIdAsync(Guid workflowId);
    Task<WorkflowStateDto?> GetStateByIdAsync(Guid id);
    Task<WorkflowStateDto> CreateStateAsync(CreateWorkflowStateDto dto);
    Task<WorkflowStateDto?> UpdateStateAsync(Guid id, UpdateWorkflowStateDto dto);
    Task<bool> DeleteStateAsync(Guid id);
    Task<WorkflowStateResponsibleDto> AddResponsibleAsync(CreateWorkflowStateResponsibleDto dto);
    Task<bool> RemoveResponsibleAsync(Guid responsibleId);
}

public interface IArtifactStateService
{
    Task<IEnumerable<ArtifactStateHistoryDto>> GetArtifactHistoryAsync(Guid artifactId);
    Task<ArtifactWithWorkflowDto?> GetArtifactWithWorkflowAsync(Guid artifactId);
    Task<ArtifactStateHistoryDto> ChangeArtifactStateAsync(ChangeArtifactStateDto dto, Guid changedBy);
    Task<ArtifactDto?> AssignWorkflowToArtifactAsync(Guid artifactId, Guid workflowId);
}

public interface IWorkflowPermissionService
{
    Task<IEnumerable<WorkflowPermissionDto>> GetPermissionsByWorkflowIdAsync(Guid workflowId);
    Task<WorkflowPermissionMatrixDto> GetPermissionMatrixAsync(Guid workflowId);
    Task<WorkflowPermissionDto> CreatePermissionAsync(CreateWorkflowPermissionDto dto);
    Task<WorkflowPermissionDto?> UpdatePermissionAsync(Guid id, UpdateWorkflowPermissionDto dto);
    Task<bool> DeletePermissionAsync(Guid id);
    Task<bool> CheckPermissionAsync(Guid workflowId, string role, string action);
}

// ========== HU-018: Global Configuration Services ==========

public interface IGlobalConfigurationService
{
    // Configuration CRUD
    Task<IEnumerable<GlobalConfigurationDto>> GetAllConfigurationsAsync();
    Task<GlobalConfigurationDto?> GetConfigurationByIdAsync(Guid id);
    Task<GlobalConfigurationDetailDto?> GetConfigurationDetailsAsync(Guid id);
    Task<GlobalConfigurationDetailDto?> GetDefaultConfigurationAsync();
    Task<GlobalConfigurationDto> CreateConfigurationAsync(CreateGlobalConfigurationDto dto, string createdBy);
    Task<GlobalConfigurationDto?> UpdateConfigurationAsync(Guid id, UpdateGlobalConfigurationDto dto);
    Task<bool> DeleteConfigurationAsync(Guid id);
    
    // Version management
    Task<GlobalConfigurationDto?> IncrementVersionAsync(Guid id, string changedBy, string changeDescription);
    Task<GlobalConfigurationDetailDto?> RollbackToVersionAsync(Guid id, int targetVersion, string changedBy, string? reason);
    
    // Change history
    Task<IEnumerable<ConfigurationChangeHistoryDto>> GetChangeHistoryAsync(Guid configurationId);
    Task<IEnumerable<ConfigurationChangeHistoryDto>> GetChangesByVersionAsync(Guid configurationId, int version);
}

public interface IRoleTemplateService
{
    Task<IEnumerable<RoleTemplateDto>> GetRolesByConfigurationAsync(Guid configurationId);
    Task<RoleTemplateDto?> GetRoleByIdAsync(Guid id);
    Task<RoleTemplateDto> CreateRoleAsync(CreateRoleTemplateDto dto, string changedBy);
    Task<RoleTemplateDto?> UpdateRoleAsync(Guid id, UpdateRoleTemplateDto dto, string changedBy);
    Task<bool> DeleteRoleAsync(Guid id, string changedBy);
}

public interface IPhaseTemplateService
{
    Task<IEnumerable<PhaseTemplateDto>> GetPhasesByConfigurationAsync(Guid configurationId);
    Task<PhaseTemplateDto?> GetPhaseByIdAsync(Guid id);
    Task<PhaseTemplateDto> CreatePhaseAsync(CreatePhaseTemplateDto dto, string changedBy);
    Task<PhaseTemplateDto?> UpdatePhaseAsync(Guid id, UpdatePhaseTemplateDto dto, string changedBy);
    Task<bool> DeletePhaseAsync(Guid id, string changedBy);
}

public interface IArtifactTypeTemplateService
{
    Task<IEnumerable<ArtifactTypeTemplateDto>> GetArtifactTypesByConfigurationAsync(Guid configurationId);
    Task<IEnumerable<ArtifactTypeTemplateDto>> GetArtifactTypesByPhaseAsync(Guid configurationId, string phaseCode);
    Task<ArtifactTypeTemplateDto?> GetArtifactTypeByIdAsync(Guid id);
    Task<ArtifactTypeTemplateDto> CreateArtifactTypeAsync(CreateArtifactTypeTemplateDto dto, string changedBy);
    Task<ArtifactTypeTemplateDto?> UpdateArtifactTypeAsync(Guid id, UpdateArtifactTypeTemplateDto dto, string changedBy);
    Task<bool> DeleteArtifactTypeAsync(Guid id, string changedBy);
}

public interface IWorkflowTemplateService
{
    Task<IEnumerable<WorkflowTemplateDto>> GetWorkflowsByConfigurationAsync(Guid configurationId);
    Task<WorkflowTemplateDto?> GetWorkflowByIdAsync(Guid id);
    Task<WorkflowTemplateDto> CreateWorkflowAsync(CreateWorkflowTemplateDto dto, string changedBy);
    Task<WorkflowTemplateDto?> UpdateWorkflowAsync(Guid id, UpdateWorkflowTemplateDto dto, string changedBy);
    Task<bool> DeleteWorkflowAsync(Guid id, string changedBy);
}

public interface IWorkflowStateTemplateService
{
    Task<IEnumerable<WorkflowStateTemplateDto>> GetStatesByWorkflowAsync(Guid workflowTemplateId);
    Task<WorkflowStateTemplateDto?> GetStateByIdAsync(Guid id);
    Task<WorkflowStateTemplateDto> CreateStateAsync(CreateWorkflowStateTemplateDto dto, string changedBy);
    Task<WorkflowStateTemplateDto?> UpdateStateAsync(Guid id, UpdateWorkflowStateTemplateDto dto, string changedBy);
    Task<bool> DeleteStateAsync(Guid id, string changedBy);
}

public interface ICustomFieldDefinitionService
{
    Task<IEnumerable<CustomFieldDefinitionDto>> GetFieldsByArtifactTypeAsync(Guid artifactTypeTemplateId);
    Task<IEnumerable<CustomFieldDefinitionDto>> GetFieldsByConfigurationAsync(Guid configurationId);
    Task<CustomFieldDefinitionDto?> GetFieldByIdAsync(Guid id);
    Task<CustomFieldDefinitionDto> CreateFieldAsync(CreateCustomFieldDefinitionDto dto, string changedBy);
    Task<CustomFieldDefinitionDto?> UpdateFieldAsync(Guid id, UpdateCustomFieldDefinitionDto dto, string changedBy);
    Task<bool> DeleteFieldAsync(Guid id, string changedBy);
}

public interface IProjectConfigurationService
{
    Task<IEnumerable<ProjectConfigurationDto>> GetAllProjectConfigurationsAsync();
    Task<IEnumerable<ProjectConfigurationDto>> GetProjectsByConfigurationAsync(Guid configurationId);
    Task<ProjectConfigurationDto?> GetProjectConfigurationAsync(Guid projectId);
    Task<ProjectConfigurationDto> ApplyConfigurationToProjectAsync(ApplyConfigurationToProjectDto dto, string appliedBy);
    Task<ProjectConfigurationDto?> UpdateProjectConfigurationSettingsAsync(Guid projectId, UpdateProjectConfigurationSettingsDto dto);
    Task<BulkUpdateResultDto> ApplyConfigurationUpdatesToProjectsAsync(Guid configurationId, ApplyConfigurationUpdatesDto dto, string appliedBy);
}

public interface IArtifactCustomFieldValueService
{
    Task<IEnumerable<ArtifactCustomFieldValueDto>> GetFieldValuesByArtifactAsync(Guid artifactId);
    Task<ArtifactCustomFieldValueDto?> GetFieldValueByIdAsync(Guid id);
    Task<ArtifactCustomFieldValueDto> SetFieldValueAsync(SetArtifactCustomFieldValueDto dto);
    Task<IEnumerable<ArtifactCustomFieldValueDto>> BulkSetFieldValuesAsync(BulkSetCustomFieldValuesDto dto);
    Task<bool> DeleteFieldValueAsync(Guid id);
}
