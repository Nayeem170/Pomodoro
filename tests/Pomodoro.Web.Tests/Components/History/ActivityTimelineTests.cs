using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Pomodoro.Web.Components.History;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services.Formatters;
using Xunit;

namespace Pomodoro.Web.Tests.Components.History;

/// <summary>
/// Tests for ActivityTimeline component.
/// Tests rendering with different activity lists.
/// </summary>
public class ActivityTimelineTests : TestContext
{
    public ActivityTimelineTests()
    {
        // Add JSInterop for Blazor
        JSInterop.Mode = JSRuntimeMode.Loose;
        // Register formatter services (ActivityTimeline uses ActivityItem which needs its own formatter)
        Services.AddScoped<ActivityTimelineFormatter>();
        Services.AddScoped<ActivityItemFormatter>();
    }

    #region Rendering Tests

    [Fact]
    public void ActivityTimeline_WithEmptyList_RendersEmptyState()
    {
        // Arrange
        var activities = new List<ActivityRecord>();

        // Act
        var cut = RenderComponent<ActivityTimeline>(parameters => parameters
            .Add(p => p.Activities, activities));

        // Assert
        Assert.NotNull(cut.Markup);
        Assert.Contains("empty-state", cut.Markup);
        Assert.Contains("No activities for this day", cut.Markup);
    }

    [Fact]
    public void ActivityTimeline_WithSingleActivity_RendersActivity()
    {
        // Arrange
        var activities = new List<ActivityRecord>
        {
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.Pomodoro,
                TaskName = "Test Task",
                CompletedAt = DateTime.UtcNow,
                DurationMinutes = 25,
                WasCompleted = true
            }
        };

        // Act
        var cut = RenderComponent<ActivityTimeline>(parameters => parameters
            .Add(p => p.Activities, activities));

        // Assert
        Assert.NotNull(cut.Markup);
        Assert.Contains("activity-timeline", cut.Markup);
        Assert.Contains("Test Task", cut.Markup);
    }

    [Fact]
    public void ActivityTimeline_WithMultipleActivities_RendersAll()
    {
        // Arrange
        var activities = new List<ActivityRecord>
        {
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.Pomodoro,
                TaskName = "Task 1",
                CompletedAt = DateTime.UtcNow,
                DurationMinutes = 25,
                WasCompleted = true
            },
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.ShortBreak,
                CompletedAt = DateTime.UtcNow,
                DurationMinutes = 5,
                WasCompleted = true
            },
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.LongBreak,
                CompletedAt = DateTime.UtcNow,
                DurationMinutes = 15,
                WasCompleted = true
            }
        };

        // Act
        var cut = RenderComponent<ActivityTimeline>(parameters => parameters
            .Add(p => p.Activities, activities));

        // Assert
        Assert.NotNull(cut.Markup);
        Assert.Contains("activity-timeline", cut.Markup);
        Assert.Contains("Task 1", cut.Markup);
    }

    [Fact]
    public void ActivityTimeline_WithPomodoroActivity_RendersCorrectly()
    {
        // Arrange
        var activities = new List<ActivityRecord>
        {
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.Pomodoro,
                TaskName = "Focus Work",
                CompletedAt = DateTime.UtcNow,
                DurationMinutes = 25,
                WasCompleted = true
            }
        };

        // Act
        var cut = RenderComponent<ActivityTimeline>(parameters => parameters
            .Add(p => p.Activities, activities));

        // Assert
        Assert.NotNull(cut.Markup);
        Assert.Contains("Focus Work", cut.Markup);
    }

    [Fact]
    public void ActivityTimeline_WithShortBreak_RendersCorrectly()
    {
        // Arrange
        var activities = new List<ActivityRecord>
        {
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.ShortBreak,
                CompletedAt = DateTime.UtcNow,
                DurationMinutes = 5,
                WasCompleted = true
            }
        };

        // Act
        var cut = RenderComponent<ActivityTimeline>(parameters => parameters
            .Add(p => p.Activities, activities));

        // Assert
        Assert.NotNull(cut.Markup);
        Assert.Contains("activity-timeline", cut.Markup);
    }

    [Fact]
    public void ActivityTimeline_WithLongBreak_RendersCorrectly()
    {
        // Arrange
        var activities = new List<ActivityRecord>
        {
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.LongBreak,
                CompletedAt = DateTime.UtcNow,
                DurationMinutes = 15,
                WasCompleted = true
            }
        };

        // Act
        var cut = RenderComponent<ActivityTimeline>(parameters => parameters
            .Add(p => p.Activities, activities));

        // Assert
        Assert.NotNull(cut.Markup);
        Assert.Contains("activity-timeline", cut.Markup);
    }

    [Fact]
    public void ActivityTimeline_WithActivityWithoutTaskName_RendersCorrectly()
    {
        // Arrange
        var activities = new List<ActivityRecord>
        {
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.Pomodoro,
                TaskName = null,
                CompletedAt = DateTime.UtcNow,
                DurationMinutes = 25,
                WasCompleted = true
            }
        };

        // Act
        var cut = RenderComponent<ActivityTimeline>(parameters => parameters
            .Add(p => p.Activities, activities));

        // Assert
        Assert.NotNull(cut.Markup);
        Assert.Contains("activity-timeline", cut.Markup);
    }

    #endregion

    #region Code-Behind Method Tests

    [Fact]
    public void GetActivityCount_WithActivities_ReturnsCount()
    {
        // Arrange
        var activities = new List<ActivityRecord>
        {
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.Pomodoro,
                TaskName = "Task 1",
                CompletedAt = DateTime.UtcNow,
                DurationMinutes = 25,
                WasCompleted = true
            },
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.ShortBreak,
                CompletedAt = DateTime.UtcNow,
                DurationMinutes = 5,
                WasCompleted = true
            }
        };

        var cut = RenderComponent<ActivityTimeline>(parameters => parameters
            .Add(p => p.Activities, activities));

        // Act
        var result = cut.Instance.GetActivityCount();

        // Assert
        Assert.Equal(2, result);
    }

    [Fact]
    public void GetActivityCount_WithEmptyList_ReturnsZero()
    {
        // Arrange
        var activities = new List<ActivityRecord>();

        var cut = RenderComponent<ActivityTimeline>(parameters => parameters
            .Add(p => p.Activities, activities));

        // Act
        var result = cut.Instance.GetActivityCount();

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void HasActivities_WithActivities_ReturnsTrue()
    {
        // Arrange
        var activities = new List<ActivityRecord>
        {
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.Pomodoro,
                TaskName = "Task 1",
                CompletedAt = DateTime.UtcNow,
                DurationMinutes = 25,
                WasCompleted = true
            }
        };

        var cut = RenderComponent<ActivityTimeline>(parameters => parameters
            .Add(p => p.Activities, activities));

        // Act
        var result = cut.Instance.HasActivities();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasActivities_WithEmptyList_ReturnsFalse()
    {
        // Arrange
        var activities = new List<ActivityRecord>();

        var cut = RenderComponent<ActivityTimeline>(parameters => parameters
            .Add(p => p.Activities, activities));

        // Act
        var result = cut.Instance.HasActivities();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetTotalDuration_WithActivities_ReturnsTotal()
    {
        // Arrange
        var activities = new List<ActivityRecord>
        {
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.Pomodoro,
                TaskName = "Task 1",
                CompletedAt = DateTime.UtcNow,
                DurationMinutes = 25,
                WasCompleted = true
            },
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.ShortBreak,
                CompletedAt = DateTime.UtcNow,
                DurationMinutes = 5,
                WasCompleted = true
            },
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.LongBreak,
                CompletedAt = DateTime.UtcNow,
                DurationMinutes = 15,
                WasCompleted = true
            }
        };

        var cut = RenderComponent<ActivityTimeline>(parameters => parameters
            .Add(p => p.Activities, activities));

        // Act
        var result = cut.Instance.GetTotalDuration();

        // Assert
        Assert.Equal(45, result);
    }

    [Fact]
    public void GetPomodoroCount_WithPomodoros_ReturnsCount()
    {
        // Arrange
        var activities = new List<ActivityRecord>
        {
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.Pomodoro,
                TaskName = "Task 1",
                CompletedAt = DateTime.UtcNow,
                DurationMinutes = 25,
                WasCompleted = true
            },
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.ShortBreak,
                CompletedAt = DateTime.UtcNow,
                DurationMinutes = 5,
                WasCompleted = true
            },
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.Pomodoro,
                TaskName = "Task 2",
                CompletedAt = DateTime.UtcNow,
                DurationMinutes = 25,
                WasCompleted = true
            }
        };

        var cut = RenderComponent<ActivityTimeline>(parameters => parameters
            .Add(p => p.Activities, activities));

        // Act
        var result = cut.Instance.GetPomodoroCount();

        // Assert
        Assert.Equal(2, result);
    }

    [Fact]
    public void GetCompletedCount_WithCompletedActivities_ReturnsCount()
    {
        // Arrange
        var activities = new List<ActivityRecord>
        {
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.Pomodoro,
                TaskName = "Task 1",
                CompletedAt = DateTime.UtcNow,
                DurationMinutes = 25,
                WasCompleted = true
            },
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.Pomodoro,
                TaskName = "Task 2",
                CompletedAt = DateTime.UtcNow,
                DurationMinutes = 25,
                WasCompleted = false
            },
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.ShortBreak,
                CompletedAt = DateTime.UtcNow,
                DurationMinutes = 5,
                WasCompleted = true
            }
        };

        var cut = RenderComponent<ActivityTimeline>(parameters => parameters
            .Add(p => p.Activities, activities));

        // Act
        var result = cut.Instance.GetCompletedCount();

        // Assert
        Assert.Equal(2, result);
    }

    [Fact]
    public void IsEmpty_WithEmptyList_ReturnsTrue()
    {
        // Arrange
        var activities = new List<ActivityRecord>();

        var cut = RenderComponent<ActivityTimeline>(parameters => parameters
            .Add(p => p.Activities, activities));

        // Act
        var result = cut.Instance.IsEmpty();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsEmpty_WithActivities_ReturnsFalse()
    {
        // Arrange
        var activities = new List<ActivityRecord>
        {
            new ActivityRecord
            {
                Id = Guid.NewGuid(),
                Type = SessionType.Pomodoro,
                TaskName = "Task 1",
                CompletedAt = DateTime.UtcNow,
                DurationMinutes = 25,
                WasCompleted = true
            }
        };

        var cut = RenderComponent<ActivityTimeline>(parameters => parameters
            .Add(p => p.Activities, activities));

        // Act
        var result = cut.Instance.IsEmpty();

        // Assert
        Assert.False(result);
    }

    #endregion
}
