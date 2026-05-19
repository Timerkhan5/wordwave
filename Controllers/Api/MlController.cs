using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using wordwave.Services;

namespace wordwave.Controllers.Api
{
    [ApiController]
    [Authorize]
    [Route("api/ml")]
    public class MlController : ControllerBase
    {
        private readonly AdaptiveTaskRecommendationService _recommendations;

        public MlController(AdaptiveTaskRecommendationService recommendations)
        {
            _recommendations = recommendations;
        }

        [HttpGet("next-task")]
        public IActionResult NextTask()
        {
            var userId = User.FindFirst("UserId")?.Value;
            if (userId == null) return Unauthorized();

            return Ok(_recommendations.RecommendNextTask(userId));
        }
    }
}
