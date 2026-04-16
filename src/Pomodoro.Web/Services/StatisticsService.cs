using Microsoft.Extensions.Logging;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services.Repositories;

namespace Pomodoro.Web.Services;

public class StatisticsService : IStatisticsService
{
    private readonly IActivityRepository _activityRepository;
    private readonly ILogger<StatisticsService> _logger;

    public StatisticsService(IActivityRepository activityRepository, ILogger<StatisticsService> logger)
    {
        _activityRepository = activityRepository;
        _logger = logger;
    }

    public async Task<WeeklyStats> GetWeeklyStatsAsync(DateTime weekStartDate)
    {
        try
        {
            var weekStart = weekStartDate.Date;
            var weekEnd = weekStart.AddDays(7);
            var previousWeekStart = weekStart.AddDays(-7);

            var currentWeekActivities = await _activityRepository.GetByDateRangeAsync(weekStart, weekEnd);
            var previousWeekActivities = await _activityRepository.GetByDateRangeAsync(previousWeekStart, weekStart);

            if (!currentWeekActivities.Any())
            {
                return new WeeklyStats
                {
                    TotalFocusMinutes = 0,
                    TotalPomodoroCount = 0,
                    UniqueTasksWorkedOn = 0,
                    DailyAverageMinutes = 0,
                    MostProductiveDay = DayOfWeek.Monday,
                    PreviousWeekFocusMinutes = 0,
                    WeekOverWeekChange = 0
                };
            }

            var pomodoroActivities = currentWeekActivities.Where(a => a.Type == SessionType.Pomodoro).ToList();
            var totalFocusMinutes = pomodoroActivities.Sum(a => a.DurationMinutes);
            var totalPomodoroCount = pomodoroActivities.Count;
            var uniqueTasks = pomodoroActivities
                .Where(a => !string.IsNullOrEmpty(a.TaskName))
                .Select(a => a.TaskName)
                .Distinct()
                .Count();

            var daysWithActivity = currentWeekActivities
                .Select(a => a.CompletedAt.ToLocalTime().Date)
                .Distinct()
                .Count();
            var dailyAverageMinutes = (double)totalFocusMinutes / daysWithActivity;

            var dayTotals = pomodoroActivities
                .GroupBy(a => a.CompletedAt.ToLocalTime().DayOfWeek)
                .Select(g => new { Day = g.Key, Minutes = g.Sum(a => a.DurationMinutes) })
                .OrderByDescending(g => g.Minutes)
                .FirstOrDefault();
            var mostProductiveDay = dayTotals?.Day ?? DayOfWeek.Monday;

            var previousWeekFocusMinutes = previousWeekActivities
                .Where(a => a.Type == SessionType.Pomodoro)
                .Sum(a => a.DurationMinutes);

            double weekOverWeekChange = 0;
            if (previousWeekFocusMinutes > 0)
            {
                weekOverWeekChange = ((double)(totalFocusMinutes - previousWeekFocusMinutes) / previousWeekFocusMinutes) * 100;
            }
            else if (totalFocusMinutes > 0)
            {
                weekOverWeekChange = 100;
            }

            return new WeeklyStats
            {
                TotalFocusMinutes = totalFocusMinutes,
                TotalPomodoroCount = totalPomodoroCount,
                UniqueTasksWorkedOn = uniqueTasks,
                DailyAverageMinutes = dailyAverageMinutes,
                MostProductiveDay = mostProductiveDay,
                PreviousWeekFocusMinutes = previousWeekFocusMinutes,
                WeekOverWeekChange = weekOverWeekChange
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Constants.Messages.LogWeeklyStatsError, weekStartDate);
            return new WeeklyStats
            {
                TotalFocusMinutes = 0,
                TotalPomodoroCount = 0,
                UniqueTasksWorkedOn = 0,
                DailyAverageMinutes = 0,
                MostProductiveDay = DayOfWeek.Monday,
                PreviousWeekFocusMinutes = 0,
                WeekOverWeekChange = 0
            };
        }
    }
}
