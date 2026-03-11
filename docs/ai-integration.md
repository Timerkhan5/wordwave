AI integration — quick start

Overview
- This project includes an LLM-based task generator.
- Endpoint: POST /admin/api/ai/generate-and-save
- Authentication: endpoint is protected by [Authorize(Roles = "admin")] — you must be logged in as an admin user.
- Provider: Ollama only.

Environment (Ollama - required)
- To run generation through local Ollama set:
  - OLLAMA_BASEURL=http://127.0.0.1:11434
  - OLLAMA_MODEL=deepseek-v3.1:671b-cloud (or any model installed in your Ollama)
  - (Optional) OLLAMA_AUTO_PULL=true to auto-pull missing model (default: true)

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
- The generator asks the model to return a strict JSON array of multiple-choice tasks, each with 4 options.
- If the model returns invalid JSON, the request may fail — we attempt to extract the JSON array from the response.

Security
- Keep admin access restricted.

Local testing (Ollama)
- Start Ollama locally and ensure model `deepseek-v3.1:671b-cloud` is available.
- Set env vars:
  - `OLLAMA_BASEURL=http://127.0.0.1:11434`
  - `OLLAMA_MODEL=deepseek-v3.1:671b-cloud`
- Run the app: `dotnet run` from project folder.
- Open admin panel, login as admin, and use **Generate tasks**.

Troubleshooting: address already in use (5080/other port)
- If `dotnet run` fails with `address already in use`, another process is already listening on that port.
- This repo uses `http://localhost:5180` and `https://localhost:7186` in `launchSettings.json`.
- You can also override URL explicitly:
  - PowerShell: `$env:ASPNETCORE_URLS="http://localhost:5199"; dotnet run`
  - bash: `ASPNETCORE_URLS=http://localhost:5199 dotnet run`

Troubleshooting: 502 Bad Gateway from /admin/api/ai/*
- 502 means backend could not get a valid response from Ollama.
- Check Ollama is running and reachable at `OLLAMA_BASEURL`.
- Check the model exists locally (`ollama list`) and exactly matches `OLLAMA_MODEL`.
- Quick health check:
  - `curl http://127.0.0.1:11434/api/tags`


Troubleshooting: model not found
- If you get `Ollama model not found`, your `OLLAMA_MODEL` is not installed locally.
- Check installed models: `ollama list`
- Pull the required model: `ollama pull deepseek-v3.1:671b-cloud`
- Or set `OLLAMA_MODEL` to one of installed models.
- If your Ollama instance can pull models, keep `OLLAMA_AUTO_PULL=true` (default) and the app will try to pull missing model automatically.
