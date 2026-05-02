using Microsoft.EntityFrameworkCore;
using ToDoApp.Data;
using ToDoApp.Services;

namespace Tests
{
    public class NotificationServiceTests
    {
        private readonly ApplicationDbContext _context;
        private readonly NotificationService _service;

        public NotificationServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            _context = new ApplicationDbContext(options);
            _service = new NotificationService(_context);
        }

        // Tests notification creation
        [Fact]
        public async Task CreateNotificationAsync_AddsToDatabase()
        {
            await _service.CreateNotificationAsync("admin", "Test message", 1);

            var notif = await _context.Notifications.FirstAsync();
            Assert.Equal("admin", notif.RecipientUsername);
            Assert.False(notif.IsRead);
        }

        // Tests getting unread notification count
        [Fact]
        public async Task GetUnreadNotificationCountAsync_ReturnsCorrectCount()
        {
            await _service.CreateNotificationAsync("user", "Msg 1", null);
            await _service.CreateNotificationAsync("user", "Msg 2", null);

            var count = await _service.GetUnreadNotificationCountAsync("user");
            Assert.Equal(2, count);
        }

        // Tests marking notifications as read
        [Fact]
        public async Task MarkNotificationsAsReadAsync_UpdatesStatus()
        {
            await _service.CreateNotificationAsync("user", "Msg", 5);
            await _service.MarkNotificationsAsReadAsync("user", 5);

            var notif = await _context.Notifications.FirstAsync();
            Assert.True(notif.IsRead);
        }
    }
}
