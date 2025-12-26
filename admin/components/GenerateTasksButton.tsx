import React, { useState } from "react";

export default function GenerateTasksButton() {
  const [topic, setTopic] = useState("");
  const [count, setCount] = useState(5);
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState<string | null>(null);

  async function generateAndSave() {
    setLoading(true);
    setMessage(null);
    try {
      const res = await fetch("/admin/api/ai/generate-and-save", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ topic, count }),
      });
      const data = await res.json();
      if (res.ok) setMessage(`Saved ${data.created} tasks (ids: ${data.ids.join(",")})`);
      else setMessage(`Error: ${data.error || JSON.stringify(data)}`);
    } catch (e: any) {
      setMessage(`Network error: ${e?.message ?? e}`);
    } finally {
      setLoading(false);
    }
  }

  return (
    <div style={{ marginTop: 12 }}>
      <h3>AI Task Generator</h3>
      <div style={{ display: "flex", gap: 8, alignItems: "center" }}>
        <input placeholder="Topic" value={topic} onChange={(e) => setTopic(e.target.value)} />
        <input type="number" min={1} max={50} value={count} onChange={(e) => setCount(Number(e.target.value))} style={{ width: 80 }} />
        <button onClick={generateAndSave} disabled={loading || !topic}>
          {loading ? "Generating..." : "Generate & Save"}
        </button>
      </div>
      {message && <div style={{ marginTop: 8 }}>{message}</div>}
    </div>
  );
}
