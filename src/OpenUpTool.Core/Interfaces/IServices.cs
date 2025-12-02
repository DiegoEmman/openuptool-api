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
