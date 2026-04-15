using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Pomodoro.Web.Components.History;
using Pomodoro.Web.Services.Formatters;
using Xunit;

namespace Pomodoro.Web.Tests.History;

/// <summary>
/// Tests for SummaryCards component
/// </summary>
[Trait("Category", "Component")]
public class SummaryCardsTests : TestContext
{
    public SummaryCardsTests()
    {
        Services.AddSingleton<TimeFormatter>();
        Services.AddSingleton<SummaryCardsFormatter>();
        Services.AddSingleton<StatCardFormatter>();
    }

    [Fact]
    public void SummaryCards_RendersWithDefaultParameters()
    {
        // Arrange & Act
        var cut = RenderComponent<SummaryCards>();

        // Assert
        Assert.NotNull(cut.Markup);
        Assert.Equal(3, cut.FindComponents<StatCard>().Count);
    }

    [Fact]
    public void SummaryCards_RendersWithCustomParameters()
    {
        // Arrange & Act
        var cut = RenderComponent<SummaryCards>(parameters => parameters
            .Add(p => p.PomodoroCount, 5)
            .Add(p => p.FocusMinutes, 120)
            .Add(p => p.TasksWorkedOn, 3));

        // Assert
        Assert.NotNull(cut.Markup);
        Assert.Equal(3, cut.FindComponents<StatCard>().Count);
    }

    [Fact]
    public void SummaryCards_UsesTimeFormatter()
    {
        // Arrange & Act
        var cut = RenderComponent<SummaryCards>(parameters => parameters
            .Add(p => p.FocusMinutes, 90));

        // Assert - Verify that time is formatted correctly
        Assert.Contains("1h 30m", cut.Markup);
    }

    [Fact]
    public void SummaryCards_FormatsZeroMinutes()
    {
        // Arrange & Act
        var cut = RenderComponent<SummaryCards>(parameters => parameters
            .Add(p => p.FocusMinutes, 0));

        // Assert
        Assert.Contains("0m", cut.Markup);
    }

    [Fact]
    public void SummaryCards_FormatsLargeMinuteValues()
    {
        // Arrange & Act
        var cut = RenderComponent<SummaryCards>(parameters => parameters
            .Add(p => p.FocusMinutes, 480)); // 8 hours

        // Assert
        Assert.Contains("8h", cut.Markup);
    }

    [Fact]
    public void SummaryCards_FormatsMinutesOnly()
    {
        // Arrange & Act
        var cut = RenderComponent<SummaryCards>(parameters => parameters
            .Add(p => p.FocusMinutes, 45));

        // Assert
        Assert.Contains("45m", cut.Markup);
    }

    [Fact]
    public void SummaryCards_DisplaysPomodoroCount()
    {
        // Arrange & Act
        var cut = RenderComponent<SummaryCards>(parameters => parameters
            .Add(p => p.PomodoroCount, 10));

        // Assert
        Assert.Contains("10", cut.Markup);
    }

    [Fact]
    public void SummaryCards_DisplaysZeroPomodoroCount()
    {
        // Arrange & Act
        var cut = RenderComponent<SummaryCards>(parameters => parameters
            .Add(p => p.PomodoroCount, 0));

        // Assert
        Assert.Contains("0", cut.Markup);
    }

    [Fact]
    public void SummaryCards_DisplaysTasksWorkedOn()
    {
        // Arrange & Act
        var cut = RenderComponent<SummaryCards>(parameters => parameters
            .Add(p => p.TasksWorkedOn, 7));

        // Assert
        Assert.Contains("7", cut.Markup);
    }

    [Fact]
    public void SummaryCards_DisplaysZeroTasksWorkedOn()
    {
        // Arrange & Act
        var cut = RenderComponent<SummaryCards>(parameters => parameters
            .Add(p => p.TasksWorkedOn, 0));

        // Assert
        Assert.Contains("0", cut.Markup);
    }

    [Fact]
    public void SummaryCards_RendersAllThreeStatCards()
    {
        // Arrange & Act
        var cut = RenderComponent<SummaryCards>(parameters => parameters
            .Add(p => p.PomodoroCount, 5)
            .Add(p => p.FocusMinutes, 150)
            .Add(p => p.TasksWorkedOn, 3));

        // Assert
        var statCards = cut.FindComponents<StatCard>();
        Assert.Equal(3, statCards.Count);
    }

    [Fact]
    public void SummaryCards_HasCorrectCssClasses()
    {
        // Arrange & Act
        var cut = RenderComponent<SummaryCards>();

        // Assert
        Assert.NotNull(cut.Find(".summary-cards"));
    }

    [Fact]
    public void SummaryCards_FormatsComplexTime()
    {
        // Arrange & Act
        var cut = RenderComponent<SummaryCards>(parameters => parameters
            .Add(p => p.FocusMinutes, 125)); // 2h 5m

        // Assert
        Assert.Contains("2h 5m", cut.Markup);
    }
}

