using Microsoft.Extensions.Logging;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Pomodoro.Web.Services.Repositories;

namespace Pomodoro.Web.Tests.Services.TaskRepositoryTests;

/// <summary>
/// Base test class for TaskRepository.
/// Contains shared setup and helper methods.
/// </summary>
public partial class TaskRepositoryTests
{
    protected readonly Mock<IIndexedDbService> MockIndexedDb;
    protected readonly Mock<ILogger<TaskRepository>> MockLogger;

    public TaskRepositoryTests()
    {
        MockIndexedDb = new Mock<IIndexedDbService>();
        MockLogger = new Mock<ILogger<TaskRepository>>();
    }

    /// <summary>
    /// Creates a TaskRepository instance with mocked dependencies.
    /// </summary>
    protected TaskRepository CreateRepository()
    {
        return new TaskRepository(MockIndexedDb.Object, MockLogger.Object);
    }

    /// <summary>
    /// Creates a sample TaskItem for testing.
    /// </summary>
    protected static TaskItem CreateSampleTask(
        Guid? id = null,
        string name = "Test Task",
        bool isDeleted = false,
        DateTime? deletedAt = null,
        bool isCompleted = false)
    {
        return new TaskItem
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
            IsDeleted = isDeleted,
            DeletedAt = deletedAt,
            IsCompleted = isCompleted,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a list of sample tasks for testing.
    /// </summary>
    protected static List<TaskItem> CreateSampleTasks(int count = 3, bool includeDeleted = false)
    {
        var tasks = new List<TaskItem>();

        for (int i = 0; i < count; i++)
        {
            tasks.Add(new TaskItem
            {
                Id = Guid.NewGuid(),
                Name = $"Task {i + 1}",
                IsDeleted = includeDeleted && i == 0,
                DeletedAt = includeDeleted && i == 0 ? DateTime.UtcNow : null,
                IsCompleted = i % 2 == 0,
                CreatedAt = DateTime.UtcNow.AddDays(-i)
            });
        }

        return tasks;
    }
}
