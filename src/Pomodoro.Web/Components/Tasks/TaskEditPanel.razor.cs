using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Pomodoro.Web.Models;

namespace Pomodoro.Web.Components.Tasks;

public class TaskEditPanelBase : ComponentBase
{
    [Parameter]
    public TaskItem Task { get; set; } = default!;

    [Parameter]
    public DateTime? ContextDate { get; set; }

    [Parameter]
    public EventCallback<TaskItem> OnSave { get; set; }

    [Parameter]
    public EventCallback OnCancel { get; set; }

    protected bool IsSubtask => Task.IsSubtask;

    protected string EditName { get; set; } = string.Empty;

    protected ElementReference _subtaskInput;

    protected RepeatType EditRepeatType { get; set; }
    protected DayOfWeek[] EditWeekdays { get; set; } = [];
    protected int EditCustomDays { get; set; } = Constants.Repeat.DefaultCustomDays;
    protected int EditMonthlyDay { get; set; } = Constants.Repeat.DefaultMonthlyDay;
    protected DateTime? EditScheduledDate { get; set; }
    protected bool EditIsPaused { get; set; }
    protected DateTime? EditPausedDate { get; set; }

    protected static DayOfWeek[] WeekdayOptions =>
    [
        DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
        DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday
    ];

    protected override void OnInitialized()
    {
        EditName = Task.Name;
        EditRepeatType = Task.Repeat?.Type ?? RepeatType.None;
        EditWeekdays = Task.Repeat?.Weekdays ?? [];
        EditCustomDays = Task.Repeat?.CustomDays > 0 ? Task.Repeat.CustomDays : Constants.Repeat.DefaultCustomDays;
        EditMonthlyDay = Task.Repeat?.MonthlyDay ?? Constants.Repeat.DefaultMonthlyDay;
        EditScheduledDate = Task.ScheduledDate;
        EditIsPaused = Task.Repeat?.IsPaused ?? false;
        EditPausedDate = Task.Repeat?.PausedDate ?? null;
    }

    protected void TogglePause()
    {
        EditIsPaused = !EditIsPaused;
        EditPausedDate = EditIsPaused ? ContextDate : null;
    }

    protected void ToggleWeekday(DayOfWeek day)
    {
        var list = EditWeekdays.ToList();
        if (list.Contains(day))
            list.Remove(day);
        else
            list.Add(day);
        EditWeekdays = [.. list.OrderBy(d => d)];
    }

    protected async Task HandleNameKey(KeyboardEventArgs e)
    {
        if (!IsSubtask)
            return;
        await HandleSubtaskKey(e);
    }

    protected async Task HandleSubtaskKey(KeyboardEventArgs e)
    {
        if (e.Key == Constants.Keys.Enter)
            await HandleSave();
        else if (e.Key == Constants.Keys.Escape)
            await HandleCancel();
    }

    protected async Task HandleSave()
    {
        Task.Name = (EditName ?? string.Empty).Trim();

        if (IsSubtask)
        {
            Task.ScheduledDate = EditScheduledDate;
            await OnSave.InvokeAsync(Task);
            return;
        }

        if (EditRepeatType == RepeatType.None)
        {
            Task.Repeat = null;
        }
        else
        {
            var repeat = Task.Repeat != null
                ? new RepeatRule
                {
                    Type = EditRepeatType,
                    Weekdays = EditWeekdays,
                    CustomDays = EditCustomDays,
                    MonthlyDay = EditMonthlyDay,
                    IsPaused = EditIsPaused,
                    PausedDate = EditIsPaused ? EditPausedDate : null,
                    StartDate = Task.Repeat.StartDate,
                    EndDate = Task.Repeat.EndDate,
                    LastCompletedDate = Task.Repeat.LastCompletedDate,
                    NextOccurrence = null
                }
                : new RepeatRule
                {
                    Type = EditRepeatType,
                    Weekdays = EditWeekdays,
                    CustomDays = EditCustomDays,
                    MonthlyDay = EditMonthlyDay,
                    IsPaused = EditIsPaused,
                    PausedDate = EditIsPaused ? EditPausedDate : null,
                    StartDate = EditScheduledDate
                };
            Task.Repeat = repeat;
        }

        Task.ScheduledDate = EditScheduledDate;
        await OnSave.InvokeAsync(Task);
    }

    protected async Task HandleCancel()
    {
        await OnCancel.InvokeAsync();
    }
}
