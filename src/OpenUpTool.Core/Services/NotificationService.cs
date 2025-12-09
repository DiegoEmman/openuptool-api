using OpenUpTool.Core.DTOs;
using OpenUpTool.Core.Entities;
using OpenUpTool.Core.Interfaces;

namespace OpenUpTool.Core.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repository;
    private readonly INotificationPreferenceRepository _preferenceRepository;

    public NotificationService(INotificationRepository repository, INotificationPreferenceRepository preferenceRepository)
    {
        _repository = repository;
        _preferenceRepository = preferenceRepository;
    }

    public async Task<IEnumerable<NotificationDto>> GetByUserIdAsync(Guid userId, bool? isRead = null)
    {
        var notifications = await _repository.GetByUserIdAsync(userId, isRead);
        return notifications.Select(MapToDto);
    }

    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        return await _repository.GetUnreadCountAsync(userId);
    }

    public async Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId)
    {
        var notification = await _repository.GetByIdAsync(notificationId);
        
        if (notification == null || notification.UserId != userId)
            return false;

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        await _repository.UpdateAsync(notification);

        return true;
    }

    public async Task MarkAllAsReadAsync(Guid userId)
    {
        await _repository.MarkAllAsReadAsync(userId);
    }

    public async Task CreateNotificationAsync(
        Guid userId, 
        string type, 
        string title, 
        string message,
        string? relatedEntityType = null, 
        Guid? relatedEntityId = null, 
        string? actionUrl = null)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = type,
            Title = title,
            Message = message,
            RelatedEntityType = relatedEntityType,
            RelatedEntityId = relatedEntityId,
            ActionUrl = actionUrl,
            IsRead = false
        };

        await _repository.CreateAsync(notification);
    }

    public async Task CreateNotificationByEmailAsync(
        string email,
        string type,
        string title,
        string message,
        string? relatedEntityType = null,
        Guid? relatedEntityId = null,
        string? actionUrl = null)
    {
        // Buscar usuario por email
        var userId = await _repository.GetUserIdByEmailAsync(email);
        
        // Solo crear notificación si el usuario existe
        if (userId.HasValue)
        {
            await CreateNotificationAsync(
                userId.Value,
                type,
                title,
                message,
                relatedEntityType,
                relatedEntityId,
                actionUrl
            );
        }
    }

    public async Task<int> DeleteByRelatedEntityAsync(Guid relatedEntityId, string relatedEntityType)
    {
        return await _repository.DeleteByRelatedEntityAsync(relatedEntityId, relatedEntityType);
    }

    // HU-021: Notification Preferences

    public async Task<UserNotificationSettingsDto> GetUserPreferencesAsync(Guid userId)
    {
        var preferences = await _preferenceRepository.GetByUserIdAsync(userId);
        var preferenceList = preferences.ToList();

        // Si no hay preferencias, inicializar con valores por defecto
        if (!preferenceList.Any())
        {
            await InitializeUserPreferencesAsync(userId);
            preferences = await _preferenceRepository.GetByUserIdAsync(userId);
            preferenceList = preferences.ToList();
        }

        return new UserNotificationSettingsDto(
            userId,
            preferenceList.Select(MapPreferenceToFullDto).ToList(),
            preferenceList.All(p => p.EmailEnabled),
            preferenceList.All(p => p.InAppEnabled)
        );
    }

    public async Task<NotificationPreferenceFullDto> UpdatePreferenceAsync(Guid userId, string notificationType, BulkPreferenceItemDto dto)
    {
        var preference = await _preferenceRepository.GetByUserAndTypeAsync(userId, notificationType);

        if (preference == null)
        {
            // Crear nueva preferencia si no existe
            preference = new NotificationPreference
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                NotificationType = notificationType,
                EmailEnabled = dto.EmailEnabled ?? false,
                InAppEnabled = dto.PlatformEnabled ?? true
            };
            await _preferenceRepository.CreateAsync(preference);
        }
        else
        {
            // Actualizar preferencia existente
            if (dto.EmailEnabled.HasValue)
                preference.EmailEnabled = dto.EmailEnabled.Value;
            if (dto.PlatformEnabled.HasValue)
                preference.InAppEnabled = dto.PlatformEnabled.Value;

            preference.UpdatedAt = DateTime.UtcNow;
            await _preferenceRepository.UpdateAsync(preference);
        }

        return MapPreferenceToFullDto(preference);
    }

    public async Task<UserNotificationSettingsDto> BulkUpdatePreferencesAsync(Guid userId, BulkUpdatePreferencesDto dto)
    {
        // Primero asegurar que el usuario tiene preferencias inicializadas
        var existingPrefs = (await _preferenceRepository.GetByUserIdAsync(userId)).ToList();
        if (!existingPrefs.Any())
        {
            await InitializeUserPreferencesAsync(userId);
        }

        // Aplicar actualizaciones globales si se especifican
        if (dto.EnableAllEmail.HasValue || dto.EnableAllPlatform.HasValue)
        {
            var allPrefs = await _preferenceRepository.GetByUserIdAsync(userId);
            foreach (var pref in allPrefs)
            {
                if (dto.EnableAllEmail.HasValue)
                    pref.EmailEnabled = dto.EnableAllEmail.Value;
                if (dto.EnableAllPlatform.HasValue)
                    pref.InAppEnabled = dto.EnableAllPlatform.Value;
                pref.UpdatedAt = DateTime.UtcNow;
                await _preferenceRepository.UpdateAsync(pref);
            }
        }

        // Aplicar actualizaciones individuales
        if (dto.Preferences != null)
        {
            foreach (var prefDto in dto.Preferences)
            {
                await UpdatePreferenceAsync(userId, prefDto.NotificationType, prefDto);
            }
        }

        return await GetUserPreferencesAsync(userId);
    }

    public async Task InitializeUserPreferencesAsync(Guid userId)
    {
        await _preferenceRepository.InitializeDefaultPreferencesAsync(userId);
    }

    public async Task<bool> ShouldNotifyAsync(Guid userId, string notificationType, bool isEmail = false)
    {
        return await _preferenceRepository.IsNotificationEnabledAsync(userId, notificationType, isEmail);
    }

    public async Task CreateSmartNotificationAsync(
        Guid userId,
        string type,
        string title,
        string message,
        string? relatedEntityType = null,
        Guid? relatedEntityId = null,
        string? actionUrl = null)
    {
        // Verificar preferencias del usuario antes de crear la notificación
        if (await ShouldNotifyAsync(userId, type, isEmail: false))
        {
            await CreateNotificationAsync(userId, type, title, message, relatedEntityType, relatedEntityId, actionUrl);
        }
        
        // Aquí se podría agregar lógica para enviar email si ShouldNotifyAsync(userId, type, isEmail: true)
        // Por ahora solo manejamos notificaciones en plataforma
    }

    private static NotificationDto MapToDto(Notification notification)
    {
        return new NotificationDto(
            notification.Id,
            notification.Type,
            notification.Title,
            notification.Message,
            notification.RelatedEntityType,
            notification.RelatedEntityId,
            notification.IsRead,
            notification.ActionUrl,
            notification.CreatedAt
        );
    }

    private static NotificationPreferenceFullDto MapPreferenceToFullDto(NotificationPreference preference)
    {
        return new NotificationPreferenceFullDto(
            preference.Id,
            preference.UserId,
            preference.NotificationType,
            preference.EmailEnabled,
            preference.InAppEnabled,
            true, // IsEnabled - siempre true ya que no existe el campo
            preference.CreatedAt,
            preference.UpdatedAt
        );
    }
}
