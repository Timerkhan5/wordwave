import React, { useEffect, useState } from "react";
import { getUserTaskProgresses, setUserTaskProgress } from "../api/userTaskProgress";

const TasksPage = ({ userId }) => {
  const [progress, setProgress] = useState<{ [taskId: string]: string }>({});

  useEffect(() => {
    getUserTaskProgresses(userId).then(res => {
      const data = res.data;
      const map = {};
      data.forEach((item: any) => {
        map[item.taskId] = item.status;
      });
      setProgress(map);
    });
  }, [userId]);

  const handleComplete = async (taskId: string) => {
    if (progress[taskId] === "completed") return;
    await setUserTaskProgress(userId, taskId, "completed");
    setProgress(prev => ({ ...prev, [taskId]: "completed" }));
  };

  return (
    <div>
      {tasks
        .slice()
        .sort((a, b) => a.id - b.id)
        .map(task => (
          <div key={task.id}>
            <span>{task.title}</span>
            <button
              onClick={() => handleComplete(task.id)}
              disabled={progress[task.id] === "completed"}
            >
              {progress[task.id] === "completed" ? "Выполнено" : "Выполнить"}
            </button>
          </div>
        ))}
    </div>
  );
};

export default TasksPage;