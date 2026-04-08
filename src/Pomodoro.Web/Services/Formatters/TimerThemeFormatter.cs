using Pomodoro.Web.Models;

namespace Pomodoro.Web.Services.Formatters;

/// <summary>
/// Service for formatting timer theme CSS classes based on session type
/// </summary>
public class TimerThemeFormatter
{
    /// <summary>
    /// Gets the CSS class for the current timer theme based on session type
    /// </summary>
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
