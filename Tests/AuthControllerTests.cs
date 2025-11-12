using AuthAPI.Controllers;
using AuthAPI.Models;
using AuthAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Tests
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly AuthController _controller;

        // Constructor to set up the test environment with a mock service.
        public AuthControllerTests()
        {
            _mockAuthService = new Mock<IAuthService>();
            _controller = new AuthController(_mockAuthService.Object);
        }

        // Test case for successful user registration.
        [Fact]
        public async Task Register_ValidUser_ReturnsOkResult()
        {
            var registerModel = new RegisterModel { Username = "test", Password = "password123", Email = "test@test.com" };
            _mockAuthService.Setup(s => s.Register(It.IsAny<RegisterModel>())).ReturnsAsync(true);
            var result = await _controller.Register(registerModel);
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value;
            Assert.NotNull(value);
            var property = value.GetType().GetProperty("success");
            Assert.NotNull(property);
            var propertyValue = property.GetValue(value);
            Assert.IsType<bool>(propertyValue);
            Assert.True((bool?)propertyValue);
        }

        // Test case for registration failure when the username already exists.
        [Fact]
        public async Task Register_ExistingUser_ReturnsBadRequest()
        {
            var registerModel = new RegisterModel { Username = "existing", Password = "password123", Email = "test@test.com" };
            _mockAuthService.Setup(s => s.Register(It.IsAny<RegisterModel>())).ThrowsAsync(new Exception("Username already exists"));

            var result = await _controller.Register(registerModel);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        // Test case for successful user login with valid credentials.
        [Fact]
        public async Task Login_ValidCredentials_ReturnsOkResult()
        {
            var loginModel = new LoginModel { Username = "test", Password = "password123" };
            var tokenResponse = new TokenResponse { Token = "valid-token", Expiration = DateTime.UtcNow.AddHours(1) };
            _mockAuthService.Setup(s => s.Login(It.IsAny<LoginModel>())).ReturnsAsync(tokenResponse);
            var result = await _controller.Login(loginModel);
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<TokenResponse>(okResult.Value);
        }

        // Test case for login failure with invalid credentials.
        [Fact]
        public async Task Login_InvalidCredentials_ReturnsUnauthorized()
        {
            var loginModel = new LoginModel { Username = "invalid", Password = "wrong" };
            _mockAuthService.Setup(s => s.Login(It.IsAny<LoginModel>())).ThrowsAsync(new Exception("Invalid credentials"));
            var result = await _controller.Login(loginModel);
            Assert.IsType<UnauthorizedObjectResult>(result);
        }
    }
}