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
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);
            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            _employeeService = new EmployeeService(_context, memoryCache);
        }

        // Verifies that GetAllEmployeesAsync returns all seeded employees.
        [Fact]
        public async Task GetAllEmployeesAsync_ReturnsAllEmployees()
        {
            _context.Employees.AddRange(
                new Employee { FirstName = "Ivan", LastName = "Ivanov", DateOfBirth = DateTime.Now.AddYears(-30), Specialty = "Developer", HireDate = DateTime.Now, Username = "ivanov" },
                new Employee { FirstName = "Elena", LastName = "Petrova", DateOfBirth = DateTime.Now.AddYears(-25), Specialty = "Designer", HireDate = DateTime.Now, Username = "petrova" }
            );
            await _context.SaveChangesAsync();
            var result = await _employeeService.GetAllEmployeesAsync();
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        // Verifies that a new employee is correctly added to the database.
        [Fact]
        public async Task CreateEmployeeAsync_ValidEmployee_AddsEmployeeToDatabase()
        {
            var employee = new Employee { FirstName = "Mikhail", LastName = "Sidorov", DateOfBirth = DateTime.Now.AddYears(-40), Specialty = "Manager", HireDate = DateTime.Now, Username = "sidorov" };

            await _employeeService.CreateEmployeeAsync(employee);

            var savedEmployee = await _context.Employees.FirstOrDefaultAsync(e => e.FirstName == "Mikhail");
            Assert.NotNull(savedEmployee);
            Assert.Equal("Sidorov", savedEmployee.LastName);
            Assert.Equal(40, savedEmployee.Age);
        }
    }
}