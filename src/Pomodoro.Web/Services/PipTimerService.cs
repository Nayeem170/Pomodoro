using System.Text.Json;
using Microsoft.JSInterop;
using Microsoft.Extensions.Logging;
using Pomodoro.Web.Models;

namespace Pomodoro.Web.Services;

/// <summary>
/// Service for Picture-in-Picture timer functionality
/// Provides a floating, always-on-top timer window
/// </summary>
public class PipTimerService : IPipTimerService, ITimerEventPublisherSubscriber
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ITimerService _timerService;
    private readonly ITaskService _taskService;
    private readonly AppState _appState;
    private readonly ILogger<PipTimerService> _logger;
    private DotNetObjectReference<PipTimerService>? _dotNetRef;
    private bool _isInitialized;
    private bool _isDisposed;

    public bool IsSupported { get; private set; }
    public bool IsOpen { get; private set; }

    public event Action? OnPipOpened;
    public event Action? OnPipClosed;

    public PipTimerService(
        IJSRuntime jsRuntime,
        ITimerService timerService,
        ITaskService taskService,
        AppState appState,
        ILogger<PipTimerService> logger)
    {
        _jsRuntime = jsRuntime;
        _timerService = timerService;
        _taskService = taskService;
        _appState = appState;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        if (_isInitialized) return;

        try
        {
            // Check if PiP API is supported
            IsSupported = await _jsRuntime.InvokeAsync<bool>(Constants.PipJsFunctions.IsSupported);

            // Create .NET reference for JS callbacks
            _dotNetRef = DotNetObjectReference.Create(this);
            await _jsRuntime.InvokeVoidAsync(Constants.PipJsFunctions.RegisterDotNetRef, _dotNetRef);

            _isInitialized = true;

            _logger.LogDebug(Constants.Messages.LogPipInitialized, IsSupported);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Constants.Messages.LogPipInitializationFailed);
        }
    }

    public async Task<bool> OpenAsync()
    {
        if (_isDisposed) return false;

        try
        {
            var timerState = GetTimerState();
            var success = await _jsRuntime.InvokeAsync<bool>(Constants.PipJsFunctions.Open, timerState);

            if (success)
            {
                IsOpen = true;
                OnPipOpened?.Invoke();
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Constants.Messages.LogPipOpenFailed);
            return false;
        }
    }

    public async Task CloseAsync()
    {
        if (_isDisposed) return;

        try
        {
            await _jsRuntime.InvokeVoidAsync(Constants.PipJsFunctions.Close);
            IsOpen = false;
            OnPipClosed?.Invoke();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Constants.Messages.LogPipCloseFailed);
        }
    }

    public async Task UpdateTimerAsync()
    {
        if (_isDisposed || !IsOpen) return;

        try
        {
            var timerState = GetTimerState();
            await _jsRuntime.InvokeVoidAsync(Constants.PipJsFunctions.Update, timerState);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Constants.Messages.LogPipUpdateFailed);
        }
    }

    /// <summary>
    /// Starts the current session type timer.
    /// Only starts pomodoro if a task is selected.
    /// </summary>
    private async Task StartCurrentSessionAsync()
    {
        var sessionType = _timerService.CurrentSessionType;
        if (sessionType == SessionType.Pomodoro)
        {
            if (_taskService.CurrentTaskId.HasValue)
            {
                await _timerService.StartPomodoroAsync(_taskService.CurrentTaskId.Value);
            }
        }
        else if (sessionType == SessionType.ShortBreak)
        {
            await _timerService.StartShortBreakAsync();
        }
        else if (sessionType == SessionType.LongBreak)
        {
            await _timerService.StartLongBreakAsync();
        }
    }

    /// <summary>
    /// Gets the current timer state for the PiP window.
    /// Single source of truth for UI state - PiP uses pre-computed values.
    /// </summary>
    private object GetTimerState()
    {
        return new
        {
            remainingSeconds = _timerService.RemainingSeconds,
            sessionType = (int)_timerService.CurrentSessionType,
            totalDurationSeconds = _timerService.Settings.GetDurationSeconds(_timerService.CurrentSessionType)
        };
    }

    /// <summary>
    /// Called from JavaScript when timer toggle is clicked in PiP window.
    /// Handles play/pause toggle and ensures PiP window state is synchronized.
    /// </summary>
    [JSInvokable(Constants.JsInvokableMethods.OnPipToggleTimer)]
    public async Task OnPipToggleTimer()
    {
        try
        {
            // Capture the operation type for logging
            var operation = _timerService.IsRunning ? "pause" :
                           _timerService.IsPaused ? "resume" : "start";

            if (_timerService.IsRunning)
            {
                await _timerService.PauseAsync();
            }
            else if (_timerService.IsPaused)
            {
                await _timerService.ResumeAsync();
            }
            else
            {
                await StartCurrentSessionAsync();
            }

            // Force immediate update with fresh state to ensure UI consistency.
            // This is critical because the event-based update via OnTimerStateChanged
            // uses SafeTaskRunner.RunAndForget which may complete before state is fully propagated.
            // We explicitly await here to ensure the PiP window receives the correct state.
            await UpdateTimerAsync();

            _logger.LogDebug("PiP toggle operation '{Operation}' completed. IsRunning: {IsRunning}, IsStarted: {IsStarted}",
                operation, _timerService.IsRunning, _timerService.IsStarted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Constants.Messages.LogPipToggleTimerError);
        }
    }

    /// <summary>
    /// Called from JavaScript when reset is clicked in PiP window
    /// </summary>
    [JSInvokable(Constants.JsInvokableMethods.OnPipResetTimer)]
    public async Task OnPipResetTimer()
    {
        try
        {
            await _timerService.ResetAsync();
            await UpdateTimerAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Constants.Messages.LogPipResetTimerError);
        }
    }

    /// <summary>
    /// Called from JavaScript when session switch is clicked in PiP window
    /// </summary>
    [JSInvokable(Constants.JsInvokableMethods.OnPipSwitchSession)]
    public async Task OnPipSwitchSession(int sessionType)
    {
        try
        {
            var type = (SessionType)sessionType;
            await _timerService.SwitchSessionTypeAsync(type);
            await UpdateTimerAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Constants.Messages.LogPipSwitchSessionError);
        }
    }

    /// <summary>
    /// Called from JavaScript when PiP window is closed
    /// </summary>
    [JSInvokable(Constants.JsInvokableMethods.OnPipClosed)]
    public void OnPipClosedJs()
    {
        IsOpen = false;
        OnPipClosed?.Invoke();
    }

    public void HandleTimerTick()
    {
        SafeTaskRunner.RunAndForget(
            UpdateTimerAsync,
            _logger,
            Constants.SafeTaskOperations.PipTimerTick
        );
    }

    public void HandleTimerStateChanged()
    {
        SafeTaskRunner.RunAndForget(
            UpdateTimerAsync,
            _logger,
            Constants.SafeTaskOperations.PipTimerStateChanged
        );
    }

    public async ValueTask DisposeAsync()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        try
        {
            await _jsRuntime.InvokeVoidAsync(Constants.PipJsFunctions.UnregisterDotNetRef);
            await CloseAsync();
        }
        catch
        {
            // Ignore errors during disposal
        }

        _dotNetRef?.Dispose();
    }
}
