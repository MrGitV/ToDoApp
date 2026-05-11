using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;
using ToDoApp.Models;
using ToDoApp.Services;

namespace ToDoApp.Controllers
{
    [Authorize(Roles = UserRole.Admin)]
    public class EmployeesController(
        IEmployeeService employeeService,
        IWebHostEnvironment webHostEnvironment,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration) : Controller
    {
        private readonly IEmployeeService _employeeService = employeeService;
        private readonly IWebHostEnvironment _webHostEnvironment = webHostEnvironment;
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
        private readonly IConfiguration _configuration = configuration;

        // Displays a list of all employees with optional filtering by name and specialty.
        public async Task<IActionResult> Index(string searchName, string searchSpecialty)
        {
            var employees = await _employeeService.GetAllEmployeesAsync(searchName, searchSpecialty);
            ViewBag.SearchName = searchName;
            ViewBag.SearchSpecialty = searchSpecialty;
            return View(employees);
        }

        // Serves the employee's avatar image from the database or returns a default one.
        [AllowAnonymous]
        public async Task<IActionResult> GetAvatar(int id)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            if (employee?.AvatarImage != null && employee.AvatarImageType != null)
            {
                return File(employee.AvatarImage, employee.AvatarImageType);
            }
            return File("~/images/default-avatar.png", "image/png");
        }

        // Shows detailed information for a specific employee.
        public async Task<IActionResult> Details(int id)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            if (employee == null) return NotFound();
            return View(employee);
        }

        // Displays the form to create a new employee.
        public IActionResult Create()
        {
            return View();
        }

        // Handles the creation of a new employee, checks duplicates, and registers in AuthAPI.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Employee employee, IFormFile? avatarFile, string? TempPassword)
        {
            var existingUser = await _employeeService.GetEmployeeByUsernameAsync(employee.Username);
            if (existingUser != null)
            {
                ModelState.AddModelError("Username", "This username is already taken by another employee.");
            }

            if (string.IsNullOrWhiteSpace(TempPassword))
            {
                ModelState.AddModelError("TempPassword", "A temporary password is required for the new account.");
            }

            if (avatarFile != null && avatarFile.Length > 0)
            {
                using var memoryStream = new MemoryStream();
                await avatarFile.CopyToAsync(memoryStream);
                employee.AvatarImage = memoryStream.ToArray();
                employee.AvatarImageType = avatarFile.ContentType;
            }
            else
            {
                var defaultAvatarPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "default-avatar.png");
                if (!System.IO.File.Exists(defaultAvatarPath))
                {
                    defaultAvatarPath = Path.Combine(_webHostEnvironment.WebRootPath, "seed-images", "default-avatar.png");
                }

                if (System.IO.File.Exists(defaultAvatarPath))
                {
                    employee.AvatarImage = await System.IO.File.ReadAllBytesAsync(defaultAvatarPath);
                    employee.AvatarImageType = "image/png";
                }
            }

            if (ModelState.IsValid)
            {
                var authApiUrl = _configuration["AuthApiUrl"] ?? "https://localhost:7001";
                var client = _httpClientFactory.CreateClient();

                var registerData = new
                {
                    employee.Username,
                    Password = TempPassword,
                    Email = $"{employee.Username}@todoapp.com",
                    Role = UserRole.Employee
                };

                var content = new StringContent(JsonSerializer.Serialize(registerData), Encoding.UTF8, "application/json");

                try
                {
                    var response = await client.PostAsync($"{authApiUrl}/api/auth/register", content);
                    if (!response.IsSuccessStatusCode)
                    {
                        var responseBody = await response.Content.ReadAsStringAsync();
                        ModelState.AddModelError(string.Empty, $"AuthAPI Error: The username might already exist in the authentication database.");
                        return View(employee);
                    }
                }
                catch (HttpRequestException)
                {
                    ModelState.AddModelError(string.Empty, "Failed to connect to the Authentication Service. Ensure AuthAPI is running.");
                    return View(employee);
                }

                await _employeeService.CreateEmployeeAsync(employee);
                return RedirectToAction(nameof(Index));
            }
            return View(employee);
        }

        // Displays the form to edit an existing employee.
        public async Task<IActionResult> Edit(int id)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            if (employee == null) return NotFound();
            return View(employee);
        }

        // Handles updating an employee's details and overriding their avatar if a new one is provided.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Employee employeeFormData, IFormFile? avatarFile)
        {
            if (id != employeeFormData.Id) return NotFound();

            var existingUser = await _employeeService.GetEmployeeByUsernameAsync(employeeFormData.Username);
            if (existingUser != null && existingUser.Id != id)
            {
                ModelState.AddModelError("Username", "This username is already taken by another employee.");
            }

            var employeeToUpdate = await _employeeService.GetTrackedEmployeeByIdAsync(id);
            if (employeeToUpdate == null) return NotFound();

            if (ModelState.IsValid)
            {
                employeeToUpdate.FirstName = employeeFormData.FirstName;
                employeeToUpdate.LastName = employeeFormData.LastName;
                employeeToUpdate.Username = employeeFormData.Username;
                employeeToUpdate.DateOfBirth = employeeFormData.DateOfBirth;
                employeeToUpdate.Specialty = employeeFormData.Specialty;
                employeeToUpdate.HireDate = employeeFormData.HireDate;
                
                if (avatarFile != null && avatarFile.Length > 0)
                {
                    using var memoryStream = new MemoryStream();
                    await avatarFile.CopyToAsync(memoryStream);
                    employeeToUpdate.AvatarImage = memoryStream.ToArray();
                    employeeToUpdate.AvatarImageType = avatarFile.ContentType;
                }

                try
                {
                    await _employeeService.UpdateEmployeeAsync(employeeToUpdate);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _employeeService.EmployeeExistsAsync(employeeToUpdate.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(employeeToUpdate);
        }

        // Displays the confirmation page for deleting an employee.
        public async Task<IActionResult> Delete(int id)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            if (employee == null) return NotFound();
            return View(employee);
        }

        // Deletes the specified employee from the database.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _employeeService.DeleteEmployeeAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}