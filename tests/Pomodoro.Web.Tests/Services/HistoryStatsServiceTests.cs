using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Xunit;

namespace Pomodoro.Web.Tests.Services;

/// <summary>
/// Tests for HistoryStatsService
/// </summary>
public class HistoryStatsServiceTests
{
    private readonly HistoryStatsService _service;

    public HistoryStatsServiceTests()
    {
        _service = new HistoryStatsService();
    }

    [Fact]
    public void CalculateStats_EmptyActivities_ReturnsEmptyStats()
    {
        // Arrange
        var activities = new List<ActivityRecord>();

        // Act
        var result = _service.CalculateStats(activities);

        // Assert
        Assert.Equal(0, result.PomodoroCount);
        Assert.Equal(0, result.FocusMinutes);
        Assert.Equal(0, result.TasksWorkedOn);
    }

    [Fact]
    public void CalculateStats_OnlyPomodoros_ReturnsCorrectStats()
    {
        // Arrange
        var activities = new List<ActivityRecord>
        {
            new ActivityRecord { Type = SessionType.Pomodoro, DurationMinutes = 25, TaskId = Guid.NewGuid() },
            new ActivityRecord { Type = SessionType.Pomodoro, DurationMinutes = 25, TaskId = Guid.NewGuid() },
            new ActivityRecord { Type = SessionType.Pomodoro, DurationMinutes = 25, TaskId = Guid.NewGuid() }
        };

        // Act
        var result = _service.CalculateStats(activities);

        // Assert
        Assert.Equal(3, result.PomodoroCount);
        Assert.Equal(75, result.FocusMinutes);
        Assert.Equal(3, result.TasksWorkedOn);
    }

    [Fact]
    public void CalculateStats_WithBreaks_ReturnsOnlyPomodoroStats()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var activities = new List<ActivityRecord>
        {
            new ActivityRecord { Type = SessionType.Pomodoro, DurationMinutes = 25, TaskId = taskId },
            new ActivityRecord { Type = SessionType.ShortBreak, DurationMinutes = 5 },
            new ActivityRecord { Type = SessionType.LongBreak, DurationMinutes = 15 },
            new ActivityRecord { Type = SessionType.Pomodoro, DurationMinutes = 25, TaskId = taskId }
        };

        // Act
        var result = _service.CalculateStats(activities);

        // Assert
        Assert.Equal(2, result.PomodoroCount);
        Assert.Equal(50, result.FocusMinutes);
        Assert.Equal(1, result.TasksWorkedOn);
    }

    [Fact]
    public void CalculateStats_WithSameTask_ReturnsUniqueTaskCount()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var activities = new List<ActivityRecord>
        {
            new ActivityRecord { Type = SessionType.Pomodoro, DurationMinutes = 25, TaskId = taskId },
            new ActivityRecord { Type = SessionType.Pomodoro, DurationMinutes = 25, TaskId = taskId },
            new ActivityRecord { Type = SessionType.Pomodoro, DurationMinutes = 25, TaskId = taskId }
        };

        // Act
        var result = _service.CalculateStats(activities);

        // Assert
        Assert.Equal(3, result.PomodoroCount);
        Assert.Equal(75, result.FocusMinutes);
        Assert.Equal(1, result.TasksWorkedOn);
    }

    [Fact]
    public void CalculateStats_WithDifferentTasks_ReturnsUniqueTaskCount()
    {
        // Arrange
        var activities = new List<ActivityRecord>
        {
            new ActivityRecord { Type = SessionType.Pomodoro, DurationMinutes = 25, TaskId = Guid.NewGuid() },
            new ActivityRecord { Type = SessionType.Pomodoro, DurationMinutes = 25, TaskId = Guid.NewGuid() },
            new ActivityRecord { Type = SessionType.Pomodoro, DurationMinutes = 25, TaskId = Guid.NewGuid() }
        };

        // Act
        var result = _service.CalculateStats(activities);

        // Assert
        Assert.Equal(3, result.PomodoroCount);
        Assert.Equal(75, result.FocusMinutes);
        Assert.Equal(3, result.TasksWorkedOn);
    }

    [Fact]
    public void CalculateStats_WithMixedTasks_ReturnsUniqueTaskCount()
    {
        // Arrange
        var task1 = Guid.NewGuid();
        var task2 = Guid.NewGuid();
        var activities = new List<ActivityRecord>
        {
            new ActivityRecord { Type = SessionType.Pomodoro, DurationMinutes = 25, TaskId = task1 },
            new ActivityRecord { Type = SessionType.Pomodoro, DurationMinutes = 25, TaskId = task2 },
            new ActivityRecord { Type = SessionType.Pomodoro, DurationMinutes = 25, TaskId = task1 },
            new ActivityRecord { Type = SessionType.Pomodoro, DurationMinutes = 25, TaskId = null },
            new ActivityRecord { Type = SessionType.Pomodoro, DurationMinutes = 25, TaskId = task2 }
        };

        // Act
        var result = _service.CalculateStats(activities);

        // Assert
        Assert.Equal(5, result.PomodoroCount);
        Assert.Equal(125, result.FocusMinutes);
        Assert.Equal(2, result.TasksWorkedOn);
    }

    [Fact]
    public void CalculateStats_WithNullTaskId_ExcludesFromTaskCount()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var activities = new List<ActivityRecord>
        {
            new ActivityRecord { Type = SessionType.Pomodoro, DurationMinutes = 25, TaskId = taskId },
            new ActivityRecord { Type = SessionType.Pomodoro, DurationMinutes = 25, TaskId = null },
            new ActivityRecord { Type = SessionType.Pomodoro, DurationMinutes = 25, TaskId = taskId }
        };

        // Act
        var result = _service.CalculateStats(activities);

        // Assert
        Assert.Equal(3, result.PomodoroCount);
        Assert.Equal(75, result.FocusMinutes);
        Assert.Equal(1, result.TasksWorkedOn);
    }

    [Fact]
    public void CalculateStats_VaryingDurations_SumsCorrectly()
    {
        // Arrange
        var activities = new List<ActivityRecord>
        {
            new ActivityRecord { Type = SessionType.Pomodoro, DurationMinutes = 20, TaskId = Guid.NewGuid() },
            new ActivityRecord { Type = SessionType.Pomodoro, DurationMinutes = 25, TaskId = Guid.NewGuid() },
            new ActivityRecord { Type = SessionType.Pomodoro, DurationMinutes = 30, TaskId = Guid.NewGuid() },
            new ActivityRecord { Type = SessionType.Pomodoro, DurationMinutes = 50, TaskId = Guid.NewGuid() }
        };

        // Act
        var result = _service.CalculateStats(activities);

        // Assert
        Assert.Equal(4, result.PomodoroCount);
        Assert.Equal(125, result.FocusMinutes);
        Assert.Equal(4, result.TasksWorkedOn);
    }

    [Fact]
    public void CalculateStats_LargeNumberOfActivities_HandlesCorrectly()
    {
        // Arrange
        var activities = Enumerable.Range(0, 100)
            .Select(i => new ActivityRecord
            {
                Type = SessionType.Pomodoro,
                DurationMinutes = 25,
                TaskId = i % 5 == 0 ? Guid.NewGuid() : (Guid?)null
            })
            .ToList();

        // Act
        var result = _service.CalculateStats(activities);

        // Assert
        Assert.Equal(100, result.PomodoroCount);
        Assert.Equal(2500, result.FocusMinutes);
        Assert.Equal(20, result.TasksWorkedOn); // Every 5th activity has a unique task ID
    }
}
