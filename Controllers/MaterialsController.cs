using Microsoft.AspNetCore.Mvc;
using wordwave.Models;
using Microsoft.AspNetCore.Authorization;

namespace wordwave.Controllers
{
    public class MaterialsController : Controller
    {
        private readonly AppDbContext _db;
        public MaterialsController(AppDbContext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            var materials = _db.Materials.ToList();
            return View(materials);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(Material material)
        {
            if (ModelState.IsValid)
            {
                _db.Materials.Add(material);
                _db.SaveChanges();
                return RedirectToAction("Index", "Admin");
            }
            return View(material);
        }
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var material = _db.Materials.Find(id);
            if (material == null) return NotFound();
            return View(material);
        }
        [HttpPost]
        public IActionResult Edit(int id, Material material)
        {
            if (id != material.Id) return NotFound();
            if (ModelState.IsValid)
            {
                _db.Update(material);
                _db.SaveChanges();
                return RedirectToAction("Index", "Admin");
            }
            return View(material);
        }
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var material = _db.Materials.Find(id);
            if (material != null)
            {
                _db.Materials.Remove(material);
                _db.SaveChanges();
            }
            return RedirectToAction("Index", "Admin");
        }
    }
}
