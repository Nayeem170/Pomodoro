namespace Pomodoro.Web.Services;

/// <summary>
/// Interface for history statistics service
/// </summary>
public interface IHistoryStatsService
{
    /// <summary>
    /// Calculate statistics from a list of activities
    /// </summary>
    /// <param name="activities">List of activities to calculate statistics for</param>
    /// <returns>Daily statistics summary</returns>
    Models.DailyStatsSummary CalculateStats(List<Models.ActivityRecord> activities);
}
