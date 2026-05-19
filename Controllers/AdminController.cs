using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using wordwave.Models;
using wordwave.Services;

namespace wordwave.Controllers
{
    [Authorize(Roles = "admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _db;
        private readonly AdaptiveTaskRecommendationService _recommendations;

        public AdminController(AppDbContext db, AdaptiveTaskRecommendationService recommendations)
        {
            _db = db;
            _recommendations = recommendations;
        }
        public IActionResult Index()
        {
            var tasks = _db.Tasks.ToList();
            var materials = _db.Materials.ToList();
            var mlRecommendations = _db.Users
                .OrderBy(u => u.Id)
                .ToList()
                .Select(user =>
                {
                    var recommendation = _recommendations.RecommendNextTask(user.Id.ToString());

                    return new AdminMlRecommendationDto
                    {
                        UserId = user.Id,
                        UserName = user.UserName,
                        TaskId = recommendation.TaskId,
                        Question = recommendation.Question,
                        Difficulty = recommendation.Difficulty,
                        SuccessProbability = recommendation.SuccessProbability,
                        TrainingSamples = recommendation.TrainingSamples,
                        ModelName = recommendation.ModelName,
                        Reason = recommendation.Reason
                    };
                })
                .ToList();

            ViewBag.Tasks = tasks;
            ViewBag.Materials = materials;
            ViewBag.MlRecommendations = mlRecommendations;
            return View();
        }
    }
}
