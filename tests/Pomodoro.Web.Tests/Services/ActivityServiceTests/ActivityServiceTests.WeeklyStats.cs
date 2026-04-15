using Microsoft.Extensions.Logging;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Pomodoro.Web.Services.Repositories;
using Xunit;

namespace Pomodoro.Web.Tests.Services;

/// <summary>
/// Tests for ActivityService weekly statistics with exception handling.
/// </summary>
[Trait("Category", "Service")]
public partial class ActivityServiceTests
{
    [Trait("Category", "Service")]
    public class WeeklyStatsTests : ActivityServiceTests
    {
        [Fact]
        public async Task GetWeeklyStatsAsync_WhenCurrentWeekRepositoryThrowsButPreviousWeekSucceeds_HandlesException()
        {
            // Arrange
            var weekStartDate = new DateTime(2024, 6, 15);
            
            // First call (current week) throws, second call (previous week) succeeds
            MockActivityRepository
                .SetupSequence(r => r.GetByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ThrowsAsync(new InvalidOperationException("Current week repository error"))
                .ReturnsAsync(new List<ActivityRecord>());
            
            var service = CreateService();
            
            // Act
            var result = await service.GetWeeklyStatsAsync(weekStartDate);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.TotalFocusMinutes);
            Assert.Equal(0, result.TotalPomodoroCount);
            Assert.Equal(0, result.UniqueTasksWorkedOn);
            Assert.Equal(0, result.DailyAverageMinutes);
            Assert.Equal(DayOfWeek.Monday, result.MostProductiveDay);
            Assert.Equal(0, result.PreviousWeekFocusMinutes);
            Assert.Equal(0, result.WeekOverWeekChange);
        }

        [Fact]
        public async Task GetWeeklyStatsAsync_WhenPreviousWeekRepositoryThrowsButCurrentWeekSucceeds_HandlesException()
        {
            // Arrange
            var weekStartDate = new DateTime(2024, 6, 15);
            
            // Create a list of activities for the current week
            var currentWeekActivities = new List<ActivityRecord>
            {
                new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = weekStartDate.AddDays(1), DurationMinutes = 25, TaskName = "Task 1" }
            };
            
            // First call (current week) succeeds, second call (previous week) throws
            MockActivityRepository
                .SetupSequence(r => r.GetByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(currentWeekActivities)
                .ThrowsAsync(new InvalidOperationException("Previous week repository error"));
            
            var service = CreateService();
            
            // Act
            var result = await service.GetWeeklyStatsAsync(weekStartDate);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.TotalFocusMinutes);
            Assert.Equal(0, result.TotalPomodoroCount);
            Assert.Equal(0, result.UniqueTasksWorkedOn);
            Assert.Equal(0, result.DailyAverageMinutes);
            Assert.Equal(0, result.PreviousWeekFocusMinutes);
            Assert.Equal(0, result.WeekOverWeekChange);
        }

        [Fact]
        public async Task GetWeeklyStatsAsync_WhenOnlyBreakActivities_ReturnsZeroPomodoroStats()
        {
            var weekStart = new DateTime(2024, 6, 15);

            var currentWeekActivities = new List<ActivityRecord>
            {
                new() { Id = Guid.NewGuid(), Type = SessionType.ShortBreak, CompletedAt = weekStart.AddDays(1), DurationMinutes = 5 },
                new() { Id = Guid.NewGuid(), Type = SessionType.LongBreak, CompletedAt = weekStart.AddDays(2), DurationMinutes = 15 }
            };

            MockActivityRepository
                .SetupSequence(r => r.GetByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(currentWeekActivities)
                .ReturnsAsync(new List<ActivityRecord>());

            var service = CreateService();

            var result = await service.GetWeeklyStatsAsync(weekStart);

            Assert.Equal(0, result.TotalFocusMinutes);
            Assert.Equal(0, result.TotalPomodoroCount);
            Assert.Equal(0, result.UniqueTasksWorkedOn);
            Assert.Equal(0, result.DailyAverageMinutes);
            Assert.Equal(DayOfWeek.Monday, result.MostProductiveDay);
        }
    }
}
