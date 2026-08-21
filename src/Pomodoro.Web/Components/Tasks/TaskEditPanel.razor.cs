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
    public IReadOnlyList<TaskListRef> GoogleLists { get; set; } = [];

    [Parameter]
    public EventCallback<TaskItem> OnSave { get; set; }

    [Parameter]
    public EventCallback OnCancel { get; set; }

    protected bool IsSubtask => Task.IsSubtask;

    protected string EditName { get; set; } = string.Empty;

    protected ElementReference _subtaskInput;

    protected bool EditFollowParent { get; set; }
    private bool _originalFollowParent;
    protected RepeatType EditRepeatType { get; set; }
    protected DayOfWeek[] EditWeekdays { get; set; } = [];
    protected int EditCustomDays { get; set; } = Constants.Repeat.DefaultCustomDays;
    protected int EditMonthlyDay { get; set; } = Constants.Repeat.DefaultMonthlyDay;
    protected int EditQuarterlyDay { get; set; } = DateTime.Now.Day;
    protected int EditQuarterlyMonth { get; set; } = RepeatRule.QuarterGroupOf(DateTime.Now);
    protected int EditYearlyDay { get; set; } = DateTime.Now.Day;
    protected int EditYearlyMonth { get; set; } = DateTime.Now.Month;
    protected DateTime? EditScheduledDate { get; set; }
    protected bool EditIsPaused { get; set; }
    protected DateTime? EditPausedDate { get; set; }
    protected string EditListId { get; set; } = Constants.TaskLists.LocalPomodoroListId;

    protected string EditRepeatChoice
    {
        get => IsSubtask && EditFollowParent ? Constants.Repeat.FollowParentChoice : EditRepeatType.ToString();
        set
        {
            if (IsSubtask && value == Constants.Repeat.FollowParentChoice)
            {
                EditFollowParent = true;
                return;
            }
            EditFollowParent = false;
            if (Enum.TryParse<RepeatType>(value, out var parsed))
                EditRepeatType = parsed;
        }
    }

    protected bool ShowPause => !EditFollowParent && EditRepeatType != RepeatType.None;

    protected static DayOfWeek[] WeekdayOptions =>
    [
        DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
        DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday
    ];

    protected override void OnInitialized()
    {
        EditName = Task.Name;
        EditFollowParent = IsSubtask && Task.FollowsParentRepeat;
        _originalFollowParent = EditFollowParent;
        EditRepeatType = Task.Repeat?.Type ?? RepeatType.None;
        EditWeekdays = Task.Repeat?.Weekdays ?? [];
        EditCustomDays = Task.Repeat?.CustomDays > 0 ? Task.Repeat.CustomDays : Constants.Repeat.DefaultCustomDays;
        EditMonthlyDay = Task.Repeat?.MonthlyDay ?? Constants.Repeat.DefaultMonthlyDay;
        var anchorDate = (Task.ScheduledDate ?? Task.Repeat?.StartDate ?? Task.CreatedAt).Date;
        EditQuarterlyDay = Task.Repeat?.QuarterlyDay ?? anchorDate.Day;
        EditQuarterlyMonth = Task.Repeat?.QuarterlyMonth ?? RepeatRule.QuarterGroupOf(anchorDate);
        EditYearlyDay = Task.Repeat?.YearlyDay ?? anchorDate.Day;
        EditYearlyMonth = Task.Repeat?.YearlyMonth ?? anchorDate.Month;
        EditScheduledDate = Task.ScheduledDate;
        EditIsPaused = Task.Repeat?.IsPaused ?? false;
        EditPausedDate = Task.Repeat?.PausedDate ?? null;
        EditListId = Task.GoogleListId ?? Constants.TaskLists.LocalPomodoroListId;
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
        var edited = Task.WithUpdates(ApplyEdits);
        await OnSave.InvokeAsync(edited);
    }

    private void ApplyEdits(TaskItem t)
    {
        t.Name = (EditName ?? string.Empty).Trim();
        t.ScheduledDate = EditScheduledDate;
        ApplyRepeatChoice(t);

        if (!IsSubtask)
            t.GoogleListId = EditListId == Constants.TaskLists.LocalPomodoroListId ? null : EditListId;
    }

    private void ApplyRepeatChoice(TaskItem t)
    {
        if (IsSubtask && EditFollowParent)
        {
            t.FollowsParentRepeat = true;
            if (!_originalFollowParent)
                t.Repeat = null;
            return;
        }

        t.FollowsParentRepeat = false;

        if (EditRepeatType == RepeatType.None)
        {
            t.Repeat = null;
            return;
        }

        t.Repeat = t.Repeat != null
            ? new RepeatRule
            {
                Type = EditRepeatType,
                Weekdays = EditWeekdays,
                CustomDays = EditCustomDays,
                MonthlyDay = EditMonthlyDay,
                QuarterlyDay = EditQuarterlyDay,
                QuarterlyMonth = EditQuarterlyMonth,
                YearlyDay = EditYearlyDay,
                YearlyMonth = EditYearlyMonth,
                IsPaused = EditIsPaused,
                PausedDate = EditIsPaused ? EditPausedDate : null,
                StartDate = t.Repeat.StartDate,
                EndDate = t.Repeat.EndDate,
                LastCompletedDate = t.Repeat.LastCompletedDate,
                NextOccurrence = null
            }
            : new RepeatRule
            {
                Type = EditRepeatType,
                Weekdays = EditWeekdays,
                CustomDays = EditCustomDays,
                MonthlyDay = EditMonthlyDay,
                QuarterlyDay = EditQuarterlyDay,
                QuarterlyMonth = EditQuarterlyMonth,
                YearlyDay = EditYearlyDay,
                YearlyMonth = EditYearlyMonth,
                IsPaused = EditIsPaused,
                PausedDate = EditIsPaused ? EditPausedDate : null,
                StartDate = EditScheduledDate
            };
    }

    protected async Task HandleCancel()
    {
        await OnCancel.InvokeAsync();
    }
}
