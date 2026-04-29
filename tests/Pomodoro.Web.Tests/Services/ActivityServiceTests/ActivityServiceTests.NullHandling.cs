using Microsoft.Extensions.Logging;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Xunit;

namespace Pomodoro.Web.Tests.Services;

/// <summary>
/// Tests for ActivityService null handling and edge cases
/// </summary>
[Trait("Category", "Service")]
public partial class ActivityServiceTests
{
    public class NullHandlingTests : ActivityServiceTests
    {
        [Fact]
        public async Task GetTaskPomodoroCounts_WithNullTaskNames_ExcludesThemFromResults()
        {
            // Arrange
            var date1 = new DateTime(2024, 6, 15);
            var date2 = new DateTime(2024, 6, 16);

            var activities = new List<ActivityRecord>
            {
                new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = date1, DurationMinutes = 25, TaskName = null },
                new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = date1, DurationMinutes = 25, TaskName = "Valid Task" },
                new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = date2, DurationMinutes = 25, TaskName = "" }
            };

            MockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(activities);

            var service = CreateService();
            await service.InitializeAsync();

            // Act
            var result = service.GetTaskPomodoroCounts(date1, date2);

            // Assert - Should only include "Valid Task", exclude null and empty
            Assert.Single(result);
            Assert.True(result.ContainsKey("Valid Task"));
            Assert.Equal(1, result["Valid Task"]);
        }

        [Fact]
        public async Task GetTaskPomodoroCounts_WithEmptyAndWhitespaceTaskNames_ExcludesBoth()
        {
            // Arrange
            var date = new DateTime(2024, 6, 15);

            var activities = new List<ActivityRecord>
            {
                new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = date, DurationMinutes = 25, TaskName = "" },
                new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = date, DurationMinutes = 25, TaskName = "   " },
                new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = date, DurationMinutes = 25, TaskName = "Valid Task" }
            };

            MockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(activities);

            var service = CreateService();
            await service.InitializeAsync();

            // Act
            var result = service.GetTaskPomodoroCounts(date, date);

            // Assert - Should exclude both empty and whitespace-only task names
            Assert.Single(result);
            Assert.True(result.ContainsKey("Valid Task"));
            Assert.Equal(1, result["Valid Task"]);
        }

        [Fact]
        public async Task GetTimeDistribution_WithNullTaskNames_UsesFocusTimeLabel()
        {
            // Arrange
            var date = new DateTime(2024, 6, 15);

            var activities = new List<ActivityRecord>
            {
                new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = date, DurationMinutes = 25, TaskName = null },
                new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = date, DurationMinutes = 25, TaskName = null },
                new() { Id = Guid.NewGuid(), Type = SessionType.ShortBreak, CompletedAt = date, DurationMinutes = 5 }
            };

            MockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(activities);

            var service = CreateService();
            await service.InitializeAsync();

            // Act
            var result = service.GetTimeDistribution(date);

            // Assert - Should use FocusTimeLabel for null task names
            Assert.Equal(2, result.Count);
            Assert.True(result.ContainsKey(Constants.Activity.FocusTimeLabel));
            Assert.Equal(50, result[Constants.Activity.FocusTimeLabel]);
            Assert.Equal(5, result[Constants.Activity.BreaksLabel]);
        }

        [Fact]
        public async Task GetTimeDistribution_WithWhitespaceTaskNames_KeepsWhitespaceAsKey()
        {
            // Arrange
            var date = new DateTime(2024, 6, 15);

            var activities = new List<ActivityRecord>
            {
                new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = date, DurationMinutes = 25, TaskName = "   " },
                new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = date, DurationMinutes = 25, TaskName = "\t" },
                new() { Id = Guid.NewGuid(), Type = SessionType.LongBreak, CompletedAt = date, DurationMinutes = 15 }
            };

            MockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(activities);

            var service = CreateService();
            await service.InitializeAsync();

            // Act
            var result = service.GetTimeDistribution(date);

            // Assert - Whitespace task names are kept as separate keys, not grouped under FocusTimeLabel
            Assert.Equal(3, result.Count);
            Assert.Equal(25, result["   "]);
            Assert.Equal(25, result["\t"]);
            Assert.Equal(15, result[Constants.Activity.BreaksLabel]);
        }

        [Fact]
        public async Task GetTimeDistribution_WithMixedNullAndValidTaskNames_GroupsCorrectly()
        {
            // Arrange
            var date = new DateTime(2024, 6, 15);

            var activities = new List<ActivityRecord>
            {
                new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = date, DurationMinutes = 25, TaskName = null },
                new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = date, DurationMinutes = 25, TaskName = "Task A" },
                new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = date, DurationMinutes = 25, TaskName = null },
                new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = date, DurationMinutes = 25, TaskName = "Task A" },
                new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = date, DurationMinutes = 25, TaskName = "Task B" }
            };

            MockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(activities);

            var service = CreateService();
            await service.InitializeAsync();

            // Act
            var result = service.GetTimeDistribution(date);

            // Assert - Should group null under FocusTimeLabel, and valid tasks separately
            Assert.Equal(3, result.Count);
            Assert.Equal(50, result[Constants.Activity.FocusTimeLabel]);
            Assert.Equal(50, result["Task A"]);
            Assert.Equal(25, result["Task B"]);
        }

        [Fact]
        public async Task GetTimeDistribution_WithOnlyBreaks_ReturnsOnlyBreakLabels()
        {
            // Arrange
            var date = new DateTime(2024, 6, 15);

            var activities = new List<ActivityRecord>
            {
                new() { Id = Guid.NewGuid(), Type = SessionType.ShortBreak, CompletedAt = date, DurationMinutes = 5 },
                new() { Id = Guid.NewGuid(), Type = SessionType.ShortBreak, CompletedAt = date, DurationMinutes = 5 },
                new() { Id = Guid.NewGuid(), Type = SessionType.LongBreak, CompletedAt = date, DurationMinutes = 15 }
            };

            MockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(activities);

            var service = CreateService();
            await service.InitializeAsync();

            // Act
            var result = service.GetTimeDistribution(date);

            // Assert - Should only have combined break label
            Assert.Single(result);
            Assert.Equal(25, result[Constants.Activity.BreaksLabel]);
        }

        [Fact]
        public async Task GetTimeDistribution_WithNoActivities_ReturnsEmptyDictionary()
        {
            // Arrange
            var date = new DateTime(2024, 6, 15);

            var activities = new List<ActivityRecord>();
            MockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(activities);

            var service = CreateService();
            await service.InitializeAsync();

            // Act
            var result = service.GetTimeDistribution(date);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetDailyPomodoroCounts_WithNullTaskName_CountsAllPomodoros()
        {
            // Arrange
            var date = new DateTime(2024, 6, 15);
            var startDate = date.AddDays(-1);
            var endDate = date.AddDays(1);

            var activities = new List<ActivityRecord>
            {
                new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = date, DurationMinutes = 25, TaskName = null },
                new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = date, DurationMinutes = 25, TaskName = "Valid Task" }
            };

            MockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(activities);

            var service = CreateService();
            await service.InitializeAsync();

            // Act
            var result = service.GetDailyPomodoroCounts(startDate, endDate);

            // Assert - GetDailyPomodoroCounts counts ALL pomodoro activities regardless of task name
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.True(result.ContainsKey(date));
            Assert.Equal(2, result[date]);
        }

        [Fact]
        public async Task GetDailyPomodoroCounts_WithAllNullTaskNames_CountsAllPomodoros()
        {
            // Arrange
            var date = new DateTime(2024, 6, 15);
            var startDate = date.AddDays(-1);
            var endDate = date.AddDays(1);

            var activities = new List<ActivityRecord>
            {
                new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = date, DurationMinutes = 25, TaskName = null },
                new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = date, DurationMinutes = 25, TaskName = null }
            };

            MockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(activities);

            var service = CreateService();
            await service.InitializeAsync();

            // Act
            var result = service.GetDailyPomodoroCounts(startDate, endDate);

            // Assert - GetDailyPomodoroCounts counts ALL pomodoro activities regardless of task name
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.True(result.ContainsKey(date));
            Assert.Equal(2, result[date]);
        }

        [Fact]
        public async Task GetDailyFocusMinutes_WithNullTaskNames_CountsAllFocusTime()
        {
            // Arrange
            var date = new DateTime(2024, 6, 15);
            var startDate = date.AddDays(-1);
            var endDate = date.AddDays(1);

            var activities = new List<ActivityRecord>
            {
                new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = date, DurationMinutes = 25, TaskName = null },
                new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = date, DurationMinutes = 25, TaskName = "Valid Task" }
            };

            MockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(activities);

            var service = CreateService();
            await service.InitializeAsync();

            // Act
            var result = service.GetDailyFocusMinutes(startDate, endDate);

            // Assert - GetDailyFocusMinutes counts ALL pomodoro focus time regardless of task name
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.True(result.ContainsKey(date));
            Assert.Equal(50, result[date]);
        }

        [Fact]
        public async Task GetDailyFocusMinutes_WithAllNullTaskNames_CountsAllFocusTime()
        {
            // Arrange
            var date = new DateTime(2024, 6, 15);
            var startDate = date.AddDays(-1);
            var endDate = date.AddDays(1);

            var activities = new List<ActivityRecord>
            {
                new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = date, DurationMinutes = 25, TaskName = null },
                new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = date, DurationMinutes = 25, TaskName = null }
            };

            MockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(activities);

            var service = CreateService();
            await service.InitializeAsync();

            // Act
            var result = service.GetDailyFocusMinutes(startDate, endDate);

            // Assert - GetDailyFocusMinutes counts ALL pomodoro focus time regardless of task name
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.True(result.ContainsKey(date));
            Assert.Equal(50, result[date]);
        }

        [Fact]
        public async Task GetDailyBreakMinutes_WithNullTaskNames_IncludesAllBreaks()
        {
            // Arrange
            var date = new DateTime(2024, 6, 15);
            var startDate = date.AddDays(-1);
            var endDate = date.AddDays(1);

            var activities = new List<ActivityRecord>
            {
                new() { Id = Guid.NewGuid(), Type = SessionType.ShortBreak, CompletedAt = date, DurationMinutes = 5, TaskName = null },
                new() { Id = Guid.NewGuid(), Type = SessionType.ShortBreak, CompletedAt = date, DurationMinutes = 5, TaskName = "Valid Task" },
                new() { Id = Guid.NewGuid(), Type = SessionType.LongBreak, CompletedAt = date, DurationMinutes = 15, TaskName = null }
            };

            MockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(activities);

            var service = CreateService();
            await service.InitializeAsync();

            // Act
            var result = service.GetDailyBreakMinutes(startDate, endDate);

            // Assert - Should include all breaks regardless of task name
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.True(result.ContainsKey(date));
            Assert.Equal(25, result[date]); // 5 + 5 + 15 minutes
        }

        [Fact]
        public async Task GetDailyBreakMinutes_WithOnlyBreaksAndNullTasks_ReturnsBreakMinutes()
        {
            // Arrange
            var date = new DateTime(2024, 6, 15);
            var startDate = date.AddDays(-1);
            var endDate = date.AddDays(1);

            var activities = new List<ActivityRecord>
            {
                new() { Id = Guid.NewGuid(), Type = SessionType.ShortBreak, CompletedAt = date, DurationMinutes = 5, TaskName = null },
                new() { Id = Guid.NewGuid(), Type = SessionType.ShortBreak, CompletedAt = date, DurationMinutes = 5, TaskName = null },
                new() { Id = Guid.NewGuid(), Type = SessionType.LongBreak, CompletedAt = date, DurationMinutes = 15, TaskName = null }
            };

            MockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(activities);

            var service = CreateService();
            await service.InitializeAsync();

            // Act
            var result = service.GetDailyBreakMinutes(startDate, endDate);

            // Assert - Should return break minutes even with null task names
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.True(result.ContainsKey(date));
            Assert.Equal(25, result[date]); // 5 + 5 + 15 minutes
        }

        [Fact]
        public async Task GetTaskPomodoroCounts_WithMixedNullAndValid_CountsOnlyValid()
        {
            // Arrange
            var date = new DateTime(2024, 6, 15);
            var startDate = date.AddDays(-1);
            var endDate = date.AddDays(1);

            var activities = new List<ActivityRecord>
            {
                new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = date, DurationMinutes = 25, TaskName = null },
                new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = date, DurationMinutes = 25, TaskName = "Task A" },
                new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = date, DurationMinutes = 25, TaskName = null },
                new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = date, DurationMinutes = 25, TaskName = "Task A" },
                new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = date, DurationMinutes = 25, TaskName = "Task B" }
            };

            MockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(activities);

            var service = CreateService();
            await service.InitializeAsync();

            // Act
            var result = service.GetTaskPomodoroCounts(startDate, endDate);

            // Assert - Should count only valid task names
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(2, result["Task A"]);
            Assert.Equal(1, result["Task B"]);
        }

        [Fact]
        public async Task GetTaskPomodoroCounts_WithAllNullTaskNames_ReturnsEmptyDictionary()
        {
            // Arrange
            var date = new DateTime(2024, 6, 15);
            var startDate = date.AddDays(-1);
            var endDate = date.AddDays(1);

            var activities = new List<ActivityRecord>
            {
                new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = date, DurationMinutes = 25, TaskName = null },
                new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = date, DurationMinutes = 25, TaskName = null }
            };

            MockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(activities);

            var service = CreateService();
            await service.InitializeAsync();

            // Act
            var result = service.GetTaskPomodoroCounts(startDate, endDate);

            // Assert - Should return empty dictionary when all task names are null
            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}

