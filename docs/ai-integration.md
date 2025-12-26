AI integration — quick start

Overview
- This project includes a simple LLM-based task generator.
- Endpoint: POST /admin/api/ai/generate-and-save
- Authentication: endpoint is protected by [Authorize(Roles = "admin")] — you must be logged in as an admin user.

Environment (OpenAI - local/testing)
- To use OpenAI from your local machine set:
  - OPENAI_API_KEY=your_openai_api_key
  - (Optional) OPENAI_BASEURL to point to a local OpenAI-compatible server or proxy (default: https://api.openai.com). You can pass either `https://host` or `https://host/v1`.
  - (Optional) OPENAI_MODEL to override model used for /v1/chat/completions (default in code: gpt-4o-mini)
  - (Optional) LLM_PROVIDER=openai to force OpenAI (default: auto)

Environment (Deepseek - self-hosted)
- To use Deepseek set:
  - DEEPSEEK_BASEURL=https://your.deepseek.instance
  - DEEPSEEK_APIKEY=your_key_if_required (optional if your instance is public)
  - (Optional) LLM_PROVIDER=deepseek to force Deepseek even if OPENAI_API_KEY is set (useful if OpenAI is blocked in your region)
  - (Optional) DEEPSEEK_MODEL to override model (default in code: deepseek-chat)
  - (Optional) DEEPSEEK_ENDPOINT_STYLE=openai (default) to use OpenAI-compatible endpoint `/v1/chat/completions`
    - If your server expects `/generate`, set DEEPSEEK_ENDPOINT_STYLE=generate


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
 - If you get 401/403 from OpenAI, verify your API key, billing, and model access (try setting OPENAI_MODEL).
 - If you get `unsupported_country_region_territory` from OpenAI, you must use Deepseek or an OpenAI-compatible proxy hosted in a supported region (set OPENAI_BASEURL and/or LLM_PROVIDER=deepseek).

Security
- Keep your OPENAI_API_KEY secret.
- Consider limiting access to this endpoint to admin roles only and adding rate-limiting.

Local testing (OpenAI)
- Set your key locally (PowerShell):
  - $env:OPENAI_API_KEY = "sk-..."
- Run the app: `dotnet run` from project folder.
- Open the admin panel in the browser, login as admin, and use the **Generate tasks** control to create and save tasks.

If you want, I can:
- Add server-side tests for the generator parsing logic.
- Add a safer preview-first flow (generate preview and then save).

