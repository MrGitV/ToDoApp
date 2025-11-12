using System.ComponentModel.DataAnnotations;

namespace ToDoApp.Models
{
    public class Comment
    {
        public int Id { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; }

        [Required]
        public string AuthorUsername { get; set; } = string.Empty;

        public int TaskId { get; set; }
    }
}