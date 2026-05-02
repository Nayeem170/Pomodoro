using Microsoft.AspNetCore.Components;
using Pomodoro.Web.Models;

namespace Pomodoro.Web.Components.Tasks;

public class TaskEditPanelBase : ComponentBase
{
    [Parameter]
    public TaskItem Task { get; set; } = default!;

    [Parameter]
    public EventCallback<TaskItem> OnSave { get; set; }

    [Parameter]
    public EventCallback OnCancel { get; set; }

    protected RepeatType EditRepeatType { get; set; }
    protected DayOfWeek[] EditWeekdays { get; set; } = [];
    protected int EditCustomDays { get; set; } = Constants.Repeat.DefaultCustomDays;
    protected int EditMonthlyDay { get; set; } = Constants.Repeat.DefaultMonthlyDay;
    protected DateTime? EditScheduledDate { get; set; }
    protected bool EditIsPaused { get; set; }

    protected static DayOfWeek[] WeekdayOptions =>
    [
        DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
        DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday
    ];

    protected override void OnInitialized()
    {
        EditRepeatType = Task.Repeat?.Type ?? RepeatType.None;
        EditWeekdays = Task.Repeat?.Weekdays ?? [];
        EditCustomDays = Task.Repeat?.CustomDays > 0 ? Task.Repeat.CustomDays : Constants.Repeat.DefaultCustomDays;
        EditMonthlyDay = Task.Repeat?.MonthlyDay ?? Constants.Repeat.DefaultMonthlyDay;
        EditScheduledDate = Task.ScheduledDate;
        EditIsPaused = Task.Repeat?.IsPaused ?? false;
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

    protected async Task HandleSave()
    {
        if (EditRepeatType == RepeatType.None)
        {
            Task.Repeat = null;
        }
        else
        {
            Task.Repeat ??= new RepeatRule();
            Task.Repeat.Type = EditRepeatType;
            Task.Repeat.Weekdays = EditWeekdays;
            Task.Repeat.CustomDays = EditCustomDays;
            Task.Repeat.MonthlyDay = EditMonthlyDay;
            Task.Repeat.IsPaused = EditIsPaused;
            Task.Repeat.NextOccurrence = null;
        }

        Task.ScheduledDate = EditScheduledDate;
        await OnSave.InvokeAsync(Task);
    }

    protected async Task HandleCancel()
    {
        await OnCancel.InvokeAsync();
    }
}
