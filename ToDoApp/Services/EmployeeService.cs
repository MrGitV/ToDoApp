using Microsoft.EntityFrameworkCore;
using ToDoApp.Data;
using ToDoApp.Models;
using Microsoft.Extensions.Caching.Memory;

namespace ToDoApp.Services
{
    public class EmployeeService(ApplicationDbContext context, IMemoryCache cache) : IEmployeeService
    {
        private readonly ApplicationDbContext _context = context;
        private readonly IMemoryCache _cache = cache;

        // Gets a list of all employees, with optional name and specialty filtering.
        public async Task<IEnumerable<Employee>> GetAllEmployeesAsync(string? searchName = null, string? searchSpecialty = null)
        {
            var query = _context.Employees.AsQueryable();

            if (!string.IsNullOrEmpty(searchName))
            {
                query = query.Where(e => e.FirstName.Contains(searchName) || e.LastName.Contains(searchName));
            }

            if (!string.IsNullOrEmpty(searchSpecialty))
            {
                query = query.Where(e => e.Specialty.Contains(searchSpecialty));
            }

            return await query.AsNoTracking().ToListAsync();
        }

        // Gets a single employee by ID, using caching.
        public async Task<Employee?> GetEmployeeByIdAsync(int id)
        {
            string cacheKey = $"Employee_{id}";
            if (_cache.TryGetValue(cacheKey, out Employee? employee))
            {
                return employee;
            }
            employee = await _context.Employees
                .Include(e => e.Tasks)
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id);
            if (employee != null)
            {
                _cache.Set(cacheKey, employee, TimeSpan.FromMinutes(5));
            }

            return employee;
        }

        // Gets a tracked employee entity by ID for updating.
        public async Task<Employee?> GetTrackedEmployeeByIdAsync(int id)
        {
            return await _context.Employees.FindAsync(id);
        }

        // Gets a single employee by their username.
        public async Task<Employee?> GetEmployeeByUsernameAsync(string username)
        {
            return await _context.Employees
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Username == username);
        }

        // Adds a new employee to the database.
        public async Task CreateEmployeeAsync(Employee employee)
        {
            await _context.Employees.AddAsync(employee);
            await _context.SaveChangesAsync();
        }

        // Saves changes to an existing employee and clears the cache.
        public async Task UpdateEmployeeAsync(Employee employee)
        {
            await _context.SaveChangesAsync();
            _cache.Remove($"Employee_{employee.Id}");
        }

        // Removes an employee from the database and clears the cache.
        public async Task DeleteEmployeeAsync(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee != null)
            {
                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();
                _cache.Remove($"Employee_{id}");
            }
        }

        // Checks if an employee exists with the given ID.
        public async Task<bool> EmployeeExists(int id)
        {
            return await _context.Employees.AnyAsync(e => e.Id == id);
        }

        // Performs a generic search across multiple employee fields.
        public async Task<IEnumerable<Employee>> SearchEmployeesAsync(string searchTerm)
        {
            return await _context.Employees
                .Where(e => e.FirstName.Contains(searchTerm) ||
                           e.LastName.Contains(searchTerm) ||
                           e.Specialty.Contains(searchTerm))
                .AsNoTracking()
                .ToListAsync();
        }
    }
}