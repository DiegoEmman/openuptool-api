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
