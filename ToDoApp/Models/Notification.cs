using System.ComponentModel.DataAnnotations;

namespace ToDoApp.Models
{
    public class Notification
    {
        public int Id { get; set; }

        [Required]
        public string RecipientUsername { get; set; } = string.Empty;

        [Required]
        public string Message { get; set; } = string.Empty;

        public bool IsRead { get; set; } = false;

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public int? TaskId { get; set; }
    }
}