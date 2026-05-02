using AuthAPI.Data;
using AuthAPI.Models;
using AuthAPI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Tests
{
    public class AuthServiceTests
    {
        private readonly AuthDbContext _context;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            var options = new DbContextOptionsBuilder<AuthDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            _context = new AuthDbContext(options);

            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(c => c["Jwt:Key"]).Returns("SuperSecretKeyThatIsAtLeast32BytesLongForTesting");

            _authService = new AuthService(_context, mockConfig.Object);
        }

        // Tests successful user registration
        [Fact]
        public async Task RegisterAsync_ValidData_ReturnsTrue()
        {
            var model = new RegisterModel { Username = "user1", Password = "123", Email = "test@test.com" };
            var result = await _authService.RegisterAsync(model);

            Assert.True(result);
            Assert.Single(_context.Users);
        }

        // Tests login with valid credentials
        [Fact]
        public async Task LoginAsync_ValidUser_ReturnsToken()
        {
            await _authService.RegisterAsync(new RegisterModel { Username = "user2", Password = "123" });

            var result = await _authService.LoginAsync(new LoginModel { Username = "user2", Password = "123" });

            Assert.NotNull(result);
            Assert.NotEmpty(result.Token);
            Assert.Equal("user2", result.Username);
        }

        // Tests login failure on wrong password
        [Fact]
        public async Task LoginAsync_InvalidUser_ThrowsException()
        {
            await Assert.ThrowsAsync<Exception>(() =>
                _authService.LoginAsync(new LoginModel { Username = "nonexistent", Password = "bad" }));
        }
    }
}
