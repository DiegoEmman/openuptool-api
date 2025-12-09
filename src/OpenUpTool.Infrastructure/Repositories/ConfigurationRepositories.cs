using Microsoft.EntityFrameworkCore;
using OpenUpTool.Core.Entities;
using OpenUpTool.Core.Interfaces;
using OpenUpTool.Infrastructure.Data;

namespace OpenUpTool.Infrastructure.Repositories;

// ========== HU-018: Global Configuration Repositories ==========

public class GlobalConfigurationRepository : IGlobalConfigurationRepository
{
    private readonly OpenUpToolDbContext _context;

    public GlobalConfigurationRepository(OpenUpToolDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<GlobalConfiguration>> GetAllAsync()
    {
        return await _context.GlobalConfigurations
            .Include(c => c.RoleTemplates)
            .Include(c => c.PhaseTemplates)
            .Include(c => c.ArtifactTypeTemplates)
            .Include(c => c.WorkflowTemplates)
            .OrderByDescending(c => c.IsDefault)
            .ThenBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<GlobalConfiguration?> GetByIdAsync(Guid id)
    {
        return await _context.GlobalConfigurations.FindAsync(id);
    }

    public async Task<GlobalConfiguration?> GetByIdWithDetailsAsync(Guid id)
    {
        return await _context.GlobalConfigurations
            .Include(c => c.RoleTemplates.OrderBy(r => r.OrderIndex))
            .Include(c => c.PhaseTemplates.OrderBy(p => p.OrderIndex))
            .Include(c => c.ArtifactTypeTemplates.OrderBy(a => a.OrderIndex))
                .ThenInclude(a => a.CustomFields.OrderBy(f => f.OrderIndex))
            .Include(c => c.WorkflowTemplates.OrderBy(w => w.OrderIndex))
                .ThenInclude(w => w.States.OrderBy(s => s.OrderIndex))
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<GlobalConfiguration?> GetDefaultAsync()
    {
        return await _context.GlobalConfigurations
            .Include(c => c.RoleTemplates.OrderBy(r => r.OrderIndex))
            .Include(c => c.PhaseTemplates.OrderBy(p => p.OrderIndex))
            .Include(c => c.ArtifactTypeTemplates.OrderBy(a => a.OrderIndex))
                .ThenInclude(a => a.CustomFields.OrderBy(f => f.OrderIndex))
            .Include(c => c.WorkflowTemplates.OrderBy(w => w.OrderIndex))
                .ThenInclude(w => w.States.OrderBy(s => s.OrderIndex))
            .FirstOrDefaultAsync(c => c.IsDefault && c.IsActive);
    }

    public async Task<GlobalConfiguration> CreateAsync(GlobalConfiguration configuration)
    {
        configuration.CreatedAt = DateTime.UtcNow;
        configuration.UpdatedAt = DateTime.UtcNow;
        _context.GlobalConfigurations.Add(configuration);
        await _context.SaveChangesAsync();
        return configuration;
    }

    public async Task<GlobalConfiguration> UpdateAsync(GlobalConfiguration configuration)
    {
        configuration.UpdatedAt = DateTime.UtcNow;
        _context.GlobalConfigurations.Update(configuration);
        await _context.SaveChangesAsync();
        return configuration;
    }

    public async Task DeleteAsync(Guid id)
    {
        var configuration = await _context.GlobalConfigurations.FindAsync(id);
        if (configuration != null)
        {
            _context.GlobalConfigurations.Remove(configuration);
            await _context.SaveChangesAsync();
        }
    }
}

public class RoleTemplateRepository : IRoleTemplateRepository
{
    private readonly OpenUpToolDbContext _context;

    public RoleTemplateRepository(OpenUpToolDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<RoleTemplate>> GetByConfigurationIdAsync(Guid configurationId)
    {
        return await _context.RoleTemplates
            .Where(r => r.ConfigurationId == configurationId)
            .OrderBy(r => r.OrderIndex)
            .ToListAsync();
    }

    public async Task<RoleTemplate?> GetByIdAsync(Guid id)
    {
        return await _context.RoleTemplates.FindAsync(id);
    }

    public async Task<RoleTemplate> CreateAsync(RoleTemplate roleTemplate)
    {
        roleTemplate.CreatedAt = DateTime.UtcNow;
        roleTemplate.UpdatedAt = DateTime.UtcNow;
        _context.RoleTemplates.Add(roleTemplate);
        await _context.SaveChangesAsync();
        return roleTemplate;
    }

    public async Task<RoleTemplate> UpdateAsync(RoleTemplate roleTemplate)
    {
        roleTemplate.UpdatedAt = DateTime.UtcNow;
        _context.RoleTemplates.Update(roleTemplate);
        await _context.SaveChangesAsync();
        return roleTemplate;
    }

    public async Task DeleteAsync(Guid id)
    {
        var roleTemplate = await _context.RoleTemplates.FindAsync(id);
        if (roleTemplate != null)
        {
            _context.RoleTemplates.Remove(roleTemplate);
            await _context.SaveChangesAsync();
        }
    }
}

public class PhaseTemplateRepository : IPhaseTemplateRepository
{
    private readonly OpenUpToolDbContext _context;

    public PhaseTemplateRepository(OpenUpToolDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PhaseTemplate>> GetByConfigurationIdAsync(Guid configurationId)
    {
        return await _context.PhaseTemplates
            .Where(p => p.ConfigurationId == configurationId)
            .OrderBy(p => p.OrderIndex)
            .ToListAsync();
    }

    public async Task<PhaseTemplate?> GetByIdAsync(Guid id)
    {
        return await _context.PhaseTemplates.FindAsync(id);
    }

    public async Task<PhaseTemplate> CreateAsync(PhaseTemplate phaseTemplate)
    {
        phaseTemplate.CreatedAt = DateTime.UtcNow;
        phaseTemplate.UpdatedAt = DateTime.UtcNow;
        _context.PhaseTemplates.Add(phaseTemplate);
        await _context.SaveChangesAsync();
        return phaseTemplate;
    }

    public async Task<PhaseTemplate> UpdateAsync(PhaseTemplate phaseTemplate)
    {
        phaseTemplate.UpdatedAt = DateTime.UtcNow;
        _context.PhaseTemplates.Update(phaseTemplate);
        await _context.SaveChangesAsync();
        return phaseTemplate;
    }

    public async Task DeleteAsync(Guid id)
    {
        var phaseTemplate = await _context.PhaseTemplates.FindAsync(id);
        if (phaseTemplate != null)
        {
            _context.PhaseTemplates.Remove(phaseTemplate);
            await _context.SaveChangesAsync();
        }
    }
}

public class ArtifactTypeTemplateRepository : IArtifactTypeTemplateRepository
{
    private readonly OpenUpToolDbContext _context;

    public ArtifactTypeTemplateRepository(OpenUpToolDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ArtifactTypeTemplate>> GetByConfigurationIdAsync(Guid configurationId)
    {
        return await _context.ArtifactTypeTemplates
            .Include(a => a.CustomFields.OrderBy(f => f.OrderIndex))
            .Where(a => a.ConfigurationId == configurationId)
            .OrderBy(a => a.OrderIndex)
            .ToListAsync();
    }

    public async Task<IEnumerable<ArtifactTypeTemplate>> GetByPhaseCodeAsync(Guid configurationId, string phaseCode)
    {
        return await _context.ArtifactTypeTemplates
            .Include(a => a.CustomFields.OrderBy(f => f.OrderIndex))
            .Where(a => a.ConfigurationId == configurationId && a.PhaseCode == phaseCode)
            .OrderBy(a => a.OrderIndex)
            .ToListAsync();
    }

    public async Task<ArtifactTypeTemplate?> GetByIdAsync(Guid id)
    {
        return await _context.ArtifactTypeTemplates.FindAsync(id);
    }

    public async Task<ArtifactTypeTemplate?> GetByIdWithCustomFieldsAsync(Guid id)
    {
        return await _context.ArtifactTypeTemplates
            .Include(a => a.CustomFields.OrderBy(f => f.OrderIndex))
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<ArtifactTypeTemplate> CreateAsync(ArtifactTypeTemplate artifactTypeTemplate)
    {
        artifactTypeTemplate.CreatedAt = DateTime.UtcNow;
        artifactTypeTemplate.UpdatedAt = DateTime.UtcNow;
        _context.ArtifactTypeTemplates.Add(artifactTypeTemplate);
        await _context.SaveChangesAsync();
        return artifactTypeTemplate;
    }

    public async Task<ArtifactTypeTemplate> UpdateAsync(ArtifactTypeTemplate artifactTypeTemplate)
    {
        artifactTypeTemplate.UpdatedAt = DateTime.UtcNow;
        _context.ArtifactTypeTemplates.Update(artifactTypeTemplate);
        await _context.SaveChangesAsync();
        return artifactTypeTemplate;
    }

    public async Task DeleteAsync(Guid id)
    {
        var artifactTypeTemplate = await _context.ArtifactTypeTemplates.FindAsync(id);
        if (artifactTypeTemplate != null)
        {
            _context.ArtifactTypeTemplates.Remove(artifactTypeTemplate);
            await _context.SaveChangesAsync();
        }
    }
}

public class WorkflowTemplateRepository : IWorkflowTemplateRepository
{
    private readonly OpenUpToolDbContext _context;

    public WorkflowTemplateRepository(OpenUpToolDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<WorkflowTemplate>> GetByConfigurationIdAsync(Guid configurationId)
    {
        return await _context.WorkflowTemplates
            .Include(w => w.States.OrderBy(s => s.OrderIndex))
            .Where(w => w.ConfigurationId == configurationId)
            .OrderBy(w => w.OrderIndex)
            .ToListAsync();
    }

    public async Task<WorkflowTemplate?> GetByIdAsync(Guid id)
    {
        return await _context.WorkflowTemplates.FindAsync(id);
    }

    public async Task<WorkflowTemplate?> GetByIdWithStatesAsync(Guid id)
    {
        return await _context.WorkflowTemplates
            .Include(w => w.States.OrderBy(s => s.OrderIndex))
            .FirstOrDefaultAsync(w => w.Id == id);
    }

    public async Task<WorkflowTemplate> CreateAsync(WorkflowTemplate workflowTemplate)
    {
        workflowTemplate.CreatedAt = DateTime.UtcNow;
        workflowTemplate.UpdatedAt = DateTime.UtcNow;
        _context.WorkflowTemplates.Add(workflowTemplate);
        await _context.SaveChangesAsync();
        return workflowTemplate;
    }

    public async Task<WorkflowTemplate> UpdateAsync(WorkflowTemplate workflowTemplate)
    {
        workflowTemplate.UpdatedAt = DateTime.UtcNow;
        _context.WorkflowTemplates.Update(workflowTemplate);
        await _context.SaveChangesAsync();
        return workflowTemplate;
    }

    public async Task DeleteAsync(Guid id)
    {
        var workflowTemplate = await _context.WorkflowTemplates.FindAsync(id);
        if (workflowTemplate != null)
        {
            _context.WorkflowTemplates.Remove(workflowTemplate);
            await _context.SaveChangesAsync();
        }
    }
}

public class WorkflowStateTemplateRepository : IWorkflowStateTemplateRepository
{
    private readonly OpenUpToolDbContext _context;

    public WorkflowStateTemplateRepository(OpenUpToolDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<WorkflowStateTemplate>> GetByWorkflowTemplateIdAsync(Guid workflowTemplateId)
    {
        return await _context.WorkflowStateTemplates
            .Where(s => s.WorkflowTemplateId == workflowTemplateId)
            .OrderBy(s => s.OrderIndex)
            .ToListAsync();
    }

    public async Task<WorkflowStateTemplate?> GetByIdAsync(Guid id)
    {
        return await _context.WorkflowStateTemplates.FindAsync(id);
    }

    public async Task<WorkflowStateTemplate> CreateAsync(WorkflowStateTemplate stateTemplate)
    {
        stateTemplate.CreatedAt = DateTime.UtcNow;
        stateTemplate.UpdatedAt = DateTime.UtcNow;
        _context.WorkflowStateTemplates.Add(stateTemplate);
        await _context.SaveChangesAsync();
        return stateTemplate;
    }

    public async Task<WorkflowStateTemplate> UpdateAsync(WorkflowStateTemplate stateTemplate)
    {
        stateTemplate.UpdatedAt = DateTime.UtcNow;
        _context.WorkflowStateTemplates.Update(stateTemplate);
        await _context.SaveChangesAsync();
        return stateTemplate;
    }

    public async Task DeleteAsync(Guid id)
    {
        var stateTemplate = await _context.WorkflowStateTemplates.FindAsync(id);
        if (stateTemplate != null)
        {
            _context.WorkflowStateTemplates.Remove(stateTemplate);
            await _context.SaveChangesAsync();
        }
    }
}

public class CustomFieldDefinitionRepository : ICustomFieldDefinitionRepository
{
    private readonly OpenUpToolDbContext _context;

    public CustomFieldDefinitionRepository(OpenUpToolDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CustomFieldDefinition>> GetByArtifactTypeTemplateIdAsync(Guid artifactTypeTemplateId)
    {
        return await _context.CustomFieldDefinitions
            .Where(f => f.ArtifactTypeTemplateId == artifactTypeTemplateId)
            .OrderBy(f => f.OrderIndex)
            .ToListAsync();
    }

    public async Task<IEnumerable<CustomFieldDefinition>> GetByConfigurationIdAsync(Guid configurationId)
    {
        return await _context.CustomFieldDefinitions
            .Include(f => f.ArtifactTypeTemplate)
            .Where(f => f.ArtifactTypeTemplate.ConfigurationId == configurationId)
            .OrderBy(f => f.ArtifactTypeTemplate.Name)
            .ThenBy(f => f.OrderIndex)
            .ToListAsync();
    }

    public async Task<CustomFieldDefinition?> GetByIdAsync(Guid id)
    {
        return await _context.CustomFieldDefinitions.FindAsync(id);
    }

    public async Task<CustomFieldDefinition> CreateAsync(CustomFieldDefinition customField)
    {
        customField.CreatedAt = DateTime.UtcNow;
        customField.UpdatedAt = DateTime.UtcNow;
        _context.CustomFieldDefinitions.Add(customField);
        await _context.SaveChangesAsync();
        return customField;
    }

    public async Task<CustomFieldDefinition> UpdateAsync(CustomFieldDefinition customField)
    {
        customField.UpdatedAt = DateTime.UtcNow;
        _context.CustomFieldDefinitions.Update(customField);
        await _context.SaveChangesAsync();
        return customField;
    }

    public async Task DeleteAsync(Guid id)
    {
        var customField = await _context.CustomFieldDefinitions.FindAsync(id);
        if (customField != null)
        {
            _context.CustomFieldDefinitions.Remove(customField);
            await _context.SaveChangesAsync();
        }
    }
}

public class ConfigurationChangeHistoryRepository : IConfigurationChangeHistoryRepository
{
    private readonly OpenUpToolDbContext _context;

    public ConfigurationChangeHistoryRepository(OpenUpToolDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ConfigurationChangeHistory>> GetByConfigurationIdAsync(Guid configurationId)
    {
        return await _context.ConfigurationChangeHistories
            .Where(h => h.ConfigurationId == configurationId)
            .OrderByDescending(h => h.ChangedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ConfigurationChangeHistory>> GetByConfigurationIdAndVersionAsync(Guid configurationId, int version)
    {
        return await _context.ConfigurationChangeHistories
            .Where(h => h.ConfigurationId == configurationId && h.ToVersion == version)
            .OrderByDescending(h => h.ChangedAt)
            .ToListAsync();
    }

    public async Task<ConfigurationChangeHistory?> GetByIdAsync(Guid id)
    {
        return await _context.ConfigurationChangeHistories.FindAsync(id);
    }

    public async Task<ConfigurationChangeHistory> CreateAsync(ConfigurationChangeHistory history)
    {
        history.ChangedAt = DateTime.UtcNow;
        _context.ConfigurationChangeHistories.Add(history);
        await _context.SaveChangesAsync();
        return history;
    }
}

public class ProjectConfigurationRepository : IProjectConfigurationRepository
{
    private readonly OpenUpToolDbContext _context;

    public ProjectConfigurationRepository(OpenUpToolDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ProjectConfiguration>> GetAllAsync()
    {
        return await _context.ProjectConfigurations
            .Include(pc => pc.Project)
            .Include(pc => pc.Configuration)
            .OrderBy(pc => pc.AppliedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProjectConfiguration>> GetByConfigurationIdAsync(Guid configurationId)
    {
        return await _context.ProjectConfigurations
            .Include(pc => pc.Project)
            .Where(pc => pc.ConfigurationId == configurationId)
            .OrderBy(pc => pc.AppliedAt)
            .ToListAsync();
    }

    public async Task<ProjectConfiguration?> GetByProjectIdAsync(Guid projectId)
    {
        return await _context.ProjectConfigurations
            .Include(pc => pc.Project)
            .Include(pc => pc.Configuration)
            .FirstOrDefaultAsync(pc => pc.ProjectId == projectId);
    }

    public async Task<ProjectConfiguration?> GetByIdAsync(Guid id)
    {
        return await _context.ProjectConfigurations
            .Include(pc => pc.Project)
            .Include(pc => pc.Configuration)
            .FirstOrDefaultAsync(pc => pc.Id == id);
    }

    public async Task<ProjectConfiguration> CreateAsync(ProjectConfiguration projectConfiguration)
    {
        projectConfiguration.AppliedAt = DateTime.UtcNow;
        _context.ProjectConfigurations.Add(projectConfiguration);
        await _context.SaveChangesAsync();
        return projectConfiguration;
    }

    public async Task<ProjectConfiguration> UpdateAsync(ProjectConfiguration projectConfiguration)
    {
        _context.ProjectConfigurations.Update(projectConfiguration);
        await _context.SaveChangesAsync();
        return projectConfiguration;
    }

    public async Task DeleteAsync(Guid id)
    {
        var projectConfiguration = await _context.ProjectConfigurations.FindAsync(id);
        if (projectConfiguration != null)
        {
            _context.ProjectConfigurations.Remove(projectConfiguration);
            await _context.SaveChangesAsync();
        }
    }
}

public class ArtifactCustomFieldValueRepository : IArtifactCustomFieldValueRepository
{
    private readonly OpenUpToolDbContext _context;

    public ArtifactCustomFieldValueRepository(OpenUpToolDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ArtifactCustomFieldValue>> GetByArtifactIdAsync(Guid artifactId)
    {
        return await _context.ArtifactCustomFieldValues
            .Include(v => v.CustomFieldDefinition)
            .Where(v => v.ArtifactId == artifactId)
            .OrderBy(v => v.CustomFieldDefinition.OrderIndex)
            .ToListAsync();
    }

    public async Task<ArtifactCustomFieldValue?> GetByIdAsync(Guid id)
    {
        return await _context.ArtifactCustomFieldValues
            .Include(v => v.CustomFieldDefinition)
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<ArtifactCustomFieldValue?> GetByArtifactAndFieldAsync(Guid artifactId, Guid customFieldDefinitionId)
    {
        return await _context.ArtifactCustomFieldValues
            .Include(v => v.CustomFieldDefinition)
            .FirstOrDefaultAsync(v => v.ArtifactId == artifactId && v.CustomFieldDefinitionId == customFieldDefinitionId);
    }

    public async Task<ArtifactCustomFieldValue> CreateAsync(ArtifactCustomFieldValue value)
    {
        value.CreatedAt = DateTime.UtcNow;
        value.UpdatedAt = DateTime.UtcNow;
        _context.ArtifactCustomFieldValues.Add(value);
        await _context.SaveChangesAsync();
        return value;
    }

    public async Task<ArtifactCustomFieldValue> UpdateAsync(ArtifactCustomFieldValue value)
    {
        value.UpdatedAt = DateTime.UtcNow;
        _context.ArtifactCustomFieldValues.Update(value);
        await _context.SaveChangesAsync();
        return value;
    }

    public async Task DeleteAsync(Guid id)
    {
        var value = await _context.ArtifactCustomFieldValues.FindAsync(id);
        if (value != null)
        {
            _context.ArtifactCustomFieldValues.Remove(value);
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteByArtifactIdAsync(Guid artifactId)
    {
        var values = await _context.ArtifactCustomFieldValues
            .Where(v => v.ArtifactId == artifactId)
            .ToListAsync();
        
        if (values.Any())
        {
            _context.ArtifactCustomFieldValues.RemoveRange(values);
            await _context.SaveChangesAsync();
        }
    }
}
