using Pomodoro.Web.Models;

namespace Pomodoro.Web.Services;

/// <summary>
/// Interface for services that publish timer events
/// </summary>
public interface ITimerEventPublisher
{
    /// <summary>
    /// Raised when a timer session completes
    /// Subscribers should handle exceptions internally
    /// </summary>
    event Func<TimerCompletedEventArgs, Task>? OnTimerCompleted;
    
    /// <summary>
    /// Raised when timer state changes (start, pause, resume, reset)
    /// </summary>
    event Action? OnTimerStateChanged;
}
