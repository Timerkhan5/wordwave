using Microsoft.AspNetCore.Mvc;
using wordwave.Models;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace wordwave.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewsController : ControllerBase
    {
        private readonly string _reviewsPath = Path.Combine(Directory.GetCurrentDirectory(), "reviews.json");
        private readonly AppDbContext _db;

        public ReviewsController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult Get()
        {
            if (!System.IO.File.Exists(_reviewsPath))
                return Ok(new List<Review>());
            var json = System.IO.File.ReadAllText(_reviewsPath);
            var reviews = JsonSerializer.Deserialize<List<Review>>(json) ?? new List<Review>();
            return Ok(reviews.OrderByDescending(r => r.CreatedAt).ToList());
        }

        [HttpGet("by-task/{taskId}")]
        public IActionResult GetByTask(int taskId)
        {
            var reviews = _db.Reviews.Where(r => r.TaskId == taskId).OrderByDescending(r => r.CreatedAt).ToList();
            return Ok(reviews);
        }

        [HttpGet("by-material/{materialId}")]
        public IActionResult GetByMaterial(int materialId)
        {
            var reviews = _db.Reviews.Where(r => r.MaterialId == materialId).OrderByDescending(r => r.CreatedAt).ToList();
            return Ok(reviews);
        }

        [HttpPost]
        public IActionResult Create([FromBody] Review review)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            review.CreatedAt = DateTime.UtcNow;
            _db.Reviews.Add(review);
            _db.SaveChanges();
            return Ok(review);
        }
    }
}