using AuthAPI.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Moq;
using ToDoApp.Data;

namespace Tests
{
    public class DataInitializersTests
    {
        // Tests ToDoApp DbInitializer
        [Fact]
        public void AppDbInitializer_SeedsData_IfEmpty()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            using var context = new ApplicationDbContext(options);

            var envMock = new Mock<IWebHostEnvironment>();
            envMock.Setup(e => e.WebRootPath).Returns("");

            DbInitializer.Initialize(context, envMock.Object);

            Assert.True(context.Employees.Any());
            Assert.True(context.Tasks.Any());
        }

        // Tests AuthAPI DbInitializer
        [Fact]
        public void AuthDbInitializer_SeedsData_IfEmpty()
        {
            var options = new DbContextOptionsBuilder<AuthDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            using var context = new AuthDbContext(options);

            AuthDbInitializer.Initialize(context);

            Assert.True(context.Users.Any());
            Assert.Contains(context.Users, u => u.Username == "admin");
        }
    }
}
