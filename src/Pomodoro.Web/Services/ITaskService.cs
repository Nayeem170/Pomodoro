using Pomodoro.Web.Models;

namespace Pomodoro.Web.Services;

/// <summary>
/// Interface for task management operations
/// </summary>
public interface ITaskService
{
    event Action? OnChange;

    /// <summary>
    /// Active tasks (excludes soft-deleted)
    /// </summary>
    List<TaskItem> Tasks { get; }

    /// <summary>
    /// All tasks including soft-deleted (for history)
    /// </summary>
    IReadOnlyList<TaskItem> AllTasks { get; }

    Guid? CurrentTaskId { get; }
    TaskItem? CurrentTask { get; }

    Task InitializeAsync();
    Task AddTaskAsync(string name);
    Task UpdateTaskAsync(TaskItem task);
    Task DeleteTaskAsync(Guid taskId);
    Task CompleteTaskAsync(Guid taskId);
    Task UncompleteTaskAsync(Guid taskId);
    Task SelectTaskAsync(Guid taskId);
    Task AddTimeToTaskAsync(Guid taskId, int minutes);
    Task SaveAsync();

    /// <summary>
    /// Reloads all task data from storage, refreshing the in-memory cache.
    /// Call this after import operations to reflect changes.
    /// </summary>
    Task ReloadAsync();
}
