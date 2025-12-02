using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenUpTool.Core.DTOs;
using OpenUpTool.Core.Interfaces;
using System.Security.Claims;

namespace OpenUpTool.Api.Controllers;

[ApiController]
[Route("api/defects")]
[Authorize]
public class DefectsController : ControllerBase
{
    private readonly IDefectService _defectService;

    public DefectsController(IDefectService defectService)
    {
        _defectService = defectService;
    }

    /// <summary>
    /// Creates a new defect
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<DefectDto>> CreateDefect([FromBody] CreateDefectDto dto)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var defect = await _defectService.CreateDefectAsync(userId, dto);
            return CreatedAtAction(nameof(GetDefectById), new { id = defect.Id }, defect);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Gets all defects for a project
    /// </summary>
    [HttpGet("project/{projectId}")]
    public async Task<ActionResult<IEnumerable<DefectDto>>> GetDefectsByProject(Guid projectId)
    {
        var defects = await _defectService.GetDefectsByProjectAsync(projectId);
        return Ok(defects);
    }

    /// <summary>
    /// Gets defects by status for a project
    /// </summary>
    [HttpGet("project/{projectId}/status/{status}")]
    public async Task<ActionResult<IEnumerable<DefectDto>>> GetDefectsByStatus(
        Guid projectId, 
        string status)
    {
        var defects = await _defectService.GetDefectsByStatusAsync(projectId, status);
        return Ok(defects);
    }

    /// <summary>
    /// Gets defects for an artifact
    /// </summary>
    [HttpGet("artifact/{artifactId}")]
    public async Task<ActionResult<IEnumerable<DefectDto>>> GetDefectsByArtifact(Guid artifactId)
    {
        var defects = await _defectService.GetDefectsByArtifactAsync(artifactId);
        return Ok(defects);
    }

    /// <summary>
    /// Gets defects for a test execution
    /// </summary>
    [HttpGet("test-execution/{testExecutionId}")]
    public async Task<ActionResult<IEnumerable<DefectDto>>> GetDefectsByTestExecution(Guid testExecutionId)
    {
        var defects = await _defectService.GetDefectsByTestExecutionAsync(testExecutionId);
        return Ok(defects);
    }

    /// <summary>
    /// Gets defects assigned to a user
    /// </summary>
    [HttpGet("assignee/{assigneeId}")]
    public async Task<ActionResult<IEnumerable<DefectDto>>> GetDefectsByAssignee(Guid assigneeId)
    {
        var defects = await _defectService.GetDefectsByAssigneeAsync(assigneeId);
        return Ok(defects);
    }

    /// <summary>
    /// Gets a defect by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<DefectDto>> GetDefectById(Guid id)
    {
        var defect = await _defectService.GetDefectByIdAsync(id);
        if (defect == null)
            return NotFound($"Defect with ID {id} not found");

        return Ok(defect);
    }

    /// <summary>
    /// Gets a defect by defect number
    /// </summary>
    [HttpGet("project/{projectId}/number/{defectNumber}")]
    public async Task<ActionResult<DefectDto>> GetDefectByNumber(Guid projectId, string defectNumber)
    {
        var defect = await _defectService.GetDefectByNumberAsync(projectId, defectNumber);
        if (defect == null)
            return NotFound($"Defect with number {defectNumber} not found in project");

        return Ok(defect);
    }

    /// <summary>
    /// Updates a defect
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<DefectDto>> UpdateDefect(
        Guid id, 
        [FromBody] UpdateDefectDto dto)
    {
        var defect = await _defectService.UpdateDefectAsync(id, dto);
        if (defect == null)
            return NotFound($"Defect with ID {id} not found");

        return Ok(defect);
    }

    /// <summary>
    /// Assigns a defect to a user
    /// </summary>
    [HttpPut("{id}/assign/{assigneeId}")]
    public async Task<ActionResult<DefectDto>> AssignDefect(Guid id, Guid assigneeId)
    {
        var defect = await _defectService.AssignDefectAsync(id, assigneeId);
        if (defect == null)
            return NotFound($"Defect with ID {id} not found");

        return Ok(defect);
    }

    /// <summary>
    /// Updates a defect's status
    /// </summary>
    [HttpPut("{id}/status/{status}")]
    public async Task<ActionResult<DefectDto>> UpdateStatus(Guid id, string status)
    {
        var defect = await _defectService.UpdateStatusAsync(id, status);
        if (defect == null)
            return NotFound($"Defect with ID {id} not found");

        return Ok(defect);
    }

    /// <summary>
    /// Resolves a defect
    /// </summary>
    [HttpPut("{id}/resolve")]
    public async Task<ActionResult<DefectDto>> ResolveDefect(
        Guid id, 
        [FromBody] ResolveDefectRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var defect = await _defectService.ResolveDefectAsync(id, userId, request.Resolution);
            if (defect == null)
                return NotFound($"Defect with ID {id} not found");

            return Ok(defect);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Closes a defect
    /// </summary>
    [HttpPut("{id}/close")]
    public async Task<ActionResult<DefectDto>> CloseDefect(Guid id)
    {
        try
        {
            var defect = await _defectService.CloseDefectAsync(id);
            if (defect == null)
                return NotFound($"Defect with ID {id} not found");

            return Ok(defect);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Reopens a defect
    /// </summary>
    [HttpPut("{id}/reopen")]
    public async Task<ActionResult<DefectDto>> ReopenDefect(Guid id)
    {
        var defect = await _defectService.ReopenDefectAsync(id);
        if (defect == null)
            return NotFound($"Defect with ID {id} not found");

        return Ok(defect);
    }

    /// <summary>
    /// Deletes a defect
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteDefect(Guid id)
    {
        var success = await _defectService.DeleteDefectAsync(id);
        if (!success)
            return NotFound($"Defect with ID {id} not found");

        return NoContent();
    }

    /// <summary>
    /// Gets defect summary for a project
    /// </summary>
    [HttpGet("project/{projectId}/summary")]
    public async Task<ActionResult<DefectSummaryDto>> GetDefectSummary(Guid projectId)
    {
        var summary = await _defectService.GetDefectSummaryAsync(projectId);
        return Ok(summary);
    }
}

public record ResolveDefectRequest(string Resolution);
