namespace OpenUpTool.Api.Models;

/// <summary>
/// Modelo para crear un artefacto con archivo
/// </summary>
public class CreateArtifactRequest
{
    public Guid ProjectId { get; set; }
    public string PhaseId { get; set; } = string.Empty;
    public Guid ArtifactTypeId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Author { get; set; }
    public bool IsMandatory { get; set; }
    public string? ContentText { get; set; }
    public string? FileCategory { get; set; }
    public string? RepositoryUrl { get; set; }
    public string? RepositoryVersion { get; set; }
    public string? BuildNumber { get; set; }
    public Guid? WorkflowId { get; set; }
    public IFormFile? File { get; set; }
}

/// <summary>
/// Modelo para actualizar un artefacto con archivo
/// </summary>
public class UpdateArtifactRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Author { get; set; }
    public string? Status { get; set; }
    public bool? IsMandatory { get; set; }
    public string? ContentText { get; set; }
    public string? FileCategory { get; set; }
    public string? RepositoryUrl { get; set; }
    public string? RepositoryVersion { get; set; }
    public string? BuildNumber { get; set; }
    public IFormFile? File { get; set; }
}
