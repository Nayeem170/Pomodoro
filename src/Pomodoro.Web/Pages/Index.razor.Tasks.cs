namespace Pomodoro.Web.Pages;

/// <summary>
/// Task actions partial for Index page
/// Contains all task-related event handlers
/// </summary>
public partial class IndexBase
{
    #region Task Actions

    private async Task TryExecuteAsync(Func<Task> action, string errorMessage)
    {
        try
        {
            await action();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"{errorMessage}: {ex.Message}";
        }
    }

    /// <summary>
    /// Handles adding a new task
    /// </summary>
    public async Task HandleTaskAdd(string taskName)
    {
        await TryExecuteAsync(async () =>
        {
            await TaskService.AddTaskAsync(taskName);
            UpdateState();
            StateHasChanged();
        }, Constants.Messages.ErrorAddingTask);
    }

    /// <summary>
    /// Handles selecting a task as the current task
    /// </summary>
    public async Task HandleTaskSelect(Guid taskId)
    {
        await TryExecuteAsync(async () =>
        {
            await TaskService.SelectTaskAsync(taskId);
            UpdateState();
            StateHasChanged();
        }, Constants.Messages.ErrorSelectingTask);
    }

    /// <summary>
    /// Handles marking a task as completed
    /// </summary>
    public async Task HandleTaskComplete(Guid taskId)
    {
        await TryExecuteAsync(async () =>
        {
            await TaskService.CompleteTaskAsync(taskId);
            UpdateState();
            StateHasChanged();
        }, Constants.Messages.ErrorCompletingTask);
    }

    /// <summary>
    /// Handles deleting a task (soft delete)
    /// </summary>
    public async Task HandleTaskDelete(Guid taskId)
    {
        await TryExecuteAsync(async () =>
        {
            await TaskService.DeleteTaskAsync(taskId);
            UpdateState();
            StateHasChanged();
        }, Constants.Messages.ErrorDeletingTask);
    }

    /// <summary>
    /// Handles uncompleting a task
    /// </summary>
    public async Task HandleTaskUncomplete(Guid taskId)
    {
        await TryExecuteAsync(async () =>
        {
            await TaskService.UncompleteTaskAsync(taskId);
            UpdateState();
            StateHasChanged();
        }, Constants.Messages.ErrorUncompletingTask);
    }

    #endregion
}
