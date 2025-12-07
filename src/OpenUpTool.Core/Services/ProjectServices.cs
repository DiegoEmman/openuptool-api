using OpenUpTool.Core.DTOs;
using OpenUpTool.Core.Entities;
using OpenUpTool.Core.Interfaces;

namespace OpenUpTool.Core.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IPhaseRepository _phaseRepository;
    private readonly IProjectUserRoleRepository _projectUserRoleRepository;

    public ProjectService(
        IProjectRepository projectRepository, 
        IPhaseRepository phaseRepository,
        IProjectUserRoleRepository projectUserRoleRepository)
    {
        _projectRepository = projectRepository;
        _phaseRepository = phaseRepository;
        _projectUserRoleRepository = projectUserRoleRepository;
    }

    public async Task<IEnumerable<ProjectDto>> GetAllProjectsAsync()
    {
        var projects = await _projectRepository.GetAllAsync();
        return projects.Select(MapToDto);
    }

    public async Task<IEnumerable<ProjectDto>> GetProjectsForUserAsync(Guid userId)
    {
        var projects = await _projectRepository.GetProjectsForUserAsync(userId);
        return projects.Select(MapToDto);
    }

    public async Task<ProjectDto?> GetProjectByIdAsync(Guid id)
    {
        var project = await _projectRepository.GetByIdAsync(id);
        return project == null ? null : MapToDto(project);
    }

    public async Task<bool> HasUserAccessAsync(Guid userId, Guid projectId)
    {
        return await _projectUserRoleRepository.HasUserAccessToProjectAsync(userId, projectId);
    }

    public async Task<ProjectDto> CreateProjectAsync(CreateProjectDto dto, Guid createdBy)
    {
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = dto.Name.Trim(),
            Identifier = dto.Identifier.Trim(),
            StartDate = DateTime.SpecifyKind(dto.StartDate, DateTimeKind.Utc),
            Status = "Creado",
            Owner = dto.Owner?.Trim(),
            Description = dto.Description?.Trim(),
            Tags = dto.Tags ?? new List<string>(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdProject = await _projectRepository.CreateAsync(project);

        // Crear las 4 fases estándar de OpenUP
        var now = DateTime.UtcNow;
        var phases = new[]
        {
            new Phase { Id = Guid.NewGuid(), ProjectId = createdProject.Id, PhaseCode = "INCEPTION", Name = "Incepción", Status = "PENDING", OrderIndex = 1, CreatedAt = now, UpdatedAt = now },
            new Phase { Id = Guid.NewGuid(), ProjectId = createdProject.Id, PhaseCode = "ELABORATION", Name = "Elaboración", Status = "PENDING", OrderIndex = 2, CreatedAt = now, UpdatedAt = now },
            new Phase { Id = Guid.NewGuid(), ProjectId = createdProject.Id, PhaseCode = "CONSTRUCTION", Name = "Construcción", Status = "PENDING", OrderIndex = 3, CreatedAt = now, UpdatedAt = now },
            new Phase { Id = Guid.NewGuid(), ProjectId = createdProject.Id, PhaseCode = "TRANSITION", Name = "Transición", Status = "PENDING", OrderIndex = 4, CreatedAt = now, UpdatedAt = now }
        };

        await _phaseRepository.CreateManyAsync(phases);

        // Asignar al creador como Manager del proyecto
        var managerRoleId = Guid.Parse("22222222-2222-2222-2222-222222222222"); // ID del rol Manager
        var projectUserRole = new ProjectUserRole
        {
            Id = Guid.NewGuid(),
            ProjectId = createdProject.Id,
            UserId = createdBy,
            RoleId = managerRoleId,
            InvitedBy = createdBy,
            InvitedAt = now,
            AcceptedAt = now,
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now
        };

        await _projectUserRoleRepository.CreateAsync(projectUserRole);

        // Recargar el proyecto con las fases
        var projectWithPhases = await _projectRepository.GetByIdAsync(createdProject.Id);
        return MapToDto(projectWithPhases!);
    }

    public async Task<ProjectDto?> UpdateProjectAsync(Guid id, UpdateProjectDto dto)
    {
        var project = await _projectRepository.GetByIdAsync(id);
        if (project == null) return null;

        if (dto.Name != null) project.Name = dto.Name.Trim();
        if (dto.Status != null) project.Status = dto.Status;
        if (dto.Owner != null) project.Owner = dto.Owner.Trim();
        if (dto.Description != null) project.Description = dto.Description.Trim();
        if (dto.Tags != null) project.Tags = dto.Tags;

        var updated = await _projectRepository.UpdateAsync(project);
        return MapToDto(updated);
    }

    public async Task DeleteProjectAsync(Guid id)
    {
        await _projectRepository.DeleteAsync(id);
    }

    public async Task<ProjectDto?> ArchiveProjectAsync(Guid id, Guid archivedBy)
    {
        var project = await _projectRepository.GetByIdAsync(id);
        if (project == null) return null;

        project.IsArchived = true;
        project.ArchivedAt = DateTime.UtcNow;
        project.ArchivedBy = archivedBy;
        project.UpdatedAt = DateTime.UtcNow;

        var updated = await _projectRepository.UpdateAsync(project);
        return MapToDto(updated);
    }

    public async Task<ProjectDto?> UnarchiveProjectAsync(Guid id)
    {
        var project = await _projectRepository.GetByIdAsync(id);
        if (project == null) return null;

        project.IsArchived = false;
        project.ArchivedAt = null;
        project.ArchivedBy = null;
        project.UpdatedAt = DateTime.UtcNow;

        var updated = await _projectRepository.UpdateAsync(project);
        return MapToDto(updated);
    }

    public async Task<IEnumerable<ProjectDto>> GetArchivedProjectsForUserAsync(Guid userId)
    {
        var projects = await _projectRepository.GetArchivedProjectsForUserAsync(userId);
        return projects.Select(MapToDto);
    }

    public async Task<bool> DeleteProjectPermanentlyAsync(Guid id, Guid deletedBy)
    {
        var project = await _projectRepository.GetByIdAsync(id);
        if (project == null) return false;

        await _projectRepository.DeleteAsync(id);
        return true;
    }

    public async Task<IEnumerable<UserDto>> GetProjectUsersAsync(Guid projectId)
    {
        var userRoles = await _projectUserRoleRepository.GetByProjectIdAsync(projectId);
        var activeUserRoles = userRoles.Where(ur => ur.Status == "active" && ur.User != null).ToList();
        
        return activeUserRoles.Select(ur => new UserDto(
            ur.UserId,
            ur.User!.Email,
            ur.User.FirstName,
            ur.User.LastName,
            ur.Role?.Name ?? "",
            ur.User.IsActive,
            ur.User.CreatedAt
        )).GroupBy(u => u.Id).Select(g => g.First()).ToList();
    }

    private static ProjectDto MapToDto(Project project)
    {
        return new ProjectDto(
            project.Id,
            project.Name,
            project.Identifier,
            project.StartDate,
            project.Status,
            project.Owner,
            project.Description,
            project.Tags,
            project.PlanId,
            project.Phases?.Select(p => p.PhaseCode).ToList() ?? new List<string> { "INCEPTION", "ELABORATION", "CONSTRUCTION", "TRANSITION" },
            project.CreatedAt,
            project.UpdatedAt
        );
    }
}

public class PhaseService : IPhaseService
{
    private readonly IPhaseRepository _phaseRepository;

    public PhaseService(IPhaseRepository phaseRepository)
    {
        _phaseRepository = phaseRepository;
    }

    public async Task<IEnumerable<PhaseDto>> GetPhasesByProjectAsync(Guid projectId)
    {
        var phases = await _phaseRepository.GetByProjectIdAsync(projectId);
        return phases.Select(MapToDto);
    }

    public async Task<PhaseDto?> GetPhaseByIdAsync(Guid id)
    {
        var phase = await _phaseRepository.GetByIdAsync(id);
        return phase == null ? null : MapToDto(phase);
    }

    public async Task<PhaseDto?> GetPhaseByCodeAsync(Guid projectId, string phaseCode)
    {
        var phase = await _phaseRepository.GetByProjectAndCodeAsync(projectId, phaseCode);
        return phase == null ? null : MapToDto(phase);
    }

    public async Task<PhaseDto?> UpdatePhaseAsync(Guid id, UpdatePhaseDto dto)
    {
        var phase = await _phaseRepository.GetByIdAsync(id);
        if (phase == null) return null;

        if (dto.Name != null) phase.Name = dto.Name;
        if (dto.StartDate.HasValue) phase.StartDate = dto.StartDate;
        if (dto.EndDate.HasValue) phase.EndDate = dto.EndDate;
        if (dto.ActualStart.HasValue) phase.ActualStart = dto.ActualStart;
        if (dto.ActualEnd.HasValue) phase.ActualEnd = dto.ActualEnd;
        if (dto.Status != null) phase.Status = dto.Status;

        var updated = await _phaseRepository.UpdateAsync(phase);
        return MapToDto(updated);
    }

    public async Task<PhaseDto?> StartPhaseAsync(Guid id)
    {
        var phase = await _phaseRepository.GetByIdAsync(id);
        if (phase == null) return null;

        phase.ActualStart = DateTime.UtcNow;
        phase.Status = "IN_PROGRESS";

        var updated = await _phaseRepository.UpdateAsync(phase);
        return MapToDto(updated);
    }

    public async Task<PhaseDto?> CompletePhaseAsync(Guid id)
    {
        var phase = await _phaseRepository.GetByIdAsync(id);
        if (phase == null) return null;

        phase.ActualEnd = DateTime.UtcNow;
        phase.Status = "COMPLETED";

        var updated = await _phaseRepository.UpdateAsync(phase);
        return MapToDto(updated);
    }

    private static PhaseDto MapToDto(Phase phase)
    {
        return new PhaseDto(
            phase.Id,
            phase.ProjectId,
            phase.PhaseCode,
            phase.Name,
            phase.StartDate,
            phase.EndDate,
            phase.ActualStart,
            phase.ActualEnd,
            phase.Status,
            phase.OrderIndex
        );
    }
}
