using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Security.Claims;
using ToDoApp.Controllers;
using ToDoApp.Models;
using ToDoApp.Models.ViewModels;
using ToDoApp.Services;

namespace Tests
{
    public class ControllersEdgeCasesTests
    {
        // Helper for setting up Controller Context
        private static ControllerContext GetContext(string username, string role)
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity([
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role)
            ], "mock"));

            return new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };
        }

        [Fact]
        public async Task AccountController_Login_InvalidModel_ReturnsView()
        {
            var controller = new AccountController(new Mock<IHttpClientFactory>().Object, new Mock<IConfiguration>().Object, new Microsoft.IdentityModel.Tokens.TokenValidationParameters());
            controller.ModelState.AddModelError("Error", "Error");
            var result = await controller.LoginAsync(new LoginViewModel()) as ViewResult;
            Assert.NotNull(result);
        }

        [Fact]
        public async Task HomeController_Index_EmployeeNotFound_ReturnsNotFound()
        {
            var empMock = new Mock<IEmployeeService>();
            empMock.Setup(s => s.GetEmployeeByUsernameAsync(It.IsAny<string>())).ReturnsAsync((Employee)null!);
            var controller = new HomeController(empMock.Object, new Mock<ITaskService>().Object)
            {
                ControllerContext = GetContext("ghost", UserRole.Employee)
            };
            var result = await controller.Index() as NotFoundObjectResult;
            Assert.NotNull(result);
        }

        [Fact]
        public async Task EmployeesController_Edit_IdMismatch_ReturnsNotFound()
        {
            var controller = new EmployeesController(new Mock<IEmployeeService>().Object);
            var result = await controller.EditAsync(1, new Employee { Id = 2 }, null) as NotFoundResult;
            Assert.NotNull(result);
        }

        [Fact]
        public async Task EmployeesController_Edit_ConcurrencyException_ReturnsNotFound()
        {
            var empMock = new Mock<IEmployeeService>();
            empMock.Setup(s => s.GetTrackedEmployeeByIdAsync(1)).ReturnsAsync(new Employee { Id = 1 });
            empMock.Setup(s => s.UpdateEmployeeAsync(It.IsAny<Employee>())).ThrowsAsync(new DbUpdateConcurrencyException());
            empMock.Setup(s => s.EmployeeExistsAsync(1)).ReturnsAsync(false); // Emulate deleted record

            var controller = new EmployeesController(empMock.Object);
            var result = await controller.EditAsync(1, new Employee { Id = 1 }, null) as NotFoundResult;
            Assert.NotNull(result);
        }

        [Fact]
        public async Task TasksController_Details_EmployeeAccessingOtherTask_ReturnsForbid()
        {
            var taskMock = new Mock<ITaskService>();
            var empMock = new Mock<IEmployeeService>();

            // Task belongs to Employee 2
            taskMock.Setup(s => s.GetTaskByIdAsync(1)).ReturnsAsync(new ToDoTask { Id = 1, EmployeeId = 2 });
            // Current User is Employee 1
            empMock.Setup(s => s.GetEmployeeByUsernameAsync("emp1")).ReturnsAsync(new Employee { Id = 1, Username = "emp1" });

            var controller = new TasksController(taskMock.Object, empMock.Object, new Mock<INotificationService>().Object)
            {
                ControllerContext = GetContext("emp1", UserRole.Employee)
            };

            var result = await controller.DetailsAsync(1) as ForbidResult;
            Assert.NotNull(result);
        }

        [Fact]
        public async Task TasksController_AddComment_EmptyContent_RedirectsWithError()
        {
            var controller = new TasksController(new Mock<ITaskService>().Object, new Mock<IEmployeeService>().Object, new Mock<INotificationService>().Object)
            {
                TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>())
            };
            var result = await controller.AddCommentAsync(1, "   ") as RedirectToActionResult;
            Assert.Equal("Details", result?.ActionName);
            Assert.True(controller.TempData.ContainsKey("Error"));
        }

        [Fact]
        public async Task TasksController_Edit_TaskNotFound_ReturnsNotFound()
        {
            var taskMock = new Mock<ITaskService>();
            taskMock.Setup(s => s.GetTaskByIdAsync(99)).ReturnsAsync((ToDoTask)null!);
            var controller = new TasksController(taskMock.Object, new Mock<IEmployeeService>().Object, new Mock<INotificationService>().Object);

            var result = await controller.EditAsync(99) as NotFoundResult;
            Assert.NotNull(result);
        }
    }
}
