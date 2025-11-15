using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToDoApp.Models;
using ToDoApp.Services;

namespace ToDoApp.Controllers
{
    [Authorize(Roles = UserRole.Admin)]
    public class EmployeesController(IEmployeeService employeeService) : Controller
    {
        private readonly IEmployeeService _employeeService = employeeService;

        // Displays a list of all employees with filtering.
        public async Task<IActionResult> IndexAsync(string searchName, string searchSpecialty)
        {
            var employees = await _employeeService.GetAllEmployeesAsync(searchName, searchSpecialty);
            ViewBag.SearchName = searchName;
            ViewBag.SearchSpecialty = searchSpecialty;
            return View(employees);
        }

        // Serves the employee's avatar image.
        [AllowAnonymous]
        public async Task<IActionResult> GetAvatarAsync(int id)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            if (employee?.AvatarImage != null && employee.AvatarImageType != null)
            {
                return File(employee.AvatarImage, employee.AvatarImageType);
            }
            return File("~/images/default-avatar.png", "image/png");
        }

        // Shows details for a specific employee.
        public async Task<IActionResult> DetailsAsync(int id)
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

        // Handles the creation of a new employee.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAsync(Employee employee, IFormFile? avatarFile)
        {
            if (avatarFile != null && avatarFile.Length > 0)
            {
                using var memoryStream = new MemoryStream();
                await avatarFile.CopyToAsync(memoryStream);
                employee.AvatarImage = memoryStream.ToArray();
                employee.AvatarImageType = avatarFile.ContentType;
            }

            if (ModelState.IsValid)
            {
                await _employeeService.CreateEmployeeAsync(employee);
                return RedirectToAction(nameof(IndexAsync));
            }
            return View(employee);
        }

        // Displays the form to edit an employee.
        public async Task<IActionResult> EditAsync(int id)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            if (employee == null) return NotFound();
            return View(employee);
        }

        // Handles updating an employee's details.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAsync(int id, Employee employeeFormData, IFormFile? avatarFile)
        {
            if (id != employeeFormData.Id)
            {
                return NotFound();
            }

            var employeeToUpdate = await _employeeService.GetTrackedEmployeeByIdAsync(id);
            if (employeeToUpdate == null)
            {
                return NotFound();
            }

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
                    if (!await _employeeService.EmployeeExistsAsync(employeeToUpdate.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(IndexAsync));
            }

            return View(employeeToUpdate);
        }

        // Displays the confirmation page for deleting an employee.
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            if (employee == null) return NotFound();
            return View(employee);
        }

        // Deletes the specified employee.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmedAsync(int id)
        {
            await _employeeService.DeleteEmployeeAsync(id);
            return RedirectToAction(nameof(IndexAsync));
        }
    }
}