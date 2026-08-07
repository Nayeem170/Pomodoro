namespace Pomodoro.Web.Services;

public interface IHistoryStatsService
{
    /// <param name="activities">List of activities to calculate statistics for</param>
    /// <returns>Daily statistics summary</returns>
    Models.DailyStatsSummary CalculateStats(List<Models.ActivityRecord> activities);
}
