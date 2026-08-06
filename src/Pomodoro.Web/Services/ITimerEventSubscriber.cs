using Pomodoro.Web.Models;

namespace Pomodoro.Web.Services;

public interface ITimerEventSubscriber
{
    Task HandleTimerCompletedAsync(TimerCompletedEventArgs args);
}
