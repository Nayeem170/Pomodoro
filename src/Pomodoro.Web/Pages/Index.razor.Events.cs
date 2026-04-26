using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;

namespace Pomodoro.Web.Pages;

/// <summary>
/// Event handlers partial for Index page
/// Contains all service event subscription handlers
/// </summary>
public partial class IndexBase
{
    #region Safe Async Helper

    /// <summary>
    /// Safely executes an async operation from event handlers.
    /// Prevents unhandled exceptions from crashing the application.
    /// </summary>
    public void SafeAsync(Func<Task> action, string handlerName)
    {
        _ = SafeAsyncInternal(action, handlerName);
    }

    public async Task SafeAsyncInternal(Func<Task> action, string handlerName)
    {
        try
        {
            await action();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, Constants.Messages.LogHandlerErrorFormat, handlerName);
        }
    }

    #endregion

    #region Task Service Events

    public void OnTaskServiceChanged()
    {
        SafeAsync(async () =>
        {
            UpdateState();
            await InvokeAsync(StateHasChanged);
        }, nameof(OnTaskServiceChanged));
    }

    #endregion

    #region Timer Service Events

    public Task OnTimerCompleted(TimerCompletedEventArgs args)
    {
        try
        {
            UpdateState();
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, Constants.Messages.ErrorInOnTimerComplete);
        }

        return Task.CompletedTask;
    }

    public void OnTimerStateChanged()
    {
        SafeAsync(async () =>
        {
            UpdateState();
            await InvokeAsync(StateHasChanged);
        }, nameof(OnTimerStateChanged));
    }

    #endregion

    #region Notification Events

    public void OnNotificationAction(string action)
    {
        SafeAsync(async () =>
        {
            // Handle notification action clicks from browser notifications
            switch (action)
            {
                case Constants.SessionTypes.ActionShortBreak:
                    // Start short break timer
                    ConsentService.HideConsentModal();
                    await TimerService.StartShortBreakAsync();
                    break;
                case Constants.SessionTypes.ActionLongBreak:
                    // Start long break timer
                    ConsentService.HideConsentModal();
                    await TimerService.StartLongBreakAsync();
                    break;
                case Constants.SessionTypes.ActionStartPomodoro:
                    // Start pomodoro with current task
                    ConsentService.HideConsentModal();
                    await TimerService.StartPomodoroAsync(AppState.CurrentTaskId);
                    break;
                case Constants.SessionTypes.ActionSkip:
                    // Do nothing - consent modal stays open, preplanned timer countdown continues
                    // The auto-start will happen when countdown reaches zero
                    break;
            }
            UpdateState();
            await InvokeAsync(StateHasChanged);
        }, nameof(OnNotificationAction));
    }

    #endregion

    #region Consent Service Events

    public void OnConsentRequired()
    {
        SafeAsync(async () =>
        {
            IsConsentModalVisible = ConsentService.IsModalVisible;
            ConsentCountdown = ConsentService.CountdownSeconds;
            ConsentOptions = ConsentService.AvailableOptions;
            await InvokeAsync(StateHasChanged);
        }, nameof(OnConsentRequired));
    }

    public void OnConsentCountdownTick()
    {
        SafeAsync(async () =>
        {
            ConsentCountdown = ConsentService.CountdownSeconds;
            await InvokeAsync(StateHasChanged);
        }, nameof(OnConsentCountdownTick));
    }

    public void OnConsentHandled()
    {
        SafeAsync(async () =>
        {
            IsConsentModalVisible = false;
            UpdateState();
            await InvokeAsync(StateHasChanged);
        }, nameof(OnConsentHandled));
    }

    #endregion

    #region Activity Service Events

    public void OnActivityChanged()
    {
        SafeAsync(async () =>
        {
            InvalidateTodayStatsCache();
            await InvokeAsync(StateHasChanged);
        }, nameof(OnActivityChanged));
    }

    #endregion

    #region PiP Timer Events

    public void OnPipOpened()
    {
        SafeAsync(async () =>
        {
            IsPipOpen = true;
            await InvokeAsync(StateHasChanged);
        }, nameof(OnPipOpened));
    }

    public void OnPipClosed()
    {
        SafeAsync(async () =>
        {
            IsPipOpen = false;
            await InvokeAsync(StateHasChanged);
        }, nameof(OnPipClosed));
    }

    #endregion
}
