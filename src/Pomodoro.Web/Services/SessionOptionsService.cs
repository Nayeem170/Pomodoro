using Pomodoro.Web.Models;

namespace Pomodoro.Web.Services;

public interface ISessionOptionsService
{
    List<ConsentOption> GetOptionsForSessionType(SessionType sessionType);
    SessionType GetDefaultOption(SessionType completedSessionType);
}

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
                new() { SessionType = SessionType.ShortBreak, Label = Constants.SessionOptionLabels.ContinueShortBreak, Duration = string.Format(Constants.DurationFormats.MinutesFormat, settings.ShortBreakMinutes), IsDefault = true },
                new() { SessionType = SessionType.Pomodoro, Label = Constants.SessionOptionLabels.StartPomodoro, Duration = string.Format(Constants.DurationFormats.MinutesFormat, settings.PomodoroMinutes), IsDefault = false }
            },
            SessionType.LongBreak => new List<ConsentOption>
            {
                new() { SessionType = SessionType.LongBreak, Label = Constants.SessionOptionLabels.ContinueLongBreak, Duration = string.Format(Constants.DurationFormats.MinutesFormat, settings.LongBreakMinutes), IsDefault = true },
                new() { SessionType = SessionType.Pomodoro, Label = Constants.SessionOptionLabels.StartPomodoro, Duration = string.Format(Constants.DurationFormats.MinutesFormat, settings.PomodoroMinutes), IsDefault = false }
            },
            _ => new List<ConsentOption>()
        };
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
            SessionType.ShortBreak => SessionType.ShortBreak,
            SessionType.LongBreak => SessionType.LongBreak,
            _ => SessionType.Pomodoro
        };
    }
}
