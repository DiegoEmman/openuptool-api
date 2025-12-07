using OpenUpTool.Core.DTOs;
using OpenUpTool.Core.Entities;
using OpenUpTool.Core.Interfaces;
using System.Text.Json;

namespace OpenUpTool.Core.Services;

public class FinalBuildService : IFinalBuildService
{
    private readonly IFinalBuildRepository _buildRepository;
    private readonly IProjectRepository _projectRepository;

    public FinalBuildService(
        IFinalBuildRepository buildRepository,
        IProjectRepository projectRepository)
    {
        _buildRepository = buildRepository;
        _projectRepository = projectRepository;
    }

    public async Task<IEnumerable<FinalBuildDto>> GetAllBuildsAsync()
    {
        var builds = await _buildRepository.GetAllAsync();
        return builds.Select(MapToDto);
    }

    public async Task<IEnumerable<FinalBuildDto>> GetBuildsByProjectIdAsync(Guid projectId)
    {
        var builds = await _buildRepository.GetByProjectIdAsync(projectId);
        return builds.Select(MapToDto);
    }

    public async Task<FinalBuildDto?> GetBuildByIdAsync(Guid id)
    {
        var build = await _buildRepository.GetByIdAsync(id);
        return build == null ? null : MapToDto(build);
    }

    public async Task<FinalBuildDto?> GetBuildByNumberAsync(Guid projectId, string buildNumber)
    {
        var build = await _buildRepository.GetByBuildNumberAsync(projectId, buildNumber);
        return build == null ? null : MapToDto(build);
    }

    public async Task<FinalBuildDto> CreateBuildAsync(CreateFinalBuildDto dto, string builtBy)
    {
        // Validar que el proyecto existe
        var project = await _projectRepository.GetByIdAsync(dto.ProjectId);
        if (project == null)
            throw new ArgumentException($"Proyecto con ID {dto.ProjectId} no encontrado");

        // Validar que no exista un build con el mismo número
        var existingBuild = await _buildRepository.GetByBuildNumberAsync(dto.ProjectId, dto.BuildNumber);
        if (existingBuild != null)
            throw new InvalidOperationException($"Ya existe un build con el número {dto.BuildNumber} para este proyecto");

        var build = new FinalBuild
        {
            Id = Guid.NewGuid(),
            ProjectId = dto.ProjectId,
            BuildNumber = dto.BuildNumber,
            Version = dto.Version,
            BuildTag = dto.BuildTag,
            CommitHash = dto.CommitHash,
            BuildDate = DateTime.UtcNow,
            BuiltBy = builtBy,
            BuildEnvironment = dto.BuildEnvironment,
            BuildConfiguration = dto.BuildConfiguration,
            BinaryArtifacts = JsonSerializer.Serialize(dto.BinaryArtifacts),
            MainDownloadUrl = dto.MainDownloadUrl,
            DocumentationUrl = dto.DocumentationUrl,
            ReleaseNotesUrl = dto.ReleaseNotesUrl,
            TargetPlatform = dto.TargetPlatform,
            Dependencies = dto.Dependencies,
            SystemRequirements = dto.SystemRequirements,
            IsStable = dto.IsStable,
            TestsPassed = dto.TestsPassed,
            TestsTotal = dto.TestsTotal,
            CodeCoverage = dto.CodeCoverage,
            QualityGateStatus = dto.QualityGateStatus,
            ClosureId = dto.ClosureId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _buildRepository.CreateAsync(build);
        created.Project = project;
        return MapToDto(created);
    }

    public async Task<FinalBuildDto?> UpdateBuildAsync(Guid id, UpdateFinalBuildDto dto)
    {
        var build = await _buildRepository.GetByIdAsync(id);
        if (build == null) return null;

        if (dto.BuildNumber != null) build.BuildNumber = dto.BuildNumber;
        if (dto.Version != null) build.Version = dto.Version;
        if (dto.BuildTag != null) build.BuildTag = dto.BuildTag;
        if (dto.CommitHash != null) build.CommitHash = dto.CommitHash;
        if (dto.BuildEnvironment != null) build.BuildEnvironment = dto.BuildEnvironment;
        if (dto.BuildConfiguration != null) build.BuildConfiguration = dto.BuildConfiguration;
        if (dto.BinaryArtifacts != null) build.BinaryArtifacts = JsonSerializer.Serialize(dto.BinaryArtifacts);
        if (dto.MainDownloadUrl != null) build.MainDownloadUrl = dto.MainDownloadUrl;
        if (dto.DocumentationUrl != null) build.DocumentationUrl = dto.DocumentationUrl;
        if (dto.ReleaseNotesUrl != null) build.ReleaseNotesUrl = dto.ReleaseNotesUrl;
        if (dto.TargetPlatform != null) build.TargetPlatform = dto.TargetPlatform;
        if (dto.Dependencies != null) build.Dependencies = dto.Dependencies;
        if (dto.SystemRequirements != null) build.SystemRequirements = dto.SystemRequirements;
        if (dto.IsStable.HasValue) build.IsStable = dto.IsStable.Value;
        if (dto.TestsPassed.HasValue) build.TestsPassed = dto.TestsPassed;
        if (dto.TestsTotal.HasValue) build.TestsTotal = dto.TestsTotal;
        if (dto.CodeCoverage.HasValue) build.CodeCoverage = dto.CodeCoverage;
        if (dto.QualityGateStatus != null) build.QualityGateStatus = dto.QualityGateStatus;
        if (dto.ClosureId.HasValue) build.ClosureId = dto.ClosureId;

        build.UpdatedAt = DateTime.UtcNow;
        var updated = await _buildRepository.UpdateAsync(build);
        return MapToDto(updated);
    }

    public async Task<bool> DeleteBuildAsync(Guid id)
    {
        var build = await _buildRepository.GetByIdAsync(id);
        if (build == null) return false;

        await _buildRepository.DeleteAsync(id);
        return true;
    }

    private static FinalBuildDto MapToDto(FinalBuild build)
    {
        var binaryArtifacts = new List<BinaryArtifactDto>();
        try
        {
            binaryArtifacts = JsonSerializer.Deserialize<List<BinaryArtifactDto>>(build.BinaryArtifacts) ?? new();
        }
        catch { }

        return new FinalBuildDto
        {
            Id = build.Id,
            ProjectId = build.ProjectId,
            ProjectName = build.Project?.Name ?? string.Empty,
            BuildNumber = build.BuildNumber,
            Version = build.Version,
            BuildTag = build.BuildTag,
            CommitHash = build.CommitHash,
            BuildDate = build.BuildDate,
            BuiltBy = build.BuiltBy,
            BuildEnvironment = build.BuildEnvironment,
            BuildConfiguration = build.BuildConfiguration,
            BinaryArtifacts = binaryArtifacts,
            MainDownloadUrl = build.MainDownloadUrl,
            DocumentationUrl = build.DocumentationUrl,
            ReleaseNotesUrl = build.ReleaseNotesUrl,
            TargetPlatform = build.TargetPlatform,
            Dependencies = build.Dependencies,
            SystemRequirements = build.SystemRequirements,
            IsStable = build.IsStable,
            TestsPassed = build.TestsPassed,
            TestsTotal = build.TestsTotal,
            CodeCoverage = build.CodeCoverage,
            QualityGateStatus = build.QualityGateStatus,
            ClosureId = build.ClosureId,
            CreatedAt = build.CreatedAt,
            UpdatedAt = build.UpdatedAt
        };
    }
}
