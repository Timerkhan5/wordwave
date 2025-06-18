using System.ComponentModel.DataAnnotations;

namespace wordwave.Models
{
    public class UserTaskProgress
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string UserId { get; set; } = null!;
        [Required]
        public int TaskId { get; set; }
        public bool IsCompleted { get; set; }
    }
}
