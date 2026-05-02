using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using ToDoApp.Data;
using ToDoApp.Models;
using ToDoApp.Services;

namespace ToDoApp.Tests
{
    public class EmployeeServiceTests
    {
        private readonly ApplicationDbContext _context;
        private readonly EmployeeService _employeeService;

        // Initializes a new in-memory database for each test.
        public EmployeeServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;
            _context = new ApplicationDbContext(options);
            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            _employeeService = new EmployeeService(_context, memoryCache);
        }

        // Verifies that GetAllEmployeesAsync returns all seeded employees.
        [Fact]
        public async Task GetAllEmployeesAsync_ReturnsAllEmployees()
        {
            _context.Employees.Add(new Employee { FirstName = "Ivan", LastName = "Ivanov", Username = "iv" });
            await _context.SaveChangesAsync();
            var result = await _employeeService.GetAllEmployeesAsync();
            Assert.Single(result);
        }

        // Verifies that a new employee is correctly added to the database.
        [Fact]
        public async Task SearchEmployeesAsync_ReturnsMatches()
        {
            _context.Employees.Add(new Employee { FirstName = "Ivan", LastName = "Ivanov", Specialty = "Dev", Username = "iv" });
            await _context.SaveChangesAsync();
            var result = await _employeeService.SearchEmployeesAsync("Ivan");
            Assert.Single(result);
        }

        [Fact]
        public async Task DeleteEmployeeAsync_RemovesFromDatabase()
        {
            var emp = new Employee { FirstName = "Test", LastName = "User", Username = "test" };
            _context.Employees.Add(emp);
            await _context.SaveChangesAsync();

            await _employeeService.DeleteEmployeeAsync(emp.Id);
            Assert.Empty(_context.Employees);
        }
    }
}
