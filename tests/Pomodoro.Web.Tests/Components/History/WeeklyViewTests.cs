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

        Assert.Contains("card", cut.Markup);
        Assert.Contains("Sessions per day", cut.Markup);
    }

    [Fact]
    public void Render_WithSelectedWeekStart_RendersWeeklyView()
    {
        var weekStart = new DateTime(2024, 1, 6);

        var cut = RenderComponent<Pomodoro.Web.Components.History.WeeklyView>(
            parameters => parameters.Add(p => p.SelectedWeekStart, weekStart)
        );

        Assert.Contains("card", cut.Markup);
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

        Assert.Contains("stat-grid", cut.Markup);
    }

    [Fact]
    public void Render_WithNullStats_DoesNotRenderWeeklySummarySection()
    {
        var cut = RenderComponent<Pomodoro.Web.Components.History.WeeklyView>(
            parameters => parameters.Add(p => p.WeeklyStats, (WeeklyStats?)null)
        );

        Assert.DoesNotContain("stat-grid", cut.Markup);
    }

    [Fact]
    public void Render_WithFocusMinutes_RendersChart()
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

        Assert.Contains("Sessions per day", cut.Markup);
        Assert.Contains("Time distribution", cut.Markup);
    }

    [Fact]
    public void Render_WithEmptyDictionaries_RendersCard()
    {
        var cut = RenderComponent<Pomodoro.Web.Components.History.WeeklyView>(
            parameters => parameters
                .Add(p => p.WeeklyFocusMinutes, new Dictionary<DateTime, int>())
                .Add(p => p.WeeklyBreakMinutes, new Dictionary<DateTime, int>())
        );

        Assert.Contains("card", cut.Markup);
    }

    [Fact]
    public void Render_AlwaysRendersCards()
    {
        var cut = RenderComponent<Pomodoro.Web.Components.History.WeeklyView>();

        Assert.Contains("Sessions per day", cut.Markup);
        Assert.Contains("Time distribution", cut.Markup);
    }
}
