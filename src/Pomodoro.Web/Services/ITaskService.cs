using Pomodoro.Web.Models;

namespace Pomodoro.Web.Services;

public interface ITaskService
{
    event Action? OnChange;

    List<TaskItem> Tasks { get; }

    IReadOnlyList<TaskItem> AllTasks { get; }

    Guid? CurrentTaskId { get; }
    TaskItem? CurrentTask { get; }

    IReadOnlyList<TaskListRef> TaskLists { get; }

    /// <summary>Connected Google lists; these are sources feeding the two tabs, not tabs themselves.</summary>
    IReadOnlyList<TaskListRef> GoogleLists { get; }
    TaskListRef? CurrentList { get; }
    string? CurrentListId { get; }

    Task InitializeAsync();
    Task AddTaskAsync(string name);
    Task UpdateTaskAsync(TaskItem task);
    Task DeleteTaskAsync(Guid taskId);
    Task RestoreTaskAsync(Guid taskId);
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

    Task<Guid?> AddSubtaskAsync(string name, Guid parentTaskId);

    Task ReparentTaskAsync(Guid taskId, Guid? newParentId);

    Task PromoteTaskAsync(Guid taskId);

    Task DemoteTaskAsync(Guid taskId, Guid targetSiblingId);

    Task<bool> ReorderTaskAsync(Guid taskId, Guid targetId, bool insertBefore);

    Task SetFollowsParentRepeatAsync(Guid taskId, bool value);

    /// <summary>Persists a virtual repeat occurrence as a real task so it can be edited independently of its series.</summary>
    Task MaterializeSingleAsync(TaskItem occurrence);

    Task ReloadAsync();
}
