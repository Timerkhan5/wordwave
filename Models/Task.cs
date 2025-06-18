using System.ComponentModel.DataAnnotations;

namespace wordwave.Models
{
    public class Task
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public required string Question { get; set; }
        [Required]
        public required string Option1 { get; set; }
        [Required]
        public required string Option2 { get; set; }
        [Required]
        public required string Option3 { get; set; }
        [Required]
        public required string Option4 { get; set; }
        [Required]
        public int CorrectOption { get; set; }
        public int Difficulty { get; set; }
        public bool IsExam { get; set; }
    }
}
