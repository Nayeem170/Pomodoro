using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;

namespace Pomodoro.Web.Components;

/// <summary>
/// Code-behind for TimerDisplay component
/// Reads directly from TimerService for real-time updates
/// </summary>
public class TimerDisplayBase : ComponentBase, IDisposable
{
    #region Dependencies
    
    [Inject]
    protected ITimerService TimerService { get; set; } = default!;

    [Inject]
    protected ILogger<TimerDisplayBase> Logger { get; set; } = default!;
    
    #endregion

    #region Parameters (for initial/override values only)
    
    [Parameter]
    public TimeSpan? RemainingTime { get; set; }

    [Parameter]
    public SessionType? SessionType { get; set; }

    [Parameter]
    public bool? IsRunning { get; set; }
    
    #endregion

    #region Private Fields
    
    #endregion

    #region Properties - Always read from service for real-time updates
    
    protected TimeSpan CurrentRemainingTime => TimerService.RemainingTime;
    protected SessionType CurrentSessionType => TimerService.CurrentSessionType;
    protected bool CurrentIsRunning => TimerService.IsRunning;
    
    #endregion

    #region Lifecycle Methods
    
    protected override void OnInitialized()
    {
        // Subscribe to timer service events
        TimerService.OnTick += OnTimerTick;
        TimerService.OnStateChanged += OnTimerStateChanged;
    }
    
    protected virtual void UpdateDisplay()
    {
        StateHasChanged();
    }

    protected virtual void HandleStateChangeError() { }
    
    private async void OnTimerTick()
    {
        try
        {
            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in OnTimerTick");
        }
    }
    
    private async void OnTimerStateChanged()
    {
        try
        {
            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in OnTimerStateChanged");
        }
    }
    
    public void Dispose()
    {
        TimerService.OnTick -= OnTimerTick;
        TimerService.OnStateChanged -= OnTimerStateChanged;
    }
    
    #endregion

    #region Business Logic Methods

    /// <summary>
    /// Formats the remaining time as MM:SS
    /// </summary>
    protected string FormatTime(TimeSpan time)
    {
        return string.Format(Constants.TimeFormats.TimerFormat, (int)time.TotalMinutes, time.Seconds);
    }

    /// <summary>
    /// Gets the display label for the current session type
    /// </summary>
    protected string GetSessionTypeLabel()
    {
        var sessionType = CurrentSessionType;
        return sessionType switch
        {
            Models.SessionType.Pomodoro => Constants.SessionTypes.PomodoroUppercase,
            Models.SessionType.ShortBreak => Constants.SessionTypes.ShortBreakUppercase,
            Models.SessionType.LongBreak => Constants.SessionTypes.LongBreakUppercase,
            _ => Constants.SessionTypes.PomodoroUppercase
        };
    }

    /// <summary>
    /// Gets the CSS class based on timer state and session type
    /// Returns both session class and paused state to allow session color with reduced opacity
    /// </summary>
    protected string GetTimerClass()
    {
        // Get session class (matches PIP window behavior)
        var sessionClass = CurrentSessionType switch
        {
            Models.SessionType.Pomodoro => Constants.SessionTypes.PomodoroClass,
            Models.SessionType.ShortBreak => Constants.SessionTypes.ShortBreakClass,
            Models.SessionType.LongBreak => Constants.SessionTypes.LongBreakClass,
            _ => Constants.SessionTypes.PomodoroClass
        };
        
        // Add paused class if not running (allows session color + reduced opacity)
        if (!CurrentIsRunning)
        {
            return $"{sessionClass} {Constants.SessionTypes.PausedState}";
        }
        
        return sessionClass;
    }
    
    #endregion
}
