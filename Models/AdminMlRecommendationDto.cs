namespace wordwave.Models
{
    public class AdminMlRecommendationDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int? TaskId { get; set; }
        public string Question { get; set; } = string.Empty;
        public int Difficulty { get; set; }
        public double SuccessProbability { get; set; }
        public int TrainingSamples { get; set; }
        public string ModelName { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }
}
