using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;

namespace Pomodoro.Web.Components.Tasks;

public class TaskListBase : ComponentBase
{
    #region Parameters (Model)

    [Parameter]
    public List<TaskItem> Tasks { get; set; } = new();

    [Parameter]
    public Guid? CurrentTaskId { get; set; }

    [Parameter]
    public EventCallback<NewTaskRequest> OnTaskAdd { get; set; }

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
    public EventCallback<DemoteRequest> OnDemote { get; set; }

    [Parameter]
    public EventCallback<Guid> OnToggleFollowParent { get; set; }

    [Parameter]
    public EventCallback<ReorderRequest> OnTaskReorder { get; set; }

    [Parameter]
    public IReadOnlyList<TaskListRef> GoogleLists { get; set; } = [];

    [Parameter]
    public string? ActiveListId { get; set; }

    [Parameter]
    public Guid? HighlightTaskId { get; set; }

    #endregion

    #region State

    protected string NewTaskName { get; set; } = string.Empty;

    protected bool _isMoreExpanded;

    protected RepeatType _newTaskRepeatType = RepeatType.None;

    protected DayOfWeek[] _newTaskWeekdays = [];

    protected int _newTaskCustomDays = Constants.Repeat.DefaultCustomDays;

    protected int _newTaskMonthlyDay = Constants.Repeat.DefaultMonthlyDay;

    protected bool _newTaskIsPaused;

    protected DateTime? _newTaskPausedDate;

    protected DateTime? _newTaskScheduledDate;

    protected string? _newTaskListId;

    protected static DayOfWeek[] WeekdayOptions =>
    [
        DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
        DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday
    ];

    protected bool IsAddDisabled => string.IsNullOrWhiteSpace(NewTaskName);

    protected bool HasCompletedTasks => CompletedTaskNodes.Any();

    protected bool _isCompletedExpanded;

    protected HashSet<Guid> _collapsed = new();
    protected HashSet<Guid> _parentIds = new();
    protected bool _isDragging;
    protected Guid? _draggedTaskId;

    protected sealed record TaskNode(TaskItem Task, int Depth, bool HasChildren, int ChildCount, bool IsUnderCompletedRoot);

    protected IReadOnlyList<TaskNode> AllNodes => BuildTree(Tasks);

    protected IReadOnlyList<TaskNode> ActiveTaskNodes => AllNodes.Where(n => !n.IsUnderCompletedRoot).ToList();

    protected IReadOnlyList<TaskNode> CompletedTaskNodes => AllNodes.Where(n => n.IsUnderCompletedRoot).ToList();

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

    protected IReadOnlyList<TaskItem> SiblingsFor(TaskItem task)
    {
        return TaskGrouping
            .GetOrderedSiblingGroup(Tasks, task)
            .Where(t => t.Id != task.Id)
            .ToList();
    }

    protected bool IsReorderableFor(TaskItem task)
    {
        var group = TaskGrouping.GetSiblingGroup(Tasks, task);
        return group.Count > 1 && group.All(t => !t.IsGoogleTask);
    }

    protected IReadOnlyList<TaskItem> OrderedGroupFor(TaskItem task) =>
        TaskGrouping.GetOrderedSiblingGroup(Tasks, task);

    protected void HandleDragStarted(Guid taskId)
    {
        _isDragging = true;
        _draggedTaskId = taskId;
        StateHasChanged();
    }

    protected void HandleDragEnded()
    {
        _isDragging = false;
        _draggedTaskId = null;
        StateHasChanged();
    }

    protected async Task HandleTaskReorder(ReorderRequest request)
    {
        _isDragging = false;
        await OnTaskReorder.InvokeAsync(request);
        StateHasChanged();
    }

    protected void ToggleCollapse(Guid taskId)
    {
        if (!_collapsed.Add(taskId))
            _collapsed.Remove(taskId);
    }

    protected void ToggleCompletedExpanded()
    {
        _isCompletedExpanded = !_isCompletedExpanded;
    }

    protected void ToggleMore()
    {
        if (!_isMoreExpanded)
        {
            _newTaskListId = ActiveListId ?? Constants.TaskLists.LocalPomodoroListId;
        }
        _isMoreExpanded = !_isMoreExpanded;
    }

    protected void CancelMore()
    {
        NewTaskName = string.Empty;
        _newTaskRepeatType = RepeatType.None;
        _newTaskWeekdays = [];
        _newTaskCustomDays = Constants.Repeat.DefaultCustomDays;
        _newTaskMonthlyDay = Constants.Repeat.DefaultMonthlyDay;
        _newTaskIsPaused = false;
        _newTaskPausedDate = null;
        _newTaskScheduledDate = null;
        _newTaskListId = null;
        _isMoreExpanded = false;
    }

    protected void ToggleNewWeekday(DayOfWeek day)
    {
        var list = _newTaskWeekdays.ToList();
        if (list.Contains(day))
            list.Remove(day);
        else
            list.Add(day);
        _newTaskWeekdays = [.. list.OrderBy(d => d)];
    }

    private IReadOnlyList<TaskNode> BuildTree(IReadOnlyList<TaskItem> tasks)
    {
        var result = new List<TaskNode>();
        if (tasks.Count == 0) return result;

        var lookups = TaskGrouping.BuildLookups(tasks);
        var childrenByLocalParent = lookups.Live
            .Where(t => t.ParentTaskId.HasValue)
            .GroupBy(t => t.ParentTaskId!.Value)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<TaskItem>)g.OrderBy(t => t.SortOrder).ThenBy(t => t.CreatedAt).ToList());
        var childrenByGoogleParent = lookups.Live
            .Where(t => !string.IsNullOrEmpty(t.GoogleParentTaskId))
            .GroupBy(t => t.GoogleParentTaskId!)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<TaskItem>)g
                .OrderBy(t => t.GooglePosition ?? string.Empty, StringComparer.Ordinal)
                .ToList());
        var visited = new HashSet<Guid>();

        int ChildCountFor(TaskItem t)
        {
            var count = 0;
            if (childrenByLocalParent.TryGetValue(t.Id, out var localKids))
                count += localKids.Count;
            if (!string.IsNullOrEmpty(t.GoogleTaskId) &&
                childrenByGoogleParent.TryGetValue(t.GoogleTaskId, out var googleKids))
                count += googleKids.Count;
            return count;
        }

        var roots = lookups.Live
            .Where(t => !TaskGrouping.HasKnownParent(t, lookups))
            .OrderBy(t => t.SortOrder)
            .ThenByDescending(t => t.CreatedAt);

        void Walk(TaskItem task, int depth, bool rootIsCompleted)
        {
            if (!visited.Add(task.Id)) return;
            var nodeRootCompleted = depth == 0 ? task.IsCompleted : rootIsCompleted;
            result.Add(new TaskNode(task, depth, _parentIds.Contains(task.Id), ChildCountFor(task), nodeRootCompleted));
            if (_collapsed.Contains(task.Id)) return;
            if (childrenByLocalParent.TryGetValue(task.Id, out var localKids))
                foreach (var kid in localKids)
                    Walk(kid, depth + 1, nodeRootCompleted);
            if (!string.IsNullOrEmpty(task.GoogleTaskId) &&
                childrenByGoogleParent.TryGetValue(task.GoogleTaskId, out var googleKids))
                foreach (var kid in googleKids)
                    Walk(kid, depth + 1, nodeRootCompleted);
        }

        foreach (var root in roots)
            Walk(root, 0, false);

        return result;
    }

    #endregion

    #region Business Logic Methods

    protected async Task HandleAddTask()
    {
        if (!string.IsNullOrWhiteSpace(NewTaskName))
        {
            await OnTaskAdd.InvokeAsync(new NewTaskRequest(
                NewTaskName.Trim(),
                _newTaskRepeatType,
                _newTaskScheduledDate,
                _newTaskWeekdays.Length > 0 ? _newTaskWeekdays : null,
                _newTaskCustomDays,
                _newTaskMonthlyDay,
                _newTaskIsPaused,
                _newTaskPausedDate,
                _newTaskListId));
            NewTaskName = string.Empty;
            _newTaskRepeatType = RepeatType.None;
            _newTaskWeekdays = [];
            _newTaskCustomDays = Constants.Repeat.DefaultCustomDays;
            _newTaskMonthlyDay = Constants.Repeat.DefaultMonthlyDay;
            _newTaskIsPaused = false;
            _newTaskPausedDate = null;
            _newTaskScheduledDate = null;
            _newTaskListId = null;
        }
    }

    protected async Task HandleKeyPress(KeyboardEventArgs e)
    {
        if (e.ShiftKey && e.Key == Constants.Keys.Enter)
        {
            _isMoreExpanded = true;
        }
        else if (e.Key == Constants.Keys.Enter && !string.IsNullOrWhiteSpace(NewTaskName))
        {
            await HandleAddTask();
        }
        else if (e.Key == Constants.Keys.Escape)
        {
            CancelMore();
        }
    }

    protected async Task HandleTaskSelect(Guid taskId)
    {
        await OnTaskSelect.InvokeAsync(taskId);
    }

    protected async Task HandleTaskComplete(Guid taskId)
    {
        await OnTaskComplete.InvokeAsync(taskId);
    }

    protected async Task HandleTaskDelete(Guid taskId)
    {
        await OnTaskDelete.InvokeAsync(taskId);
    }

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

    protected async Task HandleDemote(DemoteRequest request)
    {
        await OnDemote.InvokeAsync(request);
    }

    protected async Task HandleToggleFollowParent(Guid taskId)
    {
        await OnToggleFollowParent.InvokeAsync(taskId);
    }

    #endregion
}
