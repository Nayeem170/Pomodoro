using Pomodoro.Web.Models;

namespace Pomodoro.Web.Services;

/// <summary>
/// Interface for session options generation
/// </summary>
public interface ISessionOptionsService
{
    List<ConsentOption> GetOptionsForSessionType(SessionType sessionType, TimerSession? interruptedPomodoro);
    SessionType GetDefaultOption(SessionType completedSessionType);
}

/// <summary>
/// Service for generating session options based on completed session type
/// Separated from ConsentService for better single responsibility
/// </summary>
public class SessionOptionsService : ISessionOptionsService
{
    private readonly AppState _appState;
    private readonly ITimerService _timerService;

    public SessionOptionsService(AppState appState, ITimerService timerService)
    {
        _appState = appState;
        _timerService = timerService;
    }

    public List<ConsentOption> GetOptionsForSessionType(SessionType sessionType, TimerSession? interruptedPomodoro = null)
    {
        var settings = _appState.Settings;
        var options = sessionType switch
        {
            SessionType.Pomodoro => new List<ConsentOption>
            {
                new() { SessionType = SessionType.ShortBreak, Label = Constants.SessionOptionLabels.ShortBreak, Duration = string.Format(Constants.DurationFormats.MinutesFormat, settings.ShortBreakMinutes), IsDefault = false },
                new() { SessionType = SessionType.LongBreak, Label = Constants.SessionOptionLabels.LongBreak, Duration = string.Format(Constants.DurationFormats.MinutesFormat, settings.LongBreakMinutes), IsDefault = false },
                new() { SessionType = SessionType.Pomodoro, Label = Constants.SessionOptionLabels.AnotherPomodoro, Duration = string.Format(Constants.DurationFormats.MinutesFormat, settings.PomodoroMinutes), IsDefault = true }
            },
            SessionType.ShortBreak => new List<ConsentOption>
            {
                new() { SessionType = SessionType.Pomodoro, Label = Constants.SessionOptionLabels.StartPomodoro, Duration = string.Format(Constants.DurationFormats.MinutesFormat, settings.PomodoroMinutes), IsDefault = true }
            },
            SessionType.LongBreak => new List<ConsentOption>
            {
                new() { SessionType = SessionType.Pomodoro, Label = Constants.SessionOptionLabels.StartPomodoro, Duration = string.Format(Constants.DurationFormats.MinutesFormat, settings.PomodoroMinutes), IsDefault = true }
            },
            _ => new List<ConsentOption>()
        };

        if (sessionType != SessionType.Pomodoro && interruptedPomodoro != null)
        {
            var remainingMin = interruptedPomodoro.RemainingSeconds / 60;
            var sec = interruptedPomodoro.RemainingSeconds % 60;
            var duration = sec > 0 ? $"{remainingMin}:{sec:D2} left" : $"{remainingMin}m left";

            options.Insert(0, new ConsentOption
            {
                SessionType = SessionType.Pomodoro,
                Label = Constants.SessionOptionLabels.ResumePomodoro,
                Duration = duration,
                IsDefault = true,
                IsResume = true
            });

            foreach (var opt in options.Where(o => !o.IsResume))
            {
                opt.IsDefault = false;
            }
        }

        return options;
    }

    public SessionType GetDefaultOption(SessionType completedSessionType)
    {
        if (completedSessionType == SessionType.Pomodoro)
        {
            var interval = _appState.Settings.LongBreakInterval;
            var count = _appState.TodayPomodoroCount;
            return count > 0 && count % interval == 0
                ? SessionType.LongBreak
                : SessionType.Pomodoro;
        }

        return completedSessionType switch
        {
            SessionType.ShortBreak => SessionType.Pomodoro,
            SessionType.LongBreak => SessionType.Pomodoro,
            _ => SessionType.Pomodoro
        };
    }
}
