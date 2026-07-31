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

    IReadOnlyList<TaskListRef> TaskLists { get; }
    TaskListRef? CurrentList { get; }
    string? CurrentListId { get; }

    Task InitializeAsync();
    Task AddTaskAsync(string name);
    Task UpdateTaskAsync(TaskItem task);
    Task DeleteTaskAsync(Guid taskId);
    Task CompleteTaskAsync(Guid taskId);
    Task UncompleteTaskAsync(Guid taskId);
    Task SelectTaskAsync(Guid taskId);
    Task AddTimeToTaskAsync(Guid taskId, int minutes);
    Task SaveAsync();
    Task<IReadOnlyList<TaskItem>> GetTasksForListAsync(string listId);
    Task SelectListAsync(string listId);
    Task AddTaskAsync(string name, string? listId);
    Task RefreshGoogleListsAsync();
    Task UpdateListVisibilityAsync(string listId, bool isVisible);

    /// <summary>Adds a child task under <paramref name="parentTaskId"/>, mirroring the hierarchy to Google when the parent is a Google task.</summary>
    Task AddSubtaskAsync(string name, Guid parentTaskId);

    /// <summary>Moves a task under a new parent, or to the root when <paramref name="newParentId"/> is null.</summary>
    Task ReparentTaskAsync(Guid taskId, Guid? newParentId);

    /// <summary>Persists a virtual repeat occurrence as a real task so it can be edited independently of its series.</summary>
    Task MaterializeSingleAsync(TaskItem occurrence);

    /// <summary>Reloads all task data from storage, refreshing the in-memory cache. Call this after import operations to reflect changes.</summary>
    Task ReloadAsync();
}
