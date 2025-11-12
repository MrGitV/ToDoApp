using Microsoft.EntityFrameworkCore;
using ToDoApp.Data;
using ToDoApp.Models;

namespace ToDoApp.Services
{
    public class NotificationService(ApplicationDbContext context) : INotificationService
    {
        private readonly ApplicationDbContext _context = context;

        // Creates and saves a new notification.
        public async Task CreateNotificationAsync(string recipientUsername, string message, int? taskId)
        {
            var notification = new Notification
            {
                RecipientUsername = recipientUsername,
                Message = message,
                TaskId = taskId,
                IsRead = false,
                Timestamp = DateTime.UtcNow
            };
            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();
        }

        // Retrieves all unread notifications for a user.
        public async Task<IEnumerable<Notification>> GetUnreadNotificationsAsync(string username)
        {
            return await _context.Notifications
                .Where(n => n.RecipientUsername == username && !n.IsRead)
                .OrderByDescending(n => n.Timestamp)
                .ToListAsync();
        }

        // Gets the count of unread notifications for a user.
        public async Task<int> GetUnreadNotificationCountAsync(string username)
        {
            return await _context.Notifications
                .CountAsync(n => n.RecipientUsername == username && !n.IsRead);
        }

        // Marks notifications as read for a user, optionally filtered by task.
        public async Task MarkNotificationsAsReadAsync(string username, int? taskId = null)
        {
            var query = _context.Notifications
                .Where(n => n.RecipientUsername == username && !n.IsRead);

            if (taskId.HasValue)
            {
                query = query.Where(n => n.TaskId == taskId.Value);
            }

            var notificationsToUpdate = await query.ToListAsync();

            if (notificationsToUpdate.Count != 0)
            {
                foreach (var notification in notificationsToUpdate)
                {
                    notification.IsRead = true;
                }
                await _context.SaveChangesAsync();
            }
        }
    }
}