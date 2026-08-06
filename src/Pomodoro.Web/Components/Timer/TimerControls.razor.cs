using Microsoft.AspNetCore.Components;
using Pomodoro.Web.Models;

namespace Pomodoro.Web.Components.Timer;

public class TimerControlsBase : ComponentBase
{
    #region Parameters (Model)

    [Parameter]
    public bool IsRunning { get; set; }

    [Parameter]
    public bool IsPaused { get; set; }

    [Parameter]
    public bool IsStarted { get; set; }

    [Parameter]
    public bool CanStart { get; set; }

    [Parameter]
    public SessionType SessionType { get; set; }

    [Parameter]
    public EventCallback OnStart { get; set; }

    [Parameter]
    public EventCallback OnPause { get; set; }

    [Parameter]
    public EventCallback OnResume { get; set; }

    [Parameter]
    public EventCallback OnReset { get; set; }

    [Parameter]
    public RenderFragment? TrailingAction { get; set; }

    #endregion

    #region Computed Properties

    protected bool IsStartDisabled => !CanStart;

    #endregion

    #region Business Logic Methods

    internal string GetSessionLabel()
    {
        return SessionType switch
        {
            SessionType.Pomodoro => Constants.SessionTypes.PomodoroDisplayName,
            SessionType.ShortBreak => Constants.SessionTypes.ShortBreakDisplayName,
            SessionType.LongBreak => Constants.SessionTypes.LongBreakDisplayName,
            _ => string.Empty
        };
    }

    internal string GetSessionClass()
    {
        return SessionType switch
        {
            SessionType.Pomodoro => Constants.SessionTypes.PomodoroClass,
            SessionType.ShortBreak => Constants.SessionTypes.ShortBreakClass,
            SessionType.LongBreak => Constants.SessionTypes.LongBreakClass,
            _ => Constants.SessionTypes.PomodoroClass
        };
    }

    #endregion
}
