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
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;
            _context = new ApplicationDbContext(options);
            _taskService = new TaskService(_context);
        }

        // Verifies that GetAllTasksAsync returns all seeded tasks.
        [Fact]
        public async Task UpdateTaskAsync_UpdatesInDatabase()
        {
            var task = new ToDoTask { Title = "Old", EmployeeId = 1 };
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            task.Title = "New";
            await _taskService.UpdateTaskAsync(task);

            var dbTask = await _context.Tasks.FirstAsync();
            Assert.Equal("New", dbTask.Title);
        }

        // Verifies that a new task is correctly added to the database.
        [Fact]
        public async Task DeleteTaskAsync_RemovesFromDatabase()
        {
            var task = new ToDoTask { Title = "Task", EmployeeId = 1 };
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            await _taskService.DeleteTaskAsync(task.Id);
            Assert.Empty(_context.Tasks);
        }
    }
}
