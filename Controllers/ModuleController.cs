using Microsoft.AspNetCore.Mvc;
using System.Linq;
using wordwave.Models;
using Microsoft.AspNetCore.Authorization;

namespace wordwave.Controllers
{
    public class ModuleController : Controller
    {
        private readonly AppDbContext _db;
        public ModuleController(AppDbContext db)
        {
            _db = db;
        }

        [Authorize]
        public IActionResult Index()
        {
            var userId = User.FindFirst("UserId")?.Value;
            var allTasks = _db.Tasks.Where(t => !t.IsExam && t.Difficulty == 1).OrderBy(t => t.Id).ToList();
            var progress = userId != null
                ? _db.UserTaskProgresses.Where(p => p.UserId == userId && p.IsCompleted).ToList()
                : new List<UserTaskProgress>();
            var examTasks = _db.Tasks.Where(t => t.IsExam).ToList();
            ViewBag.Progress = progress;
            ViewBag.ExamTasks = examTasks;
            return View(allTasks);
        }
        public IActionResult Exam()
        {
            var examTasks = _db.Tasks.Where(t => t.IsExam).OrderBy(t => t.Id).ToList();
            return View("Exam", examTasks);
        }

        [HttpPost]
        [Authorize]
        public IActionResult CompleteExam()
        {
            var userId = User.FindFirst("UserId")?.Value;
            if (userId == null) return Unauthorized();

            var moduleTasks = _db.Tasks.Where(t => !t.IsExam && t.Difficulty == 1).ToList();
            foreach (var task in moduleTasks)
            {
                var progress = _db.UserTaskProgresses.FirstOrDefault(p => p.UserId == userId && p.TaskId == task.Id);
                if (progress == null)
                {
                    progress = new UserTaskProgress { UserId = userId, TaskId = task.Id, IsCompleted = true };
                    _db.UserTaskProgresses.Add(progress);
                }
                else
                {
                    progress.IsCompleted = true;
                    _db.UserTaskProgresses.Update(progress);
                }
            }

            var examTasks = _db.Tasks.Where(t => t.IsExam && t.Difficulty == 1).ToList();
            foreach (var examTask in examTasks)
            {
                var progress = _db.UserTaskProgresses.FirstOrDefault(p => p.UserId == userId && p.TaskId == examTask.Id);
                if (progress == null)
                {
                    progress = new UserTaskProgress { UserId = userId, TaskId = examTask.Id, IsCompleted = true };
                    _db.UserTaskProgresses.Add(progress);
                }
                else
                {
                    progress.IsCompleted = true;
                    _db.UserTaskProgresses.Update(progress);
                }
            }
            _db.SaveChanges();
            return Ok();
        }
    }
}
