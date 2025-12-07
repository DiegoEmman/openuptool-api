namespace OpenUpTool.Core.DTOs;

// ============= PROJECT CLOSURE DTOs =============

public class ProjectClosureDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    
    public string ClosedBy { get; set; } = string.Empty;
    public DateTime ClosureDate { get; set; }
    public string? Summary { get; set; }
    public string? LessonsLearned { get; set; }
    public string? Recommendations { get; set; }
    
    public List<ClosureCriteriaDto> Checklist { get; set; } = new();
    
    public bool AllMandatoryCriteriaMet { get; set; }
    public int TotalCriteria { get; set; }
    public int CompletedCriteria { get; set; }
    public int MandatoryCriteria { get; set; }
    public int CompletedMandatoryCriteria { get; set; }
    
    public string Status { get; set; } = "Draft";
    public string? RejectionReason { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedBy { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ClosureCriteriaDto
{
    public string CriteriaId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsMandatory { get; set; }
    public bool IsCompleted { get; set; }
    public string? Notes { get; set; }
}

public class CreateProjectClosureDto
{
    public Guid ProjectId { get; set; }
    public string? Summary { get; set; }
    public string? LessonsLearned { get; set; }
    public string? Recommendations { get; set; }
    public List<ClosureCriteriaDto> Checklist { get; set; } = new();
}

public class UpdateProjectClosureDto
{
    public string? Summary { get; set; }
    public string? LessonsLearned { get; set; }
    public string? Recommendations { get; set; }
    public List<ClosureCriteriaDto>? Checklist { get; set; }
}

public class ApproveClosureDto
{
    public bool Approve { get; set; }
    public string? RejectionReason { get; set; }
}

public class ClosureValidationDto
{
    public bool CanClose { get; set; }
    public List<string> MissingMandatoryCriteria { get; set; } = new();
    public int TotalMandatory { get; set; }
    public int CompletedMandatory { get; set; }
}

// ============= FINAL BUILD DTOs =============

public class FinalBuildDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    
    public string BuildNumber { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string? BuildTag { get; set; }
    public string? CommitHash { get; set; }
    
    public DateTime BuildDate { get; set; }
    public string BuiltBy { get; set; } = string.Empty;
    public string? BuildEnvironment { get; set; }
    public string? BuildConfiguration { get; set; }
    
    public List<BinaryArtifactDto> BinaryArtifacts { get; set; } = new();
    
    public string? MainDownloadUrl { get; set; }
    public string? DocumentationUrl { get; set; }
    public string? ReleaseNotesUrl { get; set; }
    
    public string? TargetPlatform { get; set; }
    public string? Dependencies { get; set; }
    public string? SystemRequirements { get; set; }
    
    public bool IsStable { get; set; }
    public int? TestsPassed { get; set; }
    public int? TestsTotal { get; set; }
    public double? CodeCoverage { get; set; }
    public string? QualityGateStatus { get; set; }
    
    public Guid? ClosureId { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class BinaryArtifactDto
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // EXECUTABLE, LIBRARY, PACKAGE, INSTALLER
    public string? FilePath { get; set; }
    public string? DownloadUrl { get; set; }
    public long? Size { get; set; }
    public string? Checksum { get; set; }
    public string? ChecksumType { get; set; } // MD5, SHA256
}

public class CreateFinalBuildDto
{
    public Guid ProjectId { get; set; }
    public string BuildNumber { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string? BuildTag { get; set; }
    public string? CommitHash { get; set; }
    public string? BuildEnvironment { get; set; }
    public string? BuildConfiguration { get; set; }
    public List<BinaryArtifactDto> BinaryArtifacts { get; set; } = new();
    public string? MainDownloadUrl { get; set; }
    public string? DocumentationUrl { get; set; }
    public string? ReleaseNotesUrl { get; set; }
    public string? TargetPlatform { get; set; }
    public string? Dependencies { get; set; }
    public string? SystemRequirements { get; set; }
    public bool IsStable { get; set; } = true;
    public int? TestsPassed { get; set; }
    public int? TestsTotal { get; set; }
    public double? CodeCoverage { get; set; }
    public string? QualityGateStatus { get; set; }
    public Guid? ClosureId { get; set; }
}

public class UpdateFinalBuildDto
{
    public string? BuildNumber { get; set; }
    public string? Version { get; set; }
    public string? BuildTag { get; set; }
    public string? CommitHash { get; set; }
    public string? BuildEnvironment { get; set; }
    public string? BuildConfiguration { get; set; }
    public List<BinaryArtifactDto>? BinaryArtifacts { get; set; }
    public string? MainDownloadUrl { get; set; }
    public string? DocumentationUrl { get; set; }
    public string? ReleaseNotesUrl { get; set; }
    public string? TargetPlatform { get; set; }
    public string? Dependencies { get; set; }
    public string? SystemRequirements { get; set; }
    public bool? IsStable { get; set; }
    public int? TestsPassed { get; set; }
    public int? TestsTotal { get; set; }
    public double? CodeCoverage { get; set; }
    public string? QualityGateStatus { get; set; }
    public Guid? ClosureId { get; set; }
}
