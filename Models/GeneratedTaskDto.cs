namespace wordwave.Models
{
    public class GeneratedTaskDto
    {
        public string Question { get; set; } = string.Empty;
        public string[] Options { get; set; } = new string[0];
        public int CorrectOption { get; set; }
        public int Difficulty { get; set; }
        public bool IsExam { get; set; }
    }
}
