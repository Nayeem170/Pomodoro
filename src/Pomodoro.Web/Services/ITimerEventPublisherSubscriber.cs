namespace Pomodoro.Web.Services;

public interface ITimerEventPublisherSubscriber
{
    void HandleTimerTick();
    void HandleTimerStateChanged();
}
