using OpenUpTool.Core.DTOs;
using OpenUpTool.Core.Entities;
using OpenUpTool.Core.Interfaces;
using System.Text.Json;

namespace OpenUpTool.Core.Services;

public class DefectService : IDefectService
{
    private readonly IDefectRepository _defectRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IArtifactRepository _artifactRepository;
    private readonly IArtifactVersionRepository _artifactVersionRepository;
    private readonly ITestExecutionRepository _testExecutionRepository;

    public DefectService(
        IDefectRepository defectRepository,
        IProjectRepository projectRepository,
        IArtifactRepository artifactRepository,
        IArtifactVersionRepository artifactVersionRepository,
        ITestExecutionRepository testExecutionRepository)
    {
        _defectRepository = defectRepository;
        _projectRepository = projectRepository;
        _artifactRepository = artifactRepository;
        _artifactVersionRepository = artifactVersionRepository;
        _testExecutionRepository = testExecutionRepository;
    }

    public async Task<DefectDto> CreateDefectAsync(Guid userId, CreateDefectDto dto)
    {
        // Verify project exists
        var project = await _projectRepository.GetByIdAsync(dto.ProjectId);
        if (project == null)
            throw new KeyNotFoundException($"Project with ID {dto.ProjectId} not found");

        // Verify artifact if provided
        if (dto.ArtifactId.HasValue)
        {
            var artifact = await _artifactRepository.GetByIdAsync(dto.ArtifactId.Value);
            if (artifact == null)
                throw new KeyNotFoundException($"Artifact with ID {dto.ArtifactId} not found");
        }

        // Verify artifact version if provided
        if (dto.ArtifactVersionId.HasValue)
        {
            var version = await _artifactVersionRepository.GetByIdAsync(dto.ArtifactVersionId.Value);
            if (version == null)
                throw new KeyNotFoundException($"Artifact version with ID {dto.ArtifactVersionId} not found");
        }

        // Verify test execution if provided
        if (dto.TestExecutionId.HasValue)
        {
            var execution = await _testExecutionRepository.GetByIdAsync(dto.TestExecutionId.Value);
            if (execution == null)
                throw new KeyNotFoundException($"Test execution with ID {dto.TestExecutionId} not found");
        }

        // Generate defect number
        var defectNumber = await _defectRepository.GenerateNextDefectNumberAsync(dto.ProjectId);

        var defect = new Defect
        {
            Id = Guid.NewGuid(),
            DefectNumber = defectNumber,
            Title = dto.Title,
            Description = dto.Description,
            Severity = dto.Severity,
            Status = "Open",
            Priority = dto.Priority,
            Type = dto.Type,
            ProjectId = dto.ProjectId,
            ArtifactId = dto.ArtifactId,
            ArtifactVersionId = dto.ArtifactVersionId,
            TestExecutionId = dto.TestExecutionId,
            ReportedBy = userId,
            ReportedAt = DateTime.UtcNow,
            StepsToReproduce = dto.StepsToReproduce,
            ExpectedResult = dto.ExpectedResult,
            ActualResult = dto.ActualResult,
            Environment = dto.Environment,
            Tags = dto.Tags != null ? JsonSerializer.Serialize(dto.Tags) : null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _defectRepository.CreateAsync(defect);
        var result = await _defectRepository.GetByIdAsync(created.Id);
        return MapToDto(result!);
    }

    public async Task<IEnumerable<DefectDto>> GetDefectsByProjectAsync(Guid projectId)
    {
        var defects = await _defectRepository.GetByProjectIdAsync(projectId);
        return defects.Select(MapToDto);
    }

    public async Task<IEnumerable<DefectDto>> GetDefectsByArtifactAsync(Guid artifactId)
    {
        var defects = await _defectRepository.GetByArtifactIdAsync(artifactId);
        return defects.Select(MapToDto);
    }

    public async Task<IEnumerable<DefectDto>> GetDefectsByTestExecutionAsync(Guid testExecutionId)
    {
        var defects = await _defectRepository.GetByTestExecutionIdAsync(testExecutionId);
        return defects.Select(MapToDto);
    }

    public async Task<IEnumerable<DefectDto>> GetDefectsByStatusAsync(Guid projectId, string status)
    {
        var defects = await _defectRepository.GetByStatusAsync(projectId, status);
        return defects.Select(MapToDto);
    }

    public async Task<IEnumerable<DefectDto>> GetDefectsByAssigneeAsync(Guid assigneeId)
    {
        var defects = await _defectRepository.GetByAssigneeAsync(assigneeId);
        return defects.Select(MapToDto);
    }

    public async Task<DefectDto?> GetDefectByIdAsync(Guid defectId)
    {
        var defect = await _defectRepository.GetByIdAsync(defectId);
        return defect != null ? MapToDto(defect) : null;
    }

    public async Task<DefectDto?> GetDefectByNumberAsync(Guid projectId, string defectNumber)
    {
        var defect = await _defectRepository.GetByDefectNumberAsync(projectId, defectNumber);
        return defect != null ? MapToDto(defect) : null;
    }

    public async Task<DefectDto?> UpdateDefectAsync(Guid defectId, UpdateDefectDto dto)
    {
        var defect = await _defectRepository.GetByIdAsync(defectId);
        if (defect == null)
            return null;

        if (dto.Title != null)
            defect.Title = dto.Title;

        if (dto.Description != null)
            defect.Description = dto.Description;

        if (dto.Severity != null)
            defect.Severity = dto.Severity;

        if (dto.Status != null)
            defect.Status = dto.Status;

        if (dto.Priority != null)
            defect.Priority = dto.Priority;

        if (dto.Type != null)
            defect.Type = dto.Type;

        if (dto.StepsToReproduce != null)
            defect.StepsToReproduce = dto.StepsToReproduce;

        if (dto.ExpectedResult != null)
            defect.ExpectedResult = dto.ExpectedResult;

        if (dto.ActualResult != null)
            defect.ActualResult = dto.ActualResult;

        if (dto.Environment != null)
            defect.Environment = dto.Environment;

        if (dto.Tags != null)
            defect.Tags = JsonSerializer.Serialize(dto.Tags);

        if (dto.Resolution != null)
            defect.Resolution = dto.Resolution;

        defect.UpdatedAt = DateTime.UtcNow;

        var updated = await _defectRepository.UpdateAsync(defect);
        var result = await _defectRepository.GetByIdAsync(updated.Id);
        return MapToDto(result!);
    }

    public async Task<DefectDto?> AssignDefectAsync(Guid defectId, Guid assignedTo)
    {
        var defect = await _defectRepository.GetByIdAsync(defectId);
        if (defect == null)
            return null;

        defect.AssignedTo = assignedTo;
        defect.AssignedAt = DateTime.UtcNow;
        
        // Auto-change status to InProgress if it was Open
        if (defect.Status == "Open")
            defect.Status = "InProgress";

        defect.UpdatedAt = DateTime.UtcNow;

        var updated = await _defectRepository.UpdateAsync(defect);
        var result = await _defectRepository.GetByIdAsync(updated.Id);
        return MapToDto(result!);
    }

    public async Task<DefectDto?> UpdateStatusAsync(Guid defectId, string newStatus)
    {
        var defect = await _defectRepository.GetByIdAsync(defectId);
        if (defect == null)
            return null;

        defect.Status = newStatus;
        defect.UpdatedAt = DateTime.UtcNow;

        var updated = await _defectRepository.UpdateAsync(defect);
        var result = await _defectRepository.GetByIdAsync(updated.Id);
        return MapToDto(result!);
    }

    public async Task<DefectDto?> ResolveDefectAsync(Guid defectId, Guid resolvedBy, string resolution)
    {
        var defect = await _defectRepository.GetByIdAsync(defectId);
        if (defect == null)
            return null;

        defect.Status = "Resolved";
        defect.ResolvedBy = resolvedBy;
        defect.ResolvedAt = DateTime.UtcNow;
        defect.Resolution = resolution;
        defect.UpdatedAt = DateTime.UtcNow;

        var updated = await _defectRepository.UpdateAsync(defect);
        var result = await _defectRepository.GetByIdAsync(updated.Id);
        return MapToDto(result!);
    }

    public async Task<DefectDto?> CloseDefectAsync(Guid defectId)
    {
        var defect = await _defectRepository.GetByIdAsync(defectId);
        if (defect == null)
            return null;

        // Can only close if already resolved
        if (defect.Status != "Resolved")
            throw new InvalidOperationException("Defect must be resolved before it can be closed");

        defect.Status = "Closed";
        defect.UpdatedAt = DateTime.UtcNow;

        var updated = await _defectRepository.UpdateAsync(defect);
        var result = await _defectRepository.GetByIdAsync(updated.Id);
        return MapToDto(result!);
    }

    public async Task<DefectDto?> ReopenDefectAsync(Guid defectId)
    {
        var defect = await _defectRepository.GetByIdAsync(defectId);
        if (defect == null)
            return null;

        defect.Status = "Reopened";
        defect.UpdatedAt = DateTime.UtcNow;

        var updated = await _defectRepository.UpdateAsync(defect);
        var result = await _defectRepository.GetByIdAsync(updated.Id);
        return MapToDto(result!);
    }

    public async Task<bool> DeleteDefectAsync(Guid defectId)
    {
        var defect = await _defectRepository.GetByIdAsync(defectId);
        if (defect == null)
            return false;

        await _defectRepository.DeleteAsync(defectId);
        return true;
    }

    public async Task<DefectSummaryDto> GetDefectSummaryAsync(Guid projectId)
    {
        var defects = await _defectRepository.GetByProjectIdAsync(projectId);
        var defectsList = defects.ToList();

        var total = defectsList.Count;
        var open = defectsList.Count(d => d.Status == "Open");
        var inProgress = defectsList.Count(d => d.Status == "InProgress");
        var resolved = defectsList.Count(d => d.Status == "Resolved");
        var closed = defectsList.Count(d => d.Status == "Closed");
        var reopened = defectsList.Count(d => d.Status == "Reopened");

        var critical = defectsList.Count(d => d.Severity == "Critical");
        var high = defectsList.Count(d => d.Severity == "High");
        var medium = defectsList.Count(d => d.Severity == "Medium");
        var low = defectsList.Count(d => d.Severity == "Low");

        var resolutionRate = total > 0 ? (double)(resolved + closed) / total * 100 : 0;

        return new DefectSummaryDto(
            ProjectId: projectId,
            TotalDefects: total,
            Open: open,
            InProgress: inProgress,
            Resolved: resolved,
            Closed: closed,
            Reopened: reopened,
            Critical: critical,
            High: high,
            Medium: medium,
            Low: low,
            ResolutionRate: Math.Round(resolutionRate, 2)
        );
    }

    private static DefectDto MapToDto(Defect defect)
    {
        List<string>? tagsList = null;
        if (!string.IsNullOrEmpty(defect.Tags))
        {
            try
            {
                tagsList = JsonSerializer.Deserialize<List<string>>(defect.Tags);
            }
            catch
            {
                tagsList = null;
            }
        }

        return new DefectDto(
            Id: defect.Id,
            DefectNumber: defect.DefectNumber,
            Title: defect.Title,
            Description: defect.Description,
            Severity: defect.Severity,
            Status: defect.Status,
            Priority: defect.Priority,
            Type: defect.Type,
            ProjectId: defect.ProjectId,
            ArtifactId: defect.ArtifactId,
            ArtifactVersionId: defect.ArtifactVersionId,
            TestExecutionId: defect.TestExecutionId,
            ReportedBy: defect.ReportedBy,
            ReportedAt: defect.ReportedAt,
            AssignedTo: defect.AssignedTo,
            AssignedAt: defect.AssignedAt,
            ResolvedAt: defect.ResolvedAt,
            ResolvedBy: defect.ResolvedBy,
            Resolution: defect.Resolution,
            StepsToReproduce: defect.StepsToReproduce,
            ExpectedResult: defect.ExpectedResult,
            ActualResult: defect.ActualResult,
            Environment: defect.Environment,
            Tags: tagsList,
            CreatedAt: defect.CreatedAt,
            UpdatedAt: defect.UpdatedAt,
            ArtifactTitle: defect.Artifact?.Title,
            TestCaseName: defect.TestExecution?.TestCaseName
        );
    }
}
