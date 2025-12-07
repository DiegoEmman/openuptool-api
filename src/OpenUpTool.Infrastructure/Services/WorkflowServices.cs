using OpenUpTool.Core.DTOs;
using OpenUpTool.Core.Entities;
using OpenUpTool.Core.Interfaces;

namespace OpenUpTool.Infrastructure.Services;

public class WorkflowService : IWorkflowService
{
    private readonly IWorkflowRepository _workflowRepository;
    private readonly IWorkflowStateRepository _stateRepository;
    private readonly IProjectRepository _projectRepository;

    public WorkflowService(
        IWorkflowRepository workflowRepository,
        IWorkflowStateRepository stateRepository,
        IProjectRepository projectRepository)
    {
        _workflowRepository = workflowRepository;
        _stateRepository = stateRepository;
        _projectRepository = projectRepository;
    }

    public async Task<IEnumerable<WorkflowDto>> GetAllWorkflowsAsync()
    {
        var workflows = await _workflowRepository.GetAllAsync();
        return workflows.Select(MapToDto);
    }

    public async Task<IEnumerable<WorkflowDto>> GetWorkflowsByProjectIdAsync(Guid projectId)
    {
        var workflows = await _workflowRepository.GetByProjectIdAsync(projectId);
        return workflows.Select(MapToDto);
    }

    public async Task<WorkflowWithStatesDto?> GetWorkflowByIdAsync(Guid id)
    {
        var workflow = await _workflowRepository.GetByIdWithStatesAsync(id);
        if (workflow == null) return null;

        return new WorkflowWithStatesDto
        {
            Id = workflow.Id,
            ProjectId = workflow.ProjectId,
            Name = workflow.Name,
            Description = workflow.Description,
            IsActive = workflow.IsActive,
            CreatedAt = workflow.CreatedAt,
            UpdatedAt = workflow.UpdatedAt,
            States = workflow.States.Select(MapStateToDto).ToList(),
            TotalArtifacts = workflow.Artifacts?.Count ?? 0
        };
    }

    public async Task<WorkflowDto> CreateWorkflowAsync(CreateWorkflowDto dto, Guid createdBy)
    {
        var project = await _projectRepository.GetByIdAsync(dto.ProjectId);
        if (project == null)
            throw new InvalidOperationException("Proyecto no encontrado");

        var workflow = new Workflow
        {
            Id = Guid.NewGuid(),
            ProjectId = dto.ProjectId,
            Name = dto.Name,
            Description = dto.Description,
            IsActive = true
        };

        var created = await _workflowRepository.CreateAsync(workflow);
        return MapToDto(created);
    }

    public async Task<WorkflowDto?> UpdateWorkflowAsync(Guid id, UpdateWorkflowDto dto)
    {
        var workflow = await _workflowRepository.GetByIdAsync(id);
        if (workflow == null) return null;

        workflow.Name = dto.Name;
        workflow.Description = dto.Description;
        workflow.IsActive = dto.IsActive;

        var updated = await _workflowRepository.UpdateAsync(workflow);
        return MapToDto(updated);
    }

    public async Task<bool> DeleteWorkflowAsync(Guid id)
    {
        var workflow = await _workflowRepository.GetByIdAsync(id);
        if (workflow == null) return false;

        await _workflowRepository.DeleteAsync(id);
        return true;
    }

    private static WorkflowDto MapToDto(Workflow workflow)
    {
        return new WorkflowDto
        {
            Id = workflow.Id,
            ProjectId = workflow.ProjectId,
            Name = workflow.Name,
            Description = workflow.Description,
            IsActive = workflow.IsActive,
            CreatedAt = workflow.CreatedAt,
            UpdatedAt = workflow.UpdatedAt,
            States = workflow.States?.Select(MapStateToDto).ToList() ?? new List<WorkflowStateDto>()
        };
    }

    private static WorkflowStateDto MapStateToDto(WorkflowState state)
    {
        return new WorkflowStateDto
        {
            Id = state.Id,
            WorkflowId = state.WorkflowId,
            Name = state.Name,
            Description = state.Description,
            Order = state.Order,
            Color = state.Color,
            IsInitialState = state.IsInitialState,
            IsFinalState = state.IsFinalState,
            RequiredActions = state.RequiredActions,
            CreatedAt = state.CreatedAt,
            UpdatedAt = state.UpdatedAt,
            Responsibles = state.Responsibles?.Select(r => new WorkflowStateResponsibleDto
            {
                Id = r.Id,
                WorkflowStateId = r.WorkflowStateId,
                UserId = r.UserId,
                UserName = r.User != null ? $"{r.User.FirstName} {r.User.LastName}" : string.Empty,
                UserEmail = r.User?.Email ?? string.Empty,
                Role = r.Role,
                AssignedAt = r.AssignedAt
            }).ToList() ?? new List<WorkflowStateResponsibleDto>()
        };
    }
}

public class WorkflowStateService : IWorkflowStateService
{
    private readonly IWorkflowStateRepository _stateRepository;
    private readonly IWorkflowStateResponsibleRepository _responsibleRepository;
    private readonly IWorkflowRepository _workflowRepository;

    public WorkflowStateService(
        IWorkflowStateRepository stateRepository,
        IWorkflowStateResponsibleRepository responsibleRepository,
        IWorkflowRepository workflowRepository)
    {
        _stateRepository = stateRepository;
        _responsibleRepository = responsibleRepository;
        _workflowRepository = workflowRepository;
    }

    public async Task<IEnumerable<WorkflowStateDto>> GetStatesByWorkflowIdAsync(Guid workflowId)
    {
        var states = await _stateRepository.GetByWorkflowIdAsync(workflowId);
        return states.Select(MapToDto);
    }

    public async Task<WorkflowStateDto?> GetStateByIdAsync(Guid id)
    {
        var state = await _stateRepository.GetByIdWithResponsiblesAsync(id);
        if (state == null) return null;
        return MapToDto(state);
    }

    public async Task<WorkflowStateDto> CreateStateAsync(CreateWorkflowStateDto dto)
    {
        var workflow = await _workflowRepository.GetByIdAsync(dto.WorkflowId);
        if (workflow == null)
            throw new InvalidOperationException("Workflow no encontrado");

        var state = new WorkflowState
        {
            Id = Guid.NewGuid(),
            WorkflowId = dto.WorkflowId,
            Name = dto.Name,
            Description = dto.Description,
            Order = dto.Order,
            Color = dto.Color,
            IsInitialState = dto.IsInitialState,
            IsFinalState = dto.IsFinalState,
            RequiredActions = dto.RequiredActions
        };

        var created = await _stateRepository.CreateAsync(state);
        return MapToDto(created);
    }

    public async Task<WorkflowStateDto?> UpdateStateAsync(Guid id, UpdateWorkflowStateDto dto)
    {
        var state = await _stateRepository.GetByIdAsync(id);
        if (state == null) return null;

        state.Name = dto.Name;
        state.Description = dto.Description;
        state.Order = dto.Order;
        state.Color = dto.Color;
        state.IsInitialState = dto.IsInitialState;
        state.IsFinalState = dto.IsFinalState;
        state.RequiredActions = dto.RequiredActions;

        var updated = await _stateRepository.UpdateAsync(state);
        return MapToDto(updated);
    }

    public async Task<bool> DeleteStateAsync(Guid id)
    {
        var state = await _stateRepository.GetByIdAsync(id);
        if (state == null) return false;

        await _stateRepository.DeleteAsync(id);
        return true;
    }

    public async Task<WorkflowStateResponsibleDto> AddResponsibleAsync(CreateWorkflowStateResponsibleDto dto)
    {
        var state = await _stateRepository.GetByIdAsync(dto.WorkflowStateId);
        if (state == null)
            throw new InvalidOperationException("Estado no encontrado");

        var responsible = new WorkflowStateResponsible
        {
            Id = Guid.NewGuid(),
            WorkflowStateId = dto.WorkflowStateId,
            UserId = dto.UserId,
            Role = dto.Role
        };

        var created = await _responsibleRepository.CreateAsync(responsible);
        
        return new WorkflowStateResponsibleDto
        {
            Id = created.Id,
            WorkflowStateId = created.WorkflowStateId,
            UserId = created.UserId,
            UserName = created.User != null ? $"{created.User.FirstName} {created.User.LastName}" : string.Empty,
            UserEmail = created.User?.Email ?? string.Empty,
            Role = created.Role,
            AssignedAt = created.AssignedAt
        };
    }

    public async Task<bool> RemoveResponsibleAsync(Guid responsibleId)
    {
        var responsible = await _responsibleRepository.GetByIdAsync(responsibleId);
        if (responsible == null) return false;

        await _responsibleRepository.DeleteAsync(responsibleId);
        return true;
    }

    private static WorkflowStateDto MapToDto(WorkflowState state)
    {
        return new WorkflowStateDto
        {
            Id = state.Id,
            WorkflowId = state.WorkflowId,
            Name = state.Name,
            Description = state.Description,
            Order = state.Order,
            Color = state.Color,
            IsInitialState = state.IsInitialState,
            IsFinalState = state.IsFinalState,
            RequiredActions = state.RequiredActions,
            CreatedAt = state.CreatedAt,
            UpdatedAt = state.UpdatedAt,
            Responsibles = state.Responsibles?.Select(r => new WorkflowStateResponsibleDto
            {
                Id = r.Id,
                WorkflowStateId = r.WorkflowStateId,
                UserId = r.UserId,
                UserName = r.User != null ? $"{r.User.FirstName} {r.User.LastName}" : string.Empty,
                UserEmail = r.User?.Email ?? string.Empty,
                Role = r.Role,
                AssignedAt = r.AssignedAt
            }).ToList() ?? new List<WorkflowStateResponsibleDto>()
        };
    }
}

public class ArtifactStateService : IArtifactStateService
{
    private readonly IArtifactStateHistoryRepository _historyRepository;
    private readonly IArtifactRepository _artifactRepository;
    private readonly IWorkflowStateRepository _stateRepository;

    public ArtifactStateService(
        IArtifactStateHistoryRepository historyRepository,
        IArtifactRepository artifactRepository,
        IWorkflowStateRepository stateRepository)
    {
        _historyRepository = historyRepository;
        _artifactRepository = artifactRepository;
        _stateRepository = stateRepository;
    }

    public async Task<IEnumerable<ArtifactStateHistoryDto>> GetArtifactHistoryAsync(Guid artifactId)
    {
        var history = await _historyRepository.GetByArtifactIdAsync(artifactId);
        return history.Select(h => new ArtifactStateHistoryDto
        {
            Id = h.Id,
            ArtifactId = h.ArtifactId,
            FromStateId = h.FromStateId,
            FromStateName = h.FromState?.Name,
            ToStateId = h.ToStateId,
            ToStateName = h.ToState?.Name ?? string.Empty,
            ChangedByUserId = h.ChangedByUserId,
            ChangedByUserName = h.ChangedByUser != null ? $"{h.ChangedByUser.FirstName} {h.ChangedByUser.LastName}" : string.Empty,
            ChangedAt = h.ChangedAt,
            Comments = h.Comments,
            Metadata = h.Metadata
        });
    }

    public async Task<ArtifactWithWorkflowDto?> GetArtifactWithWorkflowAsync(Guid artifactId)
    {
        var artifact = await _artifactRepository.GetByIdAsync(artifactId);
        if (artifact == null) return null;

        var history = await _historyRepository.GetByArtifactIdAsync(artifactId);

        return new ArtifactWithWorkflowDto
        {
            Id = artifact.Id,
            Title = artifact.Title,
            WorkflowId = artifact.WorkflowId,
            WorkflowName = artifact.Workflow?.Name,
            CurrentStateId = artifact.CurrentStateId,
            CurrentStateName = artifact.CurrentState?.Name,
            CurrentStateColor = artifact.CurrentState?.Color,
            StateHistory = history.Select(h => new ArtifactStateHistoryDto
            {
                Id = h.Id,
                ArtifactId = h.ArtifactId,
                FromStateId = h.FromStateId,
                FromStateName = h.FromState?.Name,
                ToStateId = h.ToStateId,
                ToStateName = h.ToState?.Name ?? string.Empty,
                ChangedByUserId = h.ChangedByUserId,
                ChangedByUserName = h.ChangedByUser != null ? $"{h.ChangedByUser.FirstName} {h.ChangedByUser.LastName}" : string.Empty,
                ChangedAt = h.ChangedAt,
                Comments = h.Comments,
                Metadata = h.Metadata
            }).ToList()
        };
    }

    public async Task<ArtifactStateHistoryDto> ChangeArtifactStateAsync(ChangeArtifactStateDto dto, Guid changedBy)
    {
        var artifact = await _artifactRepository.GetByIdAsync(dto.ArtifactId);
        if (artifact == null)
            throw new InvalidOperationException("Artefacto no encontrado");

        var newState = await _stateRepository.GetByIdAsync(dto.ToStateId);
        if (newState == null)
            throw new InvalidOperationException("Estado destino no encontrado");

        // Crear registro de historial
        var history = new ArtifactStateHistory
        {
            Id = Guid.NewGuid(),
            ArtifactId = dto.ArtifactId,
            FromStateId = artifact.CurrentStateId,
            ToStateId = dto.ToStateId,
            ChangedByUserId = changedBy,
            Comments = dto.Comments,
            Metadata = dto.Metadata
        };

        var created = await _historyRepository.CreateAsync(history);

        // Actualizar estado actual del artefacto
        artifact.CurrentStateId = dto.ToStateId;
        await _artifactRepository.UpdateAsync(artifact);

        // Cargar relaciones para el DTO
        var historyWithRelations = await _historyRepository.GetByIdAsync(created.Id);

        return new ArtifactStateHistoryDto
        {
            Id = historyWithRelations!.Id,
            ArtifactId = historyWithRelations.ArtifactId,
            FromStateId = historyWithRelations.FromStateId,
            FromStateName = historyWithRelations.FromState?.Name,
            ToStateId = historyWithRelations.ToStateId,
            ToStateName = historyWithRelations.ToState?.Name ?? string.Empty,
            ChangedByUserId = historyWithRelations.ChangedByUserId,
            ChangedByUserName = historyWithRelations.ChangedByUser != null ? $"{historyWithRelations.ChangedByUser.FirstName} {historyWithRelations.ChangedByUser.LastName}" : string.Empty,
            ChangedAt = historyWithRelations.ChangedAt,
            Comments = historyWithRelations.Comments,
            Metadata = historyWithRelations.Metadata
        };
    }

    public async Task<ArtifactDto?> AssignWorkflowToArtifactAsync(Guid artifactId, Guid workflowId)
    {
        var artifact = await _artifactRepository.GetByIdAsync(artifactId);
        if (artifact == null) return null;

        artifact.WorkflowId = workflowId;
        artifact.CurrentStateId = null;
        
        var updated = await _artifactRepository.UpdateAsync(artifact);
        
        return new ArtifactDto(
            Id: updated.Id,
            ProjectId: updated.ProjectId,
            PhaseId: updated.PhaseId,
            ArtifactTypeId: updated.ArtifactTypeId,
            Title: updated.Title,
            Description: updated.Description,
            Author: updated.Author,
            CreatedAt: updated.CreatedAt,
            Status: updated.Status,
            IsMandatory: updated.IsMandatory,
            ContentText: updated.ContentText,
            FilePath: updated.FilePath,
            FileName: updated.FileName,
            FileSize: updated.FileSize,
            MimeType: updated.MimeType,
            FileCategory: updated.FileCategory,
            RepositoryUrl: updated.RepositoryUrl,
            RepositoryVersion: updated.RepositoryVersion,
            BuildNumber: updated.BuildNumber,
            TestCases: null,
            TestResults: null,
            IterationActivities: null
        );
    }
}
