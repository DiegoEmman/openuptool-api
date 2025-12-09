using OpenUpTool.Core.DTOs;

namespace OpenUpTool.Core.Interfaces;

public interface INotificationService
{
    Task<IEnumerable<NotificationDto>> GetByUserIdAsync(Guid userId, bool? isRead = null);
    Task<int> GetUnreadCountAsync(Guid userId);
    Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId);
    Task MarkAllAsReadAsync(Guid userId);
    Task CreateNotificationAsync(Guid userId, string type, string title, string message, 
        string? relatedEntityType = null, Guid? relatedEntityId = null, string? actionUrl = null);
    Task CreateNotificationByEmailAsync(string email, string type, string title, string message,
        string? relatedEntityType = null, Guid? relatedEntityId = null, string? actionUrl = null);
    Task<int> DeleteByRelatedEntityAsync(Guid relatedEntityId, string relatedEntityType);
    
    // HU-021: Notification Preferences
    Task<UserNotificationSettingsDto> GetUserPreferencesAsync(Guid userId);
    Task<NotificationPreferenceFullDto> UpdatePreferenceAsync(Guid userId, string notificationType, BulkPreferenceItemDto dto);
    Task<UserNotificationSettingsDto> BulkUpdatePreferencesAsync(Guid userId, BulkUpdatePreferencesDto dto);
    Task InitializeUserPreferencesAsync(Guid userId);
    Task<bool> ShouldNotifyAsync(Guid userId, string notificationType, bool isEmail = false);
    Task CreateSmartNotificationAsync(Guid userId, string type, string title, string message,
        string? relatedEntityType = null, Guid? relatedEntityId = null, string? actionUrl = null);
}
