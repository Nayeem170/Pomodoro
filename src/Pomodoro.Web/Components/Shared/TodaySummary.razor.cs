using Microsoft.AspNetCore.Components;

namespace Pomodoro.Web.Components.Shared;

public class TodaySummaryBase : ComponentBase
{
    #region Parameters (Model)

    [Parameter]
    public int TotalFocusMinutes { get; set; }

    [Parameter]
    public int PomodoroCount { get; set; }

    [Parameter]
    public int TasksWorkedOn { get; set; }

    [Parameter]
    public int DailyGoal { get; set; }

    #endregion

    #region Business Logic Methods

    protected string GetProgressWidth()
    {
        if (DailyGoal <= 0) return "0%";
        var pct = Math.Min(PomodoroCount * 100.0 / DailyGoal, 100);
        return $"{pct:F0}%";
    }

    protected string FormatTime(int minutes)
    {
        if (minutes < Constants.TimeConversion.MinutesPerHour)
            return string.Format(Constants.TimeFormats.MinutesFormat, minutes);
        var hours = minutes / Constants.TimeConversion.MinutesPerHour;
        var mins = minutes % Constants.TimeConversion.MinutesPerHour;
        return string.Format(Constants.TimeFormats.HoursMinutesFormat, hours, mins);
    }

    #endregion
}
