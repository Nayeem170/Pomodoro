using Pomodoro.Web.Models;

namespace Pomodoro.Web.Tests.Models.TimerSettingsTests;

/// <summary>
/// Base test class for TimerSettings tests.
/// TimerSettings is a pure model class with no dependencies.
/// </summary>
using Xunit;
[Trait("Category", "Model")]
public partial class TimerSettingsTests
{
    /// <summary>
    /// Creates a new TimerSettings instance with default values.
    /// </summary>
    protected static TimerSettings CreateDefaultSettings() => new();

    /// <summary>
    /// Creates a TimerSettings instance with custom values.
    /// </summary>
    protected static TimerSettings CreateCustomSettings(
        int pomodoroMinutes = 30,
        int shortBreakMinutes = 10,
        int longBreakMinutes = 20,
        bool soundEnabled = true,
        bool notificationsEnabled = true,
        bool autoStartPomodoros = true,
        bool autoStartBreaks = true,
        int autoStartDelaySeconds = 15)
    {
        return new TimerSettings
        {
            PomodoroMinutes = pomodoroMinutes,
            ShortBreakMinutes = shortBreakMinutes,
            LongBreakMinutes = longBreakMinutes,
            SoundEnabled = soundEnabled,
            NotificationsEnabled = notificationsEnabled,
            AutoStartPomodoros = autoStartPomodoros,
            AutoStartBreaks = autoStartBreaks,
            AutoStartDelaySeconds = autoStartDelaySeconds
        };
    }
}

