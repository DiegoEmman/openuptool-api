using OpenUpTool.Core.DTOs;
using OpenUpTool.Core.Entities;
using OpenUpTool.Core.Interfaces;
using System.Text.Json;

namespace OpenUpTool.Core.Services;

public class TestExecutionService : ITestExecutionService
{
    private readonly ITestExecutionRepository _testExecutionRepository;
    private readonly IArtifactRepository _artifactRepository;
    private readonly IArtifactVersionRepository _artifactVersionRepository;

    public TestExecutionService(
        ITestExecutionRepository testExecutionRepository,
        IArtifactRepository artifactRepository,
        IArtifactVersionRepository artifactVersionRepository)
    {
        _testExecutionRepository = testExecutionRepository;
        _artifactRepository = artifactRepository;
        _artifactVersionRepository = artifactVersionRepository;
    }

    public async Task<TestExecutionDto> CreateExecutionAsync(Guid userId, CreateTestExecutionDto dto)
    {
        // Verify artifact exists
        var artifact = await _artifactRepository.GetByIdAsync(dto.ArtifactId);
        if (artifact == null)
            throw new KeyNotFoundException($"Artifact with ID {dto.ArtifactId} not found");

        // Verify artifact version if provided
        if (dto.ArtifactVersionId.HasValue)
        {
            var version = await _artifactVersionRepository.GetByIdAsync(dto.ArtifactVersionId.Value);
            if (version == null || version.ArtifactId != dto.ArtifactId)
                throw new KeyNotFoundException($"Artifact version with ID {dto.ArtifactVersionId} not found or doesn't belong to artifact");
        }

        var testExecution = new TestExecution
        {
            Id = Guid.NewGuid(),
            ArtifactId = dto.ArtifactId,
            TestCaseId = dto.TestCaseId,
            TestCaseName = dto.TestCaseName,
            Result = dto.Result,
            ExecutedBy = userId,
            ExecutedAt = DateTime.UtcNow,
            DurationSeconds = dto.DurationSeconds,
            Evidence = dto.Evidence != null ? JsonSerializer.Serialize(dto.Evidence) : null,
            Notes = dto.Notes,
            ArtifactVersionId = dto.ArtifactVersionId,
            Environment = dto.Environment,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _testExecutionRepository.CreateAsync(testExecution);
        var result = await _testExecutionRepository.GetByIdAsync(created.Id);
        return MapToDto(result!);
    }

    public async Task<IEnumerable<TestExecutionDto>> GetExecutionsByArtifactAsync(Guid artifactId)
    {
        var executions = await _testExecutionRepository.GetByArtifactIdAsync(artifactId);
        return executions.Select(MapToDto);
    }

    public async Task<IEnumerable<TestExecutionDto>> GetExecutionsByTestCaseAsync(Guid artifactId, string testCaseId)
    {
        var executions = await _testExecutionRepository.GetByTestCaseIdAsync(artifactId, testCaseId);
        return executions.Select(MapToDto);
    }

    public async Task<TestExecutionDto?> GetExecutionByIdAsync(Guid executionId)
    {
        var execution = await _testExecutionRepository.GetByIdAsync(executionId);
        return execution != null ? MapToDto(execution) : null;
    }

    public async Task<TestExecutionDto?> UpdateExecutionAsync(Guid executionId, UpdateTestExecutionDto dto)
    {
        var execution = await _testExecutionRepository.GetByIdAsync(executionId);
        if (execution == null)
            return null;

        if (dto.Result != null)
            execution.Result = dto.Result;

        if (dto.DurationSeconds.HasValue)
            execution.DurationSeconds = dto.DurationSeconds.Value;

        if (dto.Evidence != null)
            execution.Evidence = JsonSerializer.Serialize(dto.Evidence);

        if (dto.Notes != null)
            execution.Notes = dto.Notes;

        if (dto.Environment != null)
            execution.Environment = dto.Environment;

        execution.UpdatedAt = DateTime.UtcNow;

        var updated = await _testExecutionRepository.UpdateAsync(execution);
        var result = await _testExecutionRepository.GetByIdAsync(updated.Id);
        return MapToDto(result!);
    }

    public async Task<bool> DeleteExecutionAsync(Guid executionId)
    {
        var execution = await _testExecutionRepository.GetByIdAsync(executionId);
        if (execution == null)
            return false;

        await _testExecutionRepository.DeleteAsync(executionId);
        return true;
    }

    public async Task<TestExecutionSummaryDto> GetExecutionSummaryAsync(Guid artifactId)
    {
        var executions = await _testExecutionRepository.GetByArtifactIdAsync(artifactId);
        var executionsList = executions.ToList();

        var total = executionsList.Count;
        var passed = executionsList.Count(e => e.Result == "Passed");
        var failed = executionsList.Count(e => e.Result == "Failed");
        var blocked = executionsList.Count(e => e.Result == "Blocked");
        var pending = executionsList.Count(e => e.Result == "Pending");
        var skipped = executionsList.Count(e => e.Result == "Skipped");

        var passRate = total > 0 ? (double)passed / total * 100 : 0;

        // Get artifact info
        var artifact = executionsList.FirstOrDefault()?.Artifact;

        return new TestExecutionSummaryDto(
            ArtifactId: artifactId,
            ArtifactTitle: artifact?.Title ?? string.Empty,
            TotalExecutions: total,
            Passed: passed,
            Failed: failed,
            Blocked: blocked,
            Skipped: skipped,
            Pending: pending,
            PassRate: Math.Round(passRate, 2),
            LastExecutionDate: executionsList.Max(e => (DateTime?)e.ExecutedAt)
        );
    }

    private static TestExecutionDto MapToDto(TestExecution execution)
    {
        List<EvidenceItemDto>? evidenceList = null;
        if (!string.IsNullOrEmpty(execution.Evidence))
        {
            try
            {
                evidenceList = JsonSerializer.Deserialize<List<EvidenceItemDto>>(execution.Evidence);
            }
            catch
            {
                evidenceList = null;
            }
        }

        return new TestExecutionDto(
            Id: execution.Id,
            ArtifactId: execution.ArtifactId,
            TestCaseId: execution.TestCaseId,
            TestCaseName: execution.TestCaseName,
            Result: execution.Result,
            ExecutedBy: execution.ExecutedBy,
            ExecutedAt: execution.ExecutedAt,
            DurationSeconds: execution.DurationSeconds,
            Evidence: evidenceList,
            Notes: execution.Notes,
            ArtifactVersionId: execution.ArtifactVersionId,
            Environment: execution.Environment,
            CreatedAt: execution.CreatedAt
        );
    }
}

