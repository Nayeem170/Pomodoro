using Pomodoro.Web.Models;

namespace Pomodoro.Web.Services;

/// <summary>
/// Interface for session options generation
/// </summary>
public interface ISessionOptionsService
{
    /// <summary>
    /// Gets the available options for the completed session type
    /// </summary>
    List<ConsentOption> GetOptionsForSessionType(SessionType sessionType);
    
    /// <summary>
    /// Gets the default option for the completed session type
    /// </summary>
    SessionType GetDefaultOption(SessionType completedSessionType);
}

/// <summary>
/// Service for generating session options based on completed session type
/// Separated from ConsentService for better single responsibility
/// </summary>
public class SessionOptionsService : ISessionOptionsService
{
    private readonly AppState _appState;

    public SessionOptionsService(AppState appState)
    {
        _appState = appState;
    }

    public List<ConsentOption> GetOptionsForSessionType(SessionType sessionType)
    {
        var settings = _appState.Settings;
        
        return sessionType switch
        {
            SessionType.Pomodoro => new List<ConsentOption>
            {
                new() { SessionType = SessionType.ShortBreak, Label = Constants.SessionOptionLabels.ShortBreak, Duration = string.Format(Constants.DurationFormats.MinutesFormat, settings.ShortBreakMinutes), IsDefault = false },
                new() { SessionType = SessionType.LongBreak, Label = Constants.SessionOptionLabels.LongBreak, Duration = string.Format(Constants.DurationFormats.MinutesFormat, settings.LongBreakMinutes), IsDefault = false },
                new() { SessionType = SessionType.Pomodoro, Label = Constants.SessionOptionLabels.AnotherPomodoro, Duration = string.Format(Constants.DurationFormats.MinutesFormat, settings.PomodoroMinutes), IsDefault = true }
            },
            SessionType.ShortBreak => new List<ConsentOption>
            {
                new() { SessionType = SessionType.Pomodoro, Label = Constants.SessionOptionLabels.StartPomodoro, Duration = string.Format(Constants.DurationFormats.MinutesFormat, settings.PomodoroMinutes), IsDefault = false },
                new() { SessionType = SessionType.ShortBreak, Label = Constants.SessionOptionLabels.ShortBreak, Duration = string.Format(Constants.DurationFormats.MinutesFormat, settings.ShortBreakMinutes), IsDefault = true }
            },
            SessionType.LongBreak => new List<ConsentOption>
            {
                new() { SessionType = SessionType.Pomodoro, Label = Constants.SessionOptionLabels.StartPomodoro, Duration = string.Format(Constants.DurationFormats.MinutesFormat, settings.PomodoroMinutes), IsDefault = false },
                new() { SessionType = SessionType.LongBreak, Label = Constants.SessionOptionLabels.LongBreak, Duration = string.Format(Constants.DurationFormats.MinutesFormat, settings.LongBreakMinutes), IsDefault = true }
            },
            _ => new List<ConsentOption>()
        };
    }

    public SessionType GetDefaultOption(SessionType completedSessionType)
    {
        return completedSessionType switch
        {
            SessionType.Pomodoro => SessionType.Pomodoro, // Continue with another pomodoro
            SessionType.ShortBreak => SessionType.ShortBreak, // Continue short break
            SessionType.LongBreak => SessionType.LongBreak, // Continue long break
            _ => SessionType.Pomodoro
        };
    }
}
