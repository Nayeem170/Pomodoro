namespace Pomodoro.Web.Models;

/// <summary>
/// Summary statistics for a day's activities
/// </summary>
public class DailyStatsSummary
{
    public int PomodoroCount { get; set; }
    public int FocusMinutes { get; set; }
    public int TasksWorkedOn { get; set; }
    
    /// <summary>
    /// Formats focus minutes into human-readable format
    /// </summary>
    public string FormattedFocusTime
    {
        get
        {
            if (FocusMinutes < Constants.TimeConversion.MinutesPerHour)
                return string.Format(Constants.TimeFormats.MinutesFormat, FocusMinutes);
            var hours = FocusMinutes / Constants.TimeConversion.MinutesPerHour;
            var mins = FocusMinutes % Constants.TimeConversion.MinutesPerHour;
            return string.Format(Constants.TimeFormats.HoursMinutesFormat, hours, mins);
        }
    }
}
