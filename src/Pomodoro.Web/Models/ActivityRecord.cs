using System.Text.Json.Serialization;

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
    [JsonIgnore]
    public string DisplayText => Type switch
    {
        SessionType.Pomodoro => TaskName ?? Constants.SessionTypes.FocusTimeActivity,
        SessionType.ShortBreak => Constants.SessionTypes.ShortBreakActivity,
        SessionType.LongBreak => Constants.SessionTypes.LongBreakActivity,
        _ => Constants.SessionTypes.UnknownActivity
    };

    [JsonIgnore]
    public string Icon => Type switch
    {
        SessionType.Pomodoro => Constants.SessionTypes.PomodoroEmoji,
        SessionType.ShortBreak => Constants.SessionTypes.ShortBreakEmoji,
        SessionType.LongBreak => Constants.SessionTypes.LongBreakEmoji,
        _ => Constants.SessionTypes.TimerEmoji
    };


}
