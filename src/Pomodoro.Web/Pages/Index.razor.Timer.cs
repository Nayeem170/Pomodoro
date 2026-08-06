using Microsoft.AspNetCore.Components;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services.Formatters;

namespace Pomodoro.Web.Pages;

public partial class IndexBase
{
    [Inject] protected TimerThemeFormatter TimerThemeFormatter { get; set; } = default!;

    #region Timer Actions

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
            await UpdateStateAsync();
            StateHasChanged();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"{Constants.Messages.ErrorStartingTimer}: {ex.Message}";
        }
    }

    public async Task HandleTimerPause()
    {
        try
        {
            await TimerService.PauseAsync();
            await UpdateStateAsync();
            StateHasChanged();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"{Constants.Messages.ErrorPausingTimer}: {ex.Message}";
        }
    }

    public async Task HandleTimerResume()
    {
        try
        {
            await TimerService.ResumeAsync();
            await UpdateStateAsync();
            StateHasChanged();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"{Constants.Messages.ErrorResumingTimer}: {ex.Message}";
        }
    }

    public async Task HandleTimerReset()
    {
        try
        {
            await TimerService.ResetAsync();
            await UpdateStateAsync();
            StateHasChanged();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"{Constants.Messages.ErrorResettingTimer}: {ex.Message}";
        }
    }

    public async Task HandleSessionSwitch(SessionType sessionType)
    {
        try
        {
            await TimerService.SwitchSessionTypeAsync(sessionType);
            await UpdateStateAsync();
            StateHasChanged();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"{Constants.Messages.ErrorSwitchingSession}: {ex.Message}";
        }
    }

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
                if (!success)
                {
                    ErrorMessage = Constants.Messages.PipPopupBlocked;
                }
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

    public string GetTimerThemeClass()
    {
        return TimerThemeFormatter.GetTimerThemeClass(CurrentSessionType);
    }

    #endregion
}
