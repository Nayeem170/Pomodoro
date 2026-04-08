using Microsoft.Extensions.Logging;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;

namespace Pomodoro.Web.Services;

public class IndexPagePresenterService
{
    private readonly ILogger<IndexPagePresenterService> _logger;

    public IndexPagePresenterService(ILogger<IndexPagePresenterService> logger)
    {
        _logger = logger;
    }

    public IndexPageState UpdateState(ITaskService taskService, ITimerService timerService)
    {
        try
        {
            return new IndexPageState
            {
                Tasks = taskService.Tasks?.ToList() ?? new List<TaskItem>(),
                CurrentTaskId = taskService.CurrentTaskId,
                RemainingTime = timerService.RemainingTime,
                CurrentSessionType = timerService.CurrentSessionType,
                IsTimerRunning = timerService.IsRunning,
                IsTimerPaused = timerService.IsPaused,
                IsTimerStarted = timerService.IsStarted
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in UpdateState");
            return new IndexPageState
            {
                Tasks = new List<TaskItem>(),
                RemainingTime = TimeSpan.FromMinutes(Constants.Timer.DefaultPomodoroMinutes),
                CurrentSessionType = SessionType.Pomodoro,
                IsTimerStarted = false
            };
        }
    }
}

public class IndexPageState
{
    public List<TaskItem> Tasks { get; set; } = new();
    public Guid? CurrentTaskId { get; set; }
    public TimeSpan RemainingTime { get; set; }
    public SessionType CurrentSessionType { get; set; }
    public bool IsTimerRunning { get; set; }
    public bool IsTimerPaused { get; set; }
    public bool IsTimerStarted { get; set; }
}
