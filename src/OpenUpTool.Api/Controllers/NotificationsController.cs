using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenUpTool.Core.DTOs;
using OpenUpTool.Core.Entities;
using OpenUpTool.Core.Interfaces;
using System.Security.Claims;

namespace OpenUpTool.Api.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _service;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(INotificationService service, ILogger<NotificationsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<NotificationDto>>> GetAll([FromQuery] bool? isRead = null)
    {
        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var notifications = await _service.GetByUserIdAsync(userId, isRead);
            return Ok(notifications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener notificaciones");
            return StatusCode(500, new { message = "Error al obtener notificaciones" });
        }
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<int>> GetUnreadCount()
    {
        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var count = await _service.GetUnreadCountAsync(userId);
            return Ok(new { count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener contador de notificaciones");
            return StatusCode(500, new { message = "Error al obtener contador" });
        }
    }

    [HttpPost("{id}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var marked = await _service.MarkAsReadAsync(id, userId);
            if (!marked)
                return NotFound(new { message = "Notificación no encontrada" });

            return Ok(new { message = "Notificación marcada como leída" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al marcar notificación {Id} como leída", id);
            return StatusCode(500, new { message = "Error al marcar como leída" });
        }
    }

    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            await _service.MarkAllAsReadAsync(userId);
            return Ok(new { message = "Todas las notificaciones marcadas como leídas" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al marcar todas las notificaciones como leídas");
            return StatusCode(500, new { message = "Error al marcar todas como leídas" });
        }
    }

    // ==================== HU-021: Notification Preferences ====================

    /// <summary>
    /// Obtiene las preferencias de notificación del usuario actual
    /// </summary>
    [HttpGet("preferences")]
    public async Task<ActionResult<UserNotificationSettingsDto>> GetPreferences()
    {
        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var settings = await _service.GetUserPreferencesAsync(userId);
            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener preferencias de notificación");
            return StatusCode(500, new { message = "Error al obtener preferencias" });
        }
    }

    /// <summary>
    /// Obtiene los tipos de notificación disponibles
    /// </summary>
    [HttpGet("types")]
    public ActionResult<IEnumerable<object>> GetNotificationTypes()
    {
        try
        {
            var types = NotificationTypes.All.Select(t => new
            {
                type = t,
                description = GetTypeDescription(t)
            });
            return Ok(types);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener tipos de notificación");
            return StatusCode(500, new { message = "Error al obtener tipos" });
        }
    }

    /// <summary>
    /// Actualiza una preferencia de notificación específica
    /// </summary>
    [HttpPut("preferences/{notificationType}")]
    public async Task<ActionResult<NotificationPreferenceFullDto>> UpdatePreference(
        string notificationType,
        [FromBody] BulkPreferenceItemDto dto)
    {
        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            // Validar que el tipo de notificación sea válido
            if (!NotificationTypes.All.Contains(notificationType))
            {
                return BadRequest(new { message = $"Tipo de notificación inválido: {notificationType}" });
            }

            var preference = await _service.UpdatePreferenceAsync(userId, notificationType, dto);
            return Ok(preference);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar preferencia de notificación {Type}", notificationType);
            return StatusCode(500, new { message = "Error al actualizar preferencia" });
        }
    }

    /// <summary>
    /// Actualiza múltiples preferencias de notificación en una sola operación
    /// </summary>
    [HttpPut("preferences")]
    public async Task<ActionResult<UserNotificationSettingsDto>> BulkUpdatePreferences(
        [FromBody] BulkUpdatePreferencesDto dto)
    {
        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            // Validar tipos de notificación si se proporcionan preferencias individuales
            if (dto.Preferences != null)
            {
                foreach (var pref in dto.Preferences)
                {
                    if (!NotificationTypes.All.Contains(pref.NotificationType))
                    {
                        return BadRequest(new { message = $"Tipo de notificación inválido: {pref.NotificationType}" });
                    }
                }
            }

            var settings = await _service.BulkUpdatePreferencesAsync(userId, dto);
            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar preferencias de notificación en lote");
            return StatusCode(500, new { message = "Error al actualizar preferencias" });
        }
    }

    /// <summary>
    /// Endpoint de prueba para disparar una notificación (solo para testing)
    /// </summary>
    [HttpPost("trigger")]
    public async Task<IActionResult> TriggerNotification([FromBody] TriggerNotificationDto dto)
    {
        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            // Validar tipo de notificación
            if (!NotificationTypes.All.Contains(dto.Type))
            {
                return BadRequest(new { message = $"Tipo de notificación inválido: {dto.Type}" });
            }

            // Usar la creación inteligente que respeta preferencias
            await _service.CreateSmartNotificationAsync(
                userId,
                dto.Type,
                dto.Title,
                dto.Message,
                dto.RelatedEntityType,
                dto.RelatedEntityId,
                dto.ActionUrl
            );

            return Ok(new { 
                message = "Notificación procesada correctamente",
                note = "La notificación se creará según las preferencias del usuario"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al disparar notificación de prueba");
            return StatusCode(500, new { message = "Error al disparar notificación" });
        }
    }

    /// <summary>
    /// Inicializa las preferencias por defecto para el usuario
    /// </summary>
    [HttpPost("preferences/initialize")]
    public async Task<ActionResult<UserNotificationSettingsDto>> InitializePreferences()
    {
        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            await _service.InitializeUserPreferencesAsync(userId);
            var settings = await _service.GetUserPreferencesAsync(userId);
            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al inicializar preferencias de notificación");
            return StatusCode(500, new { message = "Error al inicializar preferencias" });
        }
    }

    private static string GetTypeDescription(string type)
    {
        return type switch
        {
            NotificationTypes.NewVersion => "Nueva versión de artefacto creada",
            NotificationTypes.StateApproved => "Estado de artefacto aprobado",
            NotificationTypes.CriticalDefect => "Defecto crítico registrado",
            NotificationTypes.DeadlineApproaching => "Fecha límite próxima",
            NotificationTypes.ArtifactBlocked => "Artefacto bloqueado",
            NotificationTypes.Invitation => "Invitación a proyecto recibida",
            NotificationTypes.InvitationAccepted => "Invitación aceptada",
            NotificationTypes.Mention => "Mencionado en comentario",
            NotificationTypes.Comment => "Nuevo comentario en artefacto",
            NotificationTypes.PhaseChange => "Cambio de fase del proyecto",
            NotificationTypes.WorkflowChange => "Cambio de flujo de trabajo",
            _ => "Notificación del sistema"
        };
    }
}
