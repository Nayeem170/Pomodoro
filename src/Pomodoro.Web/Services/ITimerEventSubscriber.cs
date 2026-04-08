using Pomodoro.Web.Models;

namespace Pomodoro.Web.Services;

/// <summary>
/// Interface for services that subscribe to timer events
/// </summary>
public interface ITimerEventSubscriber
{
    /// <summary>
    /// Handles timer completion events
    /// </summary>
    Task HandleTimerCompletedAsync(TimerCompletedEventArgs args);
}
