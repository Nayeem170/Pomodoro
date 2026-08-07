namespace Pomodoro.Web.Models;

/// <summary>
/// Configuration settings for timer durations and preferences
/// Values are automatically clamped to valid ranges on set.
/// </summary>
public class TimerSettings
{
    private int _pomodoroMinutes = Constants.Timer.DefaultPomodoroMinutes;
    private int _shortBreakMinutes = Constants.Timer.DefaultShortBreakMinutes;
    private int _longBreakMinutes = Constants.Timer.DefaultLongBreakMinutes;
    private int _dailyGoal = Constants.Timer.DefaultDailyGoal;
    private int _longBreakInterval = Constants.Timer.DefaultLongBreakInterval;

    public int PomodoroMinutes
    {
        get => _pomodoroMinutes;
        set => _pomodoroMinutes = Math.Clamp(value, Constants.Timer.MinPomodoroMinutes, Constants.Timer.MaxPomodoroMinutes);
    }

    public int ShortBreakMinutes
    {
        get => _shortBreakMinutes;
        set => _shortBreakMinutes = Math.Clamp(value, Constants.Timer.MinBreakMinutes, Constants.Timer.MaxBreakMinutes);
    }

    public int LongBreakMinutes
    {
        get => _longBreakMinutes;
        set => _longBreakMinutes = Math.Clamp(value, Constants.Timer.MinBreakMinutes, Constants.Timer.MaxBreakMinutes);
    }

    public int DailyGoal
    {
        get => _dailyGoal;
        set => _dailyGoal = Math.Clamp(value, Constants.Timer.MinDailyGoal, Constants.Timer.MaxDailyGoal);
    }

    public int LongBreakInterval
    {
        get => _longBreakInterval;
        set => _longBreakInterval = Math.Clamp(value, Constants.Timer.MinLongBreakInterval, Constants.Timer.MaxLongBreakInterval);
    }

    /// <param name="sessionType">The type of session</param>
    /// <returns>Duration in minutes for the session type</returns>
    public int GetDurationMinutes(SessionType sessionType) => sessionType switch
    {
        SessionType.Pomodoro => PomodoroMinutes,
        SessionType.ShortBreak => ShortBreakMinutes,
        SessionType.LongBreak => LongBreakMinutes,
        _ => PomodoroMinutes
    };

    /// <param name="sessionType">The type of session</param>
    /// <returns>Duration in seconds for the session type</returns>
    public int GetDurationSeconds(SessionType sessionType) =>
        GetDurationMinutes(sessionType) * Constants.TimeConversion.SecondsPerMinute;

    public bool SoundEnabled { get; set; } = true;

    public bool NotificationsEnabled { get; set; } = true;

    public bool AutoStartSession { get; set; } = true;

    public bool ExpandTimerMobile { get; set; }

    public bool RecordPartialSessions { get; set; }

    private int _autoStartDelaySeconds = Constants.Timer.DefaultAutoStartDelaySeconds;

    public int AutoStartDelaySeconds
    {
        get => _autoStartDelaySeconds;
        set => _autoStartDelaySeconds = Math.Clamp(value, Constants.Timer.MinAutoStartDelaySeconds, Constants.Timer.MaxAutoStartDelaySeconds);
    }

    public bool Equals(TimerSettings? other)
    {
        if (other is null) return false;
        return PomodoroMinutes == other.PomodoroMinutes
            && ShortBreakMinutes == other.ShortBreakMinutes
            && LongBreakMinutes == other.LongBreakMinutes
            && DailyGoal == other.DailyGoal
            && LongBreakInterval == other.LongBreakInterval
            && SoundEnabled == other.SoundEnabled
            && NotificationsEnabled == other.NotificationsEnabled
            && AutoStartSession == other.AutoStartSession
            && AutoStartDelaySeconds == other.AutoStartDelaySeconds
            && ExpandTimerMobile == other.ExpandTimerMobile
            && RecordPartialSessions == other.RecordPartialSessions;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as TimerSettings);

    public static bool operator ==(TimerSettings? left, TimerSettings? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(TimerSettings? left, TimerSettings? right)
    {
        return !(left == right);
    }

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(
        PomodoroMinutes,
        ShortBreakMinutes,
        LongBreakMinutes,
        DailyGoal,
        LongBreakInterval,
        HashCode.Combine(SoundEnabled, NotificationsEnabled, AutoStartSession, AutoStartDelaySeconds, ExpandTimerMobile, RecordPartialSessions));

    /// <returns>A new TimerSettings instance with the same values</returns>
    public TimerSettings Clone() => new()
    {
        PomodoroMinutes = PomodoroMinutes,
        ShortBreakMinutes = ShortBreakMinutes,
        LongBreakMinutes = LongBreakMinutes,
        DailyGoal = DailyGoal,
        LongBreakInterval = LongBreakInterval,
        SoundEnabled = SoundEnabled,
        NotificationsEnabled = NotificationsEnabled,
        AutoStartSession = AutoStartSession,
        AutoStartDelaySeconds = AutoStartDelaySeconds,
        ExpandTimerMobile = ExpandTimerMobile,
        RecordPartialSessions = RecordPartialSessions
    };
}
