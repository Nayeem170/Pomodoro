using Bunit;
using FluentAssertions;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Xunit;

namespace Pomodoro.Web.Tests.Pages;

/// <summary>
/// Tests for History page event handlers and edge cases during state changes
/// </summary>
[Trait("Category", "Page")]
public class HistoryPageEventHandlersTests : TestHelper
{
    [Fact]
    public void OnActivityChanged_ReloadsData()
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
        
        // Act
        var cut = RenderComponent<Pomodoro.Web.Pages.History>();
        
        // Trigger the OnActivityChanged event
        ActivityServiceMock.Raise(x => x.OnActivityChanged += null);
        
        // Assert
        // GetActivitiesPagedAsync should be called twice (initial load + reload on event)
        ActivityServiceMock.Verify(x => x.GetActivitiesPagedAsync(
            It.IsAny<DateTime>(), It.IsAny<DateTime>(), 0, 20), Times.Exactly(2));
    }
    
    [Fact]
    public void OnActivityChanged_TriggeredMultipleTimes()
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
        
        // Act
        var cut = RenderComponent<Pomodoro.Web.Pages.History>();
        
        // Trigger the OnActivityChanged event multiple times
        ActivityServiceMock.Raise(x => x.OnActivityChanged += null);
        ActivityServiceMock.Raise(x => x.OnActivityChanged += null);
        ActivityServiceMock.Raise(x => x.OnActivityChanged += null);
        
        // Assert
        // GetActivitiesPagedAsync should be called 4 times (initial load + 3 reloads)
        ActivityServiceMock.Verify(x => x.GetActivitiesPagedAsync(
            It.IsAny<DateTime>(), It.IsAny<DateTime>(), 0, 20), Times.Exactly(4));
    }
    
    [Fact]
    public void OnActivityChanged_WithDifferentDates()
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
        
        // Act
        var cut = RenderComponent<Pomodoro.Web.Pages.History>();
        
        // Trigger the OnActivityChanged event
        ActivityServiceMock.Raise(x => x.OnActivityChanged += null);
        
        // Assert
        // GetActivitiesPagedAsync should be called twice (initial load + reload on event)
        ActivityServiceMock.Verify(x => x.GetActivitiesPagedAsync(
            It.IsAny<DateTime>(), It.IsAny<DateTime>(), 0, 20), Times.Exactly(2));
        
        // GetWeeklyStatsAsync should also be called twice
        StatisticsServiceMock.Verify(x => x.GetWeeklyStatsAsync(It.IsAny<DateTime>()), Times.Exactly(2));
    }
}

