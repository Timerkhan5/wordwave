using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace wordwave.Models
{
    public class Review
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [Required]
        [JsonPropertyName("userName")]
        public string UserName { get; set; } = string.Empty;
        [Required]
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
        [Range(1,5)]
        [JsonPropertyName("rating")]
        public int Rating { get; set; }
        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [JsonPropertyName("taskId")]
        public int? TaskId { get; set; }
        [ForeignKey("TaskId")]
        public Task? Task { get; set; }
        [JsonPropertyName("materialId")]
        public int? MaterialId { get; set; }
        [ForeignKey("MaterialId")]
        public Material? Material { get; set; }
    }
}
