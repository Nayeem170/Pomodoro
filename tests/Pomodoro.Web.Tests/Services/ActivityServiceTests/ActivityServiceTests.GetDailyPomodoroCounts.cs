using Microsoft.Extensions.Logging;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Pomodoro.Web.Services.Repositories;
using Xunit;

namespace Pomodoro.Web.Tests.Services;

/// <summary>
/// Tests for ActivityService.GetDailyPomodoroCounts edge cases and uncovered branches
/// </summary>
[Trait("Category", "Service")]
public partial class ActivityServiceTests
{
    public class GetDailyPomodoroCountsTests : ActivityServiceTests
    {
        [Fact]
        public async Task GetDailyPomodoroCounts_WithCachedZeroPomodoroCount_DoesNotIncludeInResult()
        {
            // Arrange
            var date1 = new DateTime(2024, 6, 15);
            var date2 = new DateTime(2024, 6, 16);

            // Create activities with only breaks (no pomodoros)
            var activities = new List<ActivityRecord>
            {
                new() { Id = Guid.NewGuid(), Type = SessionType.ShortBreak, CompletedAt = date1, DurationMinutes = 5 },
                new() { Id = Guid.NewGuid(), Type = SessionType.LongBreak, CompletedAt = date2, DurationMinutes = 15 }
            };

            MockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(activities);

            var service = CreateService();
            await service.InitializeAsync();

            // Act - First call will compute and cache zero pomodoro counts
            var result1 = service.GetDailyPomodoroCounts(date1, date2);

            // Second call should use cached values (which are zero)
            var result2 = service.GetDailyPomodoroCounts(date1, date2);

            // Assert - Results should be empty (no dates with pomodoros)
            Assert.Empty(result1);
            Assert.Empty(result2);
        }

        [Fact]
        public async Task GetDailyPomodoroCounts_WithMixedPomodoroAndZeroCountDates_IncludesOnlyNonZeroDates()
        {
            // Arrange
            var date1 = new DateTime(2024, 6, 15);
            var date2 = new DateTime(2024, 6, 16);
            var date3 = new DateTime(2024, 6, 17);

            // Create activities: date1 has pomodoros, date2 has only breaks, date3 has pomodoros
            var activities = new List<ActivityRecord>
            {
                new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = date1, DurationMinutes = 25 },
                new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = date1, DurationMinutes = 25 },
                new() { Id = Guid.NewGuid(), Type = SessionType.ShortBreak, CompletedAt = date2, DurationMinutes = 5 },
                new() { Id = Guid.NewGuid(), Type = SessionType.LongBreak, CompletedAt = date2, DurationMinutes = 15 },
                new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = date3, DurationMinutes = 25 }
            };

            MockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(activities);

            var service = CreateService();
            await service.InitializeAsync();

            // Act
            var result = service.GetDailyPomodoroCounts(date1, date3);

            // Assert - Should include only dates with pomodoros
            Assert.Equal(2, result.Count);
            Assert.Equal(2, result[date1.Date]);
            Assert.Equal(1, result[date3.Date]);
            Assert.False(result.ContainsKey(date2.Date)); // Date with zero pomodoros should not be included
        }

        [Fact]
        public async Task GetDailyPomodoroCounts_WithEmptyDateRange_ReturnsEmptyDictionary()
        {
            // Arrange
            var date = new DateTime(2024, 6, 15);

            var activities = new List<ActivityRecord>();
            MockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(activities);

            var service = CreateService();
            await service.InitializeAsync();

            // Act
            var result = service.GetDailyPomodoroCounts(date, date);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetDailyPomodoroCounts_WithSingleDayAndNoPomodoros_ReturnsEmptyDictionary()
        {
            // Arrange
            var date = new DateTime(2024, 6, 15);

            // Create activities with only breaks
            var activities = new List<ActivityRecord>
            {
                new() { Id = Guid.NewGuid(), Type = SessionType.ShortBreak, CompletedAt = date, DurationMinutes = 5 },
                new() { Id = Guid.NewGuid(), Type = SessionType.LongBreak, CompletedAt = date, DurationMinutes = 15 }
            };

            MockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(activities);

            var service = CreateService();
            await service.InitializeAsync();

            // Act
            var result = service.GetDailyPomodoroCounts(date, date);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetDailyPomodoroCounts_WithSingleDayAndPomodoros_ReturnsCorrectCount()
        {
            // Arrange
            var date = new DateTime(2024, 6, 15);

            // Create activities with pomodoros
            var activities = new List<ActivityRecord>
            {
                new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = date, DurationMinutes = 25 },
                new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = date, DurationMinutes = 25 },
                new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = date, DurationMinutes = 30 }
            };

            MockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(activities);

            var service = CreateService();
            await service.InitializeAsync();

            // Act
            var result = service.GetDailyPomodoroCounts(date, date);

            // Assert
            Assert.Single(result);
            Assert.Equal(3, result[date.Date]);
        }

        [Fact]
        public async Task GetDailyPomodoroCounts_WithLargeDateRange_ComputesAllDates()
        {
            // Arrange
            var startDate = new DateTime(2024, 6, 1);
            var endDate = new DateTime(2024, 6, 30);

            // Create activities on specific dates
            var activities = new List<ActivityRecord>
            {
                new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = startDate, DurationMinutes = 25 },
                new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = startDate.AddDays(5), DurationMinutes = 25 },
                new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = startDate.AddDays(10), DurationMinutes = 25 },
                new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = startDate.AddDays(15), DurationMinutes = 25 },
                new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = startDate.AddDays(20), DurationMinutes = 25 },
                new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = endDate, DurationMinutes = 25 }
            };

            MockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(activities);

            var service = CreateService();
            await service.InitializeAsync();

            // Act
            var result = service.GetDailyPomodoroCounts(startDate, endDate);

            // Assert - Should have 6 dates with pomodoros
            Assert.Equal(6, result.Count);
            Assert.Equal(1, result[startDate.Date]);
            Assert.Equal(1, result[startDate.AddDays(5).Date]);
            Assert.Equal(1, result[startDate.AddDays(10).Date]);
            Assert.Equal(1, result[startDate.AddDays(15).Date]);
            Assert.Equal(1, result[startDate.AddDays(20).Date]);
            Assert.Equal(1, result[endDate.Date]);
        }

        [Fact]
        public async Task GetDailyPomodoroCounts_WithActivitiesOutsideRange_DoesNotIncludeThem()
        {
            // Arrange
            var date1 = new DateTime(2024, 6, 15);
            var date2 = new DateTime(2024, 6, 16);

            // Create activities: some inside range, some outside
            var activities = new List<ActivityRecord>
            {
                new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = date1, DurationMinutes = 25 },
                new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = date1.AddDays(-1), DurationMinutes = 25 }, // Before range
                new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = date2, DurationMinutes = 25 },
                new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = date2.AddDays(1), DurationMinutes = 25 }  // After range
            };

            MockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(activities);

            var service = CreateService();
            await service.InitializeAsync();

            // Act
            var result = service.GetDailyPomodoroCounts(date1, date2);

            // Assert - Should only include dates within range
            Assert.Equal(2, result.Count);
            Assert.Equal(1, result[date1.Date]);
            Assert.Equal(1, result[date2.Date]);
            Assert.False(result.ContainsKey(date1.AddDays(-1).Date));
            Assert.False(result.ContainsKey(date2.AddDays(1).Date));
        }
    }
}

