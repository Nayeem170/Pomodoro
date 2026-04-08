using Microsoft.Extensions.Logging;
using Pomodoro.Web.Services;

namespace Pomodoro.Web.Services;

/// <summary>
/// Service for wiring up event subscribers to timer events.
/// Extracted from Program.cs for testability.
/// </summary>
public interface IEventWiringService
{
    void WireEventSubscribers(IServiceProvider serviceProvider);
}

public class EventWiringService : IEventWiringService
{
    private readonly ILogger<EventWiringService> _logger;

    public EventWiringService(ILogger<EventWiringService> logger)
    {
        _logger = logger;
    }

    public void WireEventSubscribers(IServiceProvider serviceProvider)
    {
        // Get service references
        var timerService = serviceProvider.GetRequiredService<ITimerService>();
        var taskService = serviceProvider.GetService<ITaskService>();
        var activityService = serviceProvider.GetService<IActivityService>();

        // Wire up event subscribers to timer publisher
        if (timerService is ITimerEventPublisher timerPublisher)
        {
            // Subscribe TaskService to timer events
            if (taskService is ITimerEventSubscriber taskSubscriber)
            {
                timerPublisher.OnTimerCompleted += taskSubscriber.HandleTimerCompletedAsync;
            }

            // Subscribe ActivityService to timer events
            if (activityService is ITimerEventSubscriber activitySubscriber)
            {
                timerPublisher.OnTimerCompleted += activitySubscriber.HandleTimerCompletedAsync;
            }
        }

        _logger.LogInformation("Event subscribers wired up successfully");
    }
}
