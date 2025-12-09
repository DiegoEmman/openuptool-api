using OpenUpTool.Core.DTOs;
using OpenUpTool.Core.Entities;
using OpenUpTool.Core.Interfaces;
using System.Text.Json;

namespace OpenUpTool.Core.Services;

// ========== HU-018: Global Configuration Services ==========

public class GlobalConfigurationService : IGlobalConfigurationService
{
    private readonly IGlobalConfigurationRepository _configurationRepository;
    private readonly IConfigurationChangeHistoryRepository _historyRepository;

    public GlobalConfigurationService(
        IGlobalConfigurationRepository configurationRepository,
        IConfigurationChangeHistoryRepository historyRepository)
    {
        _configurationRepository = configurationRepository;
        _historyRepository = historyRepository;
    }

    public async Task<IEnumerable<GlobalConfigurationDto>> GetAllConfigurationsAsync()
    {
        var configs = await _configurationRepository.GetAllAsync();
        return configs.Select(MapToDto);
    }

    public async Task<GlobalConfigurationDto?> GetConfigurationByIdAsync(Guid id)
    {
        var config = await _configurationRepository.GetByIdAsync(id);
        return config == null ? null : MapToDto(config);
    }

    public async Task<GlobalConfigurationDetailDto?> GetConfigurationDetailsAsync(Guid id)
    {
        var config = await _configurationRepository.GetByIdWithDetailsAsync(id);
        return config == null ? null : MapToDetailDto(config);
    }

    public async Task<GlobalConfigurationDetailDto?> GetDefaultConfigurationAsync()
    {
        var config = await _configurationRepository.GetDefaultAsync();
        return config == null ? null : MapToDetailDto(config);
    }

    public async Task<GlobalConfigurationDto> CreateConfigurationAsync(CreateGlobalConfigurationDto dto, string createdBy)
    {
        var config = new GlobalConfiguration
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            Version = 1,
            IsActive = true,
            IsDefault = dto.IsDefault,
            CreatedBy = createdBy
        };

        // Si se marca como default, quitar el flag de otros
        if (dto.IsDefault)
        {
            var currentDefault = await _configurationRepository.GetDefaultAsync();
            if (currentDefault != null)
            {
                currentDefault.IsDefault = false;
                await _configurationRepository.UpdateAsync(currentDefault);
            }
        }

        // Copiar templates del default si se solicita
        if (dto.CopyFromDefault)
        {
            var defaultConfig = await _configurationRepository.GetDefaultAsync();
            if (defaultConfig != null)
            {
                CopyTemplatesFromConfiguration(config, defaultConfig);
            }
            else
            {
                // Crear templates OpenUP por defecto
                CreateDefaultOpenUpTemplates(config);
            }
        }

        var created = await _configurationRepository.CreateAsync(config);

        // Registrar cambio
        await _historyRepository.CreateAsync(new ConfigurationChangeHistory
        {
            Id = Guid.NewGuid(),
            ConfigurationId = created.Id,
            FromVersion = 0,
            ToVersion = 1,
            ChangeType = "CREATE",
            EntityType = "CONFIGURATION",
            EntityId = created.Id,
            EntityName = created.Name,
            NewValue = JsonSerializer.Serialize(new { created.Name, created.Description }),
            ChangedBy = createdBy,
            ChangeDescription = "Configuración creada"
        });

        return MapToDto(created);
    }

    public async Task<GlobalConfigurationDto?> UpdateConfigurationAsync(Guid id, UpdateGlobalConfigurationDto dto)
    {
        var config = await _configurationRepository.GetByIdAsync(id);
        if (config == null) return null;

        config.Name = dto.Name;
        config.Description = dto.Description;
        config.IsActive = dto.IsActive;

        if (dto.IsDefault && !config.IsDefault)
        {
            var currentDefault = await _configurationRepository.GetDefaultAsync();
            if (currentDefault != null && currentDefault.Id != id)
            {
                currentDefault.IsDefault = false;
                await _configurationRepository.UpdateAsync(currentDefault);
            }
        }
        config.IsDefault = dto.IsDefault;

        var updated = await _configurationRepository.UpdateAsync(config);
        return MapToDto(updated);
    }

    public async Task<bool> DeleteConfigurationAsync(Guid id)
    {
        var config = await _configurationRepository.GetByIdAsync(id);
        if (config == null) return false;
        if (config.IsDefault) throw new InvalidOperationException("No se puede eliminar la configuración por defecto");

        await _configurationRepository.DeleteAsync(id);
        return true;
    }

    public async Task<GlobalConfigurationDto?> IncrementVersionAsync(Guid id, string changedBy, string changeDescription)
    {
        var config = await _configurationRepository.GetByIdAsync(id);
        if (config == null) return null;

        var oldVersion = config.Version;
        config.Version++;

        await _historyRepository.CreateAsync(new ConfigurationChangeHistory
        {
            Id = Guid.NewGuid(),
            ConfigurationId = id,
            FromVersion = oldVersion,
            ToVersion = config.Version,
            ChangeType = "VERSION_INCREMENT",
            EntityType = "CONFIGURATION",
            ChangedBy = changedBy,
            ChangeDescription = changeDescription
        });

        var updated = await _configurationRepository.UpdateAsync(config);
        return MapToDto(updated);
    }

    public async Task<GlobalConfigurationDetailDto?> RollbackToVersionAsync(Guid id, int targetVersion, string changedBy, string? reason)
    {
        var config = await _configurationRepository.GetByIdWithDetailsAsync(id);
        if (config == null) return null;

        var history = await _historyRepository.GetByConfigurationIdAndVersionAsync(id, targetVersion);
        if (!history.Any()) throw new InvalidOperationException($"No se encontró historial para la versión {targetVersion}");

        var oldVersion = config.Version;
        config.Version++;

        await _historyRepository.CreateAsync(new ConfigurationChangeHistory
        {
            Id = Guid.NewGuid(),
            ConfigurationId = id,
            FromVersion = oldVersion,
            ToVersion = config.Version,
            ChangeType = "ROLLBACK",
            EntityType = "CONFIGURATION",
            ChangedBy = changedBy,
            ChangeDescription = $"Rollback a versión {targetVersion}. Razón: {reason ?? "No especificada"}"
        });

        var updated = await _configurationRepository.UpdateAsync(config);
        return MapToDetailDto(updated);
    }

    public async Task<IEnumerable<ConfigurationChangeHistoryDto>> GetChangeHistoryAsync(Guid configurationId)
    {
        var history = await _historyRepository.GetByConfigurationIdAsync(configurationId);
        return history.Select(MapHistoryToDto);
    }

    public async Task<IEnumerable<ConfigurationChangeHistoryDto>> GetChangesByVersionAsync(Guid configurationId, int version)
    {
        var history = await _historyRepository.GetByConfigurationIdAndVersionAsync(configurationId, version);
        return history.Select(MapHistoryToDto);
    }

    private void CopyTemplatesFromConfiguration(GlobalConfiguration target, GlobalConfiguration source)
    {
        foreach (var role in source.RoleTemplates)
        {
            target.RoleTemplates.Add(new RoleTemplate
            {
                Id = Guid.NewGuid(),
                Name = role.Name,
                Description = role.Description,
                Permissions = role.Permissions,
                OrderIndex = role.OrderIndex,
                IsSystem = role.IsSystem
            });
        }

        foreach (var phase in source.PhaseTemplates)
        {
            target.PhaseTemplates.Add(new PhaseTemplate
            {
                Id = Guid.NewGuid(),
                PhaseCode = phase.PhaseCode,
                Name = phase.Name,
                Description = phase.Description,
                OrderIndex = phase.OrderIndex,
                DefaultDurationDays = phase.DefaultDurationDays,
                IsMandatory = phase.IsMandatory
            });
        }

        foreach (var artifactType in source.ArtifactTypeTemplates)
        {
            var newArtifactType = new ArtifactTypeTemplate
            {
                Id = Guid.NewGuid(),
                PhaseCode = artifactType.PhaseCode,
                Code = artifactType.Code,
                Name = artifactType.Name,
                Description = artifactType.Description,
                IsMandatory = artifactType.IsMandatory,
                DefaultFormat = artifactType.DefaultFormat,
                OrderIndex = artifactType.OrderIndex
            };

            foreach (var field in artifactType.CustomFields)
            {
                newArtifactType.CustomFields.Add(new CustomFieldDefinition
                {
                    Id = Guid.NewGuid(),
                    FieldName = field.FieldName,
                    DisplayName = field.DisplayName,
                    FieldType = field.FieldType,
                    IsRequired = field.IsRequired,
                    DefaultValue = field.DefaultValue,
                    Options = field.Options,
                    ValidationRules = field.ValidationRules,
                    OrderIndex = field.OrderIndex
                });
            }

            target.ArtifactTypeTemplates.Add(newArtifactType);
        }

        foreach (var workflow in source.WorkflowTemplates)
        {
            var newWorkflow = new WorkflowTemplate
            {
                Id = Guid.NewGuid(),
                Name = workflow.Name,
                Description = workflow.Description,
                IsDefault = workflow.IsDefault,
                OrderIndex = workflow.OrderIndex
            };

            foreach (var state in workflow.States)
            {
                newWorkflow.States.Add(new WorkflowStateTemplate
                {
                    Id = Guid.NewGuid(),
                    Name = state.Name,
                    Description = state.Description,
                    OrderIndex = state.OrderIndex,
                    Color = state.Color,
                    IsInitialState = state.IsInitialState,
                    IsFinalState = state.IsFinalState
                });
            }

            target.WorkflowTemplates.Add(newWorkflow);
        }
    }

    private void CreateDefaultOpenUpTemplates(GlobalConfiguration config)
    {
        // Roles OpenUP
        config.RoleTemplates.Add(new RoleTemplate { Id = Guid.NewGuid(), Name = "Admin", Description = "Administrador del sistema", OrderIndex = 1, IsSystem = true });
        config.RoleTemplates.Add(new RoleTemplate { Id = Guid.NewGuid(), Name = "Manager", Description = "Gestor de proyecto", OrderIndex = 2, IsSystem = true });
        config.RoleTemplates.Add(new RoleTemplate { Id = Guid.NewGuid(), Name = "Developer", Description = "Desarrollador", OrderIndex = 3, IsSystem = true });
        config.RoleTemplates.Add(new RoleTemplate { Id = Guid.NewGuid(), Name = "Viewer", Description = "Solo lectura", OrderIndex = 4, IsSystem = true });

        // Fases OpenUP
        config.PhaseTemplates.Add(new PhaseTemplate { Id = Guid.NewGuid(), PhaseCode = "INCEPTION", Name = "Inception", OrderIndex = 1, IsMandatory = true });
        config.PhaseTemplates.Add(new PhaseTemplate { Id = Guid.NewGuid(), PhaseCode = "ELABORATION", Name = "Elaboration", OrderIndex = 2, IsMandatory = true });
        config.PhaseTemplates.Add(new PhaseTemplate { Id = Guid.NewGuid(), PhaseCode = "CONSTRUCTION", Name = "Construction", OrderIndex = 3, IsMandatory = true });
        config.PhaseTemplates.Add(new PhaseTemplate { Id = Guid.NewGuid(), PhaseCode = "TRANSITION", Name = "Transition", OrderIndex = 4, IsMandatory = true });

        // Workflow por defecto
        var workflow = new WorkflowTemplate { Id = Guid.NewGuid(), Name = "Default Workflow", IsDefault = true, OrderIndex = 1 };
        workflow.States.Add(new WorkflowStateTemplate { Id = Guid.NewGuid(), Name = "Pendiente", OrderIndex = 1, Color = "#6B7280", IsInitialState = true });
        workflow.States.Add(new WorkflowStateTemplate { Id = Guid.NewGuid(), Name = "En Progreso", OrderIndex = 2, Color = "#3B82F6" });
        workflow.States.Add(new WorkflowStateTemplate { Id = Guid.NewGuid(), Name = "En Revisión", OrderIndex = 3, Color = "#F59E0B" });
        workflow.States.Add(new WorkflowStateTemplate { Id = Guid.NewGuid(), Name = "Completado", OrderIndex = 4, Color = "#10B981", IsFinalState = true });
        config.WorkflowTemplates.Add(workflow);
    }

    private GlobalConfigurationDto MapToDto(GlobalConfiguration config) => new()
    {
        Id = config.Id,
        Name = config.Name,
        Description = config.Description,
        Version = config.Version,
        IsActive = config.IsActive,
        IsDefault = config.IsDefault,
        Tags = config.Tags,
        ParentTemplateId = config.ParentTemplateId,
        CreatedBy = config.CreatedBy,
        CreatedAt = config.CreatedAt,
        UpdatedAt = config.UpdatedAt,
        RoleTemplatesCount = config.RoleTemplates.Count,
        PhaseTemplatesCount = config.PhaseTemplates.Count,
        ArtifactTypeTemplatesCount = config.ArtifactTypeTemplates.Count,
        WorkflowTemplatesCount = config.WorkflowTemplates.Count
    };

    private GlobalConfigurationDetailDto MapToDetailDto(GlobalConfiguration config) => new()
    {
        Id = config.Id,
        Name = config.Name,
        Description = config.Description,
        Version = config.Version,
        IsActive = config.IsActive,
        IsDefault = config.IsDefault,
        Tags = config.Tags,
        ParentTemplateId = config.ParentTemplateId,
        CreatedBy = config.CreatedBy,
        CreatedAt = config.CreatedAt,
        UpdatedAt = config.UpdatedAt,
        RoleTemplatesCount = config.RoleTemplates.Count,
        PhaseTemplatesCount = config.PhaseTemplates.Count,
        ArtifactTypeTemplatesCount = config.ArtifactTypeTemplates.Count,
        WorkflowTemplatesCount = config.WorkflowTemplates.Count,
        RoleTemplates = config.RoleTemplates.Select(r => new RoleTemplateDto
        {
            Id = r.Id,
            ConfigurationId = r.ConfigurationId,
            Name = r.Name,
            Description = r.Description,
            Permissions = string.IsNullOrEmpty(r.Permissions) ? null : JsonSerializer.Deserialize<List<string>>(r.Permissions),
            OrderIndex = r.OrderIndex,
            IsSystem = r.IsSystem,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt
        }).ToList(),
        PhaseTemplates = config.PhaseTemplates.Select(p => new PhaseTemplateDto
        {
            Id = p.Id,
            ConfigurationId = p.ConfigurationId,
            PhaseCode = p.PhaseCode,
            Name = p.Name,
            Description = p.Description,
            OrderIndex = p.OrderIndex,
            DefaultDurationDays = p.DefaultDurationDays,
            IsMandatory = p.IsMandatory,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt
        }).ToList(),
        ArtifactTypeTemplates = config.ArtifactTypeTemplates.Select(a => new ArtifactTypeTemplateDto
        {
            Id = a.Id,
            ConfigurationId = a.ConfigurationId,
            PhaseCode = a.PhaseCode,
            Code = a.Code,
            Name = a.Name,
            Description = a.Description,
            IsMandatory = a.IsMandatory,
            DefaultFormat = a.DefaultFormat,
            OrderIndex = a.OrderIndex,
            CreatedAt = a.CreatedAt,
            UpdatedAt = a.UpdatedAt,
            CustomFields = a.CustomFields.Select(f => new CustomFieldDefinitionDto
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
            }).ToList()
        }).ToList(),
        WorkflowTemplates = config.WorkflowTemplates.Select(w => new WorkflowTemplateDto
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
        }).ToList()
    };

    private ConfigurationChangeHistoryDto MapHistoryToDto(ConfigurationChangeHistory h) => new()
    {
        Id = h.Id,
        ConfigurationId = h.ConfigurationId,
        FromVersion = h.FromVersion,
        ToVersion = h.ToVersion,
        ChangeType = h.ChangeType,
        EntityType = h.EntityType,
        EntityId = h.EntityId,
        EntityName = h.EntityName,
        OldValue = h.OldValue,
        NewValue = h.NewValue,
        ChangedBy = h.ChangedBy,
        ChangeDescription = h.ChangeDescription,
        ChangedAt = h.ChangedAt
    };
}
