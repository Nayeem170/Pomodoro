using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Pomodoro.Web.Components.History;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services.Formatters;
using Xunit;

namespace Pomodoro.Web.Tests.History;

/// <summary>
/// Tests for ActivityItem component.
/// Tests rendering with different activity types and values.
/// </summary>
[Trait("Category", "Component")]
public class ActivityItemTests : TestContext
{
    public ActivityItemTests()
    {
        // Add JSInterop for Blazor
        JSInterop.Mode = JSRuntimeMode.Loose;
        // Register formatter service
        Services.AddScoped<ActivityItemFormatter>();
    }

    #region Rendering Tests

    [Fact]
    public void ActivityItem_WithPomodoro_RendersCorrectly()
    {
        // Arrange
        var activity = new ActivityRecord
        {
            Id = Guid.NewGuid(),
            Type = SessionType.Pomodoro,
            TaskName = "Test Task",
            CompletedAt = DateTime.UtcNow,
            DurationMinutes = 25,
            WasCompleted = true
        };

        // Act
        var cut = RenderComponent<ActivityItem>(parameters => parameters
            .Add(p => p.Activity, activity));

        // Assert
        Assert.NotNull(cut.Markup);
        Assert.Contains("activity-item", cut.Markup);
    }

    [Fact]
    public void ActivityItem_WithShortBreak_RendersCorrectly()
    {
        // Arrange
        var activity = new ActivityRecord
        {
            Id = Guid.NewGuid(),
            Type = SessionType.ShortBreak,
            CompletedAt = DateTime.UtcNow,
            DurationMinutes = 5,
            WasCompleted = true
        };

        // Act
        var cut = RenderComponent<ActivityItem>(parameters => parameters
            .Add(p => p.Activity, activity));

        // Assert
        Assert.NotNull(cut.Markup);
        Assert.Contains("activity-item", cut.Markup);
    }

    [Fact]
    public void ActivityItem_WithLongBreak_RendersCorrectly()
    {
        // Arrange
        var activity = new ActivityRecord
        {
            Id = Guid.NewGuid(),
            Type = SessionType.LongBreak,
            CompletedAt = DateTime.UtcNow,
            DurationMinutes = 15,
            WasCompleted = true
        };

        // Act
        var cut = RenderComponent<ActivityItem>(parameters => parameters
            .Add(p => p.Activity, activity));

        // Assert
        Assert.NotNull(cut.Markup);
        Assert.Contains("activity-item", cut.Markup);
    }

    [Fact]
    public void ActivityItem_WithTaskName_RendersTaskName()
    {
        // Arrange
        var activity = new ActivityRecord
        {
            Id = Guid.NewGuid(),
            Type = SessionType.Pomodoro,
            TaskName = "Complete Project",
            CompletedAt = DateTime.UtcNow,
            DurationMinutes = 25,
            WasCompleted = true
        };

        // Act
        var cut = RenderComponent<ActivityItem>(parameters => parameters
            .Add(p => p.Activity, activity));

        // Assert
        Assert.Contains("Complete Project", cut.Markup);
    }

    [Fact]
    public void ActivityItem_WithNoTaskName_RendersDefaultText()
    {
        // Arrange
        var activity = new ActivityRecord
        {
            Id = Guid.NewGuid(),
            Type = SessionType.Pomodoro,
            TaskName = null,
            CompletedAt = DateTime.UtcNow,
            DurationMinutes = 25,
            WasCompleted = true
        };

        // Act
        var cut = RenderComponent<ActivityItem>(parameters => parameters
            .Add(p => p.Activity, activity));

        // Assert
        Assert.NotNull(cut.Markup);
        Assert.Contains("activity-item", cut.Markup);
    }

    [Fact]
    public void ActivityItem_RendersDuration()
    {
        // Arrange
        var activity = new ActivityRecord
        {
            Id = Guid.NewGuid(),
            Type = SessionType.Pomodoro,
            TaskName = "Test Task",
            CompletedAt = DateTime.UtcNow,
            DurationMinutes = 30,
            WasCompleted = true
        };

        // Act
        var cut = RenderComponent<ActivityItem>(parameters => parameters
            .Add(p => p.Activity, activity));

        // Assert
        Assert.Contains("30 min", cut.Markup);
    }

    [Fact]
    public void ActivityItem_RendersIcon()
    {
        // Arrange
        var activity = new ActivityRecord
        {
            Id = Guid.NewGuid(),
            Type = SessionType.Pomodoro,
            TaskName = "Test Task",
            CompletedAt = DateTime.UtcNow,
            DurationMinutes = 25,
            WasCompleted = true
        };

        // Act
        var cut = RenderComponent<ActivityItem>(parameters => parameters
            .Add(p => p.Activity, activity));

        // Assert
        Assert.NotNull(cut.Markup);
        Assert.Contains("activity-icon", cut.Markup);
    }

    [Fact]
    public void ActivityItem_RendersTime()
    {
        // Arrange
        var activity = new ActivityRecord
        {
            Id = Guid.NewGuid(),
            Type = SessionType.Pomodoro,
            TaskName = "Test Task",
            CompletedAt = DateTime.UtcNow,
            DurationMinutes = 25,
            WasCompleted = true
        };

        // Act
        var cut = RenderComponent<ActivityItem>(parameters => parameters
            .Add(p => p.Activity, activity));

        // Assert
        Assert.NotNull(cut.Markup);
        Assert.Contains("activity-time", cut.Markup);
    }

    #endregion

    #region Code-Behind Method Tests

    [Fact]
    public void IsValidActivity_WithValidActivity_ReturnsTrue()
    {
        // Arrange
        var activity = new ActivityRecord
        {
            Id = Guid.NewGuid(),
            Type = SessionType.Pomodoro,
            TaskName = "Test Task",
            CompletedAt = DateTime.UtcNow,
            DurationMinutes = 25,
            WasCompleted = true
        };

        var cut = RenderComponent<ActivityItem>(parameters => parameters
            .Add(p => p.Activity, activity));

        // Act
        var result = cut.Instance.IsValidActivity();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValidActivity_WithEmptyId_ReturnsFalse()
    {
        // Arrange
        var activity = new ActivityRecord
        {
            Id = Guid.Empty,
            Type = SessionType.Pomodoro,
            TaskName = "Test Task",
            CompletedAt = DateTime.UtcNow,
            DurationMinutes = 25,
            WasCompleted = true
        };

        var cut = RenderComponent<ActivityItem>(parameters => parameters
            .Add(p => p.Activity, activity));

        // Act
        var result = cut.Instance.IsValidActivity();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetFormattedTime_WithActivity_ReturnsFormattedTime()
    {
        // Arrange
        var activity = new ActivityRecord
        {
            Id = Guid.NewGuid(),
            Type = SessionType.Pomodoro,
            TaskName = "Test Task",
            CompletedAt = new DateTime(2024, 1, 1, 14, 30, 0),
            DurationMinutes = 25,
            WasCompleted = true
        };

        var cut = RenderComponent<ActivityItem>(parameters => parameters
            .Add(p => p.Activity, activity));

        // Act
        var result = cut.Instance.GetFormattedTime();

        // Assert
        Assert.Equal("14:30", result);
    }

    [Fact]
    public void GetFormattedDuration_WithActivity_ReturnsFormattedDuration()
    {
        // Arrange
        var activity = new ActivityRecord
        {
            Id = Guid.NewGuid(),
            Type = SessionType.Pomodoro,
            TaskName = "Test Task",
            CompletedAt = DateTime.UtcNow,
            DurationMinutes = 25,
            WasCompleted = true
        };

        var cut = RenderComponent<ActivityItem>(parameters => parameters
            .Add(p => p.Activity, activity));

        // Act
        var result = cut.Instance.GetFormattedDuration();

        // Assert
        Assert.Equal("25m", result);
    }

    [Fact]
    public void GetSessionTypeDisplay_WithPomodoro_ReturnsPomodoro()
    {
        // Arrange
        var activity = new ActivityRecord
        {
            Id = Guid.NewGuid(),
            Type = SessionType.Pomodoro,
            TaskName = "Test Task",
            CompletedAt = DateTime.UtcNow,
            DurationMinutes = 25,
            WasCompleted = true
        };

        var cut = RenderComponent<ActivityItem>(parameters => parameters
            .Add(p => p.Activity, activity));

        // Act
        var result = cut.Instance.GetSessionTypeDisplay();

        // Assert
        Assert.Equal("Pomodoro", result);
    }

    [Fact]
    public void HasTask_WithTaskName_ReturnsTrue()
    {
        // Arrange
        var activity = new ActivityRecord
        {
            Id = Guid.NewGuid(),
            Type = SessionType.Pomodoro,
            TaskName = "Test Task",
            CompletedAt = DateTime.UtcNow,
            DurationMinutes = 25,
            WasCompleted = true
        };

        var cut = RenderComponent<ActivityItem>(parameters => parameters
            .Add(p => p.Activity, activity));

        // Act
        var result = cut.Instance.HasTask();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasTask_WithoutTaskName_ReturnsFalse()
    {
        // Arrange
        var activity = new ActivityRecord
        {
            Id = Guid.NewGuid(),
            Type = SessionType.Pomodoro,
            TaskName = null,
            CompletedAt = DateTime.UtcNow,
            DurationMinutes = 25,
            WasCompleted = true
        };

        var cut = RenderComponent<ActivityItem>(parameters => parameters
            .Add(p => p.Activity, activity));

        // Act
        var result = cut.Instance.HasTask();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetTaskName_WithTaskName_ReturnsTaskName()
    {
        // Arrange
        var activity = new ActivityRecord
        {
            Id = Guid.NewGuid(),
            Type = SessionType.Pomodoro,
            TaskName = "Complete Project",
            CompletedAt = DateTime.UtcNow,
            DurationMinutes = 25,
            WasCompleted = true
        };

        var cut = RenderComponent<ActivityItem>(parameters => parameters
            .Add(p => p.Activity, activity));

        // Act
        var result = cut.Instance.GetTaskName();

        // Assert
        Assert.Equal("Complete Project", result);
    }

    [Fact]
    public void GetTaskName_WithoutTaskName_ReturnsDefault()
    {
        // Arrange
        var activity = new ActivityRecord
        {
            Id = Guid.NewGuid(),
            Type = SessionType.Pomodoro,
            TaskName = null,
            CompletedAt = DateTime.UtcNow,
            DurationMinutes = 25,
            WasCompleted = true
        };

        var cut = RenderComponent<ActivityItem>(parameters => parameters
            .Add(p => p.Activity, activity));

        // Act
        var result = cut.Instance.GetTaskName();

        // Assert
        Assert.Equal("No task", result);
    }

    #endregion
}

