using Microsoft.AspNetCore.Mvc;
using wordwave.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace wordwave.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public ReviewsController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var reviews = _db.Reviews
                .OrderByDescending(r => r.CreatedAt)
                .ToList();
            return Ok(reviews);
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
        [Authorize]
        public IActionResult Create([FromBody] Review review)
        {
            if (review == null) return BadRequest(new { error = "Review body is required" });

            if (review.TaskId == null && review.MaterialId == null)
                return BadRequest(new { error = "TaskId or MaterialId is required" });

            review.CreatedAt = DateTime.UtcNow;
            // Prevent spoofing the author name: always use current user name
            review.UserName = User?.Identity?.Name ?? "user";

            // Re-validate after server-side defaults are applied (UserName, CreatedAt)
            ModelState.Clear();
            TryValidateModel(review);
            if (!ModelState.IsValid) return BadRequest(ModelState);

            _db.Reviews.Add(review);
            _db.SaveChanges();
            return Ok(review);
        }
    }
}