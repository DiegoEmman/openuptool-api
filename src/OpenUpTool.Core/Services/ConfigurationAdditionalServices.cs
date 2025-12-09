using OpenUpTool.Core.DTOs;
using OpenUpTool.Core.Entities;
using OpenUpTool.Core.Interfaces;
using System.Text.Json;

namespace OpenUpTool.Core.Services;

// ========== Workflow Template Service ==========

public class WorkflowTemplateService : IWorkflowTemplateService
{
    private readonly IWorkflowTemplateRepository _workflowRepository;
    private readonly IGlobalConfigurationRepository _configRepository;
    private readonly IConfigurationChangeHistoryRepository _historyRepository;

    public WorkflowTemplateService(
        IWorkflowTemplateRepository workflowRepository,
        IGlobalConfigurationRepository configRepository,
        IConfigurationChangeHistoryRepository historyRepository)
    {
        _workflowRepository = workflowRepository;
        _configRepository = configRepository;
        _historyRepository = historyRepository;
    }

    public async Task<IEnumerable<WorkflowTemplateDto>> GetWorkflowsByConfigurationAsync(Guid configurationId)
    {
        var workflows = await _workflowRepository.GetByConfigurationIdAsync(configurationId);
        return workflows.Select(MapToDto);
    }

    public async Task<WorkflowTemplateDto?> GetWorkflowByIdAsync(Guid id)
    {
        var workflow = await _workflowRepository.GetByIdWithStatesAsync(id);
        return workflow == null ? null : MapToDto(workflow);
    }

    public async Task<WorkflowTemplateDto> CreateWorkflowAsync(CreateWorkflowTemplateDto dto, string changedBy)
    {
        var config = await _configRepository.GetByIdAsync(dto.ConfigurationId);
        if (config == null) throw new ArgumentException("Configuración no encontrada");

        var workflow = new WorkflowTemplate
        {
            Id = Guid.NewGuid(),
            ConfigurationId = dto.ConfigurationId,
            Name = dto.Name,
            Description = dto.Description,
            IsDefault = dto.IsDefault,
            OrderIndex = dto.OrderIndex
        };

        var created = await _workflowRepository.CreateAsync(workflow);

        await _historyRepository.CreateAsync(new ConfigurationChangeHistory
        {
            Id = Guid.NewGuid(),
            ConfigurationId = dto.ConfigurationId,
            FromVersion = config.Version,
            ToVersion = config.Version,
            ChangeType = "CREATE",
            EntityType = "WORKFLOW",
            EntityId = created.Id,
            EntityName = created.Name,
            NewValue = JsonSerializer.Serialize(dto),
            ChangedBy = changedBy,
            ChangeDescription = $"Workflow '{created.Name}' creado"
        });

        return MapToDto(created);
    }

    public async Task<WorkflowTemplateDto?> UpdateWorkflowAsync(Guid id, UpdateWorkflowTemplateDto dto, string changedBy)
    {
        var workflow = await _workflowRepository.GetByIdAsync(id);
        if (workflow == null) return null;

        var oldValue = JsonSerializer.Serialize(MapToDto(workflow));

        workflow.Name = dto.Name;
        workflow.Description = dto.Description;
        workflow.IsDefault = dto.IsDefault;
        workflow.OrderIndex = dto.OrderIndex;

        var updated = await _workflowRepository.UpdateAsync(workflow);

        await _historyRepository.CreateAsync(new ConfigurationChangeHistory
        {
            Id = Guid.NewGuid(),
            ConfigurationId = workflow.ConfigurationId,
            FromVersion = 0,
            ToVersion = 0,
            ChangeType = "UPDATE",
            EntityType = "WORKFLOW",
            EntityId = workflow.Id,
            EntityName = workflow.Name,
            OldValue = oldValue,
            NewValue = JsonSerializer.Serialize(dto),
            ChangedBy = changedBy,
            ChangeDescription = $"Workflow '{workflow.Name}' actualizado"
        });

        return MapToDto(updated);
    }

    public async Task<bool> DeleteWorkflowAsync(Guid id, string changedBy)
    {
        var workflow = await _workflowRepository.GetByIdAsync(id);
        if (workflow == null) return false;

        await _historyRepository.CreateAsync(new ConfigurationChangeHistory
        {
            Id = Guid.NewGuid(),
            ConfigurationId = workflow.ConfigurationId,
            FromVersion = 0,
            ToVersion = 0,
            ChangeType = "DELETE",
            EntityType = "WORKFLOW",
            EntityId = workflow.Id,
            EntityName = workflow.Name,
            OldValue = JsonSerializer.Serialize(MapToDto(workflow)),
            ChangedBy = changedBy,
            ChangeDescription = $"Workflow '{workflow.Name}' eliminado"
        });

        await _workflowRepository.DeleteAsync(id);
        return true;
    }

    private WorkflowTemplateDto MapToDto(WorkflowTemplate w) => new()
    {
        Id = w.Id,
        ConfigurationId = w.ConfigurationId,
        Name = w.Name,
        Description = w.Description,
        IsDefault = w.IsDefault,
        OrderIndex = w.OrderIndex,
        CreatedAt = w.CreatedAt,
        UpdatedAt = w.UpdatedAt,
        States = w.States.Select(s => new WorkflowStateTemplateDto
        {
            Id = s.Id,
            WorkflowTemplateId = s.WorkflowTemplateId,
            Name = s.Name,
            Description = s.Description,
            OrderIndex = s.OrderIndex,
            Color = s.Color,
            IsInitialState = s.IsInitialState,
            IsFinalState = s.IsFinalState,
            CreatedAt = s.CreatedAt,
            UpdatedAt = s.UpdatedAt
        }).ToList()
    };
}

// ========== Workflow State Template Service ==========

public class WorkflowStateTemplateService : IWorkflowStateTemplateService
{
    private readonly IWorkflowStateTemplateRepository _stateRepository;
    private readonly IWorkflowTemplateRepository _workflowRepository;
    private readonly IConfigurationChangeHistoryRepository _historyRepository;

    public WorkflowStateTemplateService(
        IWorkflowStateTemplateRepository stateRepository,
        IWorkflowTemplateRepository workflowRepository,
        IConfigurationChangeHistoryRepository historyRepository)
    {
        _stateRepository = stateRepository;
        _workflowRepository = workflowRepository;
        _historyRepository = historyRepository;
    }

    public async Task<IEnumerable<WorkflowStateTemplateDto>> GetStatesByWorkflowAsync(Guid workflowTemplateId)
    {
        var states = await _stateRepository.GetByWorkflowTemplateIdAsync(workflowTemplateId);
        return states.Select(MapToDto);
    }

    public async Task<WorkflowStateTemplateDto?> GetStateByIdAsync(Guid id)
    {
        var state = await _stateRepository.GetByIdAsync(id);
        return state == null ? null : MapToDto(state);
    }

    public async Task<WorkflowStateTemplateDto> CreateStateAsync(CreateWorkflowStateTemplateDto dto, string changedBy)
    {
        var workflow = await _workflowRepository.GetByIdAsync(dto.WorkflowTemplateId);
        if (workflow == null) throw new ArgumentException("Workflow no encontrado");

        var state = new WorkflowStateTemplate
        {
            Id = Guid.NewGuid(),
            WorkflowTemplateId = dto.WorkflowTemplateId,
            Name = dto.Name,
            Description = dto.Description,
            OrderIndex = dto.OrderIndex,
            Color = dto.Color,
            IsInitialState = dto.IsInitialState,
            IsFinalState = dto.IsFinalState
        };

        var created = await _stateRepository.CreateAsync(state);

        await _historyRepository.CreateAsync(new ConfigurationChangeHistory
        {
            Id = Guid.NewGuid(),
            ConfigurationId = workflow.ConfigurationId,
            FromVersion = 0,
            ToVersion = 0,
            ChangeType = "CREATE",
            EntityType = "WORKFLOW_STATE",
            EntityId = created.Id,
            EntityName = created.Name,
            NewValue = JsonSerializer.Serialize(dto),
            ChangedBy = changedBy,
            ChangeDescription = $"Estado de workflow '{created.Name}' creado"
        });

        return MapToDto(created);
    }

    public async Task<WorkflowStateTemplateDto?> UpdateStateAsync(Guid id, UpdateWorkflowStateTemplateDto dto, string changedBy)
    {
        var state = await _stateRepository.GetByIdAsync(id);
        if (state == null) return null;

        var workflow = await _workflowRepository.GetByIdAsync(state.WorkflowTemplateId);
        var oldValue = JsonSerializer.Serialize(MapToDto(state));

        state.Name = dto.Name;
        state.Description = dto.Description;
        state.OrderIndex = dto.OrderIndex;
        state.Color = dto.Color;
        state.IsInitialState = dto.IsInitialState;
        state.IsFinalState = dto.IsFinalState;

        var updated = await _stateRepository.UpdateAsync(state);

        await _historyRepository.CreateAsync(new ConfigurationChangeHistory
        {
            Id = Guid.NewGuid(),
            ConfigurationId = workflow?.ConfigurationId ?? Guid.Empty,
            FromVersion = 0,
            ToVersion = 0,
            ChangeType = "UPDATE",
            EntityType = "WORKFLOW_STATE",
            EntityId = state.Id,
            EntityName = state.Name,
            OldValue = oldValue,
            NewValue = JsonSerializer.Serialize(dto),
            ChangedBy = changedBy,
            ChangeDescription = $"Estado de workflow '{state.Name}' actualizado"
        });

        return MapToDto(updated);
    }

    public async Task<bool> DeleteStateAsync(Guid id, string changedBy)
    {
        var state = await _stateRepository.GetByIdAsync(id);
        if (state == null) return false;

        var workflow = await _workflowRepository.GetByIdAsync(state.WorkflowTemplateId);

        await _historyRepository.CreateAsync(new ConfigurationChangeHistory
        {
            Id = Guid.NewGuid(),
            ConfigurationId = workflow?.ConfigurationId ?? Guid.Empty,
            FromVersion = 0,
            ToVersion = 0,
            ChangeType = "DELETE",
            EntityType = "WORKFLOW_STATE",
            EntityId = state.Id,
            EntityName = state.Name,
            OldValue = JsonSerializer.Serialize(MapToDto(state)),
            ChangedBy = changedBy,
            ChangeDescription = $"Estado de workflow '{state.Name}' eliminado"
        });

        await _stateRepository.DeleteAsync(id);
        return true;
    }

    private WorkflowStateTemplateDto MapToDto(WorkflowStateTemplate s) => new()
    {
        Id = s.Id,
        WorkflowTemplateId = s.WorkflowTemplateId,
        Name = s.Name,
        Description = s.Description,
        OrderIndex = s.OrderIndex,
        Color = s.Color,
        IsInitialState = s.IsInitialState,
        IsFinalState = s.IsFinalState,
        CreatedAt = s.CreatedAt,
        UpdatedAt = s.UpdatedAt
    };
}

// ========== Custom Field Definition Service ==========

public class CustomFieldDefinitionService : ICustomFieldDefinitionService
{
    private readonly ICustomFieldDefinitionRepository _fieldRepository;
    private readonly IArtifactTypeTemplateRepository _artifactTypeRepository;
    private readonly IConfigurationChangeHistoryRepository _historyRepository;

    public CustomFieldDefinitionService(
        ICustomFieldDefinitionRepository fieldRepository,
        IArtifactTypeTemplateRepository artifactTypeRepository,
        IConfigurationChangeHistoryRepository historyRepository)
    {
        _fieldRepository = fieldRepository;
        _artifactTypeRepository = artifactTypeRepository;
        _historyRepository = historyRepository;
    }

    public async Task<IEnumerable<CustomFieldDefinitionDto>> GetFieldsByArtifactTypeAsync(Guid artifactTypeTemplateId)
    {
        var fields = await _fieldRepository.GetByArtifactTypeTemplateIdAsync(artifactTypeTemplateId);
        return fields.Select(MapToDto);
    }

    public async Task<IEnumerable<CustomFieldDefinitionDto>> GetFieldsByConfigurationAsync(Guid configurationId)
    {
        var fields = await _fieldRepository.GetByConfigurationIdAsync(configurationId);
        return fields.Select(MapToDto);
    }

    public async Task<CustomFieldDefinitionDto?> GetFieldByIdAsync(Guid id)
    {
        var field = await _fieldRepository.GetByIdAsync(id);
        return field == null ? null : MapToDto(field);
    }

    public async Task<CustomFieldDefinitionDto> CreateFieldAsync(CreateCustomFieldDefinitionDto dto, string changedBy)
    {
        var artifactType = await _artifactTypeRepository.GetByIdAsync(dto.ArtifactTypeTemplateId);
        if (artifactType == null) throw new ArgumentException("Tipo de artefacto no encontrado");

        // Validar tipo de campo
        var validTypes = new[] { "TEXT", "NUMBER", "DATE", "BOOLEAN", "SELECT", "MULTISELECT" };
        if (!validTypes.Contains(dto.FieldType.ToUpper()))
            throw new ArgumentException($"Tipo de campo inválido. Valores permitidos: {string.Join(", ", validTypes)}");

        var field = new CustomFieldDefinition
        {
            Id = Guid.NewGuid(),
            ArtifactTypeTemplateId = dto.ArtifactTypeTemplateId,
            FieldName = dto.FieldName,
            DisplayName = dto.DisplayName,
            FieldType = dto.FieldType.ToUpper(),
            IsRequired = dto.IsRequired,
            DefaultValue = dto.DefaultValue,
            Options = dto.Options != null ? JsonSerializer.Serialize(dto.Options) : null,
            ValidationRules = dto.ValidationRules != null ? JsonSerializer.Serialize(dto.ValidationRules) : null,
            OrderIndex = dto.OrderIndex
        };

        var created = await _fieldRepository.CreateAsync(field);

        await _historyRepository.CreateAsync(new ConfigurationChangeHistory
        {
            Id = Guid.NewGuid(),
            ConfigurationId = artifactType.ConfigurationId,
            FromVersion = 0,
            ToVersion = 0,
            ChangeType = "CREATE",
            EntityType = "CUSTOM_FIELD",
            EntityId = created.Id,
            EntityName = created.DisplayName,
            NewValue = JsonSerializer.Serialize(dto),
            ChangedBy = changedBy,
            ChangeDescription = $"Campo personalizado '{created.DisplayName}' creado"
        });

        return MapToDto(created);
    }

    public async Task<CustomFieldDefinitionDto?> UpdateFieldAsync(Guid id, UpdateCustomFieldDefinitionDto dto, string changedBy)
    {
        var field = await _fieldRepository.GetByIdAsync(id);
        if (field == null) return null;

        var artifactType = await _artifactTypeRepository.GetByIdAsync(field.ArtifactTypeTemplateId);
        var oldValue = JsonSerializer.Serialize(MapToDto(field));

        field.FieldName = dto.FieldName;
        field.DisplayName = dto.DisplayName;
        field.FieldType = dto.FieldType.ToUpper();
        field.IsRequired = dto.IsRequired;
        field.DefaultValue = dto.DefaultValue;
        field.Options = dto.Options != null ? JsonSerializer.Serialize(dto.Options) : null;
        field.ValidationRules = dto.ValidationRules != null ? JsonSerializer.Serialize(dto.ValidationRules) : null;
        field.OrderIndex = dto.OrderIndex;

        var updated = await _fieldRepository.UpdateAsync(field);

        await _historyRepository.CreateAsync(new ConfigurationChangeHistory
        {
            Id = Guid.NewGuid(),
            ConfigurationId = artifactType?.ConfigurationId ?? Guid.Empty,
            FromVersion = 0,
            ToVersion = 0,
            ChangeType = "UPDATE",
            EntityType = "CUSTOM_FIELD",
            EntityId = field.Id,
            EntityName = field.DisplayName,
            OldValue = oldValue,
            NewValue = JsonSerializer.Serialize(dto),
            ChangedBy = changedBy,
            ChangeDescription = $"Campo personalizado '{field.DisplayName}' actualizado"
        });

        return MapToDto(updated);
    }

    public async Task<bool> DeleteFieldAsync(Guid id, string changedBy)
    {
        var field = await _fieldRepository.GetByIdAsync(id);
        if (field == null) return false;

        var artifactType = await _artifactTypeRepository.GetByIdAsync(field.ArtifactTypeTemplateId);

        await _historyRepository.CreateAsync(new ConfigurationChangeHistory
        {
            Id = Guid.NewGuid(),
            ConfigurationId = artifactType?.ConfigurationId ?? Guid.Empty,
            FromVersion = 0,
            ToVersion = 0,
            ChangeType = "DELETE",
            EntityType = "CUSTOM_FIELD",
            EntityId = field.Id,
            EntityName = field.DisplayName,
            OldValue = JsonSerializer.Serialize(MapToDto(field)),
            ChangedBy = changedBy,
            ChangeDescription = $"Campo personalizado '{field.DisplayName}' eliminado"
        });

        await _fieldRepository.DeleteAsync(id);
        return true;
    }

    private CustomFieldDefinitionDto MapToDto(CustomFieldDefinition f) => new()
    {
        Id = f.Id,
        ArtifactTypeTemplateId = f.ArtifactTypeTemplateId,
        FieldName = f.FieldName,
        DisplayName = f.DisplayName,
        FieldType = f.FieldType,
        IsRequired = f.IsRequired,
        DefaultValue = f.DefaultValue,
        Options = string.IsNullOrEmpty(f.Options) ? null : JsonSerializer.Deserialize<List<string>>(f.Options),
        ValidationRules = string.IsNullOrEmpty(f.ValidationRules) ? null : JsonSerializer.Deserialize<Dictionary<string, object>>(f.ValidationRules),
        OrderIndex = f.OrderIndex,
        CreatedAt = f.CreatedAt,
        UpdatedAt = f.UpdatedAt
    };
}

// ========== Project Configuration Service ==========

public class ProjectConfigurationService : IProjectConfigurationService
{
    private readonly IProjectConfigurationRepository _projectConfigRepository;
    private readonly IGlobalConfigurationRepository _configRepository;
    private readonly IProjectRepository _projectRepository;

    public ProjectConfigurationService(
        IProjectConfigurationRepository projectConfigRepository,
        IGlobalConfigurationRepository configRepository,
        IProjectRepository projectRepository)
    {
        _projectConfigRepository = projectConfigRepository;
        _configRepository = configRepository;
        _projectRepository = projectRepository;
    }

    public async Task<IEnumerable<ProjectConfigurationDto>> GetAllProjectConfigurationsAsync()
    {
        var configs = await _projectConfigRepository.GetAllAsync();
        return configs.Select(MapToDto);
    }

    public async Task<IEnumerable<ProjectConfigurationDto>> GetProjectsByConfigurationAsync(Guid configurationId)
    {
        var configs = await _projectConfigRepository.GetByConfigurationIdAsync(configurationId);
        return configs.Select(MapToDto);
    }

    public async Task<ProjectConfigurationDto?> GetProjectConfigurationAsync(Guid projectId)
    {
        var config = await _projectConfigRepository.GetByProjectIdAsync(projectId);
        return config == null ? null : MapToDto(config);
    }

    public async Task<ProjectConfigurationDto> ApplyConfigurationToProjectAsync(ApplyConfigurationToProjectDto dto, string appliedBy)
    {
        var project = await _projectRepository.GetByIdAsync(dto.ProjectId);
        if (project == null) throw new ArgumentException("Proyecto no encontrado");

        var config = await _configRepository.GetByIdAsync(dto.ConfigurationId);
        if (config == null) throw new ArgumentException("Configuración no encontrada");

        var existing = await _projectConfigRepository.GetByProjectIdAsync(dto.ProjectId);
        if (existing != null)
        {
            existing.ConfigurationId = dto.ConfigurationId;
            existing.AppliedVersion = config.Version;
            existing.AppliedAt = DateTime.UtcNow;
            existing.AppliedBy = appliedBy;
            existing.AutoUpdate = dto.AutoUpdate;
            
            var updated = await _projectConfigRepository.UpdateAsync(existing);
            return MapToDto(updated);
        }

        var projectConfig = new ProjectConfiguration
        {
            Id = Guid.NewGuid(),
            ProjectId = dto.ProjectId,
            ConfigurationId = dto.ConfigurationId,
            AppliedVersion = config.Version,
            AppliedBy = appliedBy,
            AutoUpdate = dto.AutoUpdate
        };

        var created = await _projectConfigRepository.CreateAsync(projectConfig);
        created.Project = project;
        created.Configuration = config;
        return MapToDto(created);
    }

    public async Task<ProjectConfigurationDto?> UpdateProjectConfigurationSettingsAsync(Guid projectId, UpdateProjectConfigurationSettingsDto dto)
    {
        var config = await _projectConfigRepository.GetByProjectIdAsync(projectId);
        if (config == null) return null;

        config.AutoUpdate = dto.AutoUpdate;
        var updated = await _projectConfigRepository.UpdateAsync(config);
        return MapToDto(updated);
    }

    public async Task<BulkUpdateResultDto> ApplyConfigurationUpdatesToProjectsAsync(Guid configurationId, ApplyConfigurationUpdatesDto dto, string appliedBy)
    {
        var config = await _configRepository.GetByIdAsync(configurationId);
        if (config == null) throw new ArgumentException("Configuración no encontrada");

        var results = new List<ConfigurationUpdateResultDto>();
        var projectConfigs = await _projectConfigRepository.GetByConfigurationIdAsync(configurationId);

        var projectsToUpdate = dto.ProjectIds.Any()
            ? projectConfigs.Where(pc => dto.ProjectIds.Contains(pc.ProjectId))
            : projectConfigs.Where(pc => pc.AutoUpdate);

        foreach (var pc in projectsToUpdate)
        {
            try
            {
                pc.AppliedVersion = config.Version;
                pc.AppliedAt = DateTime.UtcNow;
                pc.AppliedBy = appliedBy;
                await _projectConfigRepository.UpdateAsync(pc);

                results.Add(new ConfigurationUpdateResultDto
                {
                    ProjectId = pc.ProjectId,
                    ProjectName = pc.Project?.Name ?? "Unknown",
                    Success = true,
                    Message = "Configuración actualizada exitosamente",
                    Changes = new List<string> { $"Versión actualizada a {config.Version}" }
                });
            }
            catch (Exception ex)
            {
                results.Add(new ConfigurationUpdateResultDto
                {
                    ProjectId = pc.ProjectId,
                    ProjectName = pc.Project?.Name ?? "Unknown",
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        return new BulkUpdateResultDto
        {
            TotalProjects = results.Count,
            SuccessCount = results.Count(r => r.Success),
            FailureCount = results.Count(r => !r.Success),
            Results = results
        };
    }

    private ProjectConfigurationDto MapToDto(ProjectConfiguration pc) => new()
    {
        Id = pc.Id,
        ProjectId = pc.ProjectId,
        ProjectName = pc.Project?.Name ?? string.Empty,
        ConfigurationId = pc.ConfigurationId,
        ConfigurationName = pc.Configuration?.Name ?? string.Empty,
        AppliedVersion = pc.AppliedVersion,
        CurrentVersion = pc.Configuration?.Version ?? pc.AppliedVersion,
        HasPendingUpdates = pc.Configuration != null && pc.Configuration.Version > pc.AppliedVersion,
        AppliedAt = pc.AppliedAt,
        AppliedBy = pc.AppliedBy,
        AutoUpdate = pc.AutoUpdate
    };
}

// ========== Artifact Custom Field Value Service ==========

public class ArtifactCustomFieldValueService : IArtifactCustomFieldValueService
{
    private readonly IArtifactCustomFieldValueRepository _valueRepository;
    private readonly ICustomFieldDefinitionRepository _fieldRepository;

    public ArtifactCustomFieldValueService(
        IArtifactCustomFieldValueRepository valueRepository,
        ICustomFieldDefinitionRepository fieldRepository)
    {
        _valueRepository = valueRepository;
        _fieldRepository = fieldRepository;
    }

    public async Task<IEnumerable<ArtifactCustomFieldValueDto>> GetFieldValuesByArtifactAsync(Guid artifactId)
    {
        var values = await _valueRepository.GetByArtifactIdAsync(artifactId);
        return values.Select(MapToDto);
    }

    public async Task<ArtifactCustomFieldValueDto?> GetFieldValueByIdAsync(Guid id)
    {
        var value = await _valueRepository.GetByIdAsync(id);
        return value == null ? null : MapToDto(value);
    }

    public async Task<ArtifactCustomFieldValueDto> SetFieldValueAsync(SetArtifactCustomFieldValueDto dto)
    {
        var field = await _fieldRepository.GetByIdAsync(dto.CustomFieldDefinitionId);
        if (field == null) throw new ArgumentException("Campo personalizado no encontrado");

        var existing = await _valueRepository.GetByArtifactAndFieldAsync(dto.ArtifactId, dto.CustomFieldDefinitionId);
        if (existing != null)
        {
            existing.Value = dto.Value;
            var updated = await _valueRepository.UpdateAsync(existing);
            return MapToDto(updated);
        }

        var value = new ArtifactCustomFieldValue
        {
            Id = Guid.NewGuid(),
            ArtifactId = dto.ArtifactId,
            CustomFieldDefinitionId = dto.CustomFieldDefinitionId,
            Value = dto.Value
        };

        var created = await _valueRepository.CreateAsync(value);
        created.CustomFieldDefinition = field;
        return MapToDto(created);
    }

    public async Task<IEnumerable<ArtifactCustomFieldValueDto>> BulkSetFieldValuesAsync(BulkSetCustomFieldValuesDto dto)
    {
        var results = new List<ArtifactCustomFieldValueDto>();

        foreach (var item in dto.Values)
        {
            var result = await SetFieldValueAsync(new SetArtifactCustomFieldValueDto
            {
                ArtifactId = dto.ArtifactId,
                CustomFieldDefinitionId = item.CustomFieldDefinitionId,
                Value = item.Value
            });
            results.Add(result);
        }

        return results;
    }

    public async Task<bool> DeleteFieldValueAsync(Guid id)
    {
        var value = await _valueRepository.GetByIdAsync(id);
        if (value == null) return false;

        await _valueRepository.DeleteAsync(id);
        return true;
    }

    private ArtifactCustomFieldValueDto MapToDto(ArtifactCustomFieldValue v) => new()
    {
        Id = v.Id,
        ArtifactId = v.ArtifactId,
        CustomFieldDefinitionId = v.CustomFieldDefinitionId,
        FieldName = v.CustomFieldDefinition?.FieldName ?? string.Empty,
        DisplayName = v.CustomFieldDefinition?.DisplayName ?? string.Empty,
        FieldType = v.CustomFieldDefinition?.FieldType ?? string.Empty,
        Value = v.Value,
        CreatedAt = v.CreatedAt,
        UpdatedAt = v.UpdatedAt
    };
}
