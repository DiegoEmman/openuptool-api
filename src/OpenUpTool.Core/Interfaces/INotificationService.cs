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
}
