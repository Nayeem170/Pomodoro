using System.Threading;
using Microsoft.JSInterop;
using Microsoft.Extensions.Logging;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services.Repositories;

namespace Pomodoro.Web.Services;

public class TimerService : ITimerService, ITimerEventPublisher, IAsyncDisposable
{
    private readonly IIndexedDbService _indexedDb;
    private readonly ISettingsRepository _settingsRepository;
    private readonly IDailyStatsService _dailyStatsService;
    private readonly IJsTimerInterop _jsTimerInterop;
    private readonly AppState _appState;
    private readonly ILogger<TimerService> _logger;
    private DotNetObjectReference<object>? _dotNetRef;
    private SynchronizationContext? _syncContext;
    private readonly SemaphoreSlim _timerCompleteLock = new(Constants.Threading.SemaphoreInitialCount, Constants.Threading.SemaphoreMaxCount);
    private readonly object _timerTickLock = new();
    private bool _isDisposed;

    // ITimerEventPublisher events
    public event Func<TimerCompletedEventArgs, Task>? OnTimerCompleted;
    public event Action? OnTimerStateChanged;
    public event Action? OnTick;

    // Public properties for UI binding
    public int RemainingSeconds => _appState.CurrentSession?.RemainingSeconds ?? 0;
    public int TickCount { get; private set; } // Used to force UI updates

    public TimerSession? CurrentSession => _appState.CurrentSession;
    public TimerSettings Settings => _appState.Settings;
    public bool IsRunning => _appState.CurrentSession?.IsRunning ?? false;
    // IsPaused requires WasStarted to distinguish between "reset but not started" and "started then paused"
    // This ensures PiP toggle logic correctly calls StartPomodoroAsync instead of ResumeAsync after reset
    public bool IsPaused => _appState.CurrentSession != null && !_appState.CurrentSession.IsRunning && _appState.CurrentSession.WasStarted;
    public bool IsStarted => _appState.CurrentSession?.WasStarted ?? false;
    public SessionType CurrentSessionType => _appState.CurrentSession?.Type ?? SessionType.Pomodoro;
    public TimeSpan RemainingTime => TimeSpan.FromSeconds(RemainingSeconds);

    public TimerService(
        IIndexedDbService indexedDb,
        ISettingsRepository settingsRepository,
        IDailyStatsService dailyStatsService,
        IJsTimerInterop jsTimerInterop,
        AppState appState,
        ILogger<TimerService> logger)
    {
        _indexedDb = indexedDb;
        _settingsRepository = settingsRepository;
        _dailyStatsService = dailyStatsService;
        _jsTimerInterop = jsTimerInterop;
        _appState = appState;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        // Capture the synchronization context for UI updates
        _syncContext = SynchronizationContext.Current;

        // Load settings from repository
        var settings = await _settingsRepository.GetAsync();
        if (settings != null)
        {
            _appState.Settings = settings;
        }

        // Load daily stats from IndexedDB
        await _dailyStatsService.InitializeTodayStatsAsync();

        // Initialize with a default Pomodoro session if none exists
        if (_appState.CurrentSession == null)
        {
            var durationSeconds = _appState.Settings.PomodoroMinutes * Constants.TimeConversion.SecondsPerMinute;
            _appState.CurrentSession = new TimerSession
            {
                Id = Guid.NewGuid(),
                TaskId = null,
                Type = SessionType.Pomodoro,
                StartedAt = DateTime.UtcNow,
                DurationSeconds = durationSeconds,
                RemainingSeconds = durationSeconds,
                IsRunning = false,
                IsCompleted = false
            };
        }

        // Create dotnet reference for JS callbacks
        _dotNetRef = DotNetObjectReference.Create<object>(this);

        // Initialize JavaScript constants with user settings for chart time calculations
        await _indexedDb.InitializeJsConstantsAsync(
            _appState.Settings.PomodoroMinutes,
            _appState.Settings.ShortBreakMinutes,
            _appState.Settings.LongBreakMinutes);

        NotifyStateChanged();
    }

    // Called from JavaScript
    [JSInvokable(Constants.JsInvokableMethods.OnTimerTick)]
    public void OnTimerTickJs()
    {
        _dailyStatsService.CheckAndResetIfNeeded();

        // Use lock to ensure thread-safe access to session state
        // This prevents race conditions if JS callback fires during other state modifications
        lock (_timerTickLock)
        {
            if (_appState.CurrentSession == null || !_appState.CurrentSession.IsRunning)
            {
                return;
            }

            var session = _appState.CurrentSession;
            if (session.EndAt.HasValue)
            {
                var remaining = (int)(session.EndAt.Value - DateTime.UtcNow).TotalSeconds;
                session.RemainingSeconds = Math.Max(0, remaining);
            }
            else
            {
                session.RemainingSeconds--;
            }
            TickCount++;

            if (_appState.CurrentSession.RemainingSeconds <= 0)
            {
                // Use SafeTaskRunner for consistent fire-and-forget handling with error logging
                SafeTaskRunner.RunAndForget(
                    HandleTimerCompleteSafeAsync,
                    _logger,
                    Constants.SafeTaskOperations.TimerComplete
                );
                return;
            }
        }

        // Use synchronization context to ensure UI update happens on main thread
        if (_syncContext != null)
        {
            _syncContext.Post(_ => NotifyTick(), null);
        }
        else
        {
            NotifyTick();
        }
    }

    public async Task StartPomodoroAsync(Guid? taskId = null)
    {
        await StartSessionAsync(SessionType.Pomodoro, taskId);
    }

    public async Task StartShortBreakAsync()
    {
        await StartSessionAsync(SessionType.ShortBreak);
    }

    public async Task StartLongBreakAsync()
    {
        await StartSessionAsync(SessionType.LongBreak);
    }

    private async Task StartSessionAsync(SessionType sessionType, Guid? taskId = null)
    {
        var durationSeconds = _appState.Settings.GetDurationSeconds(sessionType);

        _appState.CurrentSession = new TimerSession
        {
            Id = Guid.NewGuid(),
            TaskId = taskId,
            Type = sessionType,
            StartedAt = DateTime.UtcNow,
            DurationSeconds = durationSeconds,
            RemainingSeconds = durationSeconds,
            IsRunning = true,
            IsCompleted = false,
            WasStarted = true,
            EndAt = DateTime.UtcNow.AddSeconds(durationSeconds)
        };

        NotifyStateChanged();
        _dotNetRef ??= DotNetObjectReference.Create<object>(this);
        await _jsTimerInterop.StartAsync(_dotNetRef);
    }

    public async Task SwitchSessionTypeAsync(SessionType sessionType)
    {
        // Stop current timer
        await _jsTimerInterop.StopAsync();

        // Get duration for the new session type using helper method
        var durationSeconds = _appState.Settings.GetDurationSeconds(sessionType);

        // Create new session (not running, just prepared)
        _appState.CurrentSession = new TimerSession
        {
            Id = Guid.NewGuid(),
            TaskId = _appState.CurrentSession?.TaskId,
            Type = sessionType,
            StartedAt = DateTime.UtcNow,
            DurationSeconds = durationSeconds,
            RemainingSeconds = durationSeconds,
            IsRunning = false,
            IsCompleted = false,
            EndAt = null
        };

        NotifyStateChanged();
    }

    public async Task PauseAsync()
    {
        if (_appState.CurrentSession != null && _appState.CurrentSession.IsRunning)
        {
            _appState.CurrentSession.IsRunning = false;
            _appState.CurrentSession.EndAt = null;
            await _jsTimerInterop.StopAsync();
            NotifyStateChanged();
        }
    }

    public async Task ResumeAsync()
    {
        if (_appState.CurrentSession != null && !_appState.CurrentSession.IsRunning)
        {
            _appState.CurrentSession.IsRunning = true;
            _appState.CurrentSession.EndAt = DateTime.UtcNow.AddSeconds(_appState.CurrentSession.RemainingSeconds);
            NotifyStateChanged();
            _dotNetRef ??= DotNetObjectReference.Create<object>(this);
            await _jsTimerInterop.StartAsync(_dotNetRef);
        }
    }

    public async Task ResetAsync()
    {
        await _jsTimerInterop.StopAsync();

        // Reset tick count to prevent potential overflow
        TickCount = 0;

        if (_appState.CurrentSession != null)
        {
            // Use helper method to get duration for current session type
            var durationSeconds = _appState.Settings.GetDurationSeconds(_appState.CurrentSession.Type);

            _appState.CurrentSession.DurationSeconds = durationSeconds;
            _appState.CurrentSession.RemainingSeconds = durationSeconds;
            _appState.CurrentSession.IsRunning = false;
            _appState.CurrentSession.WasStarted = false;
            _appState.CurrentSession.EndAt = null;
        }

        NotifyStateChanged();
    }

    public async Task UpdateSettingsAsync(TimerSettings settings)
    {
        _appState.Settings = settings;
        await SaveSettingsAsync();

        // Update current session duration if timer hasn't started yet
        if (_appState.CurrentSession != null && !_appState.CurrentSession.WasStarted)
        {
            var durationSeconds = settings.GetDurationSeconds(_appState.CurrentSession.Type);
            _appState.CurrentSession.DurationSeconds = durationSeconds;
            _appState.CurrentSession.RemainingSeconds = durationSeconds;
        }

        // Initialize JS constants with new settings for chart time calculations
        await _indexedDb.InitializeJsConstantsAsync(settings.PomodoroMinutes, settings.ShortBreakMinutes, settings.LongBreakMinutes);

        NotifyStateChanged();
    }

    public async Task SaveSettingsAsync()
    {
        await _settingsRepository.SaveAsync(_appState.Settings);
    }

    private async Task HandleTimerCompleteAsync()
    {
        await _jsTimerInterop.StopAsync();

        var session = _appState.CurrentSession;
        if (session == null) return;

        session.IsRunning = false;
        session.IsCompleted = true;
        session.EndAt = null;

        // Reset remaining seconds back to full duration for display
        session.RemainingSeconds = session.DurationSeconds;

        // Get task name for event args (thread-safe)
        string? taskName = null;
        if (session.TaskId.HasValue)
        {
            var task = _appState.Tasks.FirstOrDefault(t => t.Id == session.TaskId.Value);
            taskName = task?.Name;
        }

        // Calculate duration using helper method
        var durationMinutes = _appState.Settings.GetDurationMinutes(session.Type);

        // If pomodoro completed, update today's stats
        if (session.Type == SessionType.Pomodoro && session.TaskId.HasValue)
        {
            _dailyStatsService.RecordPomodoroCompletion(durationMinutes, session.TaskId.Value);
            await SaveDailyStatsAsync();
        }

        // Create event args
        var eventArgs = new TimerCompletedEventArgs(
            session.Type,
            session.TaskId,
            taskName,
            durationMinutes,
            WasCompleted: true,
            CompletedAt: DateTime.UtcNow
        );

        // Raise event for subscribers (TaskService, ActivityService)
        await NotifyTimerCompletedAsync(eventArgs);

        NotifyStateChanged();

        // Note: Auto-start is handled by ConsentService which shows a consent modal
        // When auto-start is enabled, the modal appears with a countdown
        // When auto-start is disabled, no modal appears and user manually starts next session
    }

    /// <summary>
    /// Safe async wrapper for HandleTimerCompleteAsync to avoid fire-and-forget issues
    /// Uses semaphore to prevent concurrent timer completion handling
    /// </summary>
    private async Task HandleTimerCompleteSafeAsync()
    {
        if (_isDisposed) return;

        // Try to acquire lock - if another completion is in progress, skip this one
        if (!await _timerCompleteLock.WaitAsync(0))
        {
            return;
        }

        try
        {
            await HandleTimerCompleteAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Constants.Messages.TimerHandleCompleteError);
        }
        finally
        {
            try { _timerCompleteLock.Release(); } catch (ObjectDisposedException) { }
        }
    }

    private async Task SaveDailyStatsAsync()
    {
        var stats = new DailyStats
        {
            Date = AppState.GetCurrentDayKey(),
            TotalFocusMinutes = _appState.TodayTotalFocusMinutes,
            PomodoroCount = _appState.TodayPomodoroCount,
            TaskIdsWorkedOn = _appState.TodayTaskIdsWorkedOn
        };
        await _indexedDb.PutAsync(Constants.Storage.DailyStatsStore, stats);
    }

    private void NotifyTick()
    {
        OnTick?.Invoke();
    }

    private void NotifyStateChanged()
    {
        OnTimerStateChanged?.Invoke();
    }

    private async Task NotifyTimerCompletedAsync(TimerCompletedEventArgs args)
    {
        if (OnTimerCompleted != null)
        {
            // Get all handlers and invoke them
            var handlers = OnTimerCompleted.GetInvocationList();
            foreach (var handler in handlers)
            {
                try
                {
                    await ((Func<TimerCompletedEventArgs, Task>)handler)(args);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, Constants.Messages.TimerCompletionHandlerError);
                }
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        _isDisposed = true;
        await _jsTimerInterop.StopAsync();
        _dotNetRef?.Dispose();
        _timerCompleteLock.Dispose();
    }
}
