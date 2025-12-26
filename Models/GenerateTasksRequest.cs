namespace wordwave.Models
{
    public class GenerateTasksRequest
    {
        public string Topic { get; set; } = string.Empty;
        public int Count { get; set; } = 5;
        // optional hint to LLM: prefer tasks of this difficulty (1..5)
        public int Difficulty { get; set; } = 1;
    }
}
