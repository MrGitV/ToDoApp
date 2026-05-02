using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using ToDoApp.Data;
using ToDoApp.Models;
using ToDoApp.Services;

namespace Tests
{
    public class ServicesEdgeCasesTests
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;

        public ServicesEdgeCasesTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            _context = new ApplicationDbContext(options);
            _cache = new MemoryCache(new MemoryCacheOptions());
        }

        [Fact]
        public async Task EmployeeService_GetEmployeeByIdAsync_CacheHit()
        {
            var service = new EmployeeService(_context, _cache);
            var emp = new Employee { FirstName = "Cache", LastName = "Test" };
            _context.Employees.Add(emp);
            await _context.SaveChangesAsync();

            // First call (loads to cache)
            await service.GetEmployeeByIdAsync(emp.Id);

            // Second call (hits cache, DB not queried again)
            var cachedEmp = await service.GetEmployeeByIdAsync(emp.Id);

            Assert.NotNull(cachedEmp);
            Assert.Equal("Cache", cachedEmp.FirstName);
        }

        [Fact]
        public async Task EmployeeService_DeleteEmployee_Null_DoesNothing()
        {
            var service = new EmployeeService(_context, _cache);
            await service.DeleteEmployeeAsync(999); // Should not throw exception
            Assert.Empty(_context.Employees);
        }

        [Fact]
        public async Task NotificationService_MarkNotificationsAsRead_NoTaskId_MarksAllRead()
        {
            var service = new NotificationService(_context);
            _context.Notifications.AddRange(
                new Notification { RecipientUsername = "user", IsRead = false },
                new Notification { RecipientUsername = "user", IsRead = false }
            );
            await _context.SaveChangesAsync();

            // Mark all for user
            await service.MarkNotificationsAsReadAsync("user", null);

            var unread = await service.GetUnreadNotificationCountAsync("user");
            Assert.Equal(0, unread);
        }

        [Fact]
        public async Task TaskService_GetTaskById_ReturnsTask()
        {
            var service = new TaskService(_context);

            // Create an employee first so INNER JOIN in .Include() works
            var emp = new Employee { FirstName = "John", LastName = "Doe", Username = "jdoe" };
            _context.Employees.Add(emp);
            await _context.SaveChangesAsync();

            // Bind the task to the real employee ID
            var task = new ToDoTask { Title = "Test Task", EmployeeId = emp.Id };
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            // Now GetTaskByIdAsync will successfully find the task with the Employee
            var result = await service.GetTaskByIdAsync(task.Id);

            Assert.NotNull(result);
            Assert.Equal("Test Task", result.Title);
            Assert.NotNull(result.Employee);
        }

        [Fact]
        public async Task TaskService_GetCommentsByTaskId_ReturnsOrdered()
        {
            var service = new TaskService(_context);
            _context.Comments.AddRange(
                new Comment { TaskId = 1, Content = "Second", Timestamp = DateTime.Now },
                new Comment { TaskId = 1, Content = "First", Timestamp = DateTime.Now.AddMinutes(-10) }
            );
            await _context.SaveChangesAsync();

            var result = await service.GetCommentsByTaskIdAsync(1);
            Assert.Equal(2, result.Count());
            Assert.Equal("First", result.First().Content); // Because of OrderBy Timestamp
        }

        [Fact]
        public async Task TaskService_GetAllTasksAsync_WithFilters_FiltersCorrectly()
        {
            var service = new TaskService(_context);

            var emp = new Employee { FirstName = "John", LastName = "Doe", Specialty = "IT", Username = "j1" };
            _context.Employees.Add(emp);
            await _context.SaveChangesAsync();

            _context.Tasks.AddRange(
                new ToDoTask { Title = "Apple", Description = "Fruit", IsCompleted = true, EmployeeId = emp.Id },
                new ToDoTask { Title = "Banana", Description = "Yellow", IsCompleted = false, EmployeeId = emp.Id }
            );
            await _context.SaveChangesAsync();

            var result = await service.GetAllTasksAsync("Apple", "Fruit", true);
            Assert.Single(result);
            Assert.Equal("Apple", result.First().Title);

            var empTasks = await service.GetTasksByEmployeeIdAsync(emp.Id, searchTitle: "Banana", isCompleted: false);
            Assert.Single(empTasks);
            Assert.Equal("Banana", empTasks.First().Title);
        }

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
