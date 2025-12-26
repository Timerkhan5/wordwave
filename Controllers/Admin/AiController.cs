using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using wordwave.Models;
using wordwave.Services;

namespace wordwave.Controllers.Admin
{
    [Authorize(Roles = "admin")]
    [Route("admin/api/[controller]")]
    public class AiController : Controller
    {
        private readonly LlmTaskGenerator _gen;
        private readonly AppDbContext _db;
        private const string GeneratorVersion = "2025-12-23.1";

        public AiController(LlmTaskGenerator gen, AppDbContext db)
        {
            _gen = gen;
            _db = db;
        }

        [HttpPost("generate-and-save")]
        public async Task<IActionResult> GenerateAndSave([FromBody] GenerateTasksRequest req)
        {
            try
            {
                if (req == null || string.IsNullOrWhiteSpace(req.Topic)) return BadRequest(new { error = "Topic is required" });
                req.Count = Math.Clamp(req.Count, 1, 20);
                req.Difficulty = Math.Clamp(req.Difficulty, 1, 5);

                var generated = await _gen.GenerateTasksAsync(req.Topic, req.Count, req.Difficulty);
                if (generated == null || generated.Count == 0) return BadRequest(new { error = "No tasks were generated" });

                var created = new List<Models.Task>();

                foreach (var g in generated)
                {
                    if (g == null || g.Options == null || g.Options.Length != 4) continue;

                    var dbTask = new Models.Task
                    {
                        Question = g.Question ?? string.Empty,
                        Option1 = g.Options.Length > 0 ? g.Options[0] : string.Empty,
                        Option2 = g.Options.Length > 1 ? g.Options[1] : string.Empty,
                        Option3 = g.Options.Length > 2 ? g.Options[2] : string.Empty,
                        Option4 = g.Options.Length > 3 ? g.Options[3] : string.Empty,
                        CorrectOption = Math.Clamp(g.CorrectOption, 1, 4),
                        Difficulty = Math.Clamp(g.Difficulty, 1, 5),
                        IsExam = g.IsExam
                    };

                    _db.Tasks.Add(dbTask);
                    created.Add(dbTask);
                }

                await _db.SaveChangesAsync();

                return Ok(new { created = created.Count, ids = created.Select(t => t.Id).ToArray() });
            }
            catch (Exception ex)
            {
                return StatusCode(502, new { error = "LLM request failed", details = ex.Message });
            }
        }


        [HttpPost("test-generate")]
        public async Task<IActionResult> TestGenerate([FromBody] GenerateTasksRequest req)
        {
            try
            {
                if (req == null || string.IsNullOrWhiteSpace(req.Topic)) return BadRequest(new { error = "Topic is required" });
                req.Count = Math.Clamp(req.Count, 1, 20);
                req.Difficulty = Math.Clamp(req.Difficulty, 1, 5);

                var generated = await _gen.GenerateTasksAsync(req.Topic, req.Count, req.Difficulty);
                if (generated == null || generated.Count == 0) return BadRequest(new { error = "No tasks were generated" });

                return Ok(new { generatorVersion = GeneratorVersion, tasks = generated });
            }
            catch (Exception ex)
            {
                return StatusCode(502, new { error = "LLM request failed", details = ex.Message });
            }
        }
    }
}
