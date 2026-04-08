using Pomodoro.Web.Models;
using Pomodoro.Web.Services;

namespace Pomodoro.Web.Services;

public class TodayStatsService : ITodayStatsService
{
    private readonly IActivityService _activityService;

    public TodayStatsService(IActivityService activityService)
    {
        _activityService = activityService;
    }

    public int GetTodayTotalFocusMinutes()
    {
        return _activityService.GetTodayActivities()
            .Where(a => a.Type == SessionType.Pomodoro)
            .Sum(a => a.DurationMinutes);
    }

    public int GetTodayPomodoroCount()
    {
        return _activityService.GetTodayActivities()
            .Count(a => a.Type == SessionType.Pomodoro);
    }

    public int GetTodayTasksWorkedOn()
    {
        return _activityService.GetTodayActivities()
            .Where(a => a.Type == SessionType.Pomodoro && !string.IsNullOrWhiteSpace(a.TaskName))
            .Select(a => a.TaskName)
            .Distinct()
            .Count();
    }

    public (int TotalFocusMinutes, int PomodoroCount, int TasksWorkedOn) GetTodayStats()
    {
        var activities = _activityService.GetTodayActivities()
            .Where(a => a.Type == SessionType.Pomodoro)
            .ToList();

        return (
            activities.Sum(a => a.DurationMinutes),
            activities.Count,
            activities.Where(a => !string.IsNullOrWhiteSpace(a.TaskName))
                .Select(a => a.TaskName)
                .Distinct()
                .Count()
        );
    }
}
