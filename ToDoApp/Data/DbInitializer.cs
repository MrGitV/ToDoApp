using ToDoApp.Models;

namespace ToDoApp.Data
{
    public static class DbInitializer
    {
        // Seeds the main application database with initial data if it's empty.
        public static void Initialize(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            context.Database.EnsureCreated();
            if (context.Employees.Any()) return;

            byte[]? ReadImage(string fileName)
            {
                var filePath = Path.Combine(webHostEnvironment.WebRootPath, "seed-images", fileName);
                return File.Exists(filePath) ? File.ReadAllBytes(filePath) : null;
            }

            var employees = new Employee[]
            {
                new() {
                    Username = "admin", FirstName = "System", LastName = "Administrator", DateOfBirth = new DateTime(1985, 5, 20),
                    Specialty = "General Manager", HireDate = new DateTime(2015, 1, 1),
                    AvatarImage = ReadImage("default-avatar.png"), AvatarImageType = "image/png"
                },
                new() {
                    Username = "ivanov", FirstName = "Ivan", LastName = "Ivanov", DateOfBirth = new DateTime(1988, 5, 20),
                    Specialty = "Software Developer", HireDate = new DateTime(2020, 1, 15),
                    AvatarImage = ReadImage("1.png"), AvatarImageType = "image/png"
                },
                new() {
                    Username = "petrova", FirstName = "Elena", LastName = "Petrova", DateOfBirth = new DateTime(1995, 8, 15),
                    Specialty = "UI/UX Designer", HireDate = new DateTime(2021, 3, 22),
                    AvatarImage = ReadImage("2.png"), AvatarImageType = "image/png"
                },
                new() {
                    Username = "sidorov", FirstName = "Mikhail", LastName = "Sidorov", DateOfBirth = new DateTime(1981, 11, 30),
                    Specialty = "Project Manager", HireDate = new DateTime(2019, 11, 5),
                    AvatarImage = ReadImage("3.png"), AvatarImageType = "image/png"
                },
                new() {
                    Username = "kuznetsova", FirstName = "Svetlana", LastName = "Kuznetsova", DateOfBirth = new DateTime(1992, 2, 10),
                    Specialty = "Quality Assurance", HireDate = new DateTime(2022, 2, 18),
                    AvatarImage = ReadImage("4.png"), AvatarImageType = "image/png"
                }
            };
            context.Employees.AddRange(employees);
            context.SaveChanges();

            var tasks = new ToDoTask[]
            {
                new() { Title = "Develop user authentication", IsCompleted = true, EmployeeId = employees[0].Id, DueDate = DateTime.Now.AddDays(-5) },
                new() { Title = "Design homepage layout", IsCompleted = false, EmployeeId = employees[1].Id, DueDate = DateTime.Now.AddDays(3) },
                new() { Title = "Project planning meeting", IsCompleted = false, EmployeeId = employees[2].Id, DueDate = DateTime.Now.AddDays(-2) },
                new() { Title = "Write test cases", IsCompleted = false, EmployeeId = employees[3].Id, DueDate = DateTime.Now.AddDays(5) },
                new() { Title = "API integration", IsCompleted = false, EmployeeId = employees[0].Id, DueDate = DateTime.Now.AddDays(1) },
                new() { Title = "User feedback analysis", IsCompleted = true, EmployeeId = employees[1].Id, DueDate = DateTime.Now.AddDays(-1) }
            };
            context.Tasks.AddRange(tasks);
            context.SaveChanges();
        }
    }
}