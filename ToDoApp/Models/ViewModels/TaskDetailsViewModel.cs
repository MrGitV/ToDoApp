namespace ToDoApp.Models.ViewModels
{
    public class TaskDetailsViewModel
    {
        public ToDoTask Task { get; set; } = null!;
        public IEnumerable<Comment> Comments { get; set; } = [];
    }
}