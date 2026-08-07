using Pomodoro.Web.Models;

namespace Pomodoro.Web.Services;

public interface ITimerEventPublisher
{
    event Func<TimerCompletedEventArgs, Task>? OnTimerCompleted;
    event Func<TimerCompletedEventArgs, Task>? OnSessionInterrupted;
    event Action? OnTimerStateChanged;
    event Action? OnTick;
}
