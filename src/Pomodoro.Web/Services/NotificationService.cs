using Microsoft.JSInterop;
using Microsoft.Extensions.Logging;
using Pomodoro.Web.Models;

namespace Pomodoro.Web.Services;

/// <summary>
/// Implementation of notification service using browser APIs
/// </summary>
public class NotificationService : INotificationService, IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private readonly AppState _appState;
    private readonly ILogger<NotificationService> _logger;
    private DotNetObjectReference<NotificationService>? _dotNetRef;
    private Task? _initializationTask;
    private readonly SemaphoreSlim _initSemaphore = new(1, 1);

    public bool IsNotificationPermitted { get; private set; }

    public event Action<string>? OnNotificationAction;

    public NotificationService(IJSRuntime jsRuntime, AppState appState, ILogger<NotificationService> logger)
    {
        _jsRuntime = jsRuntime;
        _appState = appState;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        // Use async semaphore to ensure only one initialization runs at a time
        // and concurrent callers wait for the same initialization to complete
        await _initSemaphore.WaitAsync();
        try
        {
            // If initialization is already in progress or completed, return the existing task
            if (_initializationTask != null)
            {
                return;
            }

            // Start initialization and store the task
            _initializationTask = InitializeCoreAsync();
            await _initializationTask;
        }
        finally
        {
            _initSemaphore.Release();
        }
    }

    private async Task InitializeCoreAsync()
    {
        try
        {
            // Dispose existing reference if any (shouldn't happen, but safety check)
            _dotNetRef?.Dispose();

            // Create dotnet reference for JS callbacks
            _dotNetRef = DotNetObjectReference.Create(this);
            await _jsRuntime.InvokeVoidAsync(Constants.NotificationJsFunctions.RegisterDotNetRef, _dotNetRef);

            await RequestPermissionAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Constants.Messages.ErrorInitializingNotificationService);
            // Reset initialization task on failure to allow retry
            _initializationTask = null;
        }
    }

    public async Task<bool> RequestPermissionAsync()
    {
        try
        {
            var permission = await _jsRuntime.InvokeAsync<string>(Constants.NotificationJsFunctions.RequestPermission);
            IsNotificationPermitted = permission == Constants.NotificationPermissions.Granted;
            return IsNotificationPermitted;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Constants.Messages.ErrorRequestingNotificationPermission);
            return false;
        }
    }

    /// <summary>
    /// Refreshes the notification permission state from the browser.
    /// Call this when settings page opens to ensure UI shows current permission status.
    /// Note: This does not affect ShowNotificationAsync which checks permission directly in JS.
    /// </summary>
    public async Task RefreshPermissionStateAsync()
    {
        try
        {
            var permission = await _jsRuntime.InvokeAsync<string>(Constants.NotificationJsFunctions.RequestPermission);
            IsNotificationPermitted = permission == Constants.NotificationPermissions.Granted;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Constants.Messages.ErrorRequestingNotificationPermission);
        }
    }

    public async Task ShowNotificationAsync(string title, string body, SessionType sessionType, string? icon = null)
    {
        // Fast-fail if we know permission isn't granted (avoids unnecessary JS interop)
        if (!IsNotificationPermitted) return;

        try
        {
            // Pass session type as int (0 = Pomodoro, 1 = ShortBreak, 2 = LongBreak)
            await _jsRuntime.InvokeVoidAsync(Constants.NotificationJsFunctions.ShowNotification, title, body, icon, (int)sessionType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Constants.Messages.ErrorShowingNotification);
        }
    }

    // Called from JavaScript when notification action is clicked
    [JSInvokable(Constants.JsInvokableMethods.OnNotificationActionClick)]
    public void OnNotificationActionClick(string action)
    {
        OnNotificationAction?.Invoke(action);
    }

    public async Task PlayTimerCompleteSoundAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync(Constants.NotificationJsFunctions.PlayTimerCompleteSound);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, Constants.Messages.ErrorPlayingTimerCompleteSound);
        }
    }

    public async Task PlayBreakCompleteSoundAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync(Constants.NotificationJsFunctions.PlayBreakCompleteSound);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, Constants.Messages.ErrorPlayingBreakCompleteSound);
        }
    }

    public async ValueTask DisposeAsync()
    {
        // Unregister the .NET reference from JavaScript
        try
        {
            await _jsRuntime.InvokeVoidAsync(Constants.NotificationJsFunctions.UnregisterDotNetRef);
        }
        catch
        {
            // Ignore errors during disposal
        }

        _dotNetRef?.Dispose();
        _initSemaphore.Dispose();
    }
}
