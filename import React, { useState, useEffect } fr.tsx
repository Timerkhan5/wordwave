import React, { useState, useEffect } from 'react';
import { useRouter } from 'next/router';
import { getExamById, updateExam } from '../../lib/api';
import { Task } from '../../types';

const ExamEditor = () => {
  const router = useRouter();
  const { id } = router.query;
  const [exam, setExam] = useState<{ title: string; tasks: Task[] } | null>(null);

  useEffect(() => {
    if (id) {
      const fetchExam = async () => {
        const examData = await getExamById(id as string);
        setExam(examData);
      };

      fetchExam();
    }
  }, [id]);

  const toggleExam = async (taskId: string, currentValue: boolean) => {
    if (!exam) return;

    const updatedTasks = exam.tasks.map(task =>
      task.id === taskId ? { ...task, isExam: !currentValue } : task
    );

    setExam({ ...exam, tasks: updatedTasks });

    await updateExam(id as string, { tasks: updatedTasks });
  };

  if (!exam) return <div>Загрузка...</div>;

  return (
    <div>
      <h1>Редактирование экзамена</h1>
      <div style={{ maxHeight: '70vh', overflowY: 'auto', border: '1px solid #ccc', padding: 16, borderRadius: 8 }}>
        {exam.tasks.map(task => (
          <div key={task.id} style={{ marginBottom: 16, padding: 10, background: '#f9f9f9', borderRadius: 6 }}>
            <span>{task.title}</span>
            <input
              type="checkbox"
              checked={task.isExam}
              onChange={() => toggleExam(task.id, task.isExam)}
              style={{ marginLeft: 12 }}
            />
          </div>
        ))}
      </div>
    </div>
  );
};

export default ExamEditor;