namespace Pomodoro.Web.Models;

/// <summary>
/// Represents a completed pomodoro or break session for activity history
/// </summary>
public class ActivityRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public SessionType Type { get; set; }
    public string? TaskName { get; set; }
    public Guid? TaskId { get; set; }
    public DateTime CompletedAt { get; set; }
    public int DurationMinutes { get; set; }
    public bool WasCompleted { get; set; }
    
    // For display purposes
    public string DisplayText => Type switch
    {
        SessionType.Pomodoro => TaskName ?? Constants.SessionTypes.FocusTimeActivity,
        SessionType.ShortBreak => Constants.SessionTypes.ShortBreakActivity,
        SessionType.LongBreak => Constants.SessionTypes.LongBreakActivity,
        _ => Constants.SessionTypes.UnknownActivity
    };
    
    public string Icon => Type switch
    {
        SessionType.Pomodoro => Constants.SessionTypes.PomodoroEmoji,
        SessionType.ShortBreak => Constants.SessionTypes.ShortBreakEmoji,
        SessionType.LongBreak => Constants.SessionTypes.LongBreakEmoji,
        _ => Constants.SessionTypes.TimerEmoji
    };
    
    public string TimeAgo => GetTimeAgo(CompletedAt);
    
    private static string GetTimeAgo(DateTime completedAt)
    {
        var diff = DateTime.UtcNow - completedAt;
        
        if (diff.TotalMinutes < Constants.TimeThresholds.OneMinute)
            return Constants.TimeFormats.JustNowText;
        if (diff.TotalMinutes < Constants.TimeThresholds.OneHourInMinutes)
            return string.Format(Constants.TimeFormats.MinutesAgoFormat, (int)diff.TotalMinutes);
        if (diff.TotalHours < Constants.TimeThresholds.OneDayInHours)
            return string.Format(Constants.TimeFormats.HoursAgoFormat, (int)diff.TotalHours);
        if (diff.TotalDays < Constants.TimeThresholds.OneWeekInDays)
            return string.Format(Constants.TimeFormats.DaysAgoFormat, (int)diff.TotalDays);
        
        return completedAt.ToString(Constants.TimeFormats.ShortDateFormat);
    }
}
