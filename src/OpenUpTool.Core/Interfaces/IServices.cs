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

public interface IArtifactService
{
    Task<IEnumerable<ArtifactDto>> GetArtifactsByProjectAndPhaseAsync(Guid projectId, string phaseId);
    Task<ArtifactDto> CreateArtifactAsync(CreateArtifactDto dto);
    Task<ArtifactDto?> UpdateArtifactAsync(Guid id, UpdateArtifactDto dto);
}

public interface IArtifactTypeService
{
    Task<IEnumerable<ArtifactTypeDto>> GetAllArtifactTypesAsync();
    Task<IEnumerable<ArtifactTypeDto>> GetArtifactTypesByPhaseAsync(string phase);
    Task SeedDefaultInceptionTypesAsync();
}

public interface IFileStorageService
{
    Task<(string FilePath, string FileName, long FileSize)> SaveFileAsync(Guid projectId, Guid artifactId, int versionNumber, Stream fileStream, string fileName);
    Task<Stream?> GetFileStreamAsync(string filePath);
    Task<bool> DeleteFileAsync(string filePath);
    Task<string> GetFilePhysicalPathAsync(string filePath);
}
