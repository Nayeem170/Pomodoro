using Pomodoro.Web.Models;

namespace Pomodoro.Web.Pages;

public partial class IndexBase
{
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
            await TaskService.AddSubtaskAsync(request.Name, request.ParentTaskId);
            await UpdateStateAsync();
            StateHasChanged();
        }, Constants.Messages.ErrorAddingTask);
    }

    public async Task HandleReparentToRoot(Guid taskId)
    {
        await TryExecuteAsync(async () =>
        {
            await TaskService.ReparentTaskAsync(taskId, null);
            await UpdateStateAsync();
            StateHasChanged();
        }, Constants.Messages.ErrorUpdatingTask);
    }

    #endregion
}
