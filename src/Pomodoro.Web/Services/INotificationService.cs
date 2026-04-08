using Pomodoro.Web.Models;

namespace Pomodoro.Web.Services;

/// <summary>
/// Service for handling browser notifications and sound alerts
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Event raised when a notification action is clicked
    /// </summary>
    event Action<string>? OnNotificationAction;
    
    /// <summary>
    /// Initializes the notification service and requests permission
    /// </summary>
    Task InitializeAsync();
    
    /// <summary>
    /// Requests notification permission from the browser
    /// </summary>
    Task<bool> RequestPermissionAsync();
    
    /// <summary>
    /// Shows a browser notification with session type for appropriate actions
    /// </summary>
    Task ShowNotificationAsync(string title, string body, SessionType sessionType, string? icon = null);
    
    /// <summary>
    /// Plays the timer complete sound
    /// </summary>
    Task PlayTimerCompleteSoundAsync();
    
    /// <summary>
    /// Plays a break complete sound
    /// </summary>
    Task PlayBreakCompleteSoundAsync();
    
    /// <summary>
    /// Checks if notifications are supported and permitted
    /// </summary>
    bool IsNotificationPermitted { get; }
    
    /// <summary>
    /// Refreshes the notification permission state from the browser.
    /// Call this when settings page opens to ensure UI shows current permission status.
    /// </summary>
    Task RefreshPermissionStateAsync();
}
