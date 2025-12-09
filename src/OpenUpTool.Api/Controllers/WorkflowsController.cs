using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenUpTool.Api.Attributes;
using OpenUpTool.Core.DTOs;
using OpenUpTool.Core.Interfaces;
using System.Security.Claims;

namespace OpenUpTool.Api.Controllers;

/// <summary>
/// Controlador para gestión de Workflows (Flujos de trabajo)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WorkflowsController : ControllerBase
{
    private readonly IWorkflowService _workflowService;
    private readonly IWorkflowStateService _stateService;
    private readonly IArtifactStateService _artifactStateService;
    private readonly IWorkflowPermissionService _permissionService;
    private readonly ILogger<WorkflowsController> _logger;

    public WorkflowsController(
        IWorkflowService workflowService,
        IWorkflowStateService stateService,
        IArtifactStateService artifactStateService,
        IWorkflowPermissionService permissionService,
        ILogger<WorkflowsController> logger)
    {
        _workflowService = workflowService;
        _stateService = stateService;
        _artifactStateService = artifactStateService;
        _permissionService = permissionService;
        _logger = logger;
    }

    /// <summary>
    /// Obtener todos los workflows
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<WorkflowDto>>> GetAll()
    {
        try
        {
            var workflows = await _workflowService.GetAllWorkflowsAsync();
            return Ok(workflows);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener workflows");
            return StatusCode(500, new { message = "Error al obtener workflows", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtener workflows por proyecto
    /// </summary>
    [HttpGet("project/{projectId}")]
    public async Task<ActionResult<IEnumerable<WorkflowDto>>> GetByProject(Guid projectId)
    {
        try
        {
            var workflows = await _workflowService.GetWorkflowsByProjectIdAsync(projectId);
            return Ok(workflows);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener workflows del proyecto {ProjectId}", projectId);
            return StatusCode(500, new { message = "Error al obtener workflows del proyecto", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtener workflow por ID (con estados)
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<WorkflowWithStatesDto>> GetById(Guid id)
    {
        try
        {
            var workflow = await _workflowService.GetWorkflowByIdAsync(id);
            if (workflow == null)
                return NotFound(new { message = "Workflow no encontrado" });

            return Ok(workflow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener workflow {WorkflowId}", id);
            return StatusCode(500, new { message = "Error al obtener workflow", error = ex.Message });
        }
    }

    /// <summary>
    /// Crear nuevo workflow
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<WorkflowDto>> Create([FromBody] CreateWorkflowDto dto)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
            var workflow = await _workflowService.CreateWorkflowAsync(dto, userId);
            return CreatedAtAction(nameof(GetById), new { id = workflow.Id }, workflow);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear workflow");
            return StatusCode(500, new { message = "Error al crear workflow", error = ex.Message });
        }
    }

    /// <summary>
    /// Actualizar workflow
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<WorkflowDto>> Update(Guid id, [FromBody] UpdateWorkflowDto dto)
    {
        try
        {
            var workflow = await _workflowService.UpdateWorkflowAsync(id, dto);
            if (workflow == null)
                return NotFound(new { message = "Workflow no encontrado" });

            return Ok(workflow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar workflow {WorkflowId}", id);
            return StatusCode(500, new { message = "Error al actualizar workflow", error = ex.Message });
        }
    }

    /// <summary>
    /// Eliminar workflow
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var result = await _workflowService.DeleteWorkflowAsync(id);
            if (!result)
                return NotFound(new { message = "Workflow no encontrado" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar workflow {WorkflowId}", id);
            return StatusCode(500, new { message = "Error al eliminar workflow", error = ex.Message });
        }
    }

    // ========== WORKFLOW STATES ENDPOINTS ==========

    /// <summary>
    /// Obtener estados de un workflow
    /// </summary>
    [HttpGet("{workflowId}/states")]
    public async Task<ActionResult<IEnumerable<WorkflowStateDto>>> GetStates(Guid workflowId)
    {
        try
        {
            var states = await _stateService.GetStatesByWorkflowIdAsync(workflowId);
            return Ok(states);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener estados del workflow {WorkflowId}", workflowId);
            return StatusCode(500, new { message = "Error al obtener estados", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtener estado por ID
    /// </summary>
    [HttpGet("states/{stateId}")]
    public async Task<ActionResult<WorkflowStateDto>> GetStateById(Guid stateId)
    {
        try
        {
            var state = await _stateService.GetStateByIdAsync(stateId);
            if (state == null)
                return NotFound(new { message = "Estado no encontrado" });

            return Ok(state);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener estado {StateId}", stateId);
            return StatusCode(500, new { message = "Error al obtener estado", error = ex.Message });
        }
    }

    /// <summary>
    /// Crear nuevo estado en un workflow
    /// </summary>
    [HttpPost("states")]
    public async Task<ActionResult<WorkflowStateDto>> CreateState([FromBody] CreateWorkflowStateDto dto)
    {
        try
        {
            var state = await _stateService.CreateStateAsync(dto);
            return CreatedAtAction(nameof(GetStateById), new { stateId = state.Id }, state);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear estado");
            return StatusCode(500, new { message = "Error al crear estado", error = ex.Message });
        }
    }

    /// <summary>
    /// Actualizar estado
    /// </summary>
    [HttpPut("states/{stateId}")]
    public async Task<ActionResult<WorkflowStateDto>> UpdateState(Guid stateId, [FromBody] UpdateWorkflowStateDto dto)
    {
        try
        {
            var state = await _stateService.UpdateStateAsync(stateId, dto);
            if (state == null)
                return NotFound(new { message = "Estado no encontrado" });

            return Ok(state);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar estado {StateId}", stateId);
            return StatusCode(500, new { message = "Error al actualizar estado", error = ex.Message });
        }
    }

    /// <summary>
    /// Eliminar estado
    /// </summary>
    [HttpDelete("states/{stateId}")]
    public async Task<IActionResult> DeleteState(Guid stateId)
    {
        try
        {
            var result = await _stateService.DeleteStateAsync(stateId);
            if (!result)
                return NotFound(new { message = "Estado no encontrado" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar estado {StateId}", stateId);
            return StatusCode(500, new { message = "Error al eliminar estado", error = ex.Message });
        }
    }

    // ========== RESPONSIBLES ENDPOINTS ==========

    /// <summary>
    /// Agregar responsable a un estado
    /// </summary>
    [HttpPost("states/responsibles")]
    public async Task<ActionResult<WorkflowStateResponsibleDto>> AddResponsible([FromBody] CreateWorkflowStateResponsibleDto dto)
    {
        try
        {
            var responsible = await _stateService.AddResponsibleAsync(dto);
            return Ok(responsible);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al agregar responsable");
            return StatusCode(500, new { message = "Error al agregar responsable", error = ex.Message });
        }
    }

    /// <summary>
    /// Eliminar responsable
    /// </summary>
    [HttpDelete("states/responsibles/{responsibleId}")]
    public async Task<IActionResult> RemoveResponsible(Guid responsibleId)
    {
        try
        {
            var result = await _stateService.RemoveResponsibleAsync(responsibleId);
            if (!result)
                return NotFound(new { message = "Responsable no encontrado" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar responsable {ResponsibleId}", responsibleId);
            return StatusCode(500, new { message = "Error al eliminar responsable", error = ex.Message });
        }
    }

    // ========== ARTIFACT STATE ENDPOINTS ==========

    /// <summary>
    /// Obtener artefacto con información de workflow
    /// </summary>
    [HttpGet("artifacts/{artifactId}/workflow")]
    public async Task<ActionResult<ArtifactWithWorkflowDto>> GetArtifactWithWorkflow(Guid artifactId)
    {
        try
        {
            var artifact = await _artifactStateService.GetArtifactWithWorkflowAsync(artifactId);
            if (artifact == null)
                return NotFound(new { message = "Artefacto no encontrado" });

            return Ok(artifact);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener artefacto {ArtifactId} con workflow", artifactId);
            return StatusCode(500, new { message = "Error al obtener artefacto con workflow", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtener historial de estados de un artefacto
    /// </summary>
    [HttpGet("artifacts/{artifactId}/history")]
    public async Task<ActionResult<IEnumerable<ArtifactStateHistoryDto>>> GetArtifactHistory(Guid artifactId)
    {
        try
        {
            var history = await _artifactStateService.GetArtifactHistoryAsync(artifactId);
            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener historial del artefacto {ArtifactId}", artifactId);
            return StatusCode(500, new { message = "Error al obtener historial", error = ex.Message });
        }
    }

    /// <summary>
    /// Cambiar estado de un artefacto
    /// Requiere permiso de 'cambiar_estado' en el workflow asociado
    /// </summary>
    [HttpPost("artifacts/change-state")]
    [RequireWorkflowPermission("cambiar_estado")]
    public async Task<ActionResult<ArtifactStateHistoryDto>> ChangeArtifactState([FromBody] ChangeArtifactStateDto dto)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
            var history = await _artifactStateService.ChangeArtifactStateAsync(dto, userId);
            return Ok(history);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cambiar estado del artefacto");
            return StatusCode(500, new { message = "Error al cambiar estado", error = ex.Message });
        }
    }

    /// <summary>
    /// Asignar workflow a un artefacto
    /// </summary>
    [HttpPost("artifacts/{artifactId}/assign-workflow/{workflowId}")]
    public async Task<ActionResult<ArtifactDto>> AssignWorkflowToArtifact(Guid artifactId, Guid workflowId)
    {
        try
        {
            var artifact = await _artifactStateService.AssignWorkflowToArtifactAsync(artifactId, workflowId);
            if (artifact == null)
                return NotFound(new { message = "Artefacto no encontrado" });

            return Ok(artifact);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al asignar workflow {WorkflowId} al artefacto {ArtifactId}", workflowId, artifactId);
            return StatusCode(500, new { message = "Error al asignar workflow", error = ex.Message });
        }
    }

    // ========== WORKFLOW PERMISSIONS ENDPOINTS ==========

    /// <summary>
    /// Obtener matriz de permisos de un workflow
    /// </summary>
    [HttpGet("{workflowId}/permissions/matrix")]
    public async Task<ActionResult<WorkflowPermissionMatrixDto>> GetPermissionMatrix(Guid workflowId)
    {
        try
        {
            var matrix = await _permissionService.GetPermissionMatrixAsync(workflowId);
            return Ok(matrix);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener matriz de permisos");
            return StatusCode(500, new { message = "Error al obtener matriz de permisos", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtener todos los permisos de un workflow
    /// </summary>
    [HttpGet("{workflowId}/permissions")]
    public async Task<ActionResult<IEnumerable<WorkflowPermissionDto>>> GetPermissions(Guid workflowId)
    {
        try
        {
            var permissions = await _permissionService.GetPermissionsByWorkflowIdAsync(workflowId);
            return Ok(permissions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener permisos");
            return StatusCode(500, new { message = "Error al obtener permisos", error = ex.Message });
        }
    }

    /// <summary>
    /// Crear un nuevo permiso
    /// </summary>
    [HttpPost("permissions")]
    public async Task<ActionResult<WorkflowPermissionDto>> CreatePermission([FromBody] CreateWorkflowPermissionDto dto)
    {
        try
        {
            var permission = await _permissionService.CreatePermissionAsync(dto);
            return CreatedAtAction(nameof(GetPermissions), new { workflowId = dto.WorkflowId }, permission);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear permiso");
            return StatusCode(500, new { message = "Error al crear permiso", error = ex.Message });
        }
    }

    /// <summary>
    /// Actualizar un permiso existente
    /// </summary>
    [HttpPut("permissions/{permissionId}")]
    public async Task<ActionResult<WorkflowPermissionDto>> UpdatePermission(Guid permissionId, [FromBody] UpdateWorkflowPermissionDto dto)
    {
        try
        {
            var permission = await _permissionService.UpdatePermissionAsync(permissionId, dto);
            if (permission == null)
                return NotFound(new { message = "Permiso no encontrado" });

            return Ok(permission);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar permiso");
            return StatusCode(500, new { message = "Error al actualizar permiso", error = ex.Message });
        }
    }

    /// <summary>
    /// Eliminar un permiso
    /// </summary>
    [HttpDelete("permissions/{permissionId}")]
    public async Task<IActionResult> DeletePermission(Guid permissionId)
    {
        try
        {
            var deleted = await _permissionService.DeletePermissionAsync(permissionId);
            if (!deleted)
                return NotFound(new { message = "Permiso no encontrado" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar permiso");
            return StatusCode(500, new { message = "Error al eliminar permiso", error = ex.Message });
        }
    }

    /// <summary>
    /// Verificar si un rol tiene permiso para una acción
    /// </summary>
    [HttpGet("{workflowId}/permissions/check")]
    public async Task<ActionResult<bool>> CheckPermission(Guid workflowId, [FromQuery] string role, [FromQuery] string action)
    {
        try
        {
            var hasPermission = await _permissionService.CheckPermissionAsync(workflowId, role, action);
            return Ok(new { hasPermission, workflowId, role, action });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar permiso");
            return StatusCode(500, new { message = "Error al verificar permiso", error = ex.Message });
        }
    }

    // ========== PERMISSION TESTING ENDPOINTS ==========

    /// <summary>
    /// [TEST] Endpoint de prueba para acción "aprobar" - Requiere permiso
    /// </summary>
    [HttpPost("{workflowId}/test-approve")]
    [RequireWorkflowPermission("aprobar")]
    public ActionResult TestApproveAction(Guid workflowId)
    {
        try
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            return Ok(new
            {
                message = "Acción de aprobar ejecutada exitosamente",
                workflow = workflowId,
                userRole,
                action = "aprobar"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al ejecutar test de aprobar");
            return StatusCode(500, new { message = "Error al ejecutar test", error = ex.Message });
        }
    }

    /// <summary>
    /// [TEST] Endpoint de prueba para acción "crear" - Requiere permiso
    /// </summary>
    [HttpPost("{workflowId}/test-create")]
    [RequireWorkflowPermission("crear")]
    public ActionResult TestCreateAction(Guid workflowId)
    {
        try
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            return Ok(new
            {
                message = "Acción de crear ejecutada exitosamente",
                workflow = workflowId,
                userRole,
                action = "crear"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al ejecutar test de crear");
            return StatusCode(500, new { message = "Error al ejecutar test", error = ex.Message });
        }
    }

    /// <summary>
    /// [TEST] Endpoint de prueba para acción "editar" - Requiere permiso
    /// </summary>
    [HttpPost("{workflowId}/test-edit")]
    [RequireWorkflowPermission("editar")]
    public ActionResult TestEditAction(Guid workflowId)
    {
        try
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            return Ok(new
            {
                message = "Acción de editar ejecutada exitosamente",
                workflow = workflowId,
                userRole,
                action = "editar"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al ejecutar test de editar");
            return StatusCode(500, new { message = "Error al ejecutar test", error = ex.Message });
        }
    }
}
