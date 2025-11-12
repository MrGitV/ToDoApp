using ToDoApp.Models;

namespace ToDoApp.Services
{
    public interface INotificationService
    {
        Task CreateNotificationAsync(string recipientUsername, string message, int? taskId);
        Task<IEnumerable<Notification>> GetUnreadNotificationsAsync(string username);
        Task MarkNotificationsAsReadAsync(string username, int? taskId = null);
        Task<int> GetUnreadNotificationCountAsync(string username);
    }
}