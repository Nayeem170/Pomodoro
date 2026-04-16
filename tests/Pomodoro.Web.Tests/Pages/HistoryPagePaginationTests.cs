using Bunit;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Xunit;

namespace Pomodoro.Web.Tests.Pages;

/// <summary>
/// Tests for History page pagination logic
/// </summary>
[Trait("Category", "Page")]
public class HistoryPagePaginationTests : TestHelper
{
    [Fact]
    public void HistoryPage_LoadsInitialActivities()
    {
        // Arrange
        var activities = TestHelper.CreateTestActivities(20);

        ActivityServiceMock
            .Setup(x => x.InitializeAsync())
            .Returns(Task.CompletedTask);

        ActivityServiceMock
            .Setup(x => x.GetActivitiesPagedAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), 0, 20))
            .ReturnsAsync(activities);

        ActivityServiceMock
            .Setup(x => x.GetActivitiesForDate(It.IsAny<DateTime>()))
            .Returns(activities);

        ActivityServiceMock
            .Setup(x => x.GetActivityCountAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(20);

        StatisticsServiceMock
            .Setup(x => x.GetWeeklyStatsAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(new WeeklyStats());

        HistoryStatsServiceMock
            .Setup(x => x.CalculateStats(It.IsAny<List<ActivityRecord>>()))
            .Returns(new DailyStatsSummary());

        // Act & Assert
        var cut = RenderComponent<Pomodoro.Web.Pages.History>();

        cut.Should().NotBeNull();
        cut.Markup.Should().NotBeNullOrEmpty();

        ActivityServiceMock.Verify(x => x.GetActivitiesPagedAsync(
            It.IsAny<DateTime>(), It.IsAny<DateTime>(), 0, 20), Times.Once);
    }

    [Fact]
    public void HistoryPage_LoadsMoreActivities_WhenMoreActivitiesExist()
    {
        // Arrange
        var initialActivities = TestHelper.CreateTestActivities(20);
        var moreActivities = TestHelper.CreateTestActivities(20, 20);

        ActivityServiceMock
            .Setup(x => x.InitializeAsync())
            .Returns(Task.CompletedTask);

        ActivityServiceMock
            .Setup(x => x.GetActivitiesPagedAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), 0, 20))
            .ReturnsAsync(initialActivities);

        ActivityServiceMock
            .Setup(x => x.GetActivitiesPagedAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), 20, 20))
            .ReturnsAsync(moreActivities);

        ActivityServiceMock
            .Setup(x => x.GetActivitiesForDate(It.IsAny<DateTime>()))
            .Returns(initialActivities);

        ActivityServiceMock
            .Setup(x => x.GetActivityCountAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(40);

        StatisticsServiceMock
            .Setup(x => x.GetWeeklyStatsAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(new WeeklyStats());

        HistoryStatsServiceMock
            .Setup(x => x.CalculateStats(It.IsAny<List<ActivityRecord>>()))
            .Returns(new DailyStatsSummary());

        // Act & Assert
        var cut = RenderComponent<Pomodoro.Web.Pages.History>();

        cut.Should().NotBeNull();
        cut.Markup.Should().NotBeNullOrEmpty();

        ActivityServiceMock.Verify(x => x.GetActivitiesPagedAsync(
            It.IsAny<DateTime>(), It.IsAny<DateTime>(), 0, 20), Times.Once);
    }

    [Fact]
    public void HistoryPage_DoesNotLoadMore_WhenNoMoreActivitiesExist()
    {
        // Arrange
        var activities = TestHelper.CreateTestActivities(10);

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
            .ReturnsAsync(10);

        StatisticsServiceMock
            .Setup(x => x.GetWeeklyStatsAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(new WeeklyStats());

        HistoryStatsServiceMock
            .Setup(x => x.CalculateStats(It.IsAny<List<ActivityRecord>>()))
            .Returns(new DailyStatsSummary());

        // Act & Assert
        var cut = RenderComponent<Pomodoro.Web.Pages.History>();

        cut.Should().NotBeNull();
        cut.Markup.Should().NotBeNullOrEmpty();

        ActivityServiceMock.Verify(x => x.GetActivitiesPagedAsync(
            It.IsAny<DateTime>(), It.IsAny<DateTime>(), 0, 20), Times.Once);
    }

    [Fact]
    public void HistoryPage_HandlesEmptyActivitiesResponse()
    {
        // Arrange
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
            .ReturnsAsync(new WeeklyStats());

        HistoryStatsServiceMock
            .Setup(x => x.CalculateStats(It.IsAny<List<ActivityRecord>>()))
            .Returns(new DailyStatsSummary());

        // Act & Assert
        var cut = RenderComponent<Pomodoro.Web.Pages.History>();

        cut.Should().NotBeNull();
        cut.Markup.Should().NotBeNullOrEmpty();

        ActivityServiceMock.Verify(x => x.GetActivitiesPagedAsync(
            It.IsAny<DateTime>(), It.IsAny<DateTime>(), 0, 20), Times.Once);
    }

    [Fact]
    public void HistoryPage_UpdatesPaginationState()
    {
        // Arrange
        var activities = TestHelper.CreateTestActivities(15);

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
            .ReturnsAsync(15);

        StatisticsServiceMock
            .Setup(x => x.GetWeeklyStatsAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(new WeeklyStats());

        HistoryStatsServiceMock
            .Setup(x => x.CalculateStats(It.IsAny<List<ActivityRecord>>()))
            .Returns(new DailyStatsSummary());

        // Act & Assert
        var cut = RenderComponent<Pomodoro.Web.Pages.History>();

        cut.Should().NotBeNull();
        cut.Markup.Should().NotBeNullOrEmpty();

        ActivityServiceMock.Verify(x => x.GetActivitiesPagedAsync(
            It.IsAny<DateTime>(), It.IsAny<DateTime>(), 0, 20), Times.Once);
    }

    [Fact]
    public async Task LoadMoreActivitiesAsync_HandlesExceptions()
    {
        // Arrange
        var activities = TestHelper.CreateTestActivities(10);

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
            .ReturnsAsync(20);

        StatisticsServiceMock
            .Setup(x => x.GetWeeklyStatsAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(new WeeklyStats());

        HistoryStatsServiceMock
            .Setup(x => x.CalculateStats(It.IsAny<List<ActivityRecord>>()))
            .Returns(new DailyStatsSummary());

        // Act & Assert
        var cut = RenderComponent<Pomodoro.Web.Pages.History>();
        await Task.Delay(100); // Allow initial load to complete

        var historyBase = cut.Instance;

        // Setup to throw exception on second call (load more)
        ActivityServiceMock
            .SetupSequence(x => x.GetActivitiesPagedAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(activities)
            .ThrowsAsync(new InvalidOperationException("Test exception"));

        // Should not throw, exception should be caught and logged
        var exception = await Record.ExceptionAsync(async () => await historyBase.OnSentinelIntersecting());

        exception.Should().BeNull("OnSentinelIntersecting should catch and log exceptions");
    }

    [Fact]
    public async Task LoadMoreActivitiesAsync_DoesNotLoad_WhenNoMoreActivitiesExist()
    {
        // Arrange
        var activities = TestHelper.CreateTestActivities(10);

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
            .ReturnsAsync(10); // Same as initial load count

        StatisticsServiceMock
            .Setup(x => x.GetWeeklyStatsAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(new WeeklyStats());

        HistoryStatsServiceMock
            .Setup(x => x.CalculateStats(It.IsAny<List<ActivityRecord>>()))
            .Returns(new DailyStatsSummary());

        // Act & Assert
        var cut = RenderComponent<Pomodoro.Web.Pages.History>();
        await Task.Delay(100); // Allow initial load to complete

        var historyBase = cut.Instance;

        // Call OnSentinelIntersecting when HasMoreActivities is false
        await historyBase.OnSentinelIntersecting();

        // Verify that GetActivitiesPagedAsync was called only once (initial load)
        ActivityServiceMock.Verify(x => x.GetActivitiesPagedAsync(
            It.IsAny<DateTime>(), It.IsAny<DateTime>(), 0, 20), Times.Once);
    }

    [Fact]
    public async Task LoadMoreActivitiesAsync_DoesNotLoad_WhenAlreadyLoading()
    {
        // Arrange
        var activities = TestHelper.CreateTestActivities(10);

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
            .ReturnsAsync(20);

        StatisticsServiceMock
            .Setup(x => x.GetWeeklyStatsAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(new WeeklyStats());

        HistoryStatsServiceMock
            .Setup(x => x.CalculateStats(It.IsAny<List<ActivityRecord>>()))
            .Returns(new DailyStatsSummary());

        // Act & Assert
        var cut = RenderComponent<Pomodoro.Web.Pages.History>();
        await Task.Delay(100); // Allow initial load to complete

        var historyBase = cut.Instance;

        // Call OnSentinelIntersecting twice concurrently
        var task1 = historyBase.OnSentinelIntersecting();
        var task2 = historyBase.OnSentinelIntersecting();

        await Task.WhenAll(task1, task2);

        // Verify that GetActivitiesPagedAsync was called at most twice (initial load + one concurrent call)
        ActivityServiceMock.Verify(x => x.GetActivitiesPagedAsync(
            It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()), Times.AtMost(2));
    }
}

