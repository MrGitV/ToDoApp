using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToDoApp.Models;
using ToDoApp.Services;

namespace ToDoApp.Controllers
{
    [Authorize]
    public class ProfileController(IEmployeeService employeeService) : Controller
    {
        private readonly IEmployeeService _employeeService = employeeService;

        public async Task<IActionResult> Index()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username)) return Unauthorized();

            var employee = await _employeeService.GetEmployeeByUsernameAsync(username);

            if (employee == null)
            {
                employee = new Employee
                {
                    Username = username,
                    FirstName = "New",
                    LastName = "User",
                    Specialty = "Please update your profile",
                    DateOfBirth = DateTime.Now.AddYears(-20),
                    HireDate = DateTime.Now
                };

                await _employeeService.CreateEmployeeAsync(employee);

                employee = await _employeeService.GetEmployeeByUsernameAsync(username);
            }

            return View(employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(Employee model, IFormFile? avatarFile)
        {
            var username = User.Identity?.Name;
            var currentEmployee = await _employeeService.GetTrackedEmployeeByIdAsync(model.Id);

            if (currentEmployee == null || currentEmployee.Username != username)
            {
                return Forbid();
            }

            currentEmployee.FirstName = model.FirstName;
            currentEmployee.LastName = model.LastName;
            currentEmployee.DateOfBirth = model.DateOfBirth;

            if (avatarFile != null && avatarFile.Length > 0)
            {
                using var memoryStream = new MemoryStream();
                await avatarFile.CopyToAsync(memoryStream);
                currentEmployee.AvatarImage = memoryStream.ToArray();
                currentEmployee.AvatarImageType = avatarFile.ContentType;
            }

            await _employeeService.UpdateEmployeeAsync(currentEmployee);

            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToAction("Index");
        }
    }
}