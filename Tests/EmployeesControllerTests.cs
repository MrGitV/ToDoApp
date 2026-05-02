using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Text;
using ToDoApp.Controllers;
using ToDoApp.Models;
using ToDoApp.Services;

namespace Tests
{
    public class EmployeesControllerTests
    {
        private readonly Mock<IEmployeeService> _empServiceMock = new();
        private readonly EmployeesController _controller;

        public EmployeesControllerTests()
        {
            _controller = new EmployeesController(_empServiceMock.Object);
        }

        [Fact]
        public async Task Index_WithFilters_ReturnsView()
        {
            _empServiceMock.Setup(s => s.GetAllEmployeesAsync("Ivan", "Dev")).ReturnsAsync([]);
            var result = await _controller.IndexAsync("Ivan", "Dev") as ViewResult;
            Assert.NotNull(result);
        }

        [Fact]
        public async Task Details_ValidId_ReturnsView()
        {
            _empServiceMock.Setup(s => s.GetEmployeeByIdAsync(1)).ReturnsAsync(new Employee { Id = 1 });
            var result = await _controller.DetailsAsync(1) as ViewResult;
            Assert.NotNull(result);
        }

        [Fact]
        public async Task Create_PostValidData_RedirectsToIndex()
        {
            var emp = new Employee { FirstName = "Test" };
            var fileMock = new Mock<IFormFile>();
            var content = "Fake image content";
            var fileName = "test.png";
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(content));
            fileMock.Setup(f => f.OpenReadStream()).Returns(ms);
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(ms.Length);
            fileMock.Setup(f => f.ContentType).Returns("image/png");

            var result = await _controller.CreateAsync(emp, fileMock.Object) as RedirectToActionResult;

            Assert.Equal("IndexAsync", result?.ActionName);
            _empServiceMock.Verify(s => s.CreateEmployeeAsync(It.IsAny<Employee>()), Times.Once);
        }

        [Fact]
        public async Task Edit_PostValidData_RedirectsToIndex()
        {
            var emp = new Employee { Id = 1, FirstName = "Old" };
            _empServiceMock.Setup(s => s.GetTrackedEmployeeByIdAsync(1)).ReturnsAsync(emp);

            var result = await _controller.EditAsync(1, new Employee { Id = 1, FirstName = "New" }, null) as RedirectToActionResult;

            Assert.Equal("IndexAsync", result?.ActionName);
            _empServiceMock.Verify(s => s.UpdateEmployeeAsync(It.IsAny<Employee>()), Times.Once);
        }

        [Fact]
        public async Task Delete_GetValidId_ReturnsView()
        {
            _empServiceMock.Setup(s => s.GetEmployeeByIdAsync(1)).ReturnsAsync(new Employee { Id = 1 });
            var result = await _controller.DeleteAsync(1) as ViewResult;
            Assert.NotNull(result);
        }
    }
}
