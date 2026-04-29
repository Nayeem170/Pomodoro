using Microsoft.Extensions.Logging;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Pomodoro.Web.Services.Repositories;
using Xunit;

namespace Pomodoro.Web.Tests.Services;

[Trait("Category", "Service")]
public class StatisticsServiceCoverageTests
{
    private readonly Mock<IActivityRepository> _mockRepository;
    private readonly Mock<ILogger<StatisticsService>> _mockLogger;

    public StatisticsServiceCoverageTests()
    {
        _mockRepository = new Mock<IActivityRepository>();
        _mockLogger = new Mock<ILogger<StatisticsService>>();
    }

    private StatisticsService CreateService()
    {
        return new StatisticsService(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetWeeklyStatsAsync_WhenPreviousWeekEmptyButCurrentHasFocus_WeekOverWeekChangeIs100()
    {
        var weekStart = new DateTime(2024, 6, 15);

        var currentWeekActivities = new List<ActivityRecord>
        {
            new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = weekStart.AddDays(1), DurationMinutes = 100, TaskName = "Task A" }
        };

        _mockRepository
            .SetupSequence(r => r.GetByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(currentWeekActivities)
            .ReturnsAsync(new List<ActivityRecord>());

        var service = CreateService();
        var result = await service.GetWeeklyStatsAsync(weekStart);

        Assert.Equal(100.0, result.WeekOverWeekChange);
    }

    [Fact]
    public async Task GetWeeklyStatsAsync_WhenBothWeeksEmpty_WeekOverWeekChangeIs0()
    {
        var weekStart = new DateTime(2024, 6, 15);

        _mockRepository
            .SetupSequence(r => r.GetByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<ActivityRecord>())
            .ReturnsAsync(new List<ActivityRecord>());

        var service = CreateService();
        var result = await service.GetWeeklyStatsAsync(weekStart);

        Assert.Equal(0, result.WeekOverWeekChange);
    }

    [Fact]
    public async Task GetWeeklyStatsAsync_WhenBothWeeksHaveSameFocus_WeekOverWeekChangeIs0()
    {
        var weekStart = new DateTime(2024, 6, 15);

        var activities = new List<ActivityRecord>
        {
            new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = weekStart.AddDays(1), DurationMinutes = 50, TaskName = "Task" }
        };

        _mockRepository
            .SetupSequence(r => r.GetByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(activities)
            .ReturnsAsync(activities);

        var service = CreateService();
        var result = await service.GetWeeklyStatsAsync(weekStart);

        Assert.Equal(0.0, result.WeekOverWeekChange);
    }

    [Fact]
    public async Task GetWeeklyStatsAsync_WhenCurrentWeekFocusDecreased_WeekOverWeekChangeIsNegative()
    {
        var weekStart = new DateTime(2024, 6, 15);

        var currentWeekActivities = new List<ActivityRecord>
        {
            new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = weekStart.AddDays(1), DurationMinutes = 25, TaskName = "Task" }
        };
        var previousWeekActivities = new List<ActivityRecord>
        {
            new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = weekStart.AddDays(-6), DurationMinutes = 50, TaskName = "Task" }
        };

        _mockRepository
            .SetupSequence(r => r.GetByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(currentWeekActivities)
            .ReturnsAsync(previousWeekActivities);

        var service = CreateService();
        var result = await service.GetWeeklyStatsAsync(weekStart);

        Assert.True(result.WeekOverWeekChange < 0);
    }
}
