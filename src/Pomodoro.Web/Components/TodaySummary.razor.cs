using Microsoft.AspNetCore.Components;

namespace Pomodoro.Web.Components;

/// <summary>
/// Code-behind for TodaySummary component
/// Separates business logic from view
/// </summary>
public class TodaySummaryBase : ComponentBase
{
    #region Parameters (Model)
    
    [Parameter]
    public int TotalFocusMinutes { get; set; }

    [Parameter]
    public int PomodoroCount { get; set; }

    [Parameter]
    public int TasksWorkedOn { get; set; }
    
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
    
    #endregion
}
