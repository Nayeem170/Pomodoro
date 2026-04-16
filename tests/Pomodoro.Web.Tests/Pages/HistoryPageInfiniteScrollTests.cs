using Bunit;
using FluentAssertions;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Xunit;

namespace Pomodoro.Web.Tests.Pages;

/// <summary>
/// Tests for History page infinite scroll functionality and edge cases
/// </summary>
[Trait("Category", "Page")]
public class HistoryPageInfiniteScrollTests : TestHelper
{
    [Fact]
    public async Task OnSentinelIntersecting_DoesNotLoad_WhenCallbackInProgress()
    {
        // Arrange
        var activities = TestHelper.CreateTestActivities(25);

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
            .ReturnsAsync(25);

        StatisticsServiceMock
            .Setup(x => x.GetWeeklyStatsAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(new WeeklyStats());

        HistoryStatsServiceMock
            .Setup(x => x.CalculateStats(It.IsAny<List<ActivityRecord>>()))
            .Returns(new DailyStatsSummary());

        // Act
        var cut = RenderComponent<Pomodoro.Web.Pages.History>();
        await Task.Delay(100);

        var historyBase = cut.Instance;

        // Call OnSentinelIntersecting twice concurrently
        var task1 = historyBase.OnSentinelIntersecting();
        var task2 = historyBase.OnSentinelIntersecting();

        await Task.WhenAll(task1, task2);

        // Assert
        // GetActivitiesPagedAsync should be called at most twice (initial load + one concurrent call)
        ActivityServiceMock.Verify(x => x.GetActivitiesPagedAsync(
            It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()), Times.AtMost(2));
    }
}
