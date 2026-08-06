using Pomodoro.Web.Models;

namespace Pomodoro.Web.Services.Formatters;

public class TimerThemeFormatter
{
    /// <param name="sessionType">The current session type</param>
    /// <returns>CSS class for the timer theme</returns>
    public string GetTimerThemeClass(SessionType sessionType)
    {
        return sessionType switch
        {
            SessionType.Pomodoro => Constants.SessionTypes.PomodoroTheme,
            SessionType.ShortBreak => Constants.SessionTypes.ShortBreakTheme,
            SessionType.LongBreak => Constants.SessionTypes.LongBreakTheme,
            _ => Constants.SessionTypes.PomodoroTheme
        };
    }
}
