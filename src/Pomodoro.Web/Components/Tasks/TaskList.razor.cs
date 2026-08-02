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

    [Parameter]
    public EventCallback<TaskItem> OnTaskEdit { get; set; }

    [Parameter]
    public EventCallback<AddSubtaskRequest> OnAddSubtask { get; set; }

    [Parameter]
    public EventCallback<Guid> OnReparentToRoot { get; set; }

    [Parameter]
    public IReadOnlyList<TaskListRef> GoogleLists { get; set; } = [];

    #endregion

    #region State

    protected bool IsAddingTask { get; set; }
    protected string NewTaskName { get; set; } = string.Empty;

    /// <summary>
    /// Determines if the Add button should be disabled
    /// </summary>
    protected bool IsAddDisabled => string.IsNullOrWhiteSpace(NewTaskName);

    protected bool HasCompletedTasks => Tasks.Any(t => t.IsCompleted);

    protected bool _isCompletedExpanded;

    protected HashSet<Guid> _collapsed = new();
    protected HashSet<Guid> _parentIds = new();

    protected sealed record TaskNode(TaskItem Task, int Depth, bool HasChildren, bool IsCompletedAncestor);

    protected IReadOnlyList<TaskNode> AllNodes => BuildTree(Tasks);

    protected IReadOnlyList<TaskNode> ActiveTaskNodes => AllNodes.Where(n => !n.Task.IsCompleted && !n.IsCompletedAncestor).ToList();

    protected IReadOnlyList<TaskNode> CompletedTaskNodes => AllNodes.Where(n => n.Task.IsCompleted || n.IsCompletedAncestor).ToList();

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        var googleIdToLocalId = Tasks
            .Where(t => !string.IsNullOrEmpty(t.GoogleTaskId))
            .ToDictionary(t => t.GoogleTaskId!, t => t.Id);
        _parentIds = Tasks
            .Where(t => t.ParentTaskId.HasValue)
            .Select(t => t.ParentTaskId!.Value)
            .Union(Tasks
                .Where(t => !string.IsNullOrEmpty(t.GoogleParentTaskId))
                .Where(t => googleIdToLocalId.ContainsKey(t.GoogleParentTaskId!))
                .Select(t => googleIdToLocalId[t.GoogleParentTaskId!]))
            .ToHashSet();
    }

    protected string? GoogleListTitleFor(TaskItem task) =>
        string.IsNullOrEmpty(task.GoogleListId)
            ? null
            : GoogleLists.FirstOrDefault(l => l.Id == task.GoogleListId)?.Title;

    protected void ToggleCollapse(Guid taskId)
    {
        if (!_collapsed.Add(taskId))
            _collapsed.Remove(taskId);
    }

    protected void ToggleCompletedExpanded()
    {
        _isCompletedExpanded = !_isCompletedExpanded;
    }

    private IReadOnlyList<TaskNode> BuildTree(IReadOnlyList<TaskItem> tasks)
    {
        var result = new List<TaskNode>();
        if (tasks.Count == 0) return result;

        var taskById = tasks.ToDictionary(t => t.Id);
        var googleIdToTask = tasks
            .Where(t => !string.IsNullOrEmpty(t.GoogleTaskId))
            .GroupBy(t => t.GoogleTaskId!)
            .ToDictionary(g => g.Key, g => g.First().Id);
        var childrenByLocalParent = tasks
            .Where(t => t.ParentTaskId.HasValue)
            .GroupBy(t => t.ParentTaskId!.Value)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<TaskItem>)g.OrderBy(t => t.CreatedAt).ToList());
        var childrenByGoogleParent = tasks
            .Where(t => !string.IsNullOrEmpty(t.GoogleParentTaskId))
            .GroupBy(t => t.GoogleParentTaskId!)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<TaskItem>)g
                .OrderBy(t => t.GooglePosition ?? string.Empty, StringComparer.Ordinal)
                .ToList());
        var visited = new HashSet<Guid>();

        bool HasKnownParent(TaskItem t) =>
            (t.ParentTaskId.HasValue && taskById.ContainsKey(t.ParentTaskId.Value)) ||
            (!string.IsNullOrEmpty(t.GoogleParentTaskId) && googleIdToTask.ContainsKey(t.GoogleParentTaskId));

        var roots = tasks.Where(t => !HasKnownParent(t));

        void Walk(TaskItem task, int depth, bool underCompleted)
        {
            if (!visited.Add(task.Id)) return;
            var isUnderCompleted = underCompleted || task.IsCompleted;
            result.Add(new TaskNode(task, depth, _parentIds.Contains(task.Id), isUnderCompleted));
            if (_collapsed.Contains(task.Id)) return;
            if (childrenByLocalParent.TryGetValue(task.Id, out var localKids))
                foreach (var kid in localKids)
                    Walk(kid, depth + 1, isUnderCompleted);
            if (!string.IsNullOrEmpty(task.GoogleTaskId) &&
                childrenByGoogleParent.TryGetValue(task.GoogleTaskId, out var googleKids))
                foreach (var kid in googleKids)
                    Walk(kid, depth + 1, isUnderCompleted);
        }

        foreach (var root in roots)
            Walk(root, 0, false);

        return result;
    }

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

    protected async Task HandleTaskEdit(TaskItem task)
    {
        await OnTaskEdit.InvokeAsync(task);
    }

    protected async Task HandleAddSubtask(AddSubtaskRequest request)
    {
        await OnAddSubtask.InvokeAsync(request);
    }

    protected async Task HandleReparentToRoot(Guid taskId)
    {
        await OnReparentToRoot.InvokeAsync(taskId);
    }

    #endregion
}
