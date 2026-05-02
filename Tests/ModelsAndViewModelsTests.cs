using ToDoApp.Models;
using ToDoApp.Models.ViewModels;

namespace Tests
{
    public class ModelsAndViewModelsTests
    {
        [Fact]
        public void ViewModels_SetAndGetProperties_WorkCorrectly()
        {
            // TaskDetailsViewModel
            var taskDetails = new TaskDetailsViewModel
            {
                Task = new ToDoTask { Title = "T1" },
                Comments = [new() { Content = "C1" }]
            };
            Assert.Equal("T1", taskDetails.Task.Title);
            Assert.Single(taskDetails.Comments);

            // TokenResponseViewModel
            var tokenResp = new TokenResponseViewModel
            {
                Token = "abc",
                Expiration = DateTime.MinValue,
                Username = "user",
                Role = "Admin"
            };
            Assert.Equal("abc", tokenResp.Token);
            Assert.Equal("user", tokenResp.Username);

            // Notification
            var notif = new Notification
            {
                Id = 1,
                Message = "Test",
                IsRead = true,
                TaskId = 5
            };
            Assert.True(notif.IsRead);
            Assert.Equal(5, notif.TaskId);
        }
    }
}
