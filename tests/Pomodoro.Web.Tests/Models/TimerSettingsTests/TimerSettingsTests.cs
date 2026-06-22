using Pomodoro.Web.Models;

namespace Pomodoro.Web.Tests.Models.TimerSettingsTests;

using Xunit;
[Trait("Category", "Model")]
public partial class TimerSettingsTests
{
    protected static TimerSettings CreateDefaultSettings() => new();

    protected static TimerSettings CreateCustomSettings(
        int pomodoroMinutes = 30,
        int shortBreakMinutes = 10,
        int longBreakMinutes = 20,
        bool soundEnabled = true,
        bool notificationsEnabled = true,
        bool autoStartSession = true,
        int autoStartDelaySeconds = 15)
    {
        return new TimerSettings
        {
            PomodoroMinutes = pomodoroMinutes,
            ShortBreakMinutes = shortBreakMinutes,
            LongBreakMinutes = longBreakMinutes,
            SoundEnabled = soundEnabled,
            NotificationsEnabled = notificationsEnabled,
            AutoStartSession = autoStartSession,
            AutoStartDelaySeconds = autoStartDelaySeconds
        };
    }
}
