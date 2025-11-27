using OpenUpTool.Core.DTOs;
using OpenUpTool.Core.Entities;
using OpenUpTool.Core.Interfaces;

namespace OpenUpTool.Core.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repository;

    public NotificationService(INotificationRepository repository)
    {
        _repository = repository;
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
}
