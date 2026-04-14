using System.ComponentModel.DataAnnotations.Schema;

namespace ToDoApp.Models
{
    public class ToDoTask
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        public bool IsCompleted { get; set; } = false;
        public int EmployeeId { get; set; }
        public Employee? Employee { get; set; }
        [NotMapped]
        public bool IsOverdue => !IsCompleted && DueDate.HasValue && DueDate.Value.Date < DateTime.Now.Date;
    }
}