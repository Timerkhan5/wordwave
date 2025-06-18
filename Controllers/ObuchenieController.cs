using Microsoft.AspNetCore.Mvc;

namespace wordwave.Controllers
{
    public class ObuchenieController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
