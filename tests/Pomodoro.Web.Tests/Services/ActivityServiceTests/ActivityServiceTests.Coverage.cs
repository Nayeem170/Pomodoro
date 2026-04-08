using Microsoft.Extensions.Logging;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Pomodoro.Web.Services.Repositories;
using Xunit;

namespace Pomodoro.Web.Tests.Services;

public partial class ActivityServiceTests
{
    public class CacheHitCoverageTests : ActivityServiceTests
    {
        [Fact]
        public async Task GetDailyPomodoroCounts_SecondCallUsesCacheWithPositiveCount()
        {
            var date = new DateTime(2024, 6, 15);
            var activities = new List<ActivityRecord>
            {
                new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = date, DurationMinutes = 25 },
                new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = date, DurationMinutes = 25 }
            };

            MockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(activities);

            var service = CreateService();
            await service.InitializeAsync();

            var result1 = service.GetDailyPomodoroCounts(date, date);
            Assert.Equal(2, result1[date.Date]);

            var result2 = service.GetDailyPomodoroCounts(date, date);
            Assert.Equal(2, result2[date.Date]);
        }

        [Fact]
        public async Task GetDailyBreakMinutes_SecondCallUsesCacheWithPositiveMinutes()
        {
            var date = new DateTime(2024, 6, 15);
            var activities = new List<ActivityRecord>
            {
                new() { Id = Guid.NewGuid(), Type = SessionType.ShortBreak, CompletedAt = date, DurationMinutes = 5 },
                new() { Id = Guid.NewGuid(), Type = SessionType.LongBreak, CompletedAt = date, DurationMinutes = 15 }
            };

            MockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(activities);

            var service = CreateService();
            await service.InitializeAsync();

            var result1 = service.GetDailyBreakMinutes(date, date);
            Assert.Equal(20, result1[date.Date]);

            var result2 = service.GetDailyBreakMinutes(date, date);
            Assert.Equal(20, result2[date.Date]);
        }
    }

    public class WeeklyStatsCoverageTests : ActivityServiceTests
    {
        [Fact]
        public async Task GetWeeklyStatsAsync_WhenBothWeeksHaveData_CalculatesWeekOverWeekChange()
        {
            var weekStart = new DateTime(2024, 6, 15);
            var previousWeekStart = weekStart.AddDays(-7);

            var currentWeekActivities = new List<ActivityRecord>
            {
                new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = weekStart.AddDays(1), DurationMinutes = 50, TaskName = "Task 1" },
                new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = weekStart.AddDays(2), DurationMinutes = 50, TaskName = "Task 2" }
            };

            var previousWeekActivities = new List<ActivityRecord>
            {
                new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = previousWeekStart.AddDays(1), DurationMinutes = 50, TaskName = "Task A" }
            };

            MockActivityRepository
                .SetupSequence(r => r.GetByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(currentWeekActivities)
                .ReturnsAsync(previousWeekActivities);

            var service = CreateService();

            var result = await service.GetWeeklyStatsAsync(weekStart);

            Assert.Equal(100, result.TotalFocusMinutes);
            Assert.Equal(2, result.TotalPomodoroCount);
            Assert.Equal(50, result.PreviousWeekFocusMinutes);
            Assert.Equal(100.0, result.WeekOverWeekChange);
        }
    }
}
