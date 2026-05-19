namespace wordwave.Models
{
    public class TaskRecommendationDto
    {
        public int? TaskId { get; set; }
        public string Question { get; set; } = string.Empty;
        public int Difficulty { get; set; }
        public bool IsExam { get; set; }
        public double SuccessProbability { get; set; }
        public double Score { get; set; }
        public int TrainingSamples { get; set; }
        public string ModelName { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }
}
