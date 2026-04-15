using Pomodoro.Web.Models;

namespace Pomodoro.Web.Services;

public interface IStatisticsService
{
    Task<WeeklyStats> GetWeeklyStatsAsync(DateTime weekStartDate);
}
