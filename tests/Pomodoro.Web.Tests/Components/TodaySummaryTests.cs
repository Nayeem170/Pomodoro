using Bunit;
using Microsoft.AspNetCore.Components;
using Moq;
using Pomodoro.Web.Components.Shared;
using Xunit;

namespace Pomodoro.Web.Tests;

/// <summary>
/// bUnit tests for TodaySummary component.
/// Tests rendering of today's summary statistics.
/// </summary>
public class TodaySummaryTests : TestContext
{
    public TodaySummaryTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    #region Rendering Tests

    [Fact]
    public void TodaySummary_ShowsHeader()
    {
        // Arrange & Act
        var cut = RenderComponent<TodaySummary>(parameters => parameters
            .Add(p => p.TotalFocusMinutes, 0)
            .Add(p => p.PomodoroCount, 0)
            .Add(p => p.TasksWorkedOn, 0));

        // Assert
        Assert.Contains("TODAY'S SUMMARY", cut.Markup);
    }

    [Fact]
    public void TodaySummary_ShowsFocusTime()
    {
        // Arrange & Act
        var cut = RenderComponent<TodaySummary>(parameters => parameters
            .Add(p => p.TotalFocusMinutes, 25)
            .Add(p => p.PomodoroCount, 1)
            .Add(p => p.TasksWorkedOn, 1));

        // Assert
        Assert.Contains("25m", cut.Markup);
        Assert.Contains("focused", cut.Markup);
    }

    [Fact]
    public void TodaySummary_ShowsPomodoroCount()
    {
        // Arrange & Act
        var cut = RenderComponent<TodaySummary>(parameters => parameters
            .Add(p => p.TotalFocusMinutes, 75)
            .Add(p => p.PomodoroCount, 3)
            .Add(p => p.TasksWorkedOn, 2));

        // Assert
        Assert.Contains("3", cut.Markup);
        Assert.Contains("pomodoros", cut.Markup);
    }

    [Fact]
    public void TodaySummary_ShowsTasksWorkedOn()
    {
        // Arrange & Act
        var cut = RenderComponent<TodaySummary>(parameters => parameters
            .Add(p => p.TotalFocusMinutes, 50)
            .Add(p => p.PomodoroCount, 2)
            .Add(p => p.TasksWorkedOn, 5));

        // Assert
        Assert.Contains("5", cut.Markup);
        Assert.Contains("tasks", cut.Markup);
    }

    [Fact]
    public void TodaySummary_WithZeroValues_ShowsZeros()
    {
        // Arrange & Act
        var cut = RenderComponent<TodaySummary>(parameters => parameters
            .Add(p => p.TotalFocusMinutes, 0)
            .Add(p => p.PomodoroCount, 0)
            .Add(p => p.TasksWorkedOn, 0));

        // Assert
        Assert.Contains("0m", cut.Markup);
        Assert.Contains("0", cut.Markup);
    }

    #endregion

    #region Time Formatting Tests

    [Fact]
    public void TodaySummary_WithLessThanHour_ShowsMinutesOnly()
    {
        // Arrange & Act
        var cut = RenderComponent<TodaySummary>(parameters => parameters
            .Add(p => p.TotalFocusMinutes, 45)
            .Add(p => p.PomodoroCount, 2)
            .Add(p => p.TasksWorkedOn, 1));

        // Assert - shows minutes format
        Assert.Contains("45m", cut.Markup);
        // Does not show hours format like "0h"
        Assert.DoesNotContain("0h", cut.Markup);
    }

    [Fact]
    public void TodaySummary_WithExactlyOneHour_ShowsHoursAndMinutes()
    {
        // Arrange & Act
        var cut = RenderComponent<TodaySummary>(parameters => parameters
            .Add(p => p.TotalFocusMinutes, 60)
            .Add(p => p.PomodoroCount, 4)
            .Add(p => p.TasksWorkedOn, 1));

        // Assert
        Assert.Contains("1h", cut.Markup);
        Assert.Contains("0m", cut.Markup);
    }

    [Fact]
    public void TodaySummary_WithMoreThanHour_ShowsHoursAndMinutes()
    {
        // Arrange & Act
        var cut = RenderComponent<TodaySummary>(parameters => parameters
            .Add(p => p.TotalFocusMinutes, 90)
            .Add(p => p.PomodoroCount, 6)
            .Add(p => p.TasksWorkedOn, 2));

        // Assert
        Assert.Contains("1h", cut.Markup);
        Assert.Contains("30m", cut.Markup);
    }

    [Fact]
    public void TodaySummary_WithMultipleHours_ShowsCorrectFormat()
    {
        // Arrange & Act
        var cut = RenderComponent<TodaySummary>(parameters => parameters
            .Add(p => p.TotalFocusMinutes, 150)
            .Add(p => p.PomodoroCount, 10)
            .Add(p => p.TasksWorkedOn, 3));

        // Assert
        Assert.Contains("2h", cut.Markup);
        Assert.Contains("30m", cut.Markup);
    }

    #endregion

    #region Parameter Update Tests

    [Fact]
    public void TodaySummary_WhenParametersChange_UpdatesDisplay()
    {
        // Arrange - Start with initial values
        var cut = RenderComponent<TodaySummary>(parameters => parameters
            .Add(p => p.TotalFocusMinutes, 25)
            .Add(p => p.PomodoroCount, 1)
            .Add(p => p.TasksWorkedOn, 1));

        // Assert - Initial state
        Assert.Contains("25m", cut.Markup);
        Assert.Contains("1", cut.Markup);

        // Act - Update parameters
        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.TotalFocusMinutes, 75)
            .Add(p => p.PomodoroCount, 3)
            .Add(p => p.TasksWorkedOn, 2));

        // Assert - Updated state
        Assert.Contains("1h", cut.Markup);
        Assert.Contains("15m", cut.Markup);
    }

    #endregion

    #region Icon Tests

    [Fact]
    public void TodaySummary_ShowsTimerIcon()
    {
        // Arrange & Act
        var cut = RenderComponent<TodaySummary>(parameters => parameters
            .Add(p => p.TotalFocusMinutes, 25)
            .Add(p => p.PomodoroCount, 1)
            .Add(p => p.TasksWorkedOn, 1));

        // Assert
        Assert.Contains("⏱️", cut.Markup);
    }

    [Fact]
    public void TodaySummary_ShowsPomodoroIcon()
    {
        // Arrange & Act
        var cut = RenderComponent<TodaySummary>(parameters => parameters
            .Add(p => p.TotalFocusMinutes, 25)
            .Add(p => p.PomodoroCount, 1)
            .Add(p => p.TasksWorkedOn, 1));

        // Assert
        Assert.Contains("🍅", cut.Markup);
    }

    [Fact]
    public void TodaySummary_ShowsTaskIcon()
    {
        // Arrange & Act
        var cut = RenderComponent<TodaySummary>(parameters => parameters
            .Add(p => p.TotalFocusMinutes, 25)
            .Add(p => p.PomodoroCount, 1)
            .Add(p => p.TasksWorkedOn, 1));

        // Assert
        Assert.Contains("📋", cut.Markup);
    }

    #endregion
}
