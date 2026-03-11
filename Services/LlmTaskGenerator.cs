using System.Text;
using System.Text.Json;
using wordwave.Models;

namespace wordwave.Services
{
    public class LlmTaskGenerator
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;

        public LlmTaskGenerator(HttpClient http, IConfiguration config)
        {
            _http = http;
            _config = config;
        }

        private string GetOllamaBaseUrl()
        {
            return (_config["OLLAMA_BASEURL"] ?? _config["Ollama:BaseUrl"] ?? "http://127.0.0.1:11434").TrimEnd('/');
        }

        private string GetOllamaModel()
        {
            return _config["OLLAMA_MODEL"]
                   ?? _config["Ollama:Model"]
                   ?? "deepseek-v3.1:671b-cloud";
        }

        private async Task<List<string>> GetInstalledOllamaModelsAsync()
        {
            try
            {
                var tagsUrl = GetOllamaBaseUrl() + "/api/tags";
                var res = await _http.GetAsync(tagsUrl);
                if (!res.IsSuccessStatusCode) return new List<string>();

                var raw = await ReadBodySafe(res);
                using var doc = JsonDocument.Parse(raw);
                if (!doc.RootElement.TryGetProperty("models", out var models) || models.ValueKind != JsonValueKind.Array)
                    return new List<string>();

                var list = new List<string>();
                foreach (var m in models.EnumerateArray())
                {
                    if (m.TryGetProperty("name", out var name) && name.ValueKind == JsonValueKind.String)
                    {
                        var value = name.GetString();
                        if (!string.IsNullOrWhiteSpace(value)) list.Add(value);
                    }
                }

                return list;
            }
            catch
            {
                return new List<string>();
            }
        }

        private string ResolvePreferredModel(List<string> installed)
        {
            var requested = GetOllamaModel();
            if (installed.Count == 0) return requested;

            if (installed.Any(x => string.Equals(x, requested, StringComparison.OrdinalIgnoreCase)))
                return requested;

            var deepseekPreferred = installed.FirstOrDefault(x => x.StartsWith("deepseek", StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(deepseekPreferred))
                return deepseekPreferred;

            return requested;
        }

        private static async Task<string> ReadBodySafe(HttpResponseMessage res)
        {
            try { return await res.Content.ReadAsStringAsync(); }
            catch { return string.Empty; }
        }

        private async Task<string> GenerateViaOllamaAsync(string systemPrompt, string userPrompt)
        {
            var url = GetOllamaBaseUrl() + "/api/chat";
            var body = new
            {
                model = ResolvePreferredModel(await GetInstalledOllamaModelsAsync()),
                stream = false,
                options = new
                {
                    temperature = 0
                },
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userPrompt }
                }
            };

            var req = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
            };

            var res = await _http.SendAsync(req);
            var raw = await ReadBodySafe(res);
            if (!res.IsSuccessStatusCode)
            {
                var clipped = raw.Length > 2000 ? raw[..2000] + "..." : raw;

                if ((int)res.StatusCode == 404 && raw.Contains("model", StringComparison.OrdinalIgnoreCase) && raw.Contains("not found", StringComparison.OrdinalIgnoreCase))
                {
                    var requested = GetOllamaModel();
                    var installed = await GetInstalledOllamaModelsAsync();
                    var installedText = installed.Count == 0 ? "(none found via /api/tags)" : string.Join(", ", installed);
                    throw new InvalidOperationException(
                        $"Ollama model not found: '{requested}'. Installed models: {installedText}. " +
                        $"Set OLLAMA_MODEL to an installed model or run: ollama pull {requested}.");
                }

                throw new InvalidOperationException($"Ollama request failed: {(int)res.StatusCode} {res.ReasonPhrase}. Body: {clipped}");
            }

            return TryExtractContentFromKnownShapes(raw);
        }

        private static string TryExtractContentFromKnownShapes(string raw)
        {
            string? content = null;
            try
            {
                using var doc = JsonDocument.Parse(raw);
                var root = doc.RootElement;

                if (root.TryGetProperty("message", out var message) && message.ValueKind == JsonValueKind.Object)
                {
                    if (message.TryGetProperty("content", out var msgContent))
                    {
                        content = msgContent.GetString();
                    }
                }

                if (content == null && root.TryGetProperty("response", out var response)) content = response.GetString();
                if (content == null && root.TryGetProperty("text", out var t2)) content = t2.GetString();
            }
            catch { }

            return content ?? raw;
        }

        public async Task<List<GeneratedTaskDto>> GenerateTasksAsync(string topic, int count = 5, int difficulty = 1)
        {
            var systemPrompt = @"Ты — генератор заданий для тренажёра АНГЛИЙСКОГО ЯЗЫКА.
Выводи строго JSON и ничего кроме JSON. Никаких пояснений, никакого текста вне JSON.";

            static bool HasLatin(string? s)
            {
                if (string.IsNullOrWhiteSpace(s)) return false;
                foreach (var ch in s)
                {
                    if ((ch >= 'A' && ch <= 'Z') || (ch >= 'a' && ch <= 'z')) return true;
                }
                return false;
            }

            static bool LooksLikeInstructionalTask(GeneratedTaskDto t)
            {
                if (t == null) return false;
                if (string.IsNullOrWhiteSpace(t.Question)) return false;

                var q = t.Question.Trim();

                string[] ruMarkers =
                {
                    "выберите", "выбери", "встав", "перевед", "перевод", "как перевод", "какой перевод",
                    "подберите", "заполн", "исправ", "найдите ошиб", "определите", "укаж", "соотнес",
                    "постав", "продолж", "выберите форму", "выберите вариант"
                };

                foreach (var m in ruMarkers)
                {
                    if (q.Contains(m, StringComparison.OrdinalIgnoreCase)) return true;
                }

                string[] enMarkers = { "choose", "select", "fill", "insert", "translate", "pick", "complete", "correct" };
                foreach (var m in enMarkers)
                {
                    if (q.Contains(m, StringComparison.OrdinalIgnoreCase)) return true;
                }

                return false;
            }

            static bool LooksLikeEnglishTrainerTask(GeneratedTaskDto t)
            {
                if (t == null) return false;
                if (string.IsNullOrWhiteSpace(t.Question)) return false;
                if (t.Options == null || t.Options.Length != 4) return false;
                if (t.CorrectOption < 1 || t.CorrectOption > 4) return false;
                if (t.Difficulty < 1 || t.Difficulty > 5) return false;

                if (HasLatin(t.Question)) return true;
                foreach (var o in t.Options)
                {
                    if (HasLatin(o)) return true;
                }
                return false;
            }

            string BuildPrompt(int attempt)
            {
                var strict = attempt > 0
                    ? "КРИТИЧНО: в каждом задании обязательно должен присутствовать АНГЛИЙСКИЙ текст (латиница) — либо в question, либо в вариантах options. Запрещены вопросы на факты/общие знания по теме. Если задание можно решить без знания английского — оно запрещено."
                    : "В каждом задании должен быть английский материал (слово/фраза/предложение на латинице), который проверяется.";

                return $@"Сгенерируй РОВНО {count} заданий с выбором ответа для ТРЕНАЖЁРА АНГЛИЙСКОГО ЯЗЫКА по теме: ""{topic}"".
Не создавай вопросы на общие знания (например про семью/историю/биологию) — тема используется как ЛЕКСИЧЕСКАЯ/ГРАММАТИЧЕСКАЯ область английского.
{strict}

Требования:
- question: на русском (кириллица) и ОБЯЗАТЕЛЬНО формулировка-инструкция (начинается с ""Выберите...""/""Вставьте...""/""Переведите...""/""Исправьте...""/""Заполните..."").
- options: 4 варианта. Для грамматики/вставки слова варианты обычно на английском. Для перевода: варианты могут быть RU/EN, но всё равно должны проверять английский.
- difficulty={difficulty}: 1=A1/A2, 2=A2/B1, 3=B1/B2, 4=B2/C1, 5=C1/C2.

Примеры типов заданий (НЕ копируй дословно):
- ""Выберите правильный перевод слова 'family'"" (options: семья / ...)
- ""Вставьте правильную форму: She ___ to school every day."" (options: goes/go/going/went)
- ""Выберите правильный артикль: I saw ___ cat."" (options: a/an/the/—)

Формат каждого задания (JSON объект):
 - question (string)
 - options (array из 4 string)
 - correctOption (integer 1..4)
 - difficulty (integer 1..5)
 - isExam (boolean)

Верни ТОЛЬКО валидный JSON-массив. Никакого лишнего текста, никаких пояснений, никаких backticks.";
            }

            for (var attempt = 0; attempt < 3; attempt++)
            {
                var userPrompt = BuildPrompt(attempt);
                var responseText = await GenerateViaOllamaAsync(systemPrompt, userPrompt);

                List<GeneratedTaskDto>? parsed = null;
                try
                {
                    parsed = JsonSerializer.Deserialize<List<GeneratedTaskDto>>(responseText, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                catch
                {
                    var firstIdx = responseText.IndexOf('[');
                    var lastIdx = responseText.LastIndexOf(']');
                    if (firstIdx >= 0 && lastIdx > firstIdx)
                    {
                        var jsonPart = responseText.Substring(firstIdx, lastIdx - firstIdx + 1);
                        try
                        {
                            parsed = JsonSerializer.Deserialize<List<GeneratedTaskDto>>(jsonPart, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("Failed to parse JSON from LLM response", ex);
                        }
                    }
                }

                var result = parsed ?? new List<GeneratedTaskDto>();

                var valid = result
                    .Where(t => t != null && LooksLikeEnglishTrainerTask(t) && LooksLikeInstructionalTask(t))
                    .ToList();

                if (valid.Count == count)
                    return valid;

                if (attempt == 2)
                    return valid;
            }

            return new List<GeneratedTaskDto>();
        }
    }
}
