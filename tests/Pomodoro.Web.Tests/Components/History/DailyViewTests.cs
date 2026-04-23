using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Xunit;

namespace Pomodoro.Web.Tests.Components.History;

[Trait("Category", "Component")]
public class DailyViewTests : TestHelper
{
    public DailyViewTests()
    {
    }

    [Fact]
    public void Render_WithDefaultParameters_RendersDailyView()
    {
        var cut = RenderComponent<Pomodoro.Web.Components.History.DailyView>();

        Assert.Contains("stat-grid", cut.Markup);
        Assert.Contains("card", cut.Markup);
    }

    [Fact]
    public void Render_WithSelectedDate_RendersDailyView()
    {
        var date = new DateTime(2024, 6, 15);

        var cut = RenderComponent<Pomodoro.Web.Components.History.DailyView>(
            parameters => parameters.Add(p => p.SelectedDate, date)
        );

        Assert.Contains("stat-grid", cut.Markup);
    }

    [Fact]
    public void Render_WithActivities_RendersTimelineSection()
    {
        var activities = new List<ActivityRecord>
        {
            new() { Id = Guid.NewGuid(), Type = SessionType.Pomodoro, CompletedAt = DateTime.Now, DurationMinutes = 25 }
        };

        var cut = RenderComponent<Pomodoro.Web.Components.History.DailyView>(
            parameters => parameters
                .Add(p => p.Activities, activities)
                .Add(p => p.HasMoreActivities, false)
        );

        Assert.Contains("timeline-scroll-container", cut.Markup);
    }

    [Fact]
    public void Render_WithStats_RendersDailyView()
    {
        var stats = new DailyStatsSummary
        {
            PomodoroCount = 5,
            FocusMinutes = 125,
            TasksWorkedOn = 3
        };

        var cut = RenderComponent<Pomodoro.Web.Components.History.DailyView>(
            parameters => parameters.Add(p => p.CurrentStats, stats)
        );

        Assert.Contains("stat-grid", cut.Markup);
        Assert.Contains("5", cut.Markup);
    }

    [Fact]
    public void Render_WithNullStats_StillRendersDailyView()
    {
        var cut = RenderComponent<Pomodoro.Web.Components.History.DailyView>(
            parameters => parameters.Add(p => p.CurrentStats, (DailyStatsSummary?)null)
        );

        Assert.Contains("stat-grid", cut.Markup);
    }

    [Fact]
    public void Render_WithHasMoreActivitiesTrue_RendersTimelineSection()
    {
        var cut = RenderComponent<Pomodoro.Web.Components.History.DailyView>(
            parameters => parameters
                .Add(p => p.Activities, new List<ActivityRecord>())
                .Add(p => p.HasMoreActivities, true)
        );

        Assert.Contains("timeline-scroll-container", cut.Markup);
    }

    [Fact]
    public void Render_WithIsLoadingMoreTrue_RendersTimelineSection()
    {
        var cut = RenderComponent<Pomodoro.Web.Components.History.DailyView>(
            parameters => parameters
                .Add(p => p.Activities, new List<ActivityRecord>())
                .Add(p => p.IsLoadingMore, true)
        );

        Assert.Contains("timeline-scroll-container", cut.Markup);
    }

    [Fact]
    public void Render_AlwaysRendersTimeDistributionChart()
    {
        var cut = RenderComponent<Pomodoro.Web.Components.History.DailyView>();

        Assert.Contains("Time distribution", cut.Markup);
    }

    [Fact]
    public void Render_WithEmptyActivities_RendersTimelineSection()
    {
        var cut = RenderComponent<Pomodoro.Web.Components.History.DailyView>(
            parameters => parameters
                .Add(p => p.Activities, new List<ActivityRecord>())
                .Add(p => p.HasMoreActivities, false)
                .Add(p => p.IsLoadingMore, false)
        );

        Assert.Contains("timeline-scroll-container", cut.Markup);
    }
}
