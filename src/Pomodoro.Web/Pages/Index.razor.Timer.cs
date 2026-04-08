using Microsoft.AspNetCore.Components;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services.Formatters;

namespace Pomodoro.Web.Pages;

/// <summary>
/// Timer actions partial for Index page
/// Contains all timer-related event handlers and theme logic
/// </summary>
public partial class IndexBase
{
    [Inject] protected TimerThemeFormatter TimerThemeFormatter { get; set; } = default!;
    
    #region Timer Actions
 
    /// <summary>
    /// Handles starting of timer based on current session type
    /// </summary>
    public async Task HandleTimerStart()
    {
        try
        {
            // Start timer based on current session type
            switch (CurrentSessionType)
            {
                case SessionType.Pomodoro:
                    // Use TaskService.CurrentTaskId directly to avoid stale local copy
                    if (!TaskService.CurrentTaskId.HasValue)
                    {
                        ErrorMessage = Constants.Messages.SelectTaskBeforePomodoro;
                        StateHasChanged();
                        return;
                    }
                    await TimerService.StartPomodoroAsync(TaskService.CurrentTaskId.Value);
                    break;
                case SessionType.ShortBreak:
                    await TimerService.StartShortBreakAsync();
                    break;
                case SessionType.LongBreak:
                    await TimerService.StartLongBreakAsync();
                    break;
            }
            UpdateState();
            StateHasChanged();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"{Constants.Messages.ErrorStartingTimer}: {ex.Message}";
        }
    } 
 
    /// <summary>
    /// Handles pausing of timer
    /// </summary>
    public async Task HandleTimerPause()
    {
        try
        {
            await TimerService.PauseAsync();
            UpdateState();
            StateHasChanged();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"{Constants.Messages.ErrorPausingTimer}: {ex.Message}";
        }
    }
 
    /// <summary>
    /// Handles resuming of timer
    /// </summary>
    public async Task HandleTimerResume()
    {
        try
        {
            await TimerService.ResumeAsync();
            UpdateState();
            StateHasChanged();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"{Constants.Messages.ErrorResumingTimer}: {ex.Message}";
        }
    }
 
    /// <summary>
    /// Handles resetting of timer
    /// </summary>
    public async Task HandleTimerReset()
    {
        try
        {
            await TimerService.ResetAsync();
            UpdateState();
            StateHasChanged();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"{Constants.Messages.ErrorResettingTimer}: {ex.Message}";
        }
    }
 
    /// <summary>
    /// Handles switching to a different session type
    /// </summary>
    public async Task HandleSessionSwitch(SessionType sessionType)
    {
        try
        {
            await TimerService.SwitchSessionTypeAsync(sessionType);
            UpdateState();
            StateHasChanged();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"{Constants.Messages.ErrorSwitchingSession}: {ex.Message}";
        }
    }
 
    /// <summary>
    /// Handles toggling of Picture-in-Picture timer window
    /// </summary>
    public async Task HandleTogglePip()
    {
        try
        {
            if (PipTimerService.IsOpen)
            {
                await PipTimerService.CloseAsync();
                IsPipOpen = false;
            }
            else
            {
                var success = await PipTimerService.OpenAsync();
                IsPipOpen = success;
            }
            StateHasChanged();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"{Constants.Messages.ErrorTogglingFloatingTimer}: {ex.Message}";
        }
    }
 
    #endregion
 
    #region Timer Theme
 
    /// <summary>
    /// Gets the CSS class for the current timer theme based on session type
    /// </summary>
    public string GetTimerThemeClass()
    {
        return TimerThemeFormatter.GetTimerThemeClass(CurrentSessionType);
    }
 
    #endregion
}
