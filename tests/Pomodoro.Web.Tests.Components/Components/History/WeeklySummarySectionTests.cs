using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Xunit;

namespace Pomodoro.Web.Tests.Components.Components.History;

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

        Assert.Contains("Weekly Summary", cut.Find("h2").TextContent);
        Assert.Contains("Minutes This Week", cut.Markup);
        Assert.Contains("Pomodoros", cut.Markup);
        Assert.Contains("Daily Average", cut.Markup);
    }

    [Fact]
    public void Render_WithStats_DisplaysCorrectValues()
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

        Assert.Contains("500", cut.Markup);
        Assert.Contains("20", cut.Markup);
        Assert.Contains("71", cut.Markup);
    }

    [Fact]
    public void Render_WithPositiveWeekOverWeekChange_ShowsTrend()
    {
        var stats = new WeeklyStats
        {
            TotalFocusMinutes = 500,
            TotalPomodoroCount = 20,
            DailyAverageMinutes = 71.4,
            WeekOverWeekChange = 15
        };

        var cut = RenderComponent<Pomodoro.Web.Components.History.WeeklySummarySection>(
            parameters => parameters.Add(p => p.WeeklyStats, stats)
        );

        Assert.Contains("positive", cut.Markup);
        Assert.Contains("+15%", cut.Markup);
        Assert.Contains("vs Last Week", cut.Markup);
    }

    [Fact]
    public void Render_WithNegativeWeekOverWeekChange_ShowsNegativeTrend()
    {
        var stats = new WeeklyStats
        {
            TotalFocusMinutes = 300,
            TotalPomodoroCount = 12,
            DailyAverageMinutes = 42.8,
            WeekOverWeekChange = -10
        };

        var cut = RenderComponent<Pomodoro.Web.Components.History.WeeklySummarySection>(
            parameters => parameters.Add(p => p.WeeklyStats, stats)
        );

        Assert.Contains("negative", cut.Markup);
        Assert.Contains("-10%", cut.Markup);
    }

    [Fact]
    public void Render_WithZeroWeekOverWeekChange_HidesTrend()
    {
        var stats = new WeeklyStats
        {
            TotalFocusMinutes = 400,
            TotalPomodoroCount = 16,
            DailyAverageMinutes = 57.1,
            WeekOverWeekChange = 0
        };

        var cut = RenderComponent<Pomodoro.Web.Components.History.WeeklySummarySection>(
            parameters => parameters.Add(p => p.WeeklyStats, stats)
        );

        Assert.DoesNotContain("trend", cut.Markup);
    }

    [Fact]
    public void Render_WithStats_ShowsExactlyThreeBaseStats()
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

        Assert.Equal(3, cut.FindAll(".stat:not(.trend)").Count);
    }
}
