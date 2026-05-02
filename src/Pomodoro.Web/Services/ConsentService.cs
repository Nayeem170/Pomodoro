using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Pomodoro.Web.Services.Repositories;

namespace Pomodoro.Web.Services;

/// <summary>
/// Service for managing the consent modal after timer completion.
/// Uses PeriodicTimer for thread-safe async countdown handling.
/// Dependencies are injected via constructor for explicit dependency declaration.
/// </summary>
public class ConsentService : IConsentService, ITimerEventSubscriber, IAsyncDisposable
{
    private readonly ISessionOptionsService _sessionOptionsService;
    private readonly ILogger<ConsentService> _logger;
    private bool _isDisposed;
    private bool _isInitialized;
    private readonly object _initLock = new();

    // PeriodicTimer-based countdown for thread-safe async handling
    private PeriodicTimer? _countdownTimer;
    private CancellationTokenSource? _countdownCts;
    private readonly object _timerLock = new();

    private readonly ITimerService _timerService;
    private readonly ITaskService _taskService;
    private readonly INotificationService _notificationService;
    private readonly AppState _appState;

    public event Action? OnConsentRequired;
    public event Action? OnCountdownTick;
    public event Action? OnConsentHandled;

    public bool IsModalVisible { get; private set; }
    public SessionType CompletedSessionType { get; private set; }
    public int CountdownSeconds { get; private set; }
    public List<ConsentOption> AvailableOptions { get; private set; } = new();

    public ConsentService(
        ITimerService timerService,
        ITaskService taskService,
        INotificationService notificationService,
        AppState appState,
        ISessionOptionsService sessionOptionsService,
        ILogger<ConsentService> logger)
    {
        _timerService = timerService;
        _taskService = taskService;
        _notificationService = notificationService;
        _appState = appState;
        _sessionOptionsService = sessionOptionsService;
        _logger = logger;
    }

    /// <summary>
    /// Initializes the service - called after all services are created
    /// Uses lock to prevent duplicate event subscriptions from multiple Initialize calls
    /// </summary>
    public void Initialize()
    {
        lock (_initLock)
        {
            if (_isInitialized) return;
            _isInitialized = true;
        }
    }

    public async Task HandleTimerCompletedAsync(TimerCompletedEventArgs args)
    {
        try
        {
            await PlayCompletionSoundAndNotifyAsync(args.SessionType);

            var settings = _appState?.Settings;

            var shouldAutoStart = settings?.AutoStartSession == true;

            if (shouldAutoStart)
            {
                ShowConsentModal(args.SessionType);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Constants.Messages.LogConsentHandleCompleteError);
        }
    }

    /// <summary>
    /// Plays completion sound and shows notification based on settings
    /// </summary>
    private async Task PlayCompletionSoundAndNotifyAsync(SessionType sessionType)
    {
        if (!TryGetNotificationSettings(out var settings)) return;

        if (settings.SoundEnabled)
        {
            await PlaySoundAsync(sessionType);
        }

        if (settings.NotificationsEnabled)
        {
            await ShowNotificationAsync(sessionType);
        }
    }

    private bool TryGetNotificationSettings([NotNullWhen(true)] out TimerSettings? settings)
    {
        settings = _appState?.Settings;
        return _notificationService != null && settings != null;
    }

    private async Task PlaySoundAsync(SessionType sessionType)
    {
        if (sessionType == SessionType.Pomodoro)
        {
            await _notificationService.PlayTimerCompleteSoundAsync();
        }
        else
        {
            await _notificationService.PlayBreakCompleteSoundAsync();
        }
    }

    private async Task ShowNotificationAsync(SessionType sessionType)
    {
        if (sessionType == SessionType.Pomodoro)
        {
            await _notificationService.ShowNotificationAsync(
                $"{Constants.SessionTypes.PomodoroEmoji}{Constants.Formatting.EmojiTitleSeparator}{Constants.Messages.PomodoroNotificationTitle}",
                Constants.Messages.PomodoroNotificationMessage,
                sessionType);
        }
        else
        {
            await _notificationService.ShowNotificationAsync(
                $"{Constants.SessionTypes.TimerEmoji}{Constants.Formatting.EmojiTitleSeparator}{Constants.Messages.BreakNotificationTitle}",
                Constants.Messages.BreakNotificationMessage,
                sessionType);
        }
    }

    public void ShowConsentModal(SessionType completedSessionType)
    {
        CompletedSessionType = completedSessionType;

        // Get the countdown seconds from user settings
        var settings = _appState?.Settings;
        CountdownSeconds = settings?.AutoStartDelaySeconds ?? Constants.UI.DefaultConsentCountdownSeconds;

        AvailableOptions = _sessionOptionsService.GetOptionsForSessionType(completedSessionType, _timerService?.InterruptedPomodoro);
        IsModalVisible = true;

        StartCountdown();
        OnConsentRequired?.Invoke();
    }

    public async Task SelectOptionAsync(SessionType nextSessionType)
    {
        await HideModalAndStartSessionAsync(nextSessionType);
    }

    public async Task HandleTimeoutAsync()
    {
        var defaultOption = _sessionOptionsService.GetDefaultOption(CompletedSessionType);
        await HideModalAndStartSessionAsync(defaultOption);
    }

    public void HideConsentModal()
    {
        StopCountdown();
        IsModalVisible = false;
        OnConsentHandled?.Invoke();
    }

    public void RefreshOptions()
    {
        if (IsModalVisible)
        {
            AvailableOptions = _sessionOptionsService.GetOptionsForSessionType(CompletedSessionType, _timerService?.InterruptedPomodoro);
            OnConsentRequired?.Invoke();
        }
    }

    private async Task HideModalAndStartSessionAsync(SessionType sessionType)
    {
        StopCountdown();
        IsModalVisible = false;
        OnConsentHandled?.Invoke();

        if (_timerService == null || _taskService == null) return;

        await StartSessionAsync(sessionType);
    }

    private async Task StartSessionAsync(SessionType sessionType)
    {
        switch (sessionType)
        {
            case SessionType.Pomodoro:
                if (_taskService.CurrentTaskId.HasValue)
                {
                    await _timerService.StartPomodoroAsync(_taskService.CurrentTaskId.Value);
                }
                else
                {
                    _logger.LogWarning(Constants.Messages.LogCannotStartPomodoroNoTask);
                }
                break;
            case SessionType.ShortBreak:
                await _timerService.StartShortBreakAsync();
                break;
            case SessionType.LongBreak:
                await _timerService.StartLongBreakAsync();
                break;
        }
    }

    #region Countdown Timer Management

    /// <summary>
    /// Starts the countdown timer using PeriodicTimer for thread-safe async handling
    /// </summary>
    private void StartCountdown()
    {
        // Stop any existing countdown first
        StopCountdown();

        lock (_timerLock)
        {
            if (_isDisposed) return;

            _countdownCts = new CancellationTokenSource();
            _countdownTimer = new PeriodicTimer(TimeSpan.FromSeconds(Constants.Notifications.CountdownIntervalSeconds));

            // Run countdown in background - fire-and-forget with proper error handling
            _ = RunCountdownAsync(_countdownCts.Token);
        }
    }

    /// <summary>
    /// Stops the countdown timer and cleans up resources
    /// </summary>
    private void StopCountdown()
    {
        lock (_timerLock)
        {
            _countdownCts?.Cancel();
            _countdownCts?.Dispose();
            _countdownCts = null;

            _countdownTimer?.Dispose();
            _countdownTimer = null;
        }
    }

    /// <summary>
    /// Async countdown loop using PeriodicTimer
    /// This runs on the synchronization context when awaited, ensuring thread-safe UI updates
    /// </summary>
    private async Task RunCountdownAsync(CancellationToken cancellationToken)
    {
        try
        {
            PeriodicTimer? timer;
            lock (_timerLock)
            {
                timer = _countdownTimer;
            }

            if (timer == null) return;

            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                if (await ProcessCountdownTickAsync())
                {
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, Constants.Messages.LogCountdownError);
        }
    }

    private async Task<bool> ProcessCountdownTickAsync()
    {
        lock (_timerLock)
        {
            if (_countdownTimer == null)
            {
                return true;
            }
        }

        if (_isDisposed || !IsModalVisible)
        {
            return true;
        }

        CountdownSeconds--;
        OnCountdownTick?.Invoke();

        if (CountdownSeconds <= 0)
        {
            StopCountdown();

            if (!_isDisposed)
            {
                await HandleTimeoutAsync();
            }
            return true;
        }

        return false;
    }

    #endregion

    #region IAsyncDisposable

    public async ValueTask DisposeAsync()
    {
        _isDisposed = true;
        StopCountdown();
        IsModalVisible = false;

        await ValueTask.CompletedTask;
    }

    #endregion
}
