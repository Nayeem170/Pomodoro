using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.Extensions.Logging;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;

namespace Pomodoro.Web.Pages;

/// <summary>
/// Main partial for Index page
/// Contains dependency injection, state management, and lifecycle methods
/// </summary>
public partial class IndexBase : ComponentBase, IDisposable
{
    #region Services (Dependency Injection)

    [Inject]
    protected ITaskService TaskService { get; set; } = default!;

    [Inject]
    internal ILogger<IndexBase> Logger { get; set; } = default!;

    [Inject]
    protected ITimerService TimerService { get; set; } = default!;

    [Inject]
    protected ITimerEventPublisher TimerEventPublisher { get; set; } = default!;

    [Inject]
    protected IConsentService ConsentService { get; set; } = default!;

    [Inject]
    protected INotificationService NotificationService { get; set; } = default!;

    [Inject]
    protected IActivityService ActivityService { get; set; } = default!;

    [Inject]
    protected IPipTimerService PipTimerService { get; set; } = default!;

    [Inject]
    protected AppState AppState { get; set; } = default!;

    [Inject]
    protected IJSRuntime JSRuntime { get; set; } = default!;

    [Inject]
    protected IKeyboardShortcutService KeyboardShortcutService { get; set; } = default!;

    [Inject]
    protected ITodayStatsService TodayStatsService { get; set; } = default!;

    [Inject]
    protected ICloudSyncService CloudSyncService { get; set; } = default!;

    [Inject]
    internal IndexPagePresenterService IndexPagePresenterService { get; set; } = default!;

    #endregion

    #region State

    protected bool IsLoading { get; set; } = true;
    protected List<TaskItem> Tasks { get; set; } = new();
    protected Guid? CurrentTaskId { get; set; }
    protected TimeSpan RemainingTime { get; set; } = TimeSpan.FromMinutes(Constants.Timer.DefaultPomodoroMinutes);
    public SessionType CurrentSessionType { get; set; } = SessionType.Pomodoro;
    protected bool IsTimerRunning { get; set; }
    protected bool IsTimerPaused { get; set; }
    protected bool IsTimerStarted { get; set; }
    protected bool IsConsentModalVisible { get; set; }
    protected int ConsentCountdown { get; set; }
    protected List<ConsentOption> ConsentOptions { get; set; } = new();
    internal bool ShowKeyboardHelp { get; set; }
    public string? ErrorMessage { get; set; }
    public bool IsPipOpen { get; set; }

    private (int TotalFocusMinutes, int PomodoroCount, int TasksWorkedOn)? _cachedTodayStats;

    private void InvalidateTodayStatsCache() => _cachedTodayStats = null;

    protected int TodayTotalFocusMinutes => GetTodayStats().TotalFocusMinutes;
    protected int TodayPomodoroCount => GetTodayStats().PomodoroCount;
    protected int TodayTasksWorkedOn => GetTodayStats().TasksWorkedOn;
    protected int DailyGoal => TimerService.Settings.DailyGoal;

    private (int TotalFocusMinutes, int PomodoroCount, int TasksWorkedOn) GetTodayStats()
    {
        return _cachedTodayStats ??= TodayStatsService.GetTodayStats();
    }

    #endregion

    #region Lifecycle Methods

    protected override async Task OnInitializedAsync()
    {
        try
        {
            // Initialize notification service
            await NotificationService.InitializeAsync();

            // Initialize PiP timer service
            await PipTimerService.InitializeAsync();

            // Subscribe to service events
            TaskService.OnChange += OnTaskServiceChanged;
            TimerEventPublisher.OnTimerCompleted += OnTimerCompleted;
            TimerEventPublisher.OnTimerStateChanged += OnTimerStateChanged;
            ConsentService.OnConsentRequired += OnConsentRequired;
            ConsentService.OnCountdownTick += OnConsentCountdownTick;
            ConsentService.OnConsentHandled += OnConsentHandled;

            // Subscribe to notification action events
            NotificationService.OnNotificationAction += OnNotificationAction;

            // Subscribe to activity changes to refresh today's summary
            ActivityService.OnActivityChanged += OnActivityChanged;

            // Subscribe to PiP events
            PipTimerService.OnPipOpened += OnPipOpened;
            PipTimerService.OnPipClosed += OnPipClosed;

            // Register keyboard shortcuts with proper error handling
            KeyboardShortcutService.RegisterShortcut("space", () =>
            {
                SafeTaskRunner.RunAndForget(
                    async () =>
                    {
                        if (TimerService.IsRunning)
                        {
                            await TimerService.PauseAsync();
                        }
                        else if (TimerService.IsPaused)
                        {
                            await TimerService.ResumeAsync();
                        }
                        else
                        {
                            await TimerService.StartPomodoroAsync();
                        }
                    },
                    Logger,
                    Constants.SafeTaskOperations.KeyboardShortcutPlayPause
                );
            }, Constants.KeyboardShortcuts.PlayPauseDescription);

            KeyboardShortcutService.RegisterShortcut("r", () =>
            {
                SafeTaskRunner.RunAndForget(
                    () => TimerService.ResetAsync(),
                    Logger,
                    Constants.SafeTaskOperations.KeyboardShortcutReset
                );
            }, Constants.KeyboardShortcuts.ResetDescription);

            // Session switching shortcuts
            KeyboardShortcutService.RegisterShortcut("p", () =>
            {
                SafeTaskRunner.RunAndForget(
                    () => TimerService.StartPomodoroAsync(),
                    Logger,
                    Constants.SafeTaskOperations.KeyboardShortcutPomodoro
                );
            }, Constants.KeyboardShortcuts.PomodoroDescription);

            KeyboardShortcutService.RegisterShortcut("s", () =>
            {
                SafeTaskRunner.RunAndForget(
                    () => TimerService.StartShortBreakAsync(),
                    Logger,
                    Constants.SafeTaskOperations.KeyboardShortcutShortBreak
                );
            }, Constants.KeyboardShortcuts.ShortBreakDescription);

            KeyboardShortcutService.RegisterShortcut("l", () =>
            {
                SafeTaskRunner.RunAndForget(
                    () => TimerService.StartLongBreakAsync(),
                    Logger,
                    Constants.SafeTaskOperations.KeyboardShortcutLongBreak
                );
            }, Constants.KeyboardShortcuts.LongBreakDescription);

            // Help shortcut
            KeyboardShortcutService.RegisterShortcut("?", () =>
            {
                ShowKeyboardHelp = true;
                StateHasChanged();
            }, Constants.KeyboardShortcuts.HelpDescription);

            // Escape shortcut - close keyboard help modal
            KeyboardShortcutService.RegisterShortcut("escape", () =>
            {
                if (ShowKeyboardHelp)
                {
                    ShowKeyboardHelp = false;
                    StateHasChanged();
                }
            }, "Close keyboard shortcuts");

            // Load initial state
            UpdateState();

            // Check for pending notification action from URL
            // Delay slightly to ensure all services are ready
            // Using SafeTaskRunner for proper exception handling
            SafeTaskRunner.RunAndForget(
                async () =>
                {
                    await Task.Delay(Constants.UI.NotificationCheckDelayMs);
                    await CheckPendingNotificationActionAsync();
                    await CloudSyncService.AutoSyncOnStartAsync();
                },
                Logger,
                Constants.SafeTaskOperations.CheckPendingNotificationAction
            );
        }
        catch (Exception ex)
        {
            ErrorMessage = $"{Constants.Messages.ErrorInitializing}: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Check for pending notification action from URL parameter
    /// This handles the case when the app is opened from a notification click
    /// </summary>
    internal async Task CheckPendingNotificationActionAsync()
    {
        try
        {
            // Check URL parameter (set by service worker when opening new window)
            var urlAction = await JSRuntime.InvokeAsync<string>(Constants.JsFunctions.GetUrlParameter, Constants.UrlParameters.NotificationAction);
            if (!string.IsNullOrEmpty(urlAction))
            {
                var decodedAction = Uri.UnescapeDataString(urlAction);
                // Clean up URL
                await JSRuntime.InvokeVoidAsync(Constants.JsFunctions.RemoveUrlParameter, Constants.UrlParameters.NotificationAction);
                // Process the action
                await InvokeAsync(() => OnNotificationAction(decodedAction));
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, Constants.Messages.ErrorCheckingPendingNotificationAction);
        }
    }

    #endregion

    #region Helper Methods

    private void UpdateState()
    {
        var state = IndexPagePresenterService.UpdateState(TaskService, TimerService);

        Tasks = state.Tasks;
        CurrentTaskId = state.CurrentTaskId;
        RemainingTime = state.RemainingTime;
        CurrentSessionType = state.CurrentSessionType;
        IsTimerRunning = state.IsTimerRunning;
        IsTimerPaused = state.IsTimerPaused;
        IsTimerStarted = state.IsTimerStarted;
    }

    #endregion

    #region Cleanup

    private bool _isDisposed;

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        try
        {
            UnsubscribeFromAllServices();
            UnregisterKeyboardShortcuts();
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, Constants.Messages.ErrorInDispose);
        }
    }

    private void UnsubscribeFromAllServices()
    {
        if (TaskService != null)
            TaskService.OnChange -= OnTaskServiceChanged;
        if (TimerEventPublisher != null)
        {
            TimerEventPublisher.OnTimerCompleted -= OnTimerCompleted;
            TimerEventPublisher.OnTimerStateChanged -= OnTimerStateChanged;
        }
        if (ConsentService != null)
        {
            ConsentService.OnConsentRequired -= OnConsentRequired;
            ConsentService.OnCountdownTick -= OnConsentCountdownTick;
            ConsentService.OnConsentHandled -= OnConsentHandled;
        }
        if (NotificationService != null)
            NotificationService.OnNotificationAction -= OnNotificationAction;
        if (ActivityService != null)
            ActivityService.OnActivityChanged -= OnActivityChanged;
        if (PipTimerService != null)
        {
            PipTimerService.OnPipOpened -= OnPipOpened;
            PipTimerService.OnPipClosed -= OnPipClosed;
        }
    }

    private void UnregisterKeyboardShortcuts()
    {
        if (KeyboardShortcutService != null)
        {
            KeyboardShortcutService.UnregisterShortcut("space");
            KeyboardShortcutService.UnregisterShortcut("r");
            KeyboardShortcutService.UnregisterShortcut("p");
            KeyboardShortcutService.UnregisterShortcut("s");
            KeyboardShortcutService.UnregisterShortcut("l");
            KeyboardShortcutService.UnregisterShortcut("?");
            KeyboardShortcutService.UnregisterShortcut("escape");
        }
    }

    #endregion
}
