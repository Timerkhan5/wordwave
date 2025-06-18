using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using wordwave.Models;

namespace wordwave.Controllers
{
    [Authorize(Roles = "admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _db;
        public AdminController(AppDbContext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            var tasks = _db.Tasks.ToList();
            var materials = _db.Materials.ToList();
            ViewBag.Tasks = tasks;
            ViewBag.Materials = materials;
            return View();
        }
    }
}
