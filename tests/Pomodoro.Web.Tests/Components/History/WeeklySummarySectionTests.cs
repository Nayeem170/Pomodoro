using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Xunit;

namespace Pomodoro.Web.Tests.Components.History;

[Trait("Category", "Component")]
public class WeeklySummarySectionTests : TestHelper
{
    public WeeklySummarySectionTests()
    {
    }

    [Fact]
    public void Render_WithNullStats_RendersNothing()
    {
        var cut = RenderComponent<Pomodoro.Web.Components.History.WeeklySummarySection>(
            parameters => parameters.Add(p => p.WeeklyStats, (WeeklyStats?)null)
        );

        Assert.Empty(cut.Markup);
    }

    [Fact]
    public void Render_WithStats_RendersSummarySection()
    {
        var stats = new WeeklyStats
        {
            TotalFocusMinutes = 500,
            TotalPomodoroCount = 20,
            DailyAverageMinutes = 71.4,
            WeekOverWeekChange = 0
        };

        var cut = RenderComponent<Pomodoro.Web.Components.History.WeeklySummarySection>(
            parameters => parameters.Add(p => p.WeeklyStats, stats)
        );

        Assert.Contains("Pomodoros", cut.Markup);
        Assert.Contains("Focus time", cut.Markup);
        Assert.Contains("Daily avg", cut.Markup);
    }

    [Fact]
    public void Render_WithStats_DisplaysCorrectValues()
    {
        var stats = new WeeklyStats
        {
            TotalFocusMinutes = 60,
            TotalPomodoroCount = 20,
            DailyAverageMinutes = 71.4,
            WeekOverWeekChange = 0
        };

        var cut = RenderComponent<Pomodoro.Web.Components.History.WeeklySummarySection>(
            parameters => parameters.Add(p => p.WeeklyStats, stats)
        );

        Assert.Contains("1h", cut.Markup);
        Assert.Contains("20", cut.Markup);
        Assert.Contains("71", cut.Markup);
    }

    [Fact]
    public void Render_WithStats_ShowsFourStatCards()
    {
        var stats = new WeeklyStats
        {
            TotalFocusMinutes = 100,
            TotalPomodoroCount = 4,
            DailyAverageMinutes = 14.3,
            WeekOverWeekChange = 0
        };

        var cut = RenderComponent<Pomodoro.Web.Components.History.WeeklySummarySection>(
            parameters => parameters.Add(p => p.WeeklyStats, stats)
        );

        Assert.Equal(4, cut.FindAll(".sc").Count);
    }
}
