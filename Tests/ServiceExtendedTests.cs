using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using ToDoApp.Data;
using ToDoApp.Models;
using ToDoApp.Services;

namespace Tests
{
    public class ServiceExtendedTests
    {
        private readonly ApplicationDbContext _context;

        public ServiceExtendedTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            _context = new ApplicationDbContext(options);
        }

        // Tests if task filters work correctly
        [Fact]
        public async Task TaskService_GetAllTasksAsync_WithFilters_FiltersCorrectly()
        {
            var service = new TaskService(_context);

            // 1. Create and add an employee first to satisfy the Foreign Key and .Include()
            var emp = new Employee { FirstName = "John", LastName = "Doe", Specialty = "IT", Username = "j1" };
            _context.Employees.Add(emp);
            await _context.SaveChangesAsync();

            // 2. Add tasks linked to the employee
            _context.Tasks.AddRange(
                new ToDoTask { Title = "Apple", Description = "Fruit", IsCompleted = true, EmployeeId = emp.Id },
                new ToDoTask { Title = "Banana", Description = "Yellow", IsCompleted = false, EmployeeId = emp.Id }
            );
            await _context.SaveChangesAsync();

            // Test 1: Search by multiple text and bool filters
            var result = await service.GetAllTasksAsync("Apple", "Fruit", true);
            Assert.Single(result);
            Assert.Equal("Apple", result.First().Title);

            // Test 2: Search specific employee's tasks by filter
            var empTasks = await service.GetTasksByEmployeeIdAsync(emp.Id, searchTitle: "Banana", isCompleted: false);
            Assert.Single(empTasks);
            Assert.Equal("Banana", empTasks.First().Title);
        }

        // Tests employee filters 
        [Fact]
        public async Task EmployeeService_GetAllEmployeesAsync_WithFilters()
        {
            var service = new EmployeeService(_context, new MemoryCache(new MemoryCacheOptions()));

            _context.Employees.AddRange(
                new Employee { FirstName = "John", LastName = "Doe", Specialty = "IT", Username = "j1" },
                new Employee { FirstName = "Jane", LastName = "Smith", Specialty = "HR", Username = "j2" }
            );
            await _context.SaveChangesAsync();

            var result = await service.GetAllEmployeesAsync("John", "IT");

            Assert.Single(result);
            Assert.Equal("Doe", result.First().LastName);
        }
    }
}
