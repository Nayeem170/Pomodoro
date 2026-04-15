using Microsoft.Extensions.Logging;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Pomodoro.Web.Services.Repositories;
using Xunit;

namespace Pomodoro.Web.Tests.Services;

/// <summary>
/// Tests for ActivityService.GetDailyBreakMinutes edge cases and uncovered branches
/// </summary>
[Trait("Category", "Service")]
public partial class ActivityServiceTests
{
    [Trait("Category", "Service")]
    public class GetDailyBreakMinutesTests : ActivityServiceTests
    {
        [Fact]
        public async Task GetDailyBreakMinutes_WithCachedZeroBreakMinutes_DoesNotIncludeInResult()
        {
            // Arrange
            var date1 = new DateTime(2024, 6, 15);
            var date2 = new DateTime(2024, 6, 16);
            
            // Create activities with only pomodoros (no breaks)
            var activities = new List<ActivityRecord>
            {
                new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = date1, DurationMinutes = 25 },
                new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = date2, DurationMinutes = 25 }
            };
            
            MockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(activities);
            
            var service = CreateService();
            await service.InitializeAsync();
            
            // Act - First call will compute and cache zero break minutes
            var result1 = service.GetDailyBreakMinutes(date1, date2);
            
            // Second call should use cached values (which are zero)
            var result2 = service.GetDailyBreakMinutes(date1, date2);
            
            // Assert - Results should be empty (no dates with break minutes)
            Assert.Empty(result1);
            Assert.Empty(result2);
        }

        [Fact]
        public async Task GetDailyBreakMinutes_WithMixedBreakAndZeroCountDates_IncludesOnlyNonZeroDates()
        {
            // Arrange
            var date1 = new DateTime(2024, 6, 15);
            var date2 = new DateTime(2024, 6, 16);
            var date3 = new DateTime(2024, 6, 17);
            
            // Create activities: date1 has breaks, date2 has only pomodoros, date3 has breaks
            var activities = new List<ActivityRecord>
            {
                new() { Id = Guid.NewGuid(), Type = SessionType.ShortBreak, CompletedAt = date1, DurationMinutes = 5 },
                new() { Id = Guid.NewGuid(), Type = SessionType.LongBreak, CompletedAt = date1, DurationMinutes = 15 },
                new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = date2, DurationMinutes = 25 },
                new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = date2, DurationMinutes = 25 },
                new() { Id = Guid.NewGuid(), Type = SessionType.ShortBreak, CompletedAt = date3, DurationMinutes = 5 }
            };
            
            MockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(activities);
            
            var service = CreateService();
            await service.InitializeAsync();
            
            // Act
            var result = service.GetDailyBreakMinutes(date1, date3);
            
            // Assert - Should include only dates with breaks
            Assert.Equal(2, result.Count);
            Assert.Equal(20, result[date1.Date]); // 5 + 15
            Assert.Equal(5, result[date3.Date]);
            Assert.False(result.ContainsKey(date2.Date)); // Date with zero break minutes should not be included
        }

        [Fact]
        public async Task GetDailyBreakMinutes_WithEmptyDateRange_ReturnsEmptyDictionary()
        {
            // Arrange
            var date = new DateTime(2024, 6, 15);
            
            var activities = new List<ActivityRecord>();
            MockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(activities);
            
            var service = CreateService();
            await service.InitializeAsync();
            
            // Act
            var result = service.GetDailyBreakMinutes(date, date);
            
            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetDailyBreakMinutes_WithSingleDayAndNoBreaks_ReturnsEmptyDictionary()
        {
            // Arrange
            var date = new DateTime(2024, 6, 15);
            
            // Create activities with only pomodoros
            var activities = new List<ActivityRecord>
            {
                new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = date, DurationMinutes = 25 },
                new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = date, DurationMinutes = 25 }
            };
            
            MockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(activities);
            
            var service = CreateService();
            await service.InitializeAsync();
            
            // Act
            var result = service.GetDailyBreakMinutes(date, date);
            
            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetDailyBreakMinutes_WithSingleDayAndBreaks_ReturnsCorrectMinutes()
        {
            // Arrange
            var date = new DateTime(2024, 6, 15);
            
            // Create activities with breaks
            var activities = new List<ActivityRecord>
            {
                new() { Id = Guid.NewGuid(), Type = SessionType.ShortBreak, CompletedAt = date, DurationMinutes = 5 },
                new() { Id = Guid.NewGuid(), Type = SessionType.LongBreak, CompletedAt = date, DurationMinutes = 15 },
                new() { Id = Guid.NewGuid(), Type = SessionType.ShortBreak, CompletedAt = date, DurationMinutes = 5 }
            };
            
            MockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(activities);
            
            var service = CreateService();
            await service.InitializeAsync();
            
            // Act
            var result = service.GetDailyBreakMinutes(date, date);
            
            // Assert
            Assert.Single(result);
            Assert.Equal(25, result[date.Date]); // 5 + 15 + 5
        }

        [Fact]
        public async Task GetDailyBreakMinutes_WithLargeDateRange_ComputesAllDates()
        {
            // Arrange
            var startDate = new DateTime(2024, 6, 1);
            var endDate = new DateTime(2024, 6, 30);
            
            // Create activities on specific dates
            var activities = new List<ActivityRecord>
            {
                new() { Id = Guid.NewGuid(), Type = SessionType.ShortBreak, CompletedAt = startDate, DurationMinutes = 5 },
                new() { Id = Guid.NewGuid(), Type = SessionType.LongBreak, CompletedAt = startDate.AddDays(5), DurationMinutes = 15 },
                new() { Id = Guid.NewGuid(), Type = SessionType.ShortBreak, CompletedAt = startDate.AddDays(10), DurationMinutes = 5 },
                new() { Id = Guid.NewGuid(), Type = SessionType.LongBreak, CompletedAt = startDate.AddDays(15), DurationMinutes = 15 },
                new() { Id = Guid.NewGuid(), Type = SessionType.ShortBreak, CompletedAt = startDate.AddDays(20), DurationMinutes = 5 },
                new() { Id = Guid.NewGuid(), Type = SessionType.LongBreak, CompletedAt = endDate, DurationMinutes = 15 }
            };
            
            MockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(activities);
            
            var service = CreateService();
            await service.InitializeAsync();
            
            // Act
            var result = service.GetDailyBreakMinutes(startDate, endDate);
            
            // Assert - Should have 6 dates with breaks
            Assert.Equal(6, result.Count);
            Assert.Equal(5, result[startDate.Date]);
            Assert.Equal(15, result[startDate.AddDays(5).Date]);
            Assert.Equal(5, result[startDate.AddDays(10).Date]);
            Assert.Equal(15, result[startDate.AddDays(15).Date]);
            Assert.Equal(5, result[startDate.AddDays(20).Date]);
            Assert.Equal(15, result[endDate.Date]);
        }

        [Fact]
        public async Task GetDailyBreakMinutes_WithActivitiesOutsideRange_DoesNotIncludeThem()
        {
            // Arrange
            var date1 = new DateTime(2024, 6, 15);
            var date2 = new DateTime(2024, 6, 16);
            
            // Create activities: some inside range, some outside
            var activities = new List<ActivityRecord>
            {
                new() { Id = Guid.NewGuid(), Type = SessionType.ShortBreak, CompletedAt = date1, DurationMinutes = 5 },
                new() { Id = Guid.NewGuid(), Type = SessionType.ShortBreak, CompletedAt = date1.AddDays(-1), DurationMinutes = 5 }, // Before range
                new() { Id = Guid.NewGuid(), Type = SessionType.LongBreak, CompletedAt = date2, DurationMinutes = 15 },
                new() { Id = Guid.NewGuid(), Type = SessionType.ShortBreak, CompletedAt = date2.AddDays(1), DurationMinutes = 5 }  // After range
            };
            
            MockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(activities);
            
            var service = CreateService();
            await service.InitializeAsync();
            
            // Act
            var result = service.GetDailyBreakMinutes(date1, date2);
            
            // Assert - Should only include dates within range
            Assert.Equal(2, result.Count);
            Assert.Equal(5, result[date1.Date]);
            Assert.Equal(15, result[date2.Date]);
            Assert.False(result.ContainsKey(date1.AddDays(-1).Date));
            Assert.False(result.ContainsKey(date2.AddDays(1).Date));
        }

        [Fact]
        public async Task GetDailyBreakMinutes_WithShortAndLongBreaks_CombinesCorrectly()
        {
            // Arrange
            var date = new DateTime(2024, 6, 15);
            
            // Create activities with both short and long breaks
            var activities = new List<ActivityRecord>
            {
                new() { Id = Guid.NewGuid(), Type = SessionType.ShortBreak, CompletedAt = date, DurationMinutes = 5 },
                new() { Id = Guid.NewGuid(), Type = SessionType.ShortBreak, CompletedAt = date, DurationMinutes = 5 },
                new() { Id = Guid.NewGuid(), Type = SessionType.LongBreak, CompletedAt = date, DurationMinutes = 15 },
                new() { Id = Guid.NewGuid(), Type = SessionType.LongBreak, CompletedAt = date, DurationMinutes = 15 }
            };
            
            MockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(activities);
            
            var service = CreateService();
            await service.InitializeAsync();
            
            // Act
            var result = service.GetDailyBreakMinutes(date, date);
            
            // Assert - Should combine both short and long breaks
            Assert.Single(result);
            Assert.Equal(40, result[date.Date]); // 5 + 5 + 15 + 15
        }
    }
}

