import React from "react";
import { getUserTaskProgresses } from "../api/userTaskProgress";
import ModuleProgress from "../components/ModuleProgress";

const ProgressPage = ({ userId, modules, tasks }) => {
  const [progress, setProgress] = React.useState<{ [taskId: string]: string }>({});

  React.useEffect(() => {
    getUserTaskProgresses(userId).then(res => {
      const data = res.data;
      const map = {};
      data.forEach((item: any) => {
        map[item.taskId] = item.status;
      });
      setProgress(map);
    });
  }, [userId]);

  const getModuleStats = (moduleId: string) => {
    const moduleTasks = tasks
      .filter(t => t.moduleId === moduleId)
      .slice()
      .sort((a, b) => a.id - b.id);
    const total = moduleTasks.length;
    const completed = moduleTasks.filter(t => progress[t.id] === "completed").length;
    return { completed, total };
  };

  return (
    <div>
      {modules.map(module => {
        const stats = getModuleStats(module.id);
        return (
          <ModuleProgress
            key={module.id}
            module={module}
            completed={stats.completed}
            total={stats.total}
          />
        );
      })}
    </div>
  );
};

export default ProgressPage;