using ToDoApp.Models;

namespace ToDoApp.Services
{
    public interface ITaskService
    {
        Task<IEnumerable<ToDoTask>> GetAllTasksAsync(string? searchTitle = null, string? searchDescription = null, bool? isCompleted = null);
        Task<ToDoTask?> GetTaskByIdAsync(int id);
        Task CreateTaskAsync(ToDoTask task);
        Task UpdateTaskAsync(ToDoTask task);
        Task DeleteTaskAsync(int id);
        Task<IEnumerable<ToDoTask>> GetTasksByEmployeeIdAsync(int employeeId, string? searchTitle = null, string? searchDescription = null, bool? isCompleted = null);
        Task<IEnumerable<Comment>> GetCommentsByTaskIdAsync(int taskId);
        Task AddCommentAsync(Comment comment);
    }
}