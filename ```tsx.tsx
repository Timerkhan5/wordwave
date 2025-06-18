```tsx
import React from 'react';

const ExamPage = ({ tasks }) => {
  return (
    <div>
      <h1>Экзамен</h1>
      <div style={{ maxHeight: '70vh', overflowY: 'auto', border: '1px solid #ccc', padding: 16, borderRadius: 8 }}>
        {tasks.map(task => (
          <div key={task.id} style={{ marginBottom: 24, padding: 12, background: '#f9f9f9', borderRadius: 6 }}>
            <h2>{task.title}</h2>
            {/* ...existing code... */}
          </div>
        ))}
      </div>
    </div>
  );
};

export default ExamPage;
```