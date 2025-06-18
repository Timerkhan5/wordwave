import axios from 'axios';
import { useEffect, useState } from 'react';

function TaskComponent() {
  const [tasks, setTasks] = useState([]);

  useEffect(() => {
    axios.get('/api/all-tasks').then(res => {
      console.log(res.data.map(t => t.id)); // Посмотрите, есть ли 1
      setTasks(res.data);
    });
  }, []);

  return (
    <div>
      {tasks.map(task => (
        <div key={task.id}>{task.name}</div>
      ))}
    </div>
  );
}

export default TaskComponent;