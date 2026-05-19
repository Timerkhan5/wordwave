# ML recommendations

WordWave includes a lightweight adaptive recommendation model for choosing the next task for a learner.

Endpoint:

```http
GET /api/ml/next-task
```

Authentication:

- The endpoint is protected by cookie authentication.
- It uses the current user's `UserId` claim.

Model:

- The model trains a small logistic regression classifier in C#.
- Training data comes from `UserTaskProgress`.
- A completed task is treated as a positive label.
- A started but unfinished task is treated as a negative label.

Features:

- Task difficulty.
- Whether the task is an exam task.
- User completion rate.
- Estimated learner level from completed task difficulties.
- Gap between task difficulty and estimated learner level.

Recommendation logic:

- The model estimates the probability that the current user will complete each unfinished task.
- It prefers tasks near the target success probability of about 68%.
- Exam tasks receive a small penalty so regular practice is recommended first.

Cold start:

- If there are fewer than four progress records, the service uses an adaptive fallback score.
- Once progress data exists, logistic regression is trained from the local database on each recommendation request.
