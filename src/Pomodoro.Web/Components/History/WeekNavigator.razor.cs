using Microsoft.AspNetCore.Components;

namespace Pomodoro.Web.Components.History;

public class WeekNavigatorBase : ComponentBase
{
    #region Parameters

    [Parameter]
    public DateTime SelectedWeekStart { get; set; }

    [Parameter]
    public EventCallback<DateTime> OnWeekChanged { get; set; }

    #endregion

    #region Properties

    protected bool IsThisWeek
    {
        get
        {
            var thisWeekStart = GetWeekStart(DateTime.Now.Date);
            return SelectedWeekStart.Date == thisWeekStart.Date;
        }
    }

    #endregion

    #region Actions

    protected async Task GoToPreviousWeek()
    {
        var newWeekStart = SelectedWeekStart.AddDays(-Constants.Charts.DaysPerWeek);
        await OnWeekChanged.InvokeAsync(newWeekStart);
    }

    protected async Task GoToNextWeek()
    {
        var newWeekStart = SelectedWeekStart.AddDays(Constants.Charts.DaysPerWeek);
        await OnWeekChanged.InvokeAsync(newWeekStart);
    }

    protected async Task GoToThisWeek()
    {
        var thisWeekStart = GetWeekStart(DateTime.Now.Date);
        await OnWeekChanged.InvokeAsync(thisWeekStart);
    }

    #endregion

    #region Helper Methods

    public static DateTime GetWeekStart(DateTime date)
    {
        int dayOfWeek = (int)date.DayOfWeek;
        int daysSinceSaturday = (dayOfWeek + Constants.TimeConversion.SaturdayBasedWeekOffset) % Constants.TimeConversion.DaysInWeek; // Sat=0, Sun=1, ..., Fri=6
        return date.AddDays(-daysSinceSaturday).Date;
    }

    protected string FormatWeekRange(DateTime weekStart)
    {
        var weekEnd = weekStart.AddDays(Constants.TimeConversion.WeeklyLookbackDays);
        return string.Format(Constants.History.WeekRangeFormat, weekStart, weekEnd);
    }

    #endregion
}
