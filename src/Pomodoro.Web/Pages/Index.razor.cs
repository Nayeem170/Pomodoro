using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.Extensions.Logging;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Pomodoro.Web.Services.Formatters;

namespace Pomodoro.Web.Pages;

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
    private bool _splashHidden;
    protected List<TaskItem> Tasks { get; set; } = new();
    protected Guid? CurrentTaskId { get; set; }
    protected TimeSpan RemainingTime { get; set; } = TimeSpan.FromMinutes(Constants.Timer.DefaultPomodoroMinutes);
    public SessionType CurrentSessionType { get; set; } = SessionType.Pomodoro;
    protected bool IsTimerRunning { get; set; }
    protected bool IsTimerPaused { get; set; }
    protected bool IsTimerStarted { get; set; }
    protected bool IsConsentModalVisible { get; set; }
    protected int ConsentCountdown { get; set; }

    protected Guid? _undoTaskId;
    protected string? _undoTaskName;
    protected bool _undoToastVisible;
    private CancellationTokenSource? _undoCts;
    protected bool _errorToastVisible;
    protected string? _errorToastMessage;
    private CancellationTokenSource? _errorToastCts;
    protected List<ConsentOption> ConsentOptions { get; set; } = new();
    public string? ErrorMessage
    {
        get => _errorToastVisible ? _errorToastMessage : null;
        set
        {
            if (!string.IsNullOrEmpty(value))
                ShowErrorToast(value);
        }
    }
    public bool IsPipOpen { get; set; }
    protected IReadOnlyList<TaskListRef> TaskLists { get; set; } = [];
    protected IReadOnlyList<TaskListRef> GoogleLists { get; set; } = [];
    protected string? ActiveListId { get; set; }
    protected TaskListRef? ActiveList { get; set; }

    protected IReadOnlyList<TaskListRef> TabLists => TaskLists.Where(l => l.IsVisible).ToList();

    protected IReadOnlyList<TaskItem> TodayTasks => Tasks;

    protected bool IsScheduleView => ActiveListId == Constants.TaskLists.ScheduleListId;

    protected int _scheduleWeekOffset;

    protected IReadOnlyList<ScheduleDay> ScheduleWindow => BuildScheduleWindow(ScheduleWindowStart);

    protected string ScheduleWindowLabel =>
        $"{ScheduleWindowStart:MMM d} – {ScheduleWindowStart.AddDays(Constants.Tasks.ScheduleWindowDays - 1):MMM d}";

    private DateTime ScheduleWindowStart =>
        DateTime.Now.Date.AddDays(_scheduleWeekOffset * Constants.Tasks.ScheduleWindowDays);

    private int _updateSeq;
    private (int TotalFocusMinutes, int PomodoroCount, int TasksWorkedOn)? _cachedTodayStats;

    private void InvalidateTodayStatsCache() => _cachedTodayStats = null;

    protected int TodayTotalFocusMinutes => GetTodayStats().TotalFocusMinutes;
    protected int TodayPomodoroCount => GetTodayStats().PomodoroCount;
    protected int TodayTasksWorkedOn => GetTodayStats().TasksWorkedOn;
    protected int DailyGoal => TimerService.Settings.DailyGoal;

    protected IReadOnlyList<ActivityRecord> TodayPomodoroSessions => (ActivityService
        .GetTodayActivities() ?? [])
        .Where(a => a.Type == SessionType.Pomodoro)
        .OrderByDescending(a => a.CompletedAt)
        .ToList();

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
            CloudSyncService.OnSyncStatusChanged += OnCloudSyncStatusChanged;
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
                            await HandleTimerStart();
                        }
                    },
                    Logger,
                    Constants.SafeTaskOperations.KeyboardShortcutPlayPause
                );
            }, Constants.KeyboardShortcuts.PlayPauseDescription);

            KeyboardShortcutService.RegisterShortcut("ctrl+r", () =>
            {
                SafeTaskRunner.RunAndForget(
                    () => TimerService.ResetAsync(),
                    Logger,
                    Constants.SafeTaskOperations.KeyboardShortcutReset
                );
            }, Constants.KeyboardShortcuts.ResetDescription);

            // Session switching shortcuts
            KeyboardShortcutService.RegisterShortcut("ctrl+p", () =>
            {
                SafeTaskRunner.RunAndForget(
                    () => TimerService.StartPomodoroAsync(),
                    Logger,
                    Constants.SafeTaskOperations.KeyboardShortcutPomodoro
                );
            }, Constants.KeyboardShortcuts.PomodoroDescription);

            KeyboardShortcutService.RegisterShortcut("ctrl+s", () =>
            {
                SafeTaskRunner.RunAndForget(
                    () => TimerService.StartShortBreakAsync(),
                    Logger,
                    Constants.SafeTaskOperations.KeyboardShortcutShortBreak
                );
            }, Constants.KeyboardShortcuts.ShortBreakDescription);

            KeyboardShortcutService.RegisterShortcut("ctrl+l", () =>
            {
                SafeTaskRunner.RunAndForget(
                    () => TimerService.StartLongBreakAsync(),
                    Logger,
                    Constants.SafeTaskOperations.KeyboardShortcutLongBreak
                );
            }, Constants.KeyboardShortcuts.LongBreakDescription);

            // Load initial state
            await UpdateStateAsync();

            // Check for pending notification action from URL
            // Delay slightly to ensure all services are ready
            // Using SafeTaskRunner for proper exception handling
            SafeTaskRunner.RunAndForget(
                async () =>
                {
                    await Task.Delay(Constants.UI.NotificationCheckDelayMs);
                    await CheckPendingNotificationActionAsync();
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

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!IsLoading && !_splashHidden)
        {
            _splashHidden = true;
            try
            {
                await JSRuntime.InvokeVoidAsync(Constants.JsFunctions.HideSplash);
            }
            catch (Exception ex)
            {
                Logger.LogDebug(ex, Constants.Messages.SplashHideFailed);
            }
        }
    }

    /// <summary>
    /// Checks for a pending notification action from the URL parameter; handles the case
    /// where the app is opened from a notification click.
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

    private async Task UpdateStateAsync()
    {
        var seq = ++_updateSeq;
        try
        {
            var state = await IndexPagePresenterService.UpdateStateAsync(TaskService, TimerService, ActiveListId);

            if (seq != _updateSeq) return;

            Tasks = state.Tasks;
            CurrentTaskId = state.CurrentTaskId;
            RemainingTime = state.RemainingTime;
            CurrentSessionType = state.CurrentSessionType;
            IsTimerRunning = state.IsTimerRunning;
            IsTimerPaused = state.IsTimerPaused;
            IsTimerStarted = state.IsTimerStarted;
            TaskLists = state.TaskLists;
            GoogleLists = state.GoogleLists;
            ActiveListId = state.CurrentListId;
            ActiveList = TaskLists.FirstOrDefault(l => l.Id == ActiveListId);
            ErrorMessage = null;
        }
        catch (Exception ex)
        {
            if (seq != _updateSeq) return;

            Logger.LogError(ex, Constants.Messages.ErrorInUpdateState);
            ErrorMessage = $"{Constants.Messages.ErrorLoadingTasks}: {ex.Message}";
        }
    }

    protected void ShowErrorToast(string message)
    {
        _errorToastMessage = message;
        _errorToastVisible = true;
        _errorToastCts?.Cancel();
        _errorToastCts = new CancellationTokenSource();
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(Constants.UI.ErrorToastDurationMs, _errorToastCts.Token);
                _errorToastVisible = false;
                await InvokeAsync(StateHasChanged);
            }
            catch (OperationCanceledException) { }
        });
    }

    protected async Task HandleTabChange(string listId)
    {
        Console.WriteLine($"[TABDBG] HandleTabChange: clicked={listId} activeBefore={ActiveListId} serviceBefore={TaskService.CurrentListId}");
        ActiveListId = listId;
        await TaskService.SelectListAsync(listId);
        Console.WriteLine($"[TABDBG] HandleTabChange post-select: active={ActiveListId} service={TaskService.CurrentListId}");
        await UpdateStateAsync();
    }

    protected async Task HandleSchedulePrev()
    {
        if (_scheduleWeekOffset == 0) return;
        _scheduleWeekOffset--;
        await UpdateStateAsync();
    }

    protected async Task HandleScheduleNext()
    {
        _scheduleWeekOffset++;
        await UpdateStateAsync();
    }

    protected string? GetCurrentTaskPath()
    {
        if (!CurrentTaskId.HasValue) return null;
        return TaskPathFormatter.BuildPath(AppState.Tasks, CurrentTaskId.Value);
    }

    protected IReadOnlyList<string>? GetCurrentTaskSegments()
    {
        if (!CurrentTaskId.HasValue) return null;
        return TaskPathFormatter.BuildSegments(AppState.Tasks, CurrentTaskId.Value);
    }

    protected string? GetCurrentTaskAriaLabel()
    {
        if (!CurrentTaskId.HasValue) return null;
        return TaskPathFormatter.BuildAriaLabel(AppState.Tasks, CurrentTaskId.Value);
    }

    protected static string FormatFocusMinutes(int minutes)
    {
        if (minutes < Constants.TimeConversion.MinutesPerHour)
            return string.Format(Constants.TimeFormats.MinutesFormat, minutes);
        var hours = minutes / Constants.TimeConversion.MinutesPerHour;
        var mins = minutes % Constants.TimeConversion.MinutesPerHour;
        return string.Format(Constants.TimeFormats.HoursMinutesFormat, hours, mins);
    }

    private IReadOnlyList<ScheduleDay> BuildScheduleWindow(DateTime start)
    {
        var candidates = AppState.Tasks.Where(t => !t.IsDeleted && !t.IsSubtask).ToList();
        var days = new List<ScheduleDay>(Constants.Tasks.ScheduleWindowDays);

        for (var offset = 0; offset < Constants.Tasks.ScheduleWindowDays; offset++)
        {
            var date = start.AddDays(offset);
            var items = candidates
                .Where(t => OccursOn(t, date))
                .Select(t => new ScheduleItem
                {
                    TaskId = t.Id,
                    Title = t.Name,
                    IsRepeat = t.IsRecurring,
                    RepeatLabel = BuildRepeatLabel(t.Repeat),
                    IsGoogle = t.IsGoogleTask,
                    IsCompleted = t.IsCompleted,
                    Task = t
                })
                .ToList();

            days.Add(new ScheduleDay
            {
                Date = date,
                DayLabel = date.ToString(Constants.Tasks.ScheduleDayFormat, CultureInfo.InvariantCulture),
                Items = items
            });
        }

        return days;
    }

    private static bool OccursOn(TaskItem task, DateTime date) => task.OccursOn(date);

    private static string? BuildRepeatLabel(RepeatRule? rule) => rule?.Type switch
    {
        RepeatType.Daily => Constants.Repeat.LabelDaily,
        RepeatType.Weekly => Constants.Repeat.LabelWeekly,
        RepeatType.Monthly => Constants.Repeat.LabelMonthly,
        RepeatType.Custom => rule.CustomDays > 0 ? $"×{rule.CustomDays}d" : Constants.Repeat.LabelRepeat,
        _ => null
    };

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
            _undoCts?.Cancel();
            _undoCts?.Dispose();
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
        if (CloudSyncService != null)
            CloudSyncService.OnSyncStatusChanged -= OnCloudSyncStatusChanged;
    }

    private void UnregisterKeyboardShortcuts()
    {
        if (KeyboardShortcutService != null)
        {
            KeyboardShortcutService.UnregisterShortcut("space");
            KeyboardShortcutService.UnregisterShortcut("ctrl+r");
            KeyboardShortcutService.UnregisterShortcut("ctrl+p");
            KeyboardShortcutService.UnregisterShortcut("ctrl+s");
            KeyboardShortcutService.UnregisterShortcut("ctrl+l");
        }
    }

    #endregion
}
