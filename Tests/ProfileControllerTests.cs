using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using ToDoApp.Controllers;
using ToDoApp.Models;
using ToDoApp.Services;

namespace Tests
{
    public class ProfileControllerTests
    {
        private readonly Mock<IEmployeeService> _empServiceMock = new();
        private readonly ProfileController _controller;

        public ProfileControllerTests()
        {
            _controller = new ProfileController(_empServiceMock.Object);
            var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "user1")], "mock"));

            var tempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(
                new DefaultHttpContext(), Mock.Of<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>());

            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };
            _controller.TempData = tempData;
        }

        [Fact]
        public async Task Update_ValidUser_UpdatesAndRedirects()
        {
            var emp = new Employee { Id = 1, Username = "user1", FirstName = "Old" };
            _empServiceMock.Setup(s => s.GetTrackedEmployeeByIdAsync(1)).ReturnsAsync(emp);

            var result = await _controller.Update(new Employee { Id = 1, FirstName = "New" }, null) as RedirectToActionResult;

            Assert.Equal("Index", result?.ActionName);
            Assert.Equal("New", emp.FirstName);
            _empServiceMock.Verify(s => s.UpdateEmployeeAsync(emp), Times.Once);
        }

        [Fact]
        public async Task Update_WrongUser_ReturnsForbid()
        {
            var emp = new Employee { Id = 1, Username = "differentUser" };
            _empServiceMock.Setup(s => s.GetTrackedEmployeeByIdAsync(1)).ReturnsAsync(emp);

            var result = await _controller.Update(new Employee { Id = 1 }, null);

            Assert.IsType<ForbidResult>(result);
        }
    }
}
