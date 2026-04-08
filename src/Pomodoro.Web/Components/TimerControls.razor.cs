using Microsoft.AspNetCore.Components;
using Pomodoro.Web.Models;

namespace Pomodoro.Web.Components;

/// <summary>
/// Code-behind for TimerControls component
/// Separates business logic from view
/// </summary>
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
    
    #endregion

    #region Computed Properties

    /// <summary>
    /// Determines if the Start button should be disabled
    /// </summary>
    protected bool IsStartDisabled => !CanStart;
    
    #endregion

    #region Business Logic Methods

    /// <summary>
    /// Gets the display label for the current session type
    /// </summary>
    protected string GetSessionLabel()
    {
        return SessionType switch
        {
            SessionType.Pomodoro => Constants.SessionTypes.PomodoroDisplayName,
            SessionType.ShortBreak => Constants.SessionTypes.ShortBreakDisplayName,
            SessionType.LongBreak => Constants.SessionTypes.LongBreakDisplayName,
            _ => string.Empty
        };
    }

    /// <summary>
    /// Gets the CSS class for buttons based on current session type
    /// Matches PIP window behavior for consistent styling
    /// </summary>
    protected string GetSessionClass()
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
