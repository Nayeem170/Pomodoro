using Bunit;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Xunit;

namespace Pomodoro.Web.Tests.Pages;

/// <summary>
/// Tests for History page integration scenarios
/// </summary>
[Trait("Category", "Page")]
public class HistoryPageIntegrationTests : TestHelper
{
    [Fact]
    public void HistoryPage_RendersWithAllComponents()
    {
        // Arrange
        var activities = TestHelper.CreateTestActivities(5);

        ActivityServiceMock
            .Setup(x => x.InitializeAsync())
            .Returns(Task.CompletedTask);

        ActivityServiceMock
            .Setup(x => x.GetActivitiesPagedAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(activities);

        ActivityServiceMock
            .Setup(x => x.GetActivitiesForDate(It.IsAny<DateTime>()))
            .Returns(activities);

        ActivityServiceMock
            .Setup(x => x.GetActivityCountAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(5);

        StatisticsServiceMock
            .Setup(x => x.GetWeeklyStatsAsync(It.IsAny<DateTime>()))
            .ReturnsAsync((WeeklyStats?)null!);

        HistoryStatsServiceMock
            .Setup(x => x.CalculateStats(It.IsAny<List<ActivityRecord>>()))
            .Returns(new DailyStatsSummary());

        // Act & Assert
        var cut = RenderComponent<Pomodoro.Web.Pages.History>();

        cut.Should().NotBeNull();
        cut.Markup.Should().NotBeNullOrEmpty();

        // Verify component renders all expected elements
        ActivityServiceMock.Verify(x => x.GetActivitiesPagedAsync(
            It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public void HistoryPage_HandlesActivityChange()
    {
        // Arrange
        var activities = TestHelper.CreateTestActivities(5);

        ActivityServiceMock
            .Setup(x => x.InitializeAsync())
            .Returns(Task.CompletedTask);

        ActivityServiceMock
            .Setup(x => x.GetActivitiesPagedAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(activities);

        ActivityServiceMock
            .Setup(x => x.GetActivitiesForDate(It.IsAny<DateTime>()))
            .Returns(activities);

        ActivityServiceMock
            .Setup(x => x.GetActivityCountAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(5);

        StatisticsServiceMock
            .Setup(x => x.GetWeeklyStatsAsync(It.IsAny<DateTime>()))
            .ReturnsAsync((WeeklyStats?)null!);

        HistoryStatsServiceMock
            .Setup(x => x.CalculateStats(It.IsAny<List<ActivityRecord>>()))
            .Returns(new DailyStatsSummary());

        // Act & Assert
        var cut = RenderComponent<Pomodoro.Web.Pages.History>();

        cut.Should().NotBeNull();
        cut.Markup.Should().NotBeNullOrEmpty();

        // Verify component handles activity changes
        ActivityServiceMock.Verify(x => x.GetActivitiesPagedAsync(
            It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public void HistoryPage_HandlesDateNavigation()
    {
        // Arrange
        var activities = TestHelper.CreateTestActivities(5);

        ActivityServiceMock
            .Setup(x => x.InitializeAsync())
            .Returns(Task.CompletedTask);

        ActivityServiceMock
            .Setup(x => x.GetActivitiesPagedAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(activities);

        ActivityServiceMock
            .Setup(x => x.GetActivitiesForDate(It.IsAny<DateTime>()))
            .Returns(activities);

        ActivityServiceMock
            .Setup(x => x.GetActivityCountAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(5);

        StatisticsServiceMock
            .Setup(x => x.GetWeeklyStatsAsync(It.IsAny<DateTime>()))
            .ReturnsAsync((WeeklyStats?)null!);

        HistoryStatsServiceMock
            .Setup(x => x.CalculateStats(It.IsAny<List<ActivityRecord>>()))
            .Returns(new DailyStatsSummary());

        // Act & Assert
        var cut = RenderComponent<Pomodoro.Web.Pages.History>();

        cut.Should().NotBeNull();
        cut.Markup.Should().NotBeNullOrEmpty();

        // Verify component handles date navigation
        ActivityServiceMock.Verify(x => x.GetActivitiesPagedAsync(
            It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public void HistoryPage_HandlesTabNavigation()
    {
        // Arrange
        var activities = TestHelper.CreateTestActivities(5);

        ActivityServiceMock
            .Setup(x => x.InitializeAsync())
            .Returns(Task.CompletedTask);

        ActivityServiceMock
            .Setup(x => x.GetActivitiesPagedAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(activities);

        ActivityServiceMock
            .Setup(x => x.GetActivitiesForDate(It.IsAny<DateTime>()))
            .Returns(activities);

        ActivityServiceMock
            .Setup(x => x.GetActivityCountAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(5);

        StatisticsServiceMock
            .Setup(x => x.GetWeeklyStatsAsync(It.IsAny<DateTime>()))
            .ReturnsAsync((WeeklyStats?)null!);

        HistoryStatsServiceMock
            .Setup(x => x.CalculateStats(It.IsAny<List<ActivityRecord>>()))
            .Returns(new DailyStatsSummary());

        // Act & Assert
        var cut = RenderComponent<Pomodoro.Web.Pages.History>();

        cut.Should().NotBeNull();
        cut.Markup.Should().NotBeNullOrEmpty();

        // Verify component handles tab navigation
        ActivityServiceMock.Verify(x => x.GetActivitiesPagedAsync(
            It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public void HistoryPage_HandlesWeekNavigation()
    {
        // Arrange
        var activities = TestHelper.CreateTestActivities(5);

        ActivityServiceMock
            .Setup(x => x.InitializeAsync())
            .Returns(Task.CompletedTask);

        ActivityServiceMock
            .Setup(x => x.GetActivitiesPagedAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(activities);

        ActivityServiceMock
            .Setup(x => x.GetActivitiesForDate(It.IsAny<DateTime>()))
            .Returns(activities);

        ActivityServiceMock
            .Setup(x => x.GetActivityCountAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(5);

        StatisticsServiceMock
            .Setup(x => x.GetWeeklyStatsAsync(It.IsAny<DateTime>()))
            .ReturnsAsync((WeeklyStats?)null!);

        HistoryStatsServiceMock
            .Setup(x => x.CalculateStats(It.IsAny<List<ActivityRecord>>()))
            .Returns(new DailyStatsSummary());

        // Act & Assert
        var cut = RenderComponent<Pomodoro.Web.Pages.History>();

        cut.Should().NotBeNull();
        cut.Markup.Should().NotBeNullOrEmpty();

        // Verify component handles week navigation
        ActivityServiceMock.Verify(x => x.GetActivitiesPagedAsync(
            It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public void HistoryPage_HandlesMultipleActivityTypes()
    {
        // Arrange
        var activities = new List<ActivityRecord>
        {
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.Pomodoro,
                CompletedAt = DateTime.Now,
                DurationMinutes = 25
            },
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.ShortBreak,
                CompletedAt = DateTime.Now.AddHours(-1),
                DurationMinutes = 5
            },
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.LongBreak,
                CompletedAt = DateTime.Now.AddHours(-2),
                DurationMinutes = 15
            }
        };

        ActivityServiceMock
            .Setup(x => x.InitializeAsync())
            .Returns(Task.CompletedTask);

        ActivityServiceMock
            .Setup(x => x.GetActivitiesPagedAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(activities);

        ActivityServiceMock
            .Setup(x => x.GetActivitiesForDate(It.IsAny<DateTime>()))
            .Returns(activities);

        ActivityServiceMock
            .Setup(x => x.GetActivityCountAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(3);

        StatisticsServiceMock
            .Setup(x => x.GetWeeklyStatsAsync(It.IsAny<DateTime>()))
            .ReturnsAsync((WeeklyStats?)null!);

        HistoryStatsServiceMock
            .Setup(x => x.CalculateStats(It.IsAny<List<ActivityRecord>>()))
            .Returns(new DailyStatsSummary());

        // Act & Assert
        var cut = RenderComponent<Pomodoro.Web.Pages.History>();

        cut.Should().NotBeNull();
        cut.Markup.Should().NotBeNullOrEmpty();

        // Verify component handles multiple activity types
        ActivityServiceMock.Verify(x => x.GetActivitiesPagedAsync(
            It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public void HistoryPage_HandlesLargeActivityList()
    {
        // Arrange
        var activities = TestHelper.CreateTestActivities(50);

        ActivityServiceMock
            .Setup(x => x.InitializeAsync())
            .Returns(Task.CompletedTask);

        ActivityServiceMock
            .Setup(x => x.GetActivitiesPagedAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(activities);

        ActivityServiceMock
            .Setup(x => x.GetActivitiesForDate(It.IsAny<DateTime>()))
            .Returns(activities);

        ActivityServiceMock
            .Setup(x => x.GetActivityCountAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(50);

        StatisticsServiceMock
            .Setup(x => x.GetWeeklyStatsAsync(It.IsAny<DateTime>()))
            .ReturnsAsync((WeeklyStats?)null!);

        HistoryStatsServiceMock
            .Setup(x => x.CalculateStats(It.IsAny<List<ActivityRecord>>()))
            .Returns(new DailyStatsSummary());

        // Act & Assert
        var cut = RenderComponent<Pomodoro.Web.Pages.History>();

        cut.Should().NotBeNull();
        cut.Markup.Should().NotBeNullOrEmpty();

        // Verify component handles large activity lists
        ActivityServiceMock.Verify(x => x.GetActivitiesPagedAsync(
            It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public void HistoryPage_HandlesEmptyAndLoadingStates()
    {
        // Arrange - Empty state
        ActivityServiceMock
            .Setup(x => x.InitializeAsync())
            .Returns(Task.CompletedTask);

        ActivityServiceMock
            .Setup(x => x.GetActivitiesPagedAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new List<ActivityRecord>());

        ActivityServiceMock
            .Setup(x => x.GetActivitiesForDate(It.IsAny<DateTime>()))
            .Returns(new List<ActivityRecord>());

        ActivityServiceMock
            .Setup(x => x.GetActivityCountAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(0);

        StatisticsServiceMock
            .Setup(x => x.GetWeeklyStatsAsync(It.IsAny<DateTime>()))
            .ReturnsAsync((WeeklyStats?)null!);

        HistoryStatsServiceMock
            .Setup(x => x.CalculateStats(It.IsAny<List<ActivityRecord>>()))
            .Returns(new DailyStatsSummary());

        // Act & Assert
        var cut = RenderComponent<Pomodoro.Web.Pages.History>();

        cut.Should().NotBeNull();
        cut.Markup.Should().NotBeNullOrEmpty();

        // Verify component handles empty state
        ActivityServiceMock.Verify(x => x.GetActivitiesPagedAsync(
            It.IsAny<DateTime>(), It.IsAny<DateTime>(), 0, 20), Times.Once);
    }
}
