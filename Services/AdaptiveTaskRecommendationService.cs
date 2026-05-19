using Microsoft.EntityFrameworkCore;
using wordwave.Models;
using LearningTask = wordwave.Models.Task;

namespace wordwave.Services
{
    public class AdaptiveTaskRecommendationService
    {
        private const double TargetSuccessProbability = 0.68;
        private readonly AppDbContext _db;

        public AdaptiveTaskRecommendationService(AppDbContext db)
        {
            _db = db;
        }

        public TaskRecommendationDto RecommendNextTask(string userId)
        {
            var tasks = _db.Tasks.AsNoTracking().OrderBy(t => t.Id).ToList();
            var progress = _db.UserTaskProgresses.AsNoTracking().ToList();

            if (tasks.Count == 0)
            {
                return Empty("В базе пока нет заданий для рекомендации.");
            }

            var userProgress = progress.Where(p => p.UserId == userId).ToList();
            var completedTaskIds = userProgress.Where(p => p.IsCompleted).Select(p => p.TaskId).ToHashSet();
            var candidates = tasks.Where(t => !completedTaskIds.Contains(t.Id)).ToList();

            if (candidates.Count == 0)
            {
                return Empty("Все доступные задания уже выполнены.");
            }

            var userProfile = BuildUserProfile(userId, tasks, progress);
            var trainingSamples = BuildTrainingSamples(tasks, progress);
            var weights = trainingSamples.Count >= 4
                ? TrainLogisticRegression(trainingSamples)
                : null;

            var ranked = candidates
                .Select(task =>
                {
                    var features = BuildFeatures(task, userProfile);
                    var probability = weights == null
                        ? ColdStartProbability(task, userProfile)
                        : Sigmoid(Dot(weights, features));
                    var score = RecommendationScore(task, userProfile, probability);

                    return new
                    {
                        Task = task,
                        Probability = probability,
                        Score = score
                    };
                })
                .OrderBy(x => x.Score)
                .ThenBy(x => x.Task.IsExam)
                .ThenBy(x => x.Task.Difficulty)
                .ThenBy(x => x.Task.Id)
                .First();

            return new TaskRecommendationDto
            {
                TaskId = ranked.Task.Id,
                Question = ranked.Task.Question,
                Difficulty = ranked.Task.Difficulty,
                IsExam = ranked.Task.IsExam,
                SuccessProbability = Math.Round(ranked.Probability, 3),
                Score = Math.Round(ranked.Score, 3),
                TrainingSamples = trainingSamples.Count,
                ModelName = weights == null ? "Cold-start adaptive scoring" : "Logistic regression over progress history",
                Reason = BuildReason(ranked.Task, ranked.Probability, userProfile)
            };
        }

        private static TaskRecommendationDto Empty(string reason)
        {
            return new TaskRecommendationDto
            {
                ModelName = "Adaptive recommendation",
                Reason = reason
            };
        }

        private static UserLearningProfile BuildUserProfile(string userId, List<LearningTask> tasks, List<UserTaskProgress> progress)
        {
            var byTaskId = tasks.ToDictionary(t => t.Id);
            var userProgress = progress.Where(p => p.UserId == userId).ToList();
            var seen = Math.Max(1, userProgress.Count);
            var completed = userProgress.Count(p => p.IsCompleted);
            var completedDifficulties = userProgress
                .Where(p => p.IsCompleted && byTaskId.ContainsKey(p.TaskId))
                .Select(p => byTaskId[p.TaskId].Difficulty)
                .ToList();

            var completionRate = (double)completed / seen;
            var estimatedLevel = completedDifficulties.Count == 0
                ? 1.0
                : completedDifficulties.Average();

            return new UserLearningProfile(completionRate, estimatedLevel, completed, userProgress.Count);
        }

        private static List<TrainingSample> BuildTrainingSamples(List<LearningTask> tasks, List<UserTaskProgress> progress)
        {
            var taskById = tasks.ToDictionary(t => t.Id);
            var profiles = progress
                .Select(p => p.UserId)
                .Distinct()
                .ToDictionary(userId => userId, userId => BuildUserProfile(userId, tasks, progress));

            return progress
                .Where(p => taskById.ContainsKey(p.TaskId) && profiles.ContainsKey(p.UserId))
                .Select(p => new TrainingSample(
                    BuildFeatures(taskById[p.TaskId], profiles[p.UserId]),
                    p.IsCompleted ? 1.0 : 0.0))
                .ToList();
        }

        private static double[] BuildFeatures(LearningTask task, UserLearningProfile profile)
        {
            var difficulty = (task.Difficulty - 3.0) / 2.0;
            var levelGap = task.Difficulty - profile.EstimatedLevel;

            return new[]
            {
                1.0,
                difficulty,
                profile.CompletionRate,
                task.IsExam ? 1.0 : 0.0,
                levelGap / 4.0
            };
        }

        private static double[] TrainLogisticRegression(List<TrainingSample> samples)
        {
            var weights = new double[samples[0].Features.Length];
            const double learningRate = 0.14;
            const double regularization = 0.001;

            for (var epoch = 0; epoch < 100; epoch++)
            {
                foreach (var sample in samples)
                {
                    var prediction = Sigmoid(Dot(weights, sample.Features));
                    var error = prediction - sample.Label;

                    for (var i = 0; i < weights.Length; i++)
                    {
                        weights[i] -= learningRate * ((error * sample.Features[i]) + regularization * weights[i]);
                    }
                }
            }

            return weights;
        }

        private static double ColdStartProbability(LearningTask task, UserLearningProfile profile)
        {
            var levelGap = task.Difficulty - profile.EstimatedLevel;
            var raw = 1.15 + (1.4 * (profile.CompletionRate - 0.5)) - (0.75 * levelGap);
            if (task.IsExam) raw -= 0.35;
            return Sigmoid(raw);
        }

        private static double RecommendationScore(LearningTask task, UserLearningProfile profile, double probability)
        {
            var probabilityFit = Math.Abs(probability - TargetSuccessProbability);
            var levelFit = Math.Abs(task.Difficulty - profile.EstimatedLevel) * 0.04;
            var examPenalty = task.IsExam ? 0.12 : 0.0;

            return probabilityFit + levelFit + examPenalty;
        }

        private static string BuildReason(LearningTask task, double probability, UserLearningProfile profile)
        {
            var percent = Math.Round(probability * 100);
            if (profile.SeenCount == 0)
            {
                return $"Стартовая рекомендация уровня {task.Difficulty}: модель начнет уточнять прогноз после первых попыток.";
            }

            return $"Вероятность успеха около {percent}%, уровень задания близок к текущему профилю пользователя.";
        }

        private static double Dot(double[] weights, double[] features)
        {
            var result = 0.0;
            for (var i = 0; i < weights.Length; i++)
            {
                result += weights[i] * features[i];
            }

            return result;
        }

        private static double Sigmoid(double value)
        {
            if (value >= 35) return 1.0;
            if (value <= -35) return 0.0;
            return 1.0 / (1.0 + Math.Exp(-value));
        }

        private sealed record UserLearningProfile(double CompletionRate, double EstimatedLevel, int CompletedCount, int SeenCount);
        private sealed record TrainingSample(double[] Features, double Label);
    }
}
