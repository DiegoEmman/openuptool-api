using OpenUpTool.Core.DTOs;
using OpenUpTool.Core.Entities;
using OpenUpTool.Core.Interfaces;

namespace OpenUpTool.Infrastructure.Services;

public class WorkflowPermissionService : IWorkflowPermissionService
{
    private readonly IWorkflowPermissionRepository _permissionRepository;
    private readonly IWorkflowRepository _workflowRepository;

    public WorkflowPermissionService(
        IWorkflowPermissionRepository permissionRepository,
        IWorkflowRepository workflowRepository)
    {
        _permissionRepository = permissionRepository;
        _workflowRepository = workflowRepository;
    }

    public async Task<IEnumerable<WorkflowPermissionDto>> GetPermissionsByWorkflowIdAsync(Guid workflowId)
    {
        var permissions = await _permissionRepository.GetByWorkflowIdAsync(workflowId);
        return permissions.Select(MapToDto);
    }

    public async Task<WorkflowPermissionMatrixDto> GetPermissionMatrixAsync(Guid workflowId)
    {
        var workflow = await _workflowRepository.GetByIdAsync(workflowId);
        if (workflow == null)
            throw new InvalidOperationException("Workflow no encontrado");

        var permissions = await _permissionRepository.GetByWorkflowIdAsync(workflowId);
        
        // Agrupar permisos por rol
        var permissionsByRole = permissions
            .GroupBy(p => p.Role)
            .Select(g => new PermissionRoleDto
            {
                Role = g.Key,
                Actions = g.ToDictionary(p => p.Action, p => p.IsAllowed)
            })
            .ToList();

        return new WorkflowPermissionMatrixDto
        {
            WorkflowId = workflowId,
            WorkflowName = workflow.Name,
            Permissions = permissionsByRole
        };
    }

    public async Task<WorkflowPermissionDto> CreatePermissionAsync(CreateWorkflowPermissionDto dto)
    {
        // Verificar que el workflow existe
        var workflow = await _workflowRepository.GetByIdAsync(dto.WorkflowId);
        if (workflow == null)
            throw new InvalidOperationException("Workflow no encontrado");

        // Verificar si ya existe este permiso
        var existing = await _permissionRepository.GetByWorkflowRoleActionAsync(dto.WorkflowId, dto.Role, dto.Action);
        if (existing != null)
            throw new InvalidOperationException($"Ya existe un permiso para {dto.Role} - {dto.Action} en este workflow");

        var permission = new WorkflowPermission
        {
            Id = Guid.NewGuid(),
            WorkflowId = dto.WorkflowId,
            Role = dto.Role,
            Action = dto.Action,
            IsAllowed = dto.IsAllowed
        };

        var created = await _permissionRepository.CreateAsync(permission);
        return MapToDto(created);
    }

    public async Task<WorkflowPermissionDto?> UpdatePermissionAsync(Guid id, UpdateWorkflowPermissionDto dto)
    {
        var permission = await _permissionRepository.GetByIdAsync(id);
        if (permission == null) return null;

        permission.IsAllowed = dto.IsAllowed;
        
        var updated = await _permissionRepository.UpdateAsync(permission);
        return MapToDto(updated);
    }

    public async Task<bool> DeletePermissionAsync(Guid id)
    {
        var permission = await _permissionRepository.GetByIdAsync(id);
        if (permission == null) return false;

        await _permissionRepository.DeleteAsync(id);
        return true;
    }

    public async Task<bool> CheckPermissionAsync(Guid workflowId, string role, string action)
    {
        var permission = await _permissionRepository.GetByWorkflowRoleActionAsync(workflowId, role, action);
        return permission?.IsAllowed ?? false;
    }

    private static WorkflowPermissionDto MapToDto(WorkflowPermission permission)
    {
        return new WorkflowPermissionDto
        {
            Id = permission.Id,
            WorkflowId = permission.WorkflowId,
            Role = permission.Role,
            Action = permission.Action,
            IsAllowed = permission.IsAllowed,
            CreatedAt = permission.CreatedAt
        };
    }
}
