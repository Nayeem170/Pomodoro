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

    public async Task<IndexPageState> UpdateStateAsync(ITaskService taskService, ITimerService timerService, string? currentListId)
    {
        try
        {
            var listId = currentListId ?? taskService.CurrentListId ?? Constants.TaskLists.LocalPomodoroListId;
            var tasks = await taskService.GetTasksForListAsync(listId);

            return new IndexPageState
            {
                Tasks = tasks.ToList(),
                CurrentTaskId = taskService.CurrentTaskId,
                CurrentListId = listId,
                TaskLists = taskService.TaskLists,
                RemainingTime = timerService.RemainingTime,
                CurrentSessionType = timerService.CurrentSessionType,
                IsTimerRunning = timerService.IsRunning,
                IsTimerPaused = timerService.IsPaused,
                IsTimerStarted = timerService.IsStarted
            };
        }
        catch (Exception ex)
        {
            var fallbackListId = currentListId ?? taskService.CurrentListId ?? Constants.TaskLists.LocalPomodoroListId;
            _logger.LogError(ex, "Error in UpdateStateAsync");
            return new IndexPageState
            {
                Tasks = new List<TaskItem>(),
                TaskLists = taskService.TaskLists,
                RemainingTime = timerService.RemainingTime,
                CurrentSessionType = timerService.CurrentSessionType,
                IsTimerRunning = timerService.IsRunning,
                IsTimerPaused = timerService.IsPaused,
                IsTimerStarted = timerService.IsStarted,
                CurrentListId = fallbackListId
            };
        }
    }
}

public class IndexPageState
{
    public List<TaskItem> Tasks { get; set; } = new();
    public Guid? CurrentTaskId { get; set; }
    public string? CurrentListId { get; set; }
    public IReadOnlyList<TaskListRef> TaskLists { get; set; } = [];
    public TimeSpan RemainingTime { get; set; }
    public SessionType CurrentSessionType { get; set; }
    public bool IsTimerRunning { get; set; }
    public bool IsTimerPaused { get; set; }
    public bool IsTimerStarted { get; set; }
}
