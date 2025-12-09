using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenUpTool.Core.DTOs;
using OpenUpTool.Core.Interfaces;
using System.Security.Claims;

namespace OpenUpTool.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ConfigurationController : ControllerBase
{
    private readonly IGlobalConfigurationService _configService;
    private readonly IRoleTemplateService _roleService;
    private readonly IPhaseTemplateService _phaseService;
    private readonly IArtifactTypeTemplateService _artifactTypeService;
    private readonly IWorkflowTemplateService _workflowService;
    private readonly IWorkflowStateTemplateService _workflowStateService;
    private readonly ICustomFieldDefinitionService _customFieldService;
    private readonly IProjectConfigurationService _projectConfigService;
    private readonly IArtifactCustomFieldValueService _fieldValueService;

    public ConfigurationController(
        IGlobalConfigurationService configService,
        IRoleTemplateService roleService,
        IPhaseTemplateService phaseService,
        IArtifactTypeTemplateService artifactTypeService,
        IWorkflowTemplateService workflowService,
        IWorkflowStateTemplateService workflowStateService,
        ICustomFieldDefinitionService customFieldService,
        IProjectConfigurationService projectConfigService,
        IArtifactCustomFieldValueService fieldValueService)
    {
        _configService = configService;
        _roleService = roleService;
        _phaseService = phaseService;
        _artifactTypeService = artifactTypeService;
        _workflowService = workflowService;
        _workflowStateService = workflowStateService;
        _customFieldService = customFieldService;
        _projectConfigService = projectConfigService;
        _fieldValueService = fieldValueService;
    }

    private string GetCurrentUser() => User.FindFirst(ClaimTypes.Email)?.Value ?? "system";

    // ==================== GLOBAL CONFIGURATIONS ====================

    /// <summary>
    /// Obtiene todas las configuraciones globales
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GlobalConfigurationDto>>> GetAllConfigurations()
    {
        var configs = await _configService.GetAllConfigurationsAsync();
        return Ok(configs);
    }

    /// <summary>
    /// Obtiene una configuración por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<GlobalConfigurationDetailDto>> GetConfiguration(Guid id)
    {
        var config = await _configService.GetConfigurationDetailsAsync(id);
        if (config == null) return NotFound();
        return Ok(config);
    }

    /// <summary>
    /// Obtiene la configuración por defecto
    /// </summary>
    [HttpGet("default")]
    public async Task<ActionResult<GlobalConfigurationDetailDto>> GetDefaultConfiguration()
    {
        var config = await _configService.GetDefaultConfigurationAsync();
        if (config == null) return NotFound("No hay configuración por defecto");
        return Ok(config);
    }

    /// <summary>
    /// Crea una nueva configuración global
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<GlobalConfigurationDto>> CreateConfiguration(CreateGlobalConfigurationDto dto)
    {
        var created = await _configService.CreateConfigurationAsync(dto, GetCurrentUser());
        return CreatedAtAction(nameof(GetConfiguration), new { id = created.Id }, created);
    }

    /// <summary>
    /// Actualiza una configuración
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<GlobalConfigurationDto>> UpdateConfiguration(Guid id, UpdateGlobalConfigurationDto dto)
    {
        var updated = await _configService.UpdateConfigurationAsync(id, dto);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    /// <summary>
    /// Elimina una configuración
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteConfiguration(Guid id)
    {
        try
        {
            var deleted = await _configService.DeleteConfigurationAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Obtiene el historial de cambios de una configuración
    /// </summary>
    [HttpGet("{id}/history")]
    public async Task<ActionResult<IEnumerable<ConfigurationChangeHistoryDto>>> GetChangeHistory(Guid id)
    {
        var history = await _configService.GetChangeHistoryAsync(id);
        return Ok(history);
    }

    /// <summary>
    /// Incrementa la versión de una configuración
    /// </summary>
    [HttpPost("{id}/increment-version")]
    public async Task<ActionResult<GlobalConfigurationDto>> IncrementVersion(Guid id, [FromBody] string changeDescription)
    {
        var updated = await _configService.IncrementVersionAsync(id, GetCurrentUser(), changeDescription);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    /// <summary>
    /// Incrementa la versión de una configuración (alias)
    /// </summary>
    [HttpPost("{id}/version")]
    public async Task<ActionResult<GlobalConfigurationDto>> IncrementVersionAlias(Guid id)
    {
        var updated = await _configService.IncrementVersionAsync(id, GetCurrentUser(), "Version increment");
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    /// <summary>
    /// Establece una configuración como predeterminada
    /// </summary>
    [HttpPost("{id}/set-default")]
    public async Task<ActionResult<GlobalConfigurationDto>> SetAsDefault(Guid id)
    {
        var config = await _configService.GetConfigurationByIdAsync(id);
        if (config == null) return NotFound();

        // Update this config to be default, preserving IsActive
        var updateDto = new UpdateGlobalConfigurationDto
        {
            Name = config.Name,
            Description = config.Description,
            IsActive = config.IsActive,
            IsDefault = true
        };
        
        var updated = await _configService.UpdateConfigurationAsync(id, updateDto);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    /// <summary>
    /// Revierte a una versión anterior
    /// </summary>
    [HttpPost("{id}/rollback")]
    public async Task<ActionResult<GlobalConfigurationDetailDto>> RollbackToVersion(Guid id, RollbackConfigurationDto dto)
    {
        try
        {
            var config = await _configService.RollbackToVersionAsync(id, dto.TargetVersion, GetCurrentUser(), dto.Reason);
            if (config == null) return NotFound();
            return Ok(config);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // ==================== ROLE TEMPLATES ====================

    /// <summary>
    /// Obtiene los roles de una configuración
    /// </summary>
    [HttpGet("{configId}/roles")]
    public async Task<ActionResult<IEnumerable<RoleTemplateDto>>> GetRoles(Guid configId)
    {
        var roles = await _roleService.GetRolesByConfigurationAsync(configId);
        return Ok(roles);
    }

    /// <summary>
    /// Obtiene un rol por ID
    /// </summary>
    [HttpGet("roles/{id}")]
    public async Task<ActionResult<RoleTemplateDto>> GetRole(Guid id)
    {
        var role = await _roleService.GetRoleByIdAsync(id);
        if (role == null) return NotFound();
        return Ok(role);
    }

    /// <summary>
    /// Crea un nuevo rol
    /// </summary>
    [HttpPost("{configId}/roles")]
    public async Task<ActionResult<RoleTemplateDto>> CreateRole(Guid configId, CreateRoleTemplateDto dto)
    {
        dto.ConfigurationId = configId;
        var created = await _roleService.CreateRoleAsync(dto, GetCurrentUser());
        return CreatedAtAction(nameof(GetRole), new { id = created.Id }, created);
    }

    /// <summary>
    /// Actualiza un rol
    /// </summary>
    [HttpPut("{configId}/roles/{id}")]
    public async Task<ActionResult<RoleTemplateDto>> UpdateRole(Guid configId, Guid id, UpdateRoleTemplateDto dto)
    {
        try
        {
            var updated = await _roleService.UpdateRoleAsync(id, dto, GetCurrentUser());
            if (updated == null) return NotFound();
            return Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Elimina un rol
    /// </summary>
    [HttpDelete("{configId}/roles/{id}")]
    public async Task<IActionResult> DeleteRole(Guid configId, Guid id)
    {
        try
        {
            var deleted = await _roleService.DeleteRoleAsync(id, GetCurrentUser());
            if (!deleted) return NotFound();
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // ==================== PHASE TEMPLATES ====================

    /// <summary>
    /// Obtiene las fases de una configuración
    /// </summary>
    [HttpGet("{configId}/phases")]
    public async Task<ActionResult<IEnumerable<PhaseTemplateDto>>> GetPhases(Guid configId)
    {
        var phases = await _phaseService.GetPhasesByConfigurationAsync(configId);
        return Ok(phases);
    }

    /// <summary>
    /// Obtiene una fase por ID
    /// </summary>
    [HttpGet("phases/{id}")]
    public async Task<ActionResult<PhaseTemplateDto>> GetPhase(Guid id)
    {
        var phase = await _phaseService.GetPhaseByIdAsync(id);
        if (phase == null) return NotFound();
        return Ok(phase);
    }

    /// <summary>
    /// Crea una nueva fase
    /// </summary>
    [HttpPost("{configId}/phases")]
    public async Task<ActionResult<PhaseTemplateDto>> CreatePhase(Guid configId, CreatePhaseTemplateDto dto)
    {
        dto.ConfigurationId = configId;
        var created = await _phaseService.CreatePhaseAsync(dto, GetCurrentUser());
        return CreatedAtAction(nameof(GetPhase), new { id = created.Id }, created);
    }

    /// <summary>
    /// Actualiza una fase
    /// </summary>
    [HttpPut("{configId}/phases/{id}")]
    public async Task<ActionResult<PhaseTemplateDto>> UpdatePhase(Guid configId, Guid id, UpdatePhaseTemplateDto dto)
    {
        var updated = await _phaseService.UpdatePhaseAsync(id, dto, GetCurrentUser());
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    /// <summary>
    /// Elimina una fase
    /// </summary>
    [HttpDelete("{configId}/phases/{id}")]
    public async Task<IActionResult> DeletePhase(Guid configId, Guid id)
    {
        var deleted = await _phaseService.DeletePhaseAsync(id, GetCurrentUser());
        if (!deleted) return NotFound();
        return NoContent();
    }

    // ==================== ARTIFACT TYPE TEMPLATES ====================

    /// <summary>
    /// Obtiene los tipos de artefacto de una configuración
    /// </summary>
    [HttpGet("{configId}/artifact-types")]
    public async Task<ActionResult<IEnumerable<ArtifactTypeTemplateDto>>> GetArtifactTypes(Guid configId)
    {
        var types = await _artifactTypeService.GetArtifactTypesByConfigurationAsync(configId);
        return Ok(types);
    }

    /// <summary>
    /// Obtiene los tipos de artefacto de una fase específica
    /// </summary>
    [HttpGet("{configId}/artifact-types/phase/{phaseCode}")]
    public async Task<ActionResult<IEnumerable<ArtifactTypeTemplateDto>>> GetArtifactTypesByPhase(Guid configId, string phaseCode)
    {
        var types = await _artifactTypeService.GetArtifactTypesByPhaseAsync(configId, phaseCode);
        return Ok(types);
    }

    /// <summary>
    /// Obtiene un tipo de artefacto por ID
    /// </summary>
    [HttpGet("artifact-types/{id}")]
    public async Task<ActionResult<ArtifactTypeTemplateDto>> GetArtifactType(Guid id)
    {
        var type = await _artifactTypeService.GetArtifactTypeByIdAsync(id);
        if (type == null) return NotFound();
        return Ok(type);
    }

    /// <summary>
    /// Crea un nuevo tipo de artefacto
    /// </summary>
    [HttpPost("{configId}/artifact-types")]
    public async Task<ActionResult<ArtifactTypeTemplateDto>> CreateArtifactType(Guid configId, CreateArtifactTypeTemplateDto dto)
    {
        dto.ConfigurationId = configId;
        var created = await _artifactTypeService.CreateArtifactTypeAsync(dto, GetCurrentUser());
        return CreatedAtAction(nameof(GetArtifactType), new { id = created.Id }, created);
    }

    /// <summary>
    /// Actualiza un tipo de artefacto
    /// </summary>
    [HttpPut("{configId}/artifact-types/{id}")]
    public async Task<ActionResult<ArtifactTypeTemplateDto>> UpdateArtifactType(Guid configId, Guid id, UpdateArtifactTypeTemplateDto dto)
    {
        var updated = await _artifactTypeService.UpdateArtifactTypeAsync(id, dto, GetCurrentUser());
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    /// <summary>
    /// Elimina un tipo de artefacto
    /// </summary>
    [HttpDelete("{configId}/artifact-types/{id}")]
    public async Task<IActionResult> DeleteArtifactType(Guid configId, Guid id)
    {
        var deleted = await _artifactTypeService.DeleteArtifactTypeAsync(id, GetCurrentUser());
        if (!deleted) return NotFound();
        return NoContent();
    }

    // ==================== WORKFLOW TEMPLATES ====================

    /// <summary>
    /// Obtiene los workflows de una configuración
    /// </summary>
    [HttpGet("{configId}/workflows")]
    public async Task<ActionResult<IEnumerable<WorkflowTemplateDto>>> GetWorkflows(Guid configId)
    {
        var workflows = await _workflowService.GetWorkflowsByConfigurationAsync(configId);
        return Ok(workflows);
    }

    /// <summary>
    /// Obtiene un workflow por ID
    /// </summary>
    [HttpGet("workflows/{id}")]
    public async Task<ActionResult<WorkflowTemplateDto>> GetWorkflow(Guid id)
    {
        var workflow = await _workflowService.GetWorkflowByIdAsync(id);
        if (workflow == null) return NotFound();
        return Ok(workflow);
    }

    /// <summary>
    /// Crea un nuevo workflow
    /// </summary>
    [HttpPost("{configId}/workflows")]
    public async Task<ActionResult<WorkflowTemplateDto>> CreateWorkflow(Guid configId, CreateWorkflowTemplateDto dto)
    {
        dto.ConfigurationId = configId;
        var created = await _workflowService.CreateWorkflowAsync(dto, GetCurrentUser());
        return CreatedAtAction(nameof(GetWorkflow), new { id = created.Id }, created);
    }

    /// <summary>
    /// Actualiza un workflow
    /// </summary>
    [HttpPut("{configId}/workflows/{id}")]
    public async Task<ActionResult<WorkflowTemplateDto>> UpdateWorkflow(Guid configId, Guid id, UpdateWorkflowTemplateDto dto)
    {
        var updated = await _workflowService.UpdateWorkflowAsync(id, dto, GetCurrentUser());
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    /// <summary>
    /// Elimina un workflow
    /// </summary>
    [HttpDelete("{configId}/workflows/{id}")]
    public async Task<IActionResult> DeleteWorkflow(Guid configId, Guid id)
    {
        var deleted = await _workflowService.DeleteWorkflowAsync(id, GetCurrentUser());
        if (!deleted) return NotFound();
        return NoContent();
    }

    // ==================== WORKFLOW STATE TEMPLATES ====================

    /// <summary>
    /// Obtiene los estados de un workflow
    /// </summary>
    [HttpGet("{configId}/workflows/{workflowId}/states")]
    public async Task<ActionResult<IEnumerable<WorkflowStateTemplateDto>>> GetWorkflowStates(Guid configId, Guid workflowId)
    {
        var states = await _workflowStateService.GetStatesByWorkflowAsync(workflowId);
        return Ok(states);
    }

    /// <summary>
    /// Obtiene un estado de workflow por ID
    /// </summary>
    [HttpGet("workflow-states/{id}")]
    public async Task<ActionResult<WorkflowStateTemplateDto>> GetWorkflowState(Guid id)
    {
        var state = await _workflowStateService.GetStateByIdAsync(id);
        if (state == null) return NotFound();
        return Ok(state);
    }

    /// <summary>
    /// Crea un nuevo estado de workflow
    /// </summary>
    [HttpPost("{configId}/workflows/{workflowId}/states")]
    public async Task<ActionResult<WorkflowStateTemplateDto>> CreateWorkflowState(Guid configId, Guid workflowId, CreateWorkflowStateTemplateDto dto)
    {
        dto.WorkflowTemplateId = workflowId;
        var created = await _workflowStateService.CreateStateAsync(dto, GetCurrentUser());
        return CreatedAtAction(nameof(GetWorkflowState), new { id = created.Id }, created);
    }

    /// <summary>
    /// Actualiza un estado de workflow
    /// </summary>
    [HttpPut("{configId}/workflows/{workflowId}/states/{id}")]
    public async Task<ActionResult<WorkflowStateTemplateDto>> UpdateWorkflowState(Guid configId, Guid workflowId, Guid id, UpdateWorkflowStateTemplateDto dto)
    {
        var updated = await _workflowStateService.UpdateStateAsync(id, dto, GetCurrentUser());
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    /// <summary>
    /// Elimina un estado de workflow
    /// </summary>
    [HttpDelete("{configId}/workflows/{workflowId}/states/{id}")]
    public async Task<IActionResult> DeleteWorkflowState(Guid configId, Guid workflowId, Guid id)
    {
        var deleted = await _workflowStateService.DeleteStateAsync(id, GetCurrentUser());
        if (!deleted) return NotFound();
        return NoContent();
    }

    // ==================== CUSTOM FIELDS ====================

    /// <summary>
    /// Obtiene los campos personalizados de una configuracion
    /// </summary>
    [HttpGet("{configId}/custom-fields")]
    public async Task<ActionResult<IEnumerable<CustomFieldDefinitionDto>>> GetCustomFieldsByConfig(Guid configId)
    {
        var fields = await _customFieldService.GetFieldsByConfigurationAsync(configId);
        return Ok(fields);
    }

    /// <summary>
    /// Obtiene los campos personalizados de un tipo de artefacto
    /// </summary>
    [HttpGet("artifact-types/{artifactTypeId}/custom-fields")]
    public async Task<ActionResult<IEnumerable<CustomFieldDefinitionDto>>> GetCustomFields(Guid artifactTypeId)
    {
        var fields = await _customFieldService.GetFieldsByArtifactTypeAsync(artifactTypeId);
        return Ok(fields);
    }

    /// <summary>
    /// Obtiene un campo personalizado por ID
    /// </summary>
    [HttpGet("custom-fields/{id}")]
    public async Task<ActionResult<CustomFieldDefinitionDto>> GetCustomField(Guid id)
    {
        var field = await _customFieldService.GetFieldByIdAsync(id);
        if (field == null) return NotFound();
        return Ok(field);
    }

    /// <summary>
    /// Crea un nuevo campo personalizado
    /// </summary>
    [HttpPost("{configId}/custom-fields")]
    public async Task<ActionResult<CustomFieldDefinitionDto>> CreateCustomField(Guid configId, CreateCustomFieldDefinitionDto dto)
    {
        try
        {
            var created = await _customFieldService.CreateFieldAsync(dto, GetCurrentUser());
            return CreatedAtAction(nameof(GetCustomField), new { id = created.Id }, created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Actualiza un campo personalizado
    /// </summary>
    [HttpPut("{configId}/custom-fields/{id}")]
    public async Task<ActionResult<CustomFieldDefinitionDto>> UpdateCustomField(Guid configId, Guid id, UpdateCustomFieldDefinitionDto dto)
    {
        var updated = await _customFieldService.UpdateFieldAsync(id, dto, GetCurrentUser());
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    /// <summary>
    /// Elimina un campo personalizado
    /// </summary>
    [HttpDelete("{configId}/custom-fields/{id}")]
    public async Task<IActionResult> DeleteCustomField(Guid configId, Guid id)
    {
        var deleted = await _customFieldService.DeleteFieldAsync(id, GetCurrentUser());
        if (!deleted) return NotFound();
        return NoContent();
    }

    // ==================== PROJECT CONFIGURATIONS ====================

    /// <summary>
    /// Obtiene todas las configuraciones de proyectos
    /// </summary>
    [HttpGet("projects")]
    public async Task<ActionResult<IEnumerable<ProjectConfigurationDto>>> GetAllProjectConfigurations()
    {
        var configs = await _projectConfigService.GetAllProjectConfigurationsAsync();
        return Ok(configs);
    }

    /// <summary>
    /// Obtiene los proyectos que usan una configuración específica
    /// </summary>
    [HttpGet("{configId}/projects")]
    public async Task<ActionResult<IEnumerable<ProjectConfigurationDto>>> GetProjectsByConfiguration(Guid configId)
    {
        var configs = await _projectConfigService.GetProjectsByConfigurationAsync(configId);
        return Ok(configs);
    }

    /// <summary>
    /// Aplica esta configuración a un proyecto
    /// </summary>
    [HttpPost("{configId}/projects")]
    public async Task<ActionResult<ProjectConfigurationDto>> ApplyConfigurationToProjectByConfigId(Guid configId, [FromBody] ApplyConfigurationToProjectByConfigDto dto)
    {
        try
        {
            var applyDto = new ApplyConfigurationToProjectDto
            {
                ProjectId = dto.ProjectId,
                ConfigurationId = configId,
                AutoUpdate = dto.AutoUpdate,
                ForceUpdate = dto.ForceUpdate
            };
            var result = await _projectConfigService.ApplyConfigurationToProjectAsync(applyDto, GetCurrentUser());
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Obtiene la configuración de un proyecto específico
    /// </summary>
    [HttpGet("projects/{projectId}")]
    public async Task<ActionResult<ProjectConfigurationDto>> GetProjectConfiguration(Guid projectId)
    {
        var config = await _projectConfigService.GetProjectConfigurationAsync(projectId);
        if (config == null) return NotFound();
        return Ok(config);
    }

    /// <summary>
    /// Aplica una configuración a un proyecto
    /// </summary>
    [HttpPost("projects/apply")]
    public async Task<ActionResult<ProjectConfigurationDto>> ApplyConfigurationToProject(ApplyConfigurationToProjectDto dto)
    {
        try
        {
            var result = await _projectConfigService.ApplyConfigurationToProjectAsync(dto, GetCurrentUser());
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Actualiza la configuración de auto-update de un proyecto
    /// </summary>
    [HttpPatch("projects/{projectId}/settings")]
    public async Task<ActionResult<ProjectConfigurationDto>> UpdateProjectConfigurationSettings(Guid projectId, UpdateProjectConfigurationSettingsDto dto)
    {
        var updated = await _projectConfigService.UpdateProjectConfigurationSettingsAsync(projectId, dto);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    /// <summary>
    /// Aplica actualizaciones de configuración a múltiples proyectos
    /// </summary>
    [HttpPost("{configId}/apply-updates")]
    public async Task<ActionResult<BulkUpdateResultDto>> ApplyUpdatesToProjects(Guid configId, ApplyConfigurationUpdatesDto dto)
    {
        try
        {
            var result = await _projectConfigService.ApplyConfigurationUpdatesToProjectsAsync(configId, dto, GetCurrentUser());
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // ==================== ARTIFACT CUSTOM FIELD VALUES ====================

    /// <summary>
    /// Obtiene los valores de campos personalizados de un artefacto
    /// </summary>
    [HttpGet("artifacts/{artifactId}/field-values")]
    public async Task<ActionResult<IEnumerable<ArtifactCustomFieldValueDto>>> GetArtifactFieldValues(Guid artifactId)
    {
        var values = await _fieldValueService.GetFieldValuesByArtifactAsync(artifactId);
        return Ok(values);
    }

    /// <summary>
    /// Establece el valor de un campo personalizado
    /// </summary>
    [HttpPost("artifacts/field-values")]
    public async Task<ActionResult<ArtifactCustomFieldValueDto>> SetFieldValue(SetArtifactCustomFieldValueDto dto)
    {
        try
        {
            var result = await _fieldValueService.SetFieldValueAsync(dto);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Establece múltiples valores de campos personalizados
    /// </summary>
    [HttpPost("artifacts/field-values/bulk")]
    public async Task<ActionResult<IEnumerable<ArtifactCustomFieldValueDto>>> BulkSetFieldValues(BulkSetCustomFieldValuesDto dto)
    {
        var results = await _fieldValueService.BulkSetFieldValuesAsync(dto);
        return Ok(results);
    }

    /// <summary>
    /// Elimina un valor de campo personalizado
    /// </summary>
    [HttpDelete("field-values/{id}")]
    public async Task<IActionResult> DeleteFieldValue(Guid id)
    {
        var deleted = await _fieldValueService.DeleteFieldValueAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }

    // ==================== HU-019: OPENUP TEMPLATES ====================

    /// <summary>
    /// Lista todas las plantillas OpenUP disponibles
    /// </summary>
    [HttpGet("templates")]
    public async Task<ActionResult<IEnumerable<TemplateListItemDto>>> GetAllTemplates([FromQuery] string? tags = null)
    {
        var configs = await _configService.GetAllConfigurationsAsync();
        var projectConfigs = await _projectConfigService.GetAllProjectConfigurationsAsync();
        
        var templates = configs.Select(c => new TemplateListItemDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            Version = c.Version,
            IsDefault = c.IsDefault,
            IsActive = c.IsActive,
            CreatedBy = c.CreatedBy,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt,
            RolesCount = c.RoleTemplatesCount,
            PhasesCount = c.PhaseTemplatesCount,
            ArtifactTypesCount = c.ArtifactTypeTemplatesCount,
            WorkflowsCount = c.WorkflowTemplatesCount,
            ProjectsUsingCount = projectConfigs.Count(pc => pc.ConfigurationId == c.Id)
        });

        // Filter by tags if provided
        if (!string.IsNullOrEmpty(tags))
        {
            var tagList = tags.Split(',').Select(t => t.Trim().ToLower()).ToList();
            templates = templates.Where(t => 
                !string.IsNullOrEmpty(t.Tags) && 
                tagList.Any(tag => t.Tags.ToLower().Contains(tag)));
        }

        return Ok(templates);
    }

    /// <summary>
    /// Guarda una configuración como nueva plantilla OpenUP
    /// </summary>
    [HttpPost("{id}/save-as-template")]
    public async Task<ActionResult<GlobalConfigurationDto>> SaveAsTemplate(Guid id, SaveAsTemplateDto dto)
    {
        var source = await _configService.GetConfigurationDetailsAsync(id);
        if (source == null) return NotFound("Configuración origen no encontrada");

        // Create new configuration and clone all contents
        var clonedConfig = await CloneConfigurationWithContents(source, dto.Name, dto.Description);
        return Ok(clonedConfig);
    }

    /// <summary>
    /// Clona una plantilla existente con un nuevo nombre
    /// </summary>
    [HttpPost("{id}/clone")]
    public async Task<ActionResult<GlobalConfigurationDto>> CloneTemplate(Guid id, CloneTemplateDto dto)
    {
        var source = await _configService.GetConfigurationDetailsAsync(id);
        if (source == null) return NotFound("Plantilla origen no encontrada");

        var clonedConfig = await CloneConfigurationWithContents(source, dto.NewName, dto.NewDescription);
        return Ok(clonedConfig);
    }

    /// <summary>
    /// Compara dos plantillas y muestra las diferencias
    /// </summary>
    [HttpGet("templates/compare")]
    public async Task<ActionResult<TemplateComparisonDto>> CompareTemplates([FromQuery] Guid template1Id, [FromQuery] Guid template2Id)
    {
        var t1 = await _configService.GetConfigurationDetailsAsync(template1Id);
        var t2 = await _configService.GetConfigurationDetailsAsync(template2Id);

        if (t1 == null) return NotFound($"Plantilla 1 con ID {template1Id} no encontrada");
        if (t2 == null) return NotFound($"Plantilla 2 con ID {template2Id} no encontrada");

        var comparison = CompareConfigurations(t1, t2);
        return Ok(comparison);
    }

    /// <summary>
    /// Exporta una plantilla para backup o transferencia
    /// </summary>
    [HttpGet("{id}/export")]
    public async Task<ActionResult<TemplateExportDto>> ExportTemplate(Guid id)
    {
        var config = await _configService.GetConfigurationDetailsAsync(id);
        if (config == null) return NotFound();

        var export = new TemplateExportDto
        {
            OriginalId = config.Id,
            Name = config.Name,
            Description = config.Description,
            Version = config.Version,
            ExportedAt = DateTime.UtcNow,
            ExportedBy = GetCurrentUser(),
            Roles = config.RoleTemplates,
            Phases = config.PhaseTemplates,
            ArtifactTypes = config.ArtifactTypeTemplates,
            Workflows = config.WorkflowTemplates
        };

        return Ok(export);
    }

    /// <summary>
    /// Importa una plantilla desde datos exportados
    /// </summary>
    [HttpPost("templates/import")]
    public async Task<ActionResult<GlobalConfigurationDto>> ImportTemplate(TemplateImportDto dto)
    {
        var createDto = new CreateGlobalConfigurationDto
        {
            Name = dto.Name,
            Description = dto.Description,
            IsDefault = false,
            CopyFromDefault = false
        };

        var created = await _configService.CreateConfigurationAsync(createDto, GetCurrentUser());

        // Import roles
        if (dto.Roles != null)
        {
            foreach (var role in dto.Roles)
            {
                var roleDto = new CreateRoleTemplateDto
                {
                    ConfigurationId = created.Id,
                    Name = role.Name,
                    Description = role.Description,
                    Permissions = role.Permissions,
                    OrderIndex = role.OrderIndex
                };
                await _roleService.CreateRoleAsync(roleDto, GetCurrentUser());
            }
        }

        // Import phases
        if (dto.Phases != null)
        {
            foreach (var phase in dto.Phases)
            {
                var phaseDto = new CreatePhaseTemplateDto
                {
                    ConfigurationId = created.Id,
                    PhaseCode = phase.PhaseCode,
                    Name = phase.Name,
                    Description = phase.Description,
                    OrderIndex = phase.OrderIndex,
                    DefaultDurationDays = phase.DefaultDurationDays,
                    IsMandatory = phase.IsMandatory
                };
                await _phaseService.CreatePhaseAsync(phaseDto, GetCurrentUser());
            }
        }

        // Import artifact types
        if (dto.ArtifactTypes != null)
        {
            foreach (var at in dto.ArtifactTypes)
            {
                var atDto = new CreateArtifactTypeTemplateDto
                {
                    ConfigurationId = created.Id,
                    PhaseCode = at.PhaseCode,
                    Code = at.Code,
                    Name = at.Name,
                    Description = at.Description,
                    IsMandatory = at.IsMandatory,
                    DefaultFormat = at.DefaultFormat,
                    OrderIndex = at.OrderIndex
                };
                await _artifactTypeService.CreateArtifactTypeAsync(atDto, GetCurrentUser());
            }
        }

        // Import workflows
        if (dto.Workflows != null)
        {
            foreach (var wf in dto.Workflows)
            {
                var wfDto = new CreateWorkflowTemplateDto
                {
                    ConfigurationId = created.Id,
                    Name = wf.Name,
                    Description = wf.Description,
                    IsDefault = wf.IsDefault
                };
                var createdWf = await _workflowService.CreateWorkflowAsync(wfDto, GetCurrentUser());

                // Import workflow states
                if (wf.States != null)
                {
                    foreach (var state in wf.States)
                    {
                        var stateDto = new CreateWorkflowStateTemplateDto
                        {
                            WorkflowTemplateId = createdWf.Id,
                            Name = state.Name,
                            Description = state.Description,
                            IsInitialState = state.IsInitialState,
                            IsFinalState = state.IsFinalState,
                            Color = state.Color,
                            OrderIndex = state.OrderIndex
                        };
                        await _workflowStateService.CreateStateAsync(stateDto, GetCurrentUser());
                    }
                }
            }
        }

        // Return the fully populated configuration
        var result = await _configService.GetConfigurationByIdAsync(created.Id);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene el historial de versiones de una plantilla
    /// </summary>
    [HttpGet("{id}/versions")]
    public async Task<ActionResult<IEnumerable<ConfigurationChangeHistoryDto>>> GetTemplateVersionHistory(Guid id)
    {
        var config = await _configService.GetConfigurationByIdAsync(id);
        if (config == null) return NotFound();

        var history = await _configService.GetChangeHistoryAsync(id);
        return Ok(history);
    }

    // Helper method to clone a configuration with all its contents
    private async Task<GlobalConfigurationDto> CloneConfigurationWithContents(GlobalConfigurationDetailDto source, string newName, string? newDescription)
    {
        // Create new configuration
        var createDto = new CreateGlobalConfigurationDto
        {
            Name = newName,
            Description = newDescription ?? source.Description,
            IsDefault = false,
            CopyFromDefault = false
        };
        var created = await _configService.CreateConfigurationAsync(createDto, GetCurrentUser());

        // Clone roles
        foreach (var role in source.RoleTemplates)
        {
            var roleDto = new CreateRoleTemplateDto
            {
                ConfigurationId = created.Id,
                Name = role.Name,
                Description = role.Description,
                Permissions = role.Permissions,
                OrderIndex = role.OrderIndex
            };
            await _roleService.CreateRoleAsync(roleDto, GetCurrentUser());
        }

        // Clone phases
        foreach (var phase in source.PhaseTemplates)
        {
            var phaseDto = new CreatePhaseTemplateDto
            {
                ConfigurationId = created.Id,
                PhaseCode = phase.PhaseCode,
                Name = phase.Name,
                Description = phase.Description,
                OrderIndex = phase.OrderIndex,
                DefaultDurationDays = phase.DefaultDurationDays,
                IsMandatory = phase.IsMandatory
            };
            await _phaseService.CreatePhaseAsync(phaseDto, GetCurrentUser());
        }

        // Clone artifact types
        foreach (var at in source.ArtifactTypeTemplates)
        {
            var atDto = new CreateArtifactTypeTemplateDto
            {
                ConfigurationId = created.Id,
                PhaseCode = at.PhaseCode,
                Code = at.Code,
                Name = at.Name,
                Description = at.Description,
                IsMandatory = at.IsMandatory,
                DefaultFormat = at.DefaultFormat,
                OrderIndex = at.OrderIndex
            };
            await _artifactTypeService.CreateArtifactTypeAsync(atDto, GetCurrentUser());
        }

        // Clone workflows with states
        foreach (var wf in source.WorkflowTemplates)
        {
            var wfDto = new CreateWorkflowTemplateDto
            {
                ConfigurationId = created.Id,
                Name = wf.Name,
                Description = wf.Description,
                IsDefault = wf.IsDefault,
                OrderIndex = wf.OrderIndex
            };
            var createdWf = await _workflowService.CreateWorkflowAsync(wfDto, GetCurrentUser());

            foreach (var state in wf.States)
            {
                var stateDto = new CreateWorkflowStateTemplateDto
                {
                    WorkflowTemplateId = createdWf.Id,
                    Name = state.Name,
                    Description = state.Description,
                    IsInitialState = state.IsInitialState,
                    IsFinalState = state.IsFinalState,
                    Color = state.Color,
                    OrderIndex = state.OrderIndex
                };
                await _workflowStateService.CreateStateAsync(stateDto, GetCurrentUser());
            }
        }

        // Return the populated configuration
        var result = await _configService.GetConfigurationByIdAsync(created.Id);
        return result!;
    }

    // Helper method for comparing configurations
    private TemplateComparisonDto CompareConfigurations(GlobalConfigurationDetailDto t1, GlobalConfigurationDetailDto t2)
    {
        var differences = new List<ComparisonDifferenceDto>();

        // Compare roles
        var t1RoleNames = t1.RoleTemplates.Select(r => r.Name).ToHashSet();
        var t2RoleNames = t2.RoleTemplates.Select(r => r.Name).ToHashSet();
        
        foreach (var role in t1.RoleTemplates.Where(r => !t2RoleNames.Contains(r.Name)))
        {
            differences.Add(new ComparisonDifferenceDto
            {
                EntityType = "ROLE",
                DifferenceType = "REMOVED",
                EntityName = role.Name,
                Template1Value = role.Description,
                Template2Value = null
            });
        }
        
        foreach (var role in t2.RoleTemplates.Where(r => !t1RoleNames.Contains(r.Name)))
        {
            differences.Add(new ComparisonDifferenceDto
            {
                EntityType = "ROLE",
                DifferenceType = "ADDED",
                EntityName = role.Name,
                Template1Value = null,
                Template2Value = role.Description
            });
        }

        // Compare phases
        var t1PhaseNames = t1.PhaseTemplates.Select(p => p.Name).ToHashSet();
        var t2PhaseNames = t2.PhaseTemplates.Select(p => p.Name).ToHashSet();
        
        foreach (var phase in t1.PhaseTemplates.Where(p => !t2PhaseNames.Contains(p.Name)))
        {
            differences.Add(new ComparisonDifferenceDto
            {
                EntityType = "PHASE",
                DifferenceType = "REMOVED",
                EntityName = phase.Name,
                Template1Value = phase.Description,
                Template2Value = null
            });
        }
        
        foreach (var phase in t2.PhaseTemplates.Where(p => !t1PhaseNames.Contains(p.Name)))
        {
            differences.Add(new ComparisonDifferenceDto
            {
                EntityType = "PHASE",
                DifferenceType = "ADDED",
                EntityName = phase.Name,
                Template1Value = null,
                Template2Value = phase.Description
            });
        }

        // Compare artifact types
        var t1ATNames = t1.ArtifactTypeTemplates.Select(a => a.Name).ToHashSet();
        var t2ATNames = t2.ArtifactTypeTemplates.Select(a => a.Name).ToHashSet();
        
        foreach (var at in t1.ArtifactTypeTemplates.Where(a => !t2ATNames.Contains(a.Name)))
        {
            differences.Add(new ComparisonDifferenceDto
            {
                EntityType = "ARTIFACT_TYPE",
                DifferenceType = "REMOVED",
                EntityName = at.Name,
                Template1Value = at.Description,
                Template2Value = null
            });
        }
        
        foreach (var at in t2.ArtifactTypeTemplates.Where(a => !t1ATNames.Contains(a.Name)))
        {
            differences.Add(new ComparisonDifferenceDto
            {
                EntityType = "ARTIFACT_TYPE",
                DifferenceType = "ADDED",
                EntityName = at.Name,
                Template1Value = null,
                Template2Value = at.Description
            });
        }

        // Compare workflows
        var t1WFNames = t1.WorkflowTemplates.Select(w => w.Name).ToHashSet();
        var t2WFNames = t2.WorkflowTemplates.Select(w => w.Name).ToHashSet();
        
        foreach (var wf in t1.WorkflowTemplates.Where(w => !t2WFNames.Contains(w.Name)))
        {
            differences.Add(new ComparisonDifferenceDto
            {
                EntityType = "WORKFLOW",
                DifferenceType = "REMOVED",
                EntityName = wf.Name,
                Template1Value = wf.Description,
                Template2Value = null
            });
        }
        
        foreach (var wf in t2.WorkflowTemplates.Where(w => !t1WFNames.Contains(w.Name)))
        {
            differences.Add(new ComparisonDifferenceDto
            {
                EntityType = "WORKFLOW",
                DifferenceType = "ADDED",
                EntityName = wf.Name,
                Template1Value = null,
                Template2Value = wf.Description
            });
        }

        return new TemplateComparisonDto
        {
            Template1 = new TemplateMetadataDto
            {
                Id = t1.Id,
                Name = t1.Name,
                Description = t1.Description,
                Version = t1.Version,
                CreatedBy = t1.CreatedBy,
                CreatedAt = t1.CreatedAt,
                UpdatedAt = t1.UpdatedAt,
                RolesCount = t1.RoleTemplates.Count,
                PhasesCount = t1.PhaseTemplates.Count,
                ArtifactTypesCount = t1.ArtifactTypeTemplates.Count,
                WorkflowsCount = t1.WorkflowTemplates.Count
            },
            Template2 = new TemplateMetadataDto
            {
                Id = t2.Id,
                Name = t2.Name,
                Description = t2.Description,
                Version = t2.Version,
                CreatedBy = t2.CreatedBy,
                CreatedAt = t2.CreatedAt,
                UpdatedAt = t2.UpdatedAt,
                RolesCount = t2.RoleTemplates.Count,
                PhasesCount = t2.PhaseTemplates.Count,
                ArtifactTypesCount = t2.ArtifactTypeTemplates.Count,
                WorkflowsCount = t2.WorkflowTemplates.Count
            },
            Differences = differences,
            Summary = new TemplateComparisonSummaryDto
            {
                TotalDifferences = differences.Count,
                RolesDifferences = differences.Count(d => d.EntityType == "ROLE"),
                PhasesDifferences = differences.Count(d => d.EntityType == "PHASE"),
                ArtifactTypesDifferences = differences.Count(d => d.EntityType == "ARTIFACT_TYPE"),
                WorkflowsDifferences = differences.Count(d => d.EntityType == "WORKFLOW"),
                CustomFieldsDifferences = differences.Count(d => d.EntityType == "CUSTOM_FIELD"),
                AreIdentical = differences.Count == 0
            }
        };
    }
}
