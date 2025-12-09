namespace OpenUpTool.Core.Entities;

/// <summary>
/// Represents a global configuration that can be applied to projects
/// Contains templates for roles, phases, artifact types, and workflows
/// </summary>
public class GlobalConfiguration
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Version { get; set; } = 1;
    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; } = false;
    public string? Tags { get; set; } // Comma-separated tags for categorization (HU-019)
    public Guid? ParentTemplateId { get; set; } // Reference to original template if cloned (HU-019)
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    
    // Navigation properties
    public ICollection<RoleTemplate> RoleTemplates { get; set; } = new List<RoleTemplate>();
    public ICollection<PhaseTemplate> PhaseTemplates { get; set; } = new List<PhaseTemplate>();
    public ICollection<ArtifactTypeTemplate> ArtifactTypeTemplates { get; set; } = new List<ArtifactTypeTemplate>();
    public ICollection<WorkflowTemplate> WorkflowTemplates { get; set; } = new List<WorkflowTemplate>();
    public ICollection<ConfigurationChangeHistory> ChangeHistory { get; set; } = new List<ConfigurationChangeHistory>();
}

/// <summary>
/// Role template for configuration
/// </summary>
public class RoleTemplate
{
    public Guid Id { get; set; }
    public Guid ConfigurationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Permissions { get; set; } // JSON array of permissions
    public int OrderIndex { get; set; }
    public bool IsSystem { get; set; } = false; // System roles cannot be deleted
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation
    public GlobalConfiguration Configuration { get; set; } = null!;
}

/// <summary>
/// Phase template for configuration
/// </summary>
public class PhaseTemplate
{
    public Guid Id { get; set; }
    public Guid ConfigurationId { get; set; }
    public string PhaseCode { get; set; } = string.Empty; // INCEPTION, ELABORATION, CONSTRUCTION, TRANSITION
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int OrderIndex { get; set; }
    public int? DefaultDurationDays { get; set; }
    public bool IsMandatory { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation
    public GlobalConfiguration Configuration { get; set; } = null!;
}

/// <summary>
/// Artifact type template for configuration
/// </summary>
public class ArtifactTypeTemplate
{
    public Guid Id { get; set; }
    public Guid ConfigurationId { get; set; }
    public string PhaseCode { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsMandatory { get; set; }
    public string DefaultFormat { get; set; } = "TEXT";
    public int OrderIndex { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation
    public GlobalConfiguration Configuration { get; set; } = null!;
    public ICollection<CustomFieldDefinition> CustomFields { get; set; } = new List<CustomFieldDefinition>();
}

/// <summary>
/// Workflow template for configuration
/// </summary>
public class WorkflowTemplate
{
    public Guid Id { get; set; }
    public Guid ConfigurationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsDefault { get; set; } = false;
    public int OrderIndex { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation
    public GlobalConfiguration Configuration { get; set; } = null!;
    public ICollection<WorkflowStateTemplate> States { get; set; } = new List<WorkflowStateTemplate>();
}

/// <summary>
/// Workflow state template
/// </summary>
public class WorkflowStateTemplate
{
    public Guid Id { get; set; }
    public Guid WorkflowTemplateId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int OrderIndex { get; set; }
    public string? Color { get; set; }
    public bool IsInitialState { get; set; }
    public bool IsFinalState { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation
    public WorkflowTemplate WorkflowTemplate { get; set; } = null!;
}

/// <summary>
/// Custom field definition for artifact types
/// </summary>
public class CustomFieldDefinition
{
    public Guid Id { get; set; }
    public Guid ArtifactTypeTemplateId { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string FieldType { get; set; } = "TEXT"; // TEXT, NUMBER, DATE, BOOLEAN, SELECT, MULTISELECT
    public bool IsRequired { get; set; } = false;
    public string? DefaultValue { get; set; }
    public string? Options { get; set; } // JSON array for SELECT/MULTISELECT
    public string? ValidationRules { get; set; } // JSON for validation
    public int OrderIndex { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation
    public ArtifactTypeTemplate ArtifactTypeTemplate { get; set; } = null!;
}

/// <summary>
/// Tracks all changes made to configurations for audit and rollback
/// </summary>
public class ConfigurationChangeHistory
{
    public Guid Id { get; set; }
    public Guid ConfigurationId { get; set; }
    public int FromVersion { get; set; }
    public int ToVersion { get; set; }
    public string ChangeType { get; set; } = string.Empty; // CREATE, UPDATE, DELETE, ROLLBACK
    public string EntityType { get; set; } = string.Empty; // ROLE, PHASE, ARTIFACT_TYPE, WORKFLOW, CUSTOM_FIELD
    public Guid? EntityId { get; set; }
    public string? EntityName { get; set; }
    public string? OldValue { get; set; } // JSON snapshot before change
    public string? NewValue { get; set; } // JSON snapshot after change
    public string ChangedBy { get; set; } = string.Empty;
    public string? ChangeDescription { get; set; }
    public DateTime ChangedAt { get; set; }
    
    // Navigation
    public GlobalConfiguration Configuration { get; set; } = null!;
}

/// <summary>
/// Tracks which configuration is applied to each project
/// </summary>
public class ProjectConfiguration
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid ConfigurationId { get; set; }
    public int AppliedVersion { get; set; }
    public DateTime AppliedAt { get; set; }
    public string AppliedBy { get; set; } = string.Empty;
    public bool AutoUpdate { get; set; } = false; // Auto-update when configuration changes
    
    // Navigation
    public Project Project { get; set; } = null!;
    public GlobalConfiguration Configuration { get; set; } = null!;
}

/// <summary>
/// Custom field values for artifacts
/// </summary>
public class ArtifactCustomFieldValue
{
    public Guid Id { get; set; }
    public Guid ArtifactId { get; set; }
    public Guid CustomFieldDefinitionId { get; set; }
    public string? Value { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation
    public Artifact Artifact { get; set; } = null!;
    public CustomFieldDefinition CustomFieldDefinition { get; set; } = null!;
}
