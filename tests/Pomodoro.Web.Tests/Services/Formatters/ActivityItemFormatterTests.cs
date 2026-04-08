using Xunit;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services.Formatters;

namespace Pomodoro.Web.Tests.Services.Formatters;

/// <summary>
/// Tests for ActivityItemFormatter service
/// </summary>
public class ActivityItemFormatterTests
{
    private readonly ActivityItemFormatter _formatter = new();

    #region IsValidActivity Tests

    [Fact]
    public void IsValidActivity_WhenActivityIsNull_ReturnsFalse()
    {
        // Arrange
        ActivityRecord? activity = null;

        // Act
        var result = _formatter.IsValidActivity(activity);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValidActivity_WhenActivityIdIsEmpty_ReturnsFalse()
    {
        // Arrange
        var activity = new ActivityRecord
        {
            Id = Guid.Empty,
            Type = SessionType.Pomodoro,
            DurationMinutes = 25,
            CompletedAt = DateTime.UtcNow
        };

        // Act
        var result = _formatter.IsValidActivity(activity);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValidActivity_WhenActivityIdIsValid_ReturnsTrue()
    {
        // Arrange
        var activity = new ActivityRecord
        {
            Id = Guid.NewGuid(),
            Type = SessionType.Pomodoro,
            DurationMinutes = 25,
            CompletedAt = DateTime.UtcNow
        };

        // Act
        var result = _formatter.IsValidActivity(activity);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValidActivity_WhenActivityHasOtherFields_ReturnsTrue()
    {
        // Arrange
        var activity = new ActivityRecord
        {
            Id = Guid.NewGuid(),
            Type = SessionType.ShortBreak,
            DurationMinutes = 5,
            CompletedAt = DateTime.UtcNow,
            TaskName = "Task 1",
            WasCompleted = true
        };

        // Act
        var result = _formatter.IsValidActivity(activity);

        // Assert
        Assert.True(result);
    }

    #endregion

    #region GetFormattedTime Tests

    [Fact]
    public void GetFormattedTime_WhenActivityIsNull_ReturnsNA()
    {
        // Arrange
        ActivityRecord? activity = null;

        // Act
        var result = _formatter.GetFormattedTime(activity);

        // Assert
        Assert.Equal("N/A", result);
    }

    [Fact]
    public void GetFormattedTime_WhenActivityHasCompletedAt_ReturnsFormattedTime()
    {
        // Arrange
        var activity = new ActivityRecord
        {
            Id = Guid.NewGuid(),
            Type = SessionType.Pomodoro,
            DurationMinutes = 25,
            CompletedAt = new DateTime(2025, 1, 15, 14, 30, 0)
        };

        // Act
        var result = _formatter.GetFormattedTime(activity);

        // Assert
        Assert.Equal("14:30", result);
    }

    [Fact]
    public void GetFormattedTime_WhenActivityHasMidnightTime_ReturnsFormattedTime()
    {
        // Arrange
        var activity = new ActivityRecord
        {
            Id = Guid.NewGuid(),
            Type = SessionType.Pomodoro,
            DurationMinutes = 25,
            CompletedAt = new DateTime(2025, 1, 15, 0, 0, 0)
        };

        // Act
        var result = _formatter.GetFormattedTime(activity);

        // Assert
        Assert.Equal("00:00", result);
    }

    [Fact]
    public void GetFormattedTime_WhenActivityHasLateNightTime_ReturnsFormattedTime()
    {
        // Arrange
        var activity = new ActivityRecord
        {
            Id = Guid.NewGuid(),
            Type = SessionType.Pomodoro,
            DurationMinutes = 25,
            CompletedAt = new DateTime(2025, 1, 15, 23, 59, 0)
        };

        // Act
        var result = _formatter.GetFormattedTime(activity);

        // Assert
        Assert.Equal("23:59", result);
    }

    #endregion

    #region GetFormattedDuration Tests

    [Fact]
    public void GetFormattedDuration_WhenActivityIsNull_ReturnsZeroM()
    {
        // Arrange
        ActivityRecord? activity = null;

        // Act
        var result = _formatter.GetFormattedDuration(activity);

        // Assert
        Assert.Equal("0m", result);
    }

    [Fact]
    public void GetFormattedDuration_WhenActivityHasZeroDuration_ReturnsZeroM()
    {
        // Arrange
        var activity = new ActivityRecord
        {
            Id = Guid.NewGuid(),
            Type = SessionType.Pomodoro,
            DurationMinutes = 0,
            CompletedAt = DateTime.UtcNow
        };

        // Act
        var result = _formatter.GetFormattedDuration(activity);

        // Assert
        Assert.Equal("0m", result);
    }

    [Fact]
    public void GetFormattedDuration_WhenActivityHasPositiveDuration_ReturnsFormattedDuration()
    {
        // Arrange
        var activity = new ActivityRecord
        {
            Id = Guid.NewGuid(),
            Type = SessionType.Pomodoro,
            DurationMinutes = 25,
            CompletedAt = DateTime.UtcNow
        };

        // Act
        var result = _formatter.GetFormattedDuration(activity);

        // Assert
        Assert.Equal("25m", result);
    }

    [Fact]
    public void GetFormattedDuration_WhenActivityHasLargeDuration_ReturnsFormattedDuration()
    {
        // Arrange
        var activity = new ActivityRecord
        {
            Id = Guid.NewGuid(),
            Type = SessionType.Pomodoro,
            DurationMinutes = 120,
            CompletedAt = DateTime.UtcNow
        };

        // Act
        var result = _formatter.GetFormattedDuration(activity);

        // Assert
        Assert.Equal("120m", result);
    }

    #endregion

    #region GetSessionTypeDisplay Tests

    [Fact]
    public void GetSessionTypeDisplay_WhenActivityIsNull_ReturnsUnknown()
    {
        // Arrange
        ActivityRecord? activity = null;

        // Act
        var result = _formatter.GetSessionTypeDisplay(activity);

        // Assert
        Assert.Equal("Unknown", result);
    }

    [Fact]
    public void GetSessionTypeDisplay_WhenActivityIsPomodoro_ReturnsPomodoro()
    {
        // Arrange
        var activity = new ActivityRecord
        {
            Id = Guid.NewGuid(),
            Type = SessionType.Pomodoro,
            DurationMinutes = 25,
            CompletedAt = DateTime.UtcNow
        };

        // Act
        var result = _formatter.GetSessionTypeDisplay(activity);

        // Assert
        Assert.Equal("Pomodoro", result);
    }

    [Fact]
    public void GetSessionTypeDisplay_WhenActivityIsShortBreak_ReturnsShortBreak()
    {
        // Arrange
        var activity = new ActivityRecord
        {
            Id = Guid.NewGuid(),
            Type = SessionType.ShortBreak,
            DurationMinutes = 5,
            CompletedAt = DateTime.UtcNow
        };

        // Act
        var result = _formatter.GetSessionTypeDisplay(activity);

        // Assert
        Assert.Equal("ShortBreak", result);
    }

    [Fact]
    public void GetSessionTypeDisplay_WhenActivityIsLongBreak_ReturnsLongBreak()
    {
        // Arrange
        var activity = new ActivityRecord
        {
            Id = Guid.NewGuid(),
            Type = SessionType.LongBreak,
            DurationMinutes = 15,
            CompletedAt = DateTime.UtcNow
        };

        // Act
        var result = _formatter.GetSessionTypeDisplay(activity);

        // Assert
        Assert.Equal("LongBreak", result);
    }

    #endregion

    #region HasTask Tests

    [Fact]
    public void HasTask_WhenActivityIsNull_ReturnsFalse()
    {
        // Arrange
        ActivityRecord? activity = null;

        // Act
        var result = _formatter.HasTask(activity);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasTask_WhenActivityTaskNameIsNull_ReturnsFalse()
    {
        // Arrange
        var activity = new ActivityRecord
        {
            Id = Guid.NewGuid(),
            Type = SessionType.Pomodoro,
            DurationMinutes = 25,
            CompletedAt = DateTime.UtcNow,
            TaskName = null
        };

        // Act
        var result = _formatter.HasTask(activity);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasTask_WhenActivityTaskNameIsEmpty_ReturnsFalse()
    {
        // Arrange
        var activity = new ActivityRecord
        {
            Id = Guid.NewGuid(),
            Type = SessionType.Pomodoro,
            DurationMinutes = 25,
            CompletedAt = DateTime.UtcNow,
            TaskName = string.Empty
        };

        // Act
        var result = _formatter.HasTask(activity);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasTask_WhenActivityTaskNameIsWhitespace_ReturnsFalse()
    {
        // Arrange
        var activity = new ActivityRecord
        {
            Id = Guid.NewGuid(),
            Type = SessionType.Pomodoro,
            DurationMinutes = 25,
            CompletedAt = DateTime.UtcNow,
            TaskName = "   "
        };

        // Act
        var result = _formatter.HasTask(activity);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasTask_WhenActivityTaskNameHasValue_ReturnsTrue()
    {
        // Arrange
        var activity = new ActivityRecord
        {
            Id = Guid.NewGuid(),
            Type = SessionType.Pomodoro,
            DurationMinutes = 25,
            CompletedAt = DateTime.UtcNow,
            TaskName = "Task 1"
        };

        // Act
        var result = _formatter.HasTask(activity);

        // Assert
        Assert.True(result);
    }

    #endregion

    #region GetTaskName Tests

    [Fact]
    public void GetTaskName_WhenActivityIsNull_ReturnsNoTask()
    {
        // Arrange
        ActivityRecord? activity = null;

        // Act
        var result = _formatter.GetTaskName(activity);

        // Assert
        Assert.Equal("No task", result);
    }

    [Fact]
    public void GetTaskName_WhenActivityTaskNameIsNull_ReturnsNoTask()
    {
        // Arrange
        var activity = new ActivityRecord
        {
            Id = Guid.NewGuid(),
            Type = SessionType.Pomodoro,
            DurationMinutes = 25,
            CompletedAt = DateTime.UtcNow,
            TaskName = null
        };

        // Act
        var result = _formatter.GetTaskName(activity);

        // Assert
        Assert.Equal("No task", result);
    }

    [Fact]
    public void GetTaskName_WhenActivityTaskNameIsEmpty_ReturnsNoTask()
    {
        // Arrange
        var activity = new ActivityRecord
        {
            Id = Guid.NewGuid(),
            Type = SessionType.Pomodoro,
            DurationMinutes = 25,
            CompletedAt = DateTime.UtcNow,
            TaskName = string.Empty
        };

        // Act
        var result = _formatter.GetTaskName(activity);

        // Assert
        Assert.Equal("No task", result);
    }

    [Fact]
    public void GetTaskName_WhenActivityTaskNameHasValue_ReturnsTaskName()
    {
        // Arrange
        var activity = new ActivityRecord
        {
            Id = Guid.NewGuid(),
            Type = SessionType.Pomodoro,
            DurationMinutes = 25,
            CompletedAt = DateTime.UtcNow,
            TaskName = "Task 1"
        };

        // Act
        var result = _formatter.GetTaskName(activity);

        // Assert
        Assert.Equal("Task 1", result);
    }

    [Fact]
    public void GetTaskName_WhenActivityTaskNameIsWhitespace_ReturnsNoTask()
    {
        // Arrange
        var activity = new ActivityRecord
        {
            Id = Guid.NewGuid(),
            Type = SessionType.Pomodoro,
            DurationMinutes = 25,
            CompletedAt = DateTime.UtcNow,
            TaskName = "   "
        };

        // Act
        var result = _formatter.GetTaskName(activity);

        // Assert
        Assert.Equal("No task", result);
    }

    #endregion
}
