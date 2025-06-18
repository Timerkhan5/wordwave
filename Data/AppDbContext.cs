using Microsoft.EntityFrameworkCore;
using wordwave.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<wordwave.Models.Task> Tasks { get; set; }
    public DbSet<UserTaskProgress> UserTaskProgresses { get; set; }
    public DbSet<Material> Materials { get; set; }
    public DbSet<Review> Reviews { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<wordwave.Models.Task>().HasData(
            new wordwave.Models.Task
            {
                Id = 1,
                Question = "Столица Франции?",
                Option1 = "Берлин",
                Option2 = "Мадрид",
                Option3 = "Париж",
                Option4 = "Рим",
                CorrectOption = 3,
                Difficulty = 1
            },
            new wordwave.Models.Task
            {
                Id = 2,
                Question = "2 + 2 = ?",
                Option1 = "3",
                Option2 = "4",
                Option3 = "5",
                Option4 = "22",
                CorrectOption = 2,
                Difficulty = 1
            },
            new wordwave.Models.Task
            {
                Id = 3,
                Question = "Какой элемент обозначается символом 'O'?",
                Option1 = "Золото",
                Option2 = "Кислород",
                Option3 = "Олово",
                Option4 = "Серебро",
                CorrectOption = 2,
                Difficulty = 2
            }
        );
        modelBuilder.Entity<User>().HasData(
            new User { Id = 1, UserName = "admin", PasswordHash = "adminhash", RegisteredAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), Role = "admin" },
            new User { Id = 2, UserName = "student", PasswordHash = "studenthash", RegisteredAt = new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc), Role = "user" }
        );
        modelBuilder.Entity<Material>().HasData(
            new Material { Id = 1, Name = "Грамматика A1", Author = "Иван Иванов", Description = "Основы грамматики для начинающих", Link = "https://example.com/grammar-a1" },
            new Material { Id = 2, Name = "Лексика A2", Author = "Мария Петрова", Description = "Слова и выражения для уровня A2", Link = "https://example.com/vocab-a2" }
        );
        modelBuilder.Entity<UserTaskProgress>().HasData(
            new UserTaskProgress { Id = 1, UserId = "1", TaskId = 1, IsCompleted = true },
            new UserTaskProgress { Id = 2, UserId = "2", TaskId = 1, IsCompleted = false },
            new UserTaskProgress { Id = 3, UserId = "2", TaskId = 2, IsCompleted = true }
        );
    }
}
