using Pomodoro.Web.Models;

namespace Pomodoro.Web.Services;

/// <summary>
/// Interface for services that publish timer events
/// </summary>
public interface ITimerEventPublisher
{
    event Func<TimerCompletedEventArgs, Task>? OnTimerCompleted;
    event Action? OnTimerStateChanged;
    event Action? OnTick;
}
