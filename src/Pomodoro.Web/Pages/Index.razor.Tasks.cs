using Pomodoro.Web.Models;
using Pomodoro.Web.Services;

namespace Pomodoro.Web.Pages;

public partial class IndexBase
{
    protected Guid? _highlightTaskId;

    protected string? _reorderAnnouncement;

    private void ScheduleHighlightClear(Guid id)
    {
        SafeTaskRunner.RunAndForget(async () =>
        {
            await Task.Delay(Constants.UI.HighlightDurationMs);
            if (_highlightTaskId != id) return;
            _highlightTaskId = null;
            await InvokeAsync(StateHasChanged);
        }, Logger);
    }

    #region Task Actions

    private async Task TryExecuteAsync(Func<Task> action, string errorMessage)
    {
        try
        {
            await action();
        }
        catch (UnauthorizedAccessException)
        {
            ErrorMessage = Constants.Messages.GoogleReconnectNeeded;
        }
        catch (InvalidOperationException ex)
        {
            ErrorMessage = ex.Message;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"{errorMessage}: {ex.Message}";
        }
    }

    public async Task HandleTaskAdd(NewTaskRequest request)
    {
        await TryExecuteAsync(async () =>
        {
            var listId = string.IsNullOrEmpty(request.ListId) ? ActiveListId : request.ListId;
            await TaskService.AddTaskAsync(request.Name, listId);
            _highlightTaskId = TaskService.CurrentTaskId;
            if (_highlightTaskId.HasValue) ScheduleHighlightClear(_highlightTaskId.Value);

            if ((request.RepeatType != RepeatType.None || request.ScheduledDate.HasValue)
                && TaskService.CurrentTaskId.HasValue)
            {
                var newTask = AppState.Tasks.FirstOrDefault(t => t.Id == TaskService.CurrentTaskId.Value);
                if (newTask is not null)
                {
                    if (request.RepeatType != RepeatType.None)
                    {
                        newTask.Repeat = new RepeatRule
                        {
                            Type = request.RepeatType,
                            Weekdays = request.Weekdays ?? [],
                            CustomDays = request.CustomDays > 0 ? request.CustomDays : Constants.Repeat.DefaultCustomDays,
                            MonthlyDay = request.MonthlyDay > 0 ? request.MonthlyDay : Constants.Repeat.DefaultMonthlyDay,
                            IsPaused = request.IsPaused,
                            PausedDate = request.IsPaused ? request.PausedDate : null,
                            StartDate = request.ScheduledDate ?? DateTime.Now
                        };
                    }
                    if (request.ScheduledDate.HasValue)
                    {
                        newTask.ScheduledDate = request.ScheduledDate;
                    }
                    await TaskService.UpdateTaskAsync(newTask);
                }
            }

            await UpdateStateAsync();
            StateHasChanged();
        }, Constants.Messages.ErrorAddingTask);
    }

    public async Task HandleTaskSelect(Guid taskId)
    {
        await TryExecuteAsync(async () =>
        {
            await TaskService.SelectTaskAsync(taskId);
            if (TimerService.CurrentSession is { WasStarted: true } session
                && session.Type == SessionType.Pomodoro)
            {
                TimerService.ChangeCurrentTask(taskId);
                await PipTimerService.UpdateTimerAsync();
            }
            await UpdateStateAsync();
            StateHasChanged();
        }, Constants.Messages.ErrorSelectingTask);
    }

    public async Task HandleTaskComplete(Guid taskId)
    {
        await TryExecuteAsync(async () =>
        {
            await TaskService.CompleteTaskAsync(taskId);
            await UpdateStateAsync();
            StateHasChanged();
        }, Constants.Messages.ErrorCompletingTask);
    }

    public async Task HandleTaskReorder(ReorderRequest request)
    {
        await TryExecuteAsync(async () =>
        {
            if (await TaskService.ReorderTaskAsync(request.TaskId, request.TargetId, request.InsertBefore))
            {
                await UpdateStateAsync();
                _reorderAnnouncement = BuildReorderAnnouncement(request.TaskId);
                StateHasChanged();
            }
            else
            {
                _reorderAnnouncement = null;
            }
        }, Constants.Messages.ErrorUpdatingTask);
    }

    private string? BuildReorderAnnouncement(Guid taskId)
    {
        var task = AppState.FindTaskById(taskId);
        if (task is null) return null;

        var group = TaskGrouping.GetOrderedSiblingGroup(AppState.Tasks, task);
        var index = -1;
        for (var i = 0; i < group.Count; i++)
        {
            if (group[i].Id == taskId)
            {
                index = i;
                break;
            }
        }
        return index < 0
            ? null
            : string.Format(Constants.Messages.ReorderAnnouncementFormat, task.Name, index + 1, group.Count);
    }

    public async Task HandleTaskDelete(Guid taskId)
    {
        await TryExecuteAsync(async () =>
        {
            var task = AppState.FindTaskById(taskId);
            var taskName = task?.Name ?? "task";
            await TaskService.DeleteTaskAsync(taskId);
            await UpdateStateAsync();
            _undoTaskId = taskId;
            _undoTaskName = taskName;
            _undoToastVisible = true;
            _undoCts?.Cancel();
            _undoCts = new CancellationTokenSource();
            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(Constants.UI.UndoToastDurationMs, _undoCts.Token);
                    _undoToastVisible = false;
                    await InvokeAsync(StateHasChanged);
                }
                catch (OperationCanceledException) { }
            });
            StateHasChanged();
        }, Constants.Messages.ErrorDeletingTask);
    }

    public async Task HandleUndoDelete()
    {
        if (_undoTaskId is not { } taskId) return;
        _undoCts?.Cancel();
        _undoToastVisible = false;
        await TryExecuteAsync(async () =>
        {
            await TaskService.RestoreTaskAsync(taskId);
            await UpdateStateAsync();
            _undoTaskId = null;
            _undoTaskName = null;
            StateHasChanged();
        }, Constants.Messages.ErrorDeletingTask);
    }

    public async Task HandleTaskUncomplete(Guid taskId)
    {
        await TryExecuteAsync(async () =>
        {
            await TaskService.UncompleteTaskAsync(taskId);
            await UpdateStateAsync();
            StateHasChanged();
        }, Constants.Messages.ErrorUncompletingTask);
    }

    public async Task HandleTaskEdit(Pomodoro.Web.Models.TaskItem task)
    {
        await TryExecuteAsync(async () =>
        {
            await TaskService.UpdateTaskAsync(task);
            await UpdateStateAsync();
            StateHasChanged();
        }, Constants.Messages.ErrorUpdatingTask);
    }

    public async Task HandleScheduleEdit(Pomodoro.Web.Models.TaskItem task)
    {
        if (task.RepeatSeriesId.HasValue && task.OccurrenceDate.HasValue && !task.IsDeleted)
        {
            var existing = AppState.Tasks.FirstOrDefault(t =>
                t.RepeatSeriesId == task.RepeatSeriesId && t.OccurrenceDate.HasValue
                && t.OccurrenceDate.Value.Date == task.OccurrenceDate.Value.Date);
            if (existing is null)
            {
                await TaskService.MaterializeSingleAsync(task);
                await UpdateStateAsync();
                return;
            }
        }
        await HandleTaskEdit(task);
    }

    public async Task HandleAddSubtask(Pomodoro.Web.Models.AddSubtaskRequest request)
    {
        await TryExecuteAsync(async () =>
        {
            var newSubtaskId = await TaskService.AddSubtaskAsync(request.Name, request.ParentTaskId);
            if (newSubtaskId.HasValue)
            {
                _highlightTaskId = newSubtaskId;
                ScheduleHighlightClear(newSubtaskId.Value);
            }
            await UpdateStateAsync();
            StateHasChanged();
        }, Constants.Messages.ErrorAddingTask);
    }

    public async Task HandleReparentToRoot(Guid taskId)
    {
        await TryExecuteAsync(async () =>
        {
            await TaskService.PromoteTaskAsync(taskId);
            await UpdateStateAsync();
            StateHasChanged();
        }, Constants.Messages.ErrorUpdatingTask);
    }

    public async Task HandleDemote(DemoteRequest request)
    {
        await TryExecuteAsync(async () =>
        {
            await TaskService.DemoteTaskAsync(request.TaskId, request.TargetSiblingId);
            await UpdateStateAsync();
            StateHasChanged();
        }, Constants.Messages.ErrorUpdatingTask);
    }

    public async Task HandleToggleFollowParent(Guid taskId)
    {
        await TryExecuteAsync(async () =>
        {
            var task = AppState.Tasks.FirstOrDefault(t => t.Id == taskId);
            if (task == null) return;
            await TaskService.SetFollowsParentRepeatAsync(taskId, !task.FollowsParentRepeat);
            await UpdateStateAsync();
            StateHasChanged();
        }, Constants.Messages.ErrorUpdatingTask);
    }

    #endregion
}
