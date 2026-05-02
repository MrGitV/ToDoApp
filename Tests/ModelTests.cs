using ToDoApp.Models;

namespace Tests
{
    public class ModelTests
    {
        // Tests if Employee Age is calculated correctly
        [Fact]
        public void Employee_Age_CalculatedCorrectly()
        {
            var employee = new Employee
            {
                DateOfBirth = DateTime.Today.AddYears(-30).AddDays(-1)
            };

            Assert.Equal(30, employee.Age);
        }

        // Tests if Task is marked overdue correctly
        [Fact]
        public void ToDoTask_IsOverdue_ReturnsTrueIfPastDueDate()
        {
            var task = new ToDoTask
            {
                IsCompleted = false,
                DueDate = DateTime.Now.AddDays(-1)
            };

            Assert.True(task.IsOverdue);
        }

        // Tests if Task is not overdue when completed
        [Fact]
        public void ToDoTask_IsOverdue_ReturnsFalseIfCompleted()
        {
            var task = new ToDoTask
            {
                IsCompleted = true,
                DueDate = DateTime.Now.AddDays(-1)
            };

            Assert.False(task.IsOverdue);
        }
    }
}
