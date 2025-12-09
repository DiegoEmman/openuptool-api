using OpenUpTool.Core.DTOs;
using OpenUpTool.Core.Entities;
using OpenUpTool.Core.Interfaces;
using System.Text.Json;

namespace OpenUpTool.Core.Services;

// ========== Role Template Service ==========

public class RoleTemplateService : IRoleTemplateService
{
    private readonly IRoleTemplateRepository _roleRepository;
    private readonly IGlobalConfigurationRepository _configRepository;
    private readonly IConfigurationChangeHistoryRepository _historyRepository;

    public RoleTemplateService(
        IRoleTemplateRepository roleRepository,
        IGlobalConfigurationRepository configRepository,
        IConfigurationChangeHistoryRepository historyRepository)
    {
        _roleRepository = roleRepository;
        _configRepository = configRepository;
        _historyRepository = historyRepository;
    }

    public async Task<IEnumerable<RoleTemplateDto>> GetRolesByConfigurationAsync(Guid configurationId)
    {
        var roles = await _roleRepository.GetByConfigurationIdAsync(configurationId);
        return roles.Select(MapToDto);
    }

    public async Task<RoleTemplateDto?> GetRoleByIdAsync(Guid id)
    {
        var role = await _roleRepository.GetByIdAsync(id);
        return role == null ? null : MapToDto(role);
    }

    public async Task<RoleTemplateDto> CreateRoleAsync(CreateRoleTemplateDto dto, string changedBy)
    {
        var config = await _configRepository.GetByIdAsync(dto.ConfigurationId);
        if (config == null) throw new ArgumentException("Configuración no encontrada");

        var role = new RoleTemplate
        {
            Id = Guid.NewGuid(),
            ConfigurationId = dto.ConfigurationId,
            Name = dto.Name,
            Description = dto.Description,
            Permissions = dto.Permissions != null ? JsonSerializer.Serialize(dto.Permissions) : null,
            OrderIndex = dto.OrderIndex,
            IsSystem = false
        };

        var created = await _roleRepository.CreateAsync(role);

        await _historyRepository.CreateAsync(new ConfigurationChangeHistory
        {
            Id = Guid.NewGuid(),
            ConfigurationId = dto.ConfigurationId,
            FromVersion = config.Version,
            ToVersion = config.Version,
            ChangeType = "CREATE",
            EntityType = "ROLE",
            EntityId = created.Id,
            EntityName = created.Name,
            NewValue = JsonSerializer.Serialize(dto),
            ChangedBy = changedBy,
            ChangeDescription = $"Rol '{created.Name}' creado"
        });

        return MapToDto(created);
    }

    public async Task<RoleTemplateDto?> UpdateRoleAsync(Guid id, UpdateRoleTemplateDto dto, string changedBy)
    {
        var role = await _roleRepository.GetByIdAsync(id);
        if (role == null) return null;

        if (role.IsSystem) throw new InvalidOperationException("No se pueden modificar roles del sistema");

        var oldValue = JsonSerializer.Serialize(MapToDto(role));

        role.Name = dto.Name;
        role.Description = dto.Description;
        role.Permissions = dto.Permissions != null ? JsonSerializer.Serialize(dto.Permissions) : null;
        role.OrderIndex = dto.OrderIndex;

        var updated = await _roleRepository.UpdateAsync(role);

        await _historyRepository.CreateAsync(new ConfigurationChangeHistory
        {
            Id = Guid.NewGuid(),
            ConfigurationId = role.ConfigurationId,
            FromVersion = 0,
            ToVersion = 0,
            ChangeType = "UPDATE",
            EntityType = "ROLE",
            EntityId = role.Id,
            EntityName = role.Name,
            OldValue = oldValue,
            NewValue = JsonSerializer.Serialize(dto),
            ChangedBy = changedBy,
            ChangeDescription = $"Rol '{role.Name}' actualizado"
        });

        return MapToDto(updated);
    }

    public async Task<bool> DeleteRoleAsync(Guid id, string changedBy)
    {
        var role = await _roleRepository.GetByIdAsync(id);
        if (role == null) return false;

        if (role.IsSystem) throw new InvalidOperationException("No se pueden eliminar roles del sistema");

        await _historyRepository.CreateAsync(new ConfigurationChangeHistory
        {
            Id = Guid.NewGuid(),
            ConfigurationId = role.ConfigurationId,
            FromVersion = 0,
            ToVersion = 0,
            ChangeType = "DELETE",
            EntityType = "ROLE",
            EntityId = role.Id,
            EntityName = role.Name,
            OldValue = JsonSerializer.Serialize(MapToDto(role)),
            ChangedBy = changedBy,
            ChangeDescription = $"Rol '{role.Name}' eliminado"
        });

        await _roleRepository.DeleteAsync(id);
        return true;
    }

    private RoleTemplateDto MapToDto(RoleTemplate r) => new()
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
    };
}

// ========== Phase Template Service ==========

public class PhaseTemplateService : IPhaseTemplateService
{
    private readonly IPhaseTemplateRepository _phaseRepository;
    private readonly IGlobalConfigurationRepository _configRepository;
    private readonly IConfigurationChangeHistoryRepository _historyRepository;

    public PhaseTemplateService(
        IPhaseTemplateRepository phaseRepository,
        IGlobalConfigurationRepository configRepository,
        IConfigurationChangeHistoryRepository historyRepository)
    {
        _phaseRepository = phaseRepository;
        _configRepository = configRepository;
        _historyRepository = historyRepository;
    }

    public async Task<IEnumerable<PhaseTemplateDto>> GetPhasesByConfigurationAsync(Guid configurationId)
    {
        var phases = await _phaseRepository.GetByConfigurationIdAsync(configurationId);
        return phases.Select(MapToDto);
    }

    public async Task<PhaseTemplateDto?> GetPhaseByIdAsync(Guid id)
    {
        var phase = await _phaseRepository.GetByIdAsync(id);
        return phase == null ? null : MapToDto(phase);
    }

    public async Task<PhaseTemplateDto> CreatePhaseAsync(CreatePhaseTemplateDto dto, string changedBy)
    {
        var config = await _configRepository.GetByIdAsync(dto.ConfigurationId);
        if (config == null) throw new ArgumentException("Configuración no encontrada");

        var phase = new PhaseTemplate
        {
            Id = Guid.NewGuid(),
            ConfigurationId = dto.ConfigurationId,
            PhaseCode = dto.PhaseCode.ToUpper(),
            Name = dto.Name,
            Description = dto.Description,
            OrderIndex = dto.OrderIndex,
            DefaultDurationDays = dto.DefaultDurationDays,
            IsMandatory = dto.IsMandatory
        };

        var created = await _phaseRepository.CreateAsync(phase);

        await _historyRepository.CreateAsync(new ConfigurationChangeHistory
        {
            Id = Guid.NewGuid(),
            ConfigurationId = dto.ConfigurationId,
            FromVersion = config.Version,
            ToVersion = config.Version,
            ChangeType = "CREATE",
            EntityType = "PHASE",
            EntityId = created.Id,
            EntityName = created.Name,
            NewValue = JsonSerializer.Serialize(dto),
            ChangedBy = changedBy,
            ChangeDescription = $"Fase '{created.Name}' creada"
        });

        return MapToDto(created);
    }

    public async Task<PhaseTemplateDto?> UpdatePhaseAsync(Guid id, UpdatePhaseTemplateDto dto, string changedBy)
    {
        var phase = await _phaseRepository.GetByIdAsync(id);
        if (phase == null) return null;

        var oldValue = JsonSerializer.Serialize(MapToDto(phase));

        phase.PhaseCode = dto.PhaseCode.ToUpper();
        phase.Name = dto.Name;
        phase.Description = dto.Description;
        phase.OrderIndex = dto.OrderIndex;
        phase.DefaultDurationDays = dto.DefaultDurationDays;
        phase.IsMandatory = dto.IsMandatory;

        var updated = await _phaseRepository.UpdateAsync(phase);

        await _historyRepository.CreateAsync(new ConfigurationChangeHistory
        {
            Id = Guid.NewGuid(),
            ConfigurationId = phase.ConfigurationId,
            FromVersion = 0,
            ToVersion = 0,
            ChangeType = "UPDATE",
            EntityType = "PHASE",
            EntityId = phase.Id,
            EntityName = phase.Name,
            OldValue = oldValue,
            NewValue = JsonSerializer.Serialize(dto),
            ChangedBy = changedBy,
            ChangeDescription = $"Fase '{phase.Name}' actualizada"
        });

        return MapToDto(updated);
    }

    public async Task<bool> DeletePhaseAsync(Guid id, string changedBy)
    {
        var phase = await _phaseRepository.GetByIdAsync(id);
        if (phase == null) return false;

        await _historyRepository.CreateAsync(new ConfigurationChangeHistory
        {
            Id = Guid.NewGuid(),
            ConfigurationId = phase.ConfigurationId,
            FromVersion = 0,
            ToVersion = 0,
            ChangeType = "DELETE",
            EntityType = "PHASE",
            EntityId = phase.Id,
            EntityName = phase.Name,
            OldValue = JsonSerializer.Serialize(MapToDto(phase)),
            ChangedBy = changedBy,
            ChangeDescription = $"Fase '{phase.Name}' eliminada"
        });

        await _phaseRepository.DeleteAsync(id);
        return true;
    }

    private PhaseTemplateDto MapToDto(PhaseTemplate p) => new()
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
    };
}

// ========== Artifact Type Template Service ==========

public class ArtifactTypeTemplateService : IArtifactTypeTemplateService
{
    private readonly IArtifactTypeTemplateRepository _artifactTypeRepository;
    private readonly IGlobalConfigurationRepository _configRepository;
    private readonly IConfigurationChangeHistoryRepository _historyRepository;

    public ArtifactTypeTemplateService(
        IArtifactTypeTemplateRepository artifactTypeRepository,
        IGlobalConfigurationRepository configRepository,
        IConfigurationChangeHistoryRepository historyRepository)
    {
        _artifactTypeRepository = artifactTypeRepository;
        _configRepository = configRepository;
        _historyRepository = historyRepository;
    }

    public async Task<IEnumerable<ArtifactTypeTemplateDto>> GetArtifactTypesByConfigurationAsync(Guid configurationId)
    {
        var types = await _artifactTypeRepository.GetByConfigurationIdAsync(configurationId);
        return types.Select(MapToDto);
    }

    public async Task<IEnumerable<ArtifactTypeTemplateDto>> GetArtifactTypesByPhaseAsync(Guid configurationId, string phaseCode)
    {
        var types = await _artifactTypeRepository.GetByPhaseCodeAsync(configurationId, phaseCode.ToUpper());
        return types.Select(MapToDto);
    }

    public async Task<ArtifactTypeTemplateDto?> GetArtifactTypeByIdAsync(Guid id)
    {
        var type = await _artifactTypeRepository.GetByIdWithCustomFieldsAsync(id);
        return type == null ? null : MapToDto(type);
    }

    public async Task<ArtifactTypeTemplateDto> CreateArtifactTypeAsync(CreateArtifactTypeTemplateDto dto, string changedBy)
    {
        var config = await _configRepository.GetByIdAsync(dto.ConfigurationId);
        if (config == null) throw new ArgumentException("Configuración no encontrada");

        var artifactType = new ArtifactTypeTemplate
        {
            Id = Guid.NewGuid(),
            ConfigurationId = dto.ConfigurationId,
            PhaseCode = dto.PhaseCode.ToUpper(),
            Code = dto.Code.ToUpper(),
            Name = dto.Name,
            Description = dto.Description,
            IsMandatory = dto.IsMandatory,
            DefaultFormat = dto.DefaultFormat,
            OrderIndex = dto.OrderIndex
        };

        var created = await _artifactTypeRepository.CreateAsync(artifactType);

        await _historyRepository.CreateAsync(new ConfigurationChangeHistory
        {
            Id = Guid.NewGuid(),
            ConfigurationId = dto.ConfigurationId,
            FromVersion = config.Version,
            ToVersion = config.Version,
            ChangeType = "CREATE",
            EntityType = "ARTIFACT_TYPE",
            EntityId = created.Id,
            EntityName = created.Name,
            NewValue = JsonSerializer.Serialize(dto),
            ChangedBy = changedBy,
            ChangeDescription = $"Tipo de artefacto '{created.Name}' creado"
        });

        return MapToDto(created);
    }

    public async Task<ArtifactTypeTemplateDto?> UpdateArtifactTypeAsync(Guid id, UpdateArtifactTypeTemplateDto dto, string changedBy)
    {
        var artifactType = await _artifactTypeRepository.GetByIdAsync(id);
        if (artifactType == null) return null;

        var oldValue = JsonSerializer.Serialize(MapToDto(artifactType));

        artifactType.PhaseCode = dto.PhaseCode.ToUpper();
        artifactType.Code = dto.Code.ToUpper();
        artifactType.Name = dto.Name;
        artifactType.Description = dto.Description;
        artifactType.IsMandatory = dto.IsMandatory;
        artifactType.DefaultFormat = dto.DefaultFormat;
        artifactType.OrderIndex = dto.OrderIndex;

        var updated = await _artifactTypeRepository.UpdateAsync(artifactType);

        await _historyRepository.CreateAsync(new ConfigurationChangeHistory
        {
            Id = Guid.NewGuid(),
            ConfigurationId = artifactType.ConfigurationId,
            FromVersion = 0,
            ToVersion = 0,
            ChangeType = "UPDATE",
            EntityType = "ARTIFACT_TYPE",
            EntityId = artifactType.Id,
            EntityName = artifactType.Name,
            OldValue = oldValue,
            NewValue = JsonSerializer.Serialize(dto),
            ChangedBy = changedBy,
            ChangeDescription = $"Tipo de artefacto '{artifactType.Name}' actualizado"
        });

        return MapToDto(updated);
    }

    public async Task<bool> DeleteArtifactTypeAsync(Guid id, string changedBy)
    {
        var artifactType = await _artifactTypeRepository.GetByIdAsync(id);
        if (artifactType == null) return false;

        await _historyRepository.CreateAsync(new ConfigurationChangeHistory
        {
            Id = Guid.NewGuid(),
            ConfigurationId = artifactType.ConfigurationId,
            FromVersion = 0,
            ToVersion = 0,
            ChangeType = "DELETE",
            EntityType = "ARTIFACT_TYPE",
            EntityId = artifactType.Id,
            EntityName = artifactType.Name,
            OldValue = JsonSerializer.Serialize(MapToDto(artifactType)),
            ChangedBy = changedBy,
            ChangeDescription = $"Tipo de artefacto '{artifactType.Name}' eliminado"
        });

        await _artifactTypeRepository.DeleteAsync(id);
        return true;
    }

    private ArtifactTypeTemplateDto MapToDto(ArtifactTypeTemplate a) => new()
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
    };
}
