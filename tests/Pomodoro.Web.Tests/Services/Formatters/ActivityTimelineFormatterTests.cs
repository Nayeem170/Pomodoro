using Xunit;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services.Formatters;

namespace Pomodoro.Web.Tests.Services.Formatters;

/// <summary>
/// Tests for ActivityTimelineFormatter service
/// </summary>
[Trait("Category", "Service")]
public class ActivityTimelineFormatterTests
{
    private readonly ActivityTimelineFormatter _formatter = new();

    #region GetActivityCount Tests

    [Fact]
    public void GetActivityCount_WhenActivitiesIsNull_ReturnsZero()
    {
        // Arrange
        List<ActivityRecord>? activities = null;

        // Act
        var result = _formatter.GetActivityCount(activities);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void GetActivityCount_WhenActivitiesIsEmpty_ReturnsZero()
    {
        // Arrange
        var activities = new List<ActivityRecord>();

        // Act
        var result = _formatter.GetActivityCount(activities);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void GetActivityCount_WhenActivitiesHasOneItem_ReturnsOne()
    {
        // Arrange
        var activities = new List<ActivityRecord>
        {
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.Pomodoro,
                DurationMinutes = 25,
                CompletedAt = DateTime.UtcNow
            }
        };

        // Act
        var result = _formatter.GetActivityCount(activities);

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void GetActivityCount_WhenActivitiesHasMultipleItems_ReturnsCount()
    {
        // Arrange
        var activities = new List<ActivityRecord>
        {
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.Pomodoro,
                DurationMinutes = 25,
                CompletedAt = DateTime.UtcNow
            },
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.ShortBreak,
                DurationMinutes = 5,
                CompletedAt = DateTime.UtcNow
            },
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.LongBreak,
                DurationMinutes = 15,
                CompletedAt = DateTime.UtcNow
            }
        };

        // Act
        var result = _formatter.GetActivityCount(activities);

        // Assert
        Assert.Equal(3, result);
    }

    #endregion

    #region HasActivities Tests

    [Fact]
    public void HasActivities_WhenActivitiesIsNull_ReturnsFalse()
    {
        // Arrange
        List<ActivityRecord>? activities = null;

        // Act
        var result = _formatter.HasActivities(activities);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasActivities_WhenActivitiesIsEmpty_ReturnsFalse()
    {
        // Arrange
        var activities = new List<ActivityRecord>();

        // Act
        var result = _formatter.HasActivities(activities);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasActivities_WhenActivitiesHasOneItem_ReturnsTrue()
    {
        // Arrange
        var activities = new List<ActivityRecord>
        {
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.Pomodoro,
                DurationMinutes = 25,
                CompletedAt = DateTime.UtcNow
            }
        };

        // Act
        var result = _formatter.HasActivities(activities);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasActivities_WhenActivitiesHasMultipleItems_ReturnsTrue()
    {
        // Arrange
        var activities = new List<ActivityRecord>
        {
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.Pomodoro,
                DurationMinutes = 25,
                CompletedAt = DateTime.UtcNow
            },
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.ShortBreak,
                DurationMinutes = 5,
                CompletedAt = DateTime.UtcNow
            }
        };

        // Act
        var result = _formatter.HasActivities(activities);

        // Assert
        Assert.True(result);
    }

    #endregion

    #region GetTotalDuration Tests

    [Fact]
    public void GetTotalDuration_WhenActivitiesIsNull_ReturnsZero()
    {
        // Arrange
        List<ActivityRecord>? activities = null;

        // Act
        var result = _formatter.GetTotalDuration(activities);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void GetTotalDuration_WhenActivitiesIsEmpty_ReturnsZero()
    {
        // Arrange
        var activities = new List<ActivityRecord>();

        // Act
        var result = _formatter.GetTotalDuration(activities);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void GetTotalDuration_WhenActivitiesHasOneItem_ReturnsDuration()
    {
        // Arrange
        var activities = new List<ActivityRecord>
        {
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.Pomodoro,
                DurationMinutes = 25,
                CompletedAt = DateTime.UtcNow
            }
        };

        // Act
        var result = _formatter.GetTotalDuration(activities);

        // Assert
        Assert.Equal(25, result);
    }

    [Fact]
    public void GetTotalDuration_WhenActivitiesHasMultipleItems_ReturnsSum()
    {
        // Arrange
        var activities = new List<ActivityRecord>
        {
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.Pomodoro,
                DurationMinutes = 25,
                CompletedAt = DateTime.UtcNow
            },
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.ShortBreak,
                DurationMinutes = 5,
                CompletedAt = DateTime.UtcNow
            },
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.LongBreak,
                DurationMinutes = 15,
                CompletedAt = DateTime.UtcNow
            }
        };

        // Act
        var result = _formatter.GetTotalDuration(activities);

        // Assert
        Assert.Equal(45, result);
    }

    [Fact]
    public void GetTotalDuration_WhenActivitiesHasZeroDuration_ReturnsZero()
    {
        // Arrange
        var activities = new List<ActivityRecord>
        {
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.Pomodoro,
                DurationMinutes = 0,
                CompletedAt = DateTime.UtcNow
            }
        };

        // Act
        var result = _formatter.GetTotalDuration(activities);

        // Assert
        Assert.Equal(0, result);
    }

    #endregion

    #region GetPomodoroCount Tests

    [Fact]
    public void GetPomodoroCount_WhenActivitiesIsNull_ReturnsZero()
    {
        // Arrange
        List<ActivityRecord>? activities = null;

        // Act
        var result = _formatter.GetPomodoroCount(activities);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void GetPomodoroCount_WhenActivitiesIsEmpty_ReturnsZero()
    {
        // Arrange
        var activities = new List<ActivityRecord>();

        // Act
        var result = _formatter.GetPomodoroCount(activities);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void GetPomodoroCount_WhenActivitiesHasNoPomodoro_ReturnsZero()
    {
        // Arrange
        var activities = new List<ActivityRecord>
        {
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.ShortBreak,
                DurationMinutes = 5,
                CompletedAt = DateTime.UtcNow
            },
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.LongBreak,
                DurationMinutes = 15,
                CompletedAt = DateTime.UtcNow
            }
        };

        // Act
        var result = _formatter.GetPomodoroCount(activities);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void GetPomodoroCount_WhenActivitiesHasOnePomodoro_ReturnsOne()
    {
        // Arrange
        var activities = new List<ActivityRecord>
        {
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.Pomodoro,
                DurationMinutes = 25,
                CompletedAt = DateTime.UtcNow
            },
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.ShortBreak,
                DurationMinutes = 5,
                CompletedAt = DateTime.UtcNow
            }
        };

        // Act
        var result = _formatter.GetPomodoroCount(activities);

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void GetPomodoroCount_WhenActivitiesHasMultiplePomodoros_ReturnsCount()
    {
        // Arrange
        var activities = new List<ActivityRecord>
        {
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.Pomodoro,
                DurationMinutes = 25,
                CompletedAt = DateTime.UtcNow
            },
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.Pomodoro,
                DurationMinutes = 25,
                CompletedAt = DateTime.UtcNow
            },
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.ShortBreak,
                DurationMinutes = 5,
                CompletedAt = DateTime.UtcNow
            }
        };

        // Act
        var result = _formatter.GetPomodoroCount(activities);

        // Assert
        Assert.Equal(2, result);
    }

    #endregion

    #region GetCompletedCount Tests

    [Fact]
    public void GetCompletedCount_WhenActivitiesIsNull_ReturnsZero()
    {
        // Arrange
        List<ActivityRecord>? activities = null;

        // Act
        var result = _formatter.GetCompletedCount(activities);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void GetCompletedCount_WhenActivitiesIsEmpty_ReturnsZero()
    {
        // Arrange
        var activities = new List<ActivityRecord>();

        // Act
        var result = _formatter.GetCompletedCount(activities);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void GetCompletedCount_WhenActivitiesHasNoCompleted_ReturnsZero()
    {
        // Arrange
        var activities = new List<ActivityRecord>
        {
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.Pomodoro,
                DurationMinutes = 25,
                CompletedAt = DateTime.UtcNow,
                WasCompleted = false
            }
        };

        // Act
        var result = _formatter.GetCompletedCount(activities);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void GetCompletedCount_WhenActivitiesHasOneCompleted_ReturnsOne()
    {
        // Arrange
        var activities = new List<ActivityRecord>
        {
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.Pomodoro,
                DurationMinutes = 25,
                CompletedAt = DateTime.UtcNow,
                WasCompleted = true
            },
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.ShortBreak,
                DurationMinutes = 5,
                CompletedAt = DateTime.UtcNow,
                WasCompleted = false
            }
        };

        // Act
        var result = _formatter.GetCompletedCount(activities);

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void GetCompletedCount_WhenActivitiesHasMultipleCompleted_ReturnsCount()
    {
        // Arrange
        var activities = new List<ActivityRecord>
        {
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.Pomodoro,
                DurationMinutes = 25,
                CompletedAt = DateTime.UtcNow,
                WasCompleted = true
            },
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.Pomodoro,
                DurationMinutes = 25,
                CompletedAt = DateTime.UtcNow,
                WasCompleted = true
            },
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.ShortBreak,
                DurationMinutes = 5,
                CompletedAt = DateTime.UtcNow,
                WasCompleted = false
            }
        };

        // Act
        var result = _formatter.GetCompletedCount(activities);

        // Assert
        Assert.Equal(2, result);
    }

    #endregion

    #region IsEmpty Tests

    [Fact]
    public void IsEmpty_WhenActivitiesIsNull_ReturnsTrue()
    {
        // Arrange
        List<ActivityRecord>? activities = null;

        // Act
        var result = _formatter.IsEmpty(activities);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsEmpty_WhenActivitiesIsEmpty_ReturnsTrue()
    {
        // Arrange
        var activities = new List<ActivityRecord>();

        // Act
        var result = _formatter.IsEmpty(activities);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsEmpty_WhenActivitiesHasItems_ReturnsFalse()
    {
        // Arrange
        var activities = new List<ActivityRecord>
        {
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.Pomodoro,
                DurationMinutes = 25,
                CompletedAt = DateTime.UtcNow
            }
        };

        // Act
        var result = _formatter.IsEmpty(activities);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsEmpty_WhenActivitiesHasMultipleItems_ReturnsFalse()
    {
        // Arrange
        var activities = new List<ActivityRecord>
        {
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.Pomodoro,
                DurationMinutes = 25,
                CompletedAt = DateTime.UtcNow
            },
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.ShortBreak,
                DurationMinutes = 5,
                CompletedAt = DateTime.UtcNow
            }
        };

        // Act
        var result = _formatter.IsEmpty(activities);

        // Assert
        Assert.False(result);
    }

    #endregion
}

