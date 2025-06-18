import React, { useEffect, useState } from 'react';
import axios from 'axios';

const ExamPage = () => {
  const [tasks, setTasks] = useState([]);

  useEffect(() => {
    axios.get('/api/exam-tasks').then(res => {
      console.log(res.data); // Проверьте, есть ли здесь задание с id 1
      setTasks(res.data);
    });
  }, []);

  return (
    <div>
      <h1>Экзаменационные задания</h1>
      <ul>
        {tasks.map(task => (
          <li key={task.id}>{task.title}</li>
        ))}
      </ul>
    </div>
  );
};

export default ExamPage;