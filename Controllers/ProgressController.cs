using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using wordwave.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using wordwave.Services;

namespace wordwave.Controllers
{
    public class ProgressController : Controller
    {
        private readonly AppDbContext _db;
        private readonly AdaptiveTaskRecommendationService _recommendations;

        public ProgressController(AppDbContext db, AdaptiveTaskRecommendationService recommendations)
        {
            _db = db;
            _recommendations = recommendations;
        }

        [Authorize]
        public IActionResult Index()
        {
            var userId = User.FindFirst("UserId")?.Value;
            var tasks = _db.Tasks.OrderBy(t => t.Id).ToList();
            var progress = _db.UserTaskProgresses
                .Where(p => p.UserId == userId)
                .ToList();
            ViewBag.Tasks = tasks;
            ViewBag.Progress = progress;
            if (userId != null)
            {
                ViewBag.Recommendation = _recommendations.RecommendNextTask(userId);
            }
            return View();
        }
    }
}
