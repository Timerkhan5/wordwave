using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using wordwave.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace wordwave.Controllers
{
    public class ProgressController : Controller
    {
        private readonly AppDbContext _db;
        public ProgressController(AppDbContext db)
        {
            _db = db;
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
            return View();
        }
    }
}
