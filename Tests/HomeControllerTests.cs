using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using ToDoApp.Controllers;
using ToDoApp.Models;
using ToDoApp.Models.ViewModels;
using ToDoApp.Services;

namespace Tests
{
    public class HomeControllerTests
    {
        private readonly Mock<IEmployeeService> _empServiceMock = new();
        private readonly Mock<ITaskService> _taskServiceMock = new();

        // Helper to mock User Identity
        private static ControllerContext GetContext(string username, string role)
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity([
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role)
            ], "mock"));

            return new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };
        }

        // Tests Admin dashboard rendering
        [Fact]
        public async Task Index_AsAdmin_ReturnsAdminDashboard()
        {
            var controller = new HomeController(_empServiceMock.Object, _taskServiceMock.Object)
            {
                ControllerContext = GetContext("admin", UserRole.Admin)
            };

            _empServiceMock.Setup(s => s.GetAllEmployeesAsync(null, null)).ReturnsAsync([]);
            _taskServiceMock.Setup(s => s.GetAllTasksAsync(null, null, null)).ReturnsAsync([]);

            var result = await controller.Index() as ViewResult;
            var model = result?.Model as DashboardViewModel;

            Assert.NotNull(result);
            Assert.NotNull(model);
        }

        // Tests Employee dashboard rendering
        [Fact]
        public async Task Index_AsEmployee_ReturnsEmployeeDashboard()
        {
            var controller = new HomeController(_empServiceMock.Object, _taskServiceMock.Object)
            {
                ControllerContext = GetContext("user1", UserRole.Employee)
            };

            _empServiceMock.Setup(s => s.GetEmployeeByUsernameAsync("user1"))
                .ReturnsAsync(new Employee { Id = 1, Username = "user1" });

            _taskServiceMock.Setup(s => s.GetTasksByEmployeeIdAsync(1, null, null, null)).ReturnsAsync([]);

            var result = await controller.Index() as ViewResult;

            Assert.Equal("Index_Employee", result?.ViewName);
        }
    }
}
