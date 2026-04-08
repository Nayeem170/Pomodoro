using Pomodoro.Web.Models;
using Xunit;

namespace Pomodoro.Web.Tests.Models.DailyStatsTests;

/// <summary>
/// Tests for DailyStats model
/// </summary>
public class DailyStatsTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_InitializesTaskIdsWorkedOnAsEmptyList()
    {
        // Act
        var stats = new DailyStats();

        // Assert
        Assert.NotNull(stats.TaskIdsWorkedOn);
        Assert.Empty(stats.TaskIdsWorkedOn);
    }

    [Fact]
    public void Constructor_DateProperty_IsDefaultDateTime()
    {
        // Act
        var stats = new DailyStats();

        // Assert
        Assert.Equal(default, stats.Date);
    }

    [Fact]
    public void Constructor_TotalFocusMinutesProperty_IsZero()
    {
        // Act
        var stats = new DailyStats();

        // Assert
        Assert.Equal(0, stats.TotalFocusMinutes);
    }

    [Fact]
    public void Constructor_PomodoroCountProperty_IsZero()
    {
        // Act
        var stats = new DailyStats();

        // Assert
        Assert.Equal(0, stats.PomodoroCount);
    }

    #endregion

    #region Property Tests

    [Fact]
    public void DateProperty_CanBeSet()
    {
        // Arrange
        var stats = new DailyStats();
        var expectedDate = new DateTime(2026, 3, 2);

        // Act
        stats.Date = expectedDate;

        // Assert
        Assert.Equal(expectedDate, stats.Date);
    }

    [Fact]
    public void TotalFocusMinutesProperty_CanBeSet()
    {
        // Arrange
        var stats = new DailyStats();
        var expectedMinutes = 120;

        // Act
        stats.TotalFocusMinutes = expectedMinutes;

        // Assert
        Assert.Equal(expectedMinutes, stats.TotalFocusMinutes);
    }

    [Fact]
    public void PomodoroCountProperty_CanBeSet()
    {
        // Arrange
        var stats = new DailyStats();
        var expectedCount = 8;

        // Act
        stats.PomodoroCount = expectedCount;

        // Assert
        Assert.Equal(expectedCount, stats.PomodoroCount);
    }

    [Fact]
    public void TaskIdsWorkedOnProperty_CanBeSet()
    {
        // Arrange
        var stats = new DailyStats();
        var expectedIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

        // Act
        stats.TaskIdsWorkedOn = expectedIds;

        // Assert
        Assert.Equal(expectedIds, stats.TaskIdsWorkedOn);
    }

    [Fact]
    public void TaskIdsWorkedOnProperty_CanAddItems()
    {
        // Arrange
        var stats = new DailyStats();
        var taskId = Guid.NewGuid();

        // Act
        stats.TaskIdsWorkedOn.Add(taskId);

        // Assert
        Assert.Single(stats.TaskIdsWorkedOn);
        Assert.Equal(taskId, stats.TaskIdsWorkedOn[0]);
    }

    [Fact]
    public void TaskIdsWorkedOnProperty_CanRemoveItems()
    {
        // Arrange
        var stats = new DailyStats();
        var taskId = Guid.NewGuid();
        stats.TaskIdsWorkedOn.Add(taskId);

        // Act
        stats.TaskIdsWorkedOn.Remove(taskId);

        // Assert
        Assert.Empty(stats.TaskIdsWorkedOn);
    }

    [Fact]
    public void TaskIdsWorkedOnProperty_CanClear()
    {
        // Arrange
        var stats = new DailyStats();
        stats.TaskIdsWorkedOn.Add(Guid.NewGuid());
        stats.TaskIdsWorkedOn.Add(Guid.NewGuid());

        // Act
        stats.TaskIdsWorkedOn.Clear();

        // Assert
        Assert.Empty(stats.TaskIdsWorkedOn);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void DateProperty_CanBeSetToMinValue()
    {
        // Arrange
        var stats = new DailyStats();

        // Act
        stats.Date = DateTime.MinValue;

        // Assert
        Assert.Equal(DateTime.MinValue, stats.Date);
    }

    [Fact]
    public void DateProperty_CanBeSetToMaxValue()
    {
        // Arrange
        var stats = new DailyStats();

        // Act
        stats.Date = DateTime.MaxValue;

        // Assert
        Assert.Equal(DateTime.MaxValue, stats.Date);
    }

    [Fact]
    public void TotalFocusMinutesProperty_CanBeSetToZero()
    {
        // Arrange
        var stats = new DailyStats();

        // Act
        stats.TotalFocusMinutes = 0;

        // Assert
        Assert.Equal(0, stats.TotalFocusMinutes);
    }

    [Fact]
    public void TotalFocusMinutesProperty_CanBeSetToLargeValue()
    {
        // Arrange
        var stats = new DailyStats();
        var largeValue = 1440; // 24 hours in minutes

        // Act
        stats.TotalFocusMinutes = largeValue;

        // Assert
        Assert.Equal(largeValue, stats.TotalFocusMinutes);
    }

    [Fact]
    public void PomodoroCountProperty_CanBeSetToZero()
    {
        // Arrange
        var stats = new DailyStats();

        // Act
        stats.PomodoroCount = 0;

        // Assert
        Assert.Equal(0, stats.PomodoroCount);
    }

    [Fact]
    public void PomodoroCountProperty_CanBeSetToLargeValue()
    {
        // Arrange
        var stats = new DailyStats();
        var largeValue = 100;

        // Act
        stats.PomodoroCount = largeValue;

        // Assert
        Assert.Equal(largeValue, stats.PomodoroCount);
    }

    [Fact]
    public void TaskIdsWorkedOnProperty_CanHandleMultipleGuids()
    {
        // Arrange
        var stats = new DailyStats();
        var guids = Enumerable.Range(0, 10).Select(_ => Guid.NewGuid()).ToList();

        // Act
        stats.TaskIdsWorkedOn.AddRange(guids);

        // Assert
        Assert.Equal(10, stats.TaskIdsWorkedOn.Count);
    }

    [Fact]
    public void TaskIdsWorkedOnProperty_CanHandleDuplicateGuids()
    {
        // Arrange
        var stats = new DailyStats();
        var guid = Guid.NewGuid();

        // Act
        stats.TaskIdsWorkedOn.Add(guid);
        stats.TaskIdsWorkedOn.Add(guid);

        // Assert
        Assert.Equal(2, stats.TaskIdsWorkedOn.Count);
    }

    [Fact]
    public void TaskIdsWorkedOnProperty_CanHandleEmptyGuid()
    {
        // Arrange
        var stats = new DailyStats();

        // Act
        stats.TaskIdsWorkedOn.Add(Guid.Empty);

        // Assert
        Assert.Single(stats.TaskIdsWorkedOn);
        Assert.Equal(Guid.Empty, stats.TaskIdsWorkedOn[0]);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void FullObject_CanBeCreatedWithAllProperties()
    {
        // Arrange
        var expectedDate = new DateTime(2026, 3, 2);
        var expectedMinutes = 480;
        var expectedCount = 12;
        var expectedTaskIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

        // Act
        var stats = new DailyStats
        {
            Date = expectedDate,
            TotalFocusMinutes = expectedMinutes,
            PomodoroCount = expectedCount,
            TaskIdsWorkedOn = expectedTaskIds
        };

        // Assert
        Assert.Equal(expectedDate, stats.Date);
        Assert.Equal(expectedMinutes, stats.TotalFocusMinutes);
        Assert.Equal(expectedCount, stats.PomodoroCount);
        Assert.Equal(expectedTaskIds, stats.TaskIdsWorkedOn);
    }

    [Fact]
    public void MultipleDailyStats_CanBeCreatedIndependently()
    {
        // Arrange
        var stats1 = new DailyStats { Date = DateTime.Today, TotalFocusMinutes = 120, PomodoroCount = 3 };
        var stats2 = new DailyStats { Date = DateTime.Today.AddDays(1), TotalFocusMinutes = 240, PomodoroCount = 6 };

        // Act & Assert
        Assert.NotEqual(stats1.Date, stats2.Date);
        Assert.NotEqual(stats1.TotalFocusMinutes, stats2.TotalFocusMinutes);
        Assert.NotEqual(stats1.PomodoroCount, stats2.PomodoroCount);
        Assert.NotSame(stats1.TaskIdsWorkedOn, stats2.TaskIdsWorkedOn);
    }

    #endregion
}
