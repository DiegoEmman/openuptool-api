using OpenUpTool.Core.Entities;

namespace OpenUpTool.Core.Interfaces;

public interface INotificationRepository
{
    Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId, bool? isRead = null);
    Task<Notification?> GetByIdAsync(Guid id);
    Task<Notification> CreateAsync(Notification notification);
    Task<Notification> UpdateAsync(Notification notification);
    Task<bool> DeleteAsync(Guid id);
    Task<int> DeleteByRelatedEntityAsync(Guid relatedEntityId, string relatedEntityType);
    Task<int> GetUnreadCountAsync(Guid userId);
    Task MarkAllAsReadAsync(Guid userId);
    Task<Guid?> GetUserIdByEmailAsync(string email);
}

// HU-021: Repositorio de preferencias de notificación
public interface INotificationPreferenceRepository
{
    Task<IEnumerable<NotificationPreference>> GetByUserIdAsync(Guid userId);
    Task<NotificationPreference?> GetByUserAndTypeAsync(Guid userId, string notificationType);
    Task<NotificationPreference?> GetByIdAsync(Guid id);
    Task<NotificationPreference> CreateAsync(NotificationPreference preference);
    Task<NotificationPreference> UpdateAsync(NotificationPreference preference);
    Task DeleteAsync(Guid id);
    Task<bool> IsNotificationEnabledAsync(Guid userId, string notificationType, bool checkEmail = false);
    Task InitializeDefaultPreferencesAsync(Guid userId);
}
