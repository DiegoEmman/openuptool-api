using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenUpTool.Core.DTOs;
using OpenUpTool.Core.Interfaces;
using System.Security.Claims;

namespace OpenUpTool.Api.Controllers;

[ApiController]
[Route("api/test-executions")]
[Authorize]
public class TestExecutionsController : ControllerBase
{
    private readonly ITestExecutionService _testExecutionService;

    public TestExecutionsController(ITestExecutionService testExecutionService)
    {
        _testExecutionService = testExecutionService;
    }

    /// <summary>
    /// Creates a new test execution
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<TestExecutionDto>> CreateExecution([FromBody] CreateTestExecutionDto dto)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var execution = await _testExecutionService.CreateExecutionAsync(userId, dto);
            return CreatedAtAction(nameof(GetExecutionById), new { id = execution.Id }, execution);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Gets all test executions for an artifact
    /// </summary>
    [HttpGet("artifact/{artifactId}")]
    public async Task<ActionResult<IEnumerable<TestExecutionDto>>> GetExecutionsByArtifact(Guid artifactId)
    {
        var executions = await _testExecutionService.GetExecutionsByArtifactAsync(artifactId);
        return Ok(executions);
    }

    /// <summary>
    /// Gets test executions for a specific test case
    /// </summary>
    [HttpGet("artifact/{artifactId}/testcase/{testCaseId}")]
    public async Task<ActionResult<IEnumerable<TestExecutionDto>>> GetExecutionsByTestCase(
        Guid artifactId, 
        string testCaseId)
    {
        var executions = await _testExecutionService.GetExecutionsByTestCaseAsync(artifactId, testCaseId);
        return Ok(executions);
    }

    /// <summary>
    /// Gets a test execution by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<TestExecutionDto>> GetExecutionById(Guid id)
    {
        var execution = await _testExecutionService.GetExecutionByIdAsync(id);
        if (execution == null)
            return NotFound($"Test execution with ID {id} not found");

        return Ok(execution);
    }

    /// <summary>
    /// Updates a test execution
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<TestExecutionDto>> UpdateExecution(
        Guid id, 
        [FromBody] UpdateTestExecutionDto dto)
    {
        var execution = await _testExecutionService.UpdateExecutionAsync(id, dto);
        if (execution == null)
            return NotFound($"Test execution with ID {id} not found");

        return Ok(execution);
    }

    /// <summary>
    /// Deletes a test execution
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteExecution(Guid id)
    {
        var success = await _testExecutionService.DeleteExecutionAsync(id);
        if (!success)
            return NotFound($"Test execution with ID {id} not found");

        return NoContent();
    }

    /// <summary>
    /// Gets test execution summary for an artifact
    /// </summary>
    [HttpGet("artifact/{artifactId}/summary")]
    public async Task<ActionResult<TestExecutionSummaryDto>> GetExecutionSummary(Guid artifactId)
    {
        var summary = await _testExecutionService.GetExecutionSummaryAsync(artifactId);
        return Ok(summary);
    }
}
