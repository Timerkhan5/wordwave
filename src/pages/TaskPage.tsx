import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';

const TaskPage = () => {
    const [attempts, setAttempts] = useState(3);
    const [userAnswer, setUserAnswer] = useState('');
    const navigate = useNavigate();
    const correctAnswer = "42";

    const handleAnswer = () => {
        if (userAnswer === correctAnswer) {
            navigate('/module');
        } else {
            setAttempts(prev => prev - 1);
        }
    };

    return (
        <div>
            <div>Осталось попыток: {attempts}</div>
            <input
                value={userAnswer}
                onChange={e => setUserAnswer(e.target.value)}
            />
            <button onClick={handleAnswer} disabled={attempts === 0}>
                Ответить
            </button>
        </div>
    );
};

export default TaskPage;