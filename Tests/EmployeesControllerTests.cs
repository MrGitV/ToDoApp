using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text;
using ToDoApp.Controllers;
using ToDoApp.Models;
using ToDoApp.Services;

namespace Tests
{
    public class EmployeesControllerTests
    {
        private readonly Mock<IEmployeeService> _empServiceMock = new();
        private readonly Mock<IWebHostEnvironment> _webHostMock = new();
        private readonly Mock<IHttpClientFactory> _clientFactoryMock = new();
        private readonly Mock<IConfiguration> _configMock = new();
        private readonly EmployeesController _controller;

        public EmployeesControllerTests()
        {
            _controller = new EmployeesController(
                _empServiceMock.Object,
                _webHostMock.Object,
                _clientFactoryMock.Object,
                _configMock.Object);
        }

        [Fact]
        public async Task Index_WithFilters_ReturnsView()
        {
            _empServiceMock.Setup(s => s.GetAllEmployeesAsync("Ivan", "Dev")).ReturnsAsync([]);
            var result = await _controller.Index("Ivan", "Dev") as ViewResult;
            Assert.NotNull(result);
        }

        [Fact]
        public async Task Details_ValidId_ReturnsView()
        {
            _empServiceMock.Setup(s => s.GetEmployeeByIdAsync(1)).ReturnsAsync(new Employee { Id = 1 });
            var result = await _controller.Details(1) as ViewResult;
            Assert.NotNull(result);
        }

        [Fact]
        public async Task Create_PostValidData_RedirectsToIndex()
        {
            var emp = new Employee
            {
                FirstName = "Test",
                LastName = "User",
                Username = "testuser",
                Specialty = "Developer",
                DateOfBirth = DateTime.Now.AddYears(-20),
                HireDate = DateTime.Now
            };

            var fileMock = new Mock<IFormFile>();
            var content = "Fake image content";
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(content));
            fileMock.Setup(f => f.OpenReadStream()).Returns(ms);
            fileMock.Setup(f => f.FileName).Returns("test.png");
            fileMock.Setup(f => f.Length).Returns(ms.Length);
            fileMock.Setup(f => f.ContentType).Returns("image/png");

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"success\":true}"),
                });

            var httpClient = new HttpClient(handlerMock.Object);

            _clientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);
            _configMock.Setup(c => c["AuthApiUrl"]).Returns("http://localhost");

            var result = await _controller.Create(emp, fileMock.Object, "Password123") as RedirectToActionResult;

            Assert.NotNull(result);
            Assert.Equal("Index", result?.ActionName);
            _empServiceMock.Verify(s => s.CreateEmployeeAsync(It.IsAny<Employee>()), Times.Once);
        }

        [Fact]
        public async Task Edit_PostValidData_RedirectsToIndex()
        {
            var emp = new Employee { Id = 1, FirstName = "Old" };
            _empServiceMock.Setup(s => s.GetTrackedEmployeeByIdAsync(1)).ReturnsAsync(emp);

            var result = await _controller.Edit(1, new Employee { Id = 1, FirstName = "New" }, null) as RedirectToActionResult;

            Assert.Equal("Index", result?.ActionName);
            _empServiceMock.Verify(s => s.UpdateEmployeeAsync(It.IsAny<Employee>()), Times.Once);
        }

        [Fact]
        public async Task Delete_GetValidId_ReturnsView()
        {
            _empServiceMock.Setup(s => s.GetEmployeeByIdAsync(1)).ReturnsAsync(new Employee { Id = 1 });
            var result = await _controller.Delete(1) as ViewResult;
            Assert.NotNull(result);
        }
    }
}
