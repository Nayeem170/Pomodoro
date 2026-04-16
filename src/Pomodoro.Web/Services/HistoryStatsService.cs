using Pomodoro.Web.Models;

namespace Pomodoro.Web.Services;

/// <summary>
/// Service for calculating history page statistics
/// </summary>
public class HistoryStatsService : IHistoryStatsService
{
    /// <summary>
    /// Calculates daily statistics from a list of activities
    /// </summary>
    /// <param name="activities">List of activities to calculate stats for</param>
    /// <returns>Daily statistics summary</returns>
    public virtual DailyStatsSummary CalculateStats(List<ActivityRecord> activities)
    {
        var pomodoros = activities.Where(a => a.Type == SessionType.Pomodoro).ToList();

        return new DailyStatsSummary
        {
            PomodoroCount = pomodoros.Count,
            FocusMinutes = pomodoros.Sum(a => a.DurationMinutes),
            TasksWorkedOn = pomodoros
                .Where(a => a.TaskId.HasValue)
                .Select(a => a.TaskId!.Value)
                .Distinct()
                .Count()
        };
    }
}
