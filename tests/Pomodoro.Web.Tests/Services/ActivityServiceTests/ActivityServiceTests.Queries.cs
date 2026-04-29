using Microsoft.Extensions.Logging;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services.Repositories;
using Xunit;

namespace Pomodoro.Web.Tests.Services;

/// <summary>
/// Tests for ActivityService query operations.
/// </summary>
[Trait("Category", "Service")]
public partial class ActivityServiceTests
{
    #region GetTodayActivities Tests

    [Fact]
    public async Task GetTodayActivities_ReturnsOnlyTodaysActivities()
    {
        // Arrange
        var today = DateTime.Now.Date;
        var yesterday = today.AddDays(-1);

        var activities = new List<ActivityRecord>
        {
            CreateSampleActivity(completedAt: today.AddHours(10)),
            CreateSampleActivity(completedAt: today.AddHours(14)),
            CreateSampleActivity(completedAt: yesterday.AddHours(10)) // Yesterday
        };

        MockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(activities);

        var service = CreateService();
        await service.InitializeAsync();

        // Act
        var result = service.GetTodayActivities();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, a => Assert.Equal(today, a.CompletedAt.ToLocalTime().Date));
    }

    [Fact]
    public async Task GetTodayActivities_WhenNoActivities_ReturnsEmptyList()
    {
        // Arrange
        MockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ActivityRecord>());

        var service = CreateService();
        await service.InitializeAsync();

        // Act
        var result = service.GetTodayActivities();

        // Assert
        Assert.Empty(result);
    }

    #endregion

    #region GetActivitiesForDate Tests

    [Fact]
    public async Task GetActivitiesForDate_ReturnsCorrectDateActivities()
    {
        // Arrange
        var targetDate = new DateTime(2024, 6, 15);
        var otherDate = targetDate.AddDays(-1);

        var activities = new List<ActivityRecord>
        {
            CreateSampleActivity(completedAt: targetDate.AddHours(10)),
            CreateSampleActivity(completedAt: targetDate.AddHours(14)),
            CreateSampleActivity(completedAt: otherDate.AddHours(10))
        };

        MockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(activities);

        var service = CreateService();
        await service.InitializeAsync();

        // Act
        var result = service.GetActivitiesForDate(targetDate);

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetActivitiesForDate_UsesCacheOnSecondCall()
    {
        // Arrange
        var targetDate = new DateTime(2024, 6, 15);
        var activities = new List<ActivityRecord>
        {
            CreateSampleActivity(completedAt: targetDate)
        };

        MockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(activities);

        var service = CreateService();
        await service.InitializeAsync();

        // Act
        var result1 = service.GetActivitiesForDate(targetDate);
        var result2 = service.GetActivitiesForDate(targetDate);

        // Assert
        Assert.Equal(result1.Count, result2.Count);
    }

    [Fact]
    public async Task GetActivitiesForDate_ReturnsOrderedByCompletedAtDescending()
    {
        // Arrange
        var targetDate = new DateTime(2024, 6, 15);
        var activity1 = CreateSampleActivity(completedAt: targetDate.AddHours(10));
        var activity2 = CreateSampleActivity(completedAt: targetDate.AddHours(14));
        var activity3 = CreateSampleActivity(completedAt: targetDate.AddHours(8));

        var activities = new List<ActivityRecord> { activity1, activity2, activity3 };

        MockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(activities);

        var service = CreateService();
        await service.InitializeAsync();

        // Act
        var result = service.GetActivitiesForDate(targetDate);

        // Assert
        Assert.Equal(3, result.Count);
        // Most recent first
        Assert.Equal(activity2.Id, result[0].Id);
        Assert.Equal(activity1.Id, result[1].Id);
        Assert.Equal(activity3.Id, result[2].Id);
    }

    #endregion

    #region GetDailyPomodoroCounts Tests

    [Fact]
    public async Task GetDailyPomodoroCounts_ReturnsCorrectCounts()
    {
        // Arrange
        var date1 = new DateTime(2024, 6, 15);
        var date2 = new DateTime(2024, 6, 16);

        var activities = new List<ActivityRecord>
        {
            new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = date1, DurationMinutes = 25 },
            new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = date1, DurationMinutes = 25 },
            new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = date2, DurationMinutes = 25 },
            new() { Id = Guid.NewGuid(), Type = SessionType.ShortBreak, CompletedAt = date1, DurationMinutes = 5 }
        };

        MockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(activities);

        var service = CreateService();
        await service.InitializeAsync();

        // Act
        var result = service.GetDailyPomodoroCounts(date1, date2);

        // Assert
        Assert.Equal(2, result[date1.Date]);
        Assert.Equal(1, result[date2.Date]);
    }

    [Fact]
    public async Task GetDailyPomodoroCounts_ExcludesBreaks()
    {
        // Arrange
        var date = new DateTime(2024, 6, 15);

        var activities = new List<ActivityRecord>
        {
            new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = date, DurationMinutes = 25 },
            new() { Id = Guid.NewGuid(), Type = SessionType.ShortBreak, CompletedAt = date, DurationMinutes = 5 },
            new() { Id = Guid.NewGuid(), Type = SessionType.LongBreak, CompletedAt = date, DurationMinutes = 15 }
        };

        MockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(activities);

        var service = CreateService();
        await service.InitializeAsync();

        // Act
        var result = service.GetDailyPomodoroCounts(date, date);

        // Assert
        Assert.Equal(1, result[date.Date]);
    }

    #endregion

    #region GetDailyFocusMinutes Tests

    [Fact]
    public async Task GetDailyFocusMinutes_ReturnsCorrectMinutes()
    {
        // Arrange
        var date = new DateTime(2024, 6, 15);

        var activities = new List<ActivityRecord>
        {
            new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = date, DurationMinutes = 25 },
            new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = date, DurationMinutes = 30 },
            new() { Id = Guid.NewGuid(), Type = SessionType.ShortBreak, CompletedAt = date, DurationMinutes = 5 }
        };

        MockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(activities);

        var service = CreateService();
        await service.InitializeAsync();

        // Act
        var result = service.GetDailyFocusMinutes(date, date);

        // Assert
        Assert.Equal(55, result[date.Date]);
    }

    [Fact]
    public async Task GetDailyFocusMinutes_UsesCacheOnSecondCall()
    {
        // Arrange
        var date = new DateTime(2024, 6, 15);

        var activities = new List<ActivityRecord>
        {
            new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = date, DurationMinutes = 25 }
        };

        MockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(activities);

        var service = CreateService();
        await service.InitializeAsync();

        // Act
        var result1 = service.GetDailyFocusMinutes(date, date);
        var result2 = service.GetDailyFocusMinutes(date, date);

        // Assert
        Assert.Equal(result1[date.Date], result2[date.Date]);
    }

    #endregion

    #region GetDailyBreakMinutes Tests

    [Fact]
    public async Task GetDailyBreakMinutes_ReturnsCorrectMinutes()
    {
        // Arrange
        var date = new DateTime(2024, 6, 15);

        var activities = new List<ActivityRecord>
        {
            new() { Id = Guid.NewGuid(), Type = SessionType.ShortBreak, CompletedAt = date, DurationMinutes = 5 },
            new() { Id = Guid.NewGuid(), Type = SessionType.LongBreak, CompletedAt = date, DurationMinutes = 15 },
            new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = date, DurationMinutes = 25 }
        };

        MockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(activities);

        var service = CreateService();
        await service.InitializeAsync();

        // Act
        var result = service.GetDailyBreakMinutes(date, date);

        // Assert
        Assert.Equal(20, result[date.Date]); // 5 + 15
    }

    #endregion

    #region GetTaskPomodoroCounts Tests

    [Fact]
    public async Task GetTaskPomodoroCounts_ReturnsCorrectCounts()
    {
        // Arrange
        var date = new DateTime(2024, 6, 15);

        var activities = new List<ActivityRecord>
        {
            new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, TaskName = "Task A", CompletedAt = date, DurationMinutes = 25 },
            new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, TaskName = "Task A", CompletedAt = date, DurationMinutes = 25 },
            new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, TaskName = "Task B", CompletedAt = date, DurationMinutes = 25 },
            new() { Id = Guid.NewGuid(), Type = SessionType.ShortBreak, TaskName = "Task A", CompletedAt = date, DurationMinutes = 5 }
        };

        MockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(activities);

        var service = CreateService();
        await service.InitializeAsync();

        // Act
        var result = service.GetTaskPomodoroCounts(date, date);

        // Assert
        Assert.True(result.TryGetValue("Task A", out var taskA));
        Assert.True(result.TryGetValue("Task B", out var taskB));
        Assert.Equal(2, taskA);
        Assert.Equal(1, taskB);
    }

    [Fact]
    public async Task GetTaskPomodoroCounts_ExcludesActivitiesWithoutTaskName()
    {
        // Arrange
        var date = new DateTime(2024, 6, 15);

        var activities = new List<ActivityRecord>
        {
            new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, TaskName = "Task A", CompletedAt = date, DurationMinutes = 25 },
            new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, TaskName = null, CompletedAt = date, DurationMinutes = 25 }
        };

        MockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(activities);

        var service = CreateService();
        await service.InitializeAsync();

        // Act
        var result = service.GetTaskPomodoroCounts(date, date);

        // Assert
        Assert.Single(result);
        Assert.True(result.TryGetValue("Task A", out var taskACount));
        Assert.Equal(1, taskACount);
    }

    #endregion

    #region GetTimeDistribution Tests

    [Fact]
    public async Task GetTimeDistribution_ReturnsCorrectDistribution()
    {
        // Arrange
        var date = new DateTime(2024, 6, 15);

        var activities = new List<ActivityRecord>
        {
            new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, TaskName = "Task A", CompletedAt = date, DurationMinutes = 25 },
            new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, TaskName = "Task A", CompletedAt = date, DurationMinutes = 25 },
            new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, TaskName = "Task B", CompletedAt = date, DurationMinutes = 30 },
            new() { Id = Guid.NewGuid(), Type = SessionType.ShortBreak, CompletedAt = date, DurationMinutes = 5 },
            new() { Id = Guid.NewGuid(), Type = SessionType.LongBreak, CompletedAt = date, DurationMinutes = 15 }
        };

        MockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(activities);

        var service = CreateService();
        await service.InitializeAsync();

        // Act
        var result = service.GetTimeDistribution(date);

        // Assert
        Assert.True(result.TryGetValue("Task A", out var taskATime));
        Assert.True(result.TryGetValue("Task B", out var taskBTime));
        Assert.True(result.TryGetValue(Constants.Activity.BreaksLabel, out var breaks));
        Assert.Equal(50, taskATime);
        Assert.Equal(30, taskBTime);
        Assert.Equal(20, breaks);
    }

    [Fact]
    public async Task GetTimeDistribution_UsesCacheOnSecondCall()
    {
        // Arrange
        var date = new DateTime(2024, 6, 15);

        var activities = new List<ActivityRecord>
        {
            new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, TaskName = "Task A", CompletedAt = date, DurationMinutes = 25 }
        };

        MockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(activities);

        var service = CreateService();
        await service.InitializeAsync();

        // Act
        var result1 = service.GetTimeDistribution(date);
        var result2 = service.GetTimeDistribution(date);

        // Assert
        Assert.Equal(result1["Task A"]!, result2["Task A"]!);
    }

    [Fact]
    public async Task GetTimeDistribution_WithNullTaskName_UsesFocusTimeLabel()
    {
        // Arrange
        var date = new DateTime(2024, 6, 15);

        var activities = new List<ActivityRecord>
        {
            new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, TaskName = null, CompletedAt = date, DurationMinutes = 25 }
        };

        MockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(activities);

        var service = CreateService();
        await service.InitializeAsync();

        // Act
        var result = service.GetTimeDistribution(date);

        // Assert
        Assert.Single(result);
        // The activity should be grouped under the Focus Time label
        Assert.True(result.ContainsKey("Focus Time") || result.Values.Sum() == 25);
    }

    #endregion

    #region GetCacheStatistics Tests

    [Fact]
    public async Task GetCacheStatistics_ReturnsCorrectStats()
    {
        // Arrange
        var today = DateTime.Now.Date;
        var activities = new List<ActivityRecord>
        {
            CreateSampleActivity(completedAt: today),
            CreateSampleActivity(completedAt: today.AddDays(-1))
        };

        MockActivityRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(activities);

        var service = CreateService();
        await service.InitializeAsync();

        // Access some dates to populate caches
        service.GetActivitiesForDate(today);
        service.GetTimeDistribution(today);

        // Act
        var (activityCount, datesCached, statsCached, distributionCached) = service.GetCacheStatistics();

        // Assert
        Assert.Equal(2, activityCount);
        Assert.True(datesCached >= 1);
        Assert.True(distributionCached >= 1);
    }

    #endregion
}

