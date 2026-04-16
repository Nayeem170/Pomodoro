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

    /// <summary>
    /// Pomodoro duration in minutes. Clamped to valid range.
    /// </summary>
    public int PomodoroMinutes
    {
        get => _pomodoroMinutes;
        set => _pomodoroMinutes = Math.Clamp(value, Constants.Timer.MinPomodoroMinutes, Constants.Timer.MaxPomodoroMinutes);
    }

    /// <summary>
    /// Short break duration in minutes. Clamped to valid range.
    /// </summary>
    public int ShortBreakMinutes
    {
        get => _shortBreakMinutes;
        set => _shortBreakMinutes = Math.Clamp(value, Constants.Timer.MinBreakMinutes, Constants.Timer.MaxBreakMinutes);
    }

    /// <summary>
    /// Long break duration in minutes. Clamped to valid range.
    /// </summary>
    public int LongBreakMinutes
    {
        get => _longBreakMinutes;
        set => _longBreakMinutes = Math.Clamp(value, Constants.Timer.MinBreakMinutes, Constants.Timer.MaxBreakMinutes);
    }

    /// <summary>
    /// Gets the duration in minutes for the specified session type.
    /// Centralizes session duration logic to reduce code duplication.
    /// </summary>
    /// <param name="sessionType">The type of session</param>
    /// <returns>Duration in minutes for the session type</returns>
    public int GetDurationMinutes(SessionType sessionType) => sessionType switch
    {
        SessionType.Pomodoro => PomodoroMinutes,
        SessionType.ShortBreak => ShortBreakMinutes,
        SessionType.LongBreak => LongBreakMinutes,
        _ => PomodoroMinutes
    };

    /// <summary>
    /// Gets the duration in seconds for the specified session type.
    /// Convenience method that converts minutes to seconds.
    /// </summary>
    /// <param name="sessionType">The type of session</param>
    /// <returns>Duration in seconds for the session type</returns>
    public int GetDurationSeconds(SessionType sessionType) =>
        GetDurationMinutes(sessionType) * Constants.TimeConversion.SecondsPerMinute;

    /// <summary>
    /// Whether sound notifications are enabled
    /// </summary>
    public bool SoundEnabled { get; set; } = true;

    /// <summary>
    /// Whether browser notifications are enabled
    /// </summary>
    public bool NotificationsEnabled { get; set; } = true;

    /// <summary>
    /// Whether to automatically start the next timer (pomodoro or break) after the current one completes
    /// </summary>
    public bool AutoStartEnabled { get; set; } = true;

    private int _autoStartDelaySeconds = Constants.Timer.DefaultAutoStartDelaySeconds;

    /// <summary>
    /// Delay in seconds before auto-starting. Clamped to valid range (3-60 seconds).
    /// </summary>
    public int AutoStartDelaySeconds
    {
        get => _autoStartDelaySeconds;
        set => _autoStartDelaySeconds = Math.Clamp(value, Constants.Timer.MinAutoStartDelaySeconds, Constants.Timer.MaxAutoStartDelaySeconds);
    }

    /// <summary>
    /// Compares this settings instance with another for equality
    /// </summary>
    public bool Equals(TimerSettings? other)
    {
        if (other is null) return false;
        return PomodoroMinutes == other.PomodoroMinutes
            && ShortBreakMinutes == other.ShortBreakMinutes
            && LongBreakMinutes == other.LongBreakMinutes
            && SoundEnabled == other.SoundEnabled
            && NotificationsEnabled == other.NotificationsEnabled
            && AutoStartEnabled == other.AutoStartEnabled
            && AutoStartDelaySeconds == other.AutoStartDelaySeconds;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as TimerSettings);

    /// <summary>
    /// Equality operator for comparing two TimerSettings instances
    /// </summary>
    public static bool operator ==(TimerSettings? left, TimerSettings? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    /// <summary>
    /// Inequality operator for comparing two TimerSettings instances
    /// </summary>
    public static bool operator !=(TimerSettings? left, TimerSettings? right)
    {
        return !(left == right);
    }

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(
        PomodoroMinutes,
        ShortBreakMinutes,
        LongBreakMinutes,
        SoundEnabled,
        NotificationsEnabled,
        AutoStartEnabled,
        AutoStartDelaySeconds);

    /// <summary>
    /// Creates a deep copy of this settings instance
    /// </summary>
    /// <returns>A new TimerSettings instance with the same values</returns>
    public TimerSettings Clone() => new()
    {
        PomodoroMinutes = PomodoroMinutes,
        ShortBreakMinutes = ShortBreakMinutes,
        LongBreakMinutes = LongBreakMinutes,
        SoundEnabled = SoundEnabled,
        NotificationsEnabled = NotificationsEnabled,
        AutoStartEnabled = AutoStartEnabled,
        AutoStartDelaySeconds = AutoStartDelaySeconds
    };
}
