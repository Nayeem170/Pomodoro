using Bunit;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Xunit;

namespace Pomodoro.Web.Tests.Pages;

/// <summary>
/// Tests for History page disposal and cleanup
/// </summary>
[Trait("Category", "Page")]
public class HistoryPageDisposeTests : TestHelper
{
    [Fact]
    public async Task DisposeAsync_CallsFallbackCleanup_WhenPrimaryCleanupFails()
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
        
        InfiniteScrollInteropMock
            .Setup(x => x.IsSupportedAsync())
            .ReturnsAsync(true);
        
        InfiniteScrollInteropMock
            .Setup(x => x.CreateObserverAsync(
                It.IsAny<string>(),
                It.IsAny<Microsoft.JSInterop.DotNetObjectReference<object>>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>()))
            .ReturnsAsync(true);
        
        InfiniteScrollInteropMock
            .Setup(x => x.DestroyObserverAsync(It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("Test exception"));
        
        InfiniteScrollInteropMock
            .Setup(x => x.DestroyAllObserversAsync())
            .Returns(Task.CompletedTask);

        // Act & Assert
        var cut = RenderComponent<Pomodoro.Web.Pages.History>();
        await Task.Delay(100); // Allow initial load to complete
        
        // Dispose should not throw
        var exception = await Record.ExceptionAsync(async () => await cut.Instance.DisposeAsync());
        
        exception.Should().BeNull("DisposeAsync should handle exceptions gracefully");
        
        // Verify fallback cleanup was called
        InfiniteScrollInteropMock.Verify(
            x => x.DestroyAllObserversAsync(),
            Times.Once);
    }

    [Fact]
    public async Task DisposeAsync_HandlesTotalCleanupFailure()
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
        
        InfiniteScrollInteropMock
            .Setup(x => x.IsSupportedAsync())
            .ReturnsAsync(true);
        
        InfiniteScrollInteropMock
            .Setup(x => x.CreateObserverAsync(
                It.IsAny<string>(),
                It.IsAny<Microsoft.JSInterop.DotNetObjectReference<object>>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>()))
            .ReturnsAsync(true);
        
        InfiniteScrollInteropMock
            .Setup(x => x.DestroyObserverAsync(It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("Test exception"));
        
        InfiniteScrollInteropMock
            .Setup(x => x.DestroyAllObserversAsync())
            .ThrowsAsync(new InvalidOperationException("Fallback also failed"));

        // Act & Assert
        var cut = RenderComponent<Pomodoro.Web.Pages.History>();
        await Task.Delay(100); // Allow initial load to complete
        
        // Dispose should not throw even when both primary and fallback cleanup fail
        var exception = await Record.ExceptionAsync(async () => await cut.Instance.DisposeAsync());
        
        exception.Should().BeNull("DisposeAsync should handle total cleanup failure gracefully");
    }
}

