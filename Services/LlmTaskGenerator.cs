using System.Net.Http.Headers;
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

        private string? GetApiKey()
        {
            var key = _config["OpenAI:ApiKey"];
            if (!string.IsNullOrWhiteSpace(key)) return key;
            return Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        }

        private string GetOpenAiModel()
        {
            // Allow config override; default to a modern lightweight model.
            // (If your account doesn't have access to the default, set OPENAI_MODEL or OpenAI:Model.)
            return _config["OPENAI_MODEL"]
                   ?? _config["OpenAI:Model"]
                   ?? "gpt-4o-mini";
        }

        private string GetProvider()
        {
            // auto (default): OpenAI if key is set, otherwise Deepseek
            // openai: force OpenAI even if Deepseek is configured
            // deepseek: force Deepseek even if OPENAI_API_KEY is set (useful for region blocks)
            return (_config["LLM_PROVIDER"] ?? _config["Llm:Provider"] ?? "auto").Trim().ToLowerInvariant();
        }

        private string? GetDeepseekBaseUrl()
        {
            return _config["DEEPSEEK_BASEURL"] ?? _config["Deepseek:BaseUrl"];
        }

        private string GetDeepseekModel()
        {
            // Deepseek OpenAI-compatible APIs commonly use these model ids; allow override.
            return _config["DEEPSEEK_MODEL"]
                   ?? _config["Deepseek:Model"]
                   ?? "deepseek-chat";
        }

        private string GetDeepseekEndpointStyle()
        {
            // openai (default): POST {base}/v1/chat/completions or {base}/chat/completions if base already ends with /v1
            // generate: POST {base}/generate (legacy/custom)
            return (_config["DEEPSEEK_ENDPOINT_STYLE"] ?? _config["Deepseek:EndpointStyle"] ?? "openai")
                .Trim()
                .ToLowerInvariant();
        }

        private static string? TryExtractErrorCode(string raw)
        {
            try
            {
                using var doc = JsonDocument.Parse(raw);
                var root = doc.RootElement;
                if (root.TryGetProperty("error", out var err) && err.ValueKind == JsonValueKind.Object)
                {
                    if (err.TryGetProperty("code", out var code) && code.ValueKind == JsonValueKind.String) return code.GetString();
                }
            }
            catch { /* ignore */ }
            return null;
        }

        private static async Task<string> ReadBodySafe(HttpResponseMessage res)
        {
            try { return await res.Content.ReadAsStringAsync(); }
            catch { return string.Empty; }
        }

        private async Task<string> GenerateViaOpenAiAsync(string openAiKey, string systemPrompt, string userPrompt)
        {
            var openAiBase = _config["OPENAI_BASEURL"] ?? _config["OpenAI:BaseUrl"] ?? "https://api.openai.com";
            // Support both base formats:
            // - https://host
            // - https://host/v1
            var trimmed = openAiBase.TrimEnd('/');
            var url = trimmed.EndsWith("/v1", StringComparison.OrdinalIgnoreCase)
                ? trimmed + "/chat/completions"
                : trimmed + "/v1/chat/completions";

            var body = new
            {
                model = GetOpenAiModel(),
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userPrompt }
                },
                temperature = 0.0,
                max_tokens = 800
            };

            var req = new HttpRequestMessage(HttpMethod.Post, url);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", openAiKey);
            req.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            var res = await _http.SendAsync(req);
            var raw = await ReadBodySafe(res);
            if (!res.IsSuccessStatusCode)
            {
                var clipped = raw.Length > 2000 ? raw.Substring(0, 2000) + "..." : raw;
                throw new InvalidOperationException($"OpenAI request failed: {(int)res.StatusCode} {res.ReasonPhrase}. Body: {clipped}");
            }

            // Extract assistant message from common OpenAI response shapes
            string? extracted = null;
            try
            {
                using var doc = JsonDocument.Parse(raw);
                var root = doc.RootElement;
                if (root.TryGetProperty("choices", out var choices) && choices.ValueKind == JsonValueKind.Array && choices.GetArrayLength() > 0)
                {
                    var first = choices[0];
                    if (first.TryGetProperty("message", out var message) && message.TryGetProperty("content", out var content)) extracted = content.GetString();
                    else if (first.TryGetProperty("text", out var text)) extracted = text.GetString();
                }
            }
            catch { /* fallthrough to raw */ }

            return extracted ?? raw;
        }

        private async Task<string> GenerateViaDeepseekAsync(string systemPrompt, string userPrompt)
        {
            var baseUrl = GetDeepseekBaseUrl();
            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new InvalidOperationException("Deepseek is not configured: set DEEPSEEK_BASEURL (or Deepseek:BaseUrl).");

            var dsApiKey = _config["DEEPSEEK_APIKEY"] ?? _config["Deepseek:ApiKey"] ?? null;
            var style = GetDeepseekEndpointStyle();

            string url;
            object dsBody;

            if (style == "generate")
            {
                url = baseUrl.TrimEnd('/') + "/generate";
                dsBody = new
                {
                    input = userPrompt,
                    parameters = new { max_output_tokens = 800 }
                };
            }
            else if (style == "openai")
            {
                var trimmed = baseUrl.TrimEnd('/');
                url = trimmed.EndsWith("/v1", StringComparison.OrdinalIgnoreCase)
                    ? trimmed + "/chat/completions"
                    : trimmed + "/v1/chat/completions";

                dsBody = new
                {
                    model = GetDeepseekModel(),
                    messages = new[]
                    {
                        new { role = "system", content = systemPrompt },
                        new { role = "user", content = userPrompt }
                    },
                    temperature = 0.0,
                    max_tokens = 800
                };
            }
            else
            {
                throw new InvalidOperationException($"Unknown DEEPSEEK_ENDPOINT_STYLE value '{style}'. Use openai/generate.");
            }

            var req = new HttpRequestMessage(HttpMethod.Post, url);
            if (!string.IsNullOrWhiteSpace(dsApiKey)) req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", dsApiKey);
            req.Content = new StringContent(JsonSerializer.Serialize(dsBody), Encoding.UTF8, "application/json");

            var res = await _http.SendAsync(req);
            var raw = await ReadBodySafe(res);
            if (!res.IsSuccessStatusCode)
            {
                var clipped = raw.Length > 2000 ? raw.Substring(0, 2000) + "..." : raw;
                throw new InvalidOperationException($"Deepseek request failed: {(int)res.StatusCode} {res.ReasonPhrase}. Body: {clipped}");
            }

            // Try to extract text from common Deepseek-like responses
            string? content = null;
            try
            {
                using var doc = JsonDocument.Parse(raw);
                var root = doc.RootElement;

                // OpenAI-compatible: choices[0].message.content
                if (root.TryGetProperty("choices", out var choices) && choices.ValueKind == JsonValueKind.Array && choices.GetArrayLength() > 0)
                {
                    var first = choices[0];
                    if (first.TryGetProperty("message", out var message) && message.TryGetProperty("content", out var c))
                        content = c.GetString();
                    else if (first.TryGetProperty("text", out var t))
                        content = t.GetString();
                }

                // Deepseek-style: outputs[0].content or outputs[0].text
                if (content == null && root.TryGetProperty("outputs", out var outputs) && outputs.ValueKind == JsonValueKind.Array && outputs.GetArrayLength() > 0)
                {
                    var firstOut = outputs[0];
                    if (firstOut.TryGetProperty("content", out var cont)) content = cont.GetString();
                    else if (firstOut.TryGetProperty("text", out var text)) content = text.GetString();
                }

                if (content == null && root.TryGetProperty("result", out var result)) content = result.GetString();
                if (content == null && root.TryGetProperty("generated_text", out var gtext)) content = gtext.GetString();
                if (content == null && root.TryGetProperty("text", out var t2)) content = t2.GetString();
            }
            catch { /* fallthrough - use raw responseText */ }

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

                // RU instruction verbs (common for trainer UI)
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

                // EN instruction verbs (in case model outputs English instructions)
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

                // Must contain at least one latin token either in the question or in any option.
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

            var provider = GetProvider();

            // We'll retry with stricter wording if we detect non-English-training / non-instructional tasks.
            for (var attempt = 0; attempt < 3; attempt++)
            {
                var userPrompt = BuildPrompt(attempt);

            // Provider selection:
            // - auto: Prefer OpenAI if an API key is configured. Otherwise fallback to Deepseek.
            // - openai: Force OpenAI (requires OPENAI_API_KEY / OpenAI:ApiKey).
            // - deepseek: Force Deepseek (requires DEEPSEEK_BASEURL / Deepseek:BaseUrl).
            var openAiKey = GetApiKey();
            string responseText;

            if (provider == "openai" || (provider == "auto" && !string.IsNullOrWhiteSpace(openAiKey)))
            {
                if (string.IsNullOrWhiteSpace(openAiKey))
                    throw new InvalidOperationException("LLM provider is 'openai' but no API key configured. Set OPENAI_API_KEY or OpenAI:ApiKey.");

                try
                {
                    responseText = await GenerateViaOpenAiAsync(openAiKey, systemPrompt, userPrompt);
                }
                catch (Exception ex) when (provider == "auto" && !string.IsNullOrWhiteSpace(GetDeepseekBaseUrl()))
                {
                    // In auto mode, if OpenAI is blocked by region/policy or key issues, fallback to Deepseek when configured.
                    // This is especially useful in regions where OpenAI returns unsupported_country_region_territory.
                    var msg = ex.Message ?? string.Empty;
                    var shouldFallback =
                        msg.Contains("unsupported_country_region_territory", StringComparison.OrdinalIgnoreCase) ||
                        msg.Contains("invalid_api_key", StringComparison.OrdinalIgnoreCase) ||
                        msg.Contains("401", StringComparison.OrdinalIgnoreCase) ||
                        msg.Contains("403", StringComparison.OrdinalIgnoreCase);

                    if (!shouldFallback) throw;
                    responseText = await GenerateViaDeepseekAsync(systemPrompt, userPrompt);
                }
            }
            else
            {
                if (provider == "deepseek" || provider == "auto")
                {
                    responseText = await GenerateViaDeepseekAsync(systemPrompt, userPrompt);
                }
                else
                {
                    throw new InvalidOperationException($"Unknown LLM_PROVIDER value '{provider}'. Use auto/openai/deepseek.");
                }
            }

            // Try to parse responseText as JSON array of GeneratedTaskDto, otherwise try to isolate JSON array
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

                // Filter out "general knowledge" tasks: require (a) trainer shape + (b) latin content + (c) instructional question.
                var valid = result
                    .Where(t => t != null && LooksLikeEnglishTrainerTask(t) && LooksLikeInstructionalTask(t))
                    .ToList();

                if (valid.Count == count)
                    return valid;

                // If last attempt, return only valid tasks (never return "general" questions).
                if (attempt == 2)
                    return valid;
            }

            return new List<GeneratedTaskDto>();
        }
    }
}
