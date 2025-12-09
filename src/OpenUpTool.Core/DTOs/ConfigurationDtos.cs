namespace OpenUpTool.Core.DTOs;

// ========== Global Configuration DTOs ==========

public class GlobalConfigurationDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Version { get; set; }
    public bool IsActive { get; set; }
    public bool IsDefault { get; set; }
    public string? Tags { get; set; }
    public Guid? ParentTemplateId { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int RoleTemplatesCount { get; set; }
    public int PhaseTemplatesCount { get; set; }
    public int ArtifactTypeTemplatesCount { get; set; }
    public int WorkflowTemplatesCount { get; set; }
}

public class GlobalConfigurationDetailDto : GlobalConfigurationDto
{
    public List<RoleTemplateDto> RoleTemplates { get; set; } = new();
    public List<PhaseTemplateDto> PhaseTemplates { get; set; } = new();
    public List<ArtifactTypeTemplateDto> ArtifactTypeTemplates { get; set; } = new();
    public List<WorkflowTemplateDto> WorkflowTemplates { get; set; } = new();
}

public class CreateGlobalConfigurationDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsDefault { get; set; } = false;
    public bool CopyFromDefault { get; set; } = true; // Copy templates from default configuration
}

public class UpdateGlobalConfigurationDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public bool IsDefault { get; set; }
}

// ========== Role Template DTOs ==========

public class RoleTemplateDto
{
    public Guid Id { get; set; }
    public Guid ConfigurationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string>? Permissions { get; set; }
    public int OrderIndex { get; set; }
    public bool IsSystem { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateRoleTemplateDto
{
    public Guid ConfigurationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string>? Permissions { get; set; }
    public int OrderIndex { get; set; }
}

public class UpdateRoleTemplateDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string>? Permissions { get; set; }
    public int OrderIndex { get; set; }
}

// ========== Phase Template DTOs ==========

public class PhaseTemplateDto
{
    public Guid Id { get; set; }
    public Guid ConfigurationId { get; set; }
    public string PhaseCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int OrderIndex { get; set; }
    public int? DefaultDurationDays { get; set; }
    public bool IsMandatory { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreatePhaseTemplateDto
{
    public Guid ConfigurationId { get; set; }
    public string PhaseCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int OrderIndex { get; set; }
    public int? DefaultDurationDays { get; set; }
    public bool IsMandatory { get; set; } = true;
}

public class UpdatePhaseTemplateDto
{
    public string PhaseCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int OrderIndex { get; set; }
    public int? DefaultDurationDays { get; set; }
    public bool IsMandatory { get; set; }
}

// ========== Artifact Type Template DTOs ==========

public class ArtifactTypeTemplateDto
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
    public List<CustomFieldDefinitionDto> CustomFields { get; set; } = new();
}

public class CreateArtifactTypeTemplateDto
{
    public Guid ConfigurationId { get; set; }
    public string PhaseCode { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsMandatory { get; set; }
    public string DefaultFormat { get; set; } = "TEXT";
    public int OrderIndex { get; set; }
}

public class UpdateArtifactTypeTemplateDto
{
    public string PhaseCode { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsMandatory { get; set; }
    public string DefaultFormat { get; set; } = "TEXT";
    public int OrderIndex { get; set; }
}

// ========== Workflow Template DTOs ==========

public class WorkflowTemplateDto
{
    public Guid Id { get; set; }
    public Guid ConfigurationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsDefault { get; set; }
    public int OrderIndex { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<WorkflowStateTemplateDto> States { get; set; } = new();
}

public class CreateWorkflowTemplateDto
{
    public Guid ConfigurationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsDefault { get; set; } = false;
    public int OrderIndex { get; set; }
}

public class UpdateWorkflowTemplateDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsDefault { get; set; }
    public int OrderIndex { get; set; }
}

// ========== Workflow State Template DTOs ==========

public class WorkflowStateTemplateDto
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
}

public class CreateWorkflowStateTemplateDto
{
    public Guid WorkflowTemplateId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int OrderIndex { get; set; }
    public string? Color { get; set; }
    public bool IsInitialState { get; set; }
    public bool IsFinalState { get; set; }
}

public class UpdateWorkflowStateTemplateDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int OrderIndex { get; set; }
    public string? Color { get; set; }
    public bool IsInitialState { get; set; }
    public bool IsFinalState { get; set; }
}

// ========== Custom Field DTOs ==========

public class CustomFieldDefinitionDto
{
    public Guid Id { get; set; }
    public Guid ArtifactTypeTemplateId { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string FieldType { get; set; } = "TEXT";
    public bool IsRequired { get; set; }
    public string? DefaultValue { get; set; }
    public List<string>? Options { get; set; }
    public Dictionary<string, object>? ValidationRules { get; set; }
    public int OrderIndex { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateCustomFieldDefinitionDto
{
    public Guid ArtifactTypeTemplateId { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string FieldType { get; set; } = "TEXT"; // TEXT, NUMBER, DATE, BOOLEAN, SELECT, MULTISELECT
    public bool IsRequired { get; set; } = false;
    public string? DefaultValue { get; set; }
    public List<string>? Options { get; set; }
    public Dictionary<string, object>? ValidationRules { get; set; }
    public int OrderIndex { get; set; }
}

public class UpdateCustomFieldDefinitionDto
{
    public string FieldName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string FieldType { get; set; } = "TEXT";
    public bool IsRequired { get; set; }
    public string? DefaultValue { get; set; }
    public List<string>? Options { get; set; }
    public Dictionary<string, object>? ValidationRules { get; set; }
    public int OrderIndex { get; set; }
}

// ========== Artifact Custom Field Value DTOs ==========

public class ArtifactCustomFieldValueDto
{
    public Guid Id { get; set; }
    public Guid ArtifactId { get; set; }
    public Guid CustomFieldDefinitionId { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string FieldType { get; set; } = string.Empty;
    public string? Value { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class SetArtifactCustomFieldValueDto
{
    public Guid ArtifactId { get; set; }
    public Guid CustomFieldDefinitionId { get; set; }
    public string? Value { get; set; }
}

public class BulkSetCustomFieldValuesDto
{
    public Guid ArtifactId { get; set; }
    public List<CustomFieldValueItem> Values { get; set; } = new();
}

public class CustomFieldValueItem
{
    public Guid CustomFieldDefinitionId { get; set; }
    public string? Value { get; set; }
}

// ========== Configuration Change History DTOs ==========

public class ConfigurationChangeHistoryDto
{
    public Guid Id { get; set; }
    public Guid ConfigurationId { get; set; }
    public int FromVersion { get; set; }
    public int ToVersion { get; set; }
    public string ChangeType { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public string? EntityName { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string ChangedBy { get; set; } = string.Empty;
    public string? ChangeDescription { get; set; }
    public DateTime ChangedAt { get; set; }
}

// ========== Project Configuration DTOs ==========

public class ProjectConfigurationDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public Guid ConfigurationId { get; set; }
    public string ConfigurationName { get; set; } = string.Empty;
    public int AppliedVersion { get; set; }
    public int CurrentVersion { get; set; }
    public bool HasPendingUpdates { get; set; }
    public DateTime AppliedAt { get; set; }
    public string AppliedBy { get; set; } = string.Empty;
    public bool AutoUpdate { get; set; }
}

public class ApplyConfigurationToProjectDto
{
    public Guid ProjectId { get; set; }
    public Guid ConfigurationId { get; set; }
    public bool AutoUpdate { get; set; } = false;
    public bool ForceUpdate { get; set; } = false; // Apply even if there are conflicts
}

public class ApplyConfigurationToProjectByConfigDto
{
    public Guid ProjectId { get; set; }
    public bool AutoUpdate { get; set; } = false;
    public bool ForceUpdate { get; set; } = false;
}

public class UpdateProjectConfigurationSettingsDto
{
    public bool AutoUpdate { get; set; }
}

// ========== Rollback DTOs ==========

public class RollbackConfigurationDto
{
    public int TargetVersion { get; set; }
    public string? Reason { get; set; }
}

// ========== Apply Updates DTOs ==========

public class ApplyConfigurationUpdatesDto
{
    public List<Guid> ProjectIds { get; set; } = new();
    public bool IncludeWarning { get; set; } = true;
}

public class ConfigurationUpdateResultDto
{
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? Message { get; set; }
    public List<string> Warnings { get; set; } = new();
    public List<string> Changes { get; set; } = new();
}

public class BulkUpdateResultDto
{
    public int TotalProjects { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<ConfigurationUpdateResultDto> Results { get; set; } = new();
}

// ========== HU-019: OpenUP Template DTOs ==========

/// <summary>
/// DTO para guardar una configuración como plantilla OpenUP
/// </summary>
public class SaveAsTemplateDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Tags { get; set; } // Comma-separated tags for categorization
}

/// <summary>
/// DTO para clonar una plantilla existente
/// </summary>
public class CloneTemplateDto
{
    public string NewName { get; set; } = string.Empty;
    public string? NewDescription { get; set; }
}

/// <summary>
/// DTO para listar plantillas con metadatos resumidos
/// </summary>
public class TemplateListItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Version { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
    public string? Tags { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int RolesCount { get; set; }
    public int PhasesCount { get; set; }
    public int ArtifactTypesCount { get; set; }
    public int WorkflowsCount { get; set; }
    public int ProjectsUsingCount { get; set; }
}

/// <summary>
/// DTO para comparación de dos plantillas
/// </summary>
public class TemplateComparisonDto
{
    public TemplateMetadataDto Template1 { get; set; } = new();
    public TemplateMetadataDto Template2 { get; set; } = new();
    public List<ComparisonDifferenceDto> Differences { get; set; } = new();
    public TemplateComparisonSummaryDto Summary { get; set; } = new();
}

public class TemplateMetadataDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Version { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int RolesCount { get; set; }
    public int PhasesCount { get; set; }
    public int ArtifactTypesCount { get; set; }
    public int WorkflowsCount { get; set; }
    public int CustomFieldsCount { get; set; }
}

public class ComparisonDifferenceDto
{
    public string EntityType { get; set; } = string.Empty; // ROLE, PHASE, ARTIFACT_TYPE, WORKFLOW, CUSTOM_FIELD
    public string DifferenceType { get; set; } = string.Empty; // ADDED, REMOVED, MODIFIED
    public string EntityName { get; set; } = string.Empty;
    public string? Template1Value { get; set; }
    public string? Template2Value { get; set; }
    public string? PropertyChanged { get; set; } // For MODIFIED differences
}

public class TemplateComparisonSummaryDto
{
    public int TotalDifferences { get; set; }
    public int RolesDifferences { get; set; }
    public int PhasesDifferences { get; set; }
    public int ArtifactTypesDifferences { get; set; }
    public int WorkflowsDifferences { get; set; }
    public int CustomFieldsDifferences { get; set; }
    public bool AreIdentical { get; set; }
}

/// <summary>
/// DTO para exportar una plantilla
/// </summary>
public class TemplateExportDto
{
    public Guid OriginalId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Version { get; set; }
    public string? Tags { get; set; }
    public DateTime ExportedAt { get; set; }
    public string ExportedBy { get; set; } = string.Empty;
    public List<RoleTemplateDto> Roles { get; set; } = new();
    public List<PhaseTemplateDto> Phases { get; set; } = new();
    public List<ArtifactTypeTemplateDto> ArtifactTypes { get; set; } = new();
    public List<WorkflowTemplateDto> Workflows { get; set; } = new();
}

/// <summary>
/// DTO para importar una plantilla
/// </summary>
public class TemplateImportDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Tags { get; set; }
    public List<RoleTemplateDto>? Roles { get; set; }
    public List<PhaseTemplateDto>? Phases { get; set; }
    public List<ArtifactTypeTemplateDto>? ArtifactTypes { get; set; }
    public List<WorkflowTemplateDto>? Workflows { get; set; }
}
