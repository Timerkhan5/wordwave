using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace wordwave.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public required string UserName { get; set; }
        [Required]
        public required string PasswordHash { get; set; }
        public DateTime RegisteredAt { get; set; }
        public string Role { get; set; } = "user";
    }
}
