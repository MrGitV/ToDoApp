using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using ToDoApp.Models;
using ToDoApp.Models.ViewModels;
using ToDoApp.Services;

namespace ToDoApp.Controllers
{
    [Authorize]
    public class HomeController(IEmployeeService employeeService, ITaskService taskService) : Controller
    {
        private readonly IEmployeeService _employeeService = employeeService;
        private readonly ITaskService _taskService = taskService;

        // Displays the main dashboard for either Admin or Employee.
        public async Task<IActionResult> IndexAsync()
        {
            if (User.IsInRole(UserRole.Admin))
            {
                var allEmployees = await _employeeService.GetAllEmployeesAsync();
                var allTasks = await _taskService.GetAllTasksAsync();
                var adminViewModel = new DashboardViewModel
                {
                    TotalEmployees = allEmployees.Count(),
                    TotalTasks = allTasks.Count(),
                    PendingTasks = allTasks.Count(t => !t.IsCompleted)
                };
                return View(adminViewModel);
            }

            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username)) return Unauthorized();

            var employee = await _employeeService.GetEmployeeByUsernameAsync(username);
            if (employee == null) return NotFound("Employee profile not found.");

            var employeeTasks = await _taskService.GetTasksByEmployeeIdAsync(employee.Id);
            var employeeViewModel = new DashboardViewModel
            {
                TotalTasks = employeeTasks.Count(),
                PendingTasks = employeeTasks.Count(t => !t.IsCompleted)
            };

            return View("Index_Employee", employeeViewModel);
        }

        // Displays a generic error page.
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}