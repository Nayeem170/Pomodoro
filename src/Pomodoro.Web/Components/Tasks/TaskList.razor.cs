using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Pomodoro.Web.Models;

namespace Pomodoro.Web.Components.Tasks;

/// <summary>
/// Code-behind for TaskList component
/// Separates business logic from view
/// </summary>
public class TaskListBase : ComponentBase
{
    #region Parameters (Model)

    [Parameter]
    public List<TaskItem> Tasks { get; set; } = new();

    [Parameter]
    public Guid? CurrentTaskId { get; set; }

    [Parameter]
    public EventCallback<string> OnTaskAdd { get; set; }

    [Parameter]
    public EventCallback<Guid> OnTaskSelect { get; set; }

    [Parameter]
    public EventCallback<Guid> OnTaskComplete { get; set; }

    [Parameter]
    public EventCallback<Guid> OnTaskDelete { get; set; }

    [Parameter]
    public EventCallback<Guid> OnTaskUncomplete { get; set; }

    #endregion

    #region State

    protected bool IsAddingTask { get; set; }
    protected string NewTaskName { get; set; } = string.Empty;

    /// <summary>
    /// Determines if the Add button should be disabled
    /// </summary>
    protected bool IsAddDisabled => string.IsNullOrWhiteSpace(NewTaskName);

    protected IReadOnlyList<TaskItem> ActiveTasks
        => Tasks.Where(t => !t.IsCompleted)
                .OrderByDescending(t => t.LastWorkedOn ?? t.CreatedAt)
                .ToList();

    protected IReadOnlyList<TaskItem> CompletedTasks
        => Tasks.Where(t => t.IsCompleted)
                .OrderByDescending(t => t.LastWorkedOn ?? t.CreatedAt)
                .ToList();

    protected bool HasCompletedTasks => Tasks.Any(t => t.IsCompleted);

    #endregion

    #region Business Logic Methods

    /// <summary>
    /// Starts the add task form
    /// </summary>
    protected void StartAddTask()
    {
        IsAddingTask = true;
        NewTaskName = string.Empty;
    }

    /// <summary>
    /// Cancels adding a new task
    /// </summary>
    protected void CancelAddTask()
    {
        IsAddingTask = false;
        NewTaskName = string.Empty;
    }

    /// <summary>
    /// Handles adding a new task
    /// </summary>
    protected async Task HandleAddTask()
    {
        if (!string.IsNullOrWhiteSpace(NewTaskName))
        {
            await OnTaskAdd.InvokeAsync(NewTaskName.Trim());
            NewTaskName = string.Empty;
            IsAddingTask = false;
        }
    }

    /// <summary>
    /// Handles keyboard events in the add task input
    /// </summary>
    protected async Task HandleKeyPress(KeyboardEventArgs e)
    {
        if (e.Key == Constants.Keys.Enter && !string.IsNullOrWhiteSpace(NewTaskName))
        {
            await HandleAddTask();
        }
        else if (e.Key == Constants.Keys.Escape)
        {
            CancelAddTask();
        }
    }

    /// <summary>
    /// Handles task selection
    /// </summary>
    protected async Task HandleTaskSelect(Guid taskId)
    {
        await OnTaskSelect.InvokeAsync(taskId);
    }

    /// <summary>
    /// Handles task completion
    /// </summary>
    protected async Task HandleTaskComplete(Guid taskId)
    {
        await OnTaskComplete.InvokeAsync(taskId);
    }

    /// <summary>
    /// Handles task deletion
    /// </summary>
    protected async Task HandleTaskDelete(Guid taskId)
    {
        await OnTaskDelete.InvokeAsync(taskId);
    }

    /// <summary>
    /// Handles task uncomplete (undo completion)
    /// </summary>
    protected async Task HandleTaskUncomplete(Guid taskId)
    {
        await OnTaskUncomplete.InvokeAsync(taskId);
    }

    #endregion
}
