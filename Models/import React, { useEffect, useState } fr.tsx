import React, { useEffect, useState } from 'react';
import axios from 'axios';

const ExamEditor = () => {
  const [tasks, setTasks] = useState([]);

  useEffect(() => {
    axios.get('/api/all-tasks').then(res => setTasks(res.data));
  }, []);

  const toggleExam = async (id: number, isExam: boolean) => {
    await axios.post('/api/update-task-exam', { id, isExam: !isExam });
    setTasks(tasks.map(t => t.id === id ? { ...t, isExam: !isExam } : t));
  };

  return (
    <div>
      <h1>Редактирование экзамена</h1>
      {tasks.map(task => (
        <div key={task.id}>
          <span>{task.title}</span>
          <input
            type="checkbox"
            checked={task.isExam}
            onChange={() => toggleExam(task.id, task.isExam)}
          />
        </div>
      ))}
    </div>
  );
};

export default ExamEditor;
