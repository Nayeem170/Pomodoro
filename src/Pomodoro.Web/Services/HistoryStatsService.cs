using Pomodoro.Web.Models;

namespace Pomodoro.Web.Services;

public class HistoryStatsService : IHistoryStatsService
{
    /// <param name="activities">List of activities to calculate stats for</param>
    /// <returns>Daily statistics summary</returns>
    public virtual DailyStatsSummary CalculateStats(List<ActivityRecord> activities)
    {
        var pomodoros = activities.Where(a => a.Type == SessionType.Pomodoro).ToList();
        var breaks = activities.Where(a => a.Type is SessionType.ShortBreak or SessionType.LongBreak).ToList();

        return new DailyStatsSummary
        {
            PomodoroCount = pomodoros.Count,
            FocusMinutes = pomodoros.Sum(a => a.DurationMinutes),
            BreakMinutes = breaks.Sum(a => a.DurationMinutes),
            TasksWorkedOn = pomodoros
                .Where(a => a.TaskId.HasValue)
                .Select(a => a.TaskId!.Value)
                .Distinct()
                .Count()
        };
    }
}
