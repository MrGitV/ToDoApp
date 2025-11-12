using Microsoft.EntityFrameworkCore;
using ToDoApp.Data;
using ToDoApp.Models;
using ToDoApp.Services;

namespace ToDoApp.Tests
{
    public class TaskServiceTests
    {
        private readonly ApplicationDbContext _context;
        private readonly TaskService _taskService;

        // Initializes a new in-memory database for each test.
        public TaskServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _taskService = new TaskService(_context);
        }

        // Verifies that GetAllTasksAsync returns all seeded tasks.
        [Fact]
        public async Task GetAllTasksAsync_ReturnsAllTasks()
        {
            var employee = new Employee { Id = 1, FirstName = "Test", LastName = "User" };
            _context.Employees.Add(employee);
            _context.Tasks.AddRange(
                new ToDoTask { Title = "Task 1", IsCompleted = false, Employee = employee },
                new ToDoTask { Title = "Task 2", IsCompleted = true, Employee = employee }
            );
            await _context.SaveChangesAsync();
            var result = await _taskService.GetAllTasksAsync();
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        // Verifies that a new task is correctly added to the database.
        [Fact]
        public async Task CreateTaskAsync_ValidTask_AddsTaskToDatabase()
        {
            var employee = new Employee { Id = 1, FirstName = "Test", LastName = "User" };
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
            var task = new ToDoTask { Title = "New Task", Description = "Description", EmployeeId = 1 };
            await _taskService.CreateTaskAsync(task);
            var savedTask = await _context.Tasks.FirstOrDefaultAsync(t => t.Title == "New Task");
            Assert.NotNull(savedTask);
            Assert.Equal(1, savedTask.EmployeeId);
        }
    }
}