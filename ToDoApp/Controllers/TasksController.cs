using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToDoApp.Models;
using ToDoApp.Models.ViewModels;
using ToDoApp.Services;

namespace ToDoApp.Controllers
{
    [Authorize]
    public class TasksController(ITaskService taskService, IEmployeeService employeeService, INotificationService notificationService) : Controller
    {
        private readonly ITaskService _taskService = taskService;
        private readonly IEmployeeService _employeeService = employeeService;
        private readonly INotificationService _notificationService = notificationService;

        // Displays a list of tasks, filtered by user role.
        public async Task<IActionResult> Index(string searchTitle, string searchDescription, bool? isCompleted)
        {
            IEnumerable<ToDoTask> tasks;
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized("Username not found in token.");
            }

            if (User.IsInRole(UserRole.Admin))
            {
                tasks = await _taskService.GetAllTasksAsync(searchTitle, searchDescription, isCompleted);
            }
            else
            {
                var employee = await _employeeService.GetEmployeeByUsernameAsync(username);
                if (employee == null) return Unauthorized("Employee profile not found.");
                tasks = await _taskService.GetTasksByEmployeeIdAsync(employee.Id, searchTitle, searchDescription, isCompleted);
            }

            ViewBag.SearchTitle = searchTitle;
            ViewBag.SearchDescription = searchDescription;
            ViewBag.IsCompleted = isCompleted;
            return View(tasks);
        }

        // Shows details for a specific task, including comments.
        public async Task<IActionResult> Details(int id)
        {
            var task = await _taskService.GetTaskByIdAsync(id);
            if (task == null) return NotFound();

            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }

            if (User.IsInRole(UserRole.Employee))
            {
                var employee = await _employeeService.GetEmployeeByUsernameAsync(username);
                if (task.EmployeeId != employee?.Id)
                {
                    return Forbid();
                }
            }

            var comments = await _taskService.GetCommentsByTaskIdAsync(id);
            var viewModel = new TaskDetailsViewModel
            {
                Task = task,
                Comments = comments
            };

            await _notificationService.MarkNotificationsAsReadAsync(username, id);

            return View(viewModel);
        }

        // Adds a new comment to a task and notifies the relevant user.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int taskId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["Error"] = "Comment cannot be empty.";
                return RedirectToAction("Details", new { id = taskId });
            }

            var task = await _taskService.GetTaskByIdAsync(taskId);
            if (task == null || task.Employee == null) return NotFound();

            var authorName = User.Identity?.Name;
            if (string.IsNullOrEmpty(authorName)) return Unauthorized();

            var comment = new Comment
            {
                Content = content,
                TaskId = taskId,
                AuthorUsername = authorName,
                Timestamp = DateTime.UtcNow
            };

            await _taskService.AddCommentAsync(comment);

            string? employeeUsername = task.Employee.Username;
            if (string.IsNullOrEmpty(employeeUsername)) return RedirectToAction("Details", new { id = taskId });

            string recipientUsername = User.IsInRole(UserRole.Admin) ? employeeUsername : "admin";

            await _notificationService.CreateNotificationAsync(
                recipientUsername,
                $"New comment on task '{task.Title}' by {authorName}.",
                taskId);

            return RedirectToAction("Details", new { id = taskId });
        }


        // Displays the form to create a new task.
        [Authorize(Roles = UserRole.Admin)]
        public async Task<IActionResult> Create(int? employeeId = null)
        {
            ViewBag.Employees = await _employeeService.GetAllEmployeesAsync();
            ViewBag.EmployeeId = employeeId;
            return View();
        }

        // Handles the creation of a new task and notifies the assignee.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = UserRole.Admin)]
        public async Task<IActionResult> Create(ToDoTask task)
        {
            if (ModelState.IsValid)
            {
                await _taskService.CreateTaskAsync(task);

                var employee = await _employeeService.GetEmployeeByIdAsync(task.EmployeeId);
                if (employee != null && !string.IsNullOrEmpty(employee.Username))
                {
                    await _notificationService.CreateNotificationAsync(
                        employee.Username,
                        $"You have been assigned a new task: '{task.Title}'.",
                        task.Id
                    );
                }

                return RedirectToAction(nameof(Index));
            }
            ViewBag.Employees = await _employeeService.GetAllEmployeesAsync();
            return View(task);
        }

        // Displays the form to edit a task.
        [Authorize(Roles = UserRole.Admin)]
        public async Task<IActionResult> Edit(int id)
        {
            var task = await _taskService.GetTaskByIdAsync(id);
            if (task == null)
            {
                return NotFound();
            }
            ViewBag.Employees = await _employeeService.GetAllEmployeesAsync();
            return View(task);
        }

        // Handles updating a task's details.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = UserRole.Admin)]
        public async Task<IActionResult> Edit(int id, ToDoTask task)
        {
            if (id != task.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _taskService.UpdateTaskAsync(task);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await TaskExists(task.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Employees = await _employeeService.GetAllEmployeesAsync();
            return View(task);
        }

        // Displays the confirmation page for deleting a task.
        [Authorize(Roles = UserRole.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var task = await _taskService.GetTaskByIdAsync(id);
            if (task == null)
            {
                return NotFound();
            }
            return View(task);
        }

        // Deletes the specified task.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = UserRole.Admin)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _taskService.DeleteTaskAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // Checks if a task with the given ID exists.
        private async Task<bool> TaskExists(int id)
        {
            return await _taskService.GetTaskByIdAsync(id) != null;
        }
    }
}