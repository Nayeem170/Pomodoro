using Microsoft.Extensions.Logging;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Pomodoro.Web.Services.Repositories;
using Xunit;

namespace Pomodoro.Web.Tests.Services;

[Trait("Category", "Service")]
public class StatisticsServiceTests
{
    private readonly Mock<IActivityRepository> _mockRepository;
    private readonly Mock<ILogger<StatisticsService>> _mockLogger;

    public StatisticsServiceTests()
    {
        _mockRepository = new Mock<IActivityRepository>();
        _mockLogger = new Mock<ILogger<StatisticsService>>();
    }

    private StatisticsService CreateService()
    {
        return new StatisticsService(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetWeeklyStatsAsync_WhenCurrentWeekRepositoryThrowsButPreviousWeekSucceeds_HandlesException()
    {
        var weekStartDate = new DateTime(2024, 6, 15);

        _mockRepository
            .SetupSequence(r => r.GetByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ThrowsAsync(new InvalidOperationException("Current week repository error"))
            .ReturnsAsync(new List<ActivityRecord>());

        var service = CreateService();
        var result = await service.GetWeeklyStatsAsync(weekStartDate);

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
        var weekStartDate = new DateTime(2024, 6, 15);

        var currentWeekActivities = new List<ActivityRecord>
        {
            new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = weekStartDate.AddDays(1), DurationMinutes = 25, TaskName = "Task 1" }
        };

        _mockRepository
            .SetupSequence(r => r.GetByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(currentWeekActivities)
            .ThrowsAsync(new InvalidOperationException("Previous week repository error"));

        var service = CreateService();
        var result = await service.GetWeeklyStatsAsync(weekStartDate);

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

        _mockRepository
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

    [Fact]
    public async Task GetWeeklyStatsAsync_WithPomodoroActivities_ReturnsCorrectStats()
    {
        var weekStart = new DateTime(2024, 6, 15);

        var currentWeekActivities = new List<ActivityRecord>
        {
            new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = weekStart.AddDays(1), DurationMinutes = 25, TaskName = "Task A" },
            new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = weekStart.AddDays(2), DurationMinutes = 25, TaskName = "Task A" },
            new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = weekStart.AddDays(3), DurationMinutes = 50, TaskName = "Task B" },
            new() { Id = Guid.NewGuid(), Type = SessionType.ShortBreak, CompletedAt = weekStart.AddDays(1), DurationMinutes = 5 }
        };

        var previousWeekActivities = new List<ActivityRecord>
        {
            new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = weekStart.AddDays(-6), DurationMinutes = 50, TaskName = "Task C" }
        };

        _mockRepository
            .SetupSequence(r => r.GetByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(currentWeekActivities)
            .ReturnsAsync(previousWeekActivities);

        var service = CreateService();
        var result = await service.GetWeeklyStatsAsync(weekStart);

        Assert.Equal(100, result.TotalFocusMinutes);
        Assert.Equal(3, result.TotalPomodoroCount);
        Assert.Equal(2, result.UniqueTasksWorkedOn);
        Assert.Equal(50, result.PreviousWeekFocusMinutes);
        Assert.Equal(100.0, result.WeekOverWeekChange);
    }

    [Fact]
    public async Task GetWeeklyStatsAsync_WithNoActivities_ReturnsZeroStats()
    {
        var weekStart = new DateTime(2024, 6, 15);

        _mockRepository
            .SetupSequence(r => r.GetByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<ActivityRecord>())
            .ReturnsAsync(new List<ActivityRecord>());

        var service = CreateService();
        var result = await service.GetWeeklyStatsAsync(weekStart);

        Assert.Equal(0, result.TotalFocusMinutes);
        Assert.Equal(0, result.TotalPomodoroCount);
        Assert.Equal(0, result.UniqueTasksWorkedOn);
        Assert.Equal(0, result.DailyAverageMinutes);
        Assert.Equal(0, result.PreviousWeekFocusMinutes);
        Assert.Equal(0, result.WeekOverWeekChange);
    }
}
