using OpenUpTool.Core.DTOs;
using OpenUpTool.Core.Entities;
using OpenUpTool.Core.Interfaces;

namespace OpenUpTool.Core.Services;

public class ProjectInvitationService : IProjectInvitationService
{
    private readonly IProjectInvitationRepository _invitationRepository;
    private readonly IProjectUserRoleRepository _projectUserRoleRepository;
    private readonly INotificationService _notificationService;

    public ProjectInvitationService(
        IProjectInvitationRepository invitationRepository,
        IProjectUserRoleRepository projectUserRoleRepository,
        INotificationService notificationService)
    {
        _invitationRepository = invitationRepository;
        _projectUserRoleRepository = projectUserRoleRepository;
        _notificationService = notificationService;
    }

    public async Task<ProjectInvitationDto> CreateInvitationAsync(CreateInvitationDto dto, Guid invitedBy)
    {
        var token = Guid.NewGuid().ToString("N");
        var expiresAt = DateTime.UtcNow.AddDays(7);

        var invitation = new ProjectInvitation
        {
            Id = Guid.NewGuid(),
            ProjectId = dto.ProjectId,
            InvitedEmail = dto.InvitedEmail,
            RoleId = dto.RoleId,
            InvitedBy = invitedBy,
            Status = "pending",
            InvitationToken = token,
            ExpiresAt = expiresAt
        };

        var created = await _invitationRepository.CreateAsync(invitation);
        
        // Crear notificación para el usuario invitado
        // La notificación se crea incluso si el usuario aún no existe en el sistema
        // Cuando el usuario se registre con ese email, verá las notificaciones pendientes
        await _notificationService.CreateNotificationByEmailAsync(
            dto.InvitedEmail,
            "invitation",
            "Nueva invitación a proyecto",
            $"Has sido invitado a unirte a un proyecto",
            "project",
            dto.ProjectId,
            $"/invitations/accept/{token}"
        );
        
        return MapToDto(created);
    }

    public async Task<IEnumerable<ProjectInvitationDto>> GetByProjectIdAsync(Guid projectId)
    {
        var invitations = await _invitationRepository.GetByProjectIdAsync(projectId);
        return invitations.Select(MapToDto);
    }

    public async Task<IEnumerable<ProjectInvitationDto>> GetPendingByEmailAsync(string email)
    {
        var invitations = await _invitationRepository.GetPendingByEmailAsync(email);
        return invitations.Select(MapToDto);
    }

    public async Task<bool> AcceptInvitationAsync(string token, Guid userId)
    {
        var invitation = await _invitationRepository.GetByTokenAsync(token);
        
        if (invitation == null)
        {
            Console.WriteLine($"Invitación no encontrada para token: {token}");
            return false;
        }

        if (invitation.Status != "pending")
        {
            Console.WriteLine($"Invitación con status: {invitation.Status} (esperado: pending)");
            return false;
        }

        if (invitation.ExpiresAt < DateTime.UtcNow)
        {
            Console.WriteLine($"Invitación expirada: {invitation.ExpiresAt} < {DateTime.UtcNow}");
            return false;
        }

        // Crear el ProjectUserRole
        var projectUserRole = new ProjectUserRole
        {
            Id = Guid.NewGuid(),
            ProjectId = invitation.ProjectId,
            UserId = userId,
            RoleId = invitation.RoleId,
            InvitedBy = invitation.InvitedBy,
            InvitedAt = invitation.CreatedAt,
            AcceptedAt = DateTime.UtcNow,
            Status = "active"
        };

        await _projectUserRoleRepository.CreateAsync(projectUserRole);

        // Actualizar invitación
        invitation.Status = "accepted";
        invitation.AcceptedAt = DateTime.UtcNow;
        await _invitationRepository.UpdateAsync(invitation);

        // Eliminar la notificación de invitación original
        await _notificationService.DeleteByRelatedEntityAsync(invitation.ProjectId, "project");

        // Notificar al invitador si el proyecto tiene nombre
        if (invitation.Project != null)
        {
            await _notificationService.CreateNotificationAsync(
                invitation.InvitedBy,
                "invitation_accepted",
                "Invitación aceptada",
                $"{invitation.InvitedEmail} ha aceptado la invitación al proyecto {invitation.Project.Name}",
                "project",
                invitation.ProjectId,
                $"/projects/{invitation.ProjectId}"
            );
        }

        return true;
    }

    public async Task<bool> RejectInvitationAsync(string token)
    {
        var invitation = await _invitationRepository.GetByTokenAsync(token);
        
        if (invitation == null || invitation.Status != "pending")
            return false;

        invitation.Status = "rejected";
        await _invitationRepository.UpdateAsync(invitation);

        // Eliminar notificaciones relacionadas con esta invitación
        await _notificationService.DeleteByRelatedEntityAsync(invitation.ProjectId, "project");

        return true;
    }

    public async Task<bool> CancelInvitationAsync(Guid invitationId)
    {
        // Obtener la invitación antes de eliminarla
        var invitation = await _invitationRepository.GetByIdAsync(invitationId);
        if (invitation == null) return false;

        // Eliminar notificaciones relacionadas con esta invitación
        await _notificationService.DeleteByRelatedEntityAsync(invitation.ProjectId, "project");

        // Eliminar la invitación
        return await _invitationRepository.DeleteAsync(invitationId);
    }

    private static ProjectInvitationDto MapToDto(ProjectInvitation invitation)
    {
        return new ProjectInvitationDto(
            invitation.Id,
            invitation.ProjectId,
            invitation.Project?.Name ?? "",
            invitation.InvitedEmail,
            invitation.RoleId,
            invitation.Role?.Name ?? "",
            invitation.InvitedBy,
            invitation.Inviter != null ? $"{invitation.Inviter.FirstName} {invitation.Inviter.LastName}" : "",
            invitation.Status,
            invitation.InvitationToken,
            invitation.ExpiresAt,
            invitation.CreatedAt
        );
    }
}
