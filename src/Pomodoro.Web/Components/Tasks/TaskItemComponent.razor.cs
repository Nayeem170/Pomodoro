using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Pomodoro.Web.Models;

namespace Pomodoro.Web.Components.Tasks;

/// <summary>
/// Code-behind for TaskItem component
/// Separates business logic from view
/// </summary>
public class TaskItemBase : ComponentBase
{
    #region Parameters (Model)

    [Parameter]
    public TaskItem Item { get; set; } = default!;

    [Parameter]
    public bool IsSelected { get; set; }

    [Parameter]
    public EventCallback<Guid> OnSelect { get; set; }

    [Parameter]
    public EventCallback<Guid> OnComplete { get; set; }

    [Parameter]
    public EventCallback<Guid> OnDelete { get; set; }

    [Parameter]
    public EventCallback<Guid> OnUncomplete { get; set; }

    [Parameter]
    public EventCallback<TaskItem> OnEdit { get; set; }

    #endregion

    #region State

    protected bool IsEditing { get; set; }

    #endregion

    #region Business Logic Methods

    /// <summary>
    /// Formats minutes into human-readable time format
    /// </summary>
    protected string FormatTime(int minutes)
    {
        if (minutes < Constants.TimeConversion.MinutesPerHour)
            return string.Format(Constants.TimeFormats.MinutesFormat, minutes);
        var hours = minutes / Constants.TimeConversion.MinutesPerHour;
        var mins = minutes % Constants.TimeConversion.MinutesPerHour;
        return string.Format(Constants.TimeFormats.HoursMinutesFormat, hours, mins);
    }

    /// <summary>
    /// Gets the CSS class for the task item
    /// </summary>
    protected string GetTaskClass()
    {
        var classes = new List<string>();
        if (IsSelected) classes.Add(Constants.Tasks.SelectedClass);
        if (Item.IsCompleted) classes.Add(Constants.Tasks.CompletedClass);
        return string.Join(" ", classes);
    }

    /// <summary>
    /// Gets the status icon for the task
    /// </summary>
    protected string GetStatusIcon()
    {
        if (Item.IsCompleted) return Constants.Tasks.CompletedEmoji;
        if (Item.PomodoroCount > 0) return Constants.Tasks.HasPomodorosEmoji;
        return Constants.Tasks.DefaultEmoji;
    }

    protected string GetRepeatBadgeClass()
    {
        if (Item.Repeat?.IsPaused == true) return $"{Constants.Repeat.RepeatCssClass} {Constants.Repeat.PausedCssClass}";
        return Constants.Repeat.RepeatCssClass;
    }

    protected string GetRepeatTooltip()
    {
        if (Item.Repeat == null) return string.Empty;
        var typeLabel = Item.Repeat.Type switch
        {
            RepeatType.Daily => "Daily",
            RepeatType.Weekly => "Weekly",
            RepeatType.Custom => $"Every {Item.Repeat.CustomDays} days",
            RepeatType.Monthly => $"Monthly (day {Item.Repeat.MonthlyDay})",
            _ => "Repeats"
        };
        if (Item.Repeat.IsPaused) return $"{typeLabel} (paused)";
        return typeLabel;
    }

    /// <summary>
    /// Handles task selection click
    /// </summary>
    protected async Task HandleSelect()
    {
        if (!Item.IsCompleted)
        {
            await OnSelect.InvokeAsync(Item.Id);
        }
    }

    /// <summary>
    /// Handles task completion click
    /// </summary>
    protected async Task HandleComplete()
    {
        await OnComplete.InvokeAsync(Item.Id);
    }

    /// <summary>
    /// Handles task deletion click
    /// </summary>
    protected async Task HandleDelete()
    {
        await OnDelete.InvokeAsync(Item.Id);
    }

    /// <summary>
    /// Handles task uncomplete click (undo completion)
    /// </summary>
    protected async Task HandleUncomplete()
    {
        await OnUncomplete.InvokeAsync(Item.Id);
    }

    protected async Task HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" || e.Key == " ")
        {
            await HandleSelect();
        }
    }

    protected void HandleEdit()
    {
        IsEditing = true;
    }

    protected async Task HandleEditSave(TaskItem updatedTask)
    {
        await OnEdit.InvokeAsync(updatedTask);
        IsEditing = false;
    }

    protected void HandleEditCancel()
    {
        IsEditing = false;
    }

    #endregion
}
