using Microsoft.AspNetCore.Mvc;
using wordwave.Models;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace wordwave.Controllers
{
    public class TaskController : Controller
    {
        private readonly AppDbContext _db;
        public TaskController(AppDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            var tasks = _db.Tasks.OrderBy(t => t.Id).ToList();
            var userId = User.FindFirst("UserId")?.Value;
            var progress = userId != null
                ? _db.UserTaskProgresses.Where(p => p.UserId == userId).ToList()
                : new List<UserTaskProgress>();
            ViewBag.Progress = progress;
            return View("Index", tasks);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [Authorize]
        public IActionResult Complete(int id)
        {
            var userId = User.FindFirst("UserId")?.Value;
            if (userId == null) return Unauthorized();
            var progress = _db.UserTaskProgresses.FirstOrDefault(p => p.UserId == userId && p.TaskId == id);
            if (progress == null)
            {
                progress = new UserTaskProgress { UserId = userId, TaskId = id, IsCompleted = true };
                _db.UserTaskProgresses.Add(progress);
            }
            else
            {
                progress.IsCompleted = true;
                _db.UserTaskProgresses.Update(progress);
            }
            _db.SaveChanges();
            return Ok();
        }
        [HttpPost]
        public IActionResult Create(wordwave.Models.Task task)
        {
            if (ModelState.IsValid)
            {
                _db.Tasks.Add(task);
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(task);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var task = _db.Tasks.Find(id);
            if (task == null) return NotFound();
            return View(task);
        }

        [HttpPost]
        public IActionResult Edit(int id, wordwave.Models.Task task)
        {
            if (id != task.Id) return NotFound();
            if (ModelState.IsValid)
            {
                _db.Update(task);
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(task);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var task = _db.Tasks.Find(id);
            if (task != null)
            {
                _db.Tasks.Remove(task);
                _db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            var task = _db.Tasks.FirstOrDefault(t => t.Id == id);
            if (task == null) return NotFound();

            var userId = User.FindFirst("UserId")?.Value;
            if (userId == null) return Unauthorized();

            var progress = _db.UserTaskProgresses.FirstOrDefault(p => p.UserId == userId && p.TaskId == id);
            if (progress == null)
            {
                progress = new UserTaskProgress { UserId = userId, TaskId = id };
                _db.UserTaskProgresses.Add(progress);
                _db.SaveChanges();
            }
            return View(task);
        }

        [HttpPost]
        public IActionResult CheckAnswer(int id, string answer)
        {
            var userId = User.FindFirst("UserId")?.Value;
            if (userId == null) return Unauthorized();

            var progress = _db.UserTaskProgresses.FirstOrDefault(p => p.UserId == userId && p.TaskId == id);
            if (progress == null)
            {
                progress = new UserTaskProgress { UserId = userId, TaskId = id };
                _db.UserTaskProgresses.Add(progress);
                _db.SaveChanges();
            }
            _db.SaveChanges();
            return Json(new { success = true });
        }
    }
}
