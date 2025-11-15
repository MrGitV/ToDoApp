using ToDoApp.Models;

namespace ToDoApp.Services
{
    public interface IEmployeeService
    {
        Task<IEnumerable<Employee>> GetAllEmployeesAsync(string? searchName = null, string? searchSpecialty = null);
        Task<Employee?> GetEmployeeByIdAsync(int id);
        Task<Employee?> GetTrackedEmployeeByIdAsync(int id);
        Task<Employee?> GetEmployeeByUsernameAsync(string username);
        Task CreateEmployeeAsync(Employee employee);
        Task UpdateEmployeeAsync(Employee employee);
        Task DeleteEmployeeAsync(int id);
        Task<bool> EmployeeExistsAsync(int id);
        Task<IEnumerable<Employee>> SearchEmployeesAsync(string searchTerm);
    }
}