using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using ToDoApp.Controllers;
using ToDoApp.Models;
using ToDoApp.Services;

namespace Tests
{
    public class TasksControllerTests
    {
        private readonly Mock<ITaskService> _taskServiceMock = new();
        private readonly Mock<IEmployeeService> _empServiceMock = new();
        private readonly Mock<INotificationService> _notifServiceMock = new();
        private readonly TasksController _controller;

        public TasksControllerTests()
        {
            _controller = new TasksController(_taskServiceMock.Object, _empServiceMock.Object, _notifServiceMock.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity([
                new Claim(ClaimTypes.Name, "admin"),
                new Claim(ClaimTypes.Role, UserRole.Admin)
            ], "mock"));
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };
        }

        [Fact]
        public async Task Details_AdminAccess_ReturnsView()
        {
            _taskServiceMock.Setup(s => s.GetTaskByIdAsync(1)).ReturnsAsync(new ToDoTask { Id = 1 });
            _taskServiceMock.Setup(s => s.GetCommentsByTaskIdAsync(1)).ReturnsAsync([]);

            var result = await _controller.Details(1) as ViewResult;
            Assert.NotNull(result);
            _notifServiceMock.Verify(n => n.MarkNotificationsAsReadAsync("admin", 1), Times.Once);
        }

        [Fact]
        public async Task Edit_PostValidData_RedirectsToIndex()
        {
            var task = new ToDoTask { Id = 1, Title = "Update" };
            var result = await _controller.Edit(1, task) as RedirectToActionResult;

            Assert.Equal("Index", result?.ActionName);
            _taskServiceMock.Verify(s => s.UpdateTaskAsync(task), Times.Once);
        }

        [Fact]
        public async Task Delete_GetAndPost_Success()
        {
            _taskServiceMock.Setup(s => s.GetTaskByIdAsync(1)).ReturnsAsync(new ToDoTask { Id = 1 });

            var getResult = await _controller.Delete(1) as ViewResult;
            Assert.NotNull(getResult);

            var postResult = await _controller.DeleteConfirmed(1) as RedirectToActionResult;
            Assert.Equal("Index", postResult?.ActionName);
            _taskServiceMock.Verify(s => s.DeleteTaskAsync(1), Times.Once);
        }
    }
}
