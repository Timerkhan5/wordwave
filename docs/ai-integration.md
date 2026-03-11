AI integration — quick start

Overview
- This project includes an LLM-based task generator.
- Endpoint: POST /admin/api/ai/generate-and-save
- Authentication: endpoint is protected by [Authorize(Roles = "admin")] — you must be logged in as an admin user.

Environment (Ollama - local, recommended and default)
- To run generation through local Ollama set:
  - LLM_PROVIDER=ollama
  - OLLAMA_BASEURL=http://127.0.0.1:11434
  - OLLAMA_MODEL=deepseek-v3.1:671b-cloud
- If `LLM_PROVIDER` is not set, provider defaults to `ollama`.
- If `LLM_PROVIDER=auto`, app tries Ollama first, then OpenAI (if key exists), then Deepseek (if configured).

Environment (OpenAI - optional)
- OPENAI_API_KEY=your_openai_api_key
- (Optional) OPENAI_BASEURL to point to OpenAI-compatible server/proxy.
- (Optional) OPENAI_MODEL (default: gpt-4o-mini)
- (Optional) LLM_PROVIDER=openai

Environment (Deepseek API - optional fallback)
- DEEPSEEK_BASEURL=https://your.deepseek.instance
- DEEPSEEK_APIKEY=your_key_if_required
- (Optional) DEEPSEEK_MODEL (default: deepseek-chat)
- (Optional) DEEPSEEK_ENDPOINT_STYLE=openai|generate
- (Optional) LLM_PROVIDER=deepseek

Request shape
POST /admin/api/ai/generate-and-save
Content-Type: application/json
Body:
{
  "topic": "arrays and loops",
  "count": 5,
  "difficulty": 2
}

Response (success):
{
  "created": 5,
  "ids": [101,102,103,104,105]
}

Notes
- Count is clamped to 1..20 to avoid excessive generation cost.
- The generator asks the LLM to return a strict JSON array of multiple-choice tasks, each with 4 options.
- If the LLM returns invalid JSON, the request may fail — we attempt to extract the JSON array from the response.

Security
- Keep API keys secret (if used).
- Keep admin access restricted.

Local testing (Ollama)
- Start Ollama locally and ensure model `deepseek-v3.1:671b-cloud` is available.
- Set env vars:
  - `LLM_PROVIDER=ollama`
  - `OLLAMA_BASEURL=http://127.0.0.1:11434`
  - `OLLAMA_MODEL=deepseek-v3.1:671b-cloud`
- Run the app: `dotnet run` from project folder.
- Open admin panel, login as admin, and use **Generate tasks**.


Troubleshooting: address already in use (5080/other port)
- If `dotnet run` fails with `address already in use`, another process is already listening on that port.
- This repo now uses `http://localhost:5180` and `https://localhost:7186` in `launchSettings.json`.
- You can also override URL explicitly:
  - PowerShell: `$env:ASPNETCORE_URLS="http://localhost:5199"; dotnet run`
  - bash: `ASPNETCORE_URLS=http://localhost:5199 dotnet run`
