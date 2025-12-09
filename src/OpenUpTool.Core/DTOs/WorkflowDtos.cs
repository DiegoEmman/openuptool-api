namespace OpenUpTool.Core.DTOs;

// ========== Workflow DTOs ==========

public class WorkflowDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<WorkflowStateDto> States { get; set; } = new();
}

public class CreateWorkflowDto
{
    public Guid ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UpdateWorkflowDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

// ========== WorkflowState DTOs ==========

public class WorkflowStateDto
{
    public Guid Id { get; set; }
    public Guid WorkflowId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Order { get; set; }
    public string? Color { get; set; }
    public bool IsInitialState { get; set; }
    public bool IsFinalState { get; set; }
    public string? RequiredActions { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<WorkflowStateResponsibleDto> Responsibles { get; set; } = new();
}

public class CreateWorkflowStateDto
{
    public Guid WorkflowId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Order { get; set; }
    public string? Color { get; set; }
    public bool IsInitialState { get; set; }
    public bool IsFinalState { get; set; }
    public string? RequiredActions { get; set; }
}

public class UpdateWorkflowStateDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Order { get; set; }
    public string? Color { get; set; }
    public bool IsInitialState { get; set; }
    public bool IsFinalState { get; set; }
    public string? RequiredActions { get; set; }
}

// ========== WorkflowStateResponsible DTOs ==========

public class WorkflowStateResponsibleDto
{
    public Guid Id { get; set; }
    public Guid WorkflowStateId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string? Role { get; set; }
    public DateTime AssignedAt { get; set; }
}

public class CreateWorkflowStateResponsibleDto
{
    public Guid WorkflowStateId { get; set; }
    public Guid? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string? Role { get; set; }
}

// ========== ArtifactStateHistory DTOs ==========

public class ArtifactStateHistoryDto
{
    public Guid Id { get; set; }
    public Guid ArtifactId { get; set; }
    public Guid? FromStateId { get; set; }
    public string? FromStateName { get; set; }
    public Guid ToStateId { get; set; }
    public string ToStateName { get; set; } = string.Empty;
    public Guid ChangedByUserId { get; set; }
    public string ChangedByUserName { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }
    public string? Comments { get; set; }
    public string? Metadata { get; set; }
}

public class ChangeArtifactStateDto
{
    public Guid ArtifactId { get; set; }
    public Guid ToStateId { get; set; }
    public string? Comments { get; set; }
    public string? Metadata { get; set; }
}

// ========== Combined DTOs ==========

public class WorkflowWithStatesDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<WorkflowStateDto> States { get; set; } = new();
    public int TotalArtifacts { get; set; }
}

public class ArtifactWithWorkflowDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public Guid? WorkflowId { get; set; }
    public string? WorkflowName { get; set; }
    public Guid? CurrentStateId { get; set; }
    public string? CurrentStateName { get; set; }
    public string? CurrentStateColor { get; set; }
    public List<ArtifactStateHistoryDto> StateHistory { get; set; } = new();
}

// ========== WorkflowPermission DTOs ==========

public class WorkflowPermissionDto
{
    public Guid Id { get; set; }
    public Guid WorkflowId { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public bool IsAllowed { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateWorkflowPermissionDto
{
    public Guid WorkflowId { get; set; }
    public string Role { get; set; } = string.Empty; // autor, revisor, PO, admin
    public string Action { get; set; } = string.Empty; // crear, editar, aprobar, cambiar_estado
    public bool IsAllowed { get; set; } = true;
}

public class UpdateWorkflowPermissionDto
{
    public bool IsAllowed { get; set; }
}

public class WorkflowPermissionMatrixDto
{
    public Guid WorkflowId { get; set; }
    public string WorkflowName { get; set; } = string.Empty;
    public List<PermissionRoleDto> Permissions { get; set; } = new();
}

public class PermissionRoleDto
{
    public string Role { get; set; } = string.Empty;
    public Dictionary<string, bool> Actions { get; set; } = new();
}
