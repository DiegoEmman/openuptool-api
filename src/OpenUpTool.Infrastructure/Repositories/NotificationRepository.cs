using Microsoft.EntityFrameworkCore;
using OpenUpTool.Core.Entities;
using OpenUpTool.Core.Interfaces;
using OpenUpTool.Infrastructure.Data;

namespace OpenUpTool.Infrastructure.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly OpenUpToolDbContext _context;

    public NotificationRepository(OpenUpToolDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId, bool? isRead = null)
    {
        var query = _context.Notifications.Where(n => n.UserId == userId);

        if (isRead.HasValue)
            query = query.Where(n => n.IsRead == isRead.Value);

        return await query
            .OrderByDescending(n => n.CreatedAt)
            .Take(50) // Limitar a últimas 50
            .ToListAsync();
    }

    public async Task<Notification?> GetByIdAsync(Guid id)
    {
        return await _context.Notifications.FindAsync(id);
    }

    public async Task<Notification> CreateAsync(Notification notification)
    {
        notification.CreatedAt = DateTime.UtcNow;
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
        return notification;
    }

    public async Task<Notification> UpdateAsync(Notification notification)
    {
        _context.Notifications.Update(notification);
        await _context.SaveChangesAsync();
        return notification;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var notification = await _context.Notifications.FindAsync(id);
        if (notification == null) return false;

        _context.Notifications.Remove(notification);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> DeleteByRelatedEntityAsync(Guid relatedEntityId, string relatedEntityType)
    {
        var notifications = await _context.Notifications
            .Where(n => n.RelatedEntityId == relatedEntityId && n.RelatedEntityType == relatedEntityType)
            .ToListAsync();

        if (notifications.Count == 0) return 0;

        _context.Notifications.RemoveRange(notifications);
        await _context.SaveChangesAsync();
        return notifications.Count;
    }

    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        return await _context.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    public async Task MarkAllAsReadAsync(Guid userId)
    {
        var unreadNotifications = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var notification in unreadNotifications)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<Guid?> GetUserIdByEmailAsync(string email)
    {
        var userId = await _context.Users
            .Where(u => u.Email == email)
            .Select(u => (Guid?)u.Id)
            .FirstOrDefaultAsync();
            
        return userId;
    }
}

// HU-021: Repositorio de preferencias de notificación
public class NotificationPreferenceRepository : INotificationPreferenceRepository
{
    private readonly OpenUpToolDbContext _context;

    public NotificationPreferenceRepository(OpenUpToolDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<NotificationPreference>> GetByUserIdAsync(Guid userId)
    {
        return await _context.NotificationPreferences
            .Where(p => p.UserId == userId)
            .OrderBy(p => p.NotificationType)
            .ToListAsync();
    }

    public async Task<NotificationPreference?> GetByUserAndTypeAsync(Guid userId, string notificationType)
    {
        return await _context.NotificationPreferences
            .FirstOrDefaultAsync(p => p.UserId == userId && p.NotificationType == notificationType);
    }

    public async Task<NotificationPreference?> GetByIdAsync(Guid id)
    {
        return await _context.NotificationPreferences.FindAsync(id);
    }

    public async Task<NotificationPreference> CreateAsync(NotificationPreference preference)
    {
        preference.CreatedAt = DateTime.UtcNow;
        preference.UpdatedAt = DateTime.UtcNow;
        _context.NotificationPreferences.Add(preference);
        await _context.SaveChangesAsync();
        return preference;
    }

    public async Task<NotificationPreference> UpdateAsync(NotificationPreference preference)
    {
        preference.UpdatedAt = DateTime.UtcNow;
        _context.NotificationPreferences.Update(preference);
        await _context.SaveChangesAsync();
        return preference;
    }

    public async Task DeleteAsync(Guid id)
    {
        var preference = await _context.NotificationPreferences.FindAsync(id);
        if (preference != null)
        {
            _context.NotificationPreferences.Remove(preference);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> IsNotificationEnabledAsync(Guid userId, string notificationType, bool checkEmail = false)
    {
        var preference = await GetByUserAndTypeAsync(userId, notificationType);
        
        // Si no existe preferencia, por defecto está habilitado en app
        if (preference == null)
            return !checkEmail; // true para in-app, false para email por defecto
        
        return checkEmail ? preference.EmailEnabled : preference.InAppEnabled;
    }

    public async Task InitializeDefaultPreferencesAsync(Guid userId)
    {
        var existingPrefs = await GetByUserIdAsync(userId);
        var existingTypes = existingPrefs.Select(p => p.NotificationType).ToHashSet();

        foreach (var type in NotificationTypes.All)
        {
            if (!existingTypes.Contains(type))
            {
                var preference = new NotificationPreference
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    NotificationType = type,
                    InAppEnabled = true,
                    EmailEnabled = false
                };
                await CreateAsync(preference);
            }
        }
    }
}
