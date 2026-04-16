using Moq;
using Pomodoro.Web;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Pomodoro.Web.Services.Repositories;
using Xunit;

namespace Pomodoro.Web.Tests.Services;

/// <summary>
/// Base test class for TaskService.
/// Contains shared setup and helper methods.
/// </summary>
[Trait("Category", "Service")]
public partial class TaskServiceTests
{
    protected readonly Mock<ITaskRepository> MockTaskRepository;
    protected readonly Mock<IIndexedDbService> MockIndexedDb;
    protected readonly AppState AppState;

    public TaskServiceTests()
    {
        MockTaskRepository = new Mock<ITaskRepository>();
        MockIndexedDb = new Mock<IIndexedDbService>();
        AppState = new AppState();
    }

    /// <summary>
    /// Creates a TaskService instance with mocked dependencies.
    /// </summary>
    protected TaskService CreateService()
    {
        return new TaskService(
            MockTaskRepository.Object,
            MockIndexedDb.Object,
            AppState
        );
    }

    /// <summary>
    /// Creates a sample task for testing.
    /// </summary>
    protected static TaskItem CreateSampleTask(Guid? id = null, string name = "Sample Task", bool isCompleted = false)
    {
        return new TaskItem
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
            CreatedAt = DateTime.UtcNow,
            IsCompleted = isCompleted,
            TotalFocusMinutes = 0,
            PomodoroCount = 0
        };
    }

    /// <summary>
    /// Adds a task directly to AppState for testing.
    /// </summary>
    protected void AddTaskToState(TaskItem task)
    {
        ((List<TaskItem>)AppState.Tasks).Add(task);
    }
}

