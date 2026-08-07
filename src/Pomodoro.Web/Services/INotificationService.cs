using Pomodoro.Web.Models;

namespace Pomodoro.Web.Services;

public interface INotificationService
{
    event Action<string>? OnNotificationAction;

    Task InitializeAsync();

    Task<bool> RequestPermissionAsync();

    Task ShowNotificationAsync(string title, string body, SessionType sessionType, string? icon = null);

    Task PlayTimerCompleteSoundAsync();

    Task PlayBreakCompleteSoundAsync();

    bool IsNotificationPermitted { get; }

    Task RefreshPermissionStateAsync();
}
