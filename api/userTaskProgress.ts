import axios from "axios";

export async function getUserTaskProgresses(userId: string) {
  return axios.get(`/api/user-task-progress?userId=${userId}`);
}

export async function setUserTaskProgress(userId: string, taskId: string, status: string) {
  return axios.post(`/api/user-task-progress`, { userId, taskId, status });
}
