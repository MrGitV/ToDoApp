using AuthAPI.Models;

namespace AuthAPI.Data
{
    public static class AuthDbInitializer
    {
        // Seeds the authentication database with initial user data if it's empty.
        public static void Initialize(AuthDbContext context)
        {
            context.Database.EnsureCreated();

            if (context.Users.Any())
            {
                return;
            }

            var users = new User[]
            {
                new() {
                    Username = "admin",
                    Email = "admin@todoapp.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    Role = "Admin"
                },
                new() {
                    Username = "ivanov",
                    Email = "ivan.ivanov@todoapp.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password1"),
                    Role = "Employee"
                },
                new() {
                    Username = "petrova",
                    Email = "elena.petrova@todoapp.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password2"),
                    Role = "Employee"
                },
                new() {
                    Username = "sidorov",
                    Email = "mikhail.sidorov@todoapp.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password3"),
                    Role = "Employee"
                },
                new() {
                    Username = "kuznetsova",
                    Email = "svetlana.kuznetsova@todoapp.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password4"),
                    Role = "Employee"
                }
            };

            context.Users.AddRange(users);
            context.SaveChanges();
        }
    }
}