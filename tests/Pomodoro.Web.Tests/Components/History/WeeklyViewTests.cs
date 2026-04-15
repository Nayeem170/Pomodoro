using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Xunit;

namespace Pomodoro.Web.Tests.Components.History;

[Trait("Category", "Component")]
public class WeeklyViewTests : TestHelper
{
    public WeeklyViewTests()
    {
    }

    [Fact]
    public void Render_WithDefaultParameters_RendersWeeklyView()
    {
        var cut = RenderComponent<Pomodoro.Web.Components.History.WeeklyView>();

        Assert.Contains("weekly-view", cut.Markup);
        Assert.Contains("date-navigator-container", cut.Markup);
        Assert.Contains("weekly-chart-section", cut.Markup);
    }

    [Fact]
    public void Render_WithSelectedWeekStart_PassesToWeekNavigator()
    {
        var weekStart = new DateTime(2024, 1, 6);

        var cut = RenderComponent<Pomodoro.Web.Components.History.WeeklyView>(
            parameters => parameters.Add(p => p.SelectedWeekStart, weekStart)
        );

        Assert.Contains("weekly-view", cut.Markup);
    }

    [Fact]
    public void Render_WithStats_RendersWeeklySummarySection()
    {
        var stats = new WeeklyStats
        {
            TotalFocusMinutes = 500,
            TotalPomodoroCount = 20,
            DailyAverageMinutes = 71.4,
            WeekOverWeekChange = 0
        };

        var cut = RenderComponent<Pomodoro.Web.Components.History.WeeklyView>(
            parameters => parameters.Add(p => p.WeeklyStats, stats)
        );

        Assert.Contains("weekly-summary-section", cut.Markup);
    }

    [Fact]
    public void Render_WithNullStats_DoesNotRenderWeeklySummarySection()
    {
        var cut = RenderComponent<Pomodoro.Web.Components.History.WeeklyView>(
            parameters => parameters.Add(p => p.WeeklyStats, (WeeklyStats?)null)
        );

        Assert.DoesNotContain("weekly-summary-section", cut.Markup);
    }

    [Fact]
    public void Render_WithFocusMinutes_PassesToChart()
    {
        var focusMinutes = new Dictionary<DateTime, int>
        {
            { new DateTime(2024, 1, 6), 120 },
            { new DateTime(2024, 1, 7), 90 }
        };
        var breakMinutes = new Dictionary<DateTime, int>
        {
            { new DateTime(2024, 1, 6), 25 },
            { new DateTime(2024, 1, 7), 10 }
        };

        var cut = RenderComponent<Pomodoro.Web.Components.History.WeeklyView>(
            parameters => parameters
                .Add(p => p.WeeklyFocusMinutes, focusMinutes)
                .Add(p => p.WeeklyBreakMinutes, breakMinutes)
                .Add(p => p.SelectedWeekStart, new DateTime(2024, 1, 6))
        );

        Assert.Contains("weekly-chart-section", cut.Markup);
        Assert.Contains("Weekly Trend", cut.Find("h2").TextContent);
    }

    [Fact]
    public void Render_WithEmptyDictionaries_RendersChartSection()
    {
        var cut = RenderComponent<Pomodoro.Web.Components.History.WeeklyView>(
            parameters => parameters
                .Add(p => p.WeeklyFocusMinutes, new Dictionary<DateTime, int>())
                .Add(p => p.WeeklyBreakMinutes, new Dictionary<DateTime, int>())
        );

        Assert.Contains("weekly-chart-section", cut.Markup);
    }

    [Fact]
    public void Render_AlwaysRendersDateNavigatorAndChartSection()
    {
        var cut = RenderComponent<Pomodoro.Web.Components.History.WeeklyView>();

        Assert.Contains("date-navigator-container", cut.Markup);
        Assert.Contains("weekly-chart-section", cut.Markup);
        Assert.Contains("Weekly Trend", cut.Find("h2").TextContent);
    }
}

