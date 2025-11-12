using Microsoft.EntityFrameworkCore;
using ToDoApp.Data;
using ToDoApp.Models;

namespace ToDoApp.Services
{
    public class TaskService(ApplicationDbContext context) : ITaskService
    {
        private readonly ApplicationDbContext _context = context;

        // Gets a list of all tasks with optional filtering.
        public async Task<IEnumerable<ToDoTask>> GetAllTasksAsync(string? searchTitle = null, string? searchDescription = null, bool? isCompleted = null)
        {
            var query = _context.Tasks.Include(t => t.Employee).AsQueryable();

            if (!string.IsNullOrEmpty(searchTitle))
            {
                query = query.Where(t => t.Title.Contains(searchTitle));
            }

            if (!string.IsNullOrEmpty(searchDescription))
            {
                query = query.Where(t => t.Description != null && t.Description.Contains(searchDescription));
            }

            if (isCompleted.HasValue)
            {
                query = query.Where(t => t.IsCompleted == isCompleted.Value);
            }

            return await query.AsNoTracking().ToListAsync();
        }

        // Gets a single task by its ID.
        public async Task<ToDoTask?> GetTaskByIdAsync(int id)
        {
            return await _context.Tasks
                .Include(t => t.Employee)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        // Adds a new task to the database.
        public async Task CreateTaskAsync(ToDoTask task)
        {
            await _context.Tasks.AddAsync(task);
            await _context.SaveChangesAsync();
        }

        // Saves changes to an existing task.
        public async Task UpdateTaskAsync(ToDoTask task)
        {
            _context.Tasks.Update(task);
            await _context.SaveChangesAsync();
        }

        // Removes a task from the database.
        public async Task DeleteTaskAsync(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task != null)
            {
                _context.Tasks.Remove(task);
                await _context.SaveChangesAsync();
            }
        }

        // Gets all tasks assigned to a specific employee, with filtering.
        public async Task<IEnumerable<ToDoTask>> GetTasksByEmployeeIdAsync(int employeeId, string? searchTitle = null, string? searchDescription = null, bool? isCompleted = null)
        {
            var query = _context.Tasks
                .Where(t => t.EmployeeId == employeeId)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTitle))
            {
                query = query.Where(t => t.Title.Contains(searchTitle));
            }

            if (!string.IsNullOrEmpty(searchDescription))
            {
                query = query.Where(t => t.Description != null && t.Description.Contains(searchDescription));
            }

            if (isCompleted.HasValue)
            {
                query = query.Where(t => t.IsCompleted == isCompleted.Value);
            }

            return await query.AsNoTracking().ToListAsync();
        }

        // Retrieves all comments for a specific task.
        public async Task<IEnumerable<Comment>> GetCommentsByTaskIdAsync(int taskId)
        {
            return await _context.Comments
                .Where(c => c.TaskId == taskId)
                .OrderBy(c => c.Timestamp)
                .AsNoTracking()
                .ToListAsync();
        }

        // Adds a new comment to the database.
        public async Task AddCommentAsync(Comment comment)
        {
            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();
        }
    }
}