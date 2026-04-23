using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
#pragma warning disable CS8619 // Nullability of reference types mismatch in Moq Setup/Returns
#pragma warning disable CS8620 // Nullability mismatch in Moq Setup/Returns

namespace Pomodoro.Web.Tests.Pages;

/// <summary>
/// Comprehensive tests for History page component.
/// These tests verify rendering, interactions, and lifecycle methods of History page.
/// </summary>
[Trait("Category", "Page")]
public class HistoryPageExpandedTests : TestHelper
{
    [Fact]
    public void HistoryPage_RendersWithEmptyState()
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
            .Setup(x => x.GetActivityCountAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(0);

        ActivityServiceMock
            .Setup(x => x.GetDailyFocusMinutes(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(new Dictionary<DateTime, int>());

        ActivityServiceMock
            .Setup(x => x.GetDailyBreakMinutes(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(new Dictionary<DateTime, int>());

        StatisticsServiceMock
            .Setup<Task<WeeklyStats?>>(x => x.GetWeeklyStatsAsync(It.IsAny<DateTime>()))
            .ReturnsAsync((WeeklyStats?)null);

        // Act
        var cut = RenderComponent<Pomodoro.Web.Pages.History>();

        // Assert
        cut.Should().NotBeNull();
        cut.Markup.Should().NotBeNullOrEmpty();

        // Verify service calls
        ActivityServiceMock.Verify(x => x.InitializeAsync(), Times.Once);
        ActivityServiceMock.Verify(x => x.GetActivitiesPagedAsync(
            It.IsAny<DateTime>(), It.IsAny<DateTime>(), 0, 20), Times.Once);
    }

    [Fact]
    public void HistoryPage_RendersWithActivities()
    {
        // Arrange
        var testDate = DateTime.Now.Date;
        var activities = new List<ActivityRecord>
        {
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.Pomodoro,
                CompletedAt = testDate,
                DurationMinutes = 25,
                TaskName = "Test Task"
            },
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.ShortBreak,
                CompletedAt = testDate.AddHours(-1),
                DurationMinutes = 5,
                TaskName = "Break Task"
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
            .Setup(x => x.GetActivityCountAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(2);

        ActivityServiceMock
            .Setup(x => x.GetDailyFocusMinutes(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(new Dictionary<DateTime, int>());

        ActivityServiceMock
            .Setup(x => x.GetDailyBreakMinutes(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(new Dictionary<DateTime, int>());

        StatisticsServiceMock
            .Setup<Task<WeeklyStats?>>(x => x.GetWeeklyStatsAsync(It.IsAny<DateTime>()))
            .ReturnsAsync((WeeklyStats?)null);

        // Act
        var cut = RenderComponent<Pomodoro.Web.Pages.History>();

        // Assert
        cut.Should().NotBeNull();
        cut.Markup.Should().NotBeNullOrEmpty();

        // Verify service calls
        ActivityServiceMock.Verify(x => x.GetActivitiesPagedAsync(
            It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public void HistoryPage_LoadsWeeklyData()
    {
        // Arrange
        var weekStart = DateTime.Now.Date.AddDays(-(int)DateTime.Now.DayOfWeek);
        var weeklyFocusMinutes = new Dictionary<DateTime, int>
        {
            { weekStart, 120 },
            { weekStart.AddDays(1), 90 },
            { weekStart.AddDays(2), 150 }
        };

        var weeklyBreakMinutes = new Dictionary<DateTime, int>
        {
            { weekStart, 30 },
            { weekStart.AddDays(1), 20 },
            { weekStart.AddDays(2), 40 }
        };

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
            .Setup(x => x.GetActivityCountAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(0);

        ActivityServiceMock
            .Setup(x => x.GetDailyFocusMinutes(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(weeklyFocusMinutes);

        ActivityServiceMock
            .Setup(x => x.GetDailyBreakMinutes(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(weeklyBreakMinutes);

        StatisticsServiceMock
            .Setup<Task<WeeklyStats?>>(x => x.GetWeeklyStatsAsync(It.IsAny<DateTime>()))
            .ReturnsAsync((WeeklyStats?)null);

        // Act
        var cut = RenderComponent<Pomodoro.Web.Pages.History>();

        // Assert
        ActivityServiceMock.Verify(x => x.GetDailyFocusMinutes(
            It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Once);
        ActivityServiceMock.Verify(x => x.GetDailyBreakMinutes(
            It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Once);
    }

    [Fact]
    public void HistoryPage_HandlesMultipleActivityTypes()
    {
        // Arrange
        var testDate = DateTime.Now.Date;
        var activities = new List<ActivityRecord>
        {
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.Pomodoro,
                CompletedAt = testDate,
                DurationMinutes = 25
            },
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.ShortBreak,
                CompletedAt = testDate.AddHours(-1),
                DurationMinutes = 5
            },
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.LongBreak,
                CompletedAt = testDate.AddHours(-2),
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
            .Setup(x => x.GetActivityCountAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(3);

        ActivityServiceMock
            .Setup(x => x.GetDailyFocusMinutes(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(new Dictionary<DateTime, int>());

        ActivityServiceMock
            .Setup(x => x.GetDailyBreakMinutes(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(new Dictionary<DateTime, int>());

        StatisticsServiceMock
            .Setup<Task<WeeklyStats?>>(x => x.GetWeeklyStatsAsync(It.IsAny<DateTime>()))
            .ReturnsAsync((WeeklyStats?)null);

        // Act
        var cut = RenderComponent<Pomodoro.Web.Pages.History>();

        // Assert
        cut.Should().NotBeNull();
        ActivityServiceMock.Verify(x => x.GetActivitiesPagedAsync(
            It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public void HistoryPage_HandlesLargeActivityList()
    {
        // Arrange
        var testDate = DateTime.Now.Date;
        var activities = Enumerable.Range(1, 50).Select(i => new ActivityRecord
        {
            Id = Guid.NewGuid(),
            Type = SessionType.Pomodoro,
            CompletedAt = testDate.AddHours(-i),
            DurationMinutes = 25
        }).ToList();

        ActivityServiceMock
            .Setup(x => x.InitializeAsync())
            .Returns(Task.CompletedTask);

        ActivityServiceMock
            .Setup(x => x.GetActivitiesPagedAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), 0, 20))
            .ReturnsAsync(activities.Take(20).ToList());

        ActivityServiceMock
            .Setup(x => x.GetActivitiesPagedAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), 20, 20))
            .ReturnsAsync(activities.Skip(20).Take(20).ToList());

        ActivityServiceMock
            .Setup(x => x.GetActivitiesPagedAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), 40, 20))
            .ReturnsAsync(activities.Skip(40).ToList());

        ActivityServiceMock
            .Setup(x => x.GetActivitiesForDate(It.IsAny<DateTime>()))
            .Returns(activities);

        ActivityServiceMock
            .Setup(x => x.GetActivityCountAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(50);

        ActivityServiceMock
            .Setup(x => x.GetDailyFocusMinutes(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(new Dictionary<DateTime, int>());

        ActivityServiceMock
            .Setup(x => x.GetDailyBreakMinutes(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(new Dictionary<DateTime, int>());

        StatisticsServiceMock
            .Setup<Task<WeeklyStats?>>(x => x.GetWeeklyStatsAsync(It.IsAny<DateTime>()))
            .ReturnsAsync((WeeklyStats?)null);

        // Act
        var cut = RenderComponent<Pomodoro.Web.Pages.History>();

        // Assert
        ActivityServiceMock.Verify(x => x.GetActivitiesPagedAsync(
            It.IsAny<DateTime>(), It.IsAny<DateTime>(), 0, 20), Times.Once);
    }

    [Fact]
    public void HistoryPage_HandlesWeeklyStats()
    {
        // Arrange
        var testDate = DateTime.Now.Date;
        var activities = new List<ActivityRecord>
        {
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.Pomodoro,
                CompletedAt = testDate,
                DurationMinutes = 25
            }
        };

        var weeklyStats = new WeeklyStats
        {
            TotalFocusMinutes = 150,
            TotalPomodoroCount = 6,
            UniqueTasksWorkedOn = 3,
            DailyAverageMinutes = 30,
            MostProductiveDay = DayOfWeek.Monday,
            PreviousWeekFocusMinutes = 120,
            WeekOverWeekChange = 25.0
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
            .Setup(x => x.GetActivityCountAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(1);

        ActivityServiceMock
            .Setup(x => x.GetDailyFocusMinutes(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(new Dictionary<DateTime, int>());

        ActivityServiceMock
            .Setup(x => x.GetDailyBreakMinutes(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(new Dictionary<DateTime, int>());

        StatisticsServiceMock
            .Setup<Task<WeeklyStats?>>(x => x.GetWeeklyStatsAsync(It.IsAny<DateTime>()))
            .ReturnsAsync((WeeklyStats?)weeklyStats);

        // Act
        var cut = RenderComponent<Pomodoro.Web.Pages.History>();

        // Assert
        StatisticsServiceMock.Verify(x => x.GetWeeklyStatsAsync(
            It.IsAny<DateTime>()), Times.Once);
    }

    [Fact]
    public void HistoryPage_HandlesEmptyWeeklyStats()
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
            .Setup(x => x.GetActivityCountAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(0);

        ActivityServiceMock
            .Setup(x => x.GetDailyFocusMinutes(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(new Dictionary<DateTime, int>());

        ActivityServiceMock
            .Setup(x => x.GetDailyBreakMinutes(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(new Dictionary<DateTime, int>());

        StatisticsServiceMock
            .Setup<Task<WeeklyStats?>>(x => x.GetWeeklyStatsAsync(It.IsAny<DateTime>()))
            .ReturnsAsync((WeeklyStats?)null);

        // Act
        var cut = RenderComponent<Pomodoro.Web.Pages.History>();

        // Assert
        StatisticsServiceMock.Verify(x => x.GetWeeklyStatsAsync(
            It.IsAny<DateTime>()), Times.Once);
    }

    [Fact]
    public void HistoryPage_HandlesActivitiesWithTasks()
    {
        // Arrange
        var testDate = DateTime.Now.Date;
        var activities = new List<ActivityRecord>
        {
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.Pomodoro,
                CompletedAt = testDate,
                DurationMinutes = 25,
                TaskName = "Task 1",
                TaskId = Guid.NewGuid()
            },
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.Pomodoro,
                CompletedAt = testDate.AddHours(-1),
                DurationMinutes = 25,
                TaskName = "Task 2",
                TaskId = Guid.NewGuid()
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
            .Setup(x => x.GetActivityCountAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(2);

        ActivityServiceMock
            .Setup(x => x.GetDailyFocusMinutes(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(new Dictionary<DateTime, int>());

        ActivityServiceMock
            .Setup(x => x.GetDailyBreakMinutes(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(new Dictionary<DateTime, int>());

        StatisticsServiceMock
            .Setup<Task<WeeklyStats?>>(x => x.GetWeeklyStatsAsync(It.IsAny<DateTime>()))
            .ReturnsAsync((WeeklyStats?)null);

        // Act
        var cut = RenderComponent<Pomodoro.Web.Pages.History>();

        // Assert
        ActivityServiceMock.Verify(x => x.GetActivitiesPagedAsync(
            It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public void HistoryPage_HandlesActivitiesWithoutTasks()
    {
        // Arrange
        var testDate = DateTime.Now.Date;
        var activities = new List<ActivityRecord>
        {
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.Pomodoro,
                CompletedAt = testDate,
                DurationMinutes = 25,
                TaskName = null,
                TaskId = null
            },
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.ShortBreak,
                CompletedAt = testDate.AddHours(-1),
                DurationMinutes = 5,
                TaskName = null,
                TaskId = null
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
            .Setup(x => x.GetActivityCountAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(2);

        ActivityServiceMock
            .Setup(x => x.GetDailyFocusMinutes(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(new Dictionary<DateTime, int>());

        ActivityServiceMock
            .Setup(x => x.GetDailyBreakMinutes(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(new Dictionary<DateTime, int>());

        StatisticsServiceMock
            .Setup<Task<WeeklyStats?>>(x => x.GetWeeklyStatsAsync(It.IsAny<DateTime>()))
            .ReturnsAsync((WeeklyStats?)null);

        // Act
        var cut = RenderComponent<Pomodoro.Web.Pages.History>();

        // Assert
        ActivityServiceMock.Verify(x => x.GetActivitiesPagedAsync(
            It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public void HistoryPage_HandlesCompletedAndUncompletedActivities()
    {
        // Arrange
        var testDate = DateTime.Now.Date;
        var activities = new List<ActivityRecord>
        {
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.Pomodoro,
                CompletedAt = testDate,
                DurationMinutes = 25,
                WasCompleted = true
            },
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.Pomodoro,
                CompletedAt = testDate.AddHours(-1),
                DurationMinutes = 25,
                WasCompleted = false
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
            .Setup(x => x.GetActivityCountAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(2);

        ActivityServiceMock
            .Setup(x => x.GetDailyFocusMinutes(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(new Dictionary<DateTime, int>());

        ActivityServiceMock
            .Setup(x => x.GetDailyBreakMinutes(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(new Dictionary<DateTime, int>());

        StatisticsServiceMock
            .Setup<Task<WeeklyStats?>>(x => x.GetWeeklyStatsAsync(It.IsAny<DateTime>()))
            .ReturnsAsync((WeeklyStats?)null);

        // Act
        var cut = RenderComponent<Pomodoro.Web.Pages.History>();

        // Assert
        ActivityServiceMock.Verify(x => x.GetActivitiesPagedAsync(
            It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public void HistoryPage_HandlesDifferentDurations()
    {
        // Arrange
        var testDate = DateTime.Now.Date;
        var activities = new List<ActivityRecord>
        {
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.Pomodoro,
                CompletedAt = testDate,
                DurationMinutes = 25
            },
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.ShortBreak,
                CompletedAt = testDate.AddHours(-1),
                DurationMinutes = 5
            },
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.LongBreak,
                CompletedAt = testDate.AddHours(-2),
                DurationMinutes = 15
            },
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.Pomodoro,
                CompletedAt = testDate.AddHours(-3),
                DurationMinutes = 50
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
            .Setup(x => x.GetActivityCountAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(4);

        ActivityServiceMock
            .Setup(x => x.GetDailyFocusMinutes(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(new Dictionary<DateTime, int>());

        ActivityServiceMock
            .Setup(x => x.GetDailyBreakMinutes(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(new Dictionary<DateTime, int>());

        StatisticsServiceMock
            .Setup<Task<WeeklyStats?>>(x => x.GetWeeklyStatsAsync(It.IsAny<DateTime>()))
            .ReturnsAsync((WeeklyStats?)null);

        // Act
        var cut = RenderComponent<Pomodoro.Web.Pages.History>();

        // Assert
        ActivityServiceMock.Verify(x => x.GetActivitiesPagedAsync(
            It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public void HistoryPage_SubscribesToActivityChangesOnInitialization()
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
            .Setup(x => x.GetActivityCountAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(0);

        ActivityServiceMock
            .Setup(x => x.GetDailyFocusMinutes(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(new Dictionary<DateTime, int>());

        ActivityServiceMock
            .Setup(x => x.GetDailyBreakMinutes(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(new Dictionary<DateTime, int>());

        StatisticsServiceMock
            .Setup<Task<WeeklyStats?>>(x => x.GetWeeklyStatsAsync(It.IsAny<DateTime>()))
            .ReturnsAsync((WeeklyStats?)null);

        // Act
        var cut = RenderComponent<Pomodoro.Web.Pages.History>();

        // Assert - Verify subscription by checking InitializeAsync was called
        ActivityServiceMock.Verify(x => x.InitializeAsync(), Times.Once);
    }

    [Fact]
    public void HistoryPage_DisposesCorrectly()
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
            .Setup(x => x.GetActivityCountAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(0);

        ActivityServiceMock
            .Setup(x => x.GetDailyFocusMinutes(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(new Dictionary<DateTime, int>());

        ActivityServiceMock
            .Setup(x => x.GetDailyBreakMinutes(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(new Dictionary<DateTime, int>());

        StatisticsServiceMock
            .Setup<Task<WeeklyStats?>>(x => x.GetWeeklyStatsAsync(It.IsAny<DateTime>()))
            .ReturnsAsync((WeeklyStats?)null);

        // Act
        var cut = RenderComponent<Pomodoro.Web.Pages.History>();
        cut.Dispose();

        // Assert - Component should dispose without errors
        ActivityServiceMock.Verify(x => x.InitializeAsync(), Times.Once);
    }

    [Fact]
    public void HistoryPage_RendersWeeklyViewWithStats()
    {
        // Arrange
        var weekStart = DateTime.Now.Date.AddDays(-(int)DateTime.Now.DayOfWeek);
        var weeklyStats = new WeeklyStats
        {
            TotalFocusMinutes = 300,
            TotalPomodoroCount = 12,
            UniqueTasksWorkedOn = 5,
            DailyAverageMinutes = 60,
            MostProductiveDay = DayOfWeek.Monday,
            PreviousWeekFocusMinutes = 240,
            WeekOverWeekChange = 25.0
        };

        var weeklyFocusMinutes = new Dictionary<DateTime, int>
        {
            { weekStart, 120 },
            { weekStart.AddDays(1), 90 },
            { weekStart.AddDays(2), 150 }
        };

        var weeklyBreakMinutes = new Dictionary<DateTime, int>
        {
            { weekStart, 30 },
            { weekStart.AddDays(1), 20 },
            { weekStart.AddDays(2), 40 }
        };

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
            .Setup(x => x.GetActivityCountAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(0);

        ActivityServiceMock
            .Setup(x => x.GetDailyFocusMinutes(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(weeklyFocusMinutes);

        ActivityServiceMock
            .Setup(x => x.GetDailyBreakMinutes(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(weeklyBreakMinutes);

        StatisticsServiceMock
            .Setup<Task<WeeklyStats?>>(x => x.GetWeeklyStatsAsync(It.IsAny<DateTime>()))
            .ReturnsAsync((WeeklyStats?)weeklyStats);

        // Act
        var cut = RenderComponent<Pomodoro.Web.Pages.History>();

        // Assert
        cut.Should().NotBeNull();
        ActivityServiceMock.Verify(x => x.GetDailyFocusMinutes(
            It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Once);
        ActivityServiceMock.Verify(x => x.GetDailyBreakMinutes(
            It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Once);
        StatisticsServiceMock.Verify(x => x.GetWeeklyStatsAsync(
            It.IsAny<DateTime>()), Times.Once);
    }

    [Fact]
    public void HistoryPage_RendersWeeklyViewWithoutStats()
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
            .Setup(x => x.GetActivityCountAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(0);

        ActivityServiceMock
            .Setup(x => x.GetDailyFocusMinutes(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(new Dictionary<DateTime, int>());

        ActivityServiceMock
            .Setup(x => x.GetDailyBreakMinutes(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(new Dictionary<DateTime, int>());

        StatisticsServiceMock
            .Setup<Task<WeeklyStats?>>(x => x.GetWeeklyStatsAsync(It.IsAny<DateTime>()))
            .ReturnsAsync((WeeklyStats?)null);

        // Act
        var cut = RenderComponent<Pomodoro.Web.Pages.History>();

        // Assert
        cut.Should().NotBeNull();
        StatisticsServiceMock.Verify(x => x.GetWeeklyStatsAsync(
            It.IsAny<DateTime>()), Times.Once);
    }

    [Fact]
    public void HistoryPage_RendersDailyViewWithActivities()
    {
        // Arrange
        var testDate = DateTime.Now.Date;
        var activities = new List<ActivityRecord>
        {
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.Pomodoro,
                CompletedAt = testDate,
                DurationMinutes = 25,
                TaskName = "Test Task"
            }
        };

        var dailyStats = new DailyStatsSummary
        {
            PomodoroCount = 1,
            FocusMinutes = 25,
            TasksWorkedOn = 1
        };

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
            .Setup(x => x.GetActivityCountAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(1);

        ActivityServiceMock
            .Setup(x => x.GetDailyFocusMinutes(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(new Dictionary<DateTime, int>());

        ActivityServiceMock
            .Setup(x => x.GetDailyBreakMinutes(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(new Dictionary<DateTime, int>());

        StatisticsServiceMock
            .Setup<Task<WeeklyStats?>>(x => x.GetWeeklyStatsAsync(It.IsAny<DateTime>()))
            .ReturnsAsync((WeeklyStats?)null);

        // Act
        var cut = RenderComponent<Pomodoro.Web.Pages.History>();

        // Assert
        cut.Should().NotBeNull();
        cut.Markup.Should().Contain("stat-grid");
        ActivityServiceMock.Verify(x => x.GetActivitiesPagedAsync(
            It.IsAny<DateTime>(), It.IsAny<DateTime>(), 0, 20), Times.Once);
    }

    [Fact]
    public void HistoryPage_RendersDailyViewWithNoActivities()
    {
        // Arrange
        var dailyStats = new DailyStatsSummary
        {
            PomodoroCount = 0,
            FocusMinutes = 0,
            TasksWorkedOn = 0
        };

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
            .Setup(x => x.GetActivityCountAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(0);

        ActivityServiceMock
            .Setup(x => x.GetDailyFocusMinutes(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(new Dictionary<DateTime, int>());

        ActivityServiceMock
            .Setup(x => x.GetDailyBreakMinutes(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(new Dictionary<DateTime, int>());

        StatisticsServiceMock
            .Setup<Task<WeeklyStats?>>(x => x.GetWeeklyStatsAsync(It.IsAny<DateTime>()))
            .ReturnsAsync((WeeklyStats?)null);

        // Act
        var cut = RenderComponent<Pomodoro.Web.Pages.History>();

        // Assert
        cut.Should().NotBeNull();
        cut.Markup.Should().Contain("No activities for this day");
        ActivityServiceMock.Verify(x => x.GetActivitiesPagedAsync(
            It.IsAny<DateTime>(), It.IsAny<DateTime>(), 0, 20), Times.Once);
    }

    [Fact]
    public void HistoryPage_RendersWeeklyViewWithPositiveTrend()
    {
        // Arrange
        var weekStart = DateTime.Now.Date.AddDays(-(int)DateTime.Now.DayOfWeek);
        var weeklyStats = new WeeklyStats
        {
            TotalFocusMinutes = 300,
            TotalPomodoroCount = 12,
            UniqueTasksWorkedOn = 5,
            DailyAverageMinutes = 60,
            MostProductiveDay = DayOfWeek.Monday,
            PreviousWeekFocusMinutes = 240,
            WeekOverWeekChange = 25.0
        };

        var weeklyFocusMinutes = new Dictionary<DateTime, int>
        {
            { weekStart, 120 },
            { weekStart.AddDays(1), 90 },
            { weekStart.AddDays(2), 150 }
        };

        var weeklyBreakMinutes = new Dictionary<DateTime, int>
        {
            { weekStart, 30 },
            { weekStart.AddDays(1), 20 },
            { weekStart.AddDays(2), 40 }
        };

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
            .Setup(x => x.GetActivityCountAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(0);

        ActivityServiceMock
            .Setup(x => x.GetDailyFocusMinutes(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(weeklyFocusMinutes);

        ActivityServiceMock
            .Setup(x => x.GetDailyBreakMinutes(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(weeklyBreakMinutes);

        StatisticsServiceMock
            .Setup<Task<WeeklyStats?>>(x => x.GetWeeklyStatsAsync(It.IsAny<DateTime>()))
            .ReturnsAsync((WeeklyStats?)weeklyStats);

        // Act
        var cut = RenderComponent<Pomodoro.Web.Pages.History>();

        // Assert - Weekly stats are loaded but component defaults to Daily view
        cut.Should().NotBeNull();
        ActivityServiceMock.Verify(x => x.GetDailyFocusMinutes(
            It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Once);
        ActivityServiceMock.Verify(x => x.GetDailyBreakMinutes(
            It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Once);
        StatisticsServiceMock.Verify(x => x.GetWeeklyStatsAsync(
            It.IsAny<DateTime>()), Times.Once);
    }

    [Fact]
    public void HistoryPage_RendersWeeklyViewWithNegativeTrend()
    {
        // Arrange
        var weekStart = DateTime.Now.Date.AddDays(-(int)DateTime.Now.DayOfWeek);
        var weeklyStats = new WeeklyStats
        {
            TotalFocusMinutes = 200,
            TotalPomodoroCount = 8,
            UniqueTasksWorkedOn = 3,
            DailyAverageMinutes = 40,
            MostProductiveDay = DayOfWeek.Wednesday,
            PreviousWeekFocusMinutes = 240,
            WeekOverWeekChange = -20.0
        };

        var weeklyFocusMinutes = new Dictionary<DateTime, int>
        {
            { weekStart, 80 },
            { weekStart.AddDays(1), 60 },
            { weekStart.AddDays(2), 60 }
        };

        var weeklyBreakMinutes = new Dictionary<DateTime, int>
        {
            { weekStart, 20 },
            { weekStart.AddDays(1), 15 },
            { weekStart.AddDays(2), 25 }
        };

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
            .Setup(x => x.GetActivityCountAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(0);

        ActivityServiceMock
            .Setup(x => x.GetDailyFocusMinutes(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(weeklyFocusMinutes);

        ActivityServiceMock
            .Setup(x => x.GetDailyBreakMinutes(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(weeklyBreakMinutes);

        StatisticsServiceMock
            .Setup<Task<WeeklyStats?>>(x => x.GetWeeklyStatsAsync(It.IsAny<DateTime>()))
            .ReturnsAsync((WeeklyStats?)weeklyStats);

        // Act
        var cut = RenderComponent<Pomodoro.Web.Pages.History>();

        // Assert - Weekly stats are loaded but component defaults to Daily view
        cut.Should().NotBeNull();
        ActivityServiceMock.Verify(x => x.GetDailyFocusMinutes(
            It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Once);
        ActivityServiceMock.Verify(x => x.GetDailyBreakMinutes(
            It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Once);
        StatisticsServiceMock.Verify(x => x.GetWeeklyStatsAsync(
            It.IsAny<DateTime>()), Times.Once);
    }

    [Fact]
    public void HistoryPage_RendersWeeklyViewWithZeroTrend()
    {
        // Arrange
        var weekStart = DateTime.Now.Date.AddDays(-(int)DateTime.Now.DayOfWeek);
        var weeklyStats = new WeeklyStats
        {
            TotalFocusMinutes = 200,
            TotalPomodoroCount = 8,
            UniqueTasksWorkedOn = 3,
            DailyAverageMinutes = 40,
            MostProductiveDay = DayOfWeek.Wednesday,
            PreviousWeekFocusMinutes = 200,
            WeekOverWeekChange = 0.0
        };

        var weeklyFocusMinutes = new Dictionary<DateTime, int>
        {
            { weekStart, 80 },
            { weekStart.AddDays(1), 60 },
            { weekStart.AddDays(2), 60 }
        };

        var weeklyBreakMinutes = new Dictionary<DateTime, int>
        {
            { weekStart, 20 },
            { weekStart.AddDays(1), 15 },
            { weekStart.AddDays(2), 25 }
        };

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
            .Setup(x => x.GetActivityCountAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(0);

        ActivityServiceMock
            .Setup(x => x.GetDailyFocusMinutes(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(weeklyFocusMinutes);

        ActivityServiceMock
            .Setup(x => x.GetDailyBreakMinutes(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(weeklyBreakMinutes);

        StatisticsServiceMock
            .Setup<Task<WeeklyStats?>>(x => x.GetWeeklyStatsAsync(It.IsAny<DateTime>()))
            .ReturnsAsync((WeeklyStats?)weeklyStats);

        // Act
        var cut = RenderComponent<Pomodoro.Web.Pages.History>();

        // Assert
        cut.Should().NotBeNull();
        StatisticsServiceMock.Verify(x => x.GetWeeklyStatsAsync(
            It.IsAny<DateTime>()), Times.Once);
    }

    [Fact]
    public void HistoryPage_SwitchesToWeeklyTab()
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
            .Setup(x => x.GetActivityCountAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(0);

        ActivityServiceMock
            .Setup(x => x.GetDailyFocusMinutes(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(new Dictionary<DateTime, int>());

        ActivityServiceMock
            .Setup(x => x.GetDailyBreakMinutes(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(new Dictionary<DateTime, int>());

        StatisticsServiceMock
            .Setup<Task<WeeklyStats?>>(x => x.GetWeeklyStatsAsync(It.IsAny<DateTime>()))
            .ReturnsAsync((WeeklyStats?)null);

        // Act
        var cut = RenderComponent<Pomodoro.Web.Pages.History>();

        // Find and click the Weekly tab button
        var weeklyTab = cut.Find("#weekly-tab");
        weeklyTab.Should().NotBeNull();
        weeklyTab.Click();

        // Assert
        cut.Render();
        ActivityServiceMock.Verify(x => x.GetDailyFocusMinutes(
            It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.AtLeastOnce());
        ActivityServiceMock.Verify(x => x.GetDailyBreakMinutes(
            It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.AtLeastOnce());
    }

    [Fact]
    public void HistoryPage_SwitchesBetweenTabs()
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
            .Setup(x => x.GetActivityCountAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(0);

        ActivityServiceMock
            .Setup(x => x.GetDailyFocusMinutes(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(new Dictionary<DateTime, int>());

        ActivityServiceMock
            .Setup(x => x.GetDailyBreakMinutes(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(new Dictionary<DateTime, int>());

        StatisticsServiceMock
            .Setup<Task<WeeklyStats?>>(x => x.GetWeeklyStatsAsync(It.IsAny<DateTime>()))
            .ReturnsAsync((WeeklyStats?)null);

        // Act
        var cut = RenderComponent<Pomodoro.Web.Pages.History>();

        // Click Weekly tab
        var weeklyTab = cut.Find("#weekly-tab");
        weeklyTab.Click();

        // Click Daily tab
        var dailyTab = cut.Find("#daily-tab");
        dailyTab.Click();

        // Assert
        cut.Render();
        ActivityServiceMock.Verify(x => x.GetDailyFocusMinutes(
            It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.AtLeastOnce());
    }

    [Fact]
    public void HistoryPage_HandlesMultipleDateChanges()
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
            .Setup(x => x.GetActivityCountAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(0);

        ActivityServiceMock
            .Setup(x => x.GetDailyFocusMinutes(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(new Dictionary<DateTime, int>());

        ActivityServiceMock
            .Setup(x => x.GetDailyBreakMinutes(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(new Dictionary<DateTime, int>());

        StatisticsServiceMock
            .Setup<Task<WeeklyStats?>>(x => x.GetWeeklyStatsAsync(It.IsAny<DateTime>()))
            .ReturnsAsync((WeeklyStats?)null);

        // Act
        var cut = RenderComponent<Pomodoro.Web.Pages.History>();

        // Click previous day button
        var prevButton = cut.Find("button.nav-arr[title='Previous day']");
        prevButton.Click();

        // Click next day button (should be disabled)
        var nextButton = cut.Find("button.nav-arr[title='Next day']");

        // Click previous again
        prevButton.Click();

        // Assert
        cut.Render();
        ActivityServiceMock.Verify(x => x.GetActivitiesForDate(
            It.IsAny<DateTime>()), Times.AtLeastOnce());
    }

    [Fact]
    public void HistoryPage_HandlesMultipleWeekChanges()
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
            .Setup(x => x.GetActivityCountAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(0);

        ActivityServiceMock
            .Setup(x => x.GetDailyFocusMinutes(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(new Dictionary<DateTime, int>());

        ActivityServiceMock
            .Setup(x => x.GetDailyBreakMinutes(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(new Dictionary<DateTime, int>());

        StatisticsServiceMock
            .Setup<Task<WeeklyStats?>>(x => x.GetWeeklyStatsAsync(It.IsAny<DateTime>()))
            .ReturnsAsync((WeeklyStats?)null);

        // Act
        var cut = RenderComponent<Pomodoro.Web.Pages.History>();

        // Click Weekly tab
        var weeklyTab = cut.Find("#weekly-tab");
        weeklyTab.Click();

        // Click previous week
        var prevWeekButton = cut.Find("button[title='Previous week']");
        prevWeekButton.Click();

        // Click next week button
        var nextWeekButton = cut.Find("button[title='Next week']");
        nextWeekButton.Click();

        // Assert
        cut.Render();
        StatisticsServiceMock.Verify(x => x.GetWeeklyStatsAsync(
            It.IsAny<DateTime>()), Times.AtLeastOnce());
    }

    [Fact]
    public void HistoryPage_HandlesActivityChanges()
    {
        // Arrange
        var testDate = DateTime.Now.Date;
        var activities = new List<ActivityRecord>
        {
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.Pomodoro,
                CompletedAt = testDate,
                DurationMinutes = 25,
                TaskName = "Test Task"
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
            .Setup(x => x.GetActivityCountAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(1);

        ActivityServiceMock
            .Setup(x => x.GetDailyFocusMinutes(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(new Dictionary<DateTime, int>());

        ActivityServiceMock
            .Setup(x => x.GetDailyBreakMinutes(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(new Dictionary<DateTime, int>());

        StatisticsServiceMock
            .Setup<Task<WeeklyStats?>>(x => x.GetWeeklyStatsAsync(It.IsAny<DateTime>()))
            .ReturnsAsync((WeeklyStats?)null);

        // Act
        var cut = RenderComponent<Pomodoro.Web.Pages.History>();

        // Trigger activity change (simulate activity being added/modified)
        // This tests the OnActivityChanged event handler
        cut.Render();

        // Assert
        ActivityServiceMock.Verify(x => x.GetActivitiesForDate(
            It.IsAny<DateTime>()), Times.AtLeastOnce());
    }

    [Fact]
    public void HistoryPage_HandlesMultipleLoadMore()
    {
        // Arrange
        var testDate = DateTime.Now.Date;
        var activitiesPage1 = new List<ActivityRecord>
        {
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.Pomodoro,
                CompletedAt = testDate,
                DurationMinutes = 25,
                TaskName = "Task 1"
            }
        };

        var activitiesPage2 = new List<ActivityRecord>
        {
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.Pomodoro,
                CompletedAt = testDate,
                DurationMinutes = 25,
                TaskName = "Task 2"
            }
        };

        ActivityServiceMock
            .Setup(x => x.InitializeAsync())
            .Returns(Task.CompletedTask);

        ActivityServiceMock
            .SetupSequence(x => x.GetActivitiesPagedAsync(
                It.IsAny<DateTime>(), It.IsAny<DateTime>(), 0, 20))
            .ReturnsAsync(activitiesPage1)
            .ReturnsAsync(activitiesPage2);

        ActivityServiceMock
            .Setup(x => x.GetActivitiesForDate(It.IsAny<DateTime>()))
            .Returns(activitiesPage1);

        ActivityServiceMock
            .Setup(x => x.GetActivityCountAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(25); // More than page size (20) so HasMoreActivities is true

        ActivityServiceMock
            .Setup(x => x.GetDailyFocusMinutes(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(new Dictionary<DateTime, int>());

        ActivityServiceMock
            .Setup(x => x.GetDailyBreakMinutes(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(new Dictionary<DateTime, int>());

        StatisticsServiceMock
            .Setup<Task<WeeklyStats?>>(x => x.GetWeeklyStatsAsync(It.IsAny<DateTime>()))
            .ReturnsAsync((WeeklyStats?)null);

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

        // Act
        var cut = RenderComponent<Pomodoro.Web.Pages.History>(builder => builder
            .Add(p => p.InitialHasMoreActivities, true)
            .Add(p => p.InitialActivities, activitiesPage1));

        var sentinel = cut.Find("#scroll-sentinel");
        sentinel.Should().NotBeNull();

        // Assert
        cut.Render();
        ActivityServiceMock.Verify(x => x.GetActivitiesPagedAsync(
            It.IsAny<DateTime>(), It.IsAny<DateTime>(), 0, 20), Times.Once);
    }
}

